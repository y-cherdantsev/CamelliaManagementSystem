using System.Linq;
using AngleSharp.Dom;
using System.Collections.Generic;
using AngleSharp;
using CamelliaManagementSystem.Requests;

// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable UnusedType.Global
// ReSharper disable StringIndexOfIsCultureSpecific.1

namespace CamelliaManagementSystem.FileManage.HtmlParsers
{
    /// @author Yevgeniy Cherdantsev
    /// @date 11.02.2021 02:05:22
    /// <summary>
    /// Parsing of registration reference and getting of founders from it
    /// </summary>
    public class RegistrationHtmlParser : HtmlParser
    {
        /// <inheritdoc />
        public RegistrationHtmlParser(string path, bool deleteFile = true) : base(path, deleteFile)
        {
        }


        /// <summary>
        /// Parsing text and gets founders from it
        /// </summary>
        /// <returns>IEnumerable - List of founders</returns>
        /// /// <exception cref="CamelliaNoneDataException">If no information were found</exception>
        public IEnumerable<string> GetFounders()
        {
            var founders = new List<string>();
            if (HtmlDoc.ToHtml().ToLower().Contains("регистрации филиала"))
                return founders;

            const string from = "Учредители (участники):";
            if (HtmlDoc.ToHtml().ToLower().IndexOf(from.ToLower()) == -1)
                throw new CamelliaNoneDataException("No information were found in the reference");

            var tableRows = HtmlDoc.QuerySelectorAll("td").FirstOrDefault(x => x.GetAttribute("align") == "center")?
                .QuerySelectorAll("tr");
            var foundersRowFound = false;
            // ReSharper disable once PossibleNullReferenceException
            foreach (var row in tableRows)
            {
                if (row.InnerHtml.Contains("Учредители (участники):"))
                {
                    foundersRowFound = true;
                    continue;
                }

                if (!foundersRowFound) continue;

                if (row.QuerySelectorAll("td").Any(x => x.InnerHtml.Contains("font-weight: bold")))
                    break;
                var founder = row.QuerySelectorAll("td").FirstOrDefault(x => x.InnerHtml.Trim() != string.Empty)
                    ?.QuerySelector("span").Text();
                if (founder != null)
                    founders.Add(founder);
            }

            var result = Normalize(founders);
            return result;
        }

        /// <summary>
        /// Removing of unnecessary element and symbols in founders list
        /// </summary>
        /// <param name="founders">List of founders</param>
        /// <returns>IEnumerable - normalized list</returns>
        private static IEnumerable<string> Normalize(List<string> founders)
        {
            founders.RemoveAll(x =>
                x.Replace(" ", string.Empty).Equals("-") || x.Replace(" ", string.Empty).Equals(""));
            for (var i = 0; i < founders.Count; i++)
            {
                founders[i] = founders[i].Replace("\r", string.Empty).Replace("&amp;", "&");
                if (founders[i].EndsWith("/"))
                    founders[i] = founders[i].Replace("/", string.Empty).Trim();
            }

            founders = founders.Distinct().ToList();
            return founders;
        }
    }
}