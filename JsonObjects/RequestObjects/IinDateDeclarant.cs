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
    /// IinDateDeclarant json object 
    /// </summary>
    public class IinDateDeclarant : BinDeclarant, IDisposable
    {
        public string innerdate { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="iin">IIN of the request target</param>
        /// <param name="date">DateTime of the request target</param>
        /// <param name="declarantUin">BIIN of the sender</param>
        public IinDateDeclarant(string iin, string declarantUin, string date) : base(iin, declarantUin)
        {
            innerdate = date;
        }

        /// <inheritdoc />
        public new void Dispose()
        {
            innerdate = null;
            base.Dispose();
        }
    }
}