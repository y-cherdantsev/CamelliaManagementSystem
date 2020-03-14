using System;

namespace Camellia_Management_System.FileManage
{

    /// @author Yevgeniy Cherdantsev
    /// @date 14.03.2020 14:50:59
    /// @version 1.0
    /// <summary>
    /// INPUT
    /// </summary>
    /// <code>
    /// 
    /// </code>


    public abstract class PdfParse
    {
        
        /// @author Yevgeniy Cherdantsev
        /// @date 10.03.2020 10:32:28
        /// @version 1.0
        /// <summary>
        /// Minimization of a text and removing of unneccesary symbols before using it
        /// </summary>
        /// <param name="text"></param>
        /// <returns>string - minimized text</returns>
        protected static string MinimizeReferenceText(string text)

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
            text = text.Replace("</BODY>", string.Empty);
            text = text.Replace("</HTML>", string.Empty);
            text = text.Replace("<br>", string.Empty);

            return text;
        }
    }
}