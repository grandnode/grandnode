using System;

namespace Grand.Core
{
    /// <summary>
    /// Represents errors that occur during application execution
    /// </summary>
    [Serializable]
    public class GrandException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the Exception class.
        /// </summary>
        public GrandException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the Exception class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public GrandException(string message)
            : base(message)
        {
        }

    }
}
