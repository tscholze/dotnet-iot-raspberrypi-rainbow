using System.Device.Gpio;

namespace Rainbow.Lights;

/// <summary>
/// Represents a single GPIO LED light with control over its state.
/// </summary>
public class Light
{
    #region Public fields

    /// <summary>
    /// Gets whether the LED is currently turned on.
    /// </summary>
    public bool IsOn { get; private set; }

    #endregion

    #region Private fields

    /// <summary>
    /// The GPIO pin number assigned to this LED.
    /// </summary>
    private readonly int _pin;

    /// <summary>
    /// The GPIO controller instance used to control this LED.
    /// </summary>
    private readonly GpioController _controller;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the Light class.
    /// </summary>
    /// <param name="pin">The GPIO pin number for this LED.</param>
    /// <param name="controller">The GPIO controller instance to use for controlling the LED.</param>
    public Light(int pin, GpioController controller)
    {
        _pin = pin;
        _controller = controller;
        IsOn = false;

        _controller.OpenPin(_pin, PinMode.Output);
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Changes the LED state according to the specified target state.
    /// </summary>
    /// <param name="target">The target state to set the LED to.</param>
    public void TurnToState(Target target)
    {
        switch (target)
        {
            case Target.Off:
                Write(false);
                break;
            case Target.On:
                Write(true);
                break;
            case Target.Toggle:
                Write(!IsOn);
                break;
        }
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Writes a value to the GPIO pin to control the LED state.
    /// </summary>
    /// <param name="value">True to turn the LED on, false to turn it off.</param>
    /// <exception cref="InvalidOperationException">Thrown when the GPIO operation fails.</exception>
    void Write(bool value)
    {
        try
        {
            IsOn = value;
            _controller.Write(_pin, value ? PinValue.High : PinValue.Low);
            // Console.WriteLine($"LED on pin {_pin} set to: {(value ? "ON" : "OFF")}");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to write value {value} to GPIO pin {_pin}: {ex.Message}", ex);
        }
    }

    #endregion

    #region Light.Target enum

    /// <summary>
    /// Represents the possible target states for an LED.
    /// </summary>
    public enum Target
    {
        /// <summary>
        /// Represents the off state of the LED.
        /// </summary>
        Off,

        /// <summary>
        /// Represents the on state of the LED.
        /// </summary>
        On,

        /// <summary>
        /// Represents toggling the current state of the LED.
        /// </summary>
        Toggle
    }

    #endregion
}