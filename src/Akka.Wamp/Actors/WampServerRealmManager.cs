using Akka.Actor;
using System;
using System.Collections.Generic;
using WampSharp.V2.Realm;
using WampSharp.V2;

namespace Akka.Wamp.Actors
{
    using Messages;

    /// <summary>
    ///     Actor that manages a server-side WAMP realm.
    /// </summary>
    class WampServerRealmManager
        : ReceiveActor
    {
        /// <summary>
        ///     The WAMP realm managed by the actor.
        /// </summary>
        readonly IWampHostedRealm                   _realm;

        /// <summary>
        ///     Rx subjects for WAMP topics, keyed by topic name.
        /// </summary>
        readonly Dictionary<string, IWampSubject>   _topicSubjects = new Dictionary<string, IWampSubject>();

        /// <summary>
        ///     Create a new <see cref="WampServerRealmManager"/> actor.
        /// </summary>
        /// <param name="realm">
        ///     The WAMP realm to be managed by the actor.
        /// </param>
        public WampServerRealmManager(IWampHostedRealm realm)
        {
            if (realm == null)
                throw new ArgumentNullException(nameof(realm));

            _realm = realm;

            Become(Active);
        }

        /// <summary>
        ///     Standard behaviour for the actor.
        /// </summary>
        void Active()
        {
            Receive<CreateSubscription>(create =>
            {
                IWampSubject topicSubject;
                if (!_topicSubjects.TryGetValue(create.TopicName, out topicSubject))
                {
                    topicSubject = _realm.Services.GetSubject(create.TopicName);
                    _topicSubjects.Add(create.TopicName, topicSubject);
                }

                IActorRef subscriber = Context.ActorOf(
                    WampSubscriber.Create(topicSubject, create.TopicName, create.Owner, create.ArgumentTypes)
                );
                create.Owner.Tell(new SubscriptionCreated(
                    topicName: create.TopicName,
                    subscriber: subscriber
                ));
            });
        }
    }
}