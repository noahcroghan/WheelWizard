using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using WheelWizard.ControllerSettings;
using WheelWizard.Shared.DependencyInjection;
using WheelWizard.Views;

namespace WheelWizard.Views.Popups.ControllerManagement;

public partial class ButtonMappingEditor : UserControlBase, INotifyPropertyChanged
{
    [Inject]
    private ControllerService ControllerService { get; set; } = null!;

    [Inject]
    private ILogger<ButtonMappingEditor> Logger { get; set; } = null!;

    private Dictionary<string, string> _originalMappings = new();
    private Dictionary<string, string> _currentMappings = new();
    private Dictionary<string, Border> _buttonElements = new();
    private string? _selectedButton;
    private bool _isDetecting = false;
    private DispatcherTimer _detectionTimer;
    private int _detectionTimeout = 0;
    private const int DETECTION_TIMEOUT_SECONDS = 10;

    // Button mapping definitions
    private readonly Dictionary<string, string> _dolphinButtonMap = new()
    {
        ["AButton"] = "Buttons/A",
        ["BButton"] = "Buttons/B",
        ["XButton"] = "Buttons/X",
        ["YButton"] = "Buttons/Y",
        ["LeftShoulder"] = "Buttons/Z",
        ["RightShoulder"] = "Buttons/Z",
        ["DPadUp"] = "D-Pad/Up",
        ["DPadDown"] = "D-Pad/Down",
        ["DPadLeft"] = "D-Pad/Left",
        ["DPadRight"] = "D-Pad/Right",
        ["LeftStick"] = "Main Stick/Up",
        ["RightStick"] = "C-Stick/Up",
        ["LeftTrigger"] = "Triggers/L",
        ["RightTrigger"] = "Triggers/R",
    };

