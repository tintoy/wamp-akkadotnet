using Akka.Actor;
using Akka.Event;
using System;
using WampSharp.V2;

namespace Akka.Wamp.Actors
{
    using System.Collections.Immutable;
    using Messages;

    /// <summary>
    ///     Actor that represents a specific subscription to a WAMP topic, forwarding messages to the owning actor.
    /// </summary>
    /// <remarks>
    ///     TODO: Add logging.
    /// </remarks>
    class WampSubscriber
        : ReceiveActor
    {
        /// <summary>
        ///     The default period of time that the owner has to activate the subscription.
        /// </summary>
        public static readonly TimeSpan DefaultActivationTimeout = TimeSpan.FromSeconds(5);

        /// <summary>
        ///     The sequence of WAMP events to monitor.
        /// </summary>
        readonly IObservable<IWampSerializedEvent>    _eventSource;

        /// <summary>
        ///     The name of the topic that the subscription applies to.
        /// </summary>
        readonly string                     _topicName;

        /// <summary>
        ///     The actor that owns the subscription.
        /// </summary>
        readonly IActorRef                  _owner;

        /// <summary>
        ///     The types of arguments expected in event messages.
        /// </summary>
        readonly ImmutableList<Type>        _argumentTypes;

        /// <summary>
        ///     Cancellation for the subscription-activation timeout message.
        /// </summary>
        ICancelable                         _activationTimeout;

        /// <summary>
        ///     An <see cref="IDisposable"/> representing the subscription.
        /// </summary>
        IDisposable                         _subscription;

        /// <summary>
        ///     Create a new <see cref="WampSubscriber"/> actor.
        /// </summary>
        /// <param name="eventSource">
        ///     The sequence of WAMP events to monitor.
        /// </param>
        /// <param name="topicName">
        ///     The name of the topic that the subscription applies to.
        /// </param>
        /// <param name="owner">
        ///     The actor that owns the subscription.
        /// </param>
        /// <param name="argumentTypes">
        ///     The types of arguments expected in event messages.
        /// </param>
        public WampSubscriber(IObservable<IWampSerializedEvent> eventSource, string topicName, IActorRef owner, ImmutableList<Type> argumentTypes)
        {
            if (eventSource == null)
                throw new ArgumentNullException(nameof(eventSource));
            
            if (String.IsNullOrWhiteSpace(topicName))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'topicName'.", nameof(topicName));

            if (owner == null)
                throw new ArgumentNullException(nameof(owner));

            _eventSource = eventSource;
            _topicName = topicName;
            _owner = owner;
            _argumentTypes = argumentTypes;

            Become(WaitingForOwner);
        }

        /// <summary>
        ///     The logging facility.
        /// </summary>
        ILoggingAdapter Log { get; } = Logging.GetLogger(Context);

        /// <summary>
        ///     Behaviour when the actor is waiting for the owner to activate the subscription.
        /// </summary>
        /// <remarks>
        ///     If the owner does not activate the subscription within the timeout period, the actor will be stopped.
        /// </remarks>
        void WaitingForOwner()
        {
            Log.Debug("Subscriber created, waiting {0} for activation from owner '{1]'.",
                DefaultActivationTimeout, _owner
            );

            _activationTimeout = Context.ScheduleSelfMessageCancelable(
                delay: DefaultActivationTimeout,
                message: SubscriptionActivationTimeout.Instance
            );
            Receive<SubscriptionActivationTimeout>(_ =>
            {
                Log.Debug("Subscriber timed out after waiting {0} for activation.",
                    DefaultActivationTimeout
                );

                Context.Stop(Self);
            });

            Receive<ActivateSubscription>(_ =>
            {
                Become(Active);
            });
        }

        /// <summary>
        ///     Behaviour when the subscription is active.
        /// </summary>
        void Active()
        {
            Log.Debug("Subscriber activated.");

            if (_activationTimeout != null)
            {
                _activationTimeout.Cancel();
                _activationTimeout = null;
            }

            _subscription = _eventSource.Subscribe(wampEvent =>
            {
                // TODO: Catch deserialisation exception(s) then log and forward to subscriber.
                object[] arguments = new object[_argumentTypes.Count];
                for (int index = 0; index < arguments.Length; index++)
                {
                    arguments[0] = wampEvent.Arguments[index].Deserialize(
                        type: _argumentTypes[index]
                    );
                }

                _owner.Tell(
                    new Received(arguments)
                );
            });

            Receive<Unsubscribe>(_ =>
            {
                Log.Debug("Subscription terminated by '{0}'.", Sender.Path);

                Context.Stop(Self);
            });
        }

        /// <summary>
        ///     Called when the actor is stopped.
        /// </summary>
        protected override void PostStop()
        {
            if (_subscription != null)
            {
                _subscription.Dispose();
                _subscription = null;
            }
        }

        /// <summary>
        ///     Create <see cref="Props"/> for a new <see cref="WampSubscriber"/> actor.
        /// </summary>
        /// <param name="eventSource">
        ///     The sequence of WAMP events to monitor.
        /// </param>
        /// <param name="topicName">
        ///     The name of the topic that the subscription applies to.
        /// </param>
        /// <param name="owner">
        ///     The actor that owns the subscription.
        /// </param>
        /// <param name="argumentTypes">
        ///     The expected types of arguments for each message.
        /// </param>
        /// <returns>
        ///     The configured <see cref="Props"/>.
        /// </returns>
        public static Props Create(IObservable<IWampSerializedEvent> eventSource, string topicName, IActorRef owner, ImmutableList<Type> argumentTypes)
        {
            return Props.Create<WampSubscriber>(eventSource, topicName, owner, argumentTypes);
        }
    }
}