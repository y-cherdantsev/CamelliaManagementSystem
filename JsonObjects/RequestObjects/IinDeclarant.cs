using System;

// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

#pragma warning disable 1591
namespace CamelliaManagementSystem.JsonObjects.RequestObjects
{
    /// @author Yevgeniy Cherdantsev
    /// @date 18.02.2020 12:19:42
    /// <summary>
    /// IinDeclarant json object 
    /// </summary>
    public class IinDeclarant : Declarant, IDisposable
    {
        public string iin { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="iin">IIN of the request target</param>
        /// <param name="declarantUin">BIIN of the sender</param>
        public IinDeclarant(string iin, string declarantUin) : base(declarantUin)
        {
            this.iin = iin;
        }

        /// <inheritdoc />
        public new void Dispose()
        {
            iin = null;
            base.Dispose();
        }
    }
}