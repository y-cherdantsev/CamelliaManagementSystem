//TODO(REFACTOR)
namespace Camellia_Management_System.Requests.References
{
    public class LastChangesReference : SingleInputRequest
    {
        public LastChangesReference(CamelliaClient camelliaClient) : base(camelliaClient)
        {
        }

        protected override string RequestLink()
        {
            return "https://egov.kz/services/P30.07/";
        }

        protected override BiinType TypeOfBiin()
        {
            return BiinType.BIN;
        }
    }
}