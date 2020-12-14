using System;
using System.Diagnostics;
using System.IO;
using CamelliaManagementSystem.Requests;

// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable StringIndexOfIsCultureSpecific.1

namespace CamelliaManagementSystem.FileManage.PlainTextParsers
{
    /// @author Yevgeniy Cherdantsev
    /// @date 14.03.2020 14:50:59
    /// <summary>
    /// Parsing pdf references using plain text method, requires pdftohtml utility
    /// </summary>
    public abstract class PdfPlainTextParser
    {
        /// <summary>
        /// Inner text of a pdf
        /// </summary>
        protected string InnerText;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <param name="deleteFile">If the object should delete file after parsing</param>
        protected PdfPlainTextParser(string path, bool deleteFile = true)
        {
            var file = new FileInfo(path);

            if (!file.Exists)
                throw new CamelliaFileException($"No file has been found; Full path:'{file.FullName}'");

            try
            {
                InnerText = GetTextFromPdf(file);
                InnerText = InnerText.Replace("<br/>", "<br>");
                InnerText = InnerText.Replace("<hr/>", "<hr>");
                InnerText = InnerText.Replace("&#160;", "");
            }
            catch (Exception)
            {
                // ignored
            }

            if (deleteFile)
                file.Delete();
        }

        /// <summary>
        /// Gets text from pdf file (pdftohtml.exe util required)
        /// </summary>
        /// <param name="file">File that should be parsed</param>
        /// <returns>string - inner text</returns>
        private static string GetTextFromPdf(FileSystemInfo file)
        {
            var system = Environment.OSVersion.Platform;
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (system)
            {
                case PlatformID.Win32NT:
                {
                    var command = $"pdftohtml.exe -i -noframes -nomerge -enc UTF-8 \"{file.FullName}\"";
                    Process.Start("cmd.exe", "/C " + command)?.WaitForExit();
                    break;
                }
                case PlatformID.Unix:
                    Process.Start("/usr/bin/pdftohtml", $"-i -noframes -nomerge -enc UTF-8 \"{file.FullName}\"")
                        ?.WaitForExit();
                    break;
                default:
                    throw new Exception("This OS type not supported");
            }

            var htmlFile = new FileInfo(file.FullName.Replace(file.Extension, ".html"));
            var text = File.ReadAllText(htmlFile.FullName);
            try
            {
                htmlFile.Delete();
            }
            catch (Exception)
            {
                // ignored
            }

            return text;
        }
        
        /// <summary>
        /// Minimization of a text and removing of unnecessary symbols before using it
        /// </summary>
        /// <returns>string - minimized text</returns>
        /// todo(Not usable for all references)
        protected void MinimizeReferenceText()
        {
            InnerText = MinimizeReferenceText(InnerText);
        }
        
        /// <summary>
        /// Minimization of a text and removing of unnecessary symbols before using it
        /// </summary>
        /// <returns>string - minimized text</returns>
        /// todo(Not usable for all references)
        protected string MinimizeReferenceText(string innerText)
        {
            innerText = innerText.Trim();
            var to = "<b>Наименование";
            try
            {
                var position = innerText.ToLower().IndexOf(to.ToLower(), StringComparison.Ordinal);
                innerText = innerText.Substring(position,
                    innerText.Length - position);
            }
            catch (Exception)
            {
                // ignored
            }

            while (true)
            {
                try
                {
                    const string from = "Осы құжат «Электрондық";
                    to = "<hr>";
                    if (!innerText.ToLower().Contains(from.ToLower()))
                        break;
                    var positionFrom = innerText.ToLower().IndexOf(from.ToLower());
                    var positionTo = innerText.ToLower().IndexOf(to.ToLower());
                    var text1 = innerText.Substring(0, positionFrom);
                    var text2 = innerText.Substring(positionTo + to.Length,
                        innerText.Length - positionTo - to.Length);
                    innerText = text1.Trim() + "\n" + text2.Trim();
                }
                catch (Exception)
                {
                    break;
                }
            }

            while (true)
            {
                try
                {
                    const string from = "<a name=";
                    to = "Дата получения<br>";
                    if (!innerText.ToLower().Contains(from.ToLower()))
                        break;
                    var text1 = innerText.Substring(0, innerText.ToLower().IndexOf(from.ToLower()));
                    var text2 = innerText.Substring(innerText.ToLower().IndexOf(to.ToLower()) + to.Length,
                        innerText.Length - innerText.ToLower().IndexOf(to.ToLower()) - to.Length);
                    innerText = text1.Trim() + "\n" + text2.Trim();
                }
                catch (Exception)
                {
                    break;
                }
            }

            innerText = innerText
                .Replace("&quot;", "\"")
                .Replace("&#34;", "\"")
                .Replace("&quot", "\"")
                .Replace("&#34", "\"")
                .Replace("</BODY>", string.Empty)
                .Replace("</body>", string.Empty)
                .Replace("</HTML>", string.Empty)
                .Replace("</html>", string.Empty)
                .Replace("<br>", string.Empty);

            return innerText;
        }
    }
}