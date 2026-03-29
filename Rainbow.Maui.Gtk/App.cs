using Microsoft.Maui.Controls;

namespace Rainbow.Maui.Gtk;

public class App : Application
{
    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new NavigationPage(new MainPage())
        {
            Title = "Rainbow HAT Control Hub"
        });
    }
}
