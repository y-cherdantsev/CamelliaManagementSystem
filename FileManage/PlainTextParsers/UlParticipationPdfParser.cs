using System.Linq;
using System.Collections.Generic;
using CamelliaManagementSystem.Requests;

//TODO(REFACTOR)
namespace CamelliaManagementSystem.FileManage.PlainTextParsers
{
    /// @author Yevgeniy Cherdantsev
    /// @date 14.03.2020 14:49:19
    /// <summary>
    /// Gets bins of all child companies
    /// </summary>
    public sealed class UlParticipationPdfParser : PdfPlainTextParser
    {
        /// <inheritdoc />
        public UlParticipationPdfParser(string path, bool deleteFile = true) : base(path, deleteFile)
        {
            MinimizeReferenceText();
        }
        
        
        /// <summary>
        /// Get list of child companies from the reference
        /// </summary>
        /// <returns>IEnumerable - list of child companies</returns>
        /// <exception cref="CamelliaNoneDataException">If no information were found</exception>
        public IEnumerable<string> GetChildCompanies()
        {
            var innerText = InnerText;
            innerText = innerText.Replace("\r\n", string.Empty);
            
            var childCompanies = new List<string>();

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
                throw new CamelliaNoneDataException("No information were found in the reference");

            return childCompanies;
        }
    }
}