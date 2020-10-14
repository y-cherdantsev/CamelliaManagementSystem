using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

//TODO(REFACTOR)
namespace CamelliaManagementSystem.FileManage
{
    /// @author Yevgeniy Cherdantsev
    /// @date 07.03.2020 17:30:46
    /// <summary>
    /// Parsing of registration reference and gettion of founders from it
    /// </summary>
    public class FoundersPdfParse : PdfParse
    {
        /// <summary>
        /// Parsing text and gets founders from it
        /// </summary>
        /// <param name="innerText">Text of a pdf file</param>
        /// <returns>IEnumerable - List of founders</returns>
        /// /// <exception cref="InvalidDataException">If no information were found</exception>
        public static IEnumerable<string> GetFounders(string innerText)
        {
            var founders = new List<string>();
            var textFromPdf = innerText;
            if (textFromPdf.ToLower().Contains("регистрации филиала"))
                return founders;
            var minimized = MinimizeReferenceText(textFromPdf);

            var from = "<b>Учредители (участники):</b>";
            var to = "<b>";
            if(minimized.ToLower().IndexOf(from.ToLower())== -1)
                throw new InvalidDataException("No information were found in the reference");
            var fromPosition = minimized.ToLower().IndexOf(from.ToLower(), StringComparison.Ordinal);
            minimized = minimized.Substring(fromPosition + from.Length, minimized.Length - fromPosition - from.Length);
            minimized = minimized.Substring(0, minimized.ToLower().IndexOf(to, StringComparison.Ordinal)).Trim();
            var elements = minimized.Split(new char[] {'\n'});

            var flag = false;
            foreach (var element in elements)
            {
                
                if (element.Replace("\r", string.Empty).Replace("\n", string.Empty).Replace(" ", string.Empty)
                        .All(char.IsUpper) &&
                    element.Contains(" ") && !element.Contains("\"") && !element.ToLower().Replace(" ", "").Replace("\r", "").Equals("обществосограниченной"))
                {
                    founders.Add(element);
                    flag = false;
                }
                else
                {
                    if (element.ToLower().Trim().Replace(" ", "").StartsWith("товариществосограниченнойответственностью"))
                    {
                        flag = true;
                        founders.Add(element);
                        continue;
                    }
                    if (element.ToLower().Trim().Replace(" ", "").StartsWith("обществосограниченной"))
                    {
                        flag = true;
                        founders.Add(element);
                        continue;
                    }
                    if (element.ToLower().Trim().Replace(" ", "").StartsWith("обществосограниченной"))
                    {
                        flag = true;
                        founders.Add(element);
                        continue;
                    }
                    if (element.ToLower().Trim().Replace(" ", "").StartsWith("акционерноеобщество"))
                    {
                        flag = true;
                        founders.Add(element);
                        continue;
                    }
                    if (flag)
                    {
                        var founder = founders.Last();
                        founders.Remove(founders.Last());
                        founders.Add(founder + " " + element);
                    }else if (element.ToLower().Trim().StartsWith("республиканского"))
                    {
                        flag = true;
                        var founder = founders.Last();
                        founders.Remove(founders.Last());
                        founders.Add(founder + " " + element);
                    }else if (element.ToLower().Trim().StartsWith("\""))
                    {
                        flag = true;
                        var founder = founders.Last();
                        founders.Remove(founders.Last());
                        founders.Add(founder + " " + element);
                    }
                    else
                    {
                        flag = true;
                        founders.Add(element);
                    }

                    var elementTemp = element.Replace("\r", string.Empty).Replace("\n", string.Empty)
                        .Replace(" ", string.Empty);
                    if (founders.Last().Contains("/") && element.Contains("/"))
                    {
                        elementTemp = element.Replace("/", " ").Trim();
                    }

                    if (
                        elementTemp.EndsWith("\"") && elementTemp.Trim().Replace(" ", string.Empty).ToLower()!="товариществосограниченнойответственностью\"")

                        flag = false;


                    if (elementTemp.EndsWith("”"))

                        flag = false;


                    if (elementTemp.EndsWith("LLC"))

                        flag = false;


                    if (elementTemp.EndsWith("»"))

                        flag = false;


                    if (elementTemp.EndsWith(")"))

                        flag = false;


                    if (elementTemp.EndsWith("."))

                        flag = false;


                    // if (elementTemp.ToUpper().EndsWith("CORPORATION"))
                        // flag = false;


                    if (!element.Contains(" "))

                        flag = false;
                }
            }

            var result = Normalize(founders);
            if (result == null)
                throw new InvalidDataException("No information were found in the reference");
            return result;
        }

        /// <summary>
        /// Removing of unneccesary element and symbols in founders list
        /// </summary>
        /// <param name="founders">List of founders</param>
        /// <returns>IEnumerable - normalized list</returns>
        private static IEnumerable<string> Normalize(List<string> founders)
        {
            founders.RemoveAll(x => x.Replace(" ", string.Empty).Equals("-"));
            for (var i = 0; i < founders.Count; i++)
            {
                founders[i] = founders[i].Replace("\r", string.Empty).Replace("&amp;", "&");
                if (founders[i].EndsWith("/"))
                    founders[i] = founders[i].Replace("/", string.Empty).Trim();
            }

            founders = founders.Distinct().ToList();
            return founders.Count > 0 ? founders : null;
        }
    }
}