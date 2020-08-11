using System;
using System.IO;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable CommentTypo

namespace CamelliaManagementSystem.SignManage
{
    /// @author Yevgeniy Cherdantsev
    /// @date 18.02.2020 10:31:15
    /// <summary>
    /// Class which contains all information about user signs
    /// </summary>
    public sealed class FullSign : IDisposable
    {
        /// <summary>
        /// AUTH sign container
        /// </summary>
        public Sign authSign { get; set; }

        /// <summary>
        /// RSA sign container
        /// </summary>
        public Sign rsaSign { get; set; }


        /// @author Yevgeniy Cherdantsev
        /// @date 29.06.2020 12:16:27
        /// <summary>
        /// Constructor for creating full sign
        /// </summary>
        /// <param name="authSign">AUTH sign</param>
        /// <param name="rsaSign">RSA sign</param>
        public FullSign(Sign authSign, Sign rsaSign)
        {
            this.authSign = authSign;
            this.rsaSign = rsaSign;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            authSign.Dispose();
            rsaSign.Dispose();
        }
    }

    /// @author Yevgeniy Cherdantsev
    /// @date 18.02.2020 10:17:31
    /// <summary>
    /// Sign object creator
    /// </summary>
    public class Sign : IDisposable
    {
        /// <summary>
        /// Path to the sign file
        /// </summary>
        public string filePath { get; set; }

        /// <summary>
        /// Password for the sign
        /// </summary>
        public string password { get; set; }

        /// <summary>
        /// Name of the folder containing sign
        /// </summary>
        public string folderName => new FileInfo(filePath).Directory?.Name;

        /// @author Yevgeniy Cherdantsev
        /// @date 18.02.2020 10:34:30
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filePath">Path to the sign file</param>
        /// <param name="password">Password for the sign</param>
        public Sign(string filePath, string password)
        {
            this.filePath = filePath;
            this.password = password;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            filePath = null;
            password = null;
        }
    }
}