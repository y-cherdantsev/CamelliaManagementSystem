using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using Camellia_Management_System.JsonObjects;
using Camellia_Management_System.SignManage;

namespace Camellia_Management_System
{
    /// @author Yevgeniy Cherdantsev
    /// @date 07.03.2020 15:13:36
    /// @version 1.0
    /// <summary>
    /// Client for connecting to the service and using it
    /// </summary>
    public class CamelliaClient : IDisposable
    {
        public HttpClient HttpClient;
        public UserInformation UserInformation;
        public CookieContainer CookieContainer;
        internal readonly IWebProxy Proxy;
        internal readonly FullSign FullSign;


        /// @author Yevgeniy Cherdantsev
        /// @date 10.03.2020 10:14:34
        /// @version 1.0
        /// <summary>
        /// Constructor for creating Camellia client through proxy
        /// </summary>
        /// <param name="fullSign"></param>
        /// <param name="webProxy"></param>
        public CamelliaClient(FullSign fullSign, IWebProxy webProxy = null, int httpClientTimeout = 15000)
        {
            FullSign = fullSign;
            Proxy = webProxy;
            HttpClientHandler handler;
            handler = webProxy != null
                ? new HttpClientHandler {AllowAutoRedirect = true, UseProxy = true, Proxy = webProxy}
                : new HttpClientHandler {AllowAutoRedirect = true};
            CookieContainer = handler.CookieContainer;
            HttpClient = new HttpClient(handler);
            HttpClient.Timeout = TimeSpan.FromMilliseconds(httpClientTimeout);
            Connect();
            UserInformation = GetUserInformation();
        }

        public static bool IsCorrect(Sign sign, string bin, IWebProxy webProxy = null)
        {
            //TODO (enum)
            try
            {
                if (!new FileInfo(sign.FilePath).Exists)
                    throw new FileNotFoundException();
                bin = bin.PadLeft(12, '0');
                var camelliaClient = new CamelliaClient(new FullSign {AuthSign = sign}, webProxy);
                return camelliaClient.UserInformation.uin.PadLeft(12, '0') == bin;
            }
            catch (FileNotFoundException e)
            {
                throw;
            }
            catch (Exception e)
            {
                if (e.Message == "Some error occured while loading the key storage")
                    throw new InvalidDataException("Incorrect password");
                throw new InvalidDataException("Service unavailable");
            }
        }

        /// @author Yevgeniy Cherdantsev
        /// @date 10.03.2020 10:15:18
        /// @version 1.0
        /// <summary>
        /// Connects to the cammelia system using handler
        /// </summary>
        /// <param name="handler"></param>
        private void Connect()
        {
            HttpClient.GetStringAsync("https://www.egov.kz")
                .GetAwaiter()
                .GetResult();
            Authorize();
        }

        /// @author Yevgeniy Cherdantsev
        /// @date 10.03.2020 10:15:18
        /// @version 1.0
        /// <summary>
        /// Request of the connection token
        /// </summary>
        /// <returns>string - Connection token</returns>
        private string GetToken()
        {
            var response = HttpClient.GetStringAsync("https://idp.egov.kz/idp/sign-in")
                .GetAwaiter()
                .GetResult();
            response = response.Substring(response.IndexOf("id=\"xmlToSign\" value=\"", StringComparison.Ordinal) +
                                          "id=\"xmlToSign\" value=\"".Length);
            response = response.Substring(0, response.IndexOf("\" />"));
            response = response.Replace("&lt;", "<");
            response = response.Replace("&gt;", ">");
            response = response.Replace("&quot;", "" + "\"");
            return response;
        }


        /// @author Yevgeniy Cherdantsev
        /// @date 10.03.2020 10:15:18
        /// @version 1.0
        /// <summary>
        /// Authorization to the system using sign
        /// </summary>
        private void Authorize()
        {
            var signedToken = SignXmlTokens.SignToken(GetToken(), FullSign.AuthSign);

            var values = new Dictionary<string, string>
            {
                {"certificate", $"{signedToken}"},
                {"username", ""},
                {"lvl", "7"},
                {"url", ""},
            };

            var content = new FormUrlEncodedContent(values);

            HttpClient.PostAsync("https://idp.egov.kz/idp/eds-login.do", content).GetAwaiter().GetResult();
        }

        public bool IsLogged()
        {
            try
            {
                GetUserInformation();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }


        /// @author Yevgeniy Cherdantsev
        /// @date 10.03.2020 10:17:42
        /// @version 1.0
        /// <summary>
        /// Loading user information from camellia system
        /// </summary>
        /// <returns>UserInformation - Information about authorized user</returns>
        private UserInformation GetUserInformation()
        {
            var res = HttpClient.GetStringAsync("https://egov.kz/services/P30.11/rest/current-user")
                .GetAwaiter()
                .GetResult();
            var userInformation = JsonSerializer.Deserialize<UserInformation>(res);
            return userInformation;
        }


        /// @author Yevgeniy Cherdantsev
        /// @date 07.03.2020 15:50:14
        /// @version 1.0
        /// <summary>
        /// Logging out of camellia system
        /// </summary>
        public void Logout()
        {
            try
            {
                HttpClient.GetAsync("https://egov.kz/cms/ru/auth/logout")
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception e)
            {
                //ignore
            }
        }


        /// @author Yevgeniy Cherdantsev
        /// @date 10.03.2020 10:19:28
        /// @version 1.0
        /// <summary>
        /// Disposing
        /// </summary>
        public void Dispose()
        {
            Logout();
            HttpClient = null;
            UserInformation = null;
        }
    }
}