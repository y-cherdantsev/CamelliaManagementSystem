using System.Collections.Generic;

namespace Camellia_Management_System.JsonObjects.ResponseObjects
{
    /// @author Yevgeniy Cherdantsev
    /// @date 18.02.2020 12:18:07
    /// @version 1.0
    /// <summary>
    /// ReadinessStatus json object
    /// </summary>
    public class ReadinessStatus
    {
        public string status { get; set; } = "";
        public string bpmProcessStatus { get; set; } = "";
        public bool completed { get; set; }
        public string declarantIdentificationNumber { get; set; } = "";
        public string declarantName { get; set; } = "";
        public string applicationCode { get; set; } = "";
        public string nameRu { get; set; } = "";
        public string nameKz { get; set; } = "";
        public long appCreationDate { get; set; }
        public long updatedDate { get; set; }
        public List<ResultForDownload> resultsForDownload { get; set; } = new List<ResultForDownload>();
        public Eds eds = new Eds();
        public StatusGo statusGo = new StatusGo();
        public string operatorIin { get; set; } = "";
        public string operatorName { get; set; } = "";
        public string recipientUin { get; set; } = "";
        public string recipientFullName { get; set; } = "";
    }
}