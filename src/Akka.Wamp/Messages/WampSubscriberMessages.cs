using System.Collections.Immutable;

namespace Akka.Wamp.Messages
{
    using Actors;

    /// <summary>
    ///     Request for activation from the owner of a <see cref="WampSubscriber"/> actor to activate the subscription.
    /// </summary>
    public class ActivateSubscription
    {
        /// <summary>
        ///     The singleton instance of the <see cref="ActivateSubscription"/> message.
        /// </summary>
        public static readonly ActivateSubscription Instance = new ActivateSubscription();

        /// <summary>
        ///     Create a new <see cref="ActivateSubscription"/> message.
        /// </summary>
        public ActivateSubscription()
        {
        }
    }

    /// <summary>
    ///     Message from <see cref="WampSubscriber"/> to itself indicating that the timeout period expired before the subscription was activated by the subscription owner.
    /// </summary>
    class SubscriptionActivationTimeout
    {
        /// <summary>
        ///     The singleton instance of the <see cref="SubscriptionActivationTimeout"/> message.
        /// </summary>
        public static readonly SubscriptionActivationTimeout Instance = new SubscriptionActivationTimeout();

        /// <summary>
        ///     Create a new <see cref="ActivateSubscription"/> message.
        /// </summary>
        SubscriptionActivationTimeout()
        {
        }
    }

    /// <summary>
    ///     Message indicating that a WAMP message was received by the <see cref="WampSubscriber"/> actor.
    /// </summary>
    public class Received
    {
        /// <summary>
        ///     Create a new <see cref="Received"/> message.
        /// </summary>
        /// <param name="messageArguments">
        ///     The message arguments.
        /// </param>
        public Received(object[] messageArguments)
        {
            MessageArguments = ImmutableList.Create(messageArguments);
        }

        /// <summary>
        ///     The message arguments.
        /// </summary>
        public ImmutableList<object> MessageArguments { get; }
    }

    /// <summary>
    ///     Request from a subscription owner to a <see cref="WampSubscriber"/> requesting termination of the subscription.
    /// </summary>
    public class Unsubscribe
    {
        /// <summary>
        ///     The singleton instance of the <see cref="Unsubscribe"/> message.
        /// </summary>
        public static readonly Unsubscribe Instance = new Unsubscribe();

        /// <summary>
        ///     Create a new <see cref="Unsubscribe"/> message.
        /// </summary>
        Unsubscribe()
        {
        }
    }
}