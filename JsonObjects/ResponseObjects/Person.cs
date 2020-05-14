namespace Camellia_Management_System.JsonObjects.ResponseObjects
{
    public class Person
    {
      public bool capable{get; set;}
      public int currentAge{get; set;}
      public long dateOfBirth{get; set;}
      public string gender{get; set;}
      public bool hasActualDocuments{get; set;}
      public string iin{get; set;}
      public PersonName name { get; set; }
      public string status{get; set;}
    }

    public class PersonName
    {
        public string firstName {get;set;}
        public string lastName {get;set;}
        public string middleName {get;set;}
    }
}