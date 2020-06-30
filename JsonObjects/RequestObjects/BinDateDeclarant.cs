using System;

namespace Camellia_Management_System.JsonObjects.RequestObjects
{
    /// <inheritdoc />
    public class BinDateDeclarant : BinDeclarant, IDisposable
    {
        public string innerdate { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bin">BIN of the request target</param>
        /// <param name="date">DateTime of the request target</param>
        /// <param name="declarantUin">BIIN of the sender</param>
        public BinDateDeclarant(string bin, string declarantUin, string date) : base(bin, declarantUin)
        {
            innerdate = date;
        }

        /// <inheritdoc />
        public new void Dispose()
        {
            innerdate = null;
            base.Dispose();
        }
    }
}