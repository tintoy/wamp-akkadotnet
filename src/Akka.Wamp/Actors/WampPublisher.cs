using Akka.Actor;
using System;

namespace Akka.Wamp.Actors
{
    /// <summary>
    ///     Actor that publishes to a specific WAMP topic.
    /// </summary>
    class WampPublisher
        : ReceiveActor
    {
        readonly IActorRef _owner;
        readonly IDisposable _subscription;

        public class Publish
        {
            public object Message { get; }
        }
    }
}