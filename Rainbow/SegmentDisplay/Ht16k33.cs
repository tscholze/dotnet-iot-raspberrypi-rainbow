using System;
using System.Device.I2c;
using System.Threading;

namespace Rainbow.SegmentDisplay
{
    /// <summary>
    /// Driver for the HT16K33 LED segment display driver.
    /// This implementation is specific to the 7-segment display on the Rainbow HAT.
    /// </summary>
    public class Ht16k33 : IDisposable
    {
        #region Constants

        /// <summary>
        /// Default I2C address of the HT16K33 on the Rainbow HAT
        /// </summary>
        public const int DefaultAddress = 0x70;

        private const byte SystemSetup = 0x20;
        private const byte Oscillator = 0x01;
        private const byte DisplaySetup = 0x80;
        private const byte DisplayOn = 0x01;
        private const byte CommandBrightness = 0xE0;

        #endregion

        #region Enums

        /// <summary>
        /// Available blink rates for the display
        /// </summary>
        public enum BlinkRate : byte
        {
            /// <summary>Display does not blink</summary>
            Off = 0x00,
            /// <summary>Display blinks at 2Hz</summary>
            TwoHz = 0x02,
            /// <summary>Display blinks at 1Hz</summary>
            OneHz = 0x04,
            /// <summary>Display blinks at 0.5Hz</summary>
            HalfHz = 0x06
        }

        #endregion

        #region Private Fields

        private readonly I2cDevice _device;
        private readonly byte[] _displayBuffer;
        private bool _isInitialized;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the HT16K33 display driver
        /// </summary>
        /// <param name="device">The I2C device for communication</param>
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
        /// Sets the blink rate of the entire display
        /// </summary>
        /// <param name="rate">The desired blink rate</param>
        public void SetBlinkRate(BlinkRate rate)
        {
            _device.WriteByte((byte)(DisplaySetup | DisplayOn | (byte)rate));
        }

        /// <summary>
        /// Sets the brightness of the entire display
        /// </summary>
        /// <param name="brightness">Brightness level from 0-15</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when brightness is not between 0 and 15</exception>
        public void SetBrightness(byte brightness)
        {
            if (brightness > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(brightness), "Brightness must be a value from 0 to 15");
            }

            _device.WriteByte((byte)(CommandBrightness | brightness));
        }

        /// <summary>
        /// Sets the state of a single LED in the display matrix
        /// </summary>
        /// <param name="led">LED position (0-127)</param>
        /// <param name="state">True to turn on, false to turn off</param>
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
        /// Writes the display buffer to the HT16K33 display
        /// </summary>
        public void WriteDisplay()
        {
            for (int i = 0; i < _displayBuffer.Length; i++)
            {
                _device.Write(new[] { (byte)i, _displayBuffer[i] });
            }
        }

        /// <summary>
        /// Clears the display buffer (turns off all LEDs)
        /// </summary>
        public void Clear()
        {
            Array.Clear(_displayBuffer, 0, _displayBuffer.Length);
        }

        /// <summary>
        /// Sets a byte directly in the display buffer at the specified position
        /// </summary>
        /// <param name="position">Buffer position (0-15)</param>
        /// <param name="value">Byte value to set</param>
        public void SetBufferByte(int position, byte value)
        {
            if (position < 0 || position >= _displayBuffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(position), "Position must be 0-15");
            }
            _displayBuffer[position] = value;
        }

        /// <summary>
        /// Gets a byte from the display buffer at the specified position
        /// </summary>
        /// <param name="position">Buffer position (0-15)</param>
        /// <returns>The byte value at the specified position</returns>
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
        /// Releases resources used by the HT16K33 driver
        /// </summary>
        public void Dispose()
        {
          _device?.Dispose();
        }

        #endregion
    }
}