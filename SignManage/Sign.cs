using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
// ReSharper disable NotResolvedInText
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace CamelliaManagementSystem.SignManage
{
    /// @author Yevgeniy Cherdantsev
    /// @date 06.10.2020 13:52:25
    /// <summary>
    /// Class which contains all information about sign
    /// </summary>
    public sealed class Sign
    {
        /// <summary>
        /// AUTH sign base64 representation
        /// </summary>
        public string auth { get; set; }

        /// <summary>
        /// RSA sign base64 representation
        /// </summary>
        public string rsa { get; set; }

        /// <summary>
        /// password
        /// </summary>
        public string encryptedPassword { get; set; }

        /// <summary>
        /// Decrypted Password
        /// </summary>
        public string password => DecryptPassword(encryptedPassword, biin.ToString());

        /// <summary>
        /// IIN
        /// </summary>
        public long biin { get; set; }

        /// <summary>
        /// Owner fullname
        /// </summary>
        public string fullname { get; set; }

        /// <summary>
        /// Decrypts password using DES method
        /// </summary>
        /// <param name="encryptedPassword">Plain text</param>
        /// <param name="key">Decryption key</param>
        /// <returns>Decrypted password</returns>
        /// <exception cref="ArgumentNullException">If encrypted string is null</exception>
        public static string DecryptPassword(string encryptedPassword, string key)
        {
            var bytes = Encoding.ASCII.GetBytes(key.PadRight(8, '0').Substring(0, 8));
            if (string.IsNullOrEmpty(encryptedPassword))
                throw new ArgumentNullException
                    ("The string which needs to be decrypted can not be null.");
            var cryptoProvider = new DESCryptoServiceProvider();
            var memoryStream = new MemoryStream
                (Convert.FromBase64String(encryptedPassword));
            var cryptoStream = new CryptoStream(memoryStream,
                cryptoProvider.CreateDecryptor(bytes, bytes), CryptoStreamMode.Read);
            var reader = new StreamReader(cryptoStream);
            return reader.ReadToEnd();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"'{fullname}' : '{biin}'";
        }
    }
}