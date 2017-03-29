using Akka.Actor;
using System;
using System.Threading.Tasks;

namespace Akka.Wamp
{
    using Messages;

    /// <summary>
    ///     The WAMP extension for Akka.NET.
    /// </summary>
    public sealed class WampApi
        : IExtension
    {
        /// <summary>
        ///     Create a new <see cref="Wamp"/> extension.
        /// </summary>
        /// <param name="system">
        ///     The <see cref="ActorSystem"/> being extended.
        /// </param>
        /// <param name="manager">
        ///     The top-level management actor for WAMP functionality.
        /// </param>
        internal WampApi(ActorSystem system, IActorRef manager)
        {
            if (system == null)
                throw new ArgumentNullException(nameof(system));
            
            if (manager == null)
                throw new ArgumentNullException(nameof(manager));

            System = system;
            Manager = manager;
        }

        /// <summary>
		///		The actor system extended by the API.
		/// </summary>
		
		public ActorSystem System	{ get; }

		/// <summary>
		///		A reference to the root Rx-integration management actor.
		/// </summary>
		
		internal IActorRef Manager	{ get; }

        /// <summary>
        ///     Get or create the actor that manages a WAMP router for the specified base address.
        /// </summary>
        /// <param name="baseAddress">
        ///     The base address for the WAMP end-point.
        /// </param>
        /// <returns>
        ///     A reference to the management actor.
        /// </returns>
        public async Task<IActorRef> CreateWampRouter(Uri baseAddress)
        {
            if (baseAddress == null)
                throw new ArgumentNullException(nameof(baseAddress));

            if (baseAddress.Scheme != "ws")
                throw new ArgumentException($"Unsupported URI scheme '{baseAddress.Scheme}' (must be 'ws').", nameof(baseAddress));
            
            object response = await Manager.Ask<object>(
                new CreateWampRouter(baseAddress)
            );

            switch (response)
            {
                case WampRouterCreated created:
                {
                    return created.Manager;
                }
                case Failure failure:
                {
                    throw new InvalidOperationException(
                        $"Failed to create a WAMP router for '{baseAddress}'.",
                        failure.Exception
                    );
                }
                default:
                {
                    throw new InvalidOperationException($"Received unexpected response '{response.GetType().FullName}' from WAMP manager.");
                }
            }
        }
    }
}