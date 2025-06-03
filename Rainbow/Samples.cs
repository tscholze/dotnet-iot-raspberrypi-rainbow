using System.Diagnostics.Contracts;
using Iot.Device.Blinkt;
using Rainbow.Apa102;
using Rainbow.Bmp280;
using Rainbow.Buttons;
using Rainbow.Buzzer;
using Rainbow.Lights;
using Rainbow.SegmentDisplay;
using static Rainbow.Lights.Light;

namespace Rainbow.Samples
{
    /// <summary>
    /// Sample code demonstrating the usage of the Rainbow library components.
    /// </summary>
    public static class Samples
    {
        /// <summary>
        /// Demonstrates the RGB LED functionality of the Rainbow HAT.
        /// </summary>
        /// <param name="duration">The duration in milliseconds to show each color (default: 1 second).</param>
        /// <remarks>
        /// Cycles through red, green, and blue colors individually, then turns off all LEDs.
        /// Each color is displayed for the specified duration before changing.
        /// </remarks>
        public static void ShowRgbLights(int duration = 1_000)
        {
            Console.WriteLine("Testing Rainbow HAT RGB lights...");
            using var lights = new LightController();
            lights.Rgb(true, false, false); // Red
            Thread.Sleep(duration);
            lights.Rgb(false, true, false); // Green
            Thread.Sleep(duration);
            lights.Rgb(false, false, true); // Blue
            Thread.Sleep(duration);
            lights.Rgb(false, false, false); // Off
        }

        /// <summary>
        /// Demonstrates the APA102 LED strip functionality of the Rainbow HAT.
        /// </summary>
        /// <param name="duration">The duration in milliseconds between LED pattern changes (default: 1 second).</param>
        /// <remarks>
        /// Shows three different LED patterns:
        /// 1. All LEDs red at half brightness
        /// 2. Individual LEDs in red, green, and blue
        /// 3. All LEDs off
        /// </remarks>
        public static void ShowBlinktLights(int duration = 1_000)
        {
            Console.WriteLine("Testing Rainbow HAT APA102");
            using var controller = new Apa102Controller();

            // Set all LEDs to red at half brightness
            Console.WriteLine("    Setting all LEDs to red at half brightness");
            controller.SetAll(255, 0, 0, 0.5f);
            controller.Show();
            Thread.Sleep(duration);

            // Set individual LEDs
            Console.WriteLine("    Setting individual LEDs to red, green, and blue");
            controller.SetPixel(0, 255, 0, 0);    // First LED red
            controller.SetPixel(1, 0, 255, 0);    // Second LED green
            controller.SetPixel(2, 0, 0, 255);    // Third LED blue
            controller.Show();
            Thread.Sleep(duration);

            // Clear all LEDs
            Console.WriteLine("    Clearing all LEDs");
            controller.Clear();
            controller.Show();
            Console.WriteLine("");
            Thread.Sleep(duration);
        }

        /// <summary>
        /// Displays the current temperature and pressure readings from the BMP280 sensor.
        /// </summary>
        /// <param name="duration">The duration in milliseconds to display the information (default: 1 second).</param>
        /// <remarks>
        /// Shows both temperature (in Celsius) and atmospheric pressure (in hectopascals).
        /// The sensor provides high-precision environmental measurements.
        /// </remarks>
        public static void ShowBmp280Information(int duration = 1_000)
        {
            using var bmpController = new Bmp280Controller();
            Console.WriteLine("Testing Rainbow HAT BMP280...");
            Console.WriteLine($"    Temperature: {bmpController.Temperature?.DegreesCelsius:F0}°C");
            Console.WriteLine($"    Pressure: {bmpController.Pressure?.Hectopascals:F0}hPa");
            Console.WriteLine("");
            Thread.Sleep(duration);
        }

        /// <summary>
        /// Displays text on the alphanumeric segment display.
        /// </summary>
        /// <param name="text">The text to display (default: ".NET"). Maximum 4 characters.</param>
        /// <param name="duration">The duration in milliseconds to show the text (default: 5 seconds).</param>
        /// <remarks>
        /// The display supports alphanumeric characters and some special symbols.
        /// Text longer than 4 characters will be truncated.
        /// </remarks>
        public static void ShowTextOnSegmentDisplay(string text = ".NET", int duration = 5_000)
        {
            Console.WriteLine("Testing Rainbow HAT Segment Display...");
            using var segmentDisplayController = new SegmentDisplayController();
            segmentDisplayController.DisplayText();
            Console.WriteLine("");
            Thread.Sleep(duration);
        }

