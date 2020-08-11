using System.Collections.Generic;

//TODO(REFACTOR)
namespace CamelliaManagementSystem.FileManage
{
    /// @author Yevgeniy Cherdantsev
    /// @date 14.05.2020 14:49:19
    /// @version 1.0
    /// <summary>
    /// Parse text and gets where the person is head
    /// </summary>
    public class WhereIsHeadPdfParse : PdfParse
    {
        /// @author Yevgeniy Cherdantsev
        /// @date 14.05.2020 16:48:22
        /// @version 1.0
        /// <summary>
        /// Parse text and gets where the person is head
        /// </summary>
        /// <param name="innerText">Text of a pdf file</param>
        /// <returns>IEnumerable - List of bins</returns>
        public static IEnumerable<string> GetWhereIsHead(string innerText)
        {
            var companies = new List<string>();
            var fullname = innerText.Substring(innerText.IndexOf("<b>Ф.И.О.</b><br>")+17, 
                innerText.Substring(innerText.IndexOf("<b>Ф.И.О.</b><br>")+17).IndexOf("<br>"))
                .Replace("\n", string.Empty)
                .Replace("\r", string.Empty)
                .Replace("<br>", string.Empty)
                .Replace(" ", string.Empty);
            innerText = MinimizeReferenceText(innerText);
            while (innerText.Contains("<b>БИН</b>"))
            {
                innerText = innerText.Substring(innerText.IndexOf("<b>БИН</b>") + 12,
                    innerText.Length - innerText.IndexOf("<b>БИН</b>\r\n") - 12);
                var checkText = innerText.Substring(0, innerText.IndexOf("<b>Местонахождение</b>"))
                    .Replace("<b>Первый руководитель</b>", string.Empty)
                    .Replace("\n", string.Empty)
                    .Replace("\r", string.Empty);
                if (checkText.Replace(" ", string.Empty).Contains(fullname))
                    companies.Add(innerText.Substring(0, innerText.IndexOf("\n")).Replace("\r", string.Empty));
            }
            return companies;
        }
    }
}