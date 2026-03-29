using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Rainbow.Apa102;
using Rainbow.Bmp280;
using Rainbow.Buzzer;
using Rainbow.Lights;
using Rainbow.SegmentDisplay;
using static Rainbow.Lights.Light;

namespace Rainbow.Maui.Gtk;

/// <summary>
/// Main control hub page for the Pimoroni Rainbow HAT.
/// Provides GUI controls for all hardware features.
/// </summary>
public class MainPage : ContentPage
{
    private LightController? _lights;
    private Apa102Controller? _apa102;
    private Bmp280Controller? _bmp280;
    private BuzzerController? _buzzer;
    private SegmentDisplayController? _segmentDisplay;

    private readonly Label _statusLabel;
    private readonly Label _temperatureLabel;
    private readonly Label _pressureLabel;
    private readonly Entry _displayTextEntry;
    private readonly Label _frequencyValueLabel;
    private readonly Label _brightnessValueLabel;

    public MainPage()
    {
        Title = "🌈 Rainbow HAT Control Hub";

        _statusLabel = new Label
        {
            Text = "Ready — connect to hardware to begin.",
            TextColor = Colors.Gray,
            FontSize = 12,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };

        _temperatureLabel = new Label { Text = "—", FontSize = 18, HorizontalOptions = LayoutOptions.Center };
        _pressureLabel = new Label { Text = "—", FontSize = 18, HorizontalOptions = LayoutOptions.Center };
        _displayTextEntry = new Entry { Placeholder = "Enter text...", Text = ".NET", HorizontalOptions = LayoutOptions.Fill };
        _frequencyValueLabel = new Label { Text = "440 Hz", FontSize = 14, HorizontalOptions = LayoutOptions.Center };
        _brightnessValueLabel = new Label { Text = "20%", FontSize = 14, HorizontalOptions = LayoutOptions.Center };

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Spacing = 10,
                Padding = 20,
                Children =
                {
                    CreateHeader(),
                    _statusLabel,
                    CreateConnectionSection(),
                    CreateLedSection(),
                    CreateApa102Section(),
                    CreateSensorSection(),
                    CreateDisplaySection(),
                    CreateBuzzerSection(),
                    CreateFooter()
                }
            }
        };
    }

    private static View CreateHeader()
    {
        return new VerticalStackLayout
        {
            Spacing = 5,
            Children =
            {
                new Label
                {
                    Text = "🌈 Rainbow HAT",
                    FontSize = 28,
                    FontAttributes = FontAttributes.Bold,
                    HorizontalOptions = LayoutOptions.Center
                },
                new Label
                {
                    Text = "Pimoroni Rainbow HAT Control Hub for .NET",
                    FontSize = 14,
                    TextColor = Colors.Gray,
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, 0, 0, 10)
                }
            }
        };
    }

    private View CreateConnectionSection()
    {
        var connectButton = new Button
        {
            Text = "🔌 Connect to Hardware",
            BackgroundColor = Colors.DodgerBlue,
            TextColor = Colors.White,
            HorizontalOptions = LayoutOptions.Fill,
            Margin = new Thickness(0, 0, 0, 10)
        };
        connectButton.Clicked += OnConnectClicked;

        var disconnectButton = new Button
        {
            Text = "⏏ Disconnect",
            BackgroundColor = Colors.Gray,
            TextColor = Colors.White,
            HorizontalOptions = LayoutOptions.Fill,
            Margin = new Thickness(0, 0, 0, 5)
        };
        disconnectButton.Clicked += OnDisconnectClicked;

        return new Border
        {
            Stroke = Colors.LightGray,
            StrokeThickness = 1,
            Padding = 15,
            Content = new VerticalStackLayout
            {
                Spacing = 5,
                Children = { connectButton, disconnectButton }
            }
        };
    }

    private View CreateLedSection()
    {
        var redButton = new Button { Text = "🔴 Red", BackgroundColor = Colors.Red, TextColor = Colors.White };
        redButton.Clicked += (_, _) => ToggleLed(() => _lights?.Red.TurnToState(Target.Toggle));

        var greenButton = new Button { Text = "🟢 Green", BackgroundColor = Colors.Green, TextColor = Colors.White };
        greenButton.Clicked += (_, _) => ToggleLed(() => _lights?.Green.TurnToState(Target.Toggle));

        var blueButton = new Button { Text = "🔵 Blue", BackgroundColor = Colors.Blue, TextColor = Colors.White };
        blueButton.Clicked += (_, _) => ToggleLed(() => _lights?.Blue.TurnToState(Target.Toggle));

        var allOnButton = new Button { Text = "All On", HorizontalOptions = LayoutOptions.Fill };
        allOnButton.Clicked += (_, _) => ToggleLed(() => _lights?.Rgb(true, true, true));

        var allOffButton = new Button { Text = "All Off", HorizontalOptions = LayoutOptions.Fill };
        allOffButton.Clicked += (_, _) => ToggleLed(() => _lights?.Rgb(false, false, false));

        return CreateSection("💡 Status LEDs", new VerticalStackLayout
        {
            Spacing = 8,
            Children =
            {
                new HorizontalStackLayout
                {
                    Spacing = 10,
                    HorizontalOptions = LayoutOptions.Center,
                    Children = { redButton, greenButton, blueButton }
                },
                new HorizontalStackLayout
                {
                    Spacing = 10,
                    HorizontalOptions = LayoutOptions.Center,
                    Children = { allOnButton, allOffButton }
                }
            }
        });
    }

    private View CreateApa102Section()
    {
        var brightnessSlider = new Slider { Minimum = 0, Maximum = 100, Value = 20, HorizontalOptions = LayoutOptions.Fill };
        brightnessSlider.ValueChanged += (_, e) =>
        {
            _brightnessValueLabel.Text = $"{(int)e.NewValue}%";
        };

        var redStripButton = new Button { Text = "Red", BackgroundColor = Colors.Red, TextColor = Colors.White };
        redStripButton.Clicked += (_, _) => SetStripColor(255, 0, 0, (float)(brightnessSlider.Value / 100));

        var greenStripButton = new Button { Text = "Green", BackgroundColor = Colors.Green, TextColor = Colors.White };
        greenStripButton.Clicked += (_, _) => SetStripColor(0, 255, 0, (float)(brightnessSlider.Value / 100));

        var blueStripButton = new Button { Text = "Blue", BackgroundColor = Colors.Blue, TextColor = Colors.White };
        blueStripButton.Clicked += (_, _) => SetStripColor(0, 0, 255, (float)(brightnessSlider.Value / 100));

        var rainbowButton = new Button { Text = "🌈 Rainbow" };
        rainbowButton.Clicked += (_, _) => SetRainbowPattern((float)(brightnessSlider.Value / 100));

        var clearStripButton = new Button { Text = "Clear" };
        clearStripButton.Clicked += (_, _) => ClearStrip();

        return CreateSection("🎨 APA102 LED Strip", new VerticalStackLayout
        {
            Spacing = 8,
            Children =
            {
                new Label { Text = "Brightness:", FontSize = 14 },
                brightnessSlider,
                _brightnessValueLabel,
                new HorizontalStackLayout
                {
                    Spacing = 10,
                    HorizontalOptions = LayoutOptions.Center,
                    Children = { redStripButton, greenStripButton, blueStripButton }
                },
                new HorizontalStackLayout
                {
                    Spacing = 10,
                    HorizontalOptions = LayoutOptions.Center,
                    Children = { rainbowButton, clearStripButton }
                }
            }
        });
    }

    private View CreateSensorSection()
    {
        var readButton = new Button { Text = "📡 Read Sensor" };
        readButton.Clicked += OnReadSensorClicked;

        return CreateSection("🌡️ BMP280 Sensor", new VerticalStackLayout
        {
            Spacing = 8,
            Children =
            {
                new HorizontalStackLayout
                {
                    Spacing = 10,
                    HorizontalOptions = LayoutOptions.Center,
                    Children =
                    {
                        new Label { Text = "Temperature:", FontSize = 14, VerticalOptions = LayoutOptions.Center },
                        _temperatureLabel
                    }
                },
                new HorizontalStackLayout
                {
                    Spacing = 10,
                    HorizontalOptions = LayoutOptions.Center,
                    Children =
                    {
                        new Label { Text = "Pressure:", FontSize = 14, VerticalOptions = LayoutOptions.Center },
                        _pressureLabel
                    }
                },
                readButton
            }
        });
    }

    private View CreateDisplaySection()
    {
        var displayButton = new Button { Text = "📺 Show Text" };
        displayButton.Clicked += OnDisplayTextClicked;

        var scrollButton = new Button { Text = "📜 Scroll Text" };
        scrollButton.Clicked += OnScrollTextClicked;

        var clearDisplayButton = new Button { Text = "Clear" };
        clearDisplayButton.Clicked += OnClearDisplayClicked;

        return CreateSection("📊 Segment Display", new VerticalStackLayout
        {
            Spacing = 8,
            Children =
            {
                _displayTextEntry,
                new HorizontalStackLayout
                {
                    Spacing = 10,
                    HorizontalOptions = LayoutOptions.Center,
                    Children = { displayButton, scrollButton, clearDisplayButton }
                }
            }
        });
    }

    private View CreateBuzzerSection()
    {
        var frequencySlider = new Slider { Minimum = 200, Maximum = 2000, Value = 440, HorizontalOptions = LayoutOptions.Fill };
        frequencySlider.ValueChanged += (_, e) =>
        {
            _frequencyValueLabel.Text = $"{(int)e.NewValue} Hz";
        };

        var playButton = new Button { Text = "🔊 Play Tone" };
        playButton.Clicked += (_, _) => PlayBuzzer((int)frequencySlider.Value);

        var stopButton = new Button { Text = "🔇 Stop" };
        stopButton.Clicked += (_, _) => StopBuzzer();

        return CreateSection("🔊 Buzzer", new VerticalStackLayout
        {
            Spacing = 8,
            Children =
            {
                new Label { Text = "Frequency:", FontSize = 14 },
                frequencySlider,
                _frequencyValueLabel,
                new HorizontalStackLayout
                {
                    Spacing = 10,
                    HorizontalOptions = LayoutOptions.Center,
                    Children = { playButton, stopButton }
                }
            }
        });
    }

    private static View CreateFooter()
    {
        return new Label
        {
            Text = "Rainbow HAT .NET IoT — Powered by Platform.Maui.Linux.Gtk4",
            FontSize = 11,
            TextColor = Colors.Gray,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 20, 0, 10)
        };
    }

    private static Border CreateSection(string title, View content)
    {
        return new Border
        {
            Stroke = Colors.LightGray,
            StrokeThickness = 1,
            Padding = 15,
            Content = new VerticalStackLayout
            {
                Spacing = 10,
                Children =
                {
                    new Label
                    {
                        Text = title,
                        FontSize = 18,
                        FontAttributes = FontAttributes.Bold
                    },
                    content
                }
            }
        };
    }

    private void OnConnectClicked(object? sender, EventArgs e)
    {
        try
        {
            _lights ??= new LightController();
            _apa102 ??= new Apa102Controller();
            _bmp280 ??= new Bmp280Controller();
            _buzzer ??= new BuzzerController();
            _segmentDisplay ??= new SegmentDisplayController();

            SetStatus("Connected to Rainbow HAT hardware.", Colors.Green);
        }
        catch (Exception ex)
        {
            SetStatus($"Connection failed: {ex.Message}", Colors.Red);
        }
    }

    private void OnDisconnectClicked(object? sender, EventArgs e)
    {
        DisposeControllers();
        SetStatus("Disconnected from hardware.", Colors.Gray);
    }

    private void ToggleLed(Action action)
    {
        if (_lights is null)
        {
            SetStatus("Not connected — press Connect first.", Colors.Orange);
            return;
        }

        try
        {
            action();
            SetStatus("LED toggled.", Colors.Green);
        }
        catch (Exception ex)
        {
            SetStatus($"LED error: {ex.Message}", Colors.Red);
        }
    }

    private void SetStripColor(byte r, byte g, byte b, float brightness)
    {
        if (_apa102 is null)
        {
            SetStatus("Not connected — press Connect first.", Colors.Orange);
            return;
        }

        try
        {
            _apa102.SetAll(r, g, b, brightness);
            _apa102.Show();
            SetStatus($"LED strip set to RGB({r},{g},{b}) at {brightness:P0}.", Colors.Green);
        }
        catch (Exception ex)
        {
            SetStatus($"APA102 error: {ex.Message}", Colors.Red);
        }
    }

    private void SetRainbowPattern(float brightness)
    {
        if (_apa102 is null)
        {
            SetStatus("Not connected — press Connect first.", Colors.Orange);
            return;
        }

        try
        {
            _apa102.SetPixel(0, 255, 0, 0, brightness);
            _apa102.SetPixel(1, 255, 127, 0, brightness);
            _apa102.SetPixel(2, 255, 255, 0, brightness);
            _apa102.SetPixel(3, 0, 255, 0, brightness);
            _apa102.SetPixel(4, 0, 0, 255, brightness);
            _apa102.SetPixel(5, 75, 0, 130, brightness);
            _apa102.SetPixel(6, 148, 0, 211, brightness);
            _apa102.Show();
            SetStatus("Rainbow pattern displayed.", Colors.Green);
        }
        catch (Exception ex)
        {
            SetStatus($"APA102 error: {ex.Message}", Colors.Red);
        }
    }

    private void ClearStrip()
    {
        if (_apa102 is null)
        {
            SetStatus("Not connected — press Connect first.", Colors.Orange);
            return;
        }

        try
        {
            _apa102.Clear();
            _apa102.Show();
            SetStatus("LED strip cleared.", Colors.Green);
        }
        catch (Exception ex)
        {
            SetStatus($"APA102 error: {ex.Message}", Colors.Red);
        }
    }

    private void OnReadSensorClicked(object? sender, EventArgs e)
    {
        if (_bmp280 is null)
        {
            SetStatus("Not connected — press Connect first.", Colors.Orange);
            return;
        }

        try
        {
            var temp = _bmp280.Temperature;
            var pressure = _bmp280.Pressure;

            _temperatureLabel.Text = temp is not null
                ? $"{temp.Value.DegreesCelsius:F1} °C"
                : "N/A";

            _pressureLabel.Text = pressure is not null
                ? $"{pressure.Value.Hectopascals:F0} hPa"
                : "N/A";

            SetStatus("Sensor data read successfully.", Colors.Green);
        }
        catch (Exception ex)
        {
            SetStatus($"Sensor error: {ex.Message}", Colors.Red);
        }
    }

    private void OnDisplayTextClicked(object? sender, EventArgs e)
    {
        if (_segmentDisplay is null)
        {
            SetStatus("Not connected — press Connect first.", Colors.Orange);
            return;
        }

        try
        {
            var text = _displayTextEntry.Text;
            if (string.IsNullOrWhiteSpace(text))
            {
                text = ".NET";
            }

            _segmentDisplay.DisplayText(text);
            SetStatus($"Displaying '{text}'.", Colors.Green);
        }
        catch (Exception ex)
        {
            SetStatus($"Display error: {ex.Message}", Colors.Red);
        }
    }

    private async void OnScrollTextClicked(object? sender, EventArgs e)
    {
        if (_segmentDisplay is null)
        {
            SetStatus("Not connected — press Connect first.", Colors.Orange);
            return;
        }

        try
        {
            var text = _displayTextEntry.Text;
            if (string.IsNullOrWhiteSpace(text))
            {
                text = "Hello, Rainbow HAT!";
            }

            SetStatus($"Scrolling '{text}'...", Colors.DodgerBlue);
            await _segmentDisplay.DisplayScrollingText(text, loop: false);
            SetStatus("Scrolling complete.", Colors.Green);
        }
        catch (Exception ex)
        {
            SetStatus($"Display error: {ex.Message}", Colors.Red);
        }
    }

    private void OnClearDisplayClicked(object? sender, EventArgs e)
    {
        if (_segmentDisplay is null)
        {
            SetStatus("Not connected — press Connect first.", Colors.Orange);
            return;
        }

        try
        {
            _segmentDisplay.Clear();
            SetStatus("Display cleared.", Colors.Green);
        }
        catch (Exception ex)
        {
            SetStatus($"Display error: {ex.Message}", Colors.Red);
        }
    }

    private void PlayBuzzer(int frequency)
    {
        if (_buzzer is null)
        {
            SetStatus("Not connected — press Connect first.", Colors.Orange);
            return;
        }

        try
        {
            _buzzer.PlayNote(frequency, 1.0);
            SetStatus($"Playing {frequency} Hz tone.", Colors.Green);
        }
        catch (Exception ex)
        {
            SetStatus($"Buzzer error: {ex.Message}", Colors.Red);
        }
    }

    private void StopBuzzer()
    {
        if (_buzzer is null)
        {
            SetStatus("Not connected — press Connect first.", Colors.Orange);
            return;
        }

        try
        {
            _buzzer.Stop();
            SetStatus("Buzzer stopped.", Colors.Green);
        }
        catch (Exception ex)
        {
            SetStatus($"Buzzer error: {ex.Message}", Colors.Red);
        }
    }

    private void SetStatus(string message, Color color)
    {
        _statusLabel.Text = message;
        _statusLabel.TextColor = color;
    }

    private void DisposeControllers()
    {
        _lights?.Dispose();
        _lights = null;
        _apa102?.Dispose();
        _apa102 = null;
        _bmp280?.Dispose();
        _bmp280 = null;
        _buzzer?.Dispose();
        _buzzer = null;
        _segmentDisplay?.Dispose();
        _segmentDisplay = null;
    }
}
