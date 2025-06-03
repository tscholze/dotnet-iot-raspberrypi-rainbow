namespace Rainbow.Buttons
{
    /// <summary>
    /// Identifiers for the buttons on the Rainbow HAT.
    /// </summary>
    public enum ButtonConfiguration
    {
        /// <summary>
        /// Button A
        /// </summary>
        A = 0,

        /// <summary>
        /// Button B
        /// </summary>
        B = 1,

        /// <summary>
        /// Button C
        /// </summary>
        C = 2
    }

    /// <summary>
    /// Extension methods for working with ButtonConfiguration enum.
    /// </summary>
    public static class ButtonIdentifierExtensions
    {
        /// <summary>
        /// Gets the GPIO pin number associated with the specified button.
        /// </summary>
        /// <param name="button">The button configuration to get the pin number for.</param>
        /// <returns>The GPIO pin number for the specified button.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the button identifier is invalid.</exception>
        public static int GetPin(this ButtonConfiguration button)
        {
            return button switch
            {
                ButtonConfiguration.A => 21,
                ButtonConfiguration.B => 20,
                ButtonConfiguration.C => 16,
                _ => throw new ArgumentOutOfRangeException(nameof(button), "Invalid button identifier")
            };
        }
        /// <summary>
        /// Gets the friendly display name of the specified button.
        /// </summary>
        /// <param name="button">The button configuration to get the name for.</param>
        /// <returns>A human-readable name for the specified button (e.g., "Button A").</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the button identifier is invalid.</exception>
        public static string GetName(this ButtonConfiguration button)
        {
            return button switch
            {
                ButtonConfiguration.A => "Button A",
                ButtonConfiguration.B => "Button B",
                ButtonConfiguration.C => "Button C",
                _ => throw new ArgumentOutOfRangeException(nameof(button), "Invalid button identifier")
            };
        }
        /// <summary>
        /// Gets the zero-based index of the specified button.
        /// </summary>
        /// <param name="button">The button configuration to get the index for.</param>
        /// <returns>The index of the button (0 for A, 1 for B, 2 for C).</returns>
        public static int GetIndex(this ButtonConfiguration button)
        {
            return (int)button;
        }
    }
}