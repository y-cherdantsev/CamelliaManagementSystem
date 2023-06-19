using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using CamelliaManagementSystem.SignManage;
using CamelliaManagementSystem.JsonObjects.ResponseObjects;

// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable CognitiveComplexity

namespace CamelliaManagementSystem.Requests
{
    /// @author Yevgeniy Cherdantsev
    /// @date 07.03.2020 16:35:25
    /// <summary>
    /// Request with BIIN
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
        /// <exception cref="CamelliaNoneDataException">If some information dowsn't exist in camellia system</exception>
        /// <exception cref="CamelliaRequestException">If some error occured</exception>
        // ReSharper disable once MemberCanBeProtected.Global
        public async Task<IEnumerable<ResultForDownload>> GetReferenceAsync(string biin, int delay = 1000,
            int timeout = 60000)
        {
            // Transforms to the suitable form
            biin = biin.PadLeft(12, '0');

            // Checks registration of biin
            switch (TypeOfBiin())
            {
                case BiinType.BIN:
                    if (biin.Length == 12 && !await IsBinRegisteredAsync(biin))
                        throw new CamelliaNoneDataException("This bin is not registered");
                    break;
                case BiinType.IIN:
                    if (biin.Length == 12 && !await IsIinRegisteredAsync(biin))
                        throw new CamelliaNoneDataException("This iin is not registered");
                    break;
                default: throw new CamelliaRequestException("Unknown BiinType");
            }

            // Getting token
            var token = await GetTokenAsync(biin);
            try
            {
                token = JsonSerializer.Deserialize<Token>(token).xml;
            }
            catch (Exception)
            {
                if (token.Contains("<h1>405 Not Allowed</h1>"))
                    throw new CamelliaRequestException("Not allowed or some problem with egov occured");
                throw;
            }

            // Signing token
            var signedToken = await SignXmlTokens.SignTokenAsync(token, CamelliaClient.Sign.rsa,
                CamelliaClient.Sign.password, CamelliaClient.NcaNodeAddress);

            // Sending request and getting reference
            var requestNumber = await SendPdfRequestAsync(signedToken);
            var readinessStatus = await WaitResultAsync(requestNumber.requestNumber, delay, timeout);
            if (readinessStatus.status.Equals("APPROVED"))
                return readinessStatus.resultsForDownload;

            throw new CamelliaNoneDataException($"Readiness status equals {readinessStatus.status}");
        }
    }
}