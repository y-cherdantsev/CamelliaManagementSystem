namespace Camellia_Management_System.SignManage
{
    
    /*!
    @author Yevgeniy Cherdantsev
    @date 17.02.2020 15:00:41
    @version 1.0
    @brief Sign Object Creator

        */

    public class Sign
    {
        internal string FilePath { get; set; }
        internal string Password { get; set; }

        internal Sign()
        {
        }

        internal Sign(string filePath, string password)
        {
            FilePath = filePath;
            Password = password;
        }
    }
}