        /// <summary>
        /// Demonstrates various buzzer melodies and sound capabilities.
        /// </summary>
        /// <param name="duration">The pause duration in milliseconds between different melodies (default: 1 second).</param>
        /// <remarks>
        /// Plays a sequence of sounds:
        /// 1. A single A4 note (440Hz)
        /// 2. The Tetris theme melody
        /// Each demonstration is separated by the specified pause duration.
        /// </remarks>
        public static void PlayBuzzerMelodies(int duration = 1_000)
        {
            using var buzzerController = new BuzzerController();
            Console.WriteLine("Testing Rainbow HAT Buzzer...");
            Console.WriteLine("    Playing A4 note for 1 second");
            buzzerController.PlayNote(440, 1.0);
            Thread.Sleep(duration);
            Console.WriteLine("    Playing Tetris theme melody");
            PlayTetrisMelody();
            Thread.Sleep(duration);
            Console.WriteLine("");
        }

        /// <summary>
        /// Plays a single A4 note (440Hz) on the buzzer.
        /// </summary>
        /// <remarks>
        /// Demonstrates basic buzzer functionality with a simple tone.
        /// The note plays for exactly one second before stopping.
        /// </remarks>
        public static void PlayA4Note()
        {
            Console.WriteLine("Playing melody on the buzzer...");
            using var buzzer = new BuzzerController();
            buzzer.PlayNote(440, 1.0); // A4 note for 1 second
            Thread.Sleep(1000);
            buzzer.Stop();
            Console.WriteLine("Melody playback finished.");
        }

        /// <summary>
        /// Plays the Tetris theme melody (Korobeiniki) on the Rainbow HAT buzzer.
        /// </summary>
        /// <remarks>
        /// Implements the first four measures of the classic Tetris theme.
        /// The melody includes:
        /// - First measure: E5-B4-C5-D5-C5-B4
        /// - Second measure: A4-A4-C5-E5-D5-C5
        /// - Third measure: B4(long)-C5-D5-E5
        /// - Fourth measure: C5-A4-A4(long)
        /// 
        /// Note durations are carefully timed to match the original melody:
        /// - Quarter notes = 0.25 seconds
        /// - Eighth notes = 0.125 seconds
        /// - Dotted half notes = 0.375 seconds
        /// - Half notes = 0.5 seconds
        /// </remarks>
        public static void PlayTetrisMelody()
        {
            Console.WriteLine("Playing Tetris theme melody...");
            using var buzzer = new BuzzerController();

            // Tetris theme notes and durations (in seconds)
            // First part of the melody
            (int note, double duration)[] melody = new[]
            {
                // First measure
                (76, 0.25),  // E5
                (71, 0.125), // B4
                (72, 0.125), // C5
                (74, 0.25),  // D5
                (72, 0.125), // C5
                (71, 0.125), // B4

                // Second measure
                (69, 0.25),  // A4
                (69, 0.125), // A4
                (72, 0.125), // C5
                (76, 0.25),  // E5
                (74, 0.125), // D5
                (72, 0.125), // C5

                // Third measure
                (71, 0.375), // B4 (longer)
                (72, 0.125), // C5
                (74, 0.25),  // D5
                (76, 0.25),  // E5

                // Fourth measure
                (72, 0.25),  // C5
                (69, 0.25),  // A4
                (69, 0.5),   // A4 (longer)
            };

            // Play each note in sequence
            foreach (var (note, duration) in melody)
            {
                buzzer.PlayMidiNote(note, duration);
                Thread.Sleep((int)(duration * 1000)); // Convert to milliseconds
            }

            // Small pause at the end
            Thread.Sleep(200);
            Console.WriteLine("Melody playback finished.");
        }

        /// <summary>
        /// Demonstrates all features of the Rainbow HAT in a guided tour.
        /// </summary>
        /// <remarks>
        /// This method showcases:
        /// 1. RGB LEDs - Individual control of red, green, and blue LEDs
        /// 2. APA102 RGB LED strip - Color and brightness control
        /// 3. BMP280 sensor - Temperature and pressure readings
        /// 4. Alphanumeric segment display - Text display capabilities
        /// 5. Buttons - Event handling for button press/release
        /// 
        /// Each demonstration includes a pause between actions to allow observation of the effects.
        /// The method uses proper resource cleanup with 'using' statements for all components.
        /// </remarks>
        /// <example>
        /// Run the full demonstration:
        /// <code>
        /// Samples.FullTour();
        /// </code>
        /// </example>
        public static void FullTour()
        {
            // 1. LEDs
            ShowRgbLights();

            // 2. RGB Blinkt
            ShowBlinktLights();

            // 3. BMP280 Sensor
            ShowBmp280Information();

            // 4. Segment Display
            ShowTextOnSegmentDisplay();

            // 5. Buzzer
            PlayBuzzerMelodies();

            // 6. Buttons
            using var buttonController = new ButtonController();
            Console.WriteLine("Testing Rainbow HAT Buttons...");
            Console.WriteLine("    Press any button to see events ...");
            buttonController.ButtonPressed += (sender, e) =>
            {
                Console.WriteLine($"    Button {e.Button} pressed");
            };
            buttonController.ButtonReleased += (sender, e) =>
            {
                Console.WriteLine($"    Button {e.Button} released");
            };
            Console.WriteLine("");
            Console.WriteLine("");
        }
    }
}