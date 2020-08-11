using System;

// ReSharper disable CommentTypo
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

#pragma warning disable 1591
namespace CamelliaManagementSystem.JsonObjects.ResponseObjects
{
    /// @author Yevgeniy Cherdantsev
    /// @date 18.02.2020 12:17:45
    /// <summary>
    /// Eds json object
    /// </summary>
    public class Eds : IDisposable
    {
        public long createdDate { get; set; }
        public long endedDate { get; set; }
        public string iin { get; set; }
        public string bin { get; set; }
        public string fullname { get; set; }
        public string organization { get; set; }
        public string status { get; set; }

        /// <inheritdoc />
        public void Dispose()
        {
            iin = null;
            bin = null;
            fullname = null;
            organization = null;
            status = null;
        }
    }
}