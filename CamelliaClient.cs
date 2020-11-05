using System;
using System.Net;
using AngleSharp;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Authentication;
using CamelliaManagementSystem.Requests;
using CamelliaManagementSystem.SignManage;
using CamelliaManagementSystem.JsonObjects.ResponseObjects;

#pragma warning disable 618

// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable MemberCanBePrivate.Global

namespace CamelliaManagementSystem
{
    /// @author Yevgeniy Cherdantsev
    /// @date 07.03.2020 15:13:36
    /// <summary>
    /// Client for connecting to the service and making requests
    /// </summary>
    public class CamelliaClient
    {
        /// <summary>
        /// Http client
        /// </summary>
        public readonly HttpClient HttpClient;

        /// <summary>
        /// Information about user
        /// </summary>
        public User User;

        /// <summary>
        /// Container of cookies
        /// </summary>
        // ReSharper disable once NotAccessedField.Global
        public CookieContainer CookieContainer;

        /// <summary>
        /// Proxy if need
        /// </summary>
        internal readonly IWebProxy Proxy;

        /// <summary>
        /// Sign of a client
        /// </summary>
        public readonly Sign Sign;
        
        /// <summary>
        /// Network address of NCANode
        /// </summary>
        public readonly string NcaNodeHost;
        
        /// <summary>
        /// Network port of NCANode
        /// </summary>
        public readonly int NcaNodePort;


        /// <summary>
        /// Constructor for creating Camellia client with(out) proxy
        /// </summary>
        /// <param name="sign">AUTH and RSA signs of the user</param>
        /// <param name="ncaNodeHost">Network address of NCANode</param>
        /// <param name="ncaNodePort">Network port of NCANode</param>
        /// <param name="webProxy">Proxy</param>
        /// <param name="httpClientTimeout">Timeout of http client connected to the camellia system; Standard: 15000</param>
        public CamelliaClient(Sign sign, string ncaNodeHost, int ncaNodePort, IWebProxy webProxy = null, int httpClientTimeout = 15000)
        {
            Sign = sign;
            Proxy = webProxy;
            NcaNodeHost = ncaNodeHost;
            NcaNodePort = ncaNodePort;
            //If proxy equals null creates handler without proxy and vice versa
            var handler = Proxy != null
                ? new HttpClientHandler {UseProxy = true, Proxy = Proxy}
                : new HttpClientHandler();

            handler.AllowAutoRedirect = true;
            handler.ServerCertificateCustomValidationCallback =
                (creation, of, insecure, connection) => true;
            handler.SslProtocols = SslProtocols.None;

            CookieContainer = handler.CookieContainer;
            HttpClient = new HttpClient(handler) {Timeout = TimeSpan.FromMilliseconds(httpClientTimeout)};
            GC.KeepAlive(HttpClient);
        }

        /// <summary>
        /// Connects to the camellia system using handler
        /// </summary>
        public async Task LoginAsync()
        {
            // Getting cookies from addresses with necessary cookies for authorization
            var urls = new[]
            {
                "https://www.egov.kz",
                "https://idp.egov.kz/idp/login?lvl=2&url=https://egov.kz/cms/callback/auth/cms/"
            };

            foreach (var url in urls)
                await HttpClient.GetStringAsync(url);

            //Runs authorization script
            await AuthorizeAsync();

            User = await GetUserAsync();

            if (User.user_iin == null)
                throw new CamelliaClientException($"Sign: '{Sign.biin}' hasn't been loaded");
        }

        /// <summary>
        /// Request of the connection token
        /// </summary>
        /// <returns>string - Connection token</returns>
        private async Task<string> GetTokenAsync()
        {
            const string tokenUrl = "https://idp.egov.kz/idp/sign-in";

            var response = await HttpClient.GetStringAsync(tokenUrl);

            // Generates document for AngleSharp
            // ReSharper disable once AccessToModifiedClosure
            var angleDocument = await new BrowsingContext(Configuration.Default).OpenAsync(x => x.Content(response));

            // Gets 'value' attribute of the page for authorization
            response = angleDocument.All.First(m => m.GetAttribute("id") == "xmlToSign").GetAttribute("value");

            return response;
        }

        /// <summary>
        /// Authorization to the system using sign
        /// </summary>
        private async Task AuthorizeAsync()
        {
            var token = await GetTokenAsync();

            //Signing token for authorization
            var signedToken = await SignXmlTokens.SignTokenAsync(token, Sign.auth, Sign.password, NcaNodeHost, NcaNodePort);

            var values = new Dictionary<string, string>
            {
                {"certificate", signedToken},
                {"lvl", "2"}
            };

            var content = new FormUrlEncodedContent(values);

            // Posting signed token to the camellia system
            await HttpClient.PostAsync("https://idp.egov.kz/idp/eds-login.do", content);
        }

        /// <summary>
        /// Get the information about connection of the client to the system
        /// </summary>
        /// <returns>true if the user logged in</returns>
        public async Task<bool> IsLoggedAsync()
        {
            //Trying to get information about user, if iin equals null it's likely that client not logged in
            var user = await GetUserAsync();
            return user.user_iin != null;
        }

        /// <summary>
        /// Loading user information from camellia system
        /// </summary>
        /// <param name="numberOfTries">Number of requests if some errors has been occured</param>
        /// <param name="delay">Time in millis between requests</param>
        /// <returns>UserInformation - Information about authorized user</returns>
        [Obsolete("GetUserInformationAsync is deprecated, there is no any scenarios where it could be used")]
        // ReSharper disable once UnusedMember.Local
        private async Task<UserInformation> GetUserInformationAsync(int numberOfTries = 3, int delay = 1500)
        {
            var response = new HttpResponseMessage();
            for (var i = 0; i < numberOfTries; i++)
            {
                response = await HttpClient.GetAsync("https://egov.kz/services/P30.11/rest/current-user");
                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                switch (response.StatusCode)
                {
                    case HttpStatusCode.Redirect:
                        Thread.Sleep(delay);
                        continue;
                    case HttpStatusCode.OK:
                        var result = await response.Content.ReadAsStringAsync();
                        var userInformation = JsonSerializer.Deserialize<UserInformation>(result);
                        return userInformation;
                    default:
                        throw new CamelliaRequestException(
                            $"StatusCode:'{response.StatusCode}';\nReasonPhrase:'{response.ReasonPhrase}';\nContent is '{response.Content}';");
                }
            }

            throw new CamelliaRequestException(
                $"StatusCode:'{response.StatusCode}';\nReasonPhrase:'{response.ReasonPhrase}';\nContent is '{response.Content}';");
        }

        /// <summary>
        /// Loading user data from camellia system
        /// </summary>
        /// <returns>User object - Information about user</returns>
        [Obsolete("GetUser is deprecated, there is no any scenarios where it could be used")]
        private async Task<User> GetUserAsync()
        {
            var result = await HttpClient.GetStringAsync("https://egov.kz/cms/auth/user.json");
            var user = JsonSerializer.Deserialize<User>(result);
            return user;
        }

        /// <summary>
        /// Logging out of camellia system
        /// </summary>
        public async Task LogoutAsync()
        {
            await HttpClient.GetAsync("https://egov.kz/cms/ru/auth/logout");
            User = null;
            CookieContainer = null;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Sign} | Proxy: '{Proxy.GetProxy(new Uri("http://egov.kz")).Host}'";
        }

        /// <summary>
        /// Disposing
        /// </summary>
        // public async void Dispose()
        // {
        //     await LogoutAsync();
        //     HttpClient.Dispose();
        // }
    }
}