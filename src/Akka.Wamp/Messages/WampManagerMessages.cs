using Akka.Actor;
using System;

namespace Akka.Wamp.Messages
{
    /// <summary>
    ///     Request to the <see cref="Actors.WampManager"/> requesting the creation of a WAMP router.
    /// </summary>
    class CreateWampRouter
    {
        /// <summary>
        ///     Create a new <see cref="WampRouterCreated"/>.
        /// </summary>
        /// <param name="baseAddress">
        ///     The base address for the WAMP end-point.
        /// </param>
        public CreateWampRouter(Uri baseAddress)
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
    ///     Response from the <see cref="Actors.WampManager"/> indicating that a WAMP router has been created.
    /// </summary>
    class WampRouterCreated
    {
        /// <summary>
        ///     Create a new <see cref="WampRouterCreated"/>.
        /// </summary>
        /// <param name="wampRouterManager">
        ///     A reference to the actor that manages the WAMP router.
        /// </param>
        public WampRouterCreated(IActorRef wampRouterManager)
        {
            if (wampRouterManager == null)
                throw new ArgumentNullException(nameof(wampRouterManager));
            
            Manager = wampRouterManager;
        }

        /// <summary>
        ///     A reference to the actor that manages the WAMP router.
        /// </summary>
        public IActorRef Manager { get; }
    }
}