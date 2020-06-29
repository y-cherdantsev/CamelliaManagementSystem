using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Camellia_Management_System.JsonObjects;
using Camellia_Management_System.JsonObjects.ResponseObjects;
using Camellia_Management_System.SignManage;
//TODO(REFACTOR)
namespace Camellia_Management_System.Requests
{
    /// @author Yevgeniy Cherdantsev
    /// @date 07.03.2020 16:35:25
    /// @version 1.0
    /// <summary>
    /// 
    /// </summary>
    public abstract class SingleInputRequest : CamelliaRequest
    {
        protected SingleInputRequest(CamelliaClient camelliaClient) : base(camelliaClient)
        {
        }


        public IEnumerable<ResultForDownload> GetReference(string input, int delay = 1000, int timeout = 60000)
        {
            input = input.PadLeft(12, '0');
            if (TypeOfBiin() == BiinType.BIN)
            {
                if (input.Length == 12 && !AdditionalRequests.IsBinRegistered(CamelliaClient, input))
                    throw new InvalidDataException("This bin is not registered");
            }
            else
            {
                if (input.Length == 12 && !AdditionalRequests.IsIinRegistered(CamelliaClient, input))
                    throw new InvalidDataException("This iin is not registered");
            }

            var token = GetToken(input);
            try
            {
                token = JsonSerializer.Deserialize<TokenResponse>(token).xml;
            }
            catch (Exception e)
            {
                if (token.Contains("<h1>405 Not Allowed</h1>"))
                    throw new InvalidDataException("Not allowed or some problem with egov occured");
                throw;
            }
           
            var signedToken = SignXmlTokens.SignToken(token, CamelliaClient.FullSign.RsaSign);
            var requestNumber = SendPdfRequest(signedToken);
            var readinessStatus = WaitResult(requestNumber, delay, timeout);
            if (readinessStatus.status.Equals("APPROVED"))
                return readinessStatus.resultsForDownload;

            throw new InvalidDataException($"Readiness status equals {readinessStatus.status}");
        }
    }
}