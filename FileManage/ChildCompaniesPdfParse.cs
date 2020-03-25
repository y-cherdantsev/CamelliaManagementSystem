using System.Collections.Generic;
using System.IO;

namespace Camellia_Management_System.FileManage
{
    /// @author Yevgeniy Cherdantsev
    /// @date 14.03.2020 14:49:19
    /// @version 1.0
    /// <summary>
    /// Gets bins of all child companies
    /// </summary>
    public class ChildCompaniesPdfParse : PdfParse
    {
        public static IEnumerable<string> GetChildCompanies(string innerText)
        {
            var childCompanies = new List<string>();
            innerText = MinimizeReferenceText(innerText).Replace("\r\n", string.Empty);

            while (innerText.Contains("<b>БИН</b>"))
            {
                innerText = innerText.Substring(innerText.IndexOf("<b>БИН</b>") + 10,
                    innerText.Length - innerText.IndexOf("<b>БИН</b>") - 10);
                childCompanies.Add(innerText.Substring(0, innerText.IndexOf("<b>")).Replace("\n", string.Empty));
            }

            childCompanies.Remove(childCompanies[0]);
            if (childCompanies.Count < 1)
                throw new InvalidDataException("No information were found in the reference");

            return childCompanies;
        }
    }
}