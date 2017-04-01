using System;

namespace Akka.Wamp
{
    /// <summary>
    ///     Exception raised by WAMP integration components for Akka.NET.
    /// </summary>
    [Serializable]
    public class AkkaWampException
        : Exception
    {
        /// <summary>
        ///     Create a new <see cref="AkkaWampException"/>.
        /// </summary>
        /// <param name="message">
        ///     The exception message.
        /// </param>
        /// <param name="isTransient">
        ///     Does the exception represent a transient error?
        /// </param>
        public AkkaWampException(string message, bool isTransient = false)
            : base(message)
        {
            IsTransient = isTransient;
        }
        
        /// <summary>
        ///     Create a new <see cref="AkkaWampException"/>.
        /// </summary>
        /// <param name="message">
        ///     The exception message.
        /// </param>
        /// <param name="innerException">
        ///     The exception that caused the <see cref="AkkaWampException"/> to be raised.
        /// </param>
        /// <param name="isTransient">
        ///     Does the exception represent a transient error?
        /// </param>
        public AkkaWampException(string message, Exception innerException, bool isTransient = false)
            : base(message, innerException)
        {
            IsTransient = isTransient;
        }

        /// <summary>
        ///     Does the exception represent a transient error?
        /// </summary>
        public bool IsTransient { get; }
    }
}