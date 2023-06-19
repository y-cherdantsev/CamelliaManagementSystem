// ReSharper disable CommentTypo

namespace CamelliaManagementSystem.Requests.References
{
    /// @author Yevgeniy Cherdantsev
    /// @date 07.03.2020 16:49:47
    /// <summary>
    /// Registration activities reference with activities dates
    /// </summary>
    public class RegistrationActivitiesReferenceAlternative : RegistrationActivitiesReference
    {
        /// <inheritdoc />
        public RegistrationActivitiesReferenceAlternative(CamelliaClient camelliaClient) : base(camelliaClient)
        {
        }
        
        /// <inheritdoc />
        protected override string RequestLink() => "https://egov.kz/services/P30.25/";
    }
}