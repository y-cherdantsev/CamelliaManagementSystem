namespace Camellia_Management_System.JsonObjects.RequestObjects
{

    /// @author Yevgeniy Cherdantsev
    /// @date 07.03.2020 16:32:40
    /// @version 1.0
    /// <summary>
    /// INPUT
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