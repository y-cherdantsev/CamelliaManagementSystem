using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using CamelliaManagementSystem.JsonObjects;
using CamelliaManagementSystem.Requests.References;
using CamelliaManagementSystem.FileManage.PlainTextParsers;
using CamelliaManagementSystem.FileManage.DictionaryParsers;
using CamelliaManagementSystem.FileManage.HtmlParsers;
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
                new FileInfo(Path.Combine(companyHistoryDirectory.FullName, $"{bin.ToString()}.html"));
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
                    // Finding and creating integrity file for the company if not exist (If data wasn't found)
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
            var activitiesReference = new FileInfo(Path.Combine(historyDirectory.FullName, $"{bin.ToString()}.html"));
            if (!activitiesReference.Exists)
                throw new CamelliaFileException($"No activities reference found for company '{bin}'");

            // Getting dates that are not in history folder yet
            var dates = new RegistrationActivitiesHtmlParser(activitiesReference.FullName,
                    deleteFile: false).GetDatesChanges()
                .Select(x => x.date).Distinct().ToList();
            dates = dates.Where(x =>
                historyDirectory.GetFiles()
                    .FirstOrDefault(y =>
                        y.Name == $"{x.Day}{x.Month}{x.Year}_{bin.ToString()}.html" ||
                        y.Name == $"{x.Day}{x.Month}{x.Year}_{bin.ToString()}.pdf") == null).ToList();

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
                new RegistrationActivitiesHtmlParser(Path.Combine(historyDirectory.FullName, $"{bin}.html"), false);
            var activitiesDates =
                registrationActivitiesReference.GetDatesChanges().ToList();

            // Load reorganization changes if exist
            // ReSharper disable once StringLiteralTypo
            var reorganizationChanges =
                activitiesDates.Where(x => x.activity.type.ToLower() == "реорганизация").ToList();

            reorganizationChanges.ForEach(x => changes.Add(new CompanyChange
            {
                Date = x.date,
                Type = "reorganization"
            }));

            var dates = activitiesDates.Select(x => x.date).Where(x =>
                    historyDirectory.GetFiles().Any(y => y.Name == $"{x.Day}{x.Month}{x.Year}_{bin.ToString()}.html"
                                                         || y.Name == $"{x.Day}{x.Month}{x.Year}_{bin.ToString()}.pdf"))
                .OrderBy(x => x.Date)
                .Distinct()
                .ToList();

            //todo(fck)
            var referenceFiles = new Dictionary<DateTime, FileInfo>();
            foreach (var date in dates)
            {
                var dateFile = new FileInfo(Path.Combine(historyDirectory.FullName,
                    $"{date.Day}{date.Month}{date.Year}_{bin}.html"));
                if (!dateFile.Exists)
                    dateFile = new FileInfo(Path.Combine(historyDirectory.FullName,
                        $"{date.Day}{date.Month}{date.Year}_{bin}.pdf"));
                referenceFiles.Add(date, dateFile);
            }

            // Head changes
            var headFirstFilePath = referenceFiles.First().Value.FullName;
            var headFileExtension = headFirstFilePath.Split(".").Last();
            var head = headFileExtension switch
            {
                "html" => new RegisteredDateHtmlParser(headFirstFilePath, deleteFile: false)
                    .GetHead(),
                "pdf" => new RegisteredDatePdfTextParser(headFirstFilePath, deleteFile: false)
                    .GetHead(),
                _ => throw new ArgumentOutOfRangeException()
            };
            if (!string.IsNullOrEmpty(head))
                changes.Add(new CompanyChange
                    {Date = referenceFiles.First().Key, Type = "fullname_director", Before = null, After = head});

            foreach (var (key, value) in referenceFiles)
            {
                headFileExtension = value.FullName.Split(".").Last();
                var newHead = headFileExtension switch
                {
                    "html" => new RegisteredDateHtmlParser(value.FullName, deleteFile: false)
                        .GetHead(),
                    "pdf" => new RegisteredDatePdfTextParser(value.FullName, deleteFile: false)
                        .GetHead(),
                    _ => head
                };
                if (string.IsNullOrEmpty(newHead) || head == newHead)
                    continue;
                changes.Add(new CompanyChange
                    {Date = key, Type = "fullname_director", Before = head, After = newHead});
                head = newHead;
            }

            // Place changes
            var placeFirstFilePath = referenceFiles.First().Value.FullName;
            var placeFileExtension = placeFirstFilePath.Split(".").Last();
            var place = placeFileExtension switch
            {
                "html" => new RegisteredDateHtmlParser(placeFirstFilePath, deleteFile: false)
                    .GetPlace(),
                "pdf" => new RegisteredDatePdfTextParser(placeFirstFilePath, deleteFile: false)
                    .GetPlace(),
                _ => throw new ArgumentOutOfRangeException()
            };
            if (!string.IsNullOrEmpty(place))
                changes.Add(new CompanyChange
                    {Date = referenceFiles.First().Key, Type = "legal_address", Before = null, After = place});

            foreach (var (key, value) in referenceFiles)
            {
                placeFileExtension = value.FullName.Split(".").Last();
                var newPlace = placeFileExtension switch
                {
                    "html" => new RegisteredDateHtmlParser(value.FullName, deleteFile: false)
                        .GetPlace(),
                    "pdf" => new RegisteredDatePdfTextParser(value.FullName, deleteFile: false)
                        .GetPlace(),
                    _ => place
                };
                if (string.IsNullOrEmpty(newPlace) || place == newPlace)
                    continue;
                changes.Add(new CompanyChange
                    {Date = key, Type = "legal_address", Before = place, After = newPlace});
                place = newPlace;
            }
            

            // Name changes
            var nameFirstFilePath = referenceFiles.First().Value.FullName;
            var nameFileExtension = nameFirstFilePath.Split(".").Last();
            var name = nameFileExtension switch
            {
                "html" => new RegisteredDateHtmlParser(nameFirstFilePath, deleteFile: false)
                    .GetName(),
                "pdf" => new RegisteredDatePdfTextParser(nameFirstFilePath, deleteFile: false)
                    .GetName(),
                _ => throw new ArgumentOutOfRangeException()
            };
            if (!string.IsNullOrEmpty(name))
                changes.Add(new CompanyChange
                    {Date = referenceFiles.First().Key, Type = "name_ru", Before = null, After = name});

            foreach (var (key, value) in referenceFiles)
            {
                nameFileExtension = value.FullName.Split(".").Last();
                var newName = nameFileExtension switch
                {
                    "html" => new RegisteredDateHtmlParser(value.FullName, deleteFile: false)
                        .GetName(),
                    "pdf" => new RegisteredDatePdfTextParser(value.FullName, deleteFile: false)
                        .GetName(),
                    _ => name
                };
                if (string.IsNullOrEmpty(newName) || name == newName)
                    continue;
                changes.Add(new CompanyChange
                    {Date = key, Type = "name_ru", Before = name, After = newName});
                name = newName;
            }

            // Occupation changes
            var occupationFirstFilePath = referenceFiles.First().Value.FullName;
            var occupationFileExtension = occupationFirstFilePath.Split(".").Last();
            var occupation = occupationFileExtension switch
            {
                "html" => new RegisteredDateHtmlParser(occupationFirstFilePath, deleteFile: false)
                    .GetOccupation(),
                "pdf" => new RegisteredDatePdfTextParser(occupationFirstFilePath, deleteFile: false)
                    .GetOccupation(),
                _ => throw new ArgumentOutOfRangeException()
            };
            if (!string.IsNullOrEmpty(occupation))
                changes.Add(new CompanyChange
                    {Date = referenceFiles.First().Key, Type = "occupation", Before = null, After = occupation});

            foreach (var (key, value) in referenceFiles)
            {
                occupationFileExtension = value.FullName.Split(".").Last();
                var newOccupation = occupationFileExtension switch
                {
                    "html" => new RegisteredDateHtmlParser(value.FullName, deleteFile: false)
                        .GetOccupation(),
                    "pdf" => new RegisteredDatePdfTextParser(value.FullName, deleteFile: false)
                        .GetOccupation(),
                    _ => occupation
                };
                if (string.IsNullOrEmpty(newOccupation) || occupation == newOccupation)
                    continue;
                changes.Add(new CompanyChange
                    {Date = key, Type = "occupation", Before = occupation, After = newOccupation});
                occupation = newOccupation;
            }
            //todo(Участники) (Судя по всему убрали учредителей)
            // return changes;
            // Founders changes
            var foundersFirstFilePath = referenceFiles.First().Value.FullName;
            var foundersFileExtension = foundersFirstFilePath.Split(".").Last();
            var startFounders =
                foundersFileExtension switch
                {
                    "html" => new RegisteredDateHtmlParser(nameFirstFilePath, deleteFile: false)
                        .GetFounders(),
                    "pdf" => new RegisteredDatePdfDictionaryParser(nameFirstFilePath, deleteFile: false)
                        .GetFounders(),
                    _ => throw new ArgumentOutOfRangeException()
                };
            startFounders = startFounders.ConvertAll(d => d.ToLower());
            // var startFounders =
            //     new RegisteredDateHtmlParser(
            //         Path.Combine(historyDirectory.FullName,
            //             $"{dates.First().Day}{dates.First().Month}{dates.First().Year}_{bin}.html")).GetFounders();

            var previousFounders = new List<string>();
            if (startFounders != null)
            {
                changes.AddRange(startFounders.Select(startFounder => new CompanyChange
                    {Date = dates.First(), Type = "founder", Before = null, After = startFounder}));
                previousFounders = startFounders;
            }

            foreach (var (key, value) in referenceFiles)
            {
                foundersFileExtension = value.FullName.Split(".").Last();
                var tempFounders = 
                    foundersFileExtension switch
                {
                    "html" => new RegisteredDateHtmlParser(value.FullName, deleteFile: false)
                        .GetFounders(),
                    "pdf" => new RegisteredDatePdfDictionaryParser(value.FullName, deleteFile: false)
                        .GetFounders(),
                    _ => previousFounders
                };
                tempFounders = tempFounders.ConvertAll(d => d.ToLower());
                // Old removed
                changes.AddRange(from previousFounder in previousFounders
                    where tempFounders.All(x => x != previousFounder)
                    select new CompanyChange {Date = key, Type = "founder", Before = previousFounder, After = null});

                //New added
                changes.AddRange(from tempFounder in tempFounders
                    where previousFounders.All(x => x != tempFounder)
                    select new CompanyChange {Date = key, Type = "founder", Before = null, After = tempFounder});
                previousFounders = tempFounders;
            }

            return changes;
        }
    }
}