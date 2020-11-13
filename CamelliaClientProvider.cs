using System;
using System.Net;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using CamelliaManagementSystem.Requests;
using CamelliaManagementSystem.SignManage;

// ReSharper disable CommentTypo
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace CamelliaManagementSystem
{
    /// @author Yevgeniy Cherdantsev
    /// <summary>
    /// Controller and provider of camellia clients
    /// </summary>
    // ReSharper disable once UnusedType.Global
    public class CamelliaClientProvider
    {
        /// <summary>
        /// List of ready clients in the current cycle
        /// </summary>
        private readonly List<CamelliaClient> _camelliaClients = new List<CamelliaClient>();

        /// <summary>
        /// List of signs
        /// </summary>
        private readonly List<Sign> _signs;

        /// <summary>
        /// Lock object
        /// </summary>
        private readonly object _lock = new object();

        /// <summary>
        /// List of proxies
        /// </summary>
        private readonly IEnumerator<IWebProxy> _webProxies;

        /// <summary>
        /// Handler timeout of created clients
        /// </summary>
        private readonly int _handlerTimeout;

        /// <summary>
        /// Number of tries while creating clients
        /// </summary>
        private readonly int _numberOfTries;

        /// <summary>
        /// Number of seconds left till reloading
        /// </summary>
        private int _secondsLeft;

        /// <summary>
        /// Number of seconds left till reloading
        /// </summary>
        private readonly int _allowedDowntime;

        /// <summary>
        /// Network address of NCANode
        /// </summary>
        public readonly string NcaNodeHost;

        /// <summary>
        /// Network port of NCANode
        /// </summary>
        public readonly int NcaNodePort;

        /// @author Yevgeniy Cherdantsev
        /// @date 18.02.2020 10:31:53
        /// <summary>
        /// Creates clients from the given signs
        /// </summary>
        /// <param name="signs">List of signs</param>
        /// <param name="ncaNodeHost">Network address of NCANode</param>
        /// <param name="ncaNodePort">Network port of NCANode</param>
        /// <param name="webProxies">Proxy if need</param>
        /// <param name="handlerTimeout">Timeout</param>
        /// <param name="numberOfTries">Number Of Tries</param>
        /// <param name="allowedDowntime">Allowed downtime in seconds without clients, after it reload will be proceeded</param>
        public CamelliaClientProvider(List<Sign> signs, string ncaNodeHost, int ncaNodePort,
            List<IWebProxy> webProxies = null,
            int handlerTimeout = 20000, int numberOfTries = 5, int allowedDowntime = 240)
        {
            _signs = signs;
            _webProxies = webProxies?.GetEnumerator();
            _handlerTimeout = handlerTimeout;
            _numberOfTries = numberOfTries;
            _allowedDowntime = allowedDowntime;
            _secondsLeft = allowedDowntime;
            NcaNodeHost = ncaNodeHost;
            NcaNodePort = ncaNodePort;
            new Task(async () =>
            {
                while (true)
                {
                    await Task.Delay(10000);
                    try
                    {
                        Console.WriteLine($"Timer left: {_secondsLeft}");
                        switch (_camelliaClients.Count)
                        {
                            case 0 when _secondsLeft <= 0:
                            {
                                _camelliaClients.Clear();
                                lock (_camelliaClients)
                                    LoadClientsAsync().GetAwaiter().GetResult();
                                _secondsLeft = allowedDowntime;
                                break;
                            }
                            case 0:
                                _secondsLeft -= 10;
                                break;
                            default:
                                _secondsLeft = allowedDowntime;
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }).Start();
        }

        /// @author Yevgeniy Cherdantsev
        /// @date 30.06.2020 11:57:51
        /// <summary>
        /// Loads clients from the given signs
        /// </summary>
        /// <returns>List - Shuffled list</returns>
        public async Task LoadClientsAsync()
        {
            var tasks = new List<Task>();
            foreach (var sign in _signs)
            {
                if (_webProxies != null)
                    if (_webProxies.MoveNext())
                    {
                        _webProxies.Reset();
                        _webProxies.MoveNext();
                    }

                var client = _webProxies != null
                    ? new CamelliaClient(sign, NcaNodeHost, NcaNodePort, _webProxies.Current, _handlerTimeout)
                    : new CamelliaClient(sign, NcaNodeHost, NcaNodePort, httpClientTimeout: _handlerTimeout);

                tasks.Add(LoadClientAsync(client, _numberOfTries));
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Loading of signs failed: '{e}'");
            }

            tasks.Where(x => x.IsFaulted).ToList().ForEach(x =>
                Console.WriteLine($"Error while loading client: '{x.Exception?.Message}'"));
            if (_camelliaClients.Count < 1)
                throw new CamelliaClientProviderException("No clients has been loaded");
        }

        /// <summary>
        /// Loads client into client provider
        /// </summary>
        /// <param name="client">Client to load</param>
        /// <param name="attempts">Number of attemtps to load client</param>
        private async Task LoadClientAsync(CamelliaClient client, int attempts)
        {
            for (var i = 0; i < attempts; i++)
            {
                try
                {
                    await client.LoginAsync();
                    Console.WriteLine($"Loaded client: '{client}'");
                    if (_camelliaClients.All(x => x.Sign.biin != client.Sign.biin))
                    {
                        GC.KeepAlive(client);
                        _camelliaClients.Add(client);
                    }

                    break;
                }
                catch (CamelliaClientException)
                {
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e.Message} | {client}");
                }

                Thread.Sleep(2000);
            }
        }

        /// @author Yevgeniy Cherdantsev
        /// <summary>
        /// Get next client from provider
        /// </summary>
        /// <returns>CamelliaClient - returns connected client</returns>
        public CamelliaClient GetNextClient()
        {
            Console.WriteLine("Waiting for lock released");
            lock (_lock)
            {
                Console.WriteLine("Waiting for client released");
                // ReSharper disable once EmptyEmbeddedStatement
                while (_camelliaClients.Count < 1) Thread.Sleep(500);

                Console.WriteLine("Waiting for inner lock released");
                lock (_camelliaClients)
                {
                    Console.WriteLine("Trying to get client");
                    foreach (var camelliaClient in _camelliaClients)
                    {
                        var client = camelliaClient;
                        _camelliaClients.Remove(camelliaClient);
                        if (!client.IsLoggedAsync().Result)
                            LoadClientAsync(client, _numberOfTries).GetAwaiter().GetResult();
                        Console.WriteLine($"Client found successfully: {client}");
                        return client;
                    }

                    throw new CamelliaClientProviderException("There is no available loaded clients");
                }
            }
        }

        /// <summary>
        /// Releases client back to client provider
        /// </summary>
        /// <param name="client">CamelliaClient</param>
        public void ReleaseClient(CamelliaClient client)
        {
            Console.WriteLine($"Releasing client: {client}");
            _secondsLeft = _allowedDowntime;
            if (_camelliaClients.All(x => x.Sign.biin != client.Sign.biin))
                _camelliaClients.Add(client);
            Console.WriteLine($"Client has been released: {client}");
        }
    }
}