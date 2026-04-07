using Platform.Maui.Linux.Gtk4.Hosting;
using Microsoft.Maui.Hosting;

namespace Rainbow.Maui.Gtk;

/// <summary>
/// Configures and builds the MAUI application.
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Creates the configured MAUI application instance.
    /// </summary>
    /// <returns>The built <see cref="MauiApp"/>.</returns>
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp
            .CreateBuilder()
            .UseMauiAppLinuxGtk4<App>();

        return builder.Build();
    }
}
