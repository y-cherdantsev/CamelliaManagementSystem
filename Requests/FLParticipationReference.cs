namespace Camellia_Management_System.Requests
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
    }
}