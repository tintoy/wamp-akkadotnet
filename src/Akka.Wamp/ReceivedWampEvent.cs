using System;
using System.Collections.Immutable;

namespace Akka.Wamp.Messages
{
    /// <summary>
    ///     Message indicating that a WAMP event was received.
    /// </summary>
    public class ReceivedWampEvent
    {
        /// <summary>
        ///     Create a new <see cref="ReceivedWampEvent"/> message.
        /// </summary>
        /// <param name="arguments">
        ///     The event arguments.
        /// </param>
        public ReceivedWampEvent(object[] arguments)
        {
            if (arguments == null)
                throw new ArgumentNullException(nameof(arguments));
            
            if (arguments.Length == 0)
                throw new ArgumentException("Must specify at least one event argument.", nameof(arguments));

            Arguments = ImmutableList.Create(arguments);
        }

        /// <summary>
        ///     The event arguments.
        /// </summary>
        public ImmutableList<object> Arguments { get; }
    }
}