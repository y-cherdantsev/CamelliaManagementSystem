using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using CamelliaManagementSystem.Requests.References;

namespace CamelliaManagementSystem.FileManage.HtmlParsers
{
    public class RegistrationActivitiesHtmlParser : HtmlParser
    {
        public RegistrationActivitiesHtmlParser(string path, bool deleteFile = true) : base(path, deleteFile)
        {
        }

        /// <summary>
        /// Gets dates from registration activities reference
        /// </summary>
        /// <returns>List of DateActivity objects</returns>
        public IEnumerable<RegistrationActivitiesReference.DateActivity> GetDatesChanges()
        {
            var result = new List<RegistrationActivitiesReference.DateActivity>();
            var rows = HtmlDoc.QuerySelectorAll("tr");
            var rowsWithBold = rows.Where(x
                => x.QuerySelectorAll("span")
                    .Any(y => y.GetAttribute("style").Contains("font-weight: bold")));
            var rowsWithDate = rowsWithBold.Where(x =>
                Regex.IsMatch(
                    x.QuerySelectorAll("span")
                        .FirstOrDefault(y => y.GetAttribute("style").Contains("font-weight: bold")).Text(),
                    @".{0,}[0-3]{1}[0-9]{1}\.(0|1)[0-9]\.(1|2)(0|9)[0-9]{2}.{0,}"));
            
 
            foreach (var element in rowsWithDate)
            {
                var dateString = element.QuerySelectorAll("span").FirstOrDefault(x => x.GetAttribute("style").Contains("font-weight: bold")).Text().Trim();
                var dateParts = dateString.Split('.');
                var date = new DateTime(int.Parse(dateParts[2]), int.Parse(dateParts[1]), int.Parse(dateParts[0]));
                var activityString = element.QuerySelectorAll("span").FirstOrDefault(x => !x.GetAttribute("style").Contains("font-weight: bold")).Text().Trim();


                var typeTo = activityString.IndexOf("(");
                if (typeTo == -1)
                    typeTo = activityString.Length;


                var activity = new RegistrationActivitiesReference.Activity
                {
                    type = activityString.Substring(0, typeTo).Trim(),
                    action = new List<string>()
                };

                try
                {
                    if (activityString.IndexOf("(", StringComparison.Ordinal) == -1)
                        throw new Exception();

                    activityString = activityString.Substring(activityString.IndexOf("(") + 1,
                        activityString.Length - activityString.IndexOf("(") - 2);
                    activity.action = activityString.Trim().Split(';').ToList();
                    for (var i = 0; i < activity.action.Count; i++)
                        activity.action[i] = activity.action[i].Trim();
                }
                catch (Exception)
                {
                    // ignored
                }

                result.Add(new RegistrationActivitiesReference.DateActivity(date, activity));
            }

            return Normalize(result);
        }

        /// <summary>
        /// Removes redundant information from the list
        /// </summary>
        /// <param name="dateActivities">List of DateActivity</param>
        /// <returns>Clear DateActivity list with normalized view</returns>
        private static IEnumerable<RegistrationActivitiesReference.DateActivity> Normalize(IEnumerable<RegistrationActivitiesReference.DateActivity> dateActivities)
        {
            //Removing same values
            var result = dateActivities.ToList();
            for (var i = 0; i < result.Count - 1; i++)
            for (var j = i + 1; j < result.Count; j++)
                if (result[i].date == result[j].date && result[i].activity == result[j].activity)
                    result.RemoveAt(j);

            return result.Where(x => x != null).OrderBy(x => x.date).ToList();
        }

 
    }
}