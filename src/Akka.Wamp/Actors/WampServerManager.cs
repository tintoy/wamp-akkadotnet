using Akka.Actor;
using Akka.Event;
using System;
using System.Collections.Generic;
using WampSharp.V2.Realm;

namespace Akka.Wamp.Actors
{
    using Messages;
    using Server;

    /// <summary>
    ///     Actor that manages a WAMP server.
    /// </summary>
    class WampServerManager
        : ReceiveActor
    {
        Uri _baseUri;

        WampServer _server;

        Dictionary<string, IActorRef> _realmManagers = new Dictionary<string, IActorRef>();

        public WampServerManager(Uri baseUri)
        {
            if (baseUri == null)
                throw new ArgumentNullException(nameof(baseUri));
            
            _baseUri = baseUri;
            _server = new WampServer(_baseUri);

            Become(Stopped);
        }

        /// <summary>
        ///     The logging facility.
        /// </summary>
        ILoggingAdapter Log { get; } = Logging.GetLogger(Context);

        /// <summary>
        ///     Behaviour for when the server is running.
        /// </summary>
        void Running()
        {
            Receive<GetRealm>(getRealm =>
            {
                IActorRef realmManager = GetOrCreateRealm(getRealm.Realm);

                Sender.Tell(
                    new Realm(getRealm.Realm, realmManager)
                );
            });

            // Forward all realm-related commands to the management actor for the target realm.
            Receive<WampRealmCommand>(command =>
            {
                IActorRef realmManager = GetOrCreateRealm(command.RealmName);
                realmManager.Forward(command);
            });

            Receive<Stop>(stop =>
            {
                try
                {
                    if (_server.IsRunning)
                        _server.Stop();
                }
                catch (Exception eStartServer)
                {
                    Sender.Tell(new Failure
                    {
                        // TODO: Custom exception type.
                        Exception = new InvalidOperationException(
                            $"Failed to start WAMP server listening on '{_baseUri}'.",
                            innerException: eStartServer
                        )
                    });

                    return;
                }

                Sender.Tell(
                    new Status.Success(null)
                );

                Become(Stopped);
            });

            Receive<Start>(start =>
            {
                Sender.Tell(new Failure
                {
                    Exception = new InvalidOperationException(
                        $"WAMP server is already running."
                    )
                });
            });
        }

        /// <summary>
        ///     Behaviour for when the server is stopped.
        /// </summary>
        void Stopped()
        {
            Receive<Start>(start =>
            {
                try
                {
                    _server.Start();
                }
                catch (Exception eStartServer)
                {
                    Sender.Tell(new Failure
                    {
                        // TODO: Custom exception type.
                        Exception = new InvalidOperationException(
                            $"Failed to start WAMP server listening on '{_baseUri}'.",
                            innerException: eStartServer
                        )
                    });

                    return;
                }

                Sender.Tell(
                    new Status.Success(null)
                );

                Become(Running);
            });

            Failure notRunning = new Failure
            {
                Exception = new InvalidOperationException(
                    $"WAMP server is not running."
                )
            };

            Receive<Stop>(stop =>
            {
                Sender.Tell(notRunning);
            });
            Receive<GetRealm>(getRealm =>
            {
                Sender.Tell(notRunning);
            });
        }

        protected override void PostStop()
        {
            if (_server != null)
            {
                _server.Dispose();
                _server = null;
            }
        }

        /// <summary>
        ///     Get or create the management actor for the specified WAMP realm.
        /// </summary>
        /// <param name="name">
        ///     The name of the target WAMP realm.
        /// </param>
        /// <returns>
        ///     An <see cref="IActorRef"/> representing the realm's top-level management actor.
        /// </returns>
        IActorRef GetOrCreateRealm(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'name'.", nameof(name));

            IActorRef realmManager;
            if (!_realmManagers.TryGetValue(name, out realmManager))
            {
                IWampHostedRealm realm = _server.GetRealm(name);
                realmManager = Context.ActorOf(
                    Props.Create<WampServerRealmManager>(realm),
                    name: $"realm-{name}" // TODO: Ensure name contains only safe characters
                );
                _realmManagers.Add(name, realmManager);
            }

            return realmManager;
        }

        /// <summary>
        ///     Request the management actor for a WAMP realm.
        /// </summary>
        public class GetRealm
        {
            /// <summary>
            ///     Create a new <see cref="GetRealm"/> message.
            /// </summary>
            /// <param name="name">
            ///     The name of the target WAMP realm.
            /// </param>
            public GetRealm(string name)
            {
                if (String.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'name'.", nameof(name));
                
                Realm = name;
            }

            /// <summary>
            ///     The name of the target WAMP realm.
            /// </summary>
            public string Realm { get; }
        }

        /// <summary>
        ///     The management actor for a WAMP realm.
        /// </summary>
        public class Realm
        {
            /// <summary>
            ///     Create a new <see cref="Realm"/> message.
            /// </summary>
            /// <param name="name">
            ///     The name of the target WAMP realm.
            /// </param>
            /// <param name="manager">
            ///     The management actor for the target realm.
            /// </param>
            public Realm(string name, IActorRef manager)
            {
                if (String.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'name'.", nameof(name));

                if (manager == null)
                    throw new ArgumentNullException(nameof(manager));

                Name = name;
                Manager = manager;
            }

            /// <summary>
            ///     The name of the target WAMP realm.
            /// </summary>
            public string Name { get; }
            
            /// <summary>
            ///     The management actor for the target realm.
            /// </summary>
            public IActorRef Manager { get; }
        }
    }
}