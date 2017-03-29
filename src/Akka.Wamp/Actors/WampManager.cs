using Akka.Actor;
using System;
using System.Collections.Generic;

namespace Akka.Wamp.Actors
{
    using Server;

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
        ///     WAMP hosts, keyed by end-point URI.
        /// </summary>
        readonly Dictionary<Uri, WampServer> _hosts = new Dictionary<Uri, WampServer>();

        /// <summary>
        ///     Create a new <see cref="WampManager"/> actor.
        /// </summary>
        public WampManager()
        {
            
        }
    }
}
