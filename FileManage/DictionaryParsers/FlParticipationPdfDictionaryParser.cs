using System.Collections.Generic;
using System.Linq;

namespace CamelliaManagementSystem.FileManage.DictionaryParsers
{
    public class FlParticipationPdfDictionaryParser : PdfDictionaryParser
    {
        public FlParticipationPdfDictionaryParser(string filePath, bool deleteFile = false) : base(filePath, deleteFile)
        {
        }

        public List<string> GetWhereIsHead()
        {
            var companies = new List<string>();
            var numberOfCompanies = Dictionary.Keys.Count(x => x.StartsWith("БИН"));
            if (numberOfCompanies == 0)
                return companies;
            var currentBin = Dictionary["БИН"][0];
            var currentHead = string.Join("", Dictionary["Первый руководитель"]).Replace(" ", string.Empty).ToUpper();
            for (var i = 0; i < numberOfCompanies; i++)
            {
                if (i != 0)
                {
                    currentBin = Dictionary[$"БИН_{i}"][0];
                    currentHead = string.Join("", Dictionary[$"Первый руководитель_{i}"]).Replace(" ", string.Empty).ToUpper();
                }

                if (currentHead.Contains(Dictionary["Ф.И.О."][0].Replace(" ", string.Empty).ToUpper()))
                    companies.Add(currentBin);
            }

            return companies;
        }
    }
}