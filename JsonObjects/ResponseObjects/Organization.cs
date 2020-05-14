namespace Camellia_Management_System.JsonObjects.ResponseObjects
{
    public class Organization
    {
        public string address {get;set;}
        public string bin {get;set;}
        public string fullName {get;set;}
        public string fullNameEn {get;set;}
        public string fullNameKk {get;set;}
        public string incorporationCountry {get;set;}
        public long registrationDate {get;set;}
        public bool resident {get;set;}
        public string shortName {get;set;}
        public string shortNameEn {get;set;}
        public string shortNameKk {get;set;}
        public Status status { get; set; }
    }
    public class Status
    {
        public string code { get; set; }
        public Description description { get; set; }
    }
    public class Description
    {
        public string en { get; set; }
        public string kk { get; set; }
        public string ru { get; set; }
    }
}