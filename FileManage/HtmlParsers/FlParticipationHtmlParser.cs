using System.Data;
using System.Linq;
using AngleSharp.Dom;
using System.Collections.Generic;

// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable UnusedType.Global
// ReSharper disable StringIndexOfIsCultureSpecific.1

namespace CamelliaManagementSystem.FileManage.HtmlParsers
{
    /// @author Yevgeniy Cherdantsev
    /// @date 11.02.2021 02:05:22
    /// <summary>
    /// Parsing of fl participation reference and getting companies from it
    /// </summary>
    public class FlParticipationHtmlParser : HtmlParser
    {
        /// <inheritdoc />
        public FlParticipationHtmlParser(string path, bool deleteFile = true) : base(path, deleteFile)
        {
        }

        /// <summary>
        /// Get list of companies where person is head
        /// </summary>
        /// <returns>List of companies bin where person is head</returns>
        /// <exception cref="DataException">If some error with parsing occured</exception>
        public IEnumerable<string> GetWhereIsHead()
        {
            var companies = new List<string>();
            var fullname = GetPersonFullname();
            var htmlDoc = new AngleSharp.Html.Parser.HtmlParser().ParseDocument(InnerText);
            var tableRows = htmlDoc.QuerySelectorAll("td").FirstOrDefault(x => x.GetAttribute("align") == "center")?
                .QuerySelectorAll("tr");
            var dataRows = tableRows!.Where(x =>
                    x.InnerHtml.ToUpper().Contains(">БИН<") || x.InnerHtml.ToUpper().Contains("ПЕРВЫЙ РУКОВОДИТЕЛЬ"))
                .ToList();
            if (dataRows.Any() && dataRows.Count % 2 == 0)
            {
                for (var i = 0; i < dataRows.Count; i += 2)
                {
                    var bin = dataRows[i].QuerySelectorAll("span")
                        .FirstOrDefault(x =>
                            !x.GetAttribute("style").Contains("font-weight: bold")).Text();
                    var tempFullname = dataRows[i + 1].QuerySelectorAll("span")
                        .FirstOrDefault(x =>
                            !x.GetAttribute("style").Contains("font-weight: bold")).Text();
                    if (tempFullname!.Contains(fullname))
                        companies.Add(bin);
                }
            }
            else if (dataRows.Any() && dataRows.Count % 2 != 0)
                throw new DataException($"Some error with parsing occured; Number of found rows {dataRows.Count()}");

            return companies;
        }

        /// <summary>
        /// Get person fullname from reference
        /// </summary>
        /// <returns>Person fullname</returns>
        public string GetPersonFullname()
        {
            var htmlDoc = new AngleSharp.Html.Parser.HtmlParser().ParseDocument(InnerText);
            var fullnameRow = htmlDoc.QuerySelectorAll("td").FirstOrDefault(x => x.GetAttribute("align") == "center")?
                .QuerySelectorAll("tr").FirstOrDefault(x => x.InnerHtml.Contains("Ф.И.О."));
            var fullname = fullnameRow!.QuerySelectorAll("span")
                .FirstOrDefault(x => !x.GetAttribute("style").Contains("font-weight: bold")).Text();
            return fullname.Trim().ToUpper();
        }
    }
}