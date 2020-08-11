using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Camellia_Management_System.JsonObjects.RequestObjects;
using Camellia_Management_System.Requests;

//TODO(REFACTOR)
namespace CamelliaManagementSystem.Requests
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
            using (var outputFileStream = new FileStream(filePath, FileMode.Create))
            {
                inputStream.CopyTo(outputFileStream);
                outputFileStream.Flush();
                outputFileStream.Close();
            }
            inputStream.Flush();
            inputStream.Close();
            inputStream.Dispose();
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

            var response = CamelliaClient.HttpClient.SendAsync(request).GetAwaiter().GetResult().Content
                .ReadAsStringAsync()
                .GetAwaiter().GetResult();
            return response.Contains("\"rightCaptcha\":true");
        }
    }
}