namespace Camellia_Management_System.JsonObjects
{
/*!

@author Yevgeniy Cherdantsev
@date 17.02.2020 18:47:09
@version 1.0
@brief UserInformation json objects
    
    */

    public class UserInformation
    {
        public string uin { get; set; } = "";
        public Info info { get; set; } = new Info();
    }

    /*!

@author Yevgeniy Cherdantsev
@date 17.02.2020 18:50:49
@version 1.0
@brief Info json objects
    
    */
    public class Info
    {
        public Person person { get; set; } = new Person();
    }

    /*!

@author Yevgeniy Cherdantsev
@date 17.02.2020 18:51:03
@version 1.0
@brief Person json objects
   
   */
    public class Person
    {
        public string iin { get; set; } = "";
    }
}