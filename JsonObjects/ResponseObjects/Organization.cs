using System;

namespace Camellia_Management_System.JsonObjects.ResponseObjects
{
    /// @author Yevgeniy Cherdantsev
    /// @date 18.02.2020 12:18:07
    /// <summary>
    /// Organization json object
    /// </summary>
    public class Organization : IDisposable
    {
        public string address { get; set; }
        public string bin { get; set; }
        public string fullName { get; set; }
        public string fullNameEn { get; set; }
        public string fullNameKk { get; set; }
        public string incorporationCountry { get; set; }
        public long? registrationDate { get; set; }
        public bool? resident { get; set; }
        public string shortName { get; set; }
        public string shortNameEn { get; set; }
        public string shortNameKk { get; set; }
        public Status status { get; set; }

        /// @author Yevgeniy Cherdantsev
        /// @date 18.02.2020 12:18:07
        /// <summary>
        /// Status json object
        /// </summary>
        public class Status : IDisposable
        {
            public string code { get; set; }
            public Description description { get; set; }

            /// @author Yevgeniy Cherdantsev
            /// @date 18.02.2020 12:18:07
            /// <summary>
            /// Description json object
            /// </summary>
            public class Description : IDisposable
            {
                public string en { get; set; }
                public string kk { get; set; }
                public string ru { get; set; }

                /// <inheritdoc />
                public void Dispose()
                {
                    en = null;
                    kk = null;
                    ru = null;
                }
            }

            /// <inheritdoc />
            public void Dispose()
            {
                code = null;
                description.Dispose();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            address = null;
            bin = null;
            fullName = null;
            fullNameEn = null;
            fullNameKk = null;
            incorporationCountry = null;
            shortName = null;
            shortNameEn = null;
            shortNameKk = null;
            status.Dispose();
        }
    }
}