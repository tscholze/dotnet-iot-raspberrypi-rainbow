using Microsoft.Maui.Controls;

namespace Rainbow.Maui.Gtk;

/// <summary>
/// Defines the MAUI application for the GTK frontend.
/// </summary>
public class App : Application
{
    /// <summary>
    /// Creates the main application window.
    /// </summary>
    /// <param name="activationState">The activation state provided by the host.</param>
    /// <returns>A window hosting the main page.</returns>
    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new NavigationPage(new MainPage())
        {
            Title = "Rainbow HAT Control Hub"
        });
    }
}
