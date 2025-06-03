using System.Device.Gpio;
using System.Device.Pwm;
using System.Device.Pwm.Drivers;

namespace Rainbow.Buzzer;

/// <summary>
/// Controls the piezo buzzer on the Rainbow HAT.
/// </summary>
public class BuzzerController : IDisposable
{
    #region Constants

    /// <summary>
    /// The GPIO pin number for the buzzer.
    /// </summary>
    private const int BuzzerPin = 13;

    /// <summary>
    /// Default duty cycle for the PWM signal (50%).
    /// </summary>
    private const double DefaultDutyCycle = 0.5;

    #endregion

    #region Private Fields

    private readonly SoftwarePwmChannel _pwm;
    private readonly GpioController _gpio;
    private Timer? _durationTimer;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the BuzzerController class.
    /// </summary>
    public BuzzerController()
    {
        _gpio = new GpioController();
        _gpio.OpenPin(BuzzerPin, PinMode.Output);

        // Initialize PWM on the buzzer pin with a default frequency
        _pwm = new SoftwarePwmChannel(BuzzerPin, frequency: 440, dutyCycle: DefaultDutyCycle);
        Stop(); // Start in silent state
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Plays a tone at the specified frequency for an optional duration.
    /// </summary>
    /// <param name="frequency">The frequency in Hz (must be greater than 0)</param>
    /// <param name="duration">Optional duration in seconds. If null, the tone will sustain until stopped.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when frequency is less than or equal to 0.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when duration is less than or equal to 0.</exception>
    public void PlayNote(double frequency, double? duration = 1.0)
    {
        if (frequency <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(frequency), "Frequency must be greater than 0");
        }

        if (duration.HasValue && duration.Value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be greater than 0");
        }

        ClearDurationTimer();

        // Set the new frequency (convert to integer Hz)
        _pwm.Frequency = (int)Math.Round(frequency);
        _pwm.Start();

        // Set up duration timer if specified
        if (duration.HasValue)
        {
            _durationTimer = new Timer(_ => Stop(), null, TimeSpan.FromSeconds(duration.Value), Timeout.InfiniteTimeSpan);
        }
    }

    /// <summary>
    /// Plays a note by MIDI note number.
    /// </summary>
    /// <param name="noteNumber">The MIDI note number (A4 = 69)</param>
    /// <param name="duration">Optional duration in seconds. If null, the note will sustain until stopped.</param>
    /// <remarks>
    /// MIDI note 69 is A4 (440Hz). Each semitone is one number higher or lower.
    /// For reference:
    /// - Middle C (C4) is MIDI note 60
    /// - A4 (concert A, 440Hz) is MIDI note 69
    /// - Each octave is 12 notes
    /// </remarks>
    public void PlayMidiNote(int noteNumber, double? duration = 1.0)
    {
        // Convert MIDI note to frequency: f = 440 × 2^((n-69)/12)
        double frequency = 440.0 * Math.Pow(2, (noteNumber - 69.0) / 12.0);
        PlayNote(frequency, duration);
    }

    /// <summary>
    /// Stops the buzzer immediately.
    /// </summary>
    public void Stop()
    {
        ClearDurationTimer();
        _pwm.Stop();
    }

    #endregion

    #region Private Methods

    private void ClearDurationTimer()
    {
        if (_durationTimer != null)
        {
            _durationTimer.Dispose();
            _durationTimer = null;
        }
    }

    #endregion

    #region IDisposable Implementation

    /// <summary>
    /// Releases all resources used by the BuzzerController.
    /// </summary>
    public void Dispose()
    {
        Stop();
        _durationTimer?.Dispose();
        _pwm.Dispose();
        _gpio.ClosePin(BuzzerPin);
        _gpio.Dispose();
    }

    #endregion
}