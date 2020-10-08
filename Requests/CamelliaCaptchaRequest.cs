using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CamelliaManagementSystem.JsonObjects.RequestObjects;

// ReSharper disable CommentTypo

namespace CamelliaManagementSystem.Requests
{
    /// @author Yevgeniy Cherdantsev
    /// @date 14.03.2020 11:25:15
    /// <summary>
    /// Request that requires captcha
    /// </summary>
    public abstract class CamelliaCaptchaRequest : CamelliaRequest
    {
        /// <inheritdoc />
        protected CamelliaCaptchaRequest(CamelliaClient camelliaClient) : base(camelliaClient)
        {
        }

        /// <summary>
        /// Download captcha locally from camellia system
        /// </summary>
        /// <param name="captchaLink">Get link for captcha</param>
        /// <param name="path">Local path</param>
        protected async Task DownloadCaptchaAsync(string captchaLink, string path)
        {
            var response = await CamelliaClient.HttpClient.GetAsync(captchaLink);

            var inputStream = await response.Content.ReadAsStreamAsync();
            await using (var outputFileStream = new FileStream(path, FileMode.Create))
            {
                await inputStream.CopyToAsync(outputFileStream);
                await outputFileStream.FlushAsync();
                outputFileStream.Close();
            }

            await inputStream.FlushAsync();
            inputStream.Close();
            await inputStream.DisposeAsync();
        }

        /// <summary>
        /// Checks if captcha solved correctly
        /// </summary>
        /// <param name="solvedCaptcha">Captcha solution</param>
        /// <returns>True if answer wat correct</returns>
        protected async Task<bool> CheckCaptchaAsync(string solvedCaptcha)
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

            var response = await CamelliaClient.HttpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            return responseContent.Contains("\"rightCaptcha\":true");
        }
    }
}