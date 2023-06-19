using System;

// ReSharper disable CommentTypo
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

#pragma warning disable 1591
namespace CamelliaManagementSystem.JsonObjects.ResponseObjects
{
    /// @author Yevgeniy Cherdantsev
    /// @date 30.06.2020 10:40:45
    /// <summary>
    /// CompanyNameStatus json object
    /// </summary>
    public class CompanyNameStatus : IDisposable
    {
        public string code { get; set; }
        public string infoRu { get; set; }
        public string infoKz { get; set; }

        /// <inheritdoc />
        public void Dispose()
        {
            code = null;
            infoRu = null;
            infoKz = null;
        }
    }
}