using System;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CamelliaManagementSystem.JsonObjects;
using CamelliaManagementSystem.JsonObjects.RequestObjects;
using CamelliaManagementSystem.JsonObjects.ResponseObjects;

// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable InconsistentNaming
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
        /// <exception cref="InvalidDataException">If some error with camellia occured</exception>
        private async Task<ReadinessStatus> GetReadinessStatusAsync(string requestNumber)
        {
            try
            {
                var response = await CamelliaClient.HttpClient
                    .GetStringAsync($"{RequestLink()}/rest/request-states/{requestNumber}");
                var readinessStatus = JsonSerializer.Deserialize<ReadinessStatus>(response);
                return readinessStatus;
            }
            catch (Exception)
            {
                throw new CamelliaRequestException("It seems that camellia rejected request");
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
        protected async Task<ReadinessStatus> WaitResultAsync(string requestNumber, int delay = 1000,
            int timeout = 60000)
        {
            // Minimum delay is 1s
            delay = delay < 1000 ? 1000 : delay;

            // Counts number of requests that will be proceeded
            var leftRequests = timeout / delay;

            ReadinessStatus readinessStatus;

            do
            {
                if (leftRequests-- <= 0)
                    throw new CamelliaRequestException($"Timeout '{timeout}' exceeded");
                Thread.Sleep(delay);
                readinessStatus = await GetReadinessStatusAsync(requestNumber);
            } while (readinessStatus.status.Equals("IN_PROCESSING"));

            return readinessStatus;
        }

        /// <summary>
        /// Returns true if request has been denied
        /// </summary>
        /// <param name="requestNumber">Number of request</param>
        /// <returns></returns>
        [Obsolete("IsDenied is deprecated, there is no any scenarios where it could be used")]
        // ReSharper disable once UnusedMember.Global
        protected async Task<bool> IsDeniedAsync(string requestNumber)
        {
            var response = await CamelliaClient.HttpClient
                .GetStringAsync(
                    $"https://egov.kz/services/P30.03/rest/like/{requestNumber}/{CamelliaClient.User.user_iin}");
            return response.Contains("DENIED");
        }

        /// <summary>
        /// Sends request to get reference
        /// </summary>
        /// <param name="signedToken"></param>
        /// <param name="solvedCaptcha"></param>
        /// <returns>RequestNumber</returns>
        protected async Task<RequestNumber> SendPdfRequestAsync(string signedToken, string solvedCaptcha = null)
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
            var response = await CamelliaClient.HttpClient
                .SendAsync(request);

            var responseContent = await response.Content.ReadAsStringAsync();
            var requestNumber = JsonSerializer.Deserialize<RequestNumber>(responseContent);

            return requestNumber;
        }

        /// <summary>
        /// Gets token for sending reference request
        /// </summary>
        /// <param name="biin">Biin of reference that should be taken</param>
        /// <param name="stringDate">Date in string format if needed</param>
        /// <returns>XmlToken</returns>
        protected async Task<string> GetTokenAsync(string biin, string stringDate = null)
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

            string jsonDeclarant;

            // Generates declarant based on known values
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (TypeOfBiin())
            {
                case BiinType.BIN:
                    if (stringDate != null)
                        jsonDeclarant = JsonSerializer.Serialize(new BinDateDeclarant(biin, stringDate,
                            CamelliaClient.User.user_iin));
                    else
                        jsonDeclarant =
                            JsonSerializer.Serialize(new BinDeclarant(biin, CamelliaClient.User.user_iin));
                    break;
                case BiinType.IIN:
                    if (stringDate != null)
                        jsonDeclarant = JsonSerializer.Serialize(new IinDateDeclarant(biin, stringDate,
                            CamelliaClient.User.user_iin));
                    else
                        jsonDeclarant =
                            JsonSerializer.Serialize(new IinDeclarant(biin, CamelliaClient.User.user_iin));
                    break;
                default:
                    throw new CamelliaRequestException("Unknown BiinType");
            }

            request.Content = new StringContent(jsonDeclarant, Encoding.UTF8, "application/json");

            var response = await CamelliaClient.HttpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            return responseContent;
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