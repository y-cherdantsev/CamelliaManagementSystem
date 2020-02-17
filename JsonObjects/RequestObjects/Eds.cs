﻿﻿﻿namespace Camellia_Management_System.JsonObjects.RequestObjects
{

/*!

@author Yevgeniy Cherdantsev
@date 17.02.2020 18:49:50
@version 1.0
@brief Eds json object
    
    */

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