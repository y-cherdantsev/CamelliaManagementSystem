using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Camellia_Management_System.SignManage
{
    /*!

@author Yevgeniy Cherdantsev
@date 17.02.2020 19:00:59
@version 1.0
@brief Sign Providing system (Automatic randomizer of signs from the given directory)
     
@code
     
     var signs = new SignProvider(@"C:\...\...\signs\");
     var newSign = signs.GetNextSign();
     
@endcode
    
    */

    public class SignProvider
    {
        private readonly string _pathToSignFolders;
        private int _i = 0;
        private List<CustomSigns> _customSigns;

        /*!

@author Yevgeniy Cherdantsev
@date 17.02.2020 18:59:31
@version 1.0
@brief Sign Provider constructor
@param[in] pathToSignFolders - path to the folder with signs
@throw FileNotFoundException - если не найдено указанной папки
     
@code
     new SignProvider(@"C:\...\...\signs\");
@endcode
     
     */
        public SignProvider(string pathToSignFolders)
        {
            _pathToSignFolders = pathToSignFolders;
            _customSigns = ShuffleList(LoadSigns());
        }


        /*!

@author Yevgeniy Cherdantsev
@date 17.02.2020 19:08:30
@version 1.0
@brief generating list of signs
@return customSigns - list of signs
@throw FileNotFoundException
     
     */
        private List<CustomSigns> LoadSigns()
        {
            var customSigns = new List<CustomSigns>();
            var directoryInfo = new DirectoryInfo(_pathToSignFolders);
            if (!directoryInfo.Exists)
                throw new FileNotFoundException($"Can not find directory: '{_pathToSignFolders}'");
            if (directoryInfo.GetDirectories().Length == 0)
                throw new FileNotFoundException($"'{_pathToSignFolders}' has no inner directories");
            foreach (var directory in directoryInfo.GetDirectories())
            {
                var customSign = new CustomSigns();
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

        /*!

@author Yevgeniy Cherdantsev
@date 17.02.2020 19:33:42
@version 1.0
@brief Reterns new random sign
@return randomSign - randomized sign

@code
     var newSign = signs.GetNextSign();
@endcode
     
     */
        
        public CustomSigns GetNextSign()
        {
            if (_customSigns.Count == 0)
                _customSigns = ShuffleList(LoadSigns());

            var randomSign = _customSigns[0];
            _customSigns.RemoveAt(0);
            while (!(new FileInfo(randomSign.AuthSign.FilePath).Exists && new FileInfo(randomSign.RsaSign.FilePath).Exists))
                return GetNextSign();

            return randomSign;
        }

        /*!

@author Yevgeniy Cherdantsev
@date 17.02.2020 19:36:05
@version 1.0
@brief Temp class
    
    */

        public class CustomSigns
        {
            public Sign AuthSign { get; set; } = new Sign();
            public Sign RsaSign { get; set; } = new Sign();
        }


/*!

@author Yevgeniy Cherdantsev
@date 17.02.2020 19:36:25
@version 1.0
@brief Temp class for loading json object from 'passwords.json' file
    
    */
        
        private class CustomPasswords
        {
            public string auth { get; set; }
            public string rsa { get; set; }
        }

/*!

@author Yevgeniy Cherdantsev
@date 17.02.2020 19:37:18
@version 1.0
@brief Shiffles given list of objects
@param[in] inputList - List
@return shuffledList - Shuffled list
     
     */
        
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