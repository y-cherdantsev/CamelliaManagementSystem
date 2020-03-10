using System;
using System.Collections.Generic;
using System.Linq;

namespace Camellia_Management_System.FileManage
{
    /// @author Yevgeniy Cherdantsev
    /// @date 07.03.2020 17:30:46
    /// @version 1.0
    /// <summary>
    /// INPUT
    /// </summary>
    public class FoundersPdfParse
    {
        public static List<string> GetFounders(string innerText)
        {
            var founders = new List<string>();
            var textFromPdf = innerText;
            if (textFromPdf.ToLower().Contains("регистрации филиала"))
                return founders;
            var minimized = MinimizeRegistrationReferenceText(textFromPdf);

            var from = "Учредители (участники):</b>";
            var to = "<b>";
            var fromPosition = minimized.ToLower().IndexOf(from.ToLower(), StringComparison.Ordinal);
            minimized = minimized.Substring(fromPosition + from.Length, minimized.Length - fromPosition - from.Length);
            minimized = minimized.Substring(0, minimized.ToLower().IndexOf(to, StringComparison.Ordinal)).Trim();
            var elements = minimized.Split("\n");

            var flag = false;
            foreach (var element in elements)
            {
                if (element.Replace("\r", "").Replace("\n", "").Replace(" ", "").All(char.IsUpper) &&
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

                    if (element.Replace("\r\n", "").Replace(" ", "").EndsWith("\""))
                    {
                        flag = false;
                    }

                    if (element.Replace("\r\n", "").Replace(" ", "").EndsWith("»"))
                    {
                        flag = false;
                    }

                    if (!element.Contains(" "))
                    {
                        flag = false;
                    }
                }
            }

            return Normalize(founders);
        }


        private static string MinimizeRegistrationReferenceText(string text)

        {
            text = text.Trim();
            var to = "<b>Наименование";
            var position = text.ToLower().IndexOf(to.ToLower(), StringComparison.Ordinal);
            text = text.Substring(position,
                text.Length - position);
            while (true)
            {
                try
                {
                    var from = "Осы құжат «Электрондық";
                    to = "<hr>";
                    var positionFrom = text.ToLower().IndexOf(from.ToLower());
                    var positionTo = text.ToLower().IndexOf(to.ToLower());
                    var text1 = text.Substring(0, text.ToLower().IndexOf(from.ToLower()));
                    var text2 = text.Substring(text.ToLower().IndexOf(to.ToLower()) + to.Length,
                        text.Length - text.ToLower().IndexOf(to.ToLower()) - to.Length);
                    text = text1.Trim() + "\n" + text2.Trim();
                }
                catch (Exception)
                {
                    break;
                }
            }

            while (true)
            {
                try
                {
                    var from = "<a name=";
                    to = "Дата получения<br>";
                    var text1 = text.Substring(0, text.ToLower().IndexOf(from.ToLower()));
                    var text2 = text.Substring(text.ToLower().IndexOf(to.ToLower()) + to.Length,
                        text.Length - text.ToLower().IndexOf(to.ToLower()) - to.Length);
                    text = text1.Trim() + "\n" + text2.Trim();
                }
                catch (Exception)
                {
                    break;
                }
            }

            text = text.Replace("&quot;", "\"");
            text = text.Replace("&quot", "\"");
            text = text.Replace("</BODY>", "");
            text = text.Replace("</HTML>", "");
            text = text.Replace("<br>", "");

            return text;
        }

        private static List<string> Normalize(List<string> founders)
        {
            founders.RemoveAll(x => x.Replace(" ", "").Equals("-"));
            for (var i = 0; i < founders.Count; i++)
                founders[i] = founders[i].Replace("\r", "");
            return founders.Count > 0 ? founders : null;
        }
    }
}