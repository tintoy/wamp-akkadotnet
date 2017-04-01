namespace Akka.Wamp
{
    /// <summary>
    ///     Request for component activation from the component owner.
    /// </summary>
    public class Activate
    {
        /// <summary>
        ///     Activate the component with default options.
        /// </summary>
        public static Activate Default = new Activate();

        /// <summary>
        ///     Activate the component, enabling error notifications.
        /// </summary>
        public static Activate WithErrorNotifications = new Activate(errorNotifications: true);

        /// <summary>
        ///     Create a new <see cref="Activate"/> message.
        /// </summary>
        /// <param name="errorNotifications">
        ///     Notify the owner of errors that are encountered by the component.
        /// 
        ///     If <c>true</c>, then the errors will be published as <see cref="WampError"/>s.
        /// </param>
        public Activate(bool errorNotifications = false)
        {
            ErrorNotifications = errorNotifications;
        }

        /// <summary>
        ///     Notify the owner of errors that are encountered by the component.
        /// </summary>
        /// <remarks>
        ///     If <c>true</c>, then the errors will be published as <see cref="WampError"/>s.
        /// </remarks>
        public bool ErrorNotifications { get; }
    }
}
