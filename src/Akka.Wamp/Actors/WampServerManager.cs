using Akka.Actor;
using System;
using System.Collections.Generic;

namespace Akka.Wamp.Actors
{
    using Server;
    using WampSharp.V2.Realm;

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

        void Running()
        {
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

                Become(Stopped);
            });

            Receive<GetRealm>(getRealm =>
            {
                IActorRef realmManager;
                if (!_realmManagers.TryGetValue(getRealm.Name, out realmManager))
                {
                    IWampHostedRealm realm = _server.GetRealm(getRealm.Name);
                    realmManager = Context.ActorOf(
                        Props.Create<WampServerRealmManager>(realm),
                        name: $"realm-{getRealm.Name}" // TODO: Ensure name contains only safe characters
                    );
                    _realmManagers.Add(getRealm.Name, realmManager);
                }

                Sender.Tell(
                    new Realm(realmManager)
                );
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

        protected override void PostStop()
        {
            if (_server != null)
            {
                _server.Dispose();
                _server = null;
            }
        }

        public class Start
        {
            public static readonly Start Instance = new Start();

            Start()
            {
            }
        }

        public class Stop
        {
            public static readonly Stop Instance = new Stop();

            Stop()
            {
            }
        }

        public class GetRealm
        {
            public GetRealm(string name)
            {
                if (String.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'name'.", nameof(name));
                
                Name = name;
            }

            public string Name { get; }
        }

        public class Realm
        {
            public Realm(IActorRef manager)
            {
                if (manager == null)
                    throw new ArgumentNullException(nameof(manager));

                Manager = manager;
            }

            public IActorRef Manager { get; }
        }
    }
}