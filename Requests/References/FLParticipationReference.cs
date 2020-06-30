using System.Collections.Generic;
using System.Linq;
using Camellia_Management_System.FileManage;
//TODO(REFACTOR)
namespace Camellia_Management_System.Requests.References
{
    public sealed class FLParticipationReference : SingleInputCaptchaRequest
    {
        public FLParticipationReference(CamelliaClient camelliaClient) : base(camelliaClient)
        {
        }

        protected override string RequestLink()
        {
            return "https://egov.kz/services/P30.04/";
        }

        protected override BiinType TypeOfBiin()
        {
            return BiinType.IIN;
        }
        
        public IEnumerable<string> GetWherePersonIsHead(string iin, string captchaApiKey, int delay = 1000,
            bool deleteFile = true, int timeout = 60000)
        {
            var reference = GetReference(iin, captchaApiKey, delay, timeout);
            var temp = reference.Result.First(x => x.language.Contains("ru"));
            if (temp != null)
                return new PdfParser(temp.SaveFile("./", CamelliaClient.HttpClient), deleteFile).GetWherePersonIsHead();
            return null;
        }
    }
}