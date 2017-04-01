using Akka.Actor;
using System;

namespace Akka.Wamp.Messages
{
    using System.Collections.Immutable;
    using Actors;

    /// <summary>
    ///     Request to the <see cref="WampServerRealmManager"/> actor for creation of a subscription to a topic.
    /// </summary>
    class CreateSubscription
        : WampRealmCommand
    {
        /// <summary>
        ///     Create a new <see cref="CreateSubscription"/> message.
        /// </summary>
        /// <param name="realmName">
        ///     The name of the WAMP realm where the subscription will be hosted.
        /// </param>
        /// <param name="topicName">
        ///     The name of the WAMP topic to which the subscription will apply.
        /// </param>
        /// <param name="owner">
        ///     The actor that will own the subscription.
        /// </param>
        /// <param name="argumentTypes">
        ///     The types of arguments expected in each message.
        /// </param>
        public CreateSubscription(string realmName, string topicName, IActorRef owner, params Type[] argumentTypes)
            : base(realmName)
        {
            if (String.IsNullOrWhiteSpace(topicName))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'topicName'.", nameof(topicName));
            
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));

            if (argumentTypes == null)
                throw new ArgumentNullException(nameof(argumentTypes));
            
            if (argumentTypes.Length == 0)
                throw new ArgumentException("Must specify at least one message argument type.", nameof(argumentTypes));
            
            TopicName = topicName;
            Owner = owner;
            ArgumentTypes = ImmutableList.Create(argumentTypes);
        }

        /// <summary>
        ///     The name of the WAMP topic to which the subscription will apply.
        /// </summary>
        public string TopicName { get; }

        /// <summary>
        ///     The actor that will own the subscription.
        /// </summary>
        public IActorRef Owner { get; }

        /// <summary>
        ///     The types of arguments expected in each message.
        /// </summary>
        public ImmutableList<Type> ArgumentTypes { get; }
    }

    /// <summary>
    ///     Response from the <see cref="WampServerRealmManager"/> actor indicating that a subscription has been created.
    /// </summary>
    public class SubscriptionCreated
    {
        /// <summary>
        ///     Create a new <see cref="SubscriptionCreated"/> message.
        /// </summary>
        /// <param name="topicName">
        ///     The name of the WAMP topic to which the subscription applies.
        /// </param>
        /// <param name="subscriber">
        ///     The actor that represents the subscription.
        /// </param>
        public SubscriptionCreated(string topicName, IActorRef subscriber)
        {
            if (String.IsNullOrWhiteSpace(topicName))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'topicName'.", nameof(topicName));
            
            if (subscriber == null)
                throw new ArgumentNullException(nameof(subscriber));
            
            TopicName = topicName;
            Subscriber = subscriber;
        }

        /// <summary>
        ///     The name of the WAMP topic to which the subscription applies.
        /// </summary>
        public string TopicName { get; }

        /// <summary>
        ///     The actor that represents the subscription.
        /// </summary>
        public IActorRef Subscriber { get; }
    }
}