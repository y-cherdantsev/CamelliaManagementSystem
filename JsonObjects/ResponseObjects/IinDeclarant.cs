namespace Camellia_Management_System.JsonObjects.ResponseObjects
{
    /// @author Yevgeniy Cherdantsev
    /// @date 14.05.2020 16:21:40
    /// @version 1.0
    /// <summary>
    /// Json object that should be send in order to get reference
    /// </summary>
    public class IinDeclarant
    {
        public string iin { get; set; } = "";
        public string declarantUin { get; set; } = ""; 
        public IinDeclarant(string iin, string declarantUin)
        {
            this.iin = iin;
            this.declarantUin = declarantUin;
        }
    }
}