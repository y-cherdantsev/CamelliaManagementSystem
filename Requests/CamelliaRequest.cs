using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using CamelliaManagementSystem.JsonObjects;
using CamelliaManagementSystem.JsonObjects.RequestObjects;
using CamelliaManagementSystem.JsonObjects.ResponseObjects;

// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
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

        //Codes send from system that means that bin is not registered
        protected readonly List<string> binRegistrationStatuses = new List<string>(new[] {"031", "033", "034", "035", "041"});
        
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
        /// todo(Some references can be denied without informating about it)
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
        // [Obsolete("IsDenied is deprecated, there is no any scenarios where it could be used")]
        // ReSharper disable once UnusedMember.Global
        private async Task<bool> IsDeniedAsync(string requestNumber)
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
        /// Returns true if the given bin registered in camellia system
        /// Client should be logged in to work properly
        /// </summary>
        /// <param name="bin">bin of the company</param>
        /// <param name="numberOfTries">Number of requests if some errors has been occured</param>
        /// <param name="delay">Time in millis between requests</param>
        /// <returns>bool - true if company registered</returns>
        // ReSharper disable once CognitiveComplexity
        protected async Task<bool> IsBinRegisteredAsync(string bin,
            int numberOfTries = 15, int delay = 500)
        {
            //Padding BIN to 12 symbols
            bin = bin.PadLeft(12, '0');

            for (var i = 0; i < numberOfTries; i++)
            {
                var response = await CamelliaClient.HttpClient
                    .GetAsync($"https://egov.kz/services/P30.05/rest/gbdul/organizations/{bin}");

                // If got 302 'Moved Temporarily' StatusCode then check that user is logged in. If user is logged in then repeat request;
                if (response.StatusCode == HttpStatusCode.Redirect)
                {
                    if (!await CamelliaClient.IsLoggedAsync())
                        throw new CamelliaClientException(
                            $"'{CamelliaClient.Sign.biin}' isn't authorized to the camellia system");

                    Thread.Sleep(delay);
                    continue;
                }

                var result = string.Empty;

                if (response.Content != null)
                    result = await response.Content.ReadAsStringAsync();
                else if (response.ReasonPhrase != null)
                    throw new CamelliaRequestException(
                        $"StatusCode:'{response.StatusCode}';\nReasonPhrase:'{response.ReasonPhrase}';\nContent is null;");


                if (result.Contains("Number of connections exceeded"))
                    throw new CamelliaRequestException(
                        $"StatusCode:'{response.StatusCode}';\nReasonPhrase:'{response.ReasonPhrase}';\nNumber of connections exceeded. Please, try later;");

                try
                {
                    var organization = JsonSerializer.Deserialize<Organization>(result);
                    return !binRegistrationStatuses.Contains(organization.status.code);
                }
                catch (JsonException e)
                {
                    throw new JsonException(
                        $"Json error while deserializing next string '{result}' of the '{bin}' company to organization object",
                        e);
                }
            }

            throw new CamelliaRequestException($"{numberOfTries} tries with delay={delay} exceeded");
        }

        /// <summary>
        /// Returns boolean if the iin is registered in camellia system
        /// </summary>
        /// <param name="iin">iin of the person</param>
        /// <param name="numberOfTries">Number of requests if some errors has been occured</param>
        /// <param name="delay">Time in millis between requests</param>
        /// <returns>bool - true if person registered</returns>
        protected async Task<bool> IsIinRegisteredAsync(string iin,
            int numberOfTries = 15, int delay = 500)
        {
            //Padding IIN to 12 symbols
            iin = iin.PadLeft(12, '0');

            /*
            *
            * Returns UserInformation.Info.Person in json
            * Can be deserialized var person = JsonSerializer.Deserialize<UserInformation.Info.Person>(response.Content.ReadAsStringAsync());
            * 
            */
            for (var i = 0; i < numberOfTries; i++)
            {
                var response = await CamelliaClient.HttpClient.GetAsync(
                    $"https://egov.kz/services/P30.04/rest/gbdfl/persons/{iin}?infotype=short");

                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                switch (response.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        return false;

                    // If got 302 'Moved Temporarily' StatusCode then check that user is logged in. If user is logged in then repeat request;
                    case HttpStatusCode.Redirect when !await CamelliaClient.IsLoggedAsync():
                        throw new CamelliaClientException(
                            $"'{CamelliaClient.Sign.biin}' isn't authorized to the camellia system");
                    case HttpStatusCode.Redirect:
                        Thread.Sleep(delay);
                        continue;

                    default:
                        return true;
                }
            }

            throw new CamelliaRequestException($"{numberOfTries} tries with delay={delay} exceeded");
        }
        
        /// <summary>
        /// Gets token for sending reference request
        /// </summary>
        /// <param name="biin">Biin of reference that should be taken</param>
        /// <returns>XmlToken</returns>
        protected async Task<string> GetTokenAsync(string biin)
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

            // Generates declarant based on known values
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            var jsonDeclarant = TypeOfBiin() switch
            {
                BiinType.BIN => JsonSerializer.Serialize(new BinDeclarant(biin, CamelliaClient.User.user_iin)),
                BiinType.IIN => JsonSerializer.Serialize(new IinDeclarant(biin, CamelliaClient.User.user_iin)),
                _ => throw new CamelliaRequestException("Unknown BiinType")
            };

            request.Content = new StringContent(jsonDeclarant, Encoding.UTF8, "application/json");

            var response = await CamelliaClient.HttpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            return responseContent;
        }


        /// <summary>
        /// Gets token for signing
        /// </summary>
        /// <param name="biin">User biin</param>
        /// <param name="date">Date for request</param>
        /// <returns>Token</returns>
        protected async Task<string> GetTokenAsync(string biin, DateTime date)
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

            var json = TypeOfBiin() switch
            {
                BiinType.BIN => JsonSerializer.Serialize(new BinDateDeclarant(biin, CamelliaClient.User.user_iin,
                    stringDate)),
                BiinType.IIN => JsonSerializer.Serialize(new IinDateDeclarant(biin, CamelliaClient.User.user_iin,
                    stringDate)),
                _ => throw new CamelliaRequestException("Unknown BiinType")
            };

            request.Content =
                new StringContent(json, Encoding.UTF8, "application/json");
            var response = await CamelliaClient.HttpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }
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