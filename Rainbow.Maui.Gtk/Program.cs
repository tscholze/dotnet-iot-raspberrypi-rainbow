using Platform.Maui.Linux.Gtk4.Platform;
using Microsoft.Maui.Hosting;

namespace Rainbow.Maui.Gtk;

/// <summary>
/// Provides the GTK entry point for the MAUI application.
/// </summary>
public class Program : GtkMauiApplication
{
    /// <summary>
    /// Creates the MAUI application for the GTK host.
    /// </summary>
    /// <returns>The configured <see cref="MauiApp"/> instance.</returns>
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    /// <summary>
    /// Starts the GTK application.
    /// </summary>
    /// <param name="args">Command-line arguments supplied to the process.</param>
    public static void Main(string[] args)
    {
        var app = new Program();
        app.Run(args);
    }
}
