namespace Camellia_Management_System.Requests
{
    public class CompanyName
    {
        protected readonly CamelliaClient CamelliaClient;

        public CompanyName(CamelliaClient camelliaClient)
        {
            CamelliaClient = camelliaClient;
        }

        public string GetCompanyNameRu(string biin)
        {
            string result = "";
            return result;
        }
        public string GetCompanyNameKz(string biin)
        {
            string result = "";
            return result;
        }
        public (string, string) GetCompanyName(string biin)
        {
            string result = "";
            return (result, result);
        }
    }
}