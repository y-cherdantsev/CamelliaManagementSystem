namespace Camellia_Management_System.Requests
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


    public sealed class ParticipationReference : SingleInputCaptchaRequest
    {
        public ParticipationReference(CamelliaClient camelliaClient) : base(camelliaClient)
        {
            
        }

        protected override string RequestLink()
        {
            return "https://egov.kz/services/P30.03/";
        }
    }
}