namespace Camellia_Management_System.JsonObjects.RequestObjects
{
    /// <inheritdoc />
    public class BinDeclarant : Declarant
    {
        public string bin { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bin">BIN of the request target</param>
        /// <param name="declarantUin">BIIN of the sender</param>
        public BinDeclarant(string bin, string declarantUin) : base(declarantUin)
        {
            this.bin = bin;
        }
    }
}