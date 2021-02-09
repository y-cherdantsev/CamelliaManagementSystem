using System;
using System.Linq;
using System.Collections.Generic;
using CamelliaManagementSystem.Requests;

// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable UnusedType.Global
// ReSharper disable StringIndexOfIsCultureSpecific.1

namespace CamelliaManagementSystem.FileManage.PlainTextParsers
{
    /// @author Yevgeniy Cherdantsev
    /// @date 07.03.2020 17:30:46
    /// <summary>
    /// Parsing of registration reference and gettion of founders from it using plain text method
    /// </summary>
    public class RegistrationPdfTextParser : PdfPlainTextParser
    {
        /// <inheritdoc />
        public RegistrationPdfTextParser(string path, bool deleteFile = true) : base(path, deleteFile)
        {
        }


        /// <summary>
        /// Parsing text and gets founders from it
        /// </summary>
        /// <returns>IEnumerable - List of founders</returns>
        /// /// <exception cref="CamelliaNoneDataException">If no information were found</exception>
        public IEnumerable<string> GetFounders()
        {
            var textFromPdf = InnerText;
            var founders = new List<string>();
            if (textFromPdf.ToLower().Contains("регистрации филиала"))
                return founders;
            
            textFromPdf = MinimizeReferenceText(textFromPdf);

            const string from = "<b>Учредители (участники):</b>";
            const string to = "<b>";
            if (textFromPdf.ToLower().IndexOf(from.ToLower()) == -1)
                throw new CamelliaNoneDataException("No information were found in the reference");
            var fromPosition = textFromPdf.ToLower().IndexOf(from.ToLower(), StringComparison.Ordinal);
            textFromPdf = textFromPdf.Substring(fromPosition + from.Length,
                textFromPdf.Length - fromPosition - from.Length);
            textFromPdf = textFromPdf.Substring(0, textFromPdf.ToLower().IndexOf(to, StringComparison.Ordinal)).Trim();
            var elements = textFromPdf.Split(new char[] {'\n'});

            var flag = false;
            foreach (var element in elements)
            {
                if (element.Replace("\r", string.Empty).Replace("\n", string.Empty).Replace(" ", string.Empty)
                        .All(char.IsUpper) &&
                    element.Contains(" ") && !element.Contains("\"") && !element.ToLower().Replace(" ", "")
                        .Replace("\r", "").Equals("обществосограниченной"))
                {
                    founders.Add(element);
                    flag = false;
                }
                else
                {
                    if (element.ToLower().Trim().Replace(" ", "")
                        .StartsWith("товариществосограниченнойответственностью"))
                    {
                        flag = true;
                        founders.Add(element);
                        continue;
                    }

                    if (element.ToLower().Trim().Replace(" ", "")
                        .StartsWith("открытоеакционерноеобщество"))
                    {
                        flag = true;
                        founders.Add(element);
                        continue;
                    }
                    
                    if (element.ToLower().Trim().Replace(" ", "")
                        .StartsWith("ао\""))
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
                    }
                    else if (element.ToLower().Trim().StartsWith("республиканского"))
                    {
                        flag = true;
                        var founder = founders.Last();
                        founders.Remove(founders.Last());
                        founders.Add(founder + " " + element);
                    }
                    else if (element.ToLower().Trim().StartsWith("\"") && founders.Count > 0)
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
                        elementTemp.EndsWith("\"") && elementTemp.Trim().Replace(" ", string.Empty).ToLower() !=
                        "товариществосограниченнойответственностью\"")

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
                throw new CamelliaNoneDataException("No information were found in the reference");
            return result;
        }

        /// <summary>
        /// Removing of unnecessary element and symbols in founders list
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
            return founders.Count > 0
                ? founders
                : throw new CamelliaNoneDataException("Nothing has been found in reference");
        }
    }
}