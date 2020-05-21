using System;
using System.IO;
using System.Net;
using System.Net.Http;

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

        public string SaveFile(string path, string fileName = null, IWebProxy proxy = null)
        {
            if (fileName == null)
            {
                fileName = $"{nameEn} - {DateTime.Now.Ticks}";
            }
            else
            {
                fileName = fileName.Replace(".PDF", string.Empty).Replace(".pdf", string.Empty);
            }
            
            using var webClient = new WebClient();
            if (proxy != null)
            {
                webClient.Proxy = proxy;
            }
            webClient.Proxy = proxy;
            var fullName = $"{new DirectoryInfo(path).FullName}\\{fileName}.pdf";

            for (var i = 0; i < 10; i++)
            {
                if (!new FileInfo(fullName).Exists || new FileInfo(fullName).Length<10000)
                {
                    try
                    {
                        new FileInfo(fullName).Delete();

                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    webClient.DownloadFileTaskAsync(url, fullName).GetAwaiter()
                        .GetResult();
                }
                else
                {
                    break;
                }
            }
            

            return $"{path}\\{fileName}.pdf";
        }
    }
}