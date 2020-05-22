using System.Collections.Generic;
using System.Linq;
using System.Net;
using Camellia_Management_System.FileManage;

namespace Camellia_Management_System.Requests.References
{
    public class RegistrationActivitiesReference : SingleInputRequest
    {
        public RegistrationActivitiesReference(CamelliaClient camelliaClient) : base(camelliaClient)
        {
        }
        
        public IEnumerable<string> GetActivitiesDates(string bin, int delay = 1000, bool deleteFile = true, int timeout = 20000,IEnumerator <IWebProxy> proxy = null)
        {
            var reference = GetReference(bin, delay, timeout);

            var temp = reference.First(x => x.language.Contains("ru"));
            if (temp != null)
                return new PdfParser(AdditionalRequests.SaveFile(temp,"./", proxy:proxy ), deleteFile).GetActivitiesDates();
            return null;
        }

        protected override string RequestLink()
        {
            return "https://egov.kz/services/P30.05/";
        }

        protected override BiinType TypeOfBiin()
        {
            return BiinType.BIN;
        }
    }
}