    public ButtonMappingEditor()
    {
        InitializeComponent();
        DataContext = this;

        // Initialize button elements dictionary
        InitializeButtonElements();

        // Set up detection timer
        _detectionTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16), // ~60fps
        };
        _detectionTimer.Tick += DetectionTimer_Tick;
    }

    private void InitializeButtonElements()
    {
        _buttonElements["AButton"] = AButton;
        _buttonElements["BButton"] = BButton;
        _buttonElements["XButton"] = XButton;
        _buttonElements["YButton"] = YButton;
        _buttonElements["LeftShoulder"] = LeftShoulder;
        _buttonElements["RightShoulder"] = RightShoulder;
        _buttonElements["DPadUp"] = DPadUp;
        _buttonElements["DPadDown"] = DPadDown;
        _buttonElements["DPadLeft"] = DPadLeft;
        _buttonElements["DPadRight"] = DPadRight;
        _buttonElements["LeftStick"] = LeftStick;
        _buttonElements["RightStick"] = RightStick;
        _buttonElements["LeftTrigger"] = LeftTrigger;
        _buttonElements["RightTrigger"] = RightTrigger;
    }

    public void LoadMappings(Dictionary<string, string> mappings)
    {
        try
        {
            _originalMappings = new Dictionary<string, string>(mappings);
            _currentMappings = new Dictionary<string, string>(mappings);

            // Update button displays
            UpdateButtonDisplays();

            Logger.LogDebug("Loaded {Count} button mappings", mappings.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading button mappings");
        }
    }

    public Dictionary<string, string> GetMappings()
    {
        return new Dictionary<string, string>(_currentMappings);
    }

    public bool HasChanges()
    {
        if (_originalMappings.Count != _currentMappings.Count)
            return true;

        foreach (var kvp in _originalMappings)
        {
            if (!_currentMappings.TryGetValue(kvp.Key, out var currentValue) || currentValue != kvp.Value)
                return true;
        }

        return false;
    }

    public void ResetToDefaults()
    {
        try
        {
            // Get default mappings for the current controller type
            var defaultMapping = new Dictionary<string, string>
            {
                ["Buttons/A"] = "`Button A`",
                ["Buttons/B"] = "`Button B`",
                ["Buttons/X"] = "`Button X`",
                ["Buttons/Y"] = "`Button Y`",
                ["Buttons/Z"] = "`Shoulder R`",
                ["D-Pad/Up"] = "`Pad N`",
                ["D-Pad/Down"] = "`Pad S`",
                ["D-Pad/Left"] = "`Pad W`",
                ["D-Pad/Right"] = "`Pad E`",
                ["Main Stick/Up"] = "`Left Y+`",
                ["Main Stick/Down"] = "`Left Y-`",
                ["Main Stick/Left"] = "`Left X-`",
                ["Main Stick/Right"] = "`Left X+`",
                ["C-Stick/Up"] = "`Right Y+`",
                ["C-Stick/Down"] = "`Right Y-`",
                ["C-Stick/Left"] = "`Right X-`",
                ["C-Stick/Right"] = "`Right X+`",
                ["Triggers/L"] = "`Trigger L`",
                ["Triggers/R"] = "`Trigger R`",
            };

            _currentMappings = new Dictionary<string, string>(defaultMapping);
            UpdateButtonDisplays();

            Logger.LogInformation("Reset button mappings to defaults");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error resetting button mappings");
        }
    }

    private void Button_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border button)
        {
            var buttonName = GetButtonName(button);
            if (!string.IsNullOrEmpty(buttonName))
            {
                StartButtonDetection(buttonName);
            }
        }
    }

    private string? GetButtonName(Border button)
    {
        foreach (var kvp in _buttonElements)
        {
            if (kvp.Value == button)
                return kvp.Key;
        }
        return null;
    }

    private void StartButtonDetection(string buttonName)
    {
        try
        {
            if (_isDetecting)
            {
                StopButtonDetection();
            }

            _selectedButton = buttonName;
            _isDetecting = true;
            _detectionTimeout = 0;

            // Update UI
            SelectedButtonText.Text = GetButtonDisplayName(buttonName);
            CurrentMappingText.Text = GetCurrentMapping(buttonName);
            SelectedButtonInfo.IsVisible = true;
            NoSelectionInfo.IsVisible = false;
            DetectionStatus.IsVisible = true;

            // Highlight selected button
            UpdateButtonSelection(buttonName);

            // Start detection timer
            _detectionTimer.Start();

            Logger.LogDebug("Started button detection for: {Button}", buttonName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error starting button detection for '{Button}'", buttonName);
        }
    }

    private void StopButtonDetection()
    {
        try
        {
            _isDetecting = false;
            _detectionTimer.Stop();

            // Clear UI
            DetectionStatus.IsVisible = false;
            SelectedButtonInfo.IsVisible = false;
            NoSelectionInfo.IsVisible = true;

            // Clear button selection
            UpdateButtonSelection(null);

            _selectedButton = null;

            Logger.LogDebug("Stopped button detection");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error stopping button detection");
        }
    }

    private void DetectionTimer_Tick(object? sender, EventArgs e)
    {
        try
        {
            if (!_isDetecting || string.IsNullOrEmpty(_selectedButton))
                return;

            // Check for timeout
            _detectionTimeout++;
            if (_detectionTimeout > DETECTION_TIMEOUT_SECONDS * 60) // 60fps * seconds
            {
                StopButtonDetection();
                return;
            }

            // Check for controller input
            var detectedInput = DetectControllerInput();
            if (!string.IsNullOrEmpty(detectedInput))
            {
                AssignMapping(_selectedButton, detectedInput);
                StopButtonDetection();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in detection timer tick");
        }
    }

    private string? DetectControllerInput()
    {
        try
        {
            // Check for button presses
            var controllers = ControllerService.GetConnectedControllers();
            if (controllers.Count == 0)
                return null;

            var controllerIndex = 0; // Use first controller

            // Check face buttons
            if (ControllerService.IsButtonPressed(controllerIndex, ControllerButton.A))
                return "`Button A`";
            if (ControllerService.IsButtonPressed(controllerIndex, ControllerButton.B))
                return "`Button B`";
            if (ControllerService.IsButtonPressed(controllerIndex, ControllerButton.X))
                return "`Button X`";
            if (ControllerService.IsButtonPressed(controllerIndex, ControllerButton.Y))
                return "`Button Y`";

            // Check shoulder buttons
            if (ControllerService.IsButtonPressed(controllerIndex, ControllerButton.LeftShoulder))
                return "`Shoulder L`";
            if (ControllerService.IsButtonPressed(controllerIndex, ControllerButton.RightShoulder))
                return "`Shoulder R`";

            // Check D-Pad
            if (ControllerService.IsButtonPressed(controllerIndex, ControllerButton.DPadUp))
                return "`Pad N`";
            if (ControllerService.IsButtonPressed(controllerIndex, ControllerButton.DPadDown))
                return "`Pad S`";
            if (ControllerService.IsButtonPressed(controllerIndex, ControllerButton.DPadLeft))
                return "`Pad W`";
            if (ControllerService.IsButtonPressed(controllerIndex, ControllerButton.DPadRight))
                return "`Pad E`";

            // Check triggers
            var leftTrigger = ControllerService.GetAxisValue(controllerIndex, AxisType.LeftTrigger);
            var rightTrigger = ControllerService.GetAxisValue(controllerIndex, AxisType.RightTrigger);

            if (leftTrigger > 0.5f)
                return "`Trigger L`";
            if (rightTrigger > 0.5f)
                return "`Trigger R`";

            // Check analog sticks
            var leftStickX = ControllerService.GetAxisValue(controllerIndex, AxisType.LeftThumbstickX);
            var leftStickY = ControllerService.GetAxisValue(controllerIndex, AxisType.LeftThumbstickY);
            var rightStickX = ControllerService.GetAxisValue(controllerIndex, AxisType.RightThumbstickX);
            var rightStickY = ControllerService.GetAxisValue(controllerIndex, AxisType.RightThumbstickY);

            if (Math.Abs(leftStickX) > 0.5f || Math.Abs(leftStickY) > 0.5f)
            {
                if (Math.Abs(leftStickX) > Math.Abs(leftStickY))
                    return leftStickX > 0 ? "`Left X+`" : "`Left X-`";
                else
                    return leftStickY > 0 ? "`Left Y+`" : "`Left Y-`";
            }

            if (Math.Abs(rightStickX) > 0.5f || Math.Abs(rightStickY) > 0.5f)
            {
                if (Math.Abs(rightStickX) > Math.Abs(rightStickY))
                    return rightStickX > 0 ? "`Right X+`" : "`Right X-`";
                else
                    return rightStickY > 0 ? "`Right Y+`" : "`Right Y-`";
            }

            return null;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error detecting controller input");
            return null;
        }
    }

    private void AssignMapping(string buttonName, string input)
    {
        try
        {
            var dolphinButton = _dolphinButtonMap[buttonName];
            _currentMappings[dolphinButton] = input;

            // Update display
            UpdateButtonDisplays();
            CurrentMappingText.Text = input;

            Logger.LogInformation("Assigned mapping: {Button} -> {Input}", buttonName, input);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error assigning mapping for '{Button}'", buttonName);
        }
    }

    private void UpdateButtonDisplays()
    {
        try
        {
            foreach (var kvp in _buttonElements)
            {
                var buttonName = kvp.Key;
                var button = kvp.Value;

                if (_dolphinButtonMap.TryGetValue(buttonName, out var dolphinButton))
                {
                    var mapping = _currentMappings.TryGetValue(dolphinButton, out var value) ? value : "None";

                    // Update button tooltip
                    button.SetValue(ToolTip.TipProperty, $"{GetButtonDisplayName(buttonName)}: {mapping}");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating button displays");
        }
    }

    private void UpdateButtonSelection(string? selectedButton)
    {
        try
        {
            foreach (var kvp in _buttonElements)
            {
                var button = kvp.Value;
                var isSelected = kvp.Key == selectedButton;

                if (isSelected)
                {
                    button.Background = this.FindResource("Primary600") as Avalonia.Media.Brush;
                }
                else
                {
                    button.Background = this.FindResource("Neutral700") as Avalonia.Media.Brush;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating button selection");
        }
    }

    private string GetButtonDisplayName(string buttonName)
    {
        return buttonName switch
        {
            "AButton" => "A Button",
            "BButton" => "B Button",
            "XButton" => "X Button",
            "YButton" => "Y Button",
            "LeftShoulder" => "Left Shoulder",
            "RightShoulder" => "Right Shoulder",
            "DPadUp" => "D-Pad Up",
            "DPadDown" => "D-Pad Down",
            "DPadLeft" => "D-Pad Left",
            "DPadRight" => "D-Pad Right",
            "LeftStick" => "Left Stick",
            "RightStick" => "Right Stick",
            "LeftTrigger" => "Left Trigger",
            "RightTrigger" => "Right Trigger",
            _ => buttonName,
        };
    }

    private string GetCurrentMapping(string buttonName)
    {
        if (_dolphinButtonMap.TryGetValue(buttonName, out var dolphinButton))
        {
            return _currentMappings.TryGetValue(dolphinButton, out var mapping) ? mapping : "None";
        }
        return "None";
    }

    #region INotifyPropertyChanged Implementation

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #endregion
}
