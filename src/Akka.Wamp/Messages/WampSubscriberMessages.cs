using System.Collections.Immutable;

namespace Akka.Wamp.Messages
{
    public class ActivateSubscription
    {
        public static readonly ActivateSubscription Instance = new ActivateSubscription();

        ActivateSubscription()
        {
        }
    }

    class SubscriptionActivationTimeout
    {
        public static readonly SubscriptionActivationTimeout Instance = new SubscriptionActivationTimeout();

        SubscriptionActivationTimeout()
        {
        }
    }

    public class Received
    {
        public Received(object[] messageArguments)
        {
            MessageArguments = ImmutableList.Create(messageArguments);
        }

        public ImmutableList<object> MessageArguments { get; }
    }

    public class Unsubscribe
    {
        public static readonly Unsubscribe Instance = new Unsubscribe();

        Unsubscribe()
        {
        }
    }
}