﻿﻿namespace Camellia_Management_System.JsonObjects.RequestObjects
{

/*!

@author Yevgeniy Cherdantsev
@date 17.02.2020 18:51:40
@version 1.0
@brief StatusGo json object
    
    */

    public class StatusGo
    {
        public string code { get; set; } = "";
        public Name name { get; set; } = new Name();
    }
    
    /*!
@author Yevgeniy Cherdantsev
@date 10.01.2020 09:16:09
@version 1.0
@brief Name json object
    
    */

    public class Name
    {
        public string ru { get; set; } = "";
        public string kk { get; set; } = "";
        public string en { get; set; } = "";
    }
    
}