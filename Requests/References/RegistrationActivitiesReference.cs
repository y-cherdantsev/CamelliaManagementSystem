using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Camellia_Management_System.FileManage;
//TODO(REFACTOR)
namespace Camellia_Management_System.Requests.References
{
    public class RegistrationActivitiesReference : SingleInputRequest
    {
        public RegistrationActivitiesReference(CamelliaClient camelliaClient) : base(camelliaClient)
        {
        }
        
        public IEnumerable<ActivitiesDatePdfParse.DateActivity> GetActivitiesDates(string bin, int delay = 1000, bool deleteFile = true, int timeout = 20000)
        {
            var reference = GetReference(bin, delay, timeout);

            var temp = reference.Result.First(x => x.language.Contains("ru"));
            if (temp != null)
                return new PdfParser(temp.SaveFile("./", CamelliaClient.HttpClient), deleteFile).GetActivitiesDates();
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