using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Camellia_Management_System.SignManage;

namespace Camellia_Management_System
{
    /// @author Yevgeniy Cherdantsev
    /// @version 1.0
    /// <summary>
    /// Controller and provider of camellia clients
    /// </summary>
    public class CamelliaClientProvider
    {
        
        /// <summary>
        /// List of ready clients in the current cycle
        /// </summary>
        private List<CamelliaClient> _camelliaClients = new List<CamelliaClient>();
        
        /// <summary>
        /// List of used clients in the current cycle
        /// </summary>
        private readonly List<CamelliaClient> _usedClients = new List<CamelliaClient>();
        
        /// <summary>
        /// Sign provider
        /// </summary>
        private readonly SignProvider _signProvider;
        
        /// <summary>
        /// List of proxies
        /// </summary>
        private readonly IEnumerator<IWebProxy> _webProxies;
        
        /// <summary>
        /// Tracks if the ClientProvider is reloading now
        /// </summary>
        private bool _isReloading;
        
        /// <summary>
        /// Return the number of left clients before the next shuffle
        /// </summary>
        public int clientsLeft => _camelliaClients.Count;

        /// @author Yevgeniy Cherdantsev
        /// @date 18.02.2020 10:31:53
        /// @version 1.0
        /// <summary>
        /// Creates clients from the given signs
        /// </summary>
        /// <param name="signProvider">Sign provider</param>
        /// <param name="webProxies">Proxy if need</param>
        /// <param name="handlerTimeout">Timeout</param>
        /// <param name="numOfTries">Number Of Tries</param>
        /// <returns>List - Shuffled list</returns>
        public CamelliaClientProvider(SignProvider signProvider, IEnumerator<IWebProxy> webProxies = null,
            int handlerTimeout = 20000, int numOfTries = 5)
        {
            if (webProxies == null)
                webProxies = new List<IWebProxy>{ null}.GetEnumerator();
            _webProxies = webProxies;
            _signProvider = signProvider;
            //TODO (SEVERAL PROXIES)
            Task.Run(() =>
            {
                while (signProvider.signsLeft > 0)
                {
                    var sign = signProvider.GetNextSign();

                    for (var i = 0; i < numOfTries; i++)
                    {
                        try
                        {
                            if (!_webProxies.MoveNext())
                            {
                                _webProxies.Reset();
                                _webProxies.MoveNext();
                            }
                            var client = new CamelliaClient(sign, _webProxies.Current, handlerTimeout);
                            _camelliaClients.Add(client);
                            i = numOfTries;
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                }


                // if (_camelliaClients.Count == 0)
                // {
                    // throw new InvalidDataException("No clients has been loaded");
                    // Console.WriteLine("No clients has been loaded");
                    // return;
                // }
            });
            while (_camelliaClients.Count == 0);
        }

        /// @author Yevgeniy Cherdantsev
        /// @version 1.0
        /// <summary>
        /// Get next client from provider
        /// </summary>
        /// <returns>CamelliaClient - returns connected client</returns>
        public CamelliaClient GetNextClient()
        {
            while (_isReloading);
            if (_camelliaClients.Count == 0)
            {
                _isReloading = true;
                if (_usedClients.Count == 0)
                {
                    _signProvider.ReloadSigns();
                    while (_signProvider.signsLeft > 0)
                    {
                        var sign = _signProvider.GetNextSign();

                        for (var i = 0; i < 3; i++)
                        {
                            try
                            {
                                var client = new CamelliaClient(sign, _webProxies.Current);
                                if (!_webProxies.MoveNext())
                                    _webProxies.Reset();
                                _camelliaClients.Add(client);
                                i = 3;
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                    }
                }

                _camelliaClients = ShuffleList(_usedClients);
                _usedClients.Clear();
                _isReloading = false;
            }

            CamelliaClient result;
            lock (_camelliaClients)
            {
                try
                {
                    result = _camelliaClients[0];
                }
                catch (Exception)
                {
                    throw new InvalidDataException("Client manager has no loaded clients; Reason: service unavaliable");
                }

                _usedClients.Add(result);
                _camelliaClients.Remove(result);
                if (!result.IsLogged())
                {
                    try
                    {
                        result = new CamelliaClient(result.FullSign, result.Proxy);
                    }
                    catch (Exception)
                    {
                        return GetNextClient();
                    }
                }
            }

            return result;
        }

        /// @author Yevgeniy Cherdantsev
        /// @date 18.02.2020 10:31:53
        /// @version 1.0
        /// <summary>
        /// Shuffles given list of objects
        /// </summary>
        /// <param name="inputList">List to shuffle</param>
        /// <returns>List - Shuffled list</returns>
        private static List<TE> ShuffleList<TE>(IList<TE> inputList)
        {
            var shuffledList = new List<TE>();

            var r = new Random(DateTime.UtcNow.Millisecond);
            while (inputList.Count > 0)
            {
                var randomIndex = r.Next(0, inputList.Count);
                shuffledList.Add(inputList[randomIndex]);
                inputList.RemoveAt(randomIndex);
            }

            return shuffledList;
        }
    }
}