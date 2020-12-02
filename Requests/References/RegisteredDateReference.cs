// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace CamelliaManagementSystem.Requests.References
{
    /// @author Yevgeniy Cherdantsev
    /// @date 07.03.2020 16:49:47
    /// <summary>
    /// Registered date reference gives registration reference for the given date
    /// </summary>
    public class RegisteredDateReference : BiinDateCaptchaRequest
    {
        /// <inheritdoc />
        public RegisteredDateReference(CamelliaClient camelliaClient) : base(camelliaClient)
        {
        }

        /// <inheritdoc />
        protected override string RequestLink() => "https://egov.kz/services/P30.06/";

        /// <inheritdoc />
        protected override BiinType TypeOfBiin() => BiinType.BIN;
    }
}