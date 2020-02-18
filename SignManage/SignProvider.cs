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
        private readonly string _pathToSignFolders;
        private int _i = 0;
        private List<CustomSign> _customSign;

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
        /// </code>
        public SignProvider(string pathToSignFolders)
        {
            _pathToSignFolders = pathToSignFolders;
            _customSign = ShuffleList(LoadSigns());
        }


        /// @author Yevgeniy Cherdantsev
        /// @date 18.02.2020 10:25:57
        /// @version 1.0
        /// <summary>
        /// Generating list of signs
        /// </summary>
        /// <returns>List - list of signs</returns>
        /// <exception cref="FileNotFoundException">If file not found</exception>
        private List<CustomSign> LoadSigns()
        {
            var customSigns = new List<CustomSign>();
            var directoryInfo = new DirectoryInfo(_pathToSignFolders);
            if (!directoryInfo.Exists)
                throw new FileNotFoundException($"Can not find directory: '{_pathToSignFolders}'");
            if (directoryInfo.GetDirectories().Length == 0)
                throw new FileNotFoundException($"'{_pathToSignFolders}' has no inner directories");
            foreach (var directory in directoryInfo.GetDirectories())
            {
                var customSign = new CustomSign();
                var files = directory.GetFiles();
                foreach (var fileInfo in files)
                {
                    if (fileInfo.Name.StartsWith("AUTH"))
                        customSign.AuthSign.FilePath = fileInfo.FullName;
                    else if (fileInfo.Name.StartsWith("RSA"))
                        customSign.RsaSign.FilePath = fileInfo.FullName;
                    else if (fileInfo.Name.Equals("passwords.json"))
                    {
                        var customPasswords =
                            JsonSerializer.Deserialize<CustomPasswords>(fileInfo.OpenText().ReadToEnd());
                        customSign.AuthSign.Password = customPasswords.auth;
                        customSign.RsaSign.Password = customPasswords.rsa;
                    }
                }

                if (customSign.AuthSign.FilePath == null)
                    throw new FileNotFoundException(
                        $"Hasn't found any auth signs in '{_pathToSignFolders}' directory");

                if (customSign.AuthSign.Password == null)
                    throw new FileNotFoundException(
                        $"Hasn't found any password for auth sign in '{_pathToSignFolders}' directory");

                if (customSign.RsaSign.FilePath == null)
                    throw new FileNotFoundException(
                        $"Hasn't found any rsa signs in '{_pathToSignFolders}' directory");

                if (customSign.AuthSign.Password == null)
                    throw new FileNotFoundException(
                        $"Hasn't found any password for rsa sign in '{_pathToSignFolders}' directory");


                customSigns.Add(customSign);
            }

            return customSigns;
        }


        /// @author Yevgeniy Cherdantsev
        /// @date 18.02.2020 10:30:26
        /// @version 1.0
        /// <summary>
        /// Reterns new random sign
        /// </summary>
        /// <returns>CustomSign - randomized sign</returns>
        /// <code>
        /// var newSign = signs.GetNextSign();
        /// </code>
        public CustomSign GetNextSign()
        {
            if (_customSign.Count == 0)
                _customSign = ShuffleList(LoadSigns());

            var randomSign = _customSign[0];
            _customSign.RemoveAt(0);
            while (!(new FileInfo(randomSign.AuthSign.FilePath).Exists &&
                     new FileInfo(randomSign.RsaSign.FilePath).Exists))
                return GetNextSign();

            return randomSign;
        }


        /// @author Yevgeniy Cherdantsev
        /// @date 18.02.2020 10:31:15
        /// @version 1.0
        /// <summary>
        /// Temp class
        /// </summary>
        public class CustomSign
        {
            public Sign AuthSign { get; set; } = new Sign();
            public Sign RsaSign { get; set; } = new Sign();
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