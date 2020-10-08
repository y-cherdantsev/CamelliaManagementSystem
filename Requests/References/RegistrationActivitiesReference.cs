﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Camellia_Management_System;
using Camellia_Management_System.Requests;
using CamelliaManagementSystem.FileManage;

//TODO(REFACTOR)
namespace CamelliaManagementSystem.Requests.References
{
    /// <summary>
    /// 
    /// </summary>
    public class RegistrationActivitiesReference : BiinRequest
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="camelliaClient"></param>
        public RegistrationActivitiesReference(CamelliaClient camelliaClient) : base(camelliaClient)
        {
        }
        
        public async Task<IEnumerable<ActivitiesDatePdfParse.DateActivity>> GetActivitiesDatesAsync(string bin, int delay = 1000, bool deleteFile = true, int timeout = 20000)
        {
            var reference = await GetReferenceAsync(bin, delay, timeout);

            var temp = reference.First(x => x.language.Contains("ru"));
            if (temp != null)
                return new PdfParser(temp.SaveFile("./", CamelliaClient.HttpClient), deleteFile).GetActivitiesDates();
            return null;
        }

        protected override string RequestLink()
        {
            return "https://egov.kz/services/P30.05/";
        }

        protected override BiinType TypeOfBiin()
        {
            return BiinType.BIN;
        }
    }
}