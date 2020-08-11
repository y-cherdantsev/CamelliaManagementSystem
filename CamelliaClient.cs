using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using AngleSharp;
using Camellia_Management_System.JsonObjects.ResponseObjects;
using CamelliaManagementSystem.SignManage;

// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo

namespace CamelliaManagementSystem
{
    /// @author Yevgeniy Cherdantsev
    /// @date 07.03.2020 15:13:36
    /// <summary>
    /// Client for connecting to the service and using it
    /// </summary>
    public class CamelliaClient : IDisposable
    {
        /// <summary>
        /// Http client
        /// </summary>
        public readonly HttpClient HttpClient;

        /// <summary>
        /// Information about user
        /// </summary>
        public UserInformation UserInformation;

        /// <summary>
        /// Container of cookies
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public CookieContainer CookieContainer;

        /// <summary>
        /// Proxy if need
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        internal readonly IWebProxy Proxy;

        /// <summary>
        /// Sign of a client
        /// </summary>
        public readonly FullSign FullSign;

        /// <summary>
        /// Name of the folder with sign
        /// </summary>
        public string folderName => new FileInfo(FullSign.authSign.filePath).Directory?.Name;


        /// @author Yevgeniy Cherdantsev
        /// @date 10.03.2020 10:14:34
        /// <summary>
        /// Constructor for creating Camellia client with(out) proxy
        /// </summary>
        /// <param name="fullSign">AUTH and RSA signs of the user</param>
        /// <param name="webProxy">Proxy</param>
        /// <param name="httpClientTimeout">Timeout of http client connected to the camellia system; Standard: 15000</param>
        public CamelliaClient(FullSign fullSign, IWebProxy webProxy = null, int httpClientTimeout = 15000)
        {
            FullSign = fullSign;
            Proxy = webProxy;

            //If proxy equals null creates object without proxy and vice versa
            var handler = Proxy != null
                ? new HttpClientHandler {UseProxy = true, Proxy = Proxy}
                : new HttpClientHandler();

            handler.AllowAutoRedirect = true;
            CookieContainer = handler.CookieContainer;
            HttpClient = new HttpClient(handler) {Timeout = TimeSpan.FromMilliseconds(httpClientTimeout)};
        }

        /// @author Yevgeniy Cherdantsev
        /// @date 10.03.2020 10:15:18
        /// <summary>
        /// Connects to the camellia system using handler
        /// </summary>
        public void Login()
        {
            //Addresses with necessary cookies for authorization
            string[] cookieAddresses =
            {
                "https://www.egov.kz",
                "https://idp.egov.kz/idp/login?lvl=2&url=https://egov.kz/cms/callback/auth/cms/"
            };

            //Getting cookies
            foreach (var url in cookieAddresses)
            {
                HttpClient.GetStringAsync(url).GetAwaiter().GetResult();
            }

            //Runs authorization script
            Authorize();

            UserInformation = GetUserInformation();
        }

        /// @author Yevgeniy Cherdantsev
        /// @date 10.03.2020 10:15:18
        /// <summary>
        /// Request of the connection token
        /// </summary>
        /// <returns>string - Connection token</returns>
        private string GetToken()
        {
            const string tokenUrl = "https://idp.egov.kz/idp/sign-in";

            var response = HttpClient.GetStringAsync(tokenUrl).GetAwaiter().GetResult();

            // Generates document for AngleSharp
            // ReSharper disable once AccessToModifiedClosure
            var angleDocument = new BrowsingContext(Configuration.Default).OpenAsync(x => x.Content(response))
                .GetAwaiter().GetResult();

            // Gets 'value' attribute of the page for authorization
            response = angleDocument.All.First(m => m.GetAttribute("id") == "xmlToSign").GetAttribute("value");

            return response;
        }


        /// @author Yevgeniy Cherdantsev
        /// @date 10.03.2020 10:15:18
        /// <summary>
        /// Authorization to the system using sign
        /// </summary>
        private void Authorize()
        {
            //Check if provided sign exists
            if (!new FileInfo(FullSign.authSign.filePath).Exists)
                throw new FileNotFoundException($"Can't find '{FullSign.authSign.filePath}'");

            //Signing token from authorization page
            var signedToken = SignXmlTokens.SignToken(GetToken(), FullSign.authSign);

            var values = new Dictionary<string, string>
            {
                {"certificate", signedToken},
                {"lvl", "2"}
            };

            var content = new FormUrlEncodedContent(values);

            // Posting signed token to the camellia system
            HttpClient.PostAsync("https://idp.egov.kz/idp/eds-login.do", content).GetAwaiter().GetResult();
        }

        /// @author Yevgeniy Cherdantsev
        /// <summary>
        /// Get the information about connection of the client to the system
        /// </summary>
        /// <returns>true if the user logged in</returns>
        public bool IsLogged()
        {
            //TODO(Rethink function, it shouldn't depend on errors. If error won't tide to logging information, it could be lost)
            //Trying to get information about user, if error occured it's likely that client not logged in
            try
            {
                GetUserInformation(1);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        /// @author Yevgeniy Cherdantsev
        /// @date 10.03.2020 10:17:42
        /// <summary>
        /// Loading user information from camellia system
        /// </summary>
        /// <param name="numberOfTries">Number of requests if some errors has been occured</param>
        /// <param name="delay">Time in millis between requests</param>
        /// <returns>UserInformation - Information about authorized user</returns>
        private UserInformation GetUserInformation(int numberOfTries = 3, int delay = 500)
        {
            var response = new HttpResponseMessage();
            for (var i = 0; i < numberOfTries; i++)
            {
                response = HttpClient.GetAsync("https://egov.kz/services/P30.11/rest/current-user").GetAwaiter()
                    .GetResult();
                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                switch (response.StatusCode)
                {
                    case HttpStatusCode.Redirect:
                        Thread.Sleep(delay);
                        continue;
                    case HttpStatusCode.OK:
                        var result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        var userInformation = JsonSerializer.Deserialize<UserInformation>(result);
                        return userInformation;
                    default:
                        throw new HttpRequestException(
                            $"StatusCode:'{response.StatusCode}';\nReasonPhrase:'{response.ReasonPhrase}';\nContent is null;");
                }
            }

            throw new HttpRequestException(
                $"StatusCode:'{response.StatusCode}';\nReasonPhrase:'{response.ReasonPhrase}';\nContent is null;");
        }

        /// @author Yevgeniy Cherdantsev
        /// @date 29.06.2020 15:43:22
        /// <summary>
        /// Loading string user data from camellia system
        /// </summary>
        /// <returns>string - Information about user</returns>
        [Obsolete("GetUser is deprecated, there is no any scenarios where it could be used")]
        private string GetUser()
        {
            var res = HttpClient.GetStringAsync("https://egov.kz/cms/auth/user.json").GetAwaiter().GetResult();
            return res;
        }


        /// @author Yevgeniy Cherdantsev
        /// @date 07.03.2020 15:50:14
        /// <summary>
        /// Logging out of camellia system
        /// </summary>
        public void Logout()
        {
            // HttpClient can be already disposed, don't know the reason
            HttpClient?.GetAsync("https://egov.kz/cms/ru/auth/logout").GetAwaiter().GetResult();
            UserInformation = null;
            CookieContainer = null;
        }


        /// @author Yevgeniy Cherdantsev
        /// @date 10.03.2020 10:19:28
        /// <summary>
        /// Disposing
        /// </summary>
        public void Dispose()
        {
            HttpClient.Dispose();
            FullSign.Dispose();
        }
    }
}