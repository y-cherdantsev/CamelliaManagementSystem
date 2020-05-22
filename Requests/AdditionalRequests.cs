using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using Camellia_Management_System.JsonObjects.ResponseObjects;
using Camellia_Management_System.SignManage;

namespace Camellia_Management_System.Requests
{
    /// @author Yevgeniy Cherdantsev
    /// @date 11.03.2020 15:08:36
    /// @version 1.0
    /// <summary>
    /// Additional requests
    /// </summary>
    public static class AdditionalRequests
    {
        /// @author Yevgeniy Cherdantsev
        /// @date 11.03.2020 16:19:56
        /// @version 1.0
        /// <summary>
        /// Returns boolean if the bin is registered in camellia system
        /// </summary>
        /// <param name="camelliaClient">Camellia client</param>
        /// <param name="bin">bin of the company</param>
        /// <returns>bool - true if company registered</returns>
        public static bool IsBinRegistered(CamelliaClient camelliaClient, string bin)
        {
            bin = bin.PadLeft(12, '0');
            var res = camelliaClient.HttpClient
                .GetStringAsync($"https://egov.kz/services/P30.11/rest/gbdul/organizations/{bin}")
                .GetAwaiter()
                .GetResult();
            var organization = JsonSerializer.Deserialize<Organization>(res);
            return organization.status.code != "031" && organization.status.code != "034" &&
                   organization.status.code != "035";
        }

        /// @author Yevgeniy Cherdantsev
        /// @date 14.05.2020 14:59:56
        /// @version 1.0
        /// <summary>
        /// Returns boolean if the iin is registered in camellia system
        /// </summary>
        /// <param name="camelliaClient">Camellia client</param>
        /// <param name="iin">iin of the person</param>
        /// <returns>bool - true if person registered</returns>
        public static bool IsIinRegistered(CamelliaClient camelliaClient, string iin)
        {
            iin = iin.PadLeft(12, '0');
            try
            {
                var res = camelliaClient.HttpClient
                    .GetStringAsync($"https://egov.kz/services/P30.04/rest/gbdfl/persons/{iin}?infotype=short")
                    .GetAwaiter()
                    .GetResult();
                var person = JsonSerializer.Deserialize<Person>(res);
                return true;
            }
            catch (HttpRequestException e)
            {
                if (e.Message.Contains("Response status code does not indicate success: 404 (Not Found)"))
                {
                    return false;
                }

                throw;
            }
        }


        /// <summary>
        /// Gets information about organization
        /// </summary>
        /// <param name="camelliaClient">Camellia CLient</param>
        /// <param name="bin">Bin of the company</param>
        /// <returns>Organization object</returns>
        /// <exception cref="InvalidDataException">If company doesn't presented in the system</exception>
        public static Organization GetOrganizationInfo(CamelliaClient camelliaClient, string bin)
        {
            bin = bin.PadLeft(12, '0');
            var res = camelliaClient.HttpClient
                .GetStringAsync($"https://egov.kz/services/P30.11/rest/gbdul/organizations/{bin}")
                .GetAwaiter()
                .GetResult();
            var organization = JsonSerializer.Deserialize<Organization>(res);
            if (organization.status.code == "031" || organization.status.code == "034")
                throw new InvalidDataException("This company isn't presented in camellia system");
            return organization;
        }

        public static bool IsSignCorrect(Sign sign, string bin, IWebProxy webProxy = null)
        {
            //TODO (enum)
            try
            {
                if (!new FileInfo(sign.FilePath).Exists)
                    throw new FileNotFoundException();
                bin = bin.PadLeft(12, '0');
                var camelliaClient = new CamelliaClient(new FullSign {AuthSign = sign}, webProxy);
                return camelliaClient.UserInformation.uin.PadLeft(12, '0') == bin;
            }
            catch (FileNotFoundException e)
            {
                throw;
            }
            catch (Exception e)
            {
                if (e.Message == "Some error occured while loading the key storage")
                    throw new InvalidDataException("Incorrect password");
                throw new InvalidDataException("Service unavailable");
            }
        }

        
    }
}