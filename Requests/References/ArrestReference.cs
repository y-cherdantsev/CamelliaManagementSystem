using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data;
using CamelliaManagementSystem.FileManage.HtmlParsers;
using CamelliaManagementSystem.FileManage.PlainTextParsers;
using iText.StyledXmlParser.Jsoup.Parser;

// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace CamelliaManagementSystem.Requests.References
{
    /// @author Yevgeniy Cherdantsev
    /// @date 07.03.2020 16:49:47
    /// <summary>
    /// Arrest reference
    /// </summary>
    public sealed class ArrestReference : BiinRequest
    {
        /// <inheritdoc />
        public ArrestReference(CamelliaClient camelliaClient) : base(camelliaClient)
        {
        }

        /// <inheritdoc />
        protected override string RequestLink() => "https://egov.kz/services/P30.08/";

        /// <inheritdoc />
        protected override BiinType TypeOfBiin() => BiinType.BIN;

        /// <summary>
        /// Parsing of registration reference and getting of founders from it
        /// </summary>
        /// <param name="bin">BIN</param>
        /// <param name="saveFolderPath">Defines where to save file</param>
        /// <param name="delay">Delay of checking if the reference is in ms</param>
        /// <param name="deleteFile">If the file should be deleted after parsing</param>
        /// <param name="timeout">Timeout</param>
        /// <returns>IEnumerable - list of founders</returns>
        public async Task<Dictionary<string, string>> GetArrestsAsync(string bin, string saveFolderPath = null,
            bool deleteFile = false, int delay = 1000,
            int timeout = 20000)
        {
            saveFolderPath ??= Path.GetTempPath();

            var reference = await GetReferenceAsync(bin, delay, timeout);
            var temp = reference.First(x => x.language.Contains("ru"));

            if (temp.url.Split(".").Last().ToLower().Contains("htm") ||
                temp.url.Split(".").Last().ToLower().Contains("html"))
            {
                var file = await temp.SaveFileAsync(saveFolderPath, CamelliaClient.HttpClient,
                    $"{bin.TrimStart('0')}_arrest");
                var parser = new ArrestHtmlParser(await temp.SaveFileAsync(saveFolderPath, CamelliaClient.HttpClient,
                        $"{bin.TrimStart('0')}_arrest")
                    , deleteFile);
                parser.DeleteField(file, "Выдана");
                return parser.GetArrest();
            }

            throw new DataException($"Not found such type of file: {temp.url}");
        }
    }
}