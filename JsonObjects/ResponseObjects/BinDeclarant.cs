//TODO(REFACTOR)
namespace Camellia_Management_System.JsonObjects.ResponseObjects
{

    /// @author Yevgeniy Cherdantsev
    /// @date 07.03.2020 16:32:40
    /// @version 1.0
    /// <summary>
    /// Json object that should be send in order to get reference
    /// </summary>
    public class BinDeclarant
    {
        public string bin { get; set; } = "";
        public string declarantUin { get; set; } = ""; 
        public BinDeclarant(string bin, string declarantUin)
        {
            this.bin = bin;
            this.declarantUin = declarantUin;
        }
        
    }
}