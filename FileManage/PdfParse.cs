using System;

// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable StringIndexOfIsCultureSpecific.1

namespace CamelliaManagementSystem.FileManage
{
    /// @author Yevgeniy Cherdantsev
    /// @date 14.03.2020 14:50:59
    /// <summary>
    /// Parsing pdf references
    /// </summary>
    public abstract class PdfParse
    {
        /// <summary>
        /// Minimization of a text and removing of unnecessary symbols before using it
        /// </summary>
        /// <param name="text">Text that should be minimized</param>
        /// <returns>string - minimized text</returns>
        /// todo(Not usable for all references)
        protected static string MinimizeReferenceText(string text)
        {
            text = text.Trim();
            var to = "<b>Наименование";
            try
            {
                var position = text.ToLower().IndexOf(to.ToLower(), StringComparison.Ordinal);
                text = text.Substring(position,
                    text.Length - position);
            }
            catch (Exception)
            {
                // ignored
            }

            while (true)
            {
                try
                {
                    const string from = "Осы құжат «Электрондық";
                    to = "<hr>";
                    if (!text.ToLower().Contains(from.ToLower()))
                        break;
                    var positionFrom = text.ToLower().IndexOf(from.ToLower());
                    var positionTo = text.ToLower().IndexOf(to.ToLower());
                    var text1 = text.Substring(0, positionFrom);
                    var text2 = text.Substring(positionTo + to.Length,
                        text.Length - positionTo - to.Length);
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
                    const string from = "<a name=";
                    to = "Дата получения<br>";
                    if (!text.ToLower().Contains(from.ToLower()))
                        break;
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
            text = text.Replace("&#34;", "\"");
            text = text.Replace("&quot", "\"");
            text = text.Replace("&#34", "\"");
            text = text.Replace("</BODY>", string.Empty);
            text = text.Replace("</body>", string.Empty);
            text = text.Replace("</HTML>", string.Empty);
            text = text.Replace("</html>", string.Empty);
            text = text.Replace("<br>", string.Empty);

            return text;
        }
    }
}