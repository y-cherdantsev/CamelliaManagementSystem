using System.Collections.Generic;
using System.Linq;
using Camellia_Management_System.FileManage;

namespace Camellia_Management_System.Requests
{
    /// @author Yevgeniy Cherdantsev
    /// @date 14.03.2020 11:03:28
    /// @version 1.0
    /// <summary>
    /// INPUT
    /// </summary>
    /// <code>
    /// 
    /// </code>
    public sealed class ParticipationReference : SingleInputCaptchaRequest
    {
        public ParticipationReference(CamelliaClient camelliaClient) : base(camelliaClient)
        {
        }

        public IEnumerable<string> GetChildCompanies(string bin, int delay = 1000,
            bool deleteFile = true, int timeout = 60000)
        {
            var reference = GetReference(bin, delay, timeout);
            var temp = reference.First(x => x.language.Contains("ru"));
            if (temp != null)
                return new PdfParser(temp.SaveFile("./"), deleteFile).GetChildCompanies();
            return null;
        }

        protected override string RequestLink()
        {
            return "https://egov.kz/services/P30.03/";
        }
    }
}