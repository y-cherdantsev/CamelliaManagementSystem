﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Io;
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
        public string name => new FileInfo(FullSign.AuthSign.FilePath).Directory?.Name;


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

       

        /// @author Yevgeniy Cherdantsev
        /// @date 10.03.2020 10:15:18
        /// @version 1.0
        /// <summary>
        /// Connects to the cammelia system using handler
        /// </summary>
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
            var document = new BrowsingContext(Configuration.Default).OpenAsync(x => x.Content(response)).Result;
            response = document.All.First(m => m.GetAttribute("id")=="xmlToSign").GetAttribute("value");
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

        /// @author Yevgeniy Cherdantsev
        /// @version 1.0
        /// <summary>
        /// Get the information about connection of the client to the system
        /// </summary>
        /// <returns>true if the user logged in</returns>
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
            catch (Exception)
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