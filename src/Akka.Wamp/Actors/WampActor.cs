using Akka.Actor;
using Akka.Event;

namespace Akka.Wamp.Actors
{
    /// <summary>
    ///     The base class for all WAMP-related actors.
    /// </summary>
    abstract class WampActor
        : ReceiveActor
    {
        /// <summary>
        ///     Create a new <see cref="WampActor"/>.
        /// </summary>
        protected WampActor()
        {
        }

        /// <summary>
        ///     The logging facility.
        /// </summary>
        protected ILoggingAdapter Log { get; } = Logging.GetLogger(Context);
    }
}
