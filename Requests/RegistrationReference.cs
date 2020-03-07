namespace Camellia_Management_System.Requests
{

    /// @author Yevgeniy Cherdantsev
    /// @date 07.03.2020 16:49:47
    /// @version 1.0
    /// <summary>
    /// INPUT
    /// </summary>


    public class RegistrationReference : SingleInputRequest
    {
        public RegistrationReference(CamelliaClient camelliaClient) : base(camelliaClient)
        {
            RequestLink = "https://egov.kz/services/P30.11/";
        }
    }
}