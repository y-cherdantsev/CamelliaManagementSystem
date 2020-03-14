using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using EgovFoundersRequest.JsonObjects;
using OpenQA.Selenium;

namespace Camellia_Management_System.Requests
{

    /// @author Yevgeniy Cherdantsev
    /// @date 14.03.2020 11:25:15
    /// @version 1.0
    /// <summary>
    /// INPUT
    /// </summary>
    /// <code>
    /// 
    /// </code>


    public abstract class CamelliaCaptchaRequest : CamelliaRequest
    {
        protected CamelliaCaptchaRequest(CamelliaClient camelliaClient) : base(camelliaClient)
        {
        }

        protected void DownloadCaptcha(string captchaLink, string path)
        {
            var response = CamelliaClient.HttpClient.GetAsync(captchaLink).GetAwaiter().GetResult();

            var filePath = path;

            var inputStream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
            using var outputFileStream = new FileStream(filePath, FileMode.Create);
            inputStream.CopyTo(outputFileStream);
        }

        protected bool CheckCaptcha(string solvedCaptcha)
        {
            using var request = new HttpRequestMessage(new HttpMethod("POST"),
                $"{RequestLink()}rest/captcha/check-captcha");
            request.Headers.Add("Connection", "keep-alive");
            request.Headers.Add("Accept", "application/json, text/plain, */*");
            request.Headers.Add("Origin", "https://egov.kz");
            request.Headers.Add("Sec-Fetch-Site", "same-origin");
            request.Headers.Add("Sec-Fetch-Mode", "cors");
            request.Headers.Add("User-Agent", "Mozilla5.0 Windows NT 10.0");
            request.Headers.Add("Referer", $"{RequestLink()}");
            request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            request.Headers.Add("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");

            var json = JsonSerializer.Serialize(new Captcha(solvedCaptcha));
            request.Content =
                new StringContent(json, Encoding.UTF8, "application/json");

            var response = CamelliaClient.HttpClient.SendAsync(request).GetAwaiter().GetResult().Content.ReadAsStringAsync()
                .GetAwaiter().GetResult();
            return response.Contains("\"rightCaptcha\":true");
        }

        protected string GetCaptchaLink(IWebDriver webDriver)
        {
            foreach (Cookie cookie in CamelliaClient.CookieContainer.GetCookies(new Uri("https://www.egov.kz")).Cast<Cookie>())
            {
                // var selCookie = new OpenQA.Selenium.Cookie(cookie.Name, cookie.Value, cookie.Domain, cookie.Path,
                // cookie.Expires);
                var selCookie = new OpenQA.Selenium.Cookie(cookie.Name, cookie.Value, cookie.Path);
                webDriver.Manage().Cookies.AddCookie(selCookie);
            }

            webDriver.Navigate().GoToUrl("https://egov.kz/services/P30.03/#/declaration/0/,/");
            var src = webDriver.FindElement(By.Id("captcha_picture")).GetAttribute("src");

            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                CookieContainer = new CookieContainer()
            };
            _egov.CookieContainer = handler.CookieContainer;

            foreach (var cookie in webDriver.Manage().Cookies.AllCookies)
            {
                _egov.CookieContainer.Add(new Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain));
            }

            _egov.HttpClient = new HttpClient(handler);
            webDriver.Close();
            return src;
        }
    }
}