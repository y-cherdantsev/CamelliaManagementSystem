﻿using System.Collections.Generic;

namespace Camellia_Management_System.FileManage
{
    /// @author Yevgeniy Cherdantsev
    /// @date 14.03.2020 14:49:19
    /// @version 1.0
    /// <summary>
    /// INPUT
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
            return childCompanies.Count > 0 ? childCompanies : null;
        }
    }
}