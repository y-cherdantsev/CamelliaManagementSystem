using System;
using System.Net;
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

        /// @author Yevgeniy Cherdantsev
        /// @date 18.02.2020 10:31:53
        /// <summary>
        /// Creates clients from the given signs
        /// </summary>
        /// <param name="signs">List of signs</param>
        /// <param name="webProxies">Proxy if need</param>
        /// <param name="handlerTimeout">Timeout</param>
        /// <param name="numberOfTries">Number Of Tries</param>
        /// <returns>List - Shuffled list</returns>
        public CamelliaClientProvider(List<Sign> signs, List<IWebProxy> webProxies = null,
            int handlerTimeout = 20000, int numberOfTries = 5)
        {
            _signs = signs;
            _webProxies = webProxies?.GetEnumerator();
            _handlerTimeout = handlerTimeout;
            _numberOfTries = numberOfTries;
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
                    ? new CamelliaClient(sign, _webProxies.Current, _handlerTimeout)
                    : new CamelliaClient(sign, httpClientTimeout: _handlerTimeout);

                tasks.Add(LoadClientAsync(client, _numberOfTries));
            }

            await Task.WhenAll(tasks);
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
                }
                catch (CamelliaClientException)
                {
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            Console.WriteLine($"Loaded client: '{client.User.full_name}'");
            _camelliaClients.Add(client);
        }

        /// @author Yevgeniy Cherdantsev
        /// <summary>
        /// Get next client from provider
        /// </summary>
        /// <returns>CamelliaClient - returns connected client</returns>
        public CamelliaClient GetNextClient()
        {
            lock (_lock)
            {
                // ReSharper disable once EmptyEmbeddedStatement
                for (var i = 0; i < 120 && _camelliaClients.Count < 3; ++i) Thread.Sleep(500);

                if (_camelliaClients.Count < 3)
                {
                    _camelliaClients.Clear();
                    LoadClientsAsync().GetAwaiter().GetResult();
                }

                lock (_camelliaClients)
                {
                    foreach (var camelliaClient in _camelliaClients)
                    {
                        var client = camelliaClient;
                        _camelliaClients.Remove(camelliaClient);
                        if (client.IsLoggedAsync().Result) return client;
                        LoadClientAsync(client, _numberOfTries).GetAwaiter().GetResult();
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
            _camelliaClients.Add(client);
        }
    }
}