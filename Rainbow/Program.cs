using Rainbow.Samples;

namespace Rainbow;

/// <summary>
/// Provides the console application entry point for the Rainbow HAT samples.
/// </summary>
public static class Program
{
	/// <summary>
	/// Runs the Rainbow HAT sample tour.
	/// </summary>
	/// <param name="args">Command-line arguments supplied to the process.</param>
	/// <returns>A task that completes when the sample tour exits.</returns>
	public static async Task Main(string[] args)
	{
		Console.WriteLine(">> Pimoroni RainbowHAT meets .NET <<");
		Console.WriteLine("=====================================");
		Console.WriteLine("");
		Console.WriteLine("");

		await global::Rainbow.Samples.Samples.FullTourAsync();

		Console.ReadLine();
	}
}