using System.Linq;
using System.Collections.Generic;

namespace CamelliaManagementSystem.FileManage.DictionaryParsers
{
    public class RegisteredDatePdfDictionaryParser : PdfDictionaryParser
    {
        public RegisteredDatePdfDictionaryParser(string filePath) : base(filePath)
        {
        }

        public List<string> GetFounders()
        {
            var founders = new List<string>();

            var dictionaryList = Dictionary["Учредители (участники, члены)"];
            if (dictionaryList == null)
                return founders;

            var flag = false;
            foreach (var element in dictionaryList)
            {
                if (element.Replace(" ", "").Replace("-", "").All(char.IsUpper) &&
                    element.Contains(" ") && !element.Contains("\"") &&
                    !element.ToLower().Replace(" ", "").Equals("обществосограниченной"))
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

                    var elementTemp = element.Replace(" ", string.Empty);
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


            return founders;
        }
    }
}