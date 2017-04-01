using Akka.Actor;
using Akka.Event;
using System;
using WampSharp.V2;

namespace Akka.Wamp.Actors
{
    using Messages;

    /// <summary>
    ///     Actor that publishes to a specific WAMP topic.
    /// </summary>
    class WampPublisher
        : ReceiveActor
    {
        /// <summary>
        ///     The default period of time that the owner has to activate the subscription.
        /// </summary>
        public static readonly TimeSpan DefaultActivationTimeout = TimeSpan.FromSeconds(5);

        /// <summary>
        ///     The sink for events to publish.
        /// </summary>
        readonly IObserver<IWampEvent>  _eventSink;

        /// <summary>
        ///     The name of the topic to which events will be published.
        /// </summary>
        readonly string                 _topicName;

        /// <summary>
        ///     The actor that owns the subscriber.
        /// </summary>
        readonly IActorRef              _owner;

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
        {
            if (eventSink == null)
                throw new ArgumentNullException(nameof(eventSink));
            
            if (String.IsNullOrWhiteSpace(topicName))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'topicName'.", nameof(topicName));

            if (owner == null)
                throw new ArgumentNullException(nameof(owner));
            
            _eventSink = eventSink;
            _topicName = topicName;
            _owner = owner;
        }

        /// <summary>
        ///     The logging facility.
        /// </summary>
        ILoggingAdapter Log { get; } = Logging.GetLogger(Context);

        /// <summary>
        ///     Behaviour when the publisher is waiting for the owner to activate it.
        /// </summary>
        /// <remarks>
        ///     If the owner does not activate the publisher within the timeout period, the actor will be stopped.
        /// </remarks>
        void WaitingForActivation()
        {
            Log.Debug("Publisher created, waiting {0} for activation from owner '{1]'.",
                DefaultActivationTimeout, _owner
            );

            _activationTimeout = Context.ScheduleSelfMessageCancelable(
                delay: DefaultActivationTimeout,
                message: ActivationTimeout.Instance
            );
            Receive<ActivationTimeout>(_ =>
            {
                Log.Debug("Publisher timed out after waiting {0} for activation.",
                    DefaultActivationTimeout
                );

                Context.Stop(Self);
            });

            Receive((Wamp.Activate _) =>
            {
                Become(this.Active);
            });
        }

        /// <summary>
        ///     Behaviour when the publisher is active.
        /// </summary>
        void Active()
        {
            Log.Debug("Publisher activated (events will be published to topic '{0}').", _topicName);

            Receive<PublishWampEvent>(publish =>
            {
                // TODO: Notify owner if this fails.

                _eventSink.OnNext(
                    publish.ToWampEvent()
                );
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