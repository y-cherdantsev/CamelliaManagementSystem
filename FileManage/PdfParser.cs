using System;
using System.Collections.Generic;
using System.IO;

namespace Camellia_Management_System.FileManage
{
    /// @author Yevgeniy Cherdantsev
    /// @date 07.03.2020 17:22:27
    /// @version 1.0
    /// <summary>
    /// Class that parsing pdf and gets information from them in centralized objects
    /// </summary>
    public class PdfParser
    {
        private readonly string _innerText;

        public PdfParser(string path, bool deleteFile = false)
        {
            var file = new FileInfo(path);
            _innerText = GetTextFromPdf(file);
            if (deleteFile)
                file.Delete();
        }

        private static string GetTextFromPdf(FileSystemInfo file)
        {
            var command = $"pdftohtml.exe -i -noframes -nomerge -enc UTF-8 \"{file.FullName}\"";
            System.Diagnostics.Process.Start("cmd.exe", "/C " + command)?.WaitForExit();

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
        /// INPUT
        /// </summary>
        /// <returns>IEnumerable - list of founders</returns>
        public IEnumerable<string> GetFounders()
        {
            return FoundersPdfParse.GetFounders(_innerText);
        }
        
    }
}