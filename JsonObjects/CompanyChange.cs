using System;

//TODO(REFACTOR)
namespace Camellia_Management_System.JsonObjects
{
    /// @author Yevgeniy Cherdantsev
    /// @date 18.02.2020 12:19:42
    /// @version 1.0
    /// <summary>
    /// CompanyChange json object for 
    /// </summary>
    public class CompanyChange
    {
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public string Before { get; set; }
        public string After { get; set; }
    }
}