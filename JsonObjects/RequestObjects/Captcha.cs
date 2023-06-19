using System;

// ReSharper disable CommentTypo
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

#pragma warning disable 1591
namespace CamelliaManagementSystem.JsonObjects.RequestObjects
{
    /// @author Yevgeniy Cherdantsev
    /// @date 14.05.2020 16:21:40
    /// <summary>
    /// Captcha object that should be send to camellia system
    /// </summary>
    public class Captcha : IDisposable
    {
        public string captchaCode { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="captchaSolution">Solved captcha</param>
        public Captcha(string captchaSolution)
        {
            captchaCode = captchaSolution;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            captchaCode = null;
        }
    }
}