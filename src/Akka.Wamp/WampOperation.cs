namespace Akka.Wamp
{
    /// <summary>
    ///     Represents a well-known WAMP operation.
    /// </summary>
    public enum WampOperation
    {
        /// <summary>
        ///     An unknown operation type.
        /// </summary>
        /// <remarks>
        ///     Used to detect uninitialised values; do not use directly.
        /// </remarks>
        Unknown         = 0,

        /// <summary>
        ///     Send a WAMP event.
        /// </summary>
        SendEvent       = 1,

        /// <summary>
        ///     Receive a WAMP event.
        /// </summary>
        ReceiveEvent    = 2
    }
}
