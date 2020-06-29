namespace Camellia_Management_System.JsonObjects.ResponseObjects
{
    /// @author Yevgeniy Cherdantsev
    /// @date 18.02.2020 12:19:00
    /// @version 1.0
    /// <summary>
    /// StatusGo json object
    /// </summary>
    public class StatusGo
    {
        public string code { get; set; }
        public Name name { get; set; }


        /// @author Yevgeniy Cherdantsev
        /// @date 18.02.2020 12:19:21
        /// @version 1.0
        /// <summary>
        /// Name json object
        /// </summary>
        public class Name
        {
            public string ru { get; set; }
            public string kk { get; set; }
            public string en { get; set; }
        }
    }
}