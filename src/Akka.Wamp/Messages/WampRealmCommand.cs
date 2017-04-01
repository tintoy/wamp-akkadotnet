using System;

namespace Akka.Wamp.Messages
{
    /// <summary>
    ///     The base class for realm commands.
    /// </summary>
    abstract class WampRealmCommand
    {
        /// <summary>
        ///     Create a new <see cref="WampRealmCommand"/>.
        /// </summary>
        /// <param name="realmName">
        ///     The name of the target WAMP realm.
        /// </param>
        protected WampRealmCommand(string realmName)
        {
            if (String.IsNullOrWhiteSpace(realmName))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'realm'.", nameof(realmName));
            
            RealmName = realmName;
        }

        /// <summary>
        ///     The name of the target WAMP realm.
        /// </summary>
        public string RealmName { get; }
    }
}