using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using Camellia_Management_System.JsonObjects;
using Camellia_Management_System.JsonObjects.RequestObjects;
using Camellia_Management_System.SignManage;

namespace Camellia_Management_System.Requests
{

    /// @author Yevgeniy Cherdantsev
    /// @date 14.03.2020 11:04:36
    /// @version 1.0
    /// <summary>
    /// INPUT
    /// </summary>
    public abstract class SingleInputCaptchaRequest : CamelliaCaptchaRequest
    {
        public SingleInputCaptchaRequest(CamelliaClient camelliaClient) : base(camelliaClient)
        {
        }
        
        public IEnumerable<ResultForDownload> GetReference(string input, SeleniumProvider seleniumProvider, int delay = 1000, int numOfCaptchaTries = 5)
        {
            if (input.Length==12 && !AdditionalRequests.IsBinRegistered(CamelliaClient, input))
                throw new InvalidDataException("This bin is not registered");
            
            var webDriver = seleniumProvider.GetDriver();
            // var captcha = GetCaptchaLink(webDriver);
            var captcha = "https://egov.kz/services/P30.03/captcha?"+(long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
            seleniumProvider.ReleaseDriver(webDriver);
            var tempDirectoryPath = Environment.GetEnvironmentVariable("TEMP");
            var filePath = $"{tempDirectoryPath}\\temp_captcha_{DateTime.Now.Ticks}.jpeg";
            var solvedCaptcha = "";
            for (var i = 0; i <= numOfCaptchaTries; i++)
            {
                if (i == numOfCaptchaTries)
                    throw new Exception($"Wrong captcha {i} times");
                DownloadCaptcha(captcha, filePath);
                solvedCaptcha = CaptchaSolver.SolveCaptcha(filePath);
                if (solvedCaptcha.Equals(""))
                    continue;

                if (CheckCaptcha(solvedCaptcha))
                    break;
            }

            var token = GetToken(input);

            token = JsonSerializer.Deserialize<TokenResponse>(token).xml;

            var signedToken = SignXmlTokens.SignToken(token, CamelliaClient.FullSign.RsaSign);
            var requestNumber = SendPdfRequest(signedToken, solvedCaptcha);
            var readinessStatus = WaitResult(requestNumber, delay);

            if (readinessStatus.status.Equals("APPROVED"))
                return readinessStatus.resultsForDownload;
            if (readinessStatus.status.Equals("REJECTED"))
                throw new Exception("REJECTED");

            throw new Exception($"Readiness status equals {readinessStatus.status}");
        }
        
        
        
    }
}