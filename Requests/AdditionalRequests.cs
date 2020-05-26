using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using Camellia_Management_System.FileManage;
using Camellia_Management_System.JsonObjects;
using Camellia_Management_System.JsonObjects.ResponseObjects;
using Camellia_Management_System.Requests.References;
using Camellia_Management_System.SignManage;

namespace Camellia_Management_System.Requests
{
    /// @author Yevgeniy Cherdantsev
    /// @date 11.03.2020 15:08:36
    /// @version 1.0
    /// <summary>
    /// Additional requests
    /// </summary>
    public static class AdditionalRequests
    {
        /// @author Yevgeniy Cherdantsev
        /// @date 11.03.2020 16:19:56
        /// @version 1.0
        /// <summary>
        /// Returns boolean if the bin is registered in camellia system
        /// </summary>
        /// <param name="camelliaClient">Camellia client</param>
        /// <param name="bin">bin of the company</param>
        /// <returns>bool - true if company registered</returns>
        public static bool IsBinRegistered(CamelliaClient camelliaClient, string bin)
        {
            bin = bin.PadLeft(12, '0');
            var res = camelliaClient.HttpClient
                .GetStringAsync($"https://egov.kz/services/P30.11/rest/gbdul/organizations/{bin}")
                .GetAwaiter()
                .GetResult();
            var organization = JsonSerializer.Deserialize<Organization>(res);
            return organization.status.code != "031" && organization.status.code != "034" &&
                   organization.status.code != "035";
        }

        /// @author Yevgeniy Cherdantsev
        /// @date 14.05.2020 14:59:56
        /// @version 1.0
        /// <summary>
        /// Returns boolean if the iin is registered in camellia system
        /// </summary>
        /// <param name="camelliaClient">Camellia client</param>
        /// <param name="iin">iin of the person</param>
        /// <returns>bool - true if person registered</returns>
        public static bool IsIinRegistered(CamelliaClient camelliaClient, string iin)
        {
            iin = iin.PadLeft(12, '0');
            try
            {
                var res = camelliaClient.HttpClient
                    .GetStringAsync($"https://egov.kz/services/P30.04/rest/gbdfl/persons/{iin}?infotype=short")
                    .GetAwaiter()
                    .GetResult();
                var person = JsonSerializer.Deserialize<Person>(res);
                return true;
            }
            catch (HttpRequestException e)
            {
                if (e.Message.Contains("Response status code does not indicate success: 404 (Not Found)"))
                {
                    return false;
                }

                throw;
            }
        }


        /// <summary>
        /// Gets information about organization
        /// </summary>
        /// <param name="camelliaClient">Camellia CLient</param>
        /// <param name="bin">Bin of the company</param>
        /// <returns>Organization object</returns>
        /// <exception cref="InvalidDataException">If company doesn't presented in the system</exception>
        public static Organization GetOrganizationInfo(CamelliaClient camelliaClient, string bin)
        {
            bin = bin.PadLeft(12, '0');
            var res = camelliaClient.HttpClient
                .GetStringAsync($"https://egov.kz/services/P30.11/rest/gbdul/organizations/{bin}")
                .GetAwaiter()
                .GetResult();
            var organization = JsonSerializer.Deserialize<Organization>(res);
            if (organization.status.code == "031" || organization.status.code == "034")
                throw new InvalidDataException("This company isn't presented in camellia system");
            return organization;
        }

        public static bool IsSignCorrect(Sign sign, string bin, IWebProxy webProxy = null)
        {
            //TODO (enum)
            try
            {
                if (!new FileInfo(sign.FilePath).Exists)
                    throw new FileNotFoundException();
                bin = bin.PadLeft(12, '0');
                var camelliaClient = new CamelliaClient(new FullSign {AuthSign = sign}, webProxy);
                return camelliaClient.UserInformation.uin.PadLeft(12, '0') == bin;
            }
            catch (FileNotFoundException e)
            {
                throw;
            }
            catch (Exception e)
            {
                if (e.Message == "Some error occured while loading the key storage")
                    throw new InvalidDataException("Incorrect password");
                throw new InvalidDataException("Service unavailable");
            }
        }

