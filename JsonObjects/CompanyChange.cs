using System;

// ReSharper disable CommentTypo
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Global

#pragma warning disable 1591
namespace CamelliaManagementSystem.JsonObjects
{
    /// @author Yevgeniy Cherdantsev
    /// @date 18.02.2020 12:19:42
    /// <summary>
    /// CompanyChange json object 
    /// </summary>
    public class CompanyChange : IDisposable
    {
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public string Before { get; set; }
        public string After { get; set; }

        /// <inheritdoc />
        public void Dispose()
        {
            Type = null;
            Before = null;
            After = null;
        }
    }
}