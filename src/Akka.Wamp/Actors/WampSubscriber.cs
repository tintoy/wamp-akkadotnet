using Akka.Actor;
using Akka.Event;
using System;
using System.Collections.Immutable;
using WampSharp.V2;

namespace Akka.Wamp.Actors
{
    using Messages;

    /// <summary>
    ///     Actor that represents a specific subscription to a WAMP topic, forwarding messages to the owning actor.
    /// </summary>
    /// <remarks>
    ///     TODO: Add logging.
    /// </remarks>
    class WampSubscriber
        : WampOwnedComponentActor
    {
        /// <summary>
        ///     The sequence of WAMP events to monitor.
        /// </summary>
        readonly IObservable<IWampSerializedEvent>  _eventSource;

        /// <summary>
        ///     The name of the topic that the subscription applies to.
        /// </summary>
        readonly string                             _topicName;

        /// <summary>
        ///     The types of arguments expected in event messages.
        /// </summary>
        readonly ImmutableList<Type>                _argumentTypes;

        /// <summary>
        ///     Cancellation for the activation timeout message.
        /// </summary>
        ICancelable                                 _activationTimeout;

        /// <summary>
        ///     An <see cref="IDisposable"/> representing the subscription.
        /// </summary>
        IDisposable                                 _subscription;

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
            : base(owner)
        {
            if (eventSource == null)
                throw new ArgumentNullException(nameof(eventSource));
            
            if (String.IsNullOrWhiteSpace(topicName))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'topicName'.", nameof(topicName));

            _eventSource = eventSource;
            _topicName = topicName;
            _argumentTypes = argumentTypes;

            Become(WaitingForActivation);
        }

        /// <summary>
        ///     Behaviour when the subscription is active.
        /// </summary>
        protected override void Active()
        {
            Log.Debug("Subscriber activated.");

            _subscription = _eventSource.Subscribe(ProcessWampEvent);

            Receive<Unsubscribe>(_ =>
            {
                if (!Sender.IsNobody())
                    Log.Debug("Subscription terminated by '{0}'.", Sender.Path);
                else
                    Log.Debug("Subscription terminated.");

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
        ///     Process an incoming WAMP event.
        /// </summary>
        /// <param name="wampEvent">
        ///     The serialised WAMP event data.
        /// </param>
        void ProcessWampEvent(IWampSerializedEvent wampEvent)
        {
            if (wampEvent == null)
                throw new ArgumentNullException(nameof(wampEvent));
            
            ReceivedWampEvent notification;
            try
            {
                object[] arguments = DeserializeEventArguments(wampEvent);
                notification = new ReceivedWampEvent(arguments);
            }
            catch (Exception eDeserialiseWampEvent)
            {
                NotifyError(eDeserialiseWampEvent, WampOperation.ReceiveEvent, "Failed to deserialise WAMP event.");

                return;
            }
            
            Owner.Tell(notification);
        }

        /// <summary>
        ///     Deserialise the ordinal arguments for the specified WAMP event.
        /// </summary>
        /// <param name="wampEvent">
        ///     The serialised WAMP event.
        /// </param>
        /// <returns>
        ///     An array of event arguments.
        /// </returns>
        object[] DeserializeEventArguments(IWampSerializedEvent wampEvent)
        {
            if (wampEvent == null)
                throw new ArgumentNullException(nameof(wampEvent));
            
            object[] arguments = new object[_argumentTypes.Count];
            for (int index = 0; index < arguments.Length; index++)
            {
                arguments[0] = wampEvent.Arguments[index].Deserialize(
                    type: _argumentTypes[index]
                );
            }

            return arguments;
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
            return Props.Create(
                () => new WampSubscriber(eventSource, topicName, owner, argumentTypes)
            );
        }
    }
}