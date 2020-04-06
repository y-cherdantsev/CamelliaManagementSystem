using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Camellia_Management_System.FileManage
{
    /// @author Yevgeniy Cherdantsev
    /// @date 07.03.2020 17:30:46
    /// @version 1.0
    /// <summary>
    /// Parsing of registration reference and gettion of founders from it
    /// </summary>
    public class FoundersPdfParse : PdfParse
    {


        /// @author Yevgeniy Cherdantsev
        /// @date 10.03.2020 10:31:22
        /// @version 1.0
        /// <summary>
        /// Parsing text and gets founders from it
        /// </summary>
        /// <param name="innerText">Text of a pdf file</param>
        /// <returns>IEnumerable - List of founders</returns>
        public static IEnumerable<string> GetFounders(string innerText)
        {
            var founders = new List<string>();
            var textFromPdf = innerText;
            if (textFromPdf.ToLower().Contains("регистрации филиала"))
                return founders;
            var minimized = MinimizeReferenceText(textFromPdf);

            var from = "Учредители (участники):</b>";
            var to = "<b>";
            var fromPosition = minimized.ToLower().IndexOf(from.ToLower(), StringComparison.Ordinal);
            minimized = minimized.Substring(fromPosition + from.Length, minimized.Length - fromPosition - from.Length);
            minimized = minimized.Substring(0, minimized.ToLower().IndexOf(to, StringComparison.Ordinal)).Trim();
            var elements = minimized.Split("\n");

            var flag = false;
            foreach (var element in elements)
            {
                if (element.Replace("\r", string.Empty).Replace("\n", string.Empty).Replace(" ", string.Empty).All(char.IsUpper) &&
                    element.Contains(" "))
                {
                    founders.Add(element);
                    flag = false;
                }
                else
                {
                    if (flag)
                    {
                        var founder = founders.Last();
                        founders.Remove(founders.Last());
                        founders.Add(founder + " " + element);
                    }
                    else
                    {
                        flag = true;
                        founders.Add(element);
                    }

                    if (element.Replace("\r", string.Empty).Replace("\n", string.Empty).Replace(" ", string.Empty).EndsWith("\""))
                    {
                        flag = false;
                    }

                    if (element.Replace("\r", string.Empty).Replace("\n", string.Empty).Replace(" ", string.Empty).EndsWith("»"))
                    {
                        flag = false;
                    }

                    if (!element.Contains(" "))
                    {
                        flag = false;
                    }
                }
            }

            var result = Normalize(founders);
            if (result == null)
                throw new InvalidDataException("No information were found in the reference");
            return result;
        }




        



        /// @author Yevgeniy Cherdantsev
        /// @date 10.03.2020 10:33:42
        /// @version 1.0
        /// <summary>
        /// Removing of unneccesary element and symbols in founders list
        /// </summary>
        /// <param name="founders">List of founders</param>
        /// <returns>IEnumerable - normalized list</returns>
        private static IEnumerable<string> Normalize(List<string> founders)
        {
            founders.RemoveAll(x => x.Replace(" ", string.Empty).Equals("-"));
            for (var i = 0; i < founders.Count; i++)
                founders[i] = founders[i].Replace("\r", string.Empty).Replace("&amp;", "&");
            founders = founders.Distinct().ToList();
            return founders.Count > 0 ? founders : null;
        }
    }
}