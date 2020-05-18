using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Camellia_Management_System.SignManage
{
    /// @author Yevgeniy Cherdantsev
    /// @date 18.02.2020 10:21:54
    /// @version 1.0
    /// <summary>
    /// Sign Providing system (Automatic randomizer of signs from the given directory)
    /// </summary>
    /// <code>
    /// var signs = new SignProvider(@"C:\...\...\signs\");
    /// var newSign = signs.GetNextSign();
    /// </code>
    public class SignProvider
    {
        /// <summary>
        /// Path to the signs folder
        /// </summary>
        private readonly string _pathToSignFolders;
        
        /// <summary>
        /// List of loaded signs
        /// </summary>
        private List<FullSign> _fullSign;
        
        /// <summary>
        /// List of left signs
        /// </summary>
        public int signsLeft => _fullSign.Count;

        /// @author Yevgeniy Cherdantsev
        /// @date 18.02.2020 10:23:09
        /// @version 1.0
        /// <summary>
        /// Sign Provider constructor
        /// </summary>
        /// <param name="pathToSignFolders">path to the folder with signs</param>
        /// <exception cref="FileNotFoundException">If the folder can't be found</exception>
        /// <code>
        /// new SignProvider(@"C:\...\...\signs\");
        /// 
        /// Authentication sign should start with 'AUTH'
        /// RSA sign should start with 'RSA'
        ///
        /// passwords.json example:
        /// {
        ///     "auth":"auth_password",
        ///     "rsa":"rsa_password"
        /// }
        ///
        /// !!! This files should be in the inner folder: './signs/inner_folder1/' !!!
        /// </code>
        public SignProvider(string pathToSignFolders)
        {
            _pathToSignFolders = pathToSignFolders;
            _fullSign = ShuffleList(LoadSigns());
        }

        public void ReloadSigns()
        {
            _fullSign = LoadSigns();
        }
        
        /// @author Yevgeniy Cherdantsev
        /// @date 18.02.2020 10:25:57
        /// @version 1.0
        /// <summary>
        /// Generating list of signs
        /// </summary>
        /// <returns>List - list of signs</returns>
        /// <exception cref="FileNotFoundException">If file not found</exception>
        private List<FullSign> LoadSigns()
        {
            var fullSigns = new List<FullSign>();
            var directoryInfo = new DirectoryInfo(_pathToSignFolders);
            if (!directoryInfo.Exists)
                throw new FileNotFoundException($"Can not find directory: '{_pathToSignFolders}'");
            if (directoryInfo.GetDirectories().Length == 0)
                throw new FileNotFoundException($"'{_pathToSignFolders}' has no inner directories");
            foreach (var directory in directoryInfo.GetDirectories())
            {
                var fullSign = new FullSign();
                var files = directory.GetFiles();
                foreach (var fileInfo in files)
                {
                    if (fileInfo.Name.StartsWith("AUTH"))
                        fullSign.AuthSign.FilePath = fileInfo.FullName;
                    else if (fileInfo.Name.StartsWith("RSA"))
                        fullSign.RsaSign.FilePath = fileInfo.FullName;
                    else if (fileInfo.Name.Equals("passwords.json"))
                    {
                        var customPasswords =
                            JsonSerializer.Deserialize<CustomPasswords>(fileInfo.OpenText().ReadToEnd());
                        fullSign.AuthSign.Password = customPasswords.auth;
                        fullSign.RsaSign.Password = customPasswords.rsa;
                    }
                }

                if (fullSign.AuthSign.FilePath == null)
                    throw new FileNotFoundException(
                        $"Hasn't found any auth signs in '{_pathToSignFolders}' directory");

                if (fullSign.AuthSign.Password == null)
                    throw new FileNotFoundException(
                        $"Hasn't found any password for auth sign in '{_pathToSignFolders}' directory");

                if (fullSign.RsaSign.FilePath == null)
                    throw new FileNotFoundException(
                        $"Hasn't found any rsa signs in '{_pathToSignFolders}' directory");

                if (fullSign.AuthSign.Password == null)
                    throw new FileNotFoundException(
                        $"Hasn't found any password for rsa sign in '{_pathToSignFolders}' directory");


                fullSigns.Add(fullSign);
            }

            return fullSigns;
        }


        /// @author Yevgeniy Cherdantsev
        /// @date 18.02.2020 10:30:26
        /// @version 1.0
        /// <summary>
        /// Reterns new random sign
        /// </summary>
        /// <returns>FullSign - randomized sign</returns>
        /// <code>
        /// var newSign = signs.GetNextSign();
        /// </code>
        public FullSign GetNextSign()
        {
            if (_fullSign.Count == 0)
                _fullSign = ShuffleList(LoadSigns());

            var randomSign = _fullSign[0];
            _fullSign.RemoveAt(0);
            while (!(new FileInfo(randomSign.AuthSign.FilePath).Exists &&
                     new FileInfo(randomSign.RsaSign.FilePath).Exists))
                return GetNextSign();

            return randomSign;
        }


        /// @author Yevgeniy Cherdantsev
        /// @date 18.02.2020 10:31:36
        /// @version 1.0
        /// <summary>
        /// Temp class for loading json object from 'passwords.json' file
        /// </summary>
        private class CustomPasswords
        {
            public string auth { get; set; }
            public string rsa { get; set; }
        }

        /// @author Yevgeniy Cherdantsev
        /// @date 18.02.2020 10:31:53
        /// @version 1.0
        /// <summary>
        /// Shuffles given list of objects
        /// </summary>
        /// <param name="inputList">List to shuffle</param>
        /// <returns>List - Shuffled list</returns>
        private static List<TE> ShuffleList<TE>(IList<TE> inputList)
        {
            var shuffledList = new List<TE>();

            var r = new Random(DateTime.UtcNow.Millisecond);
            while (inputList.Count > 0)
            {
                var randomIndex = r.Next(0, inputList.Count);
                shuffledList.Add(inputList[randomIndex]);
                inputList.RemoveAt(randomIndex);
            }

            return shuffledList;
        }
    }
}