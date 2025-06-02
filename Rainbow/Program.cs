// See https://aka.ms/new-console-template for more information
using System;
using Rainbow.Bmp280;
using Rainbow.Lights;
using static Rainbow.Lights.Light;

Console.WriteLine(">> Pimoroni RainbowHAT meets .NET <<");
Console.WriteLine("=====================================");
Console.WriteLine("");
Console.WriteLine("");


Console.WriteLine("Testing Rainbow HAT LEDs...");
using var lights = new Lights();
Console.WriteLine("     Turning Red on");
lights.Red.TurnToState(Target.On);
Thread.Sleep(500);
Console.WriteLine("     Turning Green on");
lights.Green.TurnToState(Target.On);
Thread.Sleep(500);
Console.WriteLine("     Turning Blue on");
lights.Blue.TurnToState(Target.On);
Console.WriteLine("");

using var bmpController = new Bmp280Controller();
Console.WriteLine("Testing Rainbow HAT BMP280...");
Console.WriteLine($"    Temperature: {bmpController.Temperature?.DegreesCelsius:F0}°C");
Console.WriteLine($"    Pressure: {bmpController.Pressure?.Hectopascals:F0}hPa");
Console.WriteLine("");
Console.WriteLine("");
Console.WriteLine("Press any key to exit ...");
Console.ReadKey();
Console.WriteLine("");
Console.WriteLine("");