using Akka.Actor;
using System;
using System.Threading.Tasks;

namespace Akka.Wamp
{
    using Actors;
    using Messages;

    /// <summary>
    ///     The WAMP extension for Akka.NET.
    /// </summary>
    public sealed class WampApi
        : IExtension
    {
        /// <summary>
        ///     The default timeout period for commands to WAMP management actors.
        /// </summary>
        public static TimeSpan DefaultCommandTimeout = TimeSpan.FromSeconds(10);

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
        ///     Get or create the actor that manages a WAMP server for the specified base address.
        /// </summary>
        /// <param name="baseAddress">
        ///     The base address for the WAMP end-point.
        /// </param>
        /// <returns>
        ///     A reference to the router's management actor.
        /// </returns>
        public async Task<IActorRef> CreateWampServer(Uri baseAddress)
        {
            if (baseAddress == null)
                throw new ArgumentNullException(nameof(baseAddress));

            if (baseAddress.Scheme != "ws")
                throw new ArgumentException($"Unsupported URI scheme '{baseAddress.Scheme}' (must be 'ws').", nameof(baseAddress));
            
            var routerCreated = await Manager.Ask<WampManager.WampServerCreated>(
                new WampManager.CreateWampServer(baseAddress)
            );

            return routerCreated.Manager;
        }

        /// <summary>
        ///     Initiate startup for a WAMP server.
        /// </summary>
        /// <param name="serverManager">
        ///     The server manager.
        /// </param>
        public async Task StartWampServer(IActorRef serverManager)
        {
            if (serverManager == null)
                throw new ArgumentNullException(nameof(serverManager));
            
            await serverManager.Ask<Status.Success>(
                message: Start.Instance,
                timeout: DefaultCommandTimeout
            );
        }

        /// <summary>
        ///     Initiate shutdown for a WAMP server.
        /// </summary>
        /// <param name="serverManager">
        ///     The server manager.
        /// </param>
        public async Task StopWampServer(IActorRef serverManager)
        {
            if (serverManager == null)
                throw new ArgumentNullException(nameof(serverManager));
            
            await serverManager.Ask<Status.Success>(
                message: Stop.Instance,
                timeout: DefaultCommandTimeout
            );
        }

        /// <summary>
        ///     Get the actor that manages the specified server-side WAMP realm.
        /// </summary>
        /// <param name="serverManager">
        ///     The actor that manages the WAMP server where the realm is hosted.
        /// </param>
        /// <param name="realmName">
        ///     The name of the target realm.
        /// </param>
        /// <returns>
        ///     An <see cref="IActorRef"/> representing the realm management actor.
        /// </returns>
        public async Task<IActorRef> GetWampServerRealm(IActorRef serverManager, string realmName)
        {
            if (serverManager == null)
                throw new ArgumentNullException(nameof(serverManager));

            var realm = await serverManager.Ask<WampServerManager.Realm>(
                message: new WampServerManager.GetRealm(realmName),
                timeout: DefaultCommandTimeout
            );

            return realm.Manager;
        }

        /// <summary>
        ///     Subscribe to a WAMP topic.
        /// </summary>
        /// <param name="serverManager">
        ///     The actor that manages the target WAMP server.
        /// </param>
        /// <param name="realmName">
        ///     The name of the target WAMP realm.
        /// </param>
        /// <param name="topicName">
        ///     The name of the target WAMP topic.
        /// </param>
        /// <param name="parameterTypes">
        ///     The expected parameter types for the topic's events.
        /// </param>
        /// <remarks>
        ///     The owner must send an <see cref="Activate"/> message to the subscriber within 5 seconds or the subscriber will be terminated.
        /// </remarks>
        public void Subscribe(IActorRef serverManager, string realmName, string topicName, params Type[] parameterTypes)
        {
            Subscribe(
                serverManager: serverManager,
                realmName: realmName,
                topicName: topicName,
                owner: null,
                parameterTypes: parameterTypes
            );
        }

        /// <summary>
        ///     Subscribe to a WAMP topic.
        /// </summary>
        /// <param name="serverManager">
        ///     The actor that manages the target WAMP server.
        /// </param>
        /// <param name="realmName">
        ///     The name of the target WAMP realm.
        /// </param>
        /// <param name="topicName">
        ///     The name of the target WAMP topic.
        /// </param>
        /// <param name="owner">
        ///     The actor that will own the subscription and receive its events.
        /// </param>
        /// <param name="parameterTypes">
        ///     The expected parameter types for the topic's events.
        /// </param>
        /// <remarks>
        ///     The owner will receive <see cref="SubscriptionCreated"/>, and must reply with <see cref="Activate"/> within 5 seconds or the subscriber will be terminated.
        /// </remarks>
        public void Subscribe(IActorRef serverManager, string realmName, string topicName, IActorRef owner, params Type[] parameterTypes)
        {
            if (serverManager == null)
                throw new ArgumentNullException(nameof(serverManager));
            
            if (String.IsNullOrWhiteSpace(realmName))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'realmName'.", nameof(realmName));

            if (String.IsNullOrWhiteSpace(topicName))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'topicName'.", nameof(topicName));
            
            if (owner == null)
                owner = EnsureSelf();
            
            if (parameterTypes == null)
                throw new ArgumentNullException(nameof(parameterTypes));

            serverManager.Tell(
                message: new Messages.CreateSubscription(realmName, topicName, owner, parameterTypes)
            );
        }

        /// <summary>
        ///     Request that a subscriber terminate its subscription.
        /// </summary>
        /// <param name="subscriber">
        ///     The subscriber actor.
        /// </param>
        public void Unsubscribe(IActorRef subscriber)
        {
            if (subscriber == null)
                throw new ArgumentNullException(nameof(subscriber));

            subscriber.Tell(Messages.Unsubscribe.Instance);
        }

        /// <summary>
        ///     Ensure that a self-reference to the current actor is available.
        /// </summary>
        /// <returns>
        ///     The <see cref="IActorRef"/>.
        /// </returns>
        static IActorRef EnsureSelf()
        {
            IActorRef self = ActorCell.GetCurrentSelfOrNoSender();
            if (self.IsNobody())
                throw new InvalidOperationException("Cannot determine Self for the current actor (this method can only be called from within a valid actor cell).");

            return self;
        }
    }
}