namespace Rainbow.Buttons
{
    /// <summary>
    /// Provides data for button-related events on the Rainbow HAT.
    /// </summary>
    public class ButtonEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the button configuration that identifies which button triggered the event.
        /// </summary>
        /// <value>
        /// The button configuration (A, B, or C) that triggered the event.
        /// </value>
        public ButtonConfiguration Button { get; }

        /// <summary>
        /// Initializes a new instance of the ButtonEventArgs class.
        /// </summary>
        /// <param name="button">The button configuration that triggered the event.</param>
        public ButtonEventArgs(ButtonConfiguration button)
        {
            Button = button;
        }
    }
}
