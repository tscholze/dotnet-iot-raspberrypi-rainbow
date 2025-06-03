namespace Rainbow.Apa102
{
    public partial class Apa102Controller
    {
        /// <summary>
        /// Represents a single APA102 LED pixel with color and brightness information.
        /// </summary>
        record struct Pixel
        {
            /// <summary>
            /// Gets or sets the red component (0-255).
            /// </summary>
            public byte Red { get; set; }

            /// <summary>
            /// Gets or sets the green component (0-255).
            /// </summary>
            public byte Green { get; set; }

            /// <summary>
            /// Gets or sets the blue component (0-255).
            /// </summary>
            public byte Blue { get; set; }

            /// <summary>
            /// Gets or sets the brightness value (0-31).
            /// </summary>
            public byte Brightness { get; set; }
        }
    }
}