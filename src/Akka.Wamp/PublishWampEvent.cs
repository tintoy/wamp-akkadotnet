using System;
using System.Collections.Immutable;
using WampSharp.V2;

namespace Akka.Wamp.Messages
{
    /// <summary>
    ///     Request for publishing a WAMP event.
    /// </summary>
    public class PublishWampEvent
    {
        /// <summary>
        ///     Create a new <see cref="PublishWampEvent"/> message.
        /// </summary>
        /// <param name="arguments">
        ///     The event arguments.
        /// </param>
        public PublishWampEvent(params object[] arguments)
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

        /// <summary>
        ///     Convert the <see cref="PublishWampEvent"/> message to a <see cref="WampEvent"/>.
        /// </summary>
        /// <returns>
        ///     The configured <see cref="WampEvent"/>.
        /// </returns>
        public WampEvent ToWampEvent()
        {
            object[] arguments = new object[Arguments.Count];
            Arguments.CopyTo(arguments);

            return new WampEvent
            {
                Arguments = arguments
            };
        }
    }
}