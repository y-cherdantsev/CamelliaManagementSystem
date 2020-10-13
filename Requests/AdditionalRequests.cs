using System;
using System.Net;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CamelliaManagementSystem.JsonObjects.ResponseObjects;

// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable UnusedMember.Global

namespace CamelliaManagementSystem.Requests
{
    /// @author Yevgeniy Cherdantsev
    /// @date 11.03.2020 15:08:36
    /// <summary>
    /// Additional requests
    /// </summary>
    public static class AdditionalRequests
    {
        /// <summary>
        /// Returns true if the given bin registered in camellia system
        /// Client should be logged in to work properly
        /// </summary>
        /// <param name="camelliaClient">Camellia client</param>
        /// <param name="bin">bin of the company</param>
        /// <param name="numberOfTries">Number of requests if some errors has been occured</param>
        /// <param name="delay">Time in millis between requests</param>
        /// <returns>bool - true if company registered</returns>
        // ReSharper disable once CognitiveComplexity
        public static async Task<bool> IsBinRegisteredAsync(CamelliaClient camelliaClient, string bin,
            int numberOfTries = 15, int delay = 500)
        {
            //Padding BIN to 12 symbols
            bin = bin.PadLeft(12, '0');

            //Codes send from system that means that bin is not registered
            string[] knownErrorCodes = {"031", "034", "035"};

            for (var i = 0; i < numberOfTries; i++)
            {
                var response = await camelliaClient.HttpClient
                    .GetAsync($"https://egov.kz/services/P30.11/rest/gbdul/organizations/{bin}");

                // If got 302 'Moved Temporarily' StatusCode then check that user is logged in. If user is logged in then repeat request;
                if (response.StatusCode == HttpStatusCode.Redirect)
                {
                    if (!await camelliaClient.IsLoggedAsync())
                        throw new CamelliaClientException(
                            $"'{camelliaClient.Sign.iin}' isn't authorized to the camellia system");

                    Thread.Sleep(delay);
                    continue;
                }

                var result = "";

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
                    return !knownErrorCodes.Contains(organization.status.code);
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
        /// <param name="camelliaClient">Camellia client</param>
        /// <param name="iin">iin of the person</param>
        /// <param name="numberOfTries">Number of requests if some errors has been occured</param>
        /// <param name="delay">Time in millis between requests</param>
        /// <returns>bool - true if person registered</returns>
        public static async Task<bool> IsIinRegisteredAsync(CamelliaClient camelliaClient, string iin,
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
                var response = await camelliaClient.HttpClient.GetAsync(
                    $"https://egov.kz/services/P30.04/rest/gbdfl/persons/{iin}?infotype=short");

                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                switch (response.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        return false;

                    // If got 302 'Moved Temporarily' StatusCode then check that user is logged in. If user is logged in then repeat request;
                    case HttpStatusCode.Redirect when !await camelliaClient.IsLoggedAsync():
                        throw new CamelliaClientException(
                            $"'{camelliaClient.Sign.iin}' isn't authorized to the camellia system");
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
        /// Checks if there is no organization with presented name
        /// </summary>
        /// <param name="camelliaClient">Camellia CLient</param>
        /// <param name="name">Name of the company</param>
        /// <param name="numberOfTries">Number of requests if some errors has been occured</param>
        /// <param name="delay">Time in millis between requests</param>
        /// <returns>True or false</returns>
        public static async Task<bool> IsCompanyNameFreeAsync(CamelliaClient camelliaClient, string name,
            int numberOfTries = 15, int delay = 500)
        {
            var content = new StringContent($"{{\"nameKz\":\"{name}\"}}", Encoding.UTF8, "application/json");

            string[] knownCorrectCodes = {"005"};

            for (var i = 0; i < numberOfTries; i++)
            {
                var response = await camelliaClient.HttpClient
                    .PostAsync(new Uri("https://egov.kz/services/Com03/rest/app/checkNames"), content);

                // If got 302 'Moved Temporarily' StatusCode then check that user is logged in. If user is logged in then repeat request;
                if (response.StatusCode == HttpStatusCode.Redirect)
                {
                    if (!await camelliaClient.IsLoggedAsync())
                        throw new CamelliaClientException(
                            $"'{camelliaClient.Sign.iin}' isn't authorized to the camellia system");

                    Thread.Sleep(delay);
                    continue;
                }

                var result = "";

                if (response.Content != null)
                    result = await response.Content.ReadAsStringAsync();
                else if (response.ReasonPhrase != null)
                    throw new CamelliaRequestException(
                        $"StatusCode:'{response.StatusCode}';\nReasonPhrase:'{response.ReasonPhrase}';\nContent is null;");

                try
                {
                    var companyNameStatus = JsonSerializer.Deserialize<CompanyNameStatus>(result);
                    return knownCorrectCodes.Contains(companyNameStatus.code);
                }
                catch (JsonException e)
                {
                    throw new JsonException(
                        $"Json error while deserializing next string '{result}' of the '{name}' company to companyNameStatus object",
                        e);
                }
            }

            throw new CamelliaRequestException($"{numberOfTries} tries with delay={delay} exceeded");
        }

        /// <summary>
        /// Get current person social status
        /// </summary>
        /// <param name="camelliaClient">Camellia CLient</param>
        /// <param name="iin">IIN of the person</param>
        /// <param name="numberOfTries">Number of requests if some errors has been occured</param>
        /// <param name="delay">Time in millis between requests</param>
        /// <returns></returns>
        /// <exception cref="CamelliaClientException">If client not logged in</exception>
        /// <exception cref="CamelliaRequestException">If some request error occured</exception>
        /// <exception cref="JsonException">If unexpected raw text occured</exception>
        public static async Task<PersonStatus> GetPersonSocialStatus(CamelliaClient camelliaClient, string iin,
            int numberOfTries = 15, int delay = 500)
        {
            var content = new StringContent($"{{\"iin\":\"{iin}\",\"captchaCode\":null}}", Encoding.UTF8,
                "application/json");

            for (var i = 0; i < numberOfTries; i++)
            {
                var response = await camelliaClient.HttpClient
                    .PostAsync(new Uri("https://egov.kz/services/P6.02/rest/app/status"), content);

                // If got 302 'Moved Temporarily' StatusCode then check that user is logged in. If user is logged in then repeat request;
                if (response.StatusCode == HttpStatusCode.Redirect)
                {
                    if (!await camelliaClient.IsLoggedAsync())
                        throw new CamelliaClientException(
                            $"'{camelliaClient.Sign.iin}' isn't authorized to the camellia system");

                    Thread.Sleep(delay);
                    continue;
                }

                var result = "";

                if (response.Content != null)
                    result = await response.Content.ReadAsStringAsync();
                else if (response.ReasonPhrase != null)
                    throw new CamelliaRequestException(
                        $"StatusCode:'{response.StatusCode}';\nReasonPhrase:'{response.ReasonPhrase}';\nContent is null;");

                try
                {
                    var personStatus = JsonSerializer.Deserialize<PersonStatus>(result);
                    return personStatus;
                }
                catch (JsonException e)
                {
                    throw new JsonException(
                        $"Json error while deserializing next string '{result}' of the '{iin}' person to personStatus object",
                        e);
                }
            }

            throw new CamelliaRequestException($"{numberOfTries} tries with delay={delay} exceeded");
        }
    }
}