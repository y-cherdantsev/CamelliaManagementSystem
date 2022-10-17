using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data;
using CamelliaManagementSystem.FileManage.DictionaryParsers;
using CamelliaManagementSystem.FileManage.HtmlParsers;

// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace CamelliaManagementSystem.Requests.References
{
    /// @author Yevgeniy Cherdantsev
    /// @date 07.03.2020 16:49:47
    /// <summary>
    /// FL participation reference with information about participation of person in companies
    /// </summary>
    public sealed class FlParticipationReference : BiinCaptchaRequest
    {
        /// <inheritdoc />
        public FlParticipationReference(CamelliaClient camelliaClient) : base(camelliaClient)
        {
        }

        /// <inheritdoc />
        protected override string RequestLink() => "https://egov.kz/services/P30.04/";

        /// <inheritdoc />
        protected override BiinType TypeOfBiin() => BiinType.IIN;

        /// <summary>
        /// Parsing of fl participation reference and getting companies in which defined person is head
        /// </summary>
        /// <param name="iin">IIN of the person</param>
        /// <param name="captchaApiKey">API key for solving captchas</param>
        /// <param name="saveFolderPath">Defines where to save file</param>
        /// <param name="delay">Delay of checking if the reference is in ms</param>
        /// <param name="deleteFile">If the file should be deleted after parsing</param>
        /// <param name="timeout">Timeout</param>
        /// <returns>IEnumerable - list of companies where person is head</returns>
        public async Task<IEnumerable<string>> GetWherePersonIsHeadAsync(string iin, string captchaApiKey,
            string saveFolderPath = null,
            int delay = 1000,
            bool deleteFile = false, int timeout = 60000)
        {
            saveFolderPath ??= Path.GetTempPath();

            var reference = await GetReferenceAsync(iin, captchaApiKey, delay, timeout);
            var temp = reference.First(x => x.language.Contains("ru"));

            if (temp.url.Split(".").Last().ToLower().Contains("htm") ||
                temp.url.Split(".").Last().ToLower().Contains("html"))
                return new FlParticipationHtmlParser(
                        await temp.SaveFileAsync(saveFolderPath, CamelliaClient.HttpClient,
                            $"{iin.TrimStart('0')}_fl_participation"), deleteFile)
                    .GetWhereIsHead();
            if (temp.url.Split(".").Last().ToLower().Contains("pdf"))
                return new FlParticipationPdfDictionaryParser(
                        await temp.SaveFileAsync(saveFolderPath, CamelliaClient.HttpClient,
                            $"{iin.TrimStart('0')}_registration"), deleteFile)
                    .GetWhereIsHead();
            throw new DataException($"Not found such type of file: {temp.url}");
        }

        /// <summary>
        /// Parsing of fl participation reference and getting persons fullname
        /// </summary>
        /// <param name="iin">IIN of the person</param>
        /// <param name="captchaApiKey">API key for solving captchas</param>
        /// <param name="saveFolderPath">Defines where to save file</param>
        /// <param name="delay">Delay of checking if the reference is in ms</param>
        /// <param name="deleteFile">If the file should be deleted after parsing</param>
        /// <param name="timeout">Timeout</param>
        /// <returns>string - persons fullname</returns>
        public async Task<string> GetPersonFullnameAsync(string iin, string captchaApiKey,
            string saveFolderPath = null,
            int delay = 1000,
            bool deleteFile = false, int timeout = 60000)
        {
            saveFolderPath ??= Path.GetTempPath();

            var reference = await GetReferenceAsync(iin, captchaApiKey, delay, timeout);
            var temp = reference.First(x => x.language.Contains("ru"));

            if (temp.url.Split(".").Last().ToLower().Contains("htm") ||
                temp.url.Split(".").Last().ToLower().Contains("html"))
                return new FlParticipationHtmlParser(
                        await temp.SaveFileAsync(saveFolderPath, CamelliaClient.HttpClient,
                            $"{iin.TrimStart('0')}_fl_participation"), deleteFile)
                    .GetPersonFullname();
            if (temp.url.Split(".").Last().ToLower().Contains("pdf"))
                return new FlParticipationPdfDictionaryParser(
                        await temp.SaveFileAsync(saveFolderPath, CamelliaClient.HttpClient,
                            $"{iin.TrimStart('0')}_fl_participation"), deleteFile)
                    .GetPersonFullname();
            throw new DataException($"Not found such type of file: {temp.url}");
            

        }
    }
}