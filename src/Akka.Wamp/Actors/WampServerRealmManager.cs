using Akka.Actor;
using System;
using WampSharp.V2.Realm;

namespace Akka.Wamp.Actors
{
    class WampServerRealmManager
        : ReceiveActor
    {
        readonly IWampHostedRealm _realm;

        public WampServerRealmManager(IWampHostedRealm realm)
        {
            if (realm == null)
                throw new ArgumentNullException(nameof(realm));

            _realm = realm;
        }
    }
}