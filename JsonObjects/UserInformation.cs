//TODO(REFACTOR)
namespace Camellia_Management_System.JsonObjects
{
    /// @author Yevgeniy Cherdantsev
    /// @date 18.02.2020 12:20:02
    /// @version 1.0
    /// <summary>
    /// UserInformation json objects
    /// </summary>
    public class UserInformation
    {
        public string uin { get; set; } = "";
        public Info info { get; set; } = new Info();
    }

    /// @author Yevgeniy Cherdantsev
    /// @date 18.02.2020 12:20:42
    /// @version 1.0
    /// <summary>
    /// Info json objects
    /// </summary>
    public class Info
    {
        public PersonInfo person { get; set; } = new PersonInfo();
    }

    /// @author Yevgeniy Cherdantsev
    /// @date 18.02.2020 12:21:02
    /// @version 1.0
    /// <summary>
    /// Person json objects
    /// </summary>
    public class PersonInfo
    {
        public string iin { get; set; } = "";
    }
}