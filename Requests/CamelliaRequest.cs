using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using CamelliaManagementSystem.JsonObjects;
using CamelliaManagementSystem.JsonObjects.RequestObjects;
using CamelliaManagementSystem.JsonObjects.ResponseObjects;

// ReSharper disable StringLiteralTypo
// ReSharper disable InconsistentNaming
// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
// ReSharper disable PublicConstructorInAbstractClass

#pragma warning disable 1591
namespace CamelliaManagementSystem.Requests
{
    /// @author Yevgeniy Cherdantsev
    /// @date 07.03.2020 15:49:33
    /// <summary>
    /// Parent Request Class
    /// </summary>
    public abstract class CamelliaRequest
    {
        internal readonly CamelliaClient CamelliaClient;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="camelliaClient">Camellia Client that will proceed request</param>
        public CamelliaRequest(CamelliaClient camelliaClient)
        {
            CamelliaClient = camelliaClient;
        }

        /// <summary>
        /// Link for getting reference
        /// </summary>
        /// <returns>Url</returns>
        protected abstract string RequestLink();

        /// <summary>
        /// Type of reference (BIN or IIN)
        /// </summary>
        /// <returns>Enum BiinType</returns>
        protected abstract BiinType TypeOfBiin();

        /// <summary>
        /// Requests camellia system about reference readiness
        /// </summary>
        /// <param name="requestNumber">Number of request</param>
        /// <returns>Status of reference readiness</returns>
        /// <exception cref="InvalidDataException">If some error with cammelia occured</exception>
        private ReadinessStatus GetReadinessStatus(string requestNumber)
        {
            try
            {
                var result = CamelliaClient.HttpClient
                    .GetStringAsync($"{RequestLink()}/rest/request-states/{requestNumber}")
                    .GetAwaiter()
                    .GetResult();
                var readinessStatus = JsonSerializer.Deserialize<ReadinessStatus>(result);
                return readinessStatus;
            }
            catch (Exception)
            {
                throw new InvalidDataException("It seems that camellia rejected request");
            }
        }

        /// <summary>
        /// Waits for readiness result
        /// </summary>
        /// <param name="requestNumber">Number of request</param>
        /// <param name="delay">Delay between getting current readiness status</param>
        /// <param name="timeout">Timeout of </param>
        /// <returns>Status of reference readiness</returns>
        /// <exception cref="InvalidDataException">If some error with cammelia occured</exception>
        protected ReadinessStatus WaitResult(string requestNumber, int delay = 1000, int timeout = 60000)
        {
            // Minimum delay is 1s
            delay = delay < 500 ? 1000 : delay;

            // Counts number of requests that will be proceeded
            var wait = timeout / delay;

            ReadinessStatus readinessStatus;

            do
            {
                if (wait-- <= 0)
                    throw new InvalidDataException($"Timeout '{timeout}' exceeded");
                Thread.Sleep(delay);
                readinessStatus = GetReadinessStatus(requestNumber);
            } while (readinessStatus.status.Equals("IN_PROCESSING"));

            if (readinessStatus.status.Equals("APPROVED"))
                return readinessStatus;

            throw new InvalidDataException($"Readiness status equals {readinessStatus.status}");
        }


        /// <summary>
        /// Returns true if request has been denied
        /// </summary>
        /// <param name="requestNumber">Number of request</param>
        /// <returns></returns>
        [Obsolete("IsDenied is deprecated, there is no any scenarios where it could be used")]
        protected bool IsDenied(string requestNumber)
        {
            var response = CamelliaClient.HttpClient
                .GetStringAsync(
                    $"https://egov.kz/services/P30.03/rest/like/{requestNumber}/{CamelliaClient.UserInformation.uin}")
                .GetAwaiter()
                .GetResult();
            return response.Contains("DENIED");
        }

        /// <summary>
        /// Sends request to get reference
        /// </summary>
        /// <param name="signedToken"></param>
        /// <param name="solvedCaptcha"></param>
        /// <returns>RequestNumber</returns>
        protected RequestNumber SendPdfRequest(string signedToken, string solvedCaptcha = null)
        {
            // If request needs captcha adds captchaCode
            var requestUri = solvedCaptcha == null
                ? $"{RequestLink()}rest/app/send-eds"
                : $"{RequestLink()}rest/app/send-eds?captchaCode={solvedCaptcha}";

            using var request = new HttpRequestMessage(new HttpMethod("POST"), requestUri);
            request.Headers.Add("Connection", "keep-alive");
            request.Headers.Add("Accept", "application/json, text/plain, */*");
            request.Headers.Add("Cache-Control", "max-age=0");
            request.Headers.Add("Origin", "https://egov.kz");
            request.Headers.Add("User-Agent", "Mozilla5.0 Windows NT 10.0");
            request.Headers.Add("Sec-Fetch-Site", "same-origin");
            request.Headers.Add("Sec-Fetch-Mode", "cors");
            request.Headers.Add("Referer", $"{RequestLink()}");
            request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            request.Headers.Add("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
            request.Headers.Add("Cookie", "sign.certType=file;sign.file=null");

            var jsonXmlToken = JsonSerializer.Serialize(new XMLToken(signedToken));

            request.Content =
                new StringContent(jsonXmlToken, Encoding.UTF8, "application/json");
            var response = CamelliaClient.HttpClient
                .SendAsync(request).GetAwaiter().GetResult()
                .Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var requestNumber = JsonSerializer.Deserialize<RequestNumber>(response);

            return requestNumber;
        }

        /// <summary>
        /// Gets token for sending reference request
        /// </summary>
        /// <param name="biin">Biin of reference that should be taken</param>
        /// <param name="stringDate">Date in string format if needed</param>
        /// <returns>XmlToken</returns>
        protected string GetToken(string biin, string stringDate = null)
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

            var jsonDeclarant = string.Empty;

            // Generates declarant based on known values
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (TypeOfBiin())
            {
                case BiinType.BIN:
                    if (stringDate != null)
                        jsonDeclarant = JsonSerializer.Serialize(new BinDateDeclarant(biin, stringDate,
                            CamelliaClient.UserInformation.uin));
                    else
                        jsonDeclarant =
                            JsonSerializer.Serialize(new BinDeclarant(biin, CamelliaClient.UserInformation.uin));
                    break;
                case BiinType.IIN:
                    if (stringDate != null)
                        jsonDeclarant = JsonSerializer.Serialize(new IinDateDeclarant(biin, stringDate,
                            CamelliaClient.UserInformation.uin));
                    else
                        jsonDeclarant =
                            JsonSerializer.Serialize(new IinDeclarant(biin, CamelliaClient.UserInformation.uin));
                    break;
                default:
                    throw new Exception("Unknown BiinType");
            }

            request.Content = new StringContent(jsonDeclarant, Encoding.UTF8, "application/json");
            var response = CamelliaClient.HttpClient.SendAsync(request).GetAwaiter().GetResult();

            return response.Content.ReadAsStringAsync().Result;
        }
    }

    /// <summary>
    /// Types of biin (BIN and IIN)
    /// </summary>
    public enum BiinType
    {
        BIN,
        IIN
    }
}