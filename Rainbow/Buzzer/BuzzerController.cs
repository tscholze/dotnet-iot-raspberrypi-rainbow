using System.Device.Gpio;
using System.Device.Pwm.Drivers;

namespace Rainbow.Buzzer;

/// <summary>
/// Controls the piezo buzzer on the Rainbow HAT.
/// </summary>
/// <remarks>
/// This class provides control over the buzzer through PWM (Pulse Width Modulation),
/// allowing for precise frequency control and note generation. It supports both direct
/// frequency input and MIDI note numbers for musical applications.
/// </remarks>
public class BuzzerController : IDisposable
{
    #region Constants

    /// <summary>
    /// The GPIO pin number for the buzzer on the Rainbow HAT.
    /// </summary>
    /// <remarks>
    /// Pin 13 is hardwired to the piezo buzzer on the Rainbow HAT board.
    /// </remarks>
    private const int BuzzerPin = 13;

    /// <summary>
    /// Default duty cycle for the PWM signal (50%).
    /// </summary>
    /// <remarks>
    /// A 50% duty cycle provides the cleanest tone for a piezo buzzer,
    /// as it creates a symmetric square wave.
    /// </remarks>
    private const double DefaultDutyCycle = 0.5;

    #endregion

    #region Private Fields

    /// <summary>
    /// The PWM channel used to generate tones on the buzzer.
    /// </summary>
    /// <remarks>
    /// Software PWM is used for maximum frequency flexibility,
    /// though it may have some timing jitter compared to hardware PWM.
    /// </remarks>
    private readonly SoftwarePwmChannel _pwm;

    /// <summary>
    /// The GPIO controller used to manage the buzzer pin.
    /// </summary>
    private readonly GpioController _gpio;

    /// <summary>
    /// Timer used to automatically stop notes after their duration expires.
    /// </summary>
    /// <remarks>
    /// When null, the note will sustain indefinitely until explicitly stopped.
    /// </remarks>
    private Timer? _durationTimer;

    /// <summary>
    /// Tracks whether this instance has been disposed.
    /// </summary>
    private bool _disposed;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the BuzzerController class.
    /// </summary>
    /// <remarks>
    /// The constructor:
    /// 1. Initializes GPIO for the buzzer pin
    /// 2. Sets up PWM with default frequency (A4 = 440Hz)
    /// 3. Starts in silent state (stopped)
    /// </remarks>
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
    /// <remarks>
    /// <para>
    /// The frequency parameter determines the pitch of the note. For reference:
    /// - A4 (concert pitch) = 440 Hz
    /// - Middle C (C4) ≈ 261.63 Hz
    /// - Range should typically be kept between 20 Hz and 20000 Hz (human hearing range)
    /// </para>
    /// <para>
    /// If duration is specified, the note will automatically stop after that time.
    /// If duration is null, the note will continue until Stop() is called.
    /// </para>
    /// </remarks>
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
    /// <para>
    /// MIDI note numbers follow the standard MIDI specification:
    /// - Middle C (C4) is MIDI note 60
    /// - A4 (concert A, 440Hz) is MIDI note 69
    /// - Each semitone is one number higher or lower
    /// - Each octave is 12 notes
    /// </para>
    /// <para>
    /// Common MIDI note numbers:
    /// - C4 (Middle C) = 60
    /// - C5 = 72
    /// - A4 (Concert A) = 69
    /// </para>
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
    /// <remarks>
    /// This method:
    /// 1. Cancels any pending duration timer
    /// 2. Stops the PWM signal
    /// 3. Silences the buzzer immediately
    /// </remarks>
    public void Stop()
    {
        ClearDurationTimer();
        _pwm.Stop();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Clears any active duration timer.
    /// </summary>
    /// <remarks>
    /// This method ensures proper cleanup of timer resources and prevents
    /// multiple timers from running simultaneously when playing new notes.
    /// </remarks>
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
    /// <remarks>
    /// This method:
    /// 1. Stops any currently playing note
    /// 2. Cleans up the duration timer
    /// 3. Disposes of the PWM channel
    /// 4. Closes and disposes of the GPIO controller
    /// 
    /// After disposal, the controller cannot be used again.
    /// </remarks>
    public void Dispose()
    {
        if (!_disposed)
        {
            Stop();
            _durationTimer?.Dispose();
            _pwm.Dispose();
            _gpio.ClosePin(BuzzerPin);
            _gpio.Dispose();
            _disposed = true;
        }
    }

    #endregion
}