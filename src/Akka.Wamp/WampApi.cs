using Akka.Actor;
using System;

namespace Akka.Wamp
{
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
    }
}