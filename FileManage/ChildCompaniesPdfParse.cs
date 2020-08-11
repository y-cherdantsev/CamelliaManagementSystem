using System.Collections.Generic;
using System.IO;
using System.Linq;

//TODO(REFACTOR)
namespace CamelliaManagementSystem.FileManage
{
    /// @author Yevgeniy Cherdantsev
    /// @date 14.03.2020 14:49:19
    /// @version 1.0
    /// <summary>
    /// Gets bins of all child companies
    /// </summary>
    public sealed class ChildCompaniesPdfParse : PdfParse
    {
        /// <summary>
        /// Get list of child companies from the reference
        /// </summary>
        /// <param name="innerText">text of the reference</param>
        /// <returns>IEnumerable - list of child companies</returns>
        /// <exception cref="InvalidDataException">If no information were found</exception>
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
            childCompanies.RemoveAll(x => x.Contains("-"));
            childCompanies = childCompanies.Distinct().ToList();
            if (childCompanies.Count < 1)
                throw new InvalidDataException("No information were found in the reference");

            return childCompanies;
        }
    }
}