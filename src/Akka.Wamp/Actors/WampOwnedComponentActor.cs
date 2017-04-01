using Akka.Actor;
using System;

namespace Akka.Wamp.Actors
{
    using Messages;

    /// <summary>
    ///     The base class for WAMP-related actors that represent components owned by other actors.
    /// </summary>
    abstract class WampOwnedComponentActor
        : WampActor
    {
        /// <summary>
        ///     The default period of time within which a component's owner has to activate it.
        /// </summary>
        public static readonly TimeSpan DefaultActivationTimeout = TimeSpan.FromSeconds(5);

        /// <summary>
        ///     Cancellation for the activation timeout message.
        /// </summary>
        ICancelable _activationTimeout;

        /// <summary>
        ///     Create a new <see cref="WampActor"/>.
        /// </summary>
        protected WampOwnedComponentActor(IActorRef owner)
        {
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));
            
            Owner = owner;
        }

        /// <summary>
        ///     The actor that owns the component.
        /// </summary>
        protected IActorRef Owner { get; }

        /// <summary>
        ///     Enable error notifications?
        /// </summary>
        protected bool AreErrorNotificationsEnabled { get; set; }

        /// <summary>
        ///     Behaviour when the component is waiting for its owner to activate it.
        /// </summary>
        /// <remarks>
        ///     If the owner does not activate the component within the timeout period, the actor will be stopped.
        /// </remarks>
        protected virtual void WaitingForActivation()
        {
            Log.Debug("Created, waiting {0} for activation from owner '{1]'.",
                DefaultActivationTimeout, Owner.Path
            );

            _activationTimeout = Context.ScheduleSelfMessageCancelable(
                delay: DefaultActivationTimeout,
                message: ActivationTimeout.Instance
            );
            Receive<ActivationTimeout>(_ =>
            {
                Log.Debug("Timed out after waiting {0} for activation.",
                    DefaultActivationTimeout
                );

                Context.Stop(Self);
            });

            Receive<Activate>(activate =>
            {
                _activationTimeout?.Cancel();
                _activationTimeout = null;

                AreErrorNotificationsEnabled = activate.ErrorNotifications;

                Become(Active);
            });
        }

        /// <summary>
        ///     Behaviour for when the component is active.
        /// </summary>
        protected abstract void Active();

        /// <summary>
        ///     Notify the owner of an error encountered by the component (if error notifications are enabled).
        /// </summary>
        /// <param name="exception">
        ///     An <see cref="Exception"/> representing the error.
        /// </param>
        /// <param name="operation">
        ///     A <see cref="WampOperation"/> value representing the type of operation taking place when the error was encountered.
        /// </param>
        /// <param name="messageOrFormat">
        ///     An optional error message or message-format to be logged.
        /// 
        ///     If not specified, the exception message will be used.
        /// </param>
        /// <param name="formatArguments">
        ///     Optional message-format arguments.
        /// </param>
        protected virtual void NotifyError(Exception exception, WampOperation operation, string messageOrFormat = null, params object[] formatArguments)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            string message = messageOrFormat;
            if (!String.IsNullOrWhiteSpace(message))
            {
                // Handle the case where the message is not format-style (in which case we'd want to ignore things like braces).
                if (formatArguments.Length > 0)
                    message = String.Format(message, formatArguments);
            }
            else
                message = exception.Message;
            
            Log.Error(exception, message);

            if (!AreErrorNotificationsEnabled)
                return;

            if (!(exception is AkkaWampException))
            {
                exception = new AkkaWampException(message,
                    innerException: exception
                );
            }

            Owner.Tell(
                new WampError(exception, operation)
            );
        }
    }
}
