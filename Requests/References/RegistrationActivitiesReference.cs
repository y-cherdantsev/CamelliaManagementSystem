namespace Camellia_Management_System.Requests.References
{
    public class RegistrationActivitiesReference : SingleInputRequest
    {
        public RegistrationActivitiesReference(CamelliaClient camelliaClient) : base(camelliaClient)
        {
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