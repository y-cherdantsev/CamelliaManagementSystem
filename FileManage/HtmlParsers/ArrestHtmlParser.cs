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
    /// Parsing of arrest reference and getting of founders from it
    /// </summary>
    public class ArrestHtmlParser : HtmlParser
    {
        /// <inheritdoc />
        public ArrestHtmlParser(string path, bool deleteFile = true) : base(path, deleteFile)
        {
        }


        /// <summary>
        /// Parsing text and gets founders from it
        /// </summary>
        /// <returns>IEnumerable - List of founders</returns>
        /// /// <exception cref="CamelliaNoneDataException">If no information were found</exception>
        public Dictionary<string, string> GetArrest()
        {
            var arrests = new Dictionary<string, string>
            {
                { "founder", null },
                { "other_founder", null }
            };

            var tableRows = HtmlDoc.QuerySelectorAll("td").FirstOrDefault(x => x.GetAttribute("align") == "center")?
                .QuerySelectorAll("tr");

            var founderTableRow = tableRows!.FirstOrDefault(x =>
                x.InnerHtml.Contains("Обременение на долю учредителя юридического лица:"));

            var founderArrest = founderTableRow!.QuerySelectorAll("td").LastOrDefault(x => x.HasAttribute("style"))!
                .QuerySelector("span").Text().Replace("<br>", " ").Trim();
            if (founderArrest != "нет")
                arrests["founder"] = founderArrest;
            var otherFounderTableRow = tableRows!.FirstOrDefault(x =>
                x.InnerHtml.Contains(
                    "Обременение на долю юридического лица, являющегося учредителем в других юридических лицах:"));
            var otherFounderArrest = otherFounderTableRow!.QuerySelectorAll("td").LastOrDefault(x => x.HasAttribute("style"))!
                .QuerySelector("span").Text().Replace("<br>", " ").Trim();
            if (otherFounderArrest != "нет")
                arrests["other_founder"] = otherFounderArrest;

            return arrests;
        }
    }
}