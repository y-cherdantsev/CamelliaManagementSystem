using System;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo

#pragma warning disable 1591
namespace CamelliaManagementSystem.JsonObjects.RequestObjects
{
    /// @author Yevgeniy Cherdantsev
    /// @date 18.02.2020 12:19:42
    /// <summary>
    /// BinDeclarant json object 
    /// </summary>
    public class BinDeclarant : Declarant, IDisposable
    {
        public string bin { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bin">BIN of the request target</param>
        /// <param name="declarantUin">BIIN of the sender</param>
        public BinDeclarant(string bin, string declarantUin) : base(declarantUin)
        {
            this.bin = bin;
        }

        /// <inheritdoc />
        public new void Dispose()
        {
            bin = null;
            base.Dispose();
        }
    }
}