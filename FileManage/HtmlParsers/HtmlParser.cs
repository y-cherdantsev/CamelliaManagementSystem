using System;
using System.IO;
using AngleSharp.Html.Dom;
using CamelliaManagementSystem.Requests;

// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable StringIndexOfIsCultureSpecific.1

namespace CamelliaManagementSystem.FileManage.HtmlParsers
{
    /// @author Yevgeniy Cherdantsev
    /// @date 14.03.2020 14:50:59
    /// <summary>
    /// Parsing html references using plain text method
    /// </summary>
    public abstract class HtmlParser
    {
        /// <summary>
        /// Inner HTML DOM of a document
        /// </summary>
        protected readonly IHtmlDocument HtmlDoc;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <param name="deleteFile">If the object should delete file after parsing</param>
        protected HtmlParser(string path, bool deleteFile = true)
        {
            var file = new FileInfo(path);

            if (!file.Exists)
                throw new CamelliaFileException($"No file has been found; Full path:'{file.FullName}'");

            try
            {
                HtmlDoc = GetHtmlFromFile(file);
            }
            catch (Exception)
            {
                // ignored
            }

            if (deleteFile)
                file.Delete();
        }

        /// <summary>
        /// Gets text from html file
        /// </summary>
        /// <param name="file">File that should be parsed</param>
        /// <returns>string - inner text</returns>
        private static IHtmlDocument GetHtmlFromFile(FileSystemInfo file){
            var htmlFile = new FileInfo(file.FullName.Replace(file.Extension, ".html"));
            var text = File.ReadAllText(htmlFile.FullName);
            var htmlDoc = new AngleSharp.Html.Parser.HtmlParser().ParseDocument(text);
            return htmlDoc;
        }
    }
}