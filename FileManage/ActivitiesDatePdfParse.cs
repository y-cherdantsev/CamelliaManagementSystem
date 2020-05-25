using System;
using System.Collections.Generic;
using System.Linq;
using AngleSharp.Text;

namespace Camellia_Management_System.FileManage
{
    public class ActivitiesDatePdfParse : PdfParse
    {
        public static List<DateActivity> GetDatesChanges(string innerText)
        {
            var result = new List<DateActivity>();
            innerText = MinimizeReferenceText(innerText);
            innerText = innerText.Substring(innerText.IndexOf("<b>Местонахождение</b>") + 4,
                innerText.Length - innerText.IndexOf("<b>Местонахождение</b>") - 4);
            innerText = innerText.Substring(innerText.IndexOf("<b>"),
                innerText.Length - innerText.IndexOf("<b>"));
            var elements = innerText.Split("\n");
            for (var i = 0; i < elements.Length; i++)
            {
                elements[i] = elements[i].Replace("\r", string.Empty);
                if (elements[i].StartsWith("<b>") || elements[i].Equals("")) continue;
                if (elements[i].Trim().EndsWith(")"))
                {
                    elements[i - 1] = elements[i - 1] + " " + elements[i];
                }
                else
                {
                    elements[i + 1] = elements[i + 1].Replace("</b>", "</b> " + elements[i] + " ");
                }

                elements[i] = "";
            }

            elements = elements.Where(x => !x.Equals("")).ToArray();
            foreach (var element in elements)
            {
                var dateString = element.Substring(element.IndexOf("<b>") + 3, element.IndexOf("</b>") - 3).Trim();
                var dateParts = dateString.Split('.');
                var date = new DateTime(int.Parse(dateParts[2]), int.Parse(dateParts[1]), int.Parse(dateParts[0]));
                var activityString = element.Substring(element.IndexOf("</b>") + 4,
                    element.Length - element.IndexOf("</b>") - 4).Trim();
                

                var typeTo = activityString.IndexOf("(");
                if (typeTo == -1)
                    typeTo = activityString.Length;

                
                var activity = new Activity
                {
                    type = activityString.Substring(0, typeTo).Trim()
                };
                
                try
                {
                    if (activityString.IndexOf("(", StringComparison.Ordinal) == -1)
                    {
                        throw new Exception();
                    }
                    activityString = activityString.Substring(activityString.IndexOf("(") + 1,
                        activityString.Length - activityString.IndexOf("(") - 2);
                    activity.action = activityString.Trim().Split(';').ToList();
                    for (var i = 0; i < activity.action.Count; i++)
                        activity.action[i] = activity.action[i].Trim();
                    
                }
                catch (Exception)
                {
                   
                }

                result.Add(new DateActivity(date, activity));
            }

            return Normalize(result);
        }

        private static List<DateActivity> Normalize(List<DateActivity> dateActivities)
        {
            //Removing same values
            var result = dateActivities.ToList();
            for (var i = 0; i < result.Count - 1; i++)
            for (var j = i + 1; j < result.Count; j++)
                if (result[i].date == result[j].date && result[i].activity == result[j].activity)
                    result.RemoveAt(j);

            return result.Where(x => x != null).OrderBy(x => x.date).ToList();
        }

        public class DateActivity
        {
            internal DateActivity(DateTime date, Activity activity)
            {
                this.date = date;
                this.activity = activity;
            }

            public DateTime date { get; set; }
            public Activity activity { get; set; }
        }
        public class Activity
        {
            public Activity()
            {
            }

            public string type;
            public List<string> action { get; set; }
        }
    }
}