namespace Akka.Wamp
{
    /// <summary>
    ///     Request for component activation from the component owner.
    /// </summary>
    public class Activate
    {
        /// <summary>
        ///     The singleton instance of the <see cref="Activate"/> message.
        /// </summary>
        public static readonly Activate Instance = new Activate();

        /// <summary>
        ///     Create a new <see cref="Activate"/> message.
        /// </summary>
        public Activate()
        {
        }
    }
}
