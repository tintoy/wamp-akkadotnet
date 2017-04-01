using Akka.Actor;
using System;
using WampSharp.V2;

namespace Akka.Wamp.Actors
{
    using Messages;

    /// <summary>
    ///     Actor that publishes to a specific WAMP topic.
    /// </summary>
    class WampPublisher
        : WampOwnedComponentActor
    {
        /// <summary>
        ///     The sink for events to publish.
        /// </summary>
        readonly IObserver<IWampEvent>  _eventSink;

        /// <summary>
        ///     The name of the topic to which events will be published.
        /// </summary>
        readonly string                 _topicName;

        /// <summary>
        ///     Cancellation for the activation timeout message.
        /// </summary>
        ICancelable                     _activationTimeout;

        /// <summary>
        ///     Create a new <see cref="WampPublisher"/> actor.
        /// </summary>
        /// <param name="eventSink">
        ///     The sink for events to publish.
        /// </param>
        /// <param name="topicName">
        ///     The name of the topic to which events will be published.
        /// </param>
        /// <param name="owner">
        ///     The actor that owns the subscriber.
        /// </param>
        public WampPublisher(IObserver<IWampEvent> eventSink, string topicName, IActorRef owner)
            : base(owner)
        {
            if (eventSink == null)
                throw new ArgumentNullException(nameof(eventSink));
            
            if (String.IsNullOrWhiteSpace(topicName))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'topicName'.", nameof(topicName));

            _eventSink = eventSink;
            _topicName = topicName;

            Become(WaitingForActivation);
        }

        /// <summary>
        ///     Behaviour for when the publisher is active.
        /// </summary>
        protected override void Active()
        {
            Log.Debug("Publisher activated (events will be published to topic '{0}').", _topicName);

            Receive<PublishWampEvent>(publish =>
            {
                try
                {
                    _eventSink.OnNext(
                        publish.ToWampEvent()
                    );
                }
                catch (Exception ePublishEvent)
                {
                    NotifyError(ePublishEvent, WampOperation.SendEvent, "Failed to publish WAMP event.");
                }
            });
        }

        /// <summary>
        ///     Create <see cref="Props"/> for a new <see cref="WampPublisher"/> actor.
        /// </summary>
        /// <param name="eventSink">
        ///     The sink for events to publish.
        /// </param>
        /// <param name="topicName">
        ///     The name of the topic that the subscription applies to.
        /// </param>
        /// <param name="owner">
        ///     The actor that owns the subscription.
        /// </param>
        /// <returns>
        ///     The configured <see cref="Props"/>.
        /// </returns>
        public static Props Create(IObserver<IWampEvent> eventSink, string topicName, IActorRef owner)
        {
            return Props.Create(
                () => new WampPublisher(eventSink, topicName, owner)
            );
        }
    }
}