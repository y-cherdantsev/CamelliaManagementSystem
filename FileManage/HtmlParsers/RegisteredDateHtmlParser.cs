using System;
using System.Collections.Generic;
using System.Linq;
using AngleSharp;
using AngleSharp.Dom;
using CamelliaManagementSystem.FileManage.PlainTextParsers;

// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable StringIndexOfIsCultureSpecific.1

namespace CamelliaManagementSystem.FileManage.HtmlParsers
{
    /// @author Yevgeniy Cherdantsev
    /// @date 14.05.2020 14:49:19
    /// <summary>
    /// Get data about company on the given date using plain text method
    /// </summary>
    public class RegisteredDateHtmlParser : HtmlParser
    {
        /// <inheritdoc />
        public RegisteredDateHtmlParser(string path, bool deleteFile = true) : base(path, deleteFile)
        {
        }

        /// <summary>
        /// Get head of a company
        /// </summary>
        /// <returns>string - Head</returns>
        public string GetHead()
        {
            if (HtmlDoc.ToHtml().IndexOf("Руководитель:") == -1)
                return "Неизвестно";
            var elements = GetRowValue("Руководитель:")
                .Split(' ');

            var result = string.Empty;
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var element in elements)
            {
                if (!element.Equals("И") &&
                    !element.Equals("А") &&
                    !element.Equals("О") &&
                    !element.Equals("ООО") &&
                    !element.Equals("ТОО") &&
                    !element.Equals("АО") &&
                    !element.Equals("КОО") &&
                    !element.Equals("ЗАО") &&
                    !element.Equals("КОМПАС") &&
                    !element.Equals("ФИНАНС") &&
                    !element.Equals("С") &&
                    element.All(x => "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯІҢҒҮҰҚӨҺƏӘ".Contains(x)))
                    result += element.Trim() + " ";
            }

            // result = Regex.Replace(result, "[ ]+", "");
            if (result.IndexOf("КОМПАНИЯ") != -1)
                result = result.Substring(0, result.IndexOf("КОМПАНИЯ"));
            while (result.IndexOf("  ") != -1)
                result = result.Replace("  ", " ");
            result = result.Trim();
            return string.IsNullOrEmpty(result) ? null : result;
        }

        /// <summary>
        /// Get name of a company
        /// </summary>
        /// <returns>string - Name</returns>
        public string GetName()
        {
            if (HtmlDoc.ToHtml().IndexOf("Наименование:") == -1)
                return "Неизвестно";
            var result = GetRowValue("Наименование:");
            return string.IsNullOrEmpty(result) ? null : result;
        }

        /// <summary>
        /// Get place where company located
        /// </summary>
        /// <returns>string - Place</returns>
        public string GetPlace()
        {
            if (HtmlDoc.ToHtml().IndexOf("Местонахождение:") == -1)
                return "Неизвестно";
            var result = GetRowValue("Местонахождение:");
            return string.IsNullOrEmpty(result) ? null : result;
        }

        /// <summary>
        /// Get number of founders
        /// </summary>
        /// <returns>int - Number of founders</returns>
        public int? CountFounders()
        {
            if (HtmlDoc.ToHtml().IndexOf("Количество участников (членов):") == -1)
                return null;
            var count = GetRowValue("Количество участников (членов):");
            return Convert.ToInt32(count);
        }

        /// <summary>
        /// Get occupation of a company
        /// </summary>
        /// <returns>string - Occupation</returns>
        public string GetOccupation()
        {
            if (HtmlDoc.ToHtml().IndexOf("Виды деятельности:") == -1)
                return "Неизвестно";
            var result = GetRowValue("Виды деятельности:");
            return string.IsNullOrEmpty(result) ? null : result;
        }

        /// <summary>
        /// Takes value of a given row
        /// </summary>
        /// <param name="rowTitle">Title of a row</param>
        /// <returns>string - row value</returns>
        private string GetRowValue(string rowTitle)
        {
            var row = HtmlDoc.QuerySelectorAll("td").FirstOrDefault(x => x.GetAttribute("align") == "center")
                ?.QuerySelectorAll("tr")
                .FirstOrDefault(x => x.InnerHtml.Contains(rowTitle));

            var result = row?.QuerySelectorAll("span")
                .FirstOrDefault(x => !x.GetAttribute("style").Contains("font-weight: bold")).Text();
            return result;
        }

        public List<string> GetFounders()
        {      
            if (HtmlDoc.ToHtml().IndexOf("Учредители (участники, члены):") == -1)
                    return new List<string>();
            var row = HtmlDoc.QuerySelectorAll("td").FirstOrDefault(x => x.GetAttribute("align") == "center")
                ?.QuerySelectorAll("tr")
                .FirstOrDefault(x => x.InnerHtml.Contains("Учредители (участники, члены):"));

            var result = row?.QuerySelectorAll("span")
                .Where(x => !x.GetAttribute("style").Contains("font-weight: bold")).Select(x => x.InnerHtml.Trim(' ').Trim(';').Split(" БИН ")[0].Trim(','));
            return result!.ToList();
        }
    }
}