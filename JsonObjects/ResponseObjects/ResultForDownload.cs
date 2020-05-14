using System;
using System.IO;
using System.Net;

namespace Camellia_Management_System.JsonObjects.ResponseObjects
{
    /// @author Yevgeniy Cherdantsev
    /// @date 18.02.2020 12:18:34
    /// @version 1.0
    /// <summary>
    /// ResultForDownload json object
    /// </summary>
    public class ResultForDownload
    {
        public string nameKk { get; set; } = "";
        public string nameRu { get; set; } = "";
        public string nameEn { get; set; } = "";
        public string url { get; set; } = "";
        public string language { get; set; } = "";
        public string name { get; set; } = "";

        public string SaveFile(string path, string fileName = null)
        {
            if (fileName == null)
            {
                fileName = $"{nameEn} - {DateTime.Now.Ticks}";
            }else
            {
                fileName = fileName.Replace(".PDF", string.Empty).Replace(".pdf", string.Empty);
            }

            using var webClient = new WebClient();
            webClient.DownloadFileTaskAsync(url, $"{new DirectoryInfo(path).FullName}\\{fileName}.pdf").GetAwaiter().GetResult();
            return $"{path}\\{fileName}.pdf";
        }
    }
}