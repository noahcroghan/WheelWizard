using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using WheelWizard.ControllerSettings;
using WheelWizard.Features.Dolphin;
using WheelWizard.Shared.DependencyInjection;
using WheelWizard.Views.Popups.Base;

namespace WheelWizard.Views.Popups.ControllerManagement;

public partial class ButtonMappingTestWindow : PopupContent
{
    [Inject]
    private IControllerService ControllerService { get; set; } = null!;

    [Inject]
    private ILogger<ButtonMappingTestWindow> Logger { get; set; } = null!;

    private Dictionary<string, string> _mappings = new();
    private ControllerType _controllerType;
    private DispatcherTimer _testTimer;
    private Dictionary<string, Border> _resultElements = new();

    public ButtonMappingTestWindow()
        : base(true, false, true, "Button Mapping Test")
    {
        InitializeComponent();

        // Set up test timer
        _testTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16), // ~60fps
        };
        _testTimer.Tick += TestTimer_Tick;
    }

    public async Task ShowDialog(Dictionary<string, string> mappings, ControllerType controllerType)
    {
        try
        {
            Logger.LogInformation("Opening button mapping test window");

            _mappings = new Dictionary<string, string>(mappings);
            _controllerType = controllerType;

            // Initialize test results
            InitializeTestResults();

            // Start testing
            _testTimer.Start();

            // Show the dialog
            await ShowDialog<bool>();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error opening button mapping test window");
        }
    }

    private void InitializeTestResults()
    {
        try
        {
            TestResults.Children.Clear();
            _resultElements.Clear();

            // Create test result elements for each mapping
            foreach (var mapping in _mappings)
            {
                var dolphinButton = mapping.Key;
                var inputMapping = mapping.Value;

                var resultElement = CreateResultElement(dolphinButton, inputMapping);
                TestResults.Children.Add(resultElement);
                _resultElements[dolphinButton] = resultElement;
            }

            Logger.LogDebug("Initialized {Count} test result elements", _mappings.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error initializing test results");
        }
    }

    private Border CreateResultElement(string dolphinButton, string inputMapping)
    {
        var border = new Border
        {
            Background = this.FindResource("Neutral800") as Avalonia.Media.Brush,
            CornerRadius = new Avalonia.CornerRadius(6),
            Padding = new Avalonia.Thickness(10),
            Margin = new Avalonia.Thickness(0, 0, 0, 5),
        };

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), RowDefinitions = new RowDefinitions("Auto,Auto") };

        // Dolphin Button Name
        var dolphinButtonText = new TextBlock { Text = GetDolphinButtonDisplayName(dolphinButton), Classes = { "FormFieldLabel" } };
        Grid.SetColumn(dolphinButtonText, 0);
        Grid.SetRow(dolphinButtonText, 0);

        // Input Mapping
        var inputMappingText = new TextBlock
        {
            Text = inputMapping,
            Classes = { "BodyText" },
            Margin = new Avalonia.Thickness(0, 2, 0, 0),
        };
        Grid.SetColumn(inputMappingText, 0);
        Grid.SetRow(inputMappingText, 1);

        // Status Indicator
        var statusIndicator = new Border
        {
            Background = this.FindResource("Neutral700") as Avalonia.Media.Brush,
            CornerRadius = new Avalonia.CornerRadius(10),
            Width = 20,
            Height = 20,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };
        Grid.SetColumn(statusIndicator, 1);
        Grid.SetRow(statusIndicator, 0);
        Grid.SetRowSpan(statusIndicator, 2);

        grid.Children.Add(dolphinButtonText);
        grid.Children.Add(inputMappingText);
        grid.Children.Add(statusIndicator);

        border.Child = grid;

        // Store reference to status indicator
        border.Tag = statusIndicator;

        return border;
    }

    private string GetDolphinButtonDisplayName(string dolphinButton)
    {
        return dolphinButton switch
        {
            "Buttons/A" => "A Button",
            "Buttons/B" => "B Button",
            "Buttons/X" => "X Button",
            "Buttons/Y" => "Y Button",
            "Buttons/Z" => "Z Button",
            "Buttons/Start" => "Start Button",
            "D-Pad/Up" => "D-Pad Up",
            "D-Pad/Down" => "D-Pad Down",
            "D-Pad/Left" => "D-Pad Left",
            "D-Pad/Right" => "D-Pad Right",
            "Main Stick/Up" => "Main Stick Up",
            "Main Stick/Down" => "Main Stick Down",
            "Main Stick/Left" => "Main Stick Left",
            "Main Stick/Right" => "Main Stick Right",
            "C-Stick/Up" => "C-Stick Up",
            "C-Stick/Down" => "C-Stick Down",
            "C-Stick/Left" => "C-Stick Left",
            "C-Stick/Right" => "C-Stick Right",
            "Triggers/L" => "Left Trigger",
            "Triggers/R" => "Right Trigger",
            _ => dolphinButton,
        };
    }

    private void TestTimer_Tick(object? sender, EventArgs e)
    {
        try
        {
            var controllers = ControllerService.GetConnectedControllers();
            if (controllers.Count == 0)
            {
                StatusText.Text = "No controllers detected";
                return;
            }

            var controllerIndex = 0; // Use first controller
            var activeInputs = new List<string>();

            // Check all possible inputs
            CheckButtonInput(controllerIndex, ControllerButton.A, "`Button A`", activeInputs);
            CheckButtonInput(controllerIndex, ControllerButton.B, "`Button B`", activeInputs);
            CheckButtonInput(controllerIndex, ControllerButton.X, "`Button X`", activeInputs);
            CheckButtonInput(controllerIndex, ControllerButton.Y, "`Button Y`", activeInputs);
            CheckButtonInput(controllerIndex, ControllerButton.LeftShoulder, "`Shoulder L`", activeInputs);
            CheckButtonInput(controllerIndex, ControllerButton.RightShoulder, "`Shoulder R`", activeInputs);
            CheckButtonInput(controllerIndex, ControllerButton.DPadUp, "`Pad N`", activeInputs);
            CheckButtonInput(controllerIndex, ControllerButton.DPadDown, "`Pad S`", activeInputs);
            CheckButtonInput(controllerIndex, ControllerButton.DPadLeft, "`Pad W`", activeInputs);
            CheckButtonInput(controllerIndex, ControllerButton.DPadRight, "`Pad E`", activeInputs);

            // Check triggers
            var leftTrigger = ControllerService.GetAxisValue(controllerIndex, AxisType.LeftTrigger);
            var rightTrigger = ControllerService.GetAxisValue(controllerIndex, AxisType.RightTrigger);

            if (leftTrigger > 0.5f)
                activeInputs.Add("`Trigger L`");
            if (rightTrigger > 0.5f)
                activeInputs.Add("`Trigger R`");

            // Check analog sticks
            var leftStickX = ControllerService.GetAxisValue(controllerIndex, AxisType.LeftThumbstickX);
            var leftStickY = ControllerService.GetAxisValue(controllerIndex, AxisType.LeftThumbstickY);
            var rightStickX = ControllerService.GetAxisValue(controllerIndex, AxisType.RightThumbstickX);
            var rightStickY = ControllerService.GetAxisValue(controllerIndex, AxisType.RightThumbstickY);

            if (Math.Abs(leftStickX) > 0.5f)
                activeInputs.Add(leftStickX > 0 ? "`Left X+`" : "`Left X-`");
            if (Math.Abs(leftStickY) > 0.5f)
                activeInputs.Add(leftStickY > 0 ? "`Left Y+`" : "`Left Y-`");
            if (Math.Abs(rightStickX) > 0.5f)
                activeInputs.Add(rightStickX > 0 ? "`Right X+`" : "`Right X-`");
            if (Math.Abs(rightStickY) > 0.5f)
                activeInputs.Add(rightStickY > 0 ? "`Right Y+`" : "`Right Y-`");

            // Update status
            if (activeInputs.Count > 0)
            {
                StatusText.Text = $"Active inputs: {string.Join(", ", activeInputs)}";
            }
            else
            {
                StatusText.Text = "Press buttons to test mappings";
            }

            // Update result elements
            UpdateResultElements(activeInputs);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in test timer tick");
        }
    }

    private void CheckButtonInput(int controllerIndex, ControllerButton button, string inputName, List<string> activeInputs)
    {
        if (ControllerService.IsButtonPressed(controllerIndex, button))
        {
            activeInputs.Add(inputName);
        }
    }

    private void UpdateResultElements(List<string> activeInputs)
    {
        try
        {
            foreach (var kvp in _resultElements)
            {
                var dolphinButton = kvp.Key;
                var resultElement = kvp.Value;
                var statusIndicator = resultElement.Tag as Border;

                if (statusIndicator != null)
                {
                    var inputMapping = _mappings.TryGetValue(dolphinButton, out var mapping) ? mapping : "";
                    var isActive = activeInputs.Contains(inputMapping);

                    if (isActive)
                    {
                        statusIndicator.Background = this.FindResource("Primary500") as Avalonia.Media.Brush;
                    }
                    else
                    {
                        statusIndicator.Background = this.FindResource("Neutral700") as Avalonia.Media.Brush;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating result elements");
        }
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            _testTimer.Stop();
            Close();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error closing test window");
        }
    }

    protected override void BeforeClose()
    {
        _testTimer.Stop();
    }
}
