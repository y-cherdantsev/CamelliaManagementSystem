﻿﻿﻿namespace Camellia_Management_System.JsonObjects.ResponseObjects
{
    /// @author Yevgeniy Cherdantsev
    /// @date 18.02.2020 12:17:45
    /// @version 1.0
    /// <summary>
    /// Eds json object
    /// </summary>
    public class Eds
    {
        public long createdDate { get; set; }
        public long endedDate { get; set; }
        public string iin { get; set; } = "";
        public string bin { get; set; } = "";
        public string fullname { get; set; } = "";
        public string organization { get; set; } = "";
        public string status { get; set; } = "";
    }
}