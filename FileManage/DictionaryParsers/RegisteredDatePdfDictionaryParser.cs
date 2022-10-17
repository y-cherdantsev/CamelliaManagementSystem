using System.Linq;
using System.Collections.Generic;

namespace CamelliaManagementSystem.FileManage.DictionaryParsers
{
    public class RegisteredDatePdfDictionaryParser : PdfDictionaryParser
    {
        public RegisteredDatePdfDictionaryParser(string path, bool deleteFile = true) : base(path, deleteFile)
        {
        }

        public List<string> GetFounders()
        {
            var founders = new List<string>();

            var key = Dictionary.Keys.FirstOrDefault(x => x.Contains("Учредители"));

            if (key == null)
                return founders;

            var dictionaryList = Dictionary[key];
            if (dictionaryList == null)
                return founders;

            // todo(bin: 20240004974)
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

            for (var i = 0; i < founders.Count; i++)
            {
                founders[i] = founders[i].Trim('.', '-', ' '); // 60840014782 'АЛИЯ', 'АЛИЯ - -', 'АЛИЯ . -'
                while (founders[i].Contains("  ")) // Solves problem when 'НИКОЛАЙ НИКОЛАЕВИЧ' and 'НИКОЛАЙ  НИКОЛАЕВИЧ' are different objects
                    founders[i] = founders[i].Replace("  ", " ");
                while (founders[i].Contains("- ")) // "Научно-  производственное объединение
                    founders[i] = founders[i].Replace("- ", "-");
                while (founders[i].Contains(" -")) // "Научно-  производственное объединение
                    founders[i] = founders[i].Replace(" -", "-");
            }
            // for (var i = 0; i < founders.Count; i++)
            // founders[i] = founders[i].Trim('.').Trim('уведомление о ').Trim();

            return founders;
        }
    }
}