using System.Net.Http;

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
        public static bool IsBinRegistered(CamelliaClient camelliaClient,string bin)
        {
            var res = camelliaClient.HttpClient.GetStringAsync($"https://egov.kz/services/P30.11/rest/gbdul/organizations/{bin}")
                .GetAwaiter()
                .GetResult();
            return !res.Contains("\"code\":\"031\"");
        }
    }
}