//TODO(REFACTOR)﻿
namespace Camellia_Management_System.JsonObjects
{
    /// @author Yevgeniy Cherdantsev
    /// @date 18.02.2020 12:19:42
    /// @version 1.0
    /// <summary>
    /// XMLToken json object
    /// </summary>
    public class XMLToken
    {
        public string xml { get; set; } = "";

        public XMLToken(string xml)
        {
            this.xml = xml;
        }
    }
}