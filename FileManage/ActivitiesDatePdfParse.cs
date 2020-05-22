using System.Collections.Generic;

namespace Camellia_Management_System.FileManage
{
    public class ActivitiesDatePdfParse : PdfParse
    {
        public static IEnumerable<string> GetDates(string innerText)
        {
            innerText = MinimizeReferenceText(innerText);
            innerText = innerText.Substring(innerText.IndexOf("<b>Местонахождение</b>")+4,
                innerText.Length - innerText.IndexOf("<b>Местонахождение</b>")-4);
            innerText = innerText.Substring(innerText.IndexOf("<b>"),
                innerText.Length - innerText.IndexOf("<b>"));
           var elements = innerText.Split("\n");
           for (var i = 0; i < elements.Length-1; i++)
           {
               if (!elements[i].StartsWith("<b>"))
               {
                   elements[i + 1].Replace("</b>", "</b> "+elements[i] + " ");
                   elements[i] = "LOL";
               }
           }
            return null;
        }
    }
}