using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CamelliaManagementSystem.Requests;
using iText.Kernel.Colors;
using iText.PdfCleanup.Autosweep;

namespace CamelliaManagementSystem.FileManage.DictionaryParsers
{
    /// <summary>
    /// Parser pdf reference using dictionaries
    /// </summary>
    public class PdfDictionaryParser
    {
        protected internal readonly string FilePath;
        protected internal Dictionary<string, List<string>> Dictionary;

        public PdfDictionaryParser(string filePath, bool deleteFile = false)
        {
            FilePath = filePath;
            LoadDictionary(deleteFile).GetAwaiter().GetResult();
        }

        public async Task LoadDictionary(bool deleteFile)
        {
            var file = new FileInfo(FilePath);

            if (!file.Exists)
                throw new CamelliaFileException($"No file has been found; Full path:'{file.FullName}'");
            
            Dictionary = await new PdfDictTransformer()
                .ReadPdfFileCreateListData(FilePath);
            
            if (deleteFile)
                new FileInfo(FilePath).Delete();
        }

        /// <summary>
        /// Changes objects that approaches to a given regex to white color
        /// </summary>
        /// <param name="newFilePath">Result pdf file path</param>
        /// <param name="regex">Regex in string format</param>
        /// <param name="redactionColor">Color of redaction</param>
        public void DeleteField(string newFilePath, string regex, Color redactionColor = null)
        {
            using var pdf = new iText.Kernel.Pdf.PdfDocument(new iText.Kernel.Pdf.PdfReader(FilePath),
                new iText.Kernel.Pdf.PdfWriter(File.Open(newFilePath, FileMode.Create)));
            var cleanupStrategy =
                new RegexBasedCleanupStrategy(new Regex(regex, RegexOptions.IgnoreCase)).SetRedactionColor(
                    redactionColor ?? ColorConstants.WHITE);
            var autoSweep = new PdfAutoSweep(cleanupStrategy);
            autoSweep.CleanUp(pdf);
        }
    }
}