using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using CamelliaManagementSystem.JsonObjects.ResponseObjects;
using CamelliaManagementSystem.SignManage;

#pragma warning disable 618

// ReSharper disable MemberCanBePrivate.Global
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
        public User User;

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
        public readonly Sign Sign;


        /// @author Yevgeniy Cherdantsev
        /// @date 10.03.2020 10:14:34
        /// <summary>
        /// Constructor for creating Camellia client with(out) proxy
        /// </summary>
        /// <param name="sign">AUTH and RSA signs of the user</param>
        /// <param name="webProxy">Proxy</param>
        /// <param name="httpClientTimeout">Timeout of http client connected to the camellia system; Standard: 15000</param>
        public CamelliaClient(Sign sign, IWebProxy webProxy = null, int httpClientTimeout = 15000)
        {
            Sign = sign;
            Proxy = webProxy;

            //If proxy equals null creates object without proxy and vice versa
            var handler = Proxy != null
                ? new HttpClientHandler {UseProxy = true, Proxy = Proxy}
                : new HttpClientHandler();

            handler.AllowAutoRedirect = true;
            handler.ServerCertificateCustomValidationCallback =
                (creation, of, insecure, connection) => true;

            CookieContainer = handler.CookieContainer;
            HttpClient = new HttpClient(handler) {Timeout = TimeSpan.FromMilliseconds(httpClientTimeout)};
        }

        /// @author Yevgeniy Cherdantsev
        /// @date 10.03.2020 10:15:18
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
                throw new CamelliaClientException($"Sign: '{Sign.iin}' hasn't been loaded");
        }

        /// @author Yevgeniy Cherdantsev
        /// @date 10.03.2020 10:15:18
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

        /// @author Yevgeniy Cherdantsev
        /// @date 10.03.2020 10:15:18
        /// <summary>
        /// Authorization to the system using sign
        /// </summary>
        private async Task AuthorizeAsync()
        {
            var token = await GetTokenAsync();

            //Signing token from authorization page
            var signedToken = await SignXmlTokens.SignTokenAsync(token, Sign.auth, Sign.password);

            var values = new Dictionary<string, string>
            {
                {"certificate", signedToken},
                {"lvl", "2"}
            };

            var content = new FormUrlEncodedContent(values);

            // Posting signed token to the camellia system
            await HttpClient.PostAsync("https://idp.egov.kz/idp/eds-login.do", content);
        }

        /// @author Yevgeniy Cherdantsev
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

        /// @author Yevgeniy Cherdantsev
        /// @date 10.03.2020 10:17:42
        /// <summary>
        /// Loading user information from camellia system
        /// </summary>
        /// <param name="numberOfTries">Number of requests if some errors has been occured</param>
        /// <param name="delay">Time in millis between requests</param>
        /// <returns>UserInformation - Information about authorized user</returns>
        [Obsolete("GetUserInformationAsync is deprecated, there is no any scenarios where it could be used")]
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
                        throw new HttpRequestException(
                            $"StatusCode:'{response.StatusCode}';\nReasonPhrase:'{response.ReasonPhrase}';\nContent is '{response.Content}';");
                }
            }

            throw new HttpRequestException(
                $"StatusCode:'{response.StatusCode}';\nReasonPhrase:'{response.ReasonPhrase}';\nContent is '{response.Content}';");
        }

        /// @author Yevgeniy Cherdantsev
        /// @date 29.06.2020 15:43:22
        /// <summary>
        /// Loading string user data from camellia system
        /// </summary>
        /// <returns>string - Information about user</returns>
        [Obsolete("GetUser is deprecated, there is no any scenarios where it could be used")]
        private async Task<User> GetUserAsync()
        {
            var result = await HttpClient.GetStringAsync("https://egov.kz/cms/auth/user.json");
            var user = JsonSerializer.Deserialize<User>(result);
            return user;
        }

        /// @author Yevgeniy Cherdantsev
        /// @date 07.03.2020 15:50:14
        /// <summary>
        /// Logging out of camellia system
        /// </summary>
        public async Task LogoutAsync()
        {
            // HttpClient can be already disposed, don't know the reason
            await HttpClient.GetAsync("https://egov.kz/cms/ru/auth/logout");
            User = null;
            CookieContainer = null;
        }


        /// @author Yevgeniy Cherdantsev
        /// @date 10.03.2020 10:19:28
        /// <summary>
        /// Disposing
        /// </summary>
        public async void Dispose()
        {
            await LogoutAsync();
            HttpClient.Dispose();
        }
    }

    /// <summary>
    /// Custom CamelliaClient exception
    /// </summary>
    [Serializable]
    public class CamelliaClientException : Exception
    {
        /// <inheritdoc />
        public CamelliaClientException()
        {
        }

        /// <inheritdoc />
        public CamelliaClientException(string message)
            : base(message)
        {
        }

        /// <inheritdoc />
        public CamelliaClientException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}