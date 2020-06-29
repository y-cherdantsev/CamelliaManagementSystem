//TODO(REFACTOR)
namespace Camellia_Management_System.JsonObjects.ResponseObjects
{
    public class BinDateDeclarant
    {
        public string bin { get; set; } = "";
        public string declarantUin { get; set; } = ""; 
        public string innerdate { get; set; } = ""; 
        public BinDateDeclarant(string bin, string declarantUin, string date)
        {
            this.bin = bin;
            this.declarantUin = declarantUin;
            this.innerdate = date;
        }
    }
}