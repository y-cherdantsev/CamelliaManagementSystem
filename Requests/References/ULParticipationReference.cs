using System.Collections.Generic;
using System.Linq;
using System.Net;
using CamelliaManagementSystem;
using CamelliaManagementSystem.FileManage;
using CamelliaManagementSystem.Requests;

//TODO(REFACTOR)
namespace Camellia_Management_System.Requests.References
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
    public sealed class ULParticipationReference : BiinCaptchaRequest
    {
        public ULParticipationReference(CamelliaClient camelliaClient) : base(camelliaClient)
        {
        }

        public IEnumerable<string> GetChildCompanies(string bin, string captchaApiKey, int delay = 1000,
            bool deleteFile = true, int timeout = 60000)
        {
            var reference = GetReference(bin, captchaApiKey, delay, timeout);
            var temp = reference.First(x => x.language.Contains("ru"));
            if (temp != null)
                return new PdfParser(temp.SaveFile( "./",CamelliaClient.HttpClient), deleteFile).GetChildCompanies();
            return null;
        }

        protected override string RequestLink()
        {
            return "https://egov.kz/services/P30.03/";
        }

        protected override BiinType TypeOfBiin()
        {
            return BiinType.BIN;
        }
    }
}