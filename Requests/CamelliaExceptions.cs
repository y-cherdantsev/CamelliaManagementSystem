using System;

// ReSharper disable CommentTypo

namespace CamelliaManagementSystem.Requests
{
    /// @author Yevgeniy Cherdantsev
    /// @date 13.10.2020 10:42:01
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

    /// @author Yevgeniy Cherdantsev
    /// @date 13.10.2020 10:42:01
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

    /// @author Yevgeniy Cherdantsev
    /// @date 13.10.2020 10:42:01
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

    /// @author Yevgeniy Cherdantsev
    /// @date 13.10.2020 14:57:01
    /// <summary>
    /// Custom CamelliaFile exception
    /// </summary>
    [Serializable]
    public class CamelliaFileException : Exception
    {
        /// <inheritdoc />
        public CamelliaFileException()
        {
        }

        /// <inheritdoc />
        public CamelliaFileException(string message)
            : base(message)
        {
        }

        /// <inheritdoc />
        public CamelliaFileException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// @author Yevgeniy Cherdantsev
    /// @date 13.10.2020 10:42:01
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

    /// @author Yevgeniy Cherdantsev
    /// @date 13.10.2020 10:42:01
    /// <summary>
    /// Custom CamelliaClient exception
    /// </summary>
    [Serializable]
    public class CamelliaClientException : Exception
    {
        /// <inheritdoc />
        public CamelliaClientException()
        {
        }

        /// <inheritdoc />
        public CamelliaClientException(string message)
            : base(message)
        {
        }

        /// <inheritdoc />
        public CamelliaClientException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// @author Yevgeniy Cherdantsev
    /// @date 14.10.2020 13:19:09
    /// <summary>
    /// Custom CamelliaClientProvider exception
    /// </summary>
    [Serializable]
    public class CamelliaClientProviderException : Exception
    {
        /// <inheritdoc />
        public CamelliaClientProviderException()
        {
        }

        /// <inheritdoc />
        public CamelliaClientProviderException(string message)
            : base(message)
        {
        }

        /// <inheritdoc />
        public CamelliaClientProviderException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}