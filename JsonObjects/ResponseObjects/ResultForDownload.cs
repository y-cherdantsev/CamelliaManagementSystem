using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

// ReSharper disable CommentTypo
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

#pragma warning disable 1591

namespace CamelliaManagementSystem.JsonObjects.ResponseObjects
{
    /// @author Yevgeniy Cherdantsev
    /// @date 18.02.2020 12:18:34
    /// <summary>
    /// ResultForDownload json object
    /// </summary>
    public class ResultForDownload : IDisposable
    {
        public string nameKk { get; set; }
        public string nameRu { get; set; }
        public string nameEn { get; set; }
        public string url { get; set; }
        public string language { get; set; }
        public string name { get; set; }

        /// <summary>
        /// Language of the file in from Enum
        /// </summary>
        public Languages languageCode
        {
            get
            {
                return language switch
                {
                    "ru" => Languages.Ru,
                    "kz" => Languages.Kz,
                    _ => Languages.Unknown
                };
            }
        }

        /// <summary>
        /// Downloads result for download locally
        /// </summary>
        /// <param name="path">Where file should be saved</param>
        /// <param name="client">Client that will proceed request</param>
        /// <param name="fileName">Name of the file</param>
        /// <returns>Path of the file that has been saved</returns>
        /// ReSharper disable once CognitiveComplexity
        public async Task<string> SaveFileAsync(string path, HttpClient client, string fileName = null)
        {
            // Generating name of a file using known values
            fileName = fileName == null
                ? $"{nameEn} - {DateTime.Now.Ticks}"
                : fileName.Replace(".PDF", string.Empty).Replace(".pdf", string.Empty);


            var fullName = Path.Combine(path, $"{fileName}.pdf");

            for (var i = 0; i < 10; ++i)
            {
                if (new FileInfo(fullName).Exists)
                    try
                    {
                        new FileInfo(fullName).Delete();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                var result = await client.GetAsync(url);
                await using Stream contentStream =
                        await result.Content.ReadAsStreamAsync(),
                    stream = new FileStream(fullName, FileMode.Create, FileAccess.Write, FileShare.None,
                        4000000, true);
                await contentStream.CopyToAsync(stream);

                if (new FileInfo(fullName).Exists && new FileInfo(fullName).Length >= 10000) break;
            }

            return Path.Combine(path, $"{fileName}.pdf");
        }
        
        /// <inheritdoc />
        public void Dispose()
        {
            nameKk = null;
            nameRu = null;
            nameEn = null;
            url = null;
            language = null;
            name = null;
        }
        
        
        /// <summary>
        /// Enumerates different languages of the reference
        /// </summary>
        public enum Languages
        {
            /// <summary>
            /// Russian language
            /// </summary>
            Ru,

            /// <summary>
            /// Kazakh language 
            /// </summary>
            Kz,

            /// <summary>
            /// Unknown language 
            /// </summary>
            Unknown
        }
    }
}