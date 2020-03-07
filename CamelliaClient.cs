using System;
using System.Collections.Generic;
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
        private readonly FullSign _fullSign;


        public CamelliaClient(FullSign fullSign)
        {
            _fullSign = fullSign;

            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = true
            };

            Connect(handler);
            UserInformation = GetUserInformation();
        }

        public CamelliaClient(FullSign fullSign, IWebProxy webProxy)
        {
            _fullSign = fullSign;

            var handler = new HttpClientHandler {AllowAutoRedirect = true, UseProxy = true, Proxy = webProxy};

            Connect(handler);
            UserInformation = GetUserInformation();
        }

        private void Connect(HttpMessageHandler handler)
        {
            HttpClient = new HttpClient(handler);
            HttpClient.GetStringAsync("https://www.egov.kz")
                .GetAwaiter()
                .GetResult();
            HttpClient = new HttpClient(handler);
            Authorize();
        }

        private string GetToken()
        {
            var response = HttpClient.GetStringAsync("https://idp.egov.kz/idp/sign-in")
                .GetAwaiter()
                .GetResult();
            response = response.Substring(response.IndexOf("id=\"xmlToSign\" value=\"") +
                                          "id=\"xmlToSign\" value=\"".Length);
            response = response.Substring(0, response.IndexOf("\" />"));
            response = response.Replace("&lt;", "<");
            response = response.Replace("&gt;", ">");
            response = response.Replace("&quot;", "" + "\"");
            return response;
        }

        private void Authorize()
        {
            var signedToken = SignXmlTokens.SignToken(GetToken(), _fullSign.AuthSign);

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

        private UserInformation GetUserInformation()
        {
            var res = HttpClient.GetStringAsync("https://egov.kz/services/P30.11/rest/current-user")
                .GetAwaiter()
                .GetResult();
            var userInformation = JsonSerializer.Deserialize<UserInformation>(res);
            return userInformation;
        }

        public void Logout()
        {
            HttpClient.GetAsync("https://egov.kz/cms/ru/auth/logout")
                .GetAwaiter()
                .GetResult();
        }

        public void Dispose()
        {
            Logout();
            HttpClient = null;
            UserInformation = null;
        }
    }
}