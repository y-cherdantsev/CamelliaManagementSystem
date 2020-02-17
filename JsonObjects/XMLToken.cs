﻿namespace Camellia_Management_System.JsonObjects
{

/*!

@author Yevgeniy Cherdantsev
@date 17.02.2020 18:45:33
@version 1.0
@brief XMLToken json object
    
    */
    public class XMLToken
    {
        public string xml { get; set; } = "";

        public XMLToken(string xml)
        {
            this.xml = xml;
        }
    }
}