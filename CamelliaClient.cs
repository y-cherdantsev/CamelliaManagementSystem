using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AngleSharp;
using Camellia_Management_System.JsonObjects;
using Camellia_Management_System.JsonObjects.ResponseObjects;
using Camellia_Management_System.SignManage;

//TODO(REFACTOR)
namespace Camellia_Management_System
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
        public HttpClient HttpClient;

        /// <summary>
        /// Information about user
        /// </summary>
        public UserInformation UserInformation;

        /// <summary>
        /// Container of cookies
        /// </summary>
        public CookieContainer CookieContainer;

        /// <summary>
        /// Proxy if need
        /// </summary>
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
                ? new HttpClientHandler {AllowAutoRedirect = true, UseProxy = true, Proxy = Proxy}
                : new HttpClientHandler {AllowAutoRedirect = true};

            CookieContainer = handler.CookieContainer;
            HttpClient = new HttpClient(handler) {Timeout = TimeSpan.FromMilliseconds(httpClientTimeout)};
        }
        
        /// @author Yevgeniy Cherdantsev
        /// @date 10.03.2020 10:15:18
        /// <summary>
        /// Connects to the camellia system using handler
        /// </summary>
        public async Task Login()
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
                await HttpClient.GetStringAsync(url);
            }

            //Runs authorization script
            await Authorize();

            UserInformation = await GetUserInformation();
        }

        /// @author Yevgeniy Cherdantsev
        /// @date 10.03.2020 10:15:18
        /// <summary>
        /// Request of the connection token
        /// </summary>
        /// <returns>string - Connection token</returns>
        private async Task<string> GetToken()
        {
            const string tokenUrl = "https://idp.egov.kz/idp/sign-in";

            var response = await HttpClient.GetStringAsync(tokenUrl);

            //Generates document for AngleSharp
            var angleDocument = await new BrowsingContext(Configuration.Default).OpenAsync(x => x.Content(response));

            //Gets 'value' attribute of the page for authorization
            response = angleDocument.All.First(m => m.GetAttribute("id") == "xmlToSign").GetAttribute("value");

            return response;
        }


        /// @author Yevgeniy Cherdantsev
        /// @date 10.03.2020 10:15:18
        /// <summary>
        /// Authorization to the system using sign
        /// </summary>
        private async Task Authorize()
        {
            //Signing token from authorization page
            var signedToken = SignXmlTokens.SignToken(await GetToken(), FullSign.authSign);

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
        public async Task<bool> IsLogged()
        {
            //TODO(Rethink function, it shouldn't depend on errors. If error won't tide to logging information, it could be lost)
            
            //Trying to get information about user, if error occured it's likely that client not logged in
            try
            {
                await GetUserInformation();
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
        /// <returns>UserInformation - Information about authorized user</returns>
        private async Task<UserInformation> GetUserInformation()
        {
            var result = await HttpClient.GetStringAsync("https://egov.kz/services/P30.11/rest/current-user");
            var userInformation = JsonSerializer.Deserialize<UserInformation>(result);
            return userInformation;
        }

        /// @author Yevgeniy Cherdantsev
        /// @date 29.06.2020 15:43:22
        /// <summary>
        /// Loading string user data from camellia system
        /// </summary>
        /// <returns>string - Information about user</returns>
        [Obsolete("GetUser is deprecated, there is no any scenarios where it could be used")]
        private async Task<string> GetUser()
        {
            var res = await HttpClient.GetStringAsync("https://egov.kz/cms/auth/user.json");
            return res;
        }


        /// @author Yevgeniy Cherdantsev
        /// @date 07.03.2020 15:50:14
        /// <summary>
        /// Logging out of camellia system
        /// </summary>
        public async Task Logout()
        {
            await HttpClient.GetAsync("https://egov.kz/cms/ru/auth/logout");
            UserInformation = null;
            CookieContainer = null;
        }


        /// @author Yevgeniy Cherdantsev
        /// @date 10.03.2020 10:19:28
        /// <summary>
        /// Disposing
        /// </summary>
        public async void Dispose()
        {
            await Logout();
            HttpClient.Dispose();
            FullSign.Dispose();
        }
    }
}