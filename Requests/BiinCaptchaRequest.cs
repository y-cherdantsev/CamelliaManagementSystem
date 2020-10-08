using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using CamelliaManagementSystem.JsonObjects.ResponseObjects;
using CamelliaManagementSystem.SignManage;

// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable PublicConstructorInAbstractClass

namespace CamelliaManagementSystem.Requests
{
    /// @author Yevgeniy Cherdantsev
    /// @date 14.03.2020 11:04:36
    /// <summary>
    /// Request with BIIN and captcha
    /// </summary>
    public abstract class BiinCaptchaRequest : CamelliaCaptchaRequest
    {
        /// <inheritdoc />
        public BiinCaptchaRequest(CamelliaClient camelliaClient) : base(camelliaClient)
        {
        }

        /// <summary>
        /// Gets results for downloading
        /// </summary>
        /// <param name="input">Given BIIN or another input</param>
        /// <param name="captchaApiKey">API Key for captcha solving</param>
        /// <param name="delay">Delay between requests while waiting result</param>
        /// <param name="timeout">Timeout while waiting result</param>
        /// <param name="numOfCaptchaTries">Number of attempts while trying to solve captcha</param>
        /// <returns>Results for downloading</returns>
        /// <exception cref="CamelliaNoneDataException">If data not presented in camellia system</exception>
        /// <exception cref="CamelliaRequestException">If some error with camellia system occured</exception>
        /// <exception cref="CamelliaUnknownException">Unknown status or whatever</exception>
        // ReSharper disable once CognitiveComplexity
        public async Task<IEnumerable<ResultForDownload>> GetReferenceAsync(string input, string captchaApiKey, int delay = 1000,
            int timeout = 60000, int numOfCaptchaTries = 5)
        {
            input = input.PadLeft(12, '0');
            if (TypeOfBiin() == BiinType.BIN)
            {
                if (!await AdditionalRequests.IsBinRegisteredAsync(CamelliaClient, input))
                    throw new CamelliaNoneDataException("This bin is not registered");
            }
            else
            {
                if (!await AdditionalRequests.IsIinRegisteredAsync(CamelliaClient, input))
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
                    throw new CamelliaCaptchaSolverException($"Wrong captcha {i} times");
                await DownloadCaptchaAsync(captcha, filePath);
                solvedCaptcha = CaptchaSolver.SolveCaptcha(filePath, captchaApiKey);
                if (string.IsNullOrEmpty(solvedCaptcha))
                    continue;

                for (var j = 0; j < 3; j++)
                {

                    try
                    {
                        if (await CheckCaptchaAsync(solvedCaptcha))
                            goto gotoFlag;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            gotoFlag:
            var token = await GetTokenAsync(input);
            
            try
            {
                token = JsonSerializer.Deserialize<Token>(token).xml;
            }
            catch (Exception)
            {
                if (token.Contains("<h1>405 Not Allowed</h1>"))
                    throw new InvalidDataException("Not allowed or some problem with egov occured");
                throw;
            }

            var signedToken =  SignXmlTokens.SignTokenAsync(token, CamelliaClient.Sign.rsa, CamelliaClient.Sign.password).Result;
            var requestNumber = await SendPdfRequestAsync(signedToken, solvedCaptcha);
            var readinessStatus =await WaitResultAsync(requestNumber.requestNumber, delay, timeout);

            if (readinessStatus.status.Equals("APPROVED"))
                return readinessStatus.resultsForDownload;
            if (readinessStatus.status.Equals("REJECTED"))
                throw new CamelliaNoneDataException("REJECTED");

            throw new CamelliaUnknownException($"Readiness status equals {readinessStatus.status}");
        }
    }
}