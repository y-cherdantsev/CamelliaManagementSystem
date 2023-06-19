using System;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CamelliaManagementSystem.JsonObjects.RequestObjects;

// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable UnusedMember.Local

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
        private async Task DownloadCaptchaAsync(string captchaLink, string path)
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
        /// Get captcha stream from camellia system
        /// </summary>
        /// <param name="captchaLink">Get link for captcha</param>
        private async Task<Stream> GetCaptchaStream(string captchaLink)
        {
            var response = await CamelliaClient.HttpClient.GetAsync(captchaLink);
            var stream = await response.Content.ReadAsStreamAsync();
            return stream;
        }

        /// <summary>
        /// Checks if captcha solved correctly
        /// </summary>
        /// <param name="solvedCaptcha">Captcha solution</param>
        /// <returns>True if answer wat correct</returns>
        private async Task<bool> CheckCaptchaAsync(string solvedCaptcha)
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

        /// <summary>
        /// Requests and activates captcha
        /// </summary>
        /// <param name="captchaApiKey">API Key for solving captchas</param>
        /// <param name="numOfCaptchaTries">Number of attempts while solving captchas</param>
        /// <returns>Solved captcha</returns>
        /// <exception cref="CamelliaCaptchaSolverException">If some error occured while solving captcha</exception>
        protected async Task<string> PerformCaptcha(string captchaApiKey, int numOfCaptchaTries)
        {
            //Get captcha
            var captchaLink = $"{RequestLink()}captcha?" +
                              (long) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;

            // Solve captcha
            var solvedCaptcha = "";
            for (var i = 0; i <= numOfCaptchaTries; i++)
            {
                if (i == numOfCaptchaTries)
                    throw new CamelliaCaptchaSolverException($"Wrong captcha {i} times");
                var captchaStream = await GetCaptchaStream(captchaLink);
                solvedCaptcha = CaptchaSolver.SolveCaptcha(captchaStream, captchaApiKey);
                if (string.IsNullOrEmpty(solvedCaptcha))
                    continue;

                try
                {
                    if (await CheckCaptchaAsync(solvedCaptcha))
                        break;
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return solvedCaptcha;
        }
    }
}