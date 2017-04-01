namespace Akka.Wamp.Messages
{
    /// <summary>
    ///     Request to stop a server.
    /// </summary>
    class Stop
    {
        /// <summary>
        ///     The singleton instance of the <see cref="Stop"/> message.
        /// </summary>
        public static readonly Stop Instance = new Stop();

        /// <summary>
        ///     Create a new <see cref="Stop"/> message.
        /// </summary>
        Stop()
        {
        }
    }
}