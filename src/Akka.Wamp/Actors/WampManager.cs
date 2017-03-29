using Akka.Actor;

namespace Akka.Wamp.Actors
{
    /// <summary>
    ///     The top-level management actor for WAMP functionality.
    /// </summary>
    class WampManager
        : ReceiveActor
    {
        /// <summary>
		///		The well-known name of the reactive management API actor.
		/// </summary>
		public static readonly string ActorName = "wamp-manager";

        /// <summary>
        ///     Create a new <see cref="WampManager"/> actor.
        /// </summary>
        public WampManager()
        {
        }
    }
}