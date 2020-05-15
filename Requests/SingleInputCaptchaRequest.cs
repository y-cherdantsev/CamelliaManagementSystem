using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Camellia_Management_System.JsonObjects;
using Camellia_Management_System.JsonObjects.ResponseObjects;
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

        public IEnumerable<ResultForDownload> GetReference(string input, string captchaApiKey, int delay = 1000,
            int timeout = 60000, int numOfCaptchaTries = 5)
        {
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

                for (var j = 0; j < 3; j++)
                {

                    try
                    {
                        if (CheckCaptcha(solvedCaptcha))
                            goto gotoFlag;
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
            gotoFlag:
            var token = GetToken(input);
            
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
    }
}