using System;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo

namespace CamelliaManagementSystem.JsonObjects.RequestObjects
{
    /// @author Yevgeniy Cherdantsev
    /// @date 14.05.2020 16:21:40
    /// <summary>
    /// Json object that should be send in order to get reference
    /// </summary>
    public class Declarant : IDisposable
    {
        /// <summary>
        /// Should be provided to get reference
        /// </summary>
        public string declarantUin { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="declarantUin">BIIN of the sender</param>
        protected Declarant(string declarantUin)
        {
            this.declarantUin = declarantUin;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            declarantUin = null;
        }
    }
}