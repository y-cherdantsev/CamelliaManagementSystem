using System;
using System.Collections.Generic;

// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

#pragma warning disable 1591
namespace CamelliaManagementSystem.JsonObjects.ResponseObjects
{
    /// @author Yevgeniy Cherdantsev
    /// @date 18.02.2020 12:18:07
    /// <summary>
    /// ReadinessStatus json object
    /// </summary>
    public class ReadinessStatus : IDisposable
    {
        public string status { get; set; }
        public string bpmProcessStatus { get; set; }
        public bool completed { get; set; }
        public string declarantIdentificationNumber { get; set; }
        public string declarantName { get; set; }
        public string applicationCode { get; set; }
        public string nameRu { get; set; }
        public string nameKz { get; set; }
        public long appCreationDate { get; set; }
        public long updatedDate { get; set; }
        public List<ResultForDownload> resultsForDownload { get; set; } = new List<ResultForDownload>();
        public Eds eds = new Eds();
        public StatusGo statusGo = new StatusGo();
        public string operatorIin { get; set; }
        public string operatorName { get; set; }
        public string recipientUin { get; set; }
        public string recipientFullName { get; set; }

        /// <inheritdoc />
        public void Dispose()
        {
            status = null;
            bpmProcessStatus = null;
            declarantIdentificationNumber = null;
            declarantName = null;
            applicationCode = null;
            nameRu = null;
            nameKz = null;
            
            resultsForDownload.ForEach(x => x.Dispose());
            resultsForDownload.Clear();
            resultsForDownload = null;
            
            eds.Dispose();
            statusGo.Dispose();
            operatorIin = null;
            operatorName = null;
            recipientUin = null;
            recipientFullName = null;
        }
    }
}