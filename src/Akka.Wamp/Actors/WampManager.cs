using Akka.Actor;
using Akka.Event;
using System;
using System.Collections.Generic;

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
        ///     WAMP servers, keyed by end-point URI.
        /// </summary>
        readonly Dictionary<Uri, IActorRef> _serverManagers = new Dictionary<Uri, IActorRef>();

        /// <summary>
        ///     Create a new <see cref="WampManager"/> actor.
        /// </summary>
        public WampManager()
        {
            Receive<CreateWampServer>(create =>
            {
                IActorRef serverManager;
                if (!_serverManagers.TryGetValue(create.BaseAddress, out serverManager))
                {
                    serverManager = Context.ActorOf(
                        WampServerManager.Create(create.BaseAddress)
                    );
                    _serverManagers.Add(create.BaseAddress, serverManager);
                }

                Sender.Tell(
                    new WampServerCreated(serverManager)
                );
            });
        }

        /// <summary>
        ///     The logging facility.
        /// </summary>
        ILoggingAdapter Log { get; } = Logging.GetLogger(Context);

        /// <summary>
        ///     Request to the <see cref="Actors.WampManager"/> requesting the creation of a WAMP server.
        /// </summary>
        public class CreateWampServer
        {
            /// <summary>
            ///     Create a new <see cref="WampServerCreated"/>.
            /// </summary>
            /// <param name="baseAddress">
            ///     The base address for the WAMP end-point.
            /// </param>
            public CreateWampServer(Uri baseAddress)
            {
                if (baseAddress == null)
                    throw new ArgumentNullException(nameof(baseAddress));
                
                BaseAddress = baseAddress;
            }

            /// <summary>
            ///     The base address for the WAMP end-point.
            /// </summary>
            public Uri BaseAddress { get; }
        }

        /// <summary>
        ///     Response from the <see cref="Actors.WampManager"/> indicating that a WAMP server has been created.
        /// </summary>
        public class WampServerCreated
        {
            /// <summary>
            ///     Create a new <see cref="WampServerCreated"/>.
            /// </summary>
            /// <param name="wampServerManager">
            ///     A reference to the actor that manages the WAMP server.
            /// </param>
            public WampServerCreated(IActorRef wampServerManager)
            {
                if (wampServerManager == null)
                    throw new ArgumentNullException(nameof(wampServerManager));
                
                Manager = wampServerManager;
            }

            /// <summary>
            ///     A reference to the actor that manages the WAMP server.
            /// </summary>
            public IActorRef Manager { get; }
        }
    }
}
