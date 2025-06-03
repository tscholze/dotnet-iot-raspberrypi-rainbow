using System;
using System.Device.I2c;
using System.Collections.Generic;

namespace Rainbow.SegmentDisplay
{
    /// <summary>
    /// Controls the 4-digit 14-segment alphanumeric display on the Rainbow HAT
    /// </summary>
    public class SegmentDisplayController : IDisposable
    {
        #region Constants

        // Character map for alphanumeric display (ASCII 32-127)
        private static readonly Dictionary<char, ushort> CharacterMap = new()
        {
            { ' ', 0b0000000000000000 },
            { '!', 0b0000000000000110 },
            { '"', 0b0000001000100000 },
            { '#', 0b0001001011001110 },
            { '$', 0b0001001011101101 },
            { '%', 0b0000110000100100 },
            { '&', 0b0010001101011101 },
            { '\'', 0b0000010000000000 },
            { '(', 0b0010010000000000 },
            { ')', 0b0000100100000000 },
            { '*', 0b0011111111000000 },
            { '+', 0b0001001011000000 },
            { ',', 0b0000100000000000 },
            { '-', 0b0000000011000000 },
            { '.', 0b0100000000000000 }, // Decimal point is bit 14
            { '/', 0b0000110000000000 },
            { '0', 0b0000110000111111 },
            { '1', 0b0000000000000110 },
            { '2', 0b0000000011011011 },
            { '3', 0b0000000010001111 },
            { '4', 0b0000000011100110 },
            { '5', 0b0010000001101001 },
            { '6', 0b0000000011111101 },
            { '7', 0b0000000000000111 },
            { '8', 0b0000000011111111 },
            { '9', 0b0000000011101111 },
            { ':', 0b0001001000000000 },
            { ';', 0b0000101000000000 },
            { '<', 0b0010010000000000 },
            { '=', 0b0000000011001000 },
            { '>', 0b0000100100000000 },
            { '?', 0b0001000010000011 },
            { '@', 0b0000001010111011 },
            { 'A', 0b0000000011110111 },
            { 'B', 0b0001001010001111 },
            { 'C', 0b0000000000111001 },
            { 'D', 0b0001001000001111 },
            { 'E', 0b0000000011111001 },
            { 'F', 0b0000000001110001 },
            { 'G', 0b0000000010111101 },
            { 'H', 0b0000000011110110 },
            { 'I', 0b0001001000000000 },
            { 'J', 0b0000000000011110 },
            { 'K', 0b0010010001110000 },
            { 'L', 0b0000000000111000 },
            { 'M', 0b0000010100110110 },
            { 'N', 0b0010000100110110 },
            { 'O', 0b0000000000111111 },
            { 'P', 0b0000000011110011 },
            { 'Q', 0b0010000000111111 },
            { 'R', 0b0010000011110011 },
            { 'S', 0b0000000011101101 },
            { 'T', 0b0001001000000001 },
            { 'U', 0b0000000000111110 },
            { 'V', 0b0000110000110000 },
            { 'W', 0b0010100000110110 },
            { 'X', 0b0010110100000000 },
            { 'Y', 0b0001010100000000 },
            { 'Z', 0b0000110000001001 },
            { '[', 0b0000000000111001 },
            { '\\', 0b0010000100000000 },
            { ']', 0b0000000000001111 },
            { '^', 0b0000110000000011 },
            { '_', 0b0000000000001000 },
            { '`', 0b0000000100000000 },
            { 'a', 0b0001000001011000 },
            { 'b', 0b0010000001111000 },
            { 'c', 0b0000000011011000 },
            { 'd', 0b0000100010001110 },
            { 'e', 0b0000100001011000 },
            { 'f', 0b0000000001110001 },
            { 'g', 0b0000010010001110 },
            { 'h', 0b0001000001110000 },
            { 'i', 0b0001000000000000 },
            { 'j', 0b0000000000001110 },
            { 'k', 0b0011011000000000 },
            { 'l', 0b0000000000110000 },
            { 'm', 0b0001000011010100 },
            { 'n', 0b0001000001010000 },
            { 'o', 0b0000000011011100 },
            { 'p', 0b0000000101110000 },
            { 'q', 0b0000010010000110 },
            { 'r', 0b0000000001010000 },
            { 's', 0b0010000010001000 },
            { 't', 0b0000000001111000 },
            { 'u', 0b0000000000011100 },
            { 'v', 0b0010000000000100 },
            { 'w', 0b0010100000010100 },
            { 'x', 0b0010100011000000 },
            { 'y', 0b0010000000001100 },
            { 'z', 0b0000100001001000 },
            { '{', 0b0000100101001001 },
            { '|', 0b0001001000000000 },
            { '}', 0b0010010010001001 },
            { '~', 0b0000010100100000 }
        };

        // Position addresses for each digit (consecutive positions for 14-segment display)
        private const int Digit1Addr = 0;
        private const int Digit2Addr = 1;
        private const int Digit3Addr = 2;
        private const int Digit4Addr = 3;

        #endregion

        #region Private Fields

