namespace Akka.Wamp.Messages
{
    /// <summary>
    ///     Request from a subscription owner to terminate a subscription.
    /// </summary>
    class Unsubscribe
    {
        /// <summary>
        ///     The singleton instance of the <see cref="Unsubscribe"/> message.
        /// </summary>
        public static readonly Unsubscribe Instance = new Unsubscribe();

        /// <summary>
        ///     Create a new <see cref="Unsubscribe"/> message.
        /// </summary>
        Unsubscribe()
        {
        }
    }
}