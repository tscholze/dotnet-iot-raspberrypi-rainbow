using System.Device.I2c;

namespace Rainbow.SegmentDisplay
{
    /// <summary>
    /// Driver for the HT16K33 LED segment display driver.
    /// This implementation is specific to the 14-segment alphanumeric display on the Rainbow HAT.
    /// </summary>
    /// <remarks>
    /// The HT16K33 is an LED driver with an I2C interface, capable of controlling up to 128 LEDs
    /// or 16 seven-segment/fourteen-segment displays. It features adjustable brightness, blinking,
    /// and a built-in oscillator.
    /// </remarks>
    public class Ht16k33 : IDisposable
    {
        #region Constants

        /// <summary>
        /// Default I2C address of the HT16K33 on the Rainbow HAT (0x70).
        /// </summary>
        public const int DefaultAddress = 0x70;

        /// <summary>
        /// System setup command byte for controlling basic chip functions.
        /// </summary>
        private const byte SystemSetup = 0x20;

        /// <summary>
        /// Oscillator control bit. When set, enables the internal oscillator.
        /// </summary>
        private const byte Oscillator = 0x01;

        /// <summary>
        /// Display setup command byte for controlling display state and blinking.
        /// </summary>
        private const byte DisplaySetup = 0x80;

        /// <summary>
        /// Display on control bit. When set, enables the display output.
        /// </summary>
        private const byte DisplayOn = 0x01;

        /// <summary>
        /// Brightness control command byte (0xE0). Combine with brightness level (0-15).
        /// </summary>
        private const byte CommandBrightness = 0xE0;

        #endregion

        #region Enums

        /// <summary>
        /// Available blink rates for the display.
        /// </summary>
        /// <remarks>
        /// The HT16K33 supports four different blink rates, including a steady state (no blink).
        /// Blinking affects the entire display and cannot be set per-digit.
        /// </remarks>
        public enum BlinkRate : byte
        {
            /// <summary>Display does not blink (steady state)</summary>
            Off = 0x00,
            /// <summary>Display blinks at 2Hz (twice per second)</summary>
            TwoHz = 0x02,
            /// <summary>Display blinks at 1Hz (once per second)</summary>
            OneHz = 0x04,
            /// <summary>Display blinks at 0.5Hz (once every two seconds)</summary>
            HalfHz = 0x06
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// The I2C device used for communication with the HT16K33.
        /// </summary>
        private readonly I2cDevice _device;

        /// <summary>
        /// Buffer holding the current state of all display segments.
        /// 16 bytes total, 2 bytes per character for 8 characters maximum.
        /// </summary>
        private readonly byte[] _displayBuffer;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the HT16K33 display driver.
        /// </summary>
        /// <param name="device">The I2C device for communication with the HT16K33.</param>
        /// <remarks>
        /// The constructor performs the following initialization:
        /// 1. Enables the internal oscillator
        /// 2. Turns on the display with no blinking
        /// 3. Sets maximum brightness
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when device is null.</exception>
        public Ht16k33(I2cDevice device)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            _displayBuffer = new byte[16];


            // Turn on oscillator
            _device.WriteByte((byte)(SystemSetup | Oscillator));

            // Turn display on, no blinking
            SetBlinkRate(BlinkRate.Off);

            // Set maximum brightness
            SetBrightness(15);

        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the blink rate of the entire display.
        /// </summary>
        /// <param name="rate">The desired blink rate from the BlinkRate enum.</param>
        /// <remarks>
        /// The blink rate affects the entire display simultaneously.
        /// Changes take effect immediately without needing to call WriteDisplay.
        /// </remarks>
        public void SetBlinkRate(BlinkRate rate)
        {
            _device.WriteByte((byte)(DisplaySetup | DisplayOn | (byte)rate));
        }

