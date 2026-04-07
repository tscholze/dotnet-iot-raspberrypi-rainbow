using System.Device.Gpio;

namespace Rainbow.Apa102
{
    /// <summary>
    /// Represents a controller for managing APA102 LED strips on the Rainbow HAT.
    /// This class handles the low-level communication with the APA102 LED strip,
    /// including GPIO control and LED data protocols.
    /// </summary>
    /// <remarks>
    /// The APA102 LED strip uses a two-wire SPI-like protocol (clock and data)
    /// with an additional chip select line. Each LED can be individually controlled
    /// for color (RGB) and brightness.
    /// </remarks>
    public partial class Apa102Controller : IDisposable
    {
        #region Constants

        /// <summary>
        /// GPIO pin number for the data signal (SPI MOSI).
        /// </summary>
        private const int DataPin = 10;

        /// <summary>
        /// GPIO pin number for the clock signal (SPI SCK).
        /// </summary>
        private const int ClockPin = 11;

        /// <summary>
        /// GPIO pin number for the chip select signal (SPI CS).
        /// </summary>
        private const int ChipSelectPin = 8;

        /// <summary>
        /// The number of RGB LEDs in the APA102 strip on the Rainbow HAT.
        /// </summary>
        private const int NumberOfPixels = 7;

        /// <summary>
        /// Number of clock cycles needed for the start frame.
        /// The start frame consists of 32 zero bits (4 bytes).
        /// </summary>
        private const int StartFrameLength = 32;

        /// <summary>
        /// Number of clock cycles needed for the end frame.
        /// The end frame needs 36 clock cycles to ensure proper latching
        /// of the data for all LED types.
        /// </summary>
        private const int EndFrameLength = 36;

        #endregion

        #region Private Fields

        /// <summary>
        /// The GPIO controller instance used to manage pin I/O operations.
        /// This controller handles all direct communication with the hardware.
        /// </summary>
        private readonly GpioController _gpio;

        /// <summary>
        /// Array of pixel data representing the current state of each LED.
        /// The array length matches <see cref="NumberOfPixels"/>.
        /// </summary>
        private readonly Pixel[] _pixels = new Pixel[NumberOfPixels];

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the APA102Controller class.
        /// If no GpioController is provided, a new instance will be created.
        /// <param name="gpio">Optional GpioController instance for GPIO operations.</param>
        /// </summary>
        public Apa102Controller(GpioController? gpio =  null)
        {
            _gpio = gpio ?? new();

            // Initialize all pixels with default brightness
            for (int i = 0; i < NumberOfPixels; i++)
            {
                _pixels[i] = new();
            }

            _gpio.OpenPin(DataPin, PinMode.Output);
            _gpio.OpenPin(ClockPin, PinMode.Output);
            _gpio.OpenPin(ChipSelectPin, PinMode.Output);

            _gpio.Write(ChipSelectPin, PinValue.High);
            _gpio.Write(ClockPin, PinValue.Low);
            _gpio.Write(DataPin, PinValue.Low);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the color and optionally brightness of a single pixel.
        /// </summary>
        /// <param name="index">The index of the pixel (0-6)</param>
        /// <param name="red">Red component (0-255)</param>
        /// <param name="green">Green component (0-255)</param>
        /// <param name="blue">Blue component (0-255)</param>
        /// <param name="brightness">Optional brightness (0.0-1.0). Default value: 0.2</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when index is out of range.</exception>
        public void SetPixel(int index, byte red, byte green, byte blue, float brightness = 0.2f)
        {
            if (index >= NumberOfPixels)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"Invalid pixel index {index}, should be (0-{NumberOfPixels - 1})");
            }

            _pixels[index].Red = red;
            _pixels[index].Green = green;
            _pixels[index].Blue = blue;

