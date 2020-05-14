using System.Collections.Generic;

namespace Camellia_Management_System.FileManage
{
    public class WhereIsHeadPdfParse : PdfParse
    {
        /// @author Yevgeniy Cherdantsev
        /// @date 14.15.2020 16:48:22
        /// @version 1.0
        /// <summary>
        /// Parsing text and gets where the person is head
        /// </summary>
        /// <param name="innerText">Text of a pdf file</param>
        /// <returns>IEnumerable - List of bins</returns>
        public static IEnumerable<string> GetWhereIsHead(string innerText)
        {
            var companies = new List<string>();
            innerText = MinimizeReferenceText(innerText);
            while (innerText.Contains("<b>БИН</b>"))
            {
                innerText = innerText.Substring(innerText.IndexOf("<b>БИН</b>") + 12,
                    innerText.Length - innerText.IndexOf("<b>БИН</b>\r\n") - 12);
                companies.Add(innerText.Substring(0, innerText.IndexOf("\n")));
            }
            return companies;
        }
    }
}