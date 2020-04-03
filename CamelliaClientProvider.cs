using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Camellia_Management_System.SignManage;

namespace Camellia_Management_System
{
    public class CamelliaClientProvider
    {
        private List<CamelliaClient> _camelliaClients = new List<CamelliaClient>();
        private readonly List<CamelliaClient> _usedClients = new List<CamelliaClient>();

        public CamelliaClientProvider(SignProvider signProvider, IWebProxy webProxy = null)
        {
            while (signProvider.SignsLeft > 0)
            {
                try
                {
                    var sign = signProvider.GetNextSign();
                    var client = new CamelliaClient(sign, webProxy);
                    _camelliaClients.Add(client);
                }
                catch (Exception ignore)
                {
                    //ignore
                }
            }


            if (_camelliaClients.Count == 0)
                throw new InvalidDataException("No clients has been loaded");
        }

        public CamelliaClient GetNextClient()
        {
            if (_camelliaClients.Count == 0)
            {
                _camelliaClients = ShuffleList(_usedClients);
                _usedClients.Clear();
            }

            CamelliaClient result;
            lock (_camelliaClients)
            {
                result = _camelliaClients[0];
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