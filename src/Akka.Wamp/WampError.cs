using System;

namespace Akka.Wamp
{
    /// <summary>
    ///     Message indicating that a WAMP integration component has encountered an error.
    /// </summary>
    public class WampError
    {
        /// <summary>
        ///     Create a new <see cref="WampError"/> message.
        /// </summary>
        /// <param name="exception">
        ///     The exception that represents the error.
        /// </param>
        /// <param name="operation">
        ///     A <see cref="WampOperation"/> value representing the type of operation taking place when the error was encountered.
        /// </param>
        public WampError(Exception exception, WampOperation operation)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));
            
            Exception = exception;
        }

        /// <summary>
        ///     The exception that represents the error.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        ///     A <see cref="WampOperation"/> value representing the type of operation taking place when the error was encountered.
        /// </summary>
        public WampOperation Operation { get; }
    }
}
