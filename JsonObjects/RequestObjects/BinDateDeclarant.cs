using System;

// ReSharper disable CommentTypo
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable IdentifierTypo

#pragma warning disable 1591
namespace CamelliaManagementSystem.JsonObjects.RequestObjects
{
    /// @author Yevgeniy Cherdantsev
    /// @date 18.02.2020 12:19:42
    /// <summary>
    /// BinDateDeclarant json object 
    /// </summary>
    public class BinDateDeclarant : BinDeclarant, IDisposable
    {
        public string innerdate { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bin">BIN of the request target</param>
        /// <param name="date">DateTime of the request target</param>
        /// <param name="declarantUin">BIIN of the sender</param>
        public BinDateDeclarant(string bin, string declarantUin, string date) : base(bin, declarantUin)
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