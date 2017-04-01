using Akka.Actor;
using System;

namespace Akka.Wamp
{
    /// <summary>
	///		Extension methods for <see cref="ActorSystem"/>.
	/// </summary>
	public static class ActorSystemExtensions
	{
		/// <summary>
		///		Get the WAMP integration API for the actor system.
		/// </summary>
		/// <param name="system">
		///		The actor system.
		/// </param>
		/// <returns>
		///		The WAMP integration API.
		/// </returns>
		public static WampApi Wamp(this ActorSystem system)
		{
			if (system == null)
				throw new ArgumentNullException(nameof(system));

			return WampApiProvider.Instance.Apply(system);
		}

		/// <summary>
		/// 	Get the WAMP integration API for the actor's local context.
		/// </summary>
		/// <param name="context">
		/// 	The local actor context.
		/// </param>
		/// <returns>
		/// 	The WAMP integration API.
		/// </returns>
		public static WampContextApi Wamp(this IUntypedActorContext context)
		{
			if (context == null)
				throw new ArgumentNullException(nameof(context));
			
			return new WampContextApi(context);
		}
	}
}
