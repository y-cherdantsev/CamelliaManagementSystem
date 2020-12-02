using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using CamelliaManagementSystem.FileManage;

// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace CamelliaManagementSystem.Requests.References
{
    /// @author Yevgeniy Cherdantsev
    /// @date 07.03.2020 16:49:47
    /// <summary>
    /// Registration activities reference with activities dates
    /// </summary>
    public class RegistrationActivitiesReference : BiinRequest
    {
        /// <inheritdoc />
        public RegistrationActivitiesReference(CamelliaClient camelliaClient) : base(camelliaClient)
        {
        }

        /// <inheritdoc />
        protected override string RequestLink() => "https://egov.kz/services/P30.05/";

        /// <inheritdoc />
        protected override BiinType TypeOfBiin() => BiinType.BIN;

        /// <summary>
        /// Parsing of registration activities reference and getting activities dates
        /// </summary>
        /// <param name="bin">BIN</param>
        /// <param name="saveFolderPath">Defines where to save file</param>
        /// <param name="delay">Delay of checking if the reference is in ms</param>
        /// <param name="deleteFile">If the file should be deleted after parsing</param>
        /// <param name="timeout">Timeout</param>
        /// <returns>IEnumerable - list of founders</returns>
        public async Task<IEnumerable<ActivitiesDatePdfParse.DateActivity>> GetActivitiesDatesAsync(string bin,
            string saveFolderPath = null, int delay = 1000, bool deleteFile = false, int timeout = 20000)
        {
            saveFolderPath ??= Path.GetTempPath();

            var reference = await GetReferenceAsync(bin, delay, timeout);
            var temp = reference.First(x => x.language.Contains("ru"));

            return temp != null
                ? new PdfParser(
                        await temp.SaveFileAsync(saveFolderPath, CamelliaClient.HttpClient,
                            $"{bin.TrimStart('0')}_activities"), deleteFile)
                    .GetActivitiesDates()
                : null;
        }
    }
}