using System;
using System.IO;
using System.Net;
using System.Net.Http;

//TODO(REFACTOR)
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
        public string nameKk { get; set; }
        public string nameRu { get; set; }
        public string nameEn { get; set; }
        public string url { get; set; }
        public string language { get; set; }

        public Languages lang
        {
            get
            {
                if (language.Equals("ru"))
                    return Languages.RU;

                if (language.Equals("kz"))
                    return Languages.KZ;
                
                return Languages.UNKNOWN;
            }
        }

        public string name { get; set; }

        public string SaveFile(string path, HttpClient client, string fileName = null)
        {
            if (fileName == null)
            {
                fileName = $"{nameEn} - {DateTime.Now.Ticks}";
            }
            else
            {
                fileName = fileName.Replace(".PDF", string.Empty).Replace(".pdf", string.Empty);
            }


            var fullName = $"{new DirectoryInfo(path).FullName}\\{fileName}.pdf";

            for (int i = 0; i < 10; ++i)
            {
                if (!new FileInfo(fullName).Exists || new FileInfo(fullName).Length < 10000)
                {
                    try
                    {
                        new FileInfo(fullName).Delete();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    using var request = new HttpRequestMessage(HttpMethod.Get, url);
                    using Stream contentStream = (client.SendAsync(request).GetAwaiter().GetResult()).Content
                            .ReadAsStreamAsync().GetAwaiter().GetResult(),
                        stream = new FileStream(fullName, FileMode.Create, FileAccess.Write, FileShare.None,
                            4000000, true);
                    contentStream.CopyToAsync(stream).GetAwaiter().GetResult();
                }
                else
                {
                    break;
                }
            }


            return $"{path}\\{fileName}.pdf";
        }


        public enum Languages
        {
            RU,
            KZ,
            UNKNOWN
        }
    }
}