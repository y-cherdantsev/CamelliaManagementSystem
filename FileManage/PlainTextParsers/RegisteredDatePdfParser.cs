using System;
using System.Linq;

//TODO(REFACTOR)
namespace CamelliaManagementSystem.FileManage.PlainTextParsers
{
    public class RegisteredDatePdfParser : PdfPlainTextParser
    {
        /// <inheritdoc />
        public RegisteredDatePdfParser(string path, bool deleteFile = true) : base(path, deleteFile)
        {
            MinimizeReferenceText();
        }

        public string GetHead()
        {
            var innerText = InnerText;

            var result = "";
            if (innerText.IndexOf("<b>Руководитель:</b>") == -1)
                return "Неизвестно";
            innerText = innerText.Substring(innerText.IndexOf("<b>Руководитель:</b>") + 20,
                innerText.Length - innerText.IndexOf("<b>Руководитель:</b>") - 20);
            var elements = innerText.Substring(0, innerText.IndexOf("<b>")).Replace("\r", " ").Replace("\n", " ")
                .Replace(".", " ").Replace(",", " ")
                .Split(' ');
            foreach (var element in elements)
            {
                if (!element.Equals("И") &&
                    !element.Equals("А") &&
                    !element.Equals("О") &&
                    !element.Equals("ООО") &&
                    !element.Equals("ТОО") &&
                    !element.Equals("АО") &&
                    !element.Equals("КОО") &&
                    !element.Equals("ЗАО") &&
                    !element.Equals("КОМПАС") &&
                    !element.Equals("ФИНАНС") &&
                    !element.Equals("С") &&
                    element.All(x => "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯІҢҒҮҰҚӨҺƏӘ".Contains(x)))
                    result += element.Trim() + " ";
            }

            // result = Regex.Replace(result, "[ ]+", "");
            if (result.IndexOf("КОМПАНИЯ") != -1)
                result = result.Substring(0, result.IndexOf("КОМПАНИЯ"));
            while (result.IndexOf("  ") != -1)
                result = result.Replace("  ", " ");
            result = result.Trim();
            return string.IsNullOrEmpty(result) ? null : result;
        }

        public string GetName()
        {
            var innerText = InnerText;
            if (innerText.IndexOf("<b>Наименование:</b>") == -1)
                return "Неизвестно";
            innerText = innerText.Substring(innerText.IndexOf("<b>Наименование:</b>") + 20,
                innerText.Length - innerText.IndexOf("<b>Наименование:</b>") - 20);
            var result = innerText.Substring(0, innerText.IndexOf("<b>")).Replace("\r", " ").Replace("\n", " ").Trim();
            result = result.Trim();
            return string.IsNullOrEmpty(result) ? null : result;
        }

        public string GetPlace()
        {
            var innerText = InnerText;
            if (innerText.IndexOf("<b>Местонахождение:</b>") == -1)
                return "Неизвестно";
            innerText = innerText.Substring(innerText.IndexOf("<b>Местонахождение:</b>") + 23,
                innerText.Length - innerText.IndexOf("<b>Местонахождение:</b>") - 23);
            var result = innerText.Substring(0, innerText.IndexOf("Электрондық")).Replace("\r", "").Replace("\n", " ")
                .Trim();
            result = result.Replace("ақпараттық-анықтамалық қызметі\"", "");
            result = result.Replace("Касательно получения государственных услуг\"", "");
            result = result.Trim();
            return string.IsNullOrEmpty(result) ? null : result;
        }

        public int? CountFounders()
        {
            var innerText = InnerText;
            if (innerText.IndexOf("<b>Количество участников (членов):</b>") == -1)
                return null;
            innerText = innerText.Substring(innerText.IndexOf("<b>Количество участников (членов):</b>") + 38,
                innerText.Length - innerText.IndexOf("<b>Количество участников (членов):</b>") - 38);
            var result = innerText.Substring(0, innerText.IndexOf("<b>")).Replace("\r", " ").Replace("\n", " ").Trim();
            return Convert.ToInt32(result);
        }

        public string GetOccupation()
        {
            var innerText = InnerText;
            if (innerText.IndexOf("<b>Виды деятельности:</b>") == -1)
                return "Неизвестно";
            innerText = innerText.Substring(innerText.IndexOf("<b>Виды деятельности:</b>") + 25,
                innerText.Length - innerText.IndexOf("<b>Виды деятельности:</b>") - 25);
            var result = innerText.Substring(0, innerText.IndexOf("<b>")).Replace("\r", " ").Replace("\n", " ").Trim();
            result = result.Trim();
            return string.IsNullOrEmpty(result) ? null : result;
        }
    }
}