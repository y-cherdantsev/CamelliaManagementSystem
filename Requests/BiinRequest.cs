using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CamelliaManagementSystem.JsonObjects.ResponseObjects;
using CamelliaManagementSystem.SignManage;

// ReSharper disable CognitiveComplexity
// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo

namespace CamelliaManagementSystem.Requests
{
    /// @author Yevgeniy Cherdantsev
    /// @date 07.03.2020 16:35:25
    /// <summary>
    /// Request with biin and captcha
    /// </summary>
    public abstract class BiinRequest : CamelliaRequest
    {
        /// <inheritdoc />
        protected BiinRequest(CamelliaClient camelliaClient) : base(camelliaClient)
        {
        }

        /// <summary>
        /// Gets ResultForDownload references
        /// </summary>
        /// <param name="biin">BIN or IIN</param>
        /// <param name="delay">Delay between requests while waiting reference</param>
        /// <param name="timeout">Timeout while waiting reference</param>
        /// <returns>List of results for downloadings (References links)</returns>
        /// <exception cref="InvalidDataException"></exception>
        // ReSharper disable once MemberCanBeProtected.Global
        public IEnumerable<ResultForDownload> GetReference(string biin, int delay = 1000, int timeout = 60000)
        {
            // Transforms to the suitable form
            biin = biin.PadLeft(12, '0');

            // Checks registration of biin
            switch (TypeOfBiin())
            {
                case BiinType.BIN:
                    if (biin.Length == 12 && !AdditionalRequests.IsBinRegistered(CamelliaClient, biin))
                        throw new InvalidDataException("This bin is not registered");
                    break;
                case BiinType.IIN:
                    if (biin.Length == 12 && !AdditionalRequests.IsIinRegistered(CamelliaClient, biin))
                        throw new InvalidDataException("This iin is not registered");
                    break;
                default: throw new Exception("Unknown BiinType");
            }

            // Getting token
            var token = GetToken(biin);
            try
            {
                token = JsonSerializer.Deserialize<Token>(token).xml;
            }
            catch (Exception)
            {
                if (token.Contains("<h1>405 Not Allowed</h1>"))
                    throw new InvalidDataException("Not allowed or some problem with egov occured");
                throw;
            }

            var signedToken = SignXmlTokens.SignToken(token, CamelliaClient.FullSign.rsaSign);
            // var signedToken1 = SignXmlTokens.SignToken(token, CamelliaClient.FullSign.rsaSign, "localhost:6000");

            // Sending request and getting reference
            var requestNumber = SendPdfRequest(signedToken);
            var readinessStatus = WaitResult(requestNumber.requestNumber, delay, timeout);
            if (readinessStatus.status.Equals("APPROVED"))
                return readinessStatus.resultsForDownload;

            throw new InvalidDataException($"Readiness status equals {readinessStatus.status}");
        }
    }
}