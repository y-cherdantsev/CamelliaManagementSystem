using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using CamelliaManagementSystem.FileManage.DictionaryParsers;
using CamelliaManagementSystem.JsonObjects;
using CamelliaManagementSystem.Requests.References;
using CamelliaManagementSystem.FileManage.PlainTextParsers;
using CamelliaManagementSystem.JsonObjects.ResponseObjects;

// ReSharper disable CommentTypo
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace CamelliaManagementSystem.Requests
{
    /// @author Yevgeniy Cherdantsev
    /// @date 13.10.2020 15:25:36
    /// <summary>
    /// Managing histories of a companies
    /// </summary>
    public class HistoryManager
    {
        private readonly DirectoryInfo _historiesDirectory;

        /// <summary>
        /// Constructor that creates history folder if it doesn't exist
        /// </summary>
        /// <param name="historiesFolderPath"></param>
        public HistoryManager(string historiesFolderPath)
        {
            _historiesDirectory = new DirectoryInfo(historiesFolderPath);
            if (!_historiesDirectory.Exists)
                _historiesDirectory.Create();
        }

        /// <summary>
        /// Generates company history into local directory if not exist or updates company history into local directory
        /// </summary>
        /// <param name="client">Camellia Client</param>
        /// <param name="bin">Company BIN</param>
        /// <param name="captchaToken">Captcha solver API token</param>
        /// <param name="delay">Delay between requests</param>
        /// <param name="timeout">Timeout of requests</param>
        /// <exception cref="CamelliaNoneDataException">If data about this company hasn't been found</exception>
        public async Task GenerateAndUpdateCompanyHistoryAsync(CamelliaClient client, long bin,
            string captchaToken, int delay = 1000, int timeout = 20000)
        {
            // Finding and creating company history folder
            var companyHistoryDirectory = new DirectoryInfo(Path.Combine(_historiesDirectory.FullName, bin.ToString()));
            if (!companyHistoryDirectory.Exists)
                companyHistoryDirectory.Create();

            // Getting and checking activities reference
            var activitiesReference =
                new FileInfo(Path.Combine(companyHistoryDirectory.FullName, $"{bin.ToString()}.pdf"));
            if (!activitiesReference.Exists ||
                activitiesReference.LastWriteTime.Day != DateTime.Now.Day ||
                activitiesReference.LastWriteTime.Month != DateTime.Now.Month ||
                activitiesReference.LastWriteTime.Year != DateTime.Now.Year)
            {
                try
                {
                    await DownloadActivitiesReferenceAsync(client, bin, companyHistoryDirectory.FullName, delay,
                        timeout);
                }
                catch (CamelliaNoneDataException)
                {
                    // Finding and removing integrity file if exist
                    var integrityFilePath = Path.Combine(companyHistoryDirectory.FullName, "integrity");
                    var integrityFile = new FileInfo(integrityFilePath);
                    if (!integrityFile.Exists)
                        integrityFile.Create();
                    throw;
                }
            }

            // Supplements history data with references that not in folder
            await SupplementCompanyHistoryAsync(client, bin, captchaToken, companyHistoryDirectory.FullName, delay,
                timeout);
        }

        /// <summary>
        /// Downloads or updates activities reference into company history folder
        /// </summary>
        /// <param name="client">Camellia Client</param>
        /// <param name="bin">Company BIN</param>
        /// <param name="companyHistoryDirectoryPath">Company History Directory Path</param>
        /// <param name="delay">Delay between requests</param>
        /// <param name="timeout">Timeout of requests</param>
        private static async Task DownloadActivitiesReferenceAsync(CamelliaClient client, long bin,
            string companyHistoryDirectoryPath, int delay = 1000, int timeout = 20000)
        {
            var registrationActivitiesReference = new RegistrationActivitiesReference(client);
            var resultsForDownload =
                (await registrationActivitiesReference.GetReferenceAsync(bin.ToString(), delay, timeout)).ToList();

            // Finding company history folder
            var historyDirectory = new DirectoryInfo(companyHistoryDirectoryPath);

            // Finding and removing integrity file if exist
            var integrityFilePath = Path.Combine(historyDirectory.FullName, "integrity");
            var integrityFile = new FileInfo(integrityFilePath);
            if (integrityFile.Exists)
                integrityFile.Delete();

            var ruFileActivitiesReference =
                resultsForDownload.FirstOrDefault(x =>
                    x.languageCode == ResultForDownload.Languages.Ru);
            var kzFileActivitiesReference =
                resultsForDownload.FirstOrDefault(x =>
                    x.languageCode == ResultForDownload.Languages.Kz);

            if (ruFileActivitiesReference != null)
                await ruFileActivitiesReference.SaveFileAsync(
                    historyDirectory.FullName,
                    client.HttpClient,
                    bin.ToString()
                );

            if (kzFileActivitiesReference != null)
                await kzFileActivitiesReference.SaveFileAsync(
                    historyDirectory.FullName,
                    client.HttpClient,
                    $"kz{bin.ToString()}"
                );
        }

        /// <summary>
        /// Supplements history folder with all missing references
        /// </summary>
        /// <param name="client">Camellia Client</param>
        /// <param name="bin">Company BIN</param>
        /// <param name="captchaToken">Captcha solver API token</param>
        /// <param name="companyHistoryDirectoryPath">Company History Directory Path</param>
        /// <param name="delay">Delay between requests</param>
        /// <param name="timeout">Timeout of requests</param>
        /// <exception cref="CamelliaFileException"></exception>
        private static async Task SupplementCompanyHistoryAsync(CamelliaClient client, long bin,
            string captchaToken, string companyHistoryDirectoryPath, int delay = 1000, int timeout = 20000)
        {
            // Finding company history folder
            var historyDirectory = new DirectoryInfo(companyHistoryDirectoryPath);

            // Getting and checking activities reference
            var activitiesReference = new FileInfo(Path.Combine(historyDirectory.FullName, $"{bin.ToString()}.pdf"));
            if (!activitiesReference.Exists)
                throw new CamelliaFileException($"No activities reference found for company '{bin}'");

            // Getting dates that are not in history folder yet
            var dates = new RegistrationActivitiesPdfParser(activitiesReference.FullName,
                    deleteFile: false).GetDatesChanges()
                .Select(x => x.date).Distinct().ToList();
            dates = dates.Where(x =>
                historyDirectory.GetFiles()
                    .FirstOrDefault(y => y.Name == $"{x.Day}{x.Month}{x.Year}_{bin.ToString()}.pdf") == null).ToList();

            foreach (var dateTime in dates)
            {
                var registeredDateReference = new RegisteredDateReference(client);
                var resultsForDownload =
                    (await registeredDateReference.GetReferenceAsync(bin.ToString(), captchaToken,
                        dateTime, delay, timeout)).ToList();

                var ruFileDateReference =
                    resultsForDownload.FirstOrDefault(
                        x => x.languageCode == ResultForDownload.Languages.Ru);
                var kzFileDateReference =
                    resultsForDownload.FirstOrDefault(
                        x => x.languageCode == ResultForDownload.Languages.Kz);

                if (ruFileDateReference != null)
                    await ruFileDateReference.SaveFileAsync(
                        historyDirectory.FullName,
                        client.HttpClient,
                        $"{dateTime.Day}{dateTime.Month}{dateTime.Year}_{bin.ToString()}"
                    );

                if (kzFileDateReference != null)
                    await kzFileDateReference.SaveFileAsync(
                        historyDirectory.FullName,
                        client.HttpClient,
                        $"kz{dateTime.Day}{dateTime.Month}{dateTime.Year}_{bin.ToString()}"
                    );
            }

            // Creating integrity file
            var integrityFilePath = Path.Combine(historyDirectory.FullName, "integrity");
            var integrityFile = new FileInfo(integrityFilePath);
            if (!integrityFile.Exists)
                integrityFile.Create();
        }

        /// <summary>
        /// Parses local files and generates history list
        /// </summary>
        /// <param name="bin">BIN of the company</param>
        /// <param name="checkIntegrity">If the function should check integrity or not</param>
        /// <returns></returns>
        /// <exception cref="CamelliaFileException">If some reference or integrity hasn't been found</exception>
        public IEnumerable<CompanyChange> GetLocalCompanyHistory(long bin, bool checkIntegrity = false)
        {
            var changes = new List<CompanyChange>();

            // Finding company history folder
            var historyDirectory = new DirectoryInfo(Path.Combine(_historiesDirectory.FullName, bin.ToString()));
            if (!historyDirectory.Exists)
                throw new CamelliaFileException($"No activities reference found for company '{bin}'");

            var integrityFile = historyDirectory.GetFiles().FirstOrDefault(x => x.Name == "integrity");
            if (checkIntegrity && integrityFile == null)
                throw new CamelliaFileException($"Integrity not provided for company '{bin}'");

            var registrationActivitiesReference =
                new RegistrationActivitiesPdfParser(Path.Combine(historyDirectory.FullName, $"{bin}.pdf"), false);
            var activitiesDates =
                registrationActivitiesReference.GetDatesChanges().ToList();

            var dates = activitiesDates.Select(x => x.date).Where(x =>
                    historyDirectory.GetFiles().Any(y => y.Name == $"{x.Day}{x.Month}{x.Year}_{bin.ToString()}.pdf"))
                .OrderBy(x => x.Date)
                .ToList();

            // Head changes
            var head =
                new RegisteredDatePdfParser(
                    Path.Combine(historyDirectory.FullName,
                        $"{dates.First().Day}{dates.First().Month}{dates.First().Year}_{bin}.pdf"),
                    false).GetHead();
            if (!string.IsNullOrEmpty(head))
                changes.Add(new CompanyChange
                    {Date = dates.First(), Type = "fullname_director", Before = null, After = head});
            foreach (var date in dates)
            {
                var newHead = new RegisteredDatePdfParser(
                    Path.Combine(historyDirectory.FullName, $"{date.Day}{date.Month}{date.Year}_{bin}.pdf"),
                    false).GetHead();
                if (string.IsNullOrEmpty(newHead) || head == newHead)
                    continue;
                changes.Add(new CompanyChange
                    {Date = date, Type = "fullname_director", Before = head, After = newHead});
                head = newHead;
            }

            // Place changes
            var place =
                new RegisteredDatePdfParser(
                    Path.Combine(historyDirectory.FullName,
                        $"{dates.First().Day}{dates.First().Month}{dates.First().Year}_{bin}.pdf"),
                    false).GetPlace();
            if (!string.IsNullOrEmpty(place))
                changes.Add(new CompanyChange
                    {Date = dates.First(), Type = "legal_address", Before = null, After = place});
            foreach (var date in dates)
            {
                var newPlace = new RegisteredDatePdfParser(
                    Path.Combine(historyDirectory.FullName, $"{date.Day}{date.Month}{date.Year}_{bin}.pdf"),
                    false).GetPlace();
                if (string.IsNullOrEmpty(newPlace) || place == newPlace)
                    continue;
                changes.Add(new CompanyChange
                    {Date = date, Type = "legal_address", Before = place, After = newPlace});
                place = newPlace;
            }

            // Name changes
            var name =
                new RegisteredDatePdfParser(
                    Path.Combine(historyDirectory.FullName,
                        $"{dates.First().Day}{dates.First().Month}{dates.First().Year}_{bin}.pdf"),
                    false).GetName();
            if (!string.IsNullOrEmpty(name))
                changes.Add(new CompanyChange
                    {Date = dates.First(), Type = "name_ru", Before = null, After = name});
            foreach (var date in dates)
            {
                var newName = new RegisteredDatePdfParser(
                    Path.Combine(historyDirectory.FullName, $"{date.Day}{date.Month}{date.Year}_{bin}.pdf"),
                    false).GetName();
                if (string.IsNullOrEmpty(newName) || name == newName)
                    continue;
                changes.Add(new CompanyChange
                    {Date = date, Type = "name_ru", Before = name, After = newName});
                name = newName;
            }

            // Occupation changes
            var occupation =
                new RegisteredDatePdfParser(
                    Path.Combine(historyDirectory.FullName,
                        $"{dates.First().Day}{dates.First().Month}{dates.First().Year}_{bin}.pdf"),
                    false).GetOccupation();
            if (!string.IsNullOrEmpty(occupation))
                changes.Add(new CompanyChange
                    {Date = dates.First(), Type = "occupation", Before = null, After = occupation});
            foreach (var date in dates)
            {
                var newOccupation = new RegisteredDatePdfParser(
                    Path.Combine(historyDirectory.FullName, $"{date.Day}{date.Month}{date.Year}_{bin}.pdf"),
                    false).GetOccupation();
                if (string.IsNullOrEmpty(newOccupation) || occupation == newOccupation)
                    continue;
                changes.Add(new CompanyChange
                    {Date = date, Type = "occupation", Before = occupation, After = newOccupation});
                occupation = newOccupation;
            }

            // Founders changes
            var startFounders =
                new RegisteredDatePdfDictionaryParser(
                    Path.Combine(historyDirectory.FullName,$"{dates.First().Day}{dates.First().Month}{dates.First().Year}_{bin}.pdf")).GetFounders();
            
            var previousFounders = new List<string>();
            if (startFounders != null)
            {
                changes.AddRange(startFounders.Select(startFounder => new CompanyChange
                    {Date = dates.First(), Type = "founder", Before = null, After = startFounder}));
                previousFounders = startFounders;
            }
            
            foreach (var date in dates)
            {
                var tempFounders =
                    new RegisteredDatePdfDictionaryParser(
                        Path.Combine(historyDirectory.FullName,$"{date.Day}{date.Month}{date.Year}_{bin}.pdf")).GetFounders();
                // Old removed
                changes.AddRange(from previousFounder in previousFounders where tempFounders.All(x => x != previousFounder) select new CompanyChange {Date = date, Type = "founder", Before = previousFounder, After = null});
                
                //New added
                changes.AddRange(from tempFounder in tempFounders where previousFounders.All(x => x != tempFounder) select new CompanyChange {Date = date, Type = "founder", Before = null, After = tempFounder});
                previousFounders = tempFounders;
            }
            
            return changes;
        }
    }
}