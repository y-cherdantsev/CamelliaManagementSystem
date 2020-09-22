using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using CamelliaManagementSystem.JsonObjects.ResponseObjects;
using RestSharp;

// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo

namespace CamelliaManagementSystem.SignManage
{
    /// @author Yevgeniy Cherdantsev
    /// @date 18.02.2020 10:35:27
    /// <summary>
    /// Class that signs xml tokens
    /// </summary>
    public static class SignXmlTokens
    {
        // [DllImport("KalkanCryptCOM.dll")] 
        // private static extern KalkanCryptCOM KalkanCryptCOMLib();

        /// @author Yevgeniy Cherdantsev
        /// @date 18.02.2020 10:35:42
        /// <summary>
        /// Signing of the xml token
        /// </summary>
        /// <param name="inData">XML text</param>
        /// <param name="sign">Sign</param>
        /// <returns>string - signed token</returns>
        public static string SignToken(string inData, Sign sign)
        {
            /*
             * Error status for electronic digital signature
             * 0 - ok
             * any numers greater then 0 is error
             */

            // Here calling COM and initialazing it
            var kalkanCom = new KalkanCryptCOMLib.KalkanCryptCOM();
            kalkanCom.Init();

            var alias = string.Empty;

            // Loading hey with handling exception
            kalkanCom.LoadKeyStore((int) KalkanCryptCOMLib.KALKANCRYPTCOM_STORETYPE.KCST_PKCS12, sign.password,
                sign.filePath,
                alias);
            var edsError = kalkanCom.GetLastError();

            if (edsError > 0)
                throw new KalkanCryptException($"Some error occured while loading the key storage '{sign.filePath}'");

            var signNodeId = string.Empty;
            var parentSignNode = string.Empty;
            var parentNameSpace = string.Empty;

            // Signing XML
            kalkanCom.SignXML(alias, 0, signNodeId, parentSignNode, parentNameSpace,
                inData,
                out var outSign);
            kalkanCom.GetLastErrorString(out var error, out edsError);

            if (edsError > 0)
                throw new KalkanCryptException(
                    $"Some error occured while signing the token:${error}\n\nPossible reasons:\n1.Shortage of bin length (Should be 12);\n");

            var result = outSign.Replace("\n", "\r\n");
            return result;
        }


        /// @author Yevgeniy Cherdantsev
        /// @date 10.09.2020 14:39:17
        /// <summary>
        /// Signing of the xml token via API
        /// </summary>
        /// <param name="inData">XML text</param>
        /// <param name="sign">Sign</param>
        /// <returns>string - signed token</returns>
        public static string SignToken(string inData, Sign sign, string apiAddress)
        {
            var restClient = new RestClient($"http://{apiAddress}");
            var request = new RestSharp.RestRequest($"signXML", Method.POST);
            request.AlwaysMultipartFormData = true;
            request.AddHeader("Content-Type", "multipart/form-data");

            request.AddQueryParameter("password", sign.password);
            request.AddQueryParameter("xmlToken", inData);

            request.AddFile("signatureFile", sign.filePath);
            var restResponse = restClient.Execute(request);
            var signedXML = JsonSerializer.Deserialize<ESignatureApiResponse>(restResponse.Content).signedXML.Replace("\n", "\r\n");;
            return signedXML;
        }

        /// <summary>
        /// Custom KalkanCryptCOMLib exception
        /// </summary>
        [Serializable]
        public class KalkanCryptException : Exception
        {
            /// <inheritdoc />
            public KalkanCryptException()
            {
            }

            /// <inheritdoc />
            public KalkanCryptException(string message)
                : base(message)
            {
            }

            /// <inheritdoc />
            public KalkanCryptException(string message, Exception innerException)
                : base(message, innerException)
            {
            }
        }
    }
}