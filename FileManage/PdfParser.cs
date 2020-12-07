using System;
using System.IO;
using System.Diagnostics;
using iText.Kernel.Colors;
using System.Collections.Generic;
using iText.PdfCleanup.Autosweep;
using System.Text.RegularExpressions;
using CamelliaManagementSystem.Requests;

// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable UnusedMember.Global

namespace CamelliaManagementSystem.FileManage
{
    /// @author Yevgeniy Cherdantsev
    /// @date 07.03.2020 17:22:27
    /// <summary>
    /// Class that parsing pdf and gets information from them in centralized objects
    /// </summary>
    public class PdfParser
    {
        private readonly string _innerText;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <param name="deleteFile">If the object should delete file after parsing</param>
        public PdfParser(string path, bool deleteFile = true)
        {
            var file = new FileInfo(path);

            if (!file.Exists)
                throw new CamelliaFileException($"No file has been found; Full path:'{file.FullName}'");

            try
            {
                _innerText = GetTextFromPdf(file);
                _innerText = _innerText.Replace("<br/>", "<br>");
                _innerText = _innerText.Replace("<hr/>", "<hr>");
                _innerText = _innerText.Replace("&#160;", "");
            }
            catch (Exception)
            {
                // ignored
            }

            if (deleteFile)
                file.Delete();
        }

        /// <summary>
        /// Changes objects that approaches to a given regex to black color
        /// </summary>
        /// <param name="filePath">Source pdf file path</param>
        /// <param name="newFilePath">Result pdf file path</param>
        /// <param name="regex">Regex in string format</param>
        public static void DeleteField(string filePath, string newFilePath, string regex)
        {
            using var pdf = new iText.Kernel.Pdf.PdfDocument(new iText.Kernel.Pdf.PdfReader(filePath), new iText.Kernel.Pdf.PdfWriter(File.Open(newFilePath, FileMode.Create)));
            var cleanupStrategy = new RegexBasedCleanupStrategy(new Regex(regex, RegexOptions.IgnoreCase)).SetRedactionColor(ColorConstants.WHITE);
            var autoSweep = new PdfAutoSweep(cleanupStrategy);
            autoSweep.CleanUp(pdf);
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

        /// @author Yevgeniy Cherdantsev
        /// @date 10.03.2020 10:25:01
        /// @version 1.0
        /// <summary>
        /// Parsing of registration reference and getting of founders from it
        /// </summary>
        /// <returns>IEnumerable - list of founders</returns>
        public IEnumerable<string> GetFounders() => FoundersPdfParse.GetFounders(_innerText);

        /// @author Yevgeniy Cherdantsev
        /// @version 1.0
        /// <summary>
        /// Parsing of participation ul reference and getting of child companies from it
        /// </summary>
        /// <returns>IEnumerable - list of child companies</returns>
        public IEnumerable<string> GetChildCompanies() => ChildCompaniesPdfParse.GetChildCompanies(_innerText);

        /// @author Yevgeniy Cherdantsev
        /// @version 1.0
        /// <summary>
        /// Parsing of participation fl reference and getting of companies from it
        /// </summary>
        /// <returns>IEnumerable - list of companies where person is head</returns>
        public IEnumerable<string> GetWherePersonIsHead() => WhereIsHeadPdfParse.GetWhereIsHead(_innerText);

        /// <summary>
        /// Parsing of dates and changes
        /// </summary>
        /// <returns>IEnumerable - list of changes with dates</returns>
        public IEnumerable<ActivitiesDatePdfParse.DateActivity> GetActivitiesDates() =>
            ActivitiesDatePdfParse.GetDatesChanges(_innerText);

        /// <summary>
        /// Parsing of heads from registered date reference
        /// </summary>
        /// <returns>string - head of the company</returns>
        public string GetHead() => RegisteredDateParse.GetHead(_innerText);

        /// <summary>
        /// Parsing of company names from registered date reference
        /// </summary>
        /// <returns>string - name of the company</returns>
        public string GetName() => RegisteredDateParse.GetName(_innerText);

        /// <summary>
        /// Parsing of company address from registered date reference
        /// </summary>
        /// <returns>string - address of the company</returns>
        public string GetPlace() => RegisteredDateParse.GetPlace(_innerText);

        /// <summary>
        /// Parse nimber of founders from registered date reference
        /// </summary>
        /// <returns>int - number of the founders</returns>
        public int CountFounders() => Convert.ToInt32(RegisteredDateParse.CountFounders(_innerText));

        /// <summary>
        /// Parsing occupation of the company from registered date reference
        /// </summary>
        /// <returns>string - occupation of the company</returns>
        public string GetOccupation() => RegisteredDateParse.GetOccupation(_innerText);
    }
}