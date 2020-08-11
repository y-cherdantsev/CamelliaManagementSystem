using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Camellia_Management_System.JsonObjects;
using Camellia_Management_System.JsonObjects.ResponseObjects;
using Camellia_Management_System.Requests.References;
using CamelliaManagementSystem.FileManage;
using CamelliaManagementSystem.Requests.References;
using CamelliaManagementSystem.SignManage;

//TODO(REFACTOR)
namespace CamelliaManagementSystem.Requests
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
        /// <summary>
        /// Returns true if the given bin registered in camellia system
        /// Client should be logged in to work properly
        /// </summary>
        /// <param name="camelliaClient">Camellia client</param>
        /// <param name="bin">bin of the company</param>
        /// <param name="numberOfTries">Number of requests if some errors has been occured</param>
        /// <param name="delay">Time in millis between requests</param>
        /// <returns>bool - true if company registered</returns>
        public static bool IsBinRegistered(CamelliaClient camelliaClient, string bin,
            int numberOfTries = 15, int delay = 500)
        {
            //Padding BIN to 12 symbols
            bin = bin.PadLeft(12, '0');

            //Codes send from system that means that bin is not registered
            string[] knownErrorCodes = {"031", "034", "035"};


            for (var i = 0; i < numberOfTries; i++)
            {
                var response = camelliaClient.HttpClient
                    .GetAsync($"https://egov.kz/services/P30.11/rest/gbdul/organizations/{bin}").Result;

                // If got 302 'Moved Temporarily' StatusCode then check that user is logged in. If user is logged in then repeat request;
                if (response.StatusCode == HttpStatusCode.Redirect)
                {
                    if (camelliaClient.IsLogged() != true)
                        throw new AuthenticationException(
                            $"'{camelliaClient.folderName}' isn't authorized to the camellia system");

                    Thread.Sleep(delay);
                    continue;
                }

                var result = "";

                if (response.Content != null)
                    result = response.Content.ReadAsStringAsync().Result;
                else if (response.ReasonPhrase != null)
                    throw new HttpRequestException(
                        $"StatusCode:'{response.StatusCode}';\nReasonPhrase:'{response.ReasonPhrase}';\nContent is null;");


                if (result.Contains("Number of connections exceeded"))
                    throw new HttpRequestException(
                        $"StatusCode:'{response.StatusCode}';\nReasonPhrase:'{response.ReasonPhrase}';\nNumber of connections exceeded. Please, try later;");

                try
                {
                    var organization = JsonSerializer.Deserialize<Organization>(result);
                    return !knownErrorCodes.Contains(organization.status.code);
                }
                catch (JsonException e)
                {
                    throw new JsonException(
                        $"Json error while deserializing next string '{result}' of the '{bin}' company to organization object",
                        e);
                }
            }

            throw new Exception($"{numberOfTries} tries with delay={delay} exceeded");
        }

        /// @author Yevgeniy Cherdantsev
        /// @date 14.05.2020 14:59:56
        /// @version 1.0
        /// <summary>
        /// Returns boolean if the iin is registered in camellia system
        /// </summary>
        /// <param name="camelliaClient">Camellia client</param>
        /// <param name="iin">iin of the person</param>
        /// <param name="numberOfTries">Number of requests if some errors has been occured</param>
        /// <param name="delay">Time in millis between requests</param>
        /// <returns>bool - true if person registered</returns>
        public static bool IsIinRegistered(CamelliaClient camelliaClient, string iin,
            int numberOfTries = 15, int delay = 500)
        {
            //Padding IIN to 12 symbols
            iin = iin.PadLeft(12, '0');

            /*
            *
            * Returns UserInformation.Info.Person in json
            * Can be deserialized var person = JsonSerializer.Deserialize<UserInformation.Info.Person>(response.Content.ReadAsStringAsync());
            * 
            */
            for (var i = 0; i < numberOfTries; i++)
            {
                var response = camelliaClient.HttpClient.GetAsync(
                    $"https://egov.kz/services/P30.04/rest/gbdfl/persons/{iin}?infotype=short").Result;
                switch (response.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        return false;

                    // If got 302 'Moved Temporarily' StatusCode then check that user is logged in. If user is logged in then repeat request;
                    case HttpStatusCode.Redirect when camelliaClient.IsLogged() != true:
                        throw new AuthenticationException(
                            $"'{camelliaClient.folderName}' isn't authorized to the camellia system");
                    case HttpStatusCode.Redirect:
                        Thread.Sleep(delay);
                        continue;

                    default:
                        return true;
                }
            }

            throw new Exception($"{numberOfTries} tries with delay={delay} exceeded");
        }


        /// <summary>
        /// Checks if there is no organization with presented name
        /// </summary>
        /// <param name="camelliaClient">Camellia CLient</param>
        /// <param name="name">Name of the company</param>
        /// <param name="numberOfTries">Number of requests if some errors has been occured</param>
        /// <param name="delay">Time in millis between requests</param>
        /// <returns>True or false</returns>
        public static bool IsCompanyNameFree(CamelliaClient camelliaClient, string name,
            int numberOfTries = 15, int delay = 500)
        {
            var content = new StringContent($"{{\"nameKz\":\"{name}\"}}", Encoding.UTF8, "application/json");

            string[] knownCorrectCodes = {"005"};

            for (var i = 0; i < numberOfTries; i++)
            {
                var response = camelliaClient.HttpClient
                    .PostAsync(new Uri("https://egov.kz/services/Com03/rest/app/checkNames"), content).Result;

                // If got 302 'Moved Temporarily' StatusCode then check that user is logged in. If user is logged in then repeat request;
                if (response.StatusCode == HttpStatusCode.Redirect)
                {
                    if (camelliaClient.IsLogged() != true)
                        throw new AuthenticationException(
                            $"'{camelliaClient.folderName}' isn't authorized to the camellia system");

                    Thread.Sleep(delay);
                    continue;
                }

                var result = "";

                if (response.Content != null)
                    result = response.Content.ReadAsStringAsync().Result;
                else if (response.ReasonPhrase != null)
                    throw new HttpRequestException(
                        $"StatusCode:'{response.StatusCode}';\nReasonPhrase:'{response.ReasonPhrase}';\nContent is null;");

                try
                {
                    var companyNameStatus = JsonSerializer.Deserialize<CompanyNameStatus>(result);
                    return knownCorrectCodes.Contains(companyNameStatus.code);
                }
                catch (JsonException e)
                {
                    throw new JsonException(
                        $"Json error while deserializing next string '{result}' of the '{name}' company to companyNameStatus object",
                        e);
                }
            }

            throw new Exception($"{numberOfTries} tries with delay={delay} exceeded");
        }

        /// <summary>
        /// Checks if the biin equals to the biin from sign
        /// </summary>
        /// <param name="sign">Sign</param>
        /// <param name="biin">BIIN</param>
        /// <param name="webProxy">Proxy if needed</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException">If the sign hasn't been found</exception>
        /// <exception cref="InvalidDataException">If the password is incorrect</exception>
        /// <exception cref="ExternalException">If service unavailable</exception>
        public static async Task<bool> IsSignCorrect(Sign sign, string biin, IWebProxy webProxy = null)
        {
            //Padding BIIN to 12 symbols
            biin = biin.PadLeft(12, '0');

            //TODO (enum)
            try
            {
                if (!new FileInfo(sign.filePath).Exists)
                    throw new FileNotFoundException();
                var camelliaClient = new CamelliaClient(new FullSign(sign, null), webProxy);
                camelliaClient.Login();
                return camelliaClient.UserInformation.uin.PadLeft(12, '0') == biin;
            }
            catch (SignXmlTokens.KalkanCryptException e)
            {
                throw new InvalidDataException("Incorrect password", e);
            }
            catch (Exception)
            {
                throw new ExternalException("Service unavailable");
            }
        }

        /// <summary>
        /// Gets changes of the company
        /// </summary>
        /// <param name="client">Camellia Client</param>
        /// <param name="bin">BIN of the company</param>
        /// <param name="captchaToken">Captcha token</param>
        /// <param name="fromDate">First date that will be used for parsing</param>
        /// <param name="delay">Delay for the waiting of references</param>
        /// <param name="deleteFiles">If the references should be deleted after parsing</param>
        /// <param name="timeout">Timeout of waiting of the sign</param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException">If camellia system doesn't contain such information</exception>
        /// <exception cref="ExternalException">If service unavaliable</exception>
        public static List<CompanyChange> GetCompanyChanges(CamelliaClient client, string bin,
            string captchaToken, DateTime? fromDate = null,
            int delay = 1000, bool deleteFiles = true, int timeout = 20000)
        {
            var changes = new List<CompanyChange>();
            var registrationActivitiesReference = new RegistrationActivitiesReference(client);
            var activitiesDates =
                registrationActivitiesReference.GetActivitiesDates(bin, delay: delay, timeout: timeout)
                    .OrderBy(x => x.date).ToList();

            // if (fromDate != null)
                // activitiesDates = activitiesDates.Where(x => x.date >= fromDate).ToList();

            // foreach (var activitiesDate in activitiesDates)
            // if (activitiesDate.activity.action != null)
            // foreach (var s in activitiesDate.activity.action)
            // Console.WriteLine(activitiesDate.date + " : " + s);


            var dirName = $"{bin}-{DateTime.UtcNow.Ticks.ToString()}";
            var directoryWithReferences = new DirectoryInfo(dirName);
            directoryWithReferences.Create();
            activitiesDates = activitiesDates.Where(x =>
                x.date == activitiesDates[0].date
                || x.activity.action.Contains("Изменение руководителя")
                || x.activity.action.Contains("Изменение местонахождения")
                || x.activity.action.Contains("Изменение состава участников")
                || x.activity.action.Contains("Изменение состава учредителей (членов, участников)")
                || x.activity.type.Contains("Первичная регистрация")
                || x.activity.type.Contains("Перерегистрация")
                || x.activity.type.Contains("Внесение изменений")
                || x.activity.action.Contains("Изменение места нахождения")
                || x.activity.action.Contains("Изменение наименования")
                || x.activity.action.Contains("Изменение видов деятельности")
                || x.date == activitiesDates.Last().date
            ).ToList();
            foreach (var activitiesDate in activitiesDates)
            {
                var tempDateRef = new RegisteredDateReference(client);
                foreach (var tempReference in tempDateRef.GetReference(bin, activitiesDate.date,
                        captchaToken,
                        delay: delay, timeout: timeout))
                    // if (activitiesDate.date == activitiesDates[0].date
                    //     || activitiesDate.activity.action.Contains("Изменение руководителя")
                    //     || activitiesDate.activity.action.Contains(
                    //         "Изменение местонахождения")
                    //     || activitiesDate.activity.action.Contains(
                    //         "Изменение состава участников")
                    //     || activitiesDate.activity.action.Contains(
                    //         "Изменение состава учредителей (членов, участников)")
                    //     || activitiesDate.activity.type.Contains(
                    //         "Первичная регистрация")
                    //     || activitiesDate.activity.action.Contains(
                    //         "Изменение места нахождения")
                    //     || activitiesDate.activity.action.Contains("Изменение наименования")
                    //     || activitiesDate.activity.action.Contains("Изменение видов деятельности")
                    //     || activitiesDate.date == activitiesDates.Last().date
                    // )
                    if (tempReference.language.Contains("ru"))
                        tempReference.SaveFile(directoryWithReferences.FullName, client.HttpClient,
                            $"{bin}-{activitiesDate.date.Year}-{activitiesDate.date.Month}-{activitiesDate.date.Day}");
            }

            var headChanges = activitiesDates.Where(x =>
                x.activity.action != null && (x.activity.action.Contains("Изменение руководителя") ||
                                              x.activity.action.Contains("Изменение состава участников") ||
                                              x.activity.action.Contains(
                                                  "Изменение состава учредителей (членов, участников)") ||
                                              x.activity.type.Contains("Перерегистрация") ||
                                              x.activity.type.Contains("Внесение изменений")
                )).ToList();
            {
                var head =
                    new PdfParser(
                        $"{directoryWithReferences}\\{bin}-{activitiesDates[0].date.Year}-{activitiesDates[0].date.Month}-{activitiesDates[0].date.Day}.pdf",
                        false).GetHead();
                // if (fromDate == null)
                    changes.Add(new CompanyChange
                        {Date = activitiesDates[0].date, Type = "fullname_director", Before = null, After = head});
                foreach (var dateActivity in headChanges)
                {
                    var newHead = new PdfParser(
                        $"{directoryWithReferences}\\{bin}-{dateActivity.date.Year}-{dateActivity.date.Month}-{dateActivity.date.Day}.pdf",
                        false).GetHead();
                    if (head.Equals(newHead))
                        continue;
                    changes.Add(new CompanyChange
                        {Date = dateActivity.date, Type = "fullname_director", Before = head, After = newHead});
                    head = newHead;
                }

                //TODO(Справки не существует)
                // var lastHead =
                //     new PdfParser(
                //         $"{directoryWithReferences}\\{bin}-{activitiesDates.Last().date.Year}-{activitiesDates.Last().date.Month}-{activitiesDates.Last().date.Day}.pdf",
                //         false).GetHead();
                // if (!head.Equals(lastHead))
                //     changes.Add(new CompanyChange
                //     {
                //         date = activitiesDates.Last().date, type = "fullname_director", before = head, after = lastHead
                //     });
            }

            var placeChanges = activitiesDates.Where(x =>
                    x.activity.action != null &&
                    (x.activity.action.Contains("Изменение местонахождения (с изменением места регистрации)")
                     || x.activity.action.Contains("Изменение места нахождения (без изменения места регистрации)")))
                .ToList();
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
                    {
                        Date = dateActivity.date, Type = "legal_address", Before = place, After = newPlace
                    });
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
                        {Date = dateActivity.date, Type = "name_ru", Before = name, After = newName});
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
                    {
                        Date = dateActivity.date, Type = "occupation", Before = occupation,
                        After = newOccupation
                    });
                    occupation = newOccupation;
                }
            }
            if (deleteFiles)
                directoryWithReferences.Delete(true);


            return changes.OrderBy(x => x.Date).ToList();
        }
    }
}