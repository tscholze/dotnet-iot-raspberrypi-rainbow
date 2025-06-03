using System;
using System.Device.Gpio;

namespace Rainbow.Buttons
{
    /// <summary>
    /// Controls and manages the three touch-sensitive buttons (A, B, C) on the Rainbow HAT.
    /// Provides unified access to all buttons and their events.
    /// </summary>
    public class ButtonController : IDisposable
    {
        #region Private fields

        /// <summary>
        /// The GPIO controller instance used to interact with the buttons
        /// </summary>
        private readonly GpioController _controller = new GpioController();

        /// <summary>
        /// Array containing all button instances (A, B, C)
        /// </summary>
        private readonly Button[] _buttons;

        #endregion

        #region Public properties

        /// <summary>
        /// Gets Button A of the Rainbow HAT
        /// </summary>
        /// <value>The Button A instance</value>
        public Button A => _buttons[(int)ButtonConfiguration.A];

        /// <summary>
        /// Gets Button B of the Rainbow HAT
        /// </summary>
        /// <value>The Button B instance</value>
        public Button B => _buttons[(int)ButtonConfiguration.B];

        /// <summary>
        /// Gets Button C of the Rainbow HAT
        /// </summary>
        /// <value>The Button C instance</value>
        public Button C => _buttons[(int)ButtonConfiguration.C];

        /// <summary>
        /// Gets a button by its index (0=A, 1=B, 2=C)
        /// </summary>
        /// <param name="index">The index of the button to get</param>
        /// <returns>The Button instance at the specified index</returns>
        public Button this[int index] => _buttons[index];

        /// <summary>
        /// Event raised when any button is pressed
        /// </summary>
        public event EventHandler<ButtonEventArgs>? ButtonPressed;

        /// <summary>
        /// Event raised when any button is released
        /// </summary>
        public event EventHandler<ButtonEventArgs>? ButtonReleased;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the ButtonController class.
        /// Sets up all three buttons and their event handlers.
        /// </summary>
        public ButtonController()
        {
            // Initialize to prevent null checks
            ButtonPressed += (s, e) => { };
            ButtonReleased += (s, e) => { };


            _buttons =
            [
                new Button(ButtonConfiguration.A, _controller),
                new Button(ButtonConfiguration.B, _controller),
                new Button(ButtonConfiguration.C, _controller)
            ];

            foreach (var button in _buttons)
            {
                button.Pressed += (s, e) => ButtonPressed?.Invoke(this, e);
                button.Released += (s, e) => ButtonReleased?.Invoke(this, e);
            }
        }

        #endregion

        #region IDisposable implementation

        /// <summary>
        /// Releases all resources used by the ButtonController and its buttons
        /// </summary>
        public void Dispose()
        {
            foreach (var button in _buttons)
            {
                button?.Dispose();
            }

            _controller?.Dispose();
        }

        #endregion
    }
}