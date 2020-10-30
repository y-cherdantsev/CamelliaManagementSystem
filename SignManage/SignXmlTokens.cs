using System;
using System.Threading.Tasks;
using NCANode.Models.NCANode.Requests;

// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo

namespace CamelliaManagementSystem.SignManage
{
    /// @author Yevgeniy Cherdantsev
    /// @date 18.02.2020 10:35:27
    /// <summary>
    /// Class that signs xml tokens
    /// </summary>
    public static class SignXmlTokens
    {
        /// @author Yevgeniy Cherdantsev
        /// @date 10.09.2020 14:39:17
        /// <summary>
        /// Signing of the xml token via NCANode API
        /// </summary>
        /// <param name="inData">XML text</param>
        /// <param name="base64Sign">Base64 representation of sign file</param>
        /// <param name="password">Password</param>
        /// <param name="host">API host</param>
        /// <param name="port">API port</param>
        /// <returns>string - signed token</returns>
        public static async Task<string> SignTokenAsync(string inData, string base64Sign, string password,
            string host,
            int port)
        {
            var ncaNode = new NCANode.NCANode(host, port);
            var xmlSignRequest = new XMLSignRequest
            {
                useTsaPotspHashAlgorithm = TspHashAlgorithm.SHA256,
                @params = new XMLSignRequest.XMLSignRequestParams
                {
                    p12 = base64Sign,
                    password = password,
                    xml = inData
                }
            };
            try
            {
                var signedXml = await ncaNode.SignXMLAsync(xmlSignRequest);
                if (signedXml.status != 0) throw new XmlSignException(signedXml.message);

                return signedXml.result.xml;
            }
            catch (Exception e)
            {
                throw new XmlSignException("Error occured while signing XML", e);
            }
        }

        /// <summary>
        /// Custom XMLSign exception
        /// </summary>
        [Serializable]
        public class XmlSignException : Exception
        {
            /// <inheritdoc />
            public XmlSignException()
            {
            }

            /// <inheritdoc />
            public XmlSignException(string message)
                : base(message)
            {
            }

            /// <inheritdoc />
            public XmlSignException(string message, Exception innerException)
                : base(message, innerException)
            {
            }
        }
    }
}