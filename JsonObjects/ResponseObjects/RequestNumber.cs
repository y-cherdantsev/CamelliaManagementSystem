using System;

// ReSharper disable CommentTypo
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

#pragma warning disable 1591
namespace CamelliaManagementSystem.JsonObjects.ResponseObjects
{
    /// @author Yevgeniy Cherdantsev
    /// @date 11.08.2020 17:11:34
    /// <summary>
    /// RequestNumber json object
    /// </summary>
    public class RequestNumber : IDisposable
    {
        public string requestNumber { get; set; }

        /// <inheritdoc />
        public void Dispose()
        {
            requestNumber = null;
        }
    }
}