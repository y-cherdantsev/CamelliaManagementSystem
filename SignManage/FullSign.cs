namespace Camellia_Management_System.SignManage
{
    /// @author Yevgeniy Cherdantsev
    /// @date 18.02.2020 10:31:15
    /// @version 1.0
    /// <summary>
    /// Class which contains all information about user signs
    /// </summary>
    public class FullSign
    {
        public Sign AuthSign { get; set; } = new Sign();
        public Sign RsaSign { get; set; } = new Sign();
    }

    /// @author Yevgeniy Cherdantsev
    /// @date 18.02.2020 10:17:31
    /// @version 1.0
    /// <summary>
    /// Sign Object Creator
    /// </summary>
    public class Sign
    {
        /// <summary>
        /// Path to the sign file
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Password for the sign
        /// </summary>
        public string Password { get; set; }

        /// @author Yevgeniy Cherdantsev
        /// @date 18.02.2020 10:34:30
        /// @version 1.0
        /// <summary>
        /// Constructor
        /// </summary>
        public Sign()
        {
        }


        /// @author Yevgeniy Cherdantsev
        /// @date 18.02.2020 10:34:30
        /// @version 1.0
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filePath">Path to the sign file</param>
        /// <param name="password">Password for the sign</param>
        internal Sign(string filePath, string password)
        {
            FilePath = filePath;
            Password = password;
        }
    }
}