        /// <summary>
        /// Sets the brightness of the entire display.
        /// </summary>
        /// <param name="brightness">Brightness level from 0 (minimum) to 15 (maximum).</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when brightness is not between 0 and 15.</exception>
        /// <remarks>
        /// The brightness setting affects all LEDs simultaneously.
        /// Changes take effect immediately without needing to call WriteDisplay.
        /// </remarks>
        public void SetBrightness(byte brightness)
        {
            if (brightness > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(brightness), "Brightness must be a value from 0 to 15");
            }

            _device.WriteByte((byte)(CommandBrightness | brightness));
        }

        /// <summary>
        /// Sets the state of a single LED in the display matrix.
        /// </summary>
        /// <param name="led">LED position (0-127)</param>
        /// <param name="state">True to turn on the LED, false to turn it off</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when led is greater than 127.</exception>
        /// <remarks>
        /// This method only updates the display buffer. Call WriteDisplay to update the physical display.
        /// LED positions are mapped as follows:
        /// - Each character uses 16 LEDs (14 segments + decimal point + optional colon)
        /// - LEDs 0-7 are the lower byte of a character
        /// - LEDs 8-15 are the upper byte of a character
        /// </remarks>
        public void SetLed(byte led, bool state)
        {
            if (led > 127)
            {
                throw new ArgumentOutOfRangeException(nameof(led), "LED position must be 0-127");
            }

            int pos = led / 8;
            int offset = led % 8;

            if (state)
            {
                _displayBuffer[pos] |= (byte)(1 << offset);
            }
            else
            {
                _displayBuffer[pos] &= (byte)~(1 << offset);
            }
        }

        /// <summary>
        /// Writes the display buffer to the HT16K33 display.
        /// </summary>
        /// <remarks>
        /// This method must be called after any buffer changes to update the physical display.
        /// The entire buffer is written in sequence, starting from address 0.
        /// </remarks>
        public void WriteDisplay()
        {
            for (int i = 0; i < _displayBuffer.Length; i++)
            {
                _device.Write(new[] { (byte)i, _displayBuffer[i] });
            }
        }

        /// <summary>
        /// Clears the display buffer, turning off all LEDs.
        /// </summary>
        /// <remarks>
        /// This method only clears the buffer. Call WriteDisplay to update the physical display.
        /// All 16 bytes of the display buffer are set to 0.
        /// </remarks>
        public void Clear()
        {
            Array.Clear(_displayBuffer, 0, _displayBuffer.Length);
        }

        /// <summary>
        /// Sets a byte directly in the display buffer at the specified position.
        /// </summary>
        /// <param name="position">Buffer position (0-15)</param>
        /// <param name="value">Byte value to set</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when position is not between 0 and 15.</exception>
        /// <remarks>
        /// This method provides direct access to the display buffer:
        /// - Even positions (0,2,4,6) contain the lower byte of each character
        /// - Odd positions (1,3,5,7) contain the upper byte of each character
        /// Call WriteDisplay to update the physical display after making changes.
        /// </remarks>
        public void SetBufferByte(int position, byte value)
        {
            if (position < 0 || position >= _displayBuffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(position), "Position must be 0-15");
            }
            _displayBuffer[position] = value;
        }

        /// <summary>
        /// Gets a byte from the display buffer at the specified position.
        /// </summary>
        /// <param name="position">Buffer position (0-15)</param>
        /// <returns>The byte value at the specified position</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when position is not between 0 and 15.</exception>
        /// <remarks>
        /// This method provides read access to the display buffer:
        /// - Even positions (0,2,4,6) contain the lower byte of each character
        /// - Odd positions (1,3,5,7) contain the upper byte of each character
        /// </remarks>
        public byte GetBufferByte(int position)
        {
            if (position < 0 || position >= _displayBuffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(position), "Position must be 0-15");
            }
            return _displayBuffer[position];
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        /// Releases resources used by the HT16K33 driver.
        /// </summary>
        /// <remarks>
        /// Disposes of the I2C device. The display state is not modified during disposal.
        /// </remarks>
        public void Dispose()
        {
          _device?.Dispose();
        }

        #endregion
    }
}