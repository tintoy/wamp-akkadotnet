using Akka.Actor;
using System;

namespace Akka.Wamp
{
    using Actors;

	/// <summary>
	///		Extension provider for WAMP integration.
	/// </summary>
	class WampApiProvider :
		ExtensionIdProvider<WampApi>
	{
		/// <summary>
		///		The singleton instance of the WAMP extension provider.
		/// </summary>
		public static readonly WampApiProvider Instance = new WampApiProvider();

		/// <summary>
		///		Create a new WAMP extension provider.
		/// </summary>
		WampApiProvider()
		{
		}

		/// <summary>
		///		Create an instance of the extension.
		/// </summary>
		/// <param name="system">
		///		The actor system being extended.
		/// </param>
		/// <returns>
		///		The extension.
		/// </returns>
		public override WampApi CreateExtension(ExtendedActorSystem system)
		{
			if (system == null)
				throw new ArgumentNullException(nameof(system));

			IActorRef manager = system.ActorOf(
				Props.Create<WampManager>(),
				name: WampManager.ActorName
			);

			return new WampApi(system, manager);
		}
	}
}