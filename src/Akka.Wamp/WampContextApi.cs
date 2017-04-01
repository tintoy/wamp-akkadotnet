using Akka.Actor;
using System;

namespace Akka.Wamp
{
    /// <summary>
    ///     The WAMP API for an an actor's local context.
    /// </summary>
    public sealed class WampContextApi
    {
        /// <summary>
        ///     Create a new <see cref="WampContextApi"/>.
        /// </summary>
        /// <param name="context">
        ///     The local actor context.
        /// </param>
        internal WampContextApi(IUntypedActorContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            Context = context;
        }

        /// <summary>
        ///     The local actor context.
        /// </summary>
        public IUntypedActorContext Context { get; }

        /// <summary>
        ///     The global WAMP API for the actor system.
        /// </summary>
        public WampApi WampApi => Context.System.Wamp();

        /// <summary>
        ///     Request that a subscriber terminate its subscription.
        /// </summary>
        /// <param name="subscriber">
        ///     The subscriber actor.
        /// </param>
        public void Unsubscribe(IActorRef subscriber)
        {
            if (subscriber == null)
                throw new ArgumentNullException(nameof(subscriber));

            subscriber.Tell(Messages.Unsubscribe.Instance);
        }
    }
}
