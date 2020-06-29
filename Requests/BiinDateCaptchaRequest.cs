using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Camellia_Management_System.JsonObjects;
using Camellia_Management_System.JsonObjects.ResponseObjects;
using Camellia_Management_System.SignManage;
//TODO(REFACTOR)
namespace Camellia_Management_System.Requests
{
    public abstract class BiinDateCaptchaRequest : CamelliaCaptchaRequest
    {
        protected BiinDateCaptchaRequest(CamelliaClient camelliaClient) : base(camelliaClient)
        {
        }

        public IEnumerable<ResultForDownload> GetReference(string input, DateTime date, string captchaApiKey, int delay = 1000,
            int timeout = 60000, int numOfCaptchaTries = 5)
        {
            var stringDate = $"{date.Year}-{date.Month}-{date.Day}T18:00:00.000Z";
            input = input.PadLeft(12, '0');
            if (TypeOfBiin() == BiinType.BIN)
            {
                if (input.Length == 12 && !AdditionalRequests.IsBinRegistered(CamelliaClient, input))
                    throw new InvalidDataException("This bin is not registered");
            }
            else
            {
                if (input.Length == 12 && !AdditionalRequests.IsIinRegistered(CamelliaClient, input))
                    throw new InvalidDataException("This Iin is not registered");
            }

            var captcha = $"{RequestLink()}captcha?" +
                          (long) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
            var tempDirectoryPath = Environment.GetEnvironmentVariable("TEMP");
            var filePath = $"{tempDirectoryPath}\\temp_captcha_{DateTime.Now.Ticks}.jpeg";
            var solvedCaptcha = "";
            for (var i = 0; i <= numOfCaptchaTries; i++)
            {
                if (i == numOfCaptchaTries)
                    throw new Exception($"Wrong captcha {i} times");
                DownloadCaptcha(captcha, filePath);
                solvedCaptcha = CaptchaSolver.SolveCaptcha(filePath, captchaApiKey);
                if (solvedCaptcha.Equals(""))
                    continue;

                if (CheckCaptcha(solvedCaptcha))
                    break;
            }

            var token = GetToken(input, stringDate);
            
            try
            {
                token = JsonSerializer.Deserialize<TokenResponse>(token).xml;
            }
            catch (Exception)
            {
                if (token.Contains("<h1>405 Not Allowed</h1>"))
                    throw new InvalidDataException("Not allowed or some problem with egov occured");
                throw;
            }

            var signedToken = SignXmlTokens.SignToken(token, CamelliaClient.FullSign.RsaSign);
            var requestNumber = SendPdfRequest(signedToken, solvedCaptcha);
            var readinessStatus = WaitResult(requestNumber, delay, timeout);

            if (readinessStatus.status.Equals("APPROVED"))
                return readinessStatus.resultsForDownload;
            if (readinessStatus.status.Equals("REJECTED"))
                throw new InvalidDataException("REJECTED");

            throw new InvalidDataException($"Readiness status equals {readinessStatus.status}");
        }
        
        protected string GetToken(string biin, string date)
        {
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

            string json = "";
            if(TypeOfBiin() == BiinType.BIN)
                json = JsonSerializer.Serialize(new BinDateDeclarant(biin, CamelliaClient.UserInformation.uin, date));
            // else
                // json = JsonSerializer.Serialize(new IinDateDeclarant(biin, CamelliaClient.UserInformation.uin, date));

            request.Content =
                new StringContent(json, Encoding.UTF8, "application/json");
            var response = CamelliaClient.HttpClient.SendAsync(request).GetAwaiter().GetResult();
            return response.Content.ReadAsStringAsync().Result;
        }

    }
}