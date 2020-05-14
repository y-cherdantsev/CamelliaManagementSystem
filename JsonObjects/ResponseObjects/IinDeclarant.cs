namespace Camellia_Management_System.JsonObjects.ResponseObjects
{
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