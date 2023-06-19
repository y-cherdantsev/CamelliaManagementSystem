// ReSharper disable CommentTypo
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

#pragma warning disable 1591
namespace CamelliaManagementSystem.JsonObjects.ResponseObjects
{
    /// @author Yevgeniy Cherdantsev
    /// @date 18.02.2020 12:20:02
    /// <summary>
    /// UserInformation json objects
    /// </summary>
    /// todo(IMPLEMENT LEFT FIELDS)
    public class UserInformation
    {
        public string uin { get; set; }
        public string email { get; set; }
        public string[] roles { get; set; }
        public Info info { get; set; }
        public PhoneInfo phoneInfo { get; set; }
        public Availability availability { get; set; }


        /// @author Yevgeniy Cherdantsev
        /// @date 18.02.2020 12:20:42
        /// <summary>
        /// Info json objects
        /// </summary>
        public class Info
        {
            public Person person { get; set; } = new Person();

            /// @author Yevgeniy Cherdantsev
            /// @date 18.02.2020 12:21:02
            /// <summary>
            /// Person json objects
            /// </summary>
            public class Person
            {
                public bool capable { get; set; }
                public int currentAge { get; set; }
                public long dateOfBirth { get; set; }
                public string gender { get; set; }

                public bool hasActualDocuments { get; set; }
                public string iin { get; set; }
                public PersonName name { get; set; }
                public string fullName { get; set; }
                public Nationality nationality { get; set; }
                public string status { get; set; }
                public string statusCode { get; set; }

                /// @author Yevgeniy Cherdantsev
                /// @date 18.02.2020 12:21:02
                /// <summary>
                /// Name json objects
                /// </summary>
                public class PersonName
                {
                    public string firstName { get; set; }
                    public string lastName { get; set; }
                    public string middleName { get; set; }

                    public override string ToString() =>
                        $"{lastName} {firstName} {middleName}".Replace("  ", " ").Trim();
                }

                /// @author Yevgeniy Cherdantsev
                /// @date 18.02.2020 12:21:02
                /// <summary>
                /// Nationality json objects
                /// </summary>
                public class Nationality
                {
                    public string code { get; set; }
                    public string name { get; set; }
                    public long changeDate { get; set; }
                }
            }
        }

        /// @author Yevgeniy Cherdantsev
        /// @date 19.06.2020 16:39:09
        /// <summary>
        /// PhoneInfo json objects
        /// </summary>
        public class PhoneInfo
        {
            public string phone { get; set; }
            public int errCode { get; set; }
        }

        /// @author Yevgeniy Cherdantsev
        /// @date 19.06.2020 16:42:32
        /// <summary>
        /// Availability json objects
        /// </summary>
        public class Availability
        {
            public string message { get; set; }
            public bool available { get; set; }
        }
    }
}