        private readonly Ht16k33 _display;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the SegmentDisplayController
        /// </summary>
        public SegmentDisplayController()
        {
            var settings = new I2cConnectionSettings(1, Ht16k33.DefaultAddress);
            var device = I2cDevice.Create(settings);
            _display = new Ht16k33(device);
            Clear();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Displays a string on the alphanumeric display.
        /// </summary>
        /// <param name="text">Text to display (max 4 characters)</param>
        /// <param name="justifyRight">Whether to right-justify the text</param>
        public void DisplayText(string text = ".NET", bool justifyRight = true)
        {
            if (string.IsNullOrEmpty(text))
            {
                Clear();
                return;
            }

            if (text.Length > 4)
            {
                text = text[..4];
            }

            Clear();

            int startPos = justifyRight ? 4 - text.Length : 0;
            for (int i = 0; i < text.Length; i++)
            {
                SetChar(startPos + i, text[i]);
            }

            _display.WriteDisplay();
        }

        /// <summary>
        /// Displays a number with optional decimal points
        /// </summary>
        /// <param name="value">The number to display</param>
        /// <param name="decimalPlaces">Number of decimal places to show</param>
        /// <param name="justifyRight">Whether to right-justify the number</param>
        public void DisplayFloat(float value, int decimalPlaces = 2, bool justifyRight = true)
        {
            string format = $"F{Math.Min(decimalPlaces, 3)}";
            string text = value.ToString(format);

            if (text.Length > 5) // Including decimal point
            {
                if (value < 0 && value > -1)
                {
                    decimalPlaces = 3 - text.IndexOf('.');
                }
                else
                {
                    decimalPlaces = 4 - (text.IndexOf('.') > 0 ? text.IndexOf('.') : text.Length);
                }
                format = $"F{Math.Max(0, decimalPlaces)}";
                text = value.ToString(format);
            }

            DisplayNumber(text, justifyRight);
        }

        /// <summary>
        /// Displays a hexadecimal number (0000-FFFF)
        /// </summary>
        /// <param name="value">The value to display in hexadecimal</param>
        /// <param name="justifyRight">Whether to right-justify the number</param>
        public void DisplayHex(int value, bool justifyRight = true)
        {
            if (value < 0 || value > 0xFFFF)
            {
                return;
            }

            DisplayText(value.ToString("X"), justifyRight);
        }

        /// <summary>
        /// Displays a number string with optional decimal points
        /// </summary>
        /// <param name="number">The number string to display</param>
        /// <param name="justifyRight">Whether to right-justify the number</param>
        public void DisplayNumber(string number, bool justifyRight = true)
        {
            if (string.IsNullOrEmpty(number))
            {
                Clear();
                return;
            }

            Clear();

            var digits = new List<(char digit, bool hasDecimal)>();
            for (int i = 0; i < number.Length; i++)
            {
                if (number[i] == '.')
                {
                    if (digits.Count > 0)
                    {
                        digits[^1] = (digits[^1].digit, true);
                    }
                }
                else
                {
                    digits.Add((number[i], false));
                }
            }

            if (digits.Count > 4)
            {
                DisplayText("----", justifyRight);
                return;
            }

            int startPos = justifyRight ? 4 - digits.Count : 0;
            for (int i = 0; i < digits.Count; i++)
            {
                var (digit, hasDecimal) = digits[i];
                SetChar(startPos + i, digit, hasDecimal);
            }

            _display.WriteDisplay();
        }

        /// <summary>
        /// Sets the brightness of the display
        /// </summary>
        /// <param name="brightness">Brightness value between 0.0 and 1.0</param>
        public void SetBrightness(float brightness)
        {
            if (brightness < 0f || brightness > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(brightness), "Brightness must be between 0.0 and 1.0");
            }

            byte level = (byte)(brightness * 15);
            _display.SetBrightness(level);
            _display.WriteDisplay();
        }

        /// <summary>
        /// Sets the blink rate of the display
        /// </summary>
        /// <param name="rate">The desired blink rate</param>
        public void SetBlinkRate(Ht16k33.BlinkRate rate)
        {
            _display.SetBlinkRate(rate);
            _display.WriteDisplay();
        }

        /// <summary>
        /// Clears the display
        /// </summary>
        public void Clear()
        {
            _display.Clear();
            _display.WriteDisplay();
        }

        #endregion

        #region Private Methods

        private void SetChar(int position, char character, bool showDecimal = false)
        {
            // Validate position (0-3 for 4 digits)
            if (position < 0 || position > 3)
                return;

            // Validate character (only ASCII characters 32-127)
            if (!CharacterMap.TryGetValue(character, out ushort charBits))
            {
                charBits = CharacterMap['?'];
            }

            // Convert 16-bit character value to two bytes for the display buffer
            byte lowByte = (byte)(charBits & 0xFF);
            byte highByte = (byte)((charBits >> 8) & 0xFF);

            // Set decimal point if needed (bit 14)
            if (showDecimal || character == '.')
            {
                highByte |= 0x40; // Set bit 6 in high byte (bit 14 overall)
            }

            // Update display buffer - each character takes 2 bytes
            _display.SetBufferByte(position * 2, lowByte);
            _display.SetBufferByte(position * 2 + 1, highByte);
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        /// Releases all resources used by the SegmentDisplayController
        /// </summary>
        public void Dispose()
        {
            Clear();
            _display?.Dispose();
        }

        #endregion
    }
}