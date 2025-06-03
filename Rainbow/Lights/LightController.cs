using System;
using System.Device.Gpio;

namespace Rainbow.Lights;

/// <summary>
/// Represents the set of RGB LEDs on the Rainbow HAT, providing control over individual and combined LED states.
/// </summary>
public class LightController : IDisposable
{
    #region Constants

    /// <summary>
    /// The GPIO pin number for the red LED.
    /// </summary>
    private const int RedPin = 6;

    /// <summary>
    /// The GPIO pin number for the green LED.
    /// </summary>
    private const int GreenPin = 19;

    /// <summary>
    /// The GPIO pin number for the blue LED.
    /// </summary>
    private const int BluePin = 26;

    #endregion

    #region Private fields

    /// <summary>
    /// The GPIO controller instance used to manage the LED pins.
    /// </summary>
    private readonly GpioController _controller;

    /// <summary>
    /// Array containing all LED instances for collective operations.
    /// </summary>
    private readonly Light[] _all;

    #endregion

    #region Public properties
    /// <summary>
    /// Gets the red LED light instance.
    /// </summary>
    public Light Red { get; }

    /// <summary>
    /// Gets the green LED light instance.
    /// </summary>
    public Light Green { get; }

    /// <summary>
    /// Gets the blue LED light instance.
    /// </summary>
    public Light Blue { get; }

    /// <summary>
    /// Gets a light instance by its index (0 = Red, 1 = Green, 2 = Blue).
    /// </summary>
    /// <param name="index">The index of the light to access.</param>
    /// <returns>The Light instance at the specified index.</returns>
    public Light this[int index] => _all[index];

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the Lights class and sets up the GPIO controller for all LEDs.
    /// </summary>
    public LightController()
    {
        _controller = new GpioController();

        Red = new Light(RedPin, _controller);
        Green = new Light(GreenPin, _controller);
        Blue = new Light(BluePin, _controller);
        _all = [Red, Green, Blue];
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Sets the state of all RGB LEDs individually.
    /// </summary>
    /// <param name="r">True to turn on the red LED, false to turn it off.</param>
    /// <param name="g">True to turn on the green LED, false to turn it off.</param>
    /// <param name="b">True to turn on the blue LED, false to turn it off.</param>
    public void Rgb(bool r, bool g, bool b)
    {
        Red.TurnToState(r ? Light.Target.On : Light.Target.Off);
        Green.TurnToState(g ? Light.Target.On : Light.Target.Off);
        Blue.TurnToState(b ? Light.Target.On : Light.Target.Off);
    }

    #endregion

    #region IDisposable implementation

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting resources.
    /// Turns off all LEDs and disposes of the GPIO controller.
    /// </summary>
    public void Dispose()
    {
        // iterate _all and turn off all lights
        foreach (var light in _all)
        {
            light.TurnToState(Light.Target.Off);
        }

        // close the GPIO pins
        _controller?.Dispose();
    }

    #endregion
}