            if (brightness < 0 || brightness > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(brightness), "Brightness should be between 0.0 and 1.0");
            }
            // Scale brightness to 0-31 range
            _pixels[index].Brightness = (byte)(31.0f * brightness);
        }

        /// <summary>
        /// Sets the same color and optionally brightness for all pixels.
        /// </summary>
        /// <param name="red">Red component (0-255)</param>
        /// <param name="green">Green component (0-255)</param>
        /// <param name="blue">Blue component (0-255)</param>
        /// <param name="brightness">Optional brightness (0.0-1.0). If null, uses default value of 0.2</param>
        public void SetAll(byte red, byte green, byte blue, float? brightness = null)
        {
            float actualBrightness = brightness ?? 0.2f;
            for (int i = 0; i < NumberOfPixels; i++)
            {
                SetPixel(i, red, green, blue, actualBrightness);
            }
        }

        /// <summary>
        /// Sets the global brightness for all pixels.
        /// </summary>
        /// <param name="brightness">Brightness value between 0.0 and 1.0</param>
        public void SetBrightness(float brightness)
        {
            if (brightness < 0 || brightness > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(brightness), "Brightness should be between 0.0 and 1.0");
            }

            byte brightnessValue = (byte)(31.0f * brightness);
            for (int i = 0; i < NumberOfPixels; i++)
            {
                _pixels[i].Brightness = brightnessValue;
            }
        }

        /// <summary>
        /// Clears all pixels (sets them to black).
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < NumberOfPixels; i++)
            {
                _pixels[i].Red = 0;
                _pixels[i].Green = 0;
                _pixels[i].Blue = 0;
            }
        }

        /// <summary>
        /// Updates the LED strip with the current pixel values.
        /// </summary>
        public void Show()
        {
            _gpio.Write(ChipSelectPin, PinValue.Low);
            WriteStartFrame();

            foreach (var pixel in _pixels)
            {
                WriteByte((byte)(0b11100000 | (pixel.Brightness & 0b11111)));
                WriteByte(pixel.Blue);
                WriteByte(pixel.Green);
                WriteByte(pixel.Red);
            }

            WriteEndFrame();
            _gpio.Write(ChipSelectPin, PinValue.High);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Writes a single byte to the LED strip using the SPI-like protocol.
        /// </summary>
        /// <param name="value">The byte value to write to the LED strip.</param>
        /// <remarks>
        /// Each bit is written by:
        /// 1. Setting the data line to the bit value
        /// 2. Pulsing the clock line high then low
        /// The most significant bit is sent first.
        /// </remarks>
        private void WriteByte(byte value)
        {
            for (int i = 7; i >= 0; i--)
            {
                _gpio.Write(DataPin, ((value >> i) & 1) == 1 ? PinValue.High : PinValue.Low);
                _gpio.Write(ClockPin, PinValue.High);
                _gpio.Write(ClockPin, PinValue.Low);
            }
        }

        /// <summary>
        /// Writes the start frame of the APA102 protocol.
        /// </summary>
        /// <remarks>
        /// The start frame consists of 32 zero bits that prepare
        /// the LED strip to receive new pixel data.
        /// </remarks>
        private void WriteStartFrame()
        {
            _gpio.Write(DataPin, PinValue.Low);
            for (int i = 0; i < StartFrameLength; i++)
            {
                _gpio.Write(ClockPin, PinValue.High);
                _gpio.Write(ClockPin, PinValue.Low);
            }
        }

        /// <summary>
        /// Writes the end frame of the APA102 protocol.
        /// </summary>
        /// <remarks>
        /// The end frame consists of 36 clock pulses that ensure
        /// all data is properly latched into the LED strip.
        /// This specific number of pulses is required for proper
        /// operation with all variants of the APA102 LED.
        /// </remarks>
        private void WriteEndFrame()
        {
            _gpio.Write(DataPin, PinValue.Low);
            for (int i = 0; i < EndFrameLength; i++)
            {
                _gpio.Write(ClockPin, PinValue.High);
                _gpio.Write(ClockPin, PinValue.Low);
            }
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing,
        /// or resetting unmanaged resources.
        /// </summary>
        /// <remarks>
        /// This implementation:
        /// 1. Clears all LEDs to ensure they're turned off
        /// 2. Closes all GPIO pins used by the controller
        /// 3. Disposes of the GPIO controller
        /// </remarks>
        public void Dispose()
        {
            Clear();
            Show();

            _gpio.ClosePin(DataPin);
            _gpio.ClosePin(ClockPin);
            _gpio.ClosePin(ChipSelectPin);
            _gpio.Dispose();
        }

        #endregion
    }
}