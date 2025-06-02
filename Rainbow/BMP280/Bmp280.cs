using System.Device.I2c;
using Iot.Device.Bmxx80;
using UnitsNet;

namespace Rainbow.Bmp280
{
    /// <summary>
    /// Represents a BMP280 temperature and pressure sensor controller.
    /// This class manages the communication with a BMP280 sensor over I2C,
    /// providing temperature and pressure readings.
    /// </summary>
    public class Bmp280Controller : IDisposable
    {
        #region Private properties

        /// <summary>
        /// The underlying BMP280 sensor instance from the Iot.Device.Bmxx80 library.
        /// Handles direct communication with the hardware.
        /// </summary>
        private readonly Iot.Device.Bmxx80.Bmp280 _sensor;

        #endregion

        #region Public properties

        /// <summary>
        /// Gets the current temperature reading from the sensor.
        /// </summary>
        /// <value>
        /// A Temperature value representing the current ambient temperature.
        /// May be null if the reading fails or the sensor is not initialized.
        /// </value>
        /// <remarks>
        /// Each access to this property performs a new reading from the sensor.
        /// </remarks>
        public Temperature? Temperature => _sensor.Read().Temperature;

        /// <summary>
        /// Gets the current atmospheric pressure reading from the sensor.
        /// </summary>
        /// <value>
        /// A Pressure value representing the current atmospheric pressure.
        /// May be null if the reading fails or the sensor is not initialized.
        /// </value>
        /// <remarks>
        /// Each access to this property performs a new reading from the sensor.
        /// The pressure reading is temperature compensated for improved accuracy.
        /// </remarks>
        public Pressure? Pressure => _sensor.Read().Pressure;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Bmp280Controller"/> class.
        /// Sets up the I2C communication and configures the sensor for optimal readings.
        /// </summary>
        /// <remarks>
        /// The constructor:
        /// - Initializes I2C communication on bus 1 with address 0x77
        /// - Configures the sensor for ultra-high resolution temperature and pressure readings
        /// - Maintains the sensor instance for continuous readings
        /// </remarks>
        /// <exception cref="System.Device.I2c.I2cException">
        /// Thrown when communication with the sensor cannot be established.
        /// </exception>
        public Bmp280Controller()
        {
            var i2cSettings = new I2cConnectionSettings(1, 0x77);
            var i2cDevice = I2cDevice.Create(i2cSettings);
            
            _sensor = new Iot.Device.Bmxx80.Bmp280(i2cDevice)
            {
                TemperatureSampling = Sampling.UltraHighResolution,
                PressureSampling = Sampling.UltraHighResolution
            };
        }

        #endregion

        #region IDisposable implementation

        /// <summary>
        /// Releases all resources used by the <see cref="Bmp280Controller"/> instance.
        /// </summary>
        /// <remarks>
        /// This method:
        /// - Disposes of the underlying sensor instance
        /// - Closes the I2C communication
        /// - Should be called when the controller is no longer needed
        /// </remarks>
        public void Dispose()
        {
            _sensor?.Dispose();
        }

        #endregion
    }
}