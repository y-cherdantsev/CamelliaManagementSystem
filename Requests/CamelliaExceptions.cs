using System;

namespace CamelliaManagementSystem.Requests
{
    /// <summary>
    /// Custom CamelliaRequest exception
    /// </summary>
    [Serializable]
    public class CamelliaRequestException : Exception
    {
        /// <inheritdoc />
        public CamelliaRequestException()
        {
        }

        /// <inheritdoc />
        public CamelliaRequestException(string message)
            : base(message)
        {
        }

        /// <inheritdoc />
        public CamelliaRequestException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Custom CamelliaRequest exception
    /// </summary>
    [Serializable]
    public class CamelliaNoneDataException : Exception
    {
        /// <inheritdoc />
        public CamelliaNoneDataException()
        {
        }

        /// <inheritdoc />
        public CamelliaNoneDataException(string message)
            : base(message)
        {
        }

        /// <inheritdoc />
        public CamelliaNoneDataException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Custom CamelliaUnknown exception
    /// </summary>
    [Serializable]
    public class CamelliaUnknownException : Exception
    {
        /// <inheritdoc />
        public CamelliaUnknownException()
        {
        }

        /// <inheritdoc />
        public CamelliaUnknownException(string message)
            : base(message)
        {
        }

        /// <inheritdoc />
        public CamelliaUnknownException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Custom CaptchaSolve exception
    /// </summary>
    [Serializable]
    public class CamelliaCaptchaSolverException : Exception
    {
        /// <inheritdoc />
        public CamelliaCaptchaSolverException()
        {
        }

        /// <inheritdoc />
        public CamelliaCaptchaSolverException(string message)
            : base(message)
        {
        }

        /// <inheritdoc />
        public CamelliaCaptchaSolverException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}