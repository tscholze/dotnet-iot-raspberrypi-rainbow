using System.Diagnostics.Contracts;
using Rainbow.Apa102;
using Rainbow.Bmp280;
using Rainbow.Buttons;
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
        /// Displays a simple text on the segment display for a specified duration.
        /// The text defaults to ".NET" if no text is provided.
        /// The display will be cleared after the duration.
        /// </summary>
        /// <param name="text">Text to show in display.</param>
        /// <param name="duration">Duration to display the text (in milliseconds).</param>
        public static void ShowText(string text = ".NET", int duration = 5_000)
        {
            Console.WriteLine($"Displayed text '{text}' on the segment display.");
            using var segmentDisplayController = new SegmentDisplayController();
            segmentDisplayController.DisplayText(text);
            Thread.Sleep(duration);
            Console.WriteLine("Segment display text cleared.");
        }

        /// <summary>
        /// Runs an interactive demonstration of all Rainbow HAT hardware features in sequence.
        /// </summary>
        /// <remarks>
        /// This asynchronous method demonstrates the following components in order:
        /// 1. Status LEDs (Red, Green, Blue) - Individual control with 1-second intervals
        /// 2. APA102 RGB LED Strip - Color patterns and brightness control
        /// 3. BMP280 Environmental Sensor - Temperature and pressure readings
        /// 4. Alphanumeric Display - Scrolling text demonstration
        /// 5. Touch Buttons (A, B, C) - Event handling with LED feedback
        /// 
        /// Each component demonstration includes console output to explain what's happening.
        /// The tour uses a consistent pause interval (1000ms) between actions for clear visualization.
        /// Button events remain active until the method completes, toggling the green LED when pressed.
        /// </remarks>
        /// <returns>
        /// A Task representing the asynchronous operation. The task completes when the demonstration
        /// finishes, but button event handlers remain active.
        /// </returns>
        /// <example>
        /// Run the full hardware demonstration:
        /// <code>
        /// await Samples.FullTourAsync();
        /// </code>
        /// </example>
        public static async Task FullTourAsync()
        {
            const int PauseInterval = 1000;

            Console.WriteLine(">> Pimoroni RainbowHAT meets .NET <<");
            Console.WriteLine("=====================================");
            Console.WriteLine("");
            Console.WriteLine("");

            // 1. LEDs
            Console.WriteLine("Testing Rainbow HAT LEDs...");
            using var lights = new LightController();
            Console.WriteLine("     Turning Red on");
            lights.Red.TurnToState(Target.On);
            Thread.Sleep(PauseInterval);
            Console.WriteLine("     Turning Green on");
            lights.Green.TurnToState(Target.On);
            Thread.Sleep(PauseInterval);
            Console.WriteLine("     Turning Blue on");
            lights.Blue.TurnToState(Target.On);
            Thread.Sleep(PauseInterval);
            Console.WriteLine("     Turning all off");
            lights.Rgb(false, false, false);
            Thread.Sleep(PauseInterval);
            Console.WriteLine("");

            // 2. RGB Blinkt
            Console.WriteLine("Testing Rainbow HAT APA102");
            using var controller = new Apa102Controller();

            // Set all LEDs to red at half brightness
            Console.WriteLine("    Setting all LEDs to red at half brightness");
            controller.SetAll(255, 0, 0, 0.5f);
            controller.Show();
            Thread.Sleep(PauseInterval);

            // Set individual LEDs
            Console.WriteLine("    Setting individual LEDs to red, green, and blue");
            controller.SetPixel(0, 255, 0, 0);    // First LED red
            controller.SetPixel(1, 0, 255, 0);    // Second LED green
            controller.SetPixel(2, 0, 0, 255);    // Third LED blue
            controller.Show();
            Thread.Sleep(PauseInterval);

            // Clear all LEDs
            Console.WriteLine("    Clearing all LEDs");
            controller.Clear();
            controller.Show();
            Console.WriteLine("");
            Thread.Sleep(PauseInterval);

            // 3. BMP280 Sensor
            using var bmpController = new Bmp280Controller();
            Console.WriteLine("Testing Rainbow HAT BMP280...");
            Console.WriteLine($"    Temperature: {bmpController.Temperature?.DegreesCelsius:F0}°C");
            Console.WriteLine($"    Pressure: {bmpController.Pressure?.Hectopascals:F0}hPa");
            Console.WriteLine("");
            Thread.Sleep(PauseInterval);

            Console.WriteLine("Testing Rainbow HAT Segment Display...");
            using var segmentDisplayController = new SegmentDisplayController();
            await segmentDisplayController.DisplayScrollingText("Hello, Rainbow HAT!", loop: false);
            Console.WriteLine("");
            Thread.Sleep(PauseInterval);

            // 5. Buttons
            using var buttonController = new ButtonController();
            Console.WriteLine("Testing Rainbow HAT Buttons...");
            Console.WriteLine("    Press any button to see events ...");
            buttonController.ButtonPressed += (sender, e) =>
            {
                lights.Green.TurnToState(Target.Toggle);
                Console.WriteLine($"    Button {e.Button} pressed");
            };
            buttonController.ButtonReleased += (sender, e) =>
            {
                lights.Green.TurnToState(Target.Toggle);
                Console.WriteLine($"    Button {e.Button} released");
            };
            Console.WriteLine("");
            Console.WriteLine("");
        }
    }
}