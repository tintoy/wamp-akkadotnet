namespace Akka.Wamp.Messages
{
    /// <summary>
    ///     Message indicating that the timeout period expired before a component was activated by its owner.
    /// </summary>
    class ActivationTimeout
    {
        /// <summary>
        ///     The singleton instance of the <see cref="ActivationTimeout"/> message.
        /// </summary>
        public static readonly ActivationTimeout Instance = new ActivationTimeout();

        /// <summary>
        ///     Create a new <see cref="ActivationTimeout"/> message.
        /// </summary>
        ActivationTimeout()
        {
        }
    }
}
