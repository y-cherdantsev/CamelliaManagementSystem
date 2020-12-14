using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using CamelliaManagementSystem.FileManage.PlainTextParsers;

// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace CamelliaManagementSystem.Requests.References
{
    /// @author Yevgeniy Cherdantsev
    /// @date 14.03.2020 11:03:28
    /// <summary>
    /// UL participation reference with information about child companies
    /// </summary>
    public sealed class UlParticipationReference : BiinCaptchaRequest
    {
        /// <inheritdoc />
        public UlParticipationReference(CamelliaClient camelliaClient) : base(camelliaClient)
        {
        }

        /// <inheritdoc />
        protected override string RequestLink() => "https://egov.kz/services/P30.03/";
        
        /// <inheritdoc />
        protected override BiinType TypeOfBiin() => BiinType.BIN;

        /// <summary>
        /// Parsing of ul participation reference and getting child companies
        /// </summary>
        /// <param name="bin">BIN of the company</param>
        /// <param name="captchaApiKey">API key for solving captchas</param>
        /// <param name="saveFolderPath">Defines where to save file</param>
        /// <param name="delay">Delay of checking if the reference is in ms</param>
        /// <param name="deleteFile">If the file should be deleted after parsing</param>
        /// <param name="timeout">Timeout</param>
        /// <returns>IEnumerable - list of companies where person is head</returns>
        public async Task<IEnumerable<string>> GetChildCompaniesAsync(string bin, string captchaApiKey,
            string saveFolderPath = null,
            int delay = 1000,
            bool deleteFile = false, int timeout = 60000)
        {
            saveFolderPath ??= Path.GetTempPath();

            var reference = await GetReferenceAsync(bin, captchaApiKey, delay, timeout);
            var temp = reference.First(x => x.language.Contains("ru"));

            return temp != null
                ? new UlParticipationPdfParser(
                        await temp.SaveFileAsync(saveFolderPath, CamelliaClient.HttpClient,
                            $"{bin.TrimStart('0')}_ul_participation"), deleteFile)
                    .GetChildCompanies()
                : null;
        }
    }
}