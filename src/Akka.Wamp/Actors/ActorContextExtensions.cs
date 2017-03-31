using Akka.Actor;
using System;

namespace Akka.Wamp.Actors
{
    /// <summary>
    ///     Extension methods for <see cref="IActorContext"/> / <see cref="IUntypedActorContext"/>.
    /// </summary>
    static class ActorContextExtensions
    {
        /// <summary>
        ///     Schedule a message from the current actor to itself.
        /// </summary>
        /// <param name="context">
        ///     The actor context.
        /// </param>
        /// <param name="message">
        ///     The message to schdule.
        /// </param>
        /// <param name="delay">
        ///     The delay before the message is sent.
        /// </param>
        public static void ScheduleSelfMessage(this IUntypedActorContext context, object message, TimeSpan delay)
        {
            context.System.Scheduler.ScheduleTellOnce(
                delay: delay,
                receiver: context.Self,
                message: message,
                sender: context.Self
            );
        }

        /// <summary>
        ///     Schedule a message from the current actor to itself (with cancellation support).
        /// </summary>
        /// <param name="context">
        ///     The actor context.
        /// </param>
        /// <param name="message">
        ///     The message to schdule.
        /// </param>
        /// <param name="delay">
        ///     The delay before the message is sent.
        /// </param>
        /// <returns>
        ///     An <see cref="ICancelable"/> that can be used to cancel message delivery.
        /// </returns>
        public static ICancelable ScheduleSelfMessageCancelable(this IUntypedActorContext context, object message, TimeSpan delay)
        {
            return context.System.Scheduler.ScheduleTellOnceCancelable(
                delay: delay,
                receiver: context.Self,
                message: message,
                sender: context.Self
            );
        }
    }
}