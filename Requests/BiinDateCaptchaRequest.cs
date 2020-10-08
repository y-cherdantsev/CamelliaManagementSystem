using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CamelliaManagementSystem.JsonObjects.RequestObjects;
using CamelliaManagementSystem.JsonObjects.ResponseObjects;
using CamelliaManagementSystem.SignManage;

// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo

namespace CamelliaManagementSystem.Requests
{
    /// @author Yevgeniy Cherdantsev
    /// @date 07.03.2020 16:35:25
    /// <summary>
    /// Request with BIIN, date and captcha
    /// </summary>
    public abstract class BiinDateCaptchaRequest : CamelliaCaptchaRequest
    {
        /// <inheritdoc />
        protected BiinDateCaptchaRequest(CamelliaClient camelliaClient) : base(camelliaClient)
        {
        }

        /// <summary>
        /// Gets results for downloading
        /// </summary>
        /// <param name="input">Given BIIN or another input</param>
        /// <param name="date">Given date</param>
        /// <param name="captchaApiKey">API Key for captcha solving</param>
        /// <param name="delay">Delay between requests while waiting result</param>
        /// <param name="timeout">Timeout while waiting result</param>
        /// <param name="numOfCaptchaTries">Number of attempts while trying to solve captcha</param>
        /// <returns>Results for downloading</returns>
        /// <exception cref="CamelliaNoneDataException">If data not presented in camellia system</exception>
        /// <exception cref="CamelliaRequestException">If some error with camellia system occured</exception>
        /// <exception cref="CamelliaUnknownException">Unknown status or whatever</exception>
        // ReSharper disable once CognitiveComplexity
        public async Task<IEnumerable<ResultForDownload>> GetReferenceAsync(string input, DateTime date,
            string captchaApiKey, int delay = 1000,
            int timeout = 60000, int numOfCaptchaTries = 5)
        {
            input = input.PadLeft(12, '0');
            if (TypeOfBiin() == BiinType.BIN)
            {
                if (input.Length == 12 && !await AdditionalRequests.IsBinRegisteredAsync(CamelliaClient, input))
                    throw new CamelliaNoneDataException("This bin is not registered");
            }
            else
            {
                if (input.Length == 12 && !await AdditionalRequests.IsIinRegisteredAsync(CamelliaClient, input))
                    throw new CamelliaNoneDataException("This Iin is not registered");
            }

            var captcha = $"{RequestLink()}captcha?" +
                          (long) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
            var tempDirectoryPath = Environment.GetEnvironmentVariable("TEMP");
            var filePath = $"{tempDirectoryPath}\\temp_captcha_{DateTime.Now.Ticks}.jpeg";
            var solvedCaptcha = "";
            for (var i = 0; i <= numOfCaptchaTries; i++)
            {
                if (i == numOfCaptchaTries)
                    throw new CamelliaRequestException($"Wrong captcha {i} times");
                await DownloadCaptchaAsync(captcha, filePath);
                solvedCaptcha = CaptchaSolver.SolveCaptcha(filePath, captchaApiKey);
                if (string.IsNullOrEmpty(solvedCaptcha))
                    continue;

                if (await CheckCaptchaAsync(solvedCaptcha))
                    break;
            }

            var token = await GetTokenAsync(input, date);

            try
            {
                token = JsonSerializer.Deserialize<Token>(token).xml;
            }
            catch (Exception)
            {
                if (token.Contains("<h1>405 Not Allowed</h1>"))
                    throw new CamelliaRequestException("Not allowed or some problem with egov occured");
                throw;
            }

            var signedToken =
                await SignXmlTokens.SignTokenAsync(token, CamelliaClient.Sign.rsa, CamelliaClient.Sign.password);
            var requestNumber = await SendPdfRequestAsync(signedToken, solvedCaptcha);
            var readinessStatus = await WaitResultAsync(requestNumber.requestNumber, delay, timeout);

            if (readinessStatus.status.Equals("APPROVED"))
                return readinessStatus.resultsForDownload;
            if (readinessStatus.status.Equals("REJECTED"))
                throw new CamelliaNoneDataException("REJECTED");

            throw new CamelliaUnknownException($"Readiness status equals {readinessStatus.status}");
        }

        /// <summary>
        /// Gets token for signing
        /// </summary>
        /// <param name="biin">User biin</param>
        /// <param name="date">Date for request</param>
        /// <returns>Token</returns>
        private async Task<string> GetTokenAsync(string biin, DateTime date)
        {
            var stringDate = $"{date.Year}-{date.Month}-{date.Day}T18:00:00.000Z";
            using var request = new HttpRequestMessage(new HttpMethod("POST"),
                $"{RequestLink()}/rest/app/xml");
            request.Headers.Add("Connection", "keep-alive");
            request.Headers.Add("Cache-Control", "max-age=0");
            request.Headers.Add("Origin", "https://idp.egov.kz");
            request.Headers.Add("User-Agent", "Mozilla5.0 Windows NT 10.0");
            request.Headers.Add("Sec-Fetch-Site", "same-origin");
            request.Headers.Add("Sec-Fetch-Mode", "navigate");
            request.Headers.Add("Referer", "https://idp.egov.kz/idp/sign-in");
            request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            request.Headers.Add("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");

            var json = string.Empty;
            if (TypeOfBiin() == BiinType.BIN)
                json = JsonSerializer.Serialize(new BinDateDeclarant(biin, CamelliaClient.User.user_iin, stringDate));
            // else
            // json = JsonSerializer.Serialize(new IinDateDeclarant(biin, CamelliaClient.UserInformation.uin, date));

            request.Content =
                new StringContent(json, Encoding.UTF8, "application/json");
            var response = await CamelliaClient.HttpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }
    }
}