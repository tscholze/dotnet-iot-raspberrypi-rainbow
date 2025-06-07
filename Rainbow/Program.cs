using System.Device.Gpio;
using System.Device.I2c;
using Iot.Device.Ads1115;
using Rainbow.Apa102;
using Rainbow.Lights;
using Rainbow.Samples;
using Rainbow.SegmentDisplay;


Console.WriteLine(">> Pimoroni RainbowHAT meets .NET <<");
Console.WriteLine("=====================================");
Console.WriteLine("");
Console.WriteLine("");

using var device = I2cDevice.Create(new I2cConnectionSettings(1, Ht16k33.DefaultAddress));
using var gpio = new GpioController();

using var segmentDisplayController = new SegmentDisplayController(device);
await segmentDisplayController.DisplayScrollingText("Hello, Rainbow HAT!");

Console.ReadLine();