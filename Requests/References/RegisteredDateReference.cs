//TODO(REFACTOR)
namespace Camellia_Management_System.Requests.References
{
    public class RegisteredDateReference : BiinDateCaptchaRequest
    {
        public RegisteredDateReference(CamelliaClient camelliaClient) : base(camelliaClient)
        {
        }

        protected override string RequestLink()
        {
            return "https://egov.kz/services/P30.06/";
        }

        protected override BiinType TypeOfBiin()
        {
            return BiinType.BIN;
        }
    }
}