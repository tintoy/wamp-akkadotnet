using Akka.Actor;
using System;
using System.Collections.Generic;

namespace Akka.Wamp.Actors
{
    using Messages;

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
        readonly Dictionary<Uri, IActorRef> _routerManagers = new Dictionary<Uri, IActorRef>();

        /// <summary>
        ///     Create a new <see cref="WampManager"/> actor.
        /// </summary>
        public WampManager()
        {
            Receive<CreateWampRouter>(create =>
            {
                IActorRef routerManager;
                if (!_routerManagers.TryGetValue(create.BaseAddress, out routerManager))
                {
                    routerManager = Context.ActorOf(
                        Props.Create<WampServerManager>()
                    );
                    _routerManagers.Add(create.BaseAddress, routerManager);
                }

                Sender.Tell(
                    new WampRouterCreated(routerManager)
                );
            });
        }
    }
}
