using System.Collections.Generic;
using System.Linq;
using Camellia_Management_System.FileManage;

namespace Camellia_Management_System.Requests
{
    /// @author Yevgeniy Cherdantsev
    /// @date 07.03.2020 16:49:47
    /// @version 1.0
    /// <summary>
    /// 
    /// </summary>
    public class RegistrationReference : SingleInputRequest
    {
        public RegistrationReference(CamelliaClient camelliaClient) : base(camelliaClient)
        {
            RequestLink = "https://egov.kz/services/P30.11/";
        }


        /// @author Yevgeniy Cherdantsev
        /// @date 11.03.2020 15:42:39
        /// @version 1.0
        /// <summary>
        /// Parsing of registration reference and getting of founders from it
        /// </summary>
        /// <param name="bin">Bin</param>
        /// <param name="delay">Delay of checking if the reference is in ms</param>
        /// <returns>IEnumerable - list of founders</returns>
        public IEnumerable<string> GetFounders(string bin, int delay = 1000)
        {
            var reference = GetReference(bin, delay);

            var temp = reference.First(x => x.language.Contains("ru"));
            if (temp != null)
                return new PdfParser(temp.SaveFile("./")).GetFounders();
            return null;
        }
    }
}