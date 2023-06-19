using System;

// ReSharper disable CommentTypo
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

#pragma warning disable 1591
namespace CamelliaManagementSystem.JsonObjects.ResponseObjects
{
    /// @author Yevgeniy Cherdantsev
    /// @date 18.02.2020 12:19:00
    /// <summary>
    /// StatusGo json object
    /// </summary>
    public class StatusGo : IDisposable
    {
        public string code { get; set; }
        public Name name { get; set; }


        /// @author Yevgeniy Cherdantsev
        /// @date 18.02.2020 12:19:21
        /// <summary>
        /// Name json object
        /// </summary>
        public class Name : IDisposable
        {
            public string ru { get; set; }
            public string kk { get; set; }
            public string en { get; set; }

            /// <inheritdoc />
            public void Dispose()
            {
                ru = null;
                kk = null;
                en = null;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            code = null;
            name.Dispose();
        }
    }
}