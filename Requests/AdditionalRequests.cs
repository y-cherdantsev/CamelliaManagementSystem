using System.IO;
using System.Text.Json;
using Camellia_Management_System.JsonObjects.ResponseObjects;

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
            bin = bin.PadLeft(12,'0');
            var res = camelliaClient.HttpClient
                .GetStringAsync($"https://egov.kz/services/P30.11/rest/gbdul/organizations/{bin}")
                .GetAwaiter()
                .GetResult();
            var organization = JsonSerializer.Deserialize<Organization>(res);
            return organization.status.code != "031" && organization.status.code != "034";
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
            bin = bin.PadLeft(12,'0');
            var res = camelliaClient.HttpClient
                .GetStringAsync($"https://egov.kz/services/P30.11/rest/gbdul/organizations/{bin}")
                .GetAwaiter()
                .GetResult();
            var organization = JsonSerializer.Deserialize<Organization>(res);
            if (organization.status.code == "031" || organization.status.code == "034")
                throw new InvalidDataException("This company doesn't presented in camellia system");
            return organization;
        }
    }
}