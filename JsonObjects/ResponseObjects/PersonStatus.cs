using System;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CommentTypo
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

#pragma warning disable 1591
namespace CamelliaManagementSystem.JsonObjects.ResponseObjects
{
    /// @author Yevgeniy Cherdantsev
    /// @date 22.09.2020 10:58:05
    /// <summary>
    /// PersonStatus json object
    /// </summary>
    public class PersonStatus : IDisposable
    {
        public string iin { get; set; }
        public int statusCode { get; set; }
        public int subStatusCode { get; set; }
        public string statusFactRu { get; set; }
        public string statusFactKz { get; set; }

        /// <inheritdoc />
        public void Dispose()
        {
            iin = null;
            statusFactRu = null;
            statusFactKz = null;
        }
    }
}