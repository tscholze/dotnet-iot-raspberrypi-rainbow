using System;
using System.Device.Gpio;

namespace Rainbow.Buttons
{
    /// <summary>
    /// Represents a single touch-sensitive button on the Rainbow HAT.
    /// Handles the GPIO interactions and events for a physical button.
    /// </summary>
    public class Button : IDisposable
    {
        #region Private fields

        /// <summary>
        /// The button identifier configuration.
        /// </summary>
        private readonly ButtonConfiguration _id;

        /// <summary>
        /// The GPIO controller instance used to interact with the physical pin.
        /// </summary>
        private readonly GpioController _controller;

        #endregion

        #region Public properties

        /// <summary>
        /// Gets whether the button is currently pressed.
        /// The value is true when the button is pressed (circuit closed) and false when released.
        /// </summary>
        public bool IsPressed { get; private set; }

        #endregion

        #region Events

        /// <summary>
        /// Event raised when the button is pressed (circuit closed).
        /// </summary>
        public event EventHandler<ButtonEventArgs>? Pressed;

        /// <summary>
        /// Event raised when the button is released (circuit opened).
        /// </summary>
        public event EventHandler<ButtonEventArgs>? Released;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the Button class.
        /// </summary>
        /// <param name="id">The button identifier (A, B, or C)</param>
        /// <param name="controller">The GPIO controller instance to use</param>
        public Button(ButtonConfiguration id, GpioController controller)
        {
            _id = id;
            _controller = controller;

            // Configure the pin with a pull-up resistor
            _controller.OpenPin(id.GetPin(), PinMode.InputPullUp);
            _controller.RegisterCallbackForPinValueChangedEvent(
                id.GetPin(),
                PinEventTypes.Rising | PinEventTypes.Falling,
                HandleButtonEvent
            );

            IsPressed = false;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Handles GPIO pin value changed events and raises the appropriate button events.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The pin value change event arguments.</param>
        private void HandleButtonEvent(object sender, PinValueChangedEventArgs args)
        {
            // Update pressed state (Falling edge means pressed because of pull-up resistor)
            IsPressed = args.ChangeType == PinEventTypes.Falling;

            // Create event args with button configuration
            var buttonArgs = new ButtonEventArgs(_id);

            if (IsPressed)
            {
                Pressed?.Invoke(this, buttonArgs);
            }
            else
            {
                Released?.Invoke(this, buttonArgs);
            }
        }

        #endregion

        #region IDisposable implementation

        /// <summary>
        /// Releases the GPIO resources used by the button.
        /// </summary>
        public void Dispose()
        {
            _controller.UnregisterCallbackForPinValueChangedEvent(_id.GetPin(), HandleButtonEvent);
            _controller.ClosePin(_id.GetPin());
        }

        #endregion
    }
}