        public static List<CompanyChange> GetCompanyChanges(CamelliaClient client, string bin, string captchaToken,
            int delay = 1000, bool deleteFiles = true, int timeout = 20000)
        {
            var changes = new List<CompanyChange>();
            var registrationActivitiesReference = new RegistrationActivitiesReference(client);
            var activitiesDates =
                registrationActivitiesReference.GetActivitiesDates(bin, delay: delay, timeout: timeout);
            foreach (var activitiesDate in activitiesDates)
                if (activitiesDate.activity.action != null)
                    foreach (var s in activitiesDate.activity.action)
                        Console.WriteLine(activitiesDate.date + " : " + s);


            var dirName = $"{bin}-{DateTime.UtcNow.Ticks.ToString()}";
            var directoryWithReferences = new DirectoryInfo(dirName);
            directoryWithReferences.Create();
            foreach (var activitiesDate in activitiesDates)
            {
                for (var i = 0; i < 10; i++)
                {
                    try
                    {
                        var tempDateRef = new RegisteredDateReference(client);
                        foreach (var tempReference in tempDateRef.GetReference(bin, activitiesDate.date, captchaToken,
                            delay: delay, timeout: timeout))
                            if (tempReference.language.Contains("ru"))
                                tempReference.SaveFile(directoryWithReferences.FullName, client.HttpClient,
                                    $"{bin}-{activitiesDate.date.Year}-{activitiesDate.date.Month}-{activitiesDate.date.Day}");
                        break;
                    }
                    catch (Exception)
                    {
                    }
                }
            }


            var headChanges = activitiesDates.Where(x =>
                x.activity.action != null && x.activity.action.Contains("Изменение руководителя")).ToList();
            {
                var head =
                    new PdfParser(
                        $"{directoryWithReferences}\\{bin}-{activitiesDates[0].date.Year}-{activitiesDates[0].date.Month}-{activitiesDates[0].date.Day}.pdf",
                        false).GetHead();
                foreach (var dateActivity in headChanges)
                {
                    var newHead = new PdfParser(
                        $"{directoryWithReferences}\\{bin}-{dateActivity.date.Year}-{dateActivity.date.Month}-{dateActivity.date.Day}.pdf",
                        false).GetHead();
                    if (head.Equals(newHead))
                        continue;
                    changes.Add(new CompanyChange
                        {date = dateActivity.date, type = "head", before = head, after = newHead});
                    head = newHead;
                }
            }

            var placeChanges = activitiesDates.Where(x =>
                x.activity.action != null && (x.activity.action.Contains("Изменение местонахождения (с изменением места регистрации)") 
                                              || x.activity.action.Contains("Изменение места нахождения (без изменения места регистрации)"))).ToList();
            {
                var place =
                    new PdfParser(
                        $"{directoryWithReferences}\\{bin}-{activitiesDates[0].date.Year}-{activitiesDates[0].date.Month}-{activitiesDates[0].date.Day}.pdf",
                        false).GetPlace();
                foreach (var dateActivity in placeChanges)
                {
                    var newPlace = new PdfParser(
                        $"{directoryWithReferences}\\{bin}-{dateActivity.date.Year}-{dateActivity.date.Month}-{dateActivity.date.Day}.pdf",
                        false).GetPlace();
                    if (place.Equals(newPlace))
                        continue;
                        changes.Add(new CompanyChange
                        {date = dateActivity.date, type = "place", before = place, after = newPlace});
                    place = newPlace;
                }
            }

            var nameChanges = activitiesDates.Where(x =>
                x.activity.action != null && x.activity.action.Contains("Изменение наименования")).ToList();
            {
                var name =
                    new PdfParser(
                        $"{directoryWithReferences}\\{bin}-{activitiesDates[0].date.Year}-{activitiesDates[0].date.Month}-{activitiesDates[0].date.Day}.pdf",
                        false).GetName();
                foreach (var dateActivity in nameChanges)
                {
                    var newName = new PdfParser(
                        $"{directoryWithReferences}\\{bin}-{dateActivity.date.Year}-{dateActivity.date.Month}-{dateActivity.date.Day}.pdf",
                        false).GetName();
                    if (name.Equals(newName))
                        continue;
                    changes.Add(new CompanyChange
                        {date = dateActivity.date, type = "name", before = name, after = newName});
                    name = newName;
                }
            }

            var occupationChanges = activitiesDates.Where(x =>
                x.activity.action != null && x.activity.action.Contains("Изменение видов деятельности")).ToList();
            {
                var occupation =
                    new PdfParser(
                        $"{directoryWithReferences}\\{bin}-{activitiesDates[0].date.Year}-{activitiesDates[0].date.Month}-{activitiesDates[0].date.Day}.pdf",
                        false).GetOccupation();
                foreach (var dateActivity in nameChanges)
                {
                    var newOccupation = new PdfParser(
                        $"{directoryWithReferences}\\{bin}-{dateActivity.date.Year}-{dateActivity.date.Month}-{dateActivity.date.Day}.pdf",
                        false).GetOccupation();
                    if (occupation.Equals(newOccupation))
                        continue;
                    changes.Add(new CompanyChange
                        {date = dateActivity.date, type = "occupation", before = occupation, after = newOccupation});
                    occupation = newOccupation;
                }
            }
            if (deleteFiles)
                directoryWithReferences.Delete(true);


            return changes.OrderBy(x => x.date).ToList();
        }
    }
}