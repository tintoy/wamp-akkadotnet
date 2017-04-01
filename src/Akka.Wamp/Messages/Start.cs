namespace Akka.Wamp.Messages
{
    /// <summary>
    ///     Request to start a server.
    /// </summary>
    class Start
    {
        /// <summary>
        ///     The singleton instance of the <see cref="Start"/> message.
        /// </summary>
        public static readonly Start Instance = new Start();

        /// <summary>
        ///     Create a new <see cref="Start"/> message.
        /// </summary>
        Start()
        {
        }
    }
}