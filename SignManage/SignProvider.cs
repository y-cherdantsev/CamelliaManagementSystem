using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Camellia_Management_System.SignManage
{
    /// @author Yevgeniy Cherdantsev
    /// @date 18.02.2020 10:21:54
    /// <summary>
    /// Sign Providing system (Automatic randomizer of signs from the given directory)
    /// </summary>
    /// <code>
    /// var signs = new SignProvider(@"C:\...\...\signs\");
    /// var newSign = signs.GetNextSign();
    /// </code>
    public class SignProvider : IDisposable
    {
        /// <summary>
        /// Path to the signs folder
        /// </summary>
        private string _pathToSignFolders;

        /// <summary>
        /// List of loaded signs
        /// </summary>
        private List<FullSign> _fullSigns;

        /// <summary>
        /// List of left signs
        /// </summary>
        public int signsLeft => _fullSigns.Count;

        /// @author Yevgeniy Cherdantsev
        /// @date 18.02.2020 10:23:09
        /// <summary>
        /// SignProvider constructor
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
        /// !!! This files should be in the inner folder: './signs/inner_folder/' !!!
        /// </code>
        public SignProvider(string pathToSignFolders)
        {
            _pathToSignFolders = pathToSignFolders;
        }

        /// @author Yevgeniy Cherdantsev
        /// @date 18.02.2020 10:23:09
        /// <summary>
        /// Forcibly loads list of signs from the folder
        /// </summary>
        public void LoadSigns()
        {
            _fullSigns = LoadRandomizedSigns();
        }

        /// @author Yevgeniy Cherdantsev
        /// @date 18.02.2020 10:25:57
        /// <summary>
        /// Generating list of signs
        /// </summary>
        /// <returns>List - list of signs</returns>
        /// <exception cref="FileNotFoundException">If file not found</exception>
        private List<FullSign> LoadRandomizedSigns()
        {
            var fullSigns = new List<FullSign>();
            var directoryInfo = new DirectoryInfo(_pathToSignFolders);

            //Check main possible fault reasons
            if (!directoryInfo.Exists)
                throw new FileNotFoundException($"Can not find directory: '{_pathToSignFolders}'");
            if (directoryInfo.GetDirectories().Length == 0)
                throw new FileNotFoundException($"'{_pathToSignFolders}' has no inner directories");

            foreach (var directory in directoryInfo.GetDirectories())
            {
                var files = directory.GetFiles();

                //Searching for necessary files
                var passwordFile = files.FirstOrDefault(x => x.Name.Equals("passwords.json"));
                var authFile = files.FirstOrDefault(x => x.Name.ToUpper().StartsWith("AUTH"));
                var rsaFile = files.FirstOrDefault(x => x.Name.ToUpper().StartsWith("RSA"));

                //Checks if all of the files exists
                if (passwordFile == null)
                    throw new FileNotFoundException($"Can not find passwords.json file in '{_pathToSignFolders}'");
                if (authFile == null)
                    throw new FileNotFoundException($"Can not find AUTH file in '{_pathToSignFolders}'");
                if (rsaFile == null)
                    throw new FileNotFoundException($"Can not find RSA file in '{_pathToSignFolders}'");

                //Generating full sign object
                var signPasswords =
                    JsonSerializer.Deserialize<SignPasswords>(passwordFile.OpenText().ReadToEnd());
                var authSign = new Sign(authFile.FullName, signPasswords.auth);
                var rsaSign = new Sign(rsaFile.FullName, signPasswords.rsa);
                var fullSign = new FullSign(authSign, rsaSign);

                fullSigns.Add(fullSign);
            }

            fullSigns = fullSigns.OrderBy(x => new Random().NextDouble()).ToList();
            return fullSigns;
        }


        /// @author Yevgeniy Cherdantsev
        /// @date 18.02.2020 10:30:26
        /// <summary>
        /// Returns new random sign
        /// </summary>
        /// <returns>FullSign - randomized sign</returns>
        /// <code>
        /// var newSign = signs.GetNextSign();
        /// </code>
        public FullSign GetNextSign()
        {
            //TODO(CHECK ERROR ACCURACY)

            //Works with lock for stability reasons
            lock (_fullSigns)
            {
                //Creates list to keep signs that hasn't been found in their location
                var lostSigns = new List<FullSign>();

                //If previous pool of signs ends it's generating new random pool
                if (_fullSigns.Count == 0)
                    _fullSigns = LoadRandomizedSigns();

                //Works till the emptiness of the list if needed
                while (_fullSigns.Count > 0)
                {
                    //Gets sign and removes it from the list
                    var randomSign = _fullSigns[0];
                    _fullSigns.RemoveAt(0);

                    //Check if the provided sign exists
                    if (new FileInfo(randomSign.authSign.filePath).Exists &&
                        new FileInfo(randomSign.rsaSign.filePath).Exists) return randomSign;

                    //If sign hasn't been found its added to lostSigns and repeats process 
                    lostSigns.Add(randomSign);
                }

                /*
                 *
                 * Generating error if signs hasn't been found
                 * Throws only if all signs till the end of existing list has been lost
                 *    For ex. If only one sign was in the list and it has been lost, exception will be raised
                 * 
                 */
                var errorMessage = lostSigns.Aggregate("!!!WARNING!!!\n",
                    (currentMessage, lostSign) => $"{currentMessage}{lostSign.authSign.filePath};\n");
                errorMessage +=
                    "Upper signs hasn't been found in defined location!!!\nPossible reasons:\n1.AUTH file has been removed;\n2.RSA file has been removed;\n3.Destination folder has been removed";
                throw new NullReferenceException(errorMessage);
            }
        }


        /// @author Yevgeniy Cherdantsev
        /// @date 18.02.2020 10:31:36
        /// <summary>
        /// Temp class for loading json object from 'passwords.json' file
        /// </summary>
        private class SignPasswords
        {
            public string auth { get; set; }
            public string rsa { get; set; }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _pathToSignFolders = null;
            _fullSigns.Clear();
            _fullSigns = null;
        }
    }
}