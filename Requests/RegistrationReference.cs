using System.Collections.Generic;
using Camellia_Management_System.FileManage;

namespace Camellia_Management_System.Requests
{
    /// @author Yevgeniy Cherdantsev
    /// @date 07.03.2020 16:49:47
    /// @version 1.0
    /// <summary>
    /// INPUT
    /// </summary>
    public class RegistrationReference : SingleInputRequest
    {
        public RegistrationReference(CamelliaClient camelliaClient) : base(camelliaClient)
        {
            RequestLink = "https://egov.kz/services/P30.11/";
        }

        public IEnumerable<string> GetFounders(string bin, int delay = 1000)
        {
            var reference = GetReference(bin, delay);
            foreach (var resultForDownload in reference)
            {
                if (!resultForDownload.language.Contains("ru")) continue;
                return new PdfParser(resultForDownload.SaveFile("./")).GetFounders();
            }

            return null;
        }
    }
}