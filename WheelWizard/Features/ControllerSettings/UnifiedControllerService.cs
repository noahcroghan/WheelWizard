using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework.Input;

namespace WheelWizard.ControllerSettings;

public class UnifiedControllerService : IControllerDetectionService
{
    private readonly ILogger<UnifiedControllerService> _logger;
    private readonly Dictionary<int, ControllerInfo> _controllers;
    private readonly Dictionary<int, ControllerState> _currentStates;
    private readonly Dictionary<int, ControllerState> _previousStates;
    private bool _disposed;

    public UnifiedControllerService(ILogger<UnifiedControllerService> logger)
    {
        _logger = logger;
        _controllers = new Dictionary<int, ControllerInfo>();
        _currentStates = new Dictionary<int, ControllerState>();
        _previousStates = new Dictionary<int, ControllerState>();

        _logger.LogInformation("Unified controller service initializing...");

        // Log platform and MonoGame information
        _logger.LogInformation("Platform: {Platform}", Environment.OSVersion);
        _logger.LogInformation("MonoGame MaximumGamePadCount: {MaxCount}", GamePad.MaximumGamePadCount);
        _logger.LogInformation("MonoGame GamePadCapabilities available: {HasCapabilities}", typeof(GamePadCapabilities) != null);

        RefreshControllers();
        _logger.LogInformation("Unified controller service initialized successfully");
    }

    public List<ControllerInfo> GetConnectedControllers()
    {
        var controllers = _controllers.Values.Where(c => c.IsConnected).ToList();
        _logger.LogDebug("GetConnectedControllers called, found {Count} connected controllers", controllers.Count);
        return controllers;
    }

    public void RefreshControllers()
    {
        try
        {
            _logger.LogInformation("Starting controller refresh...");

            // Check for common issues
            CheckForCommonIssues();

            var previousControllers = new Dictionary<int, ControllerInfo>(_controllers);
            _controllers.Clear();

            _logger.LogInformation("Checking {MaxCount} possible controller slots", GamePad.MaximumGamePadCount);

            // Check all possible controller slots
            for (int i = 0; i < GamePad.MaximumGamePadCount; i++)
            {
                _logger.LogDebug("Checking controller slot {Index}", i);

                try
                {
                    var capabilities = GamePad.GetCapabilities(i);
                    var state = GamePad.GetState(i);

                    _logger.LogDebug(
                        "Controller {Index} - Capabilities.IsConnected: {CapConnected}, State.IsConnected: {StateConnected}",
                        i,
                        capabilities.IsConnected,
                        state.IsConnected
                    );

                    if (capabilities.IsConnected && state.IsConnected)
                    {
                        var controllerInfo = new ControllerInfo
                        {
                            Index = i,
                            Name = GetControllerName(capabilities, i),
                            IsConnected = true,
                            ControllerType = DetermineControllerType(capabilities),
                            VendorId = 0, // MonoGame doesn't expose these
                            ProductId = 0,
                        };

                        _controllers[i] = controllerInfo;

                        // Initialize states if new controller
                        if (!_currentStates.ContainsKey(i))
                        {
                            _currentStates[i] = new ControllerState();
                            _previousStates[i] = new ControllerState();
                        }

                        // Log only new controllers
                        if (!previousControllers.ContainsKey(i))
                        {
                            _logger.LogInformation(
                                "Controller connected: {Name} at index {Index} (Type: {Type})",
                                controllerInfo.Name,
                                i,
                                controllerInfo.ControllerType
                            );

                            // Log detailed capabilities for debugging
                            LogControllerCapabilities(i, capabilities);
                        }
                    }
                    else
                    {
                        _logger.LogDebug(
                            "Controller {Index} not connected - Capabilities: {CapConnected}, State: {StateConnected}",
                            i,
                            capabilities.IsConnected,
                            state.IsConnected
                        );
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking controller slot {Index}", i);
                }
            }

            // Log disconnected controllers
            foreach (var kvp in previousControllers)
            {
                if (!_controllers.ContainsKey(kvp.Key))
                {
                    _currentStates.Remove(kvp.Key);
                    _previousStates.Remove(kvp.Key);
                    _logger.LogInformation("Controller disconnected: {Name} at index {Index}", kvp.Value.Name, kvp.Key);
                }
            }

            _logger.LogInformation("Controller refresh completed. Found {Count} connected controllers", _controllers.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing controllers");
        }
    }

    private void CheckForCommonIssues()
    {
        _logger.LogInformation("Checking for common controller detection issues...");

        // Check if we're running on Windows
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            _logger.LogInformation("Running on Windows - checking for XInput availability");

            // Check if XInput is available (this is what MonoGame uses on Windows)
            try
            {
                // Try to access XInput through reflection to see if it's available
                var xinputType = Type.GetType("Microsoft.Xna.Framework.Input.GamePad, MonoGame.Framework.DesktopGL");
                if (xinputType != null)
                {
                    _logger.LogInformation("XInput/MonoGame GamePad type found: {Type}", xinputType.FullName);
                }
                else
                {
                    _logger.LogWarning("XInput/MonoGame GamePad type not found - this may indicate a MonoGame issue");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking XInput availability");
            }
        }
        else
        {
            _logger.LogInformation("Running on non-Windows platform: {Platform}", Environment.OSVersion.Platform);
        }

        // Check if any controllers are detected at all
        var anyConnected = false;
        for (int i = 0; i < GamePad.MaximumGamePadCount; i++)
        {
            try
            {
                var capabilities = GamePad.GetCapabilities(i);
                if (capabilities.IsConnected)
                {
                    anyConnected = true;
                    break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error checking capabilities for slot {Index}", i);
            }
        }

        if (!anyConnected)
        {
            _logger.LogWarning("No controllers detected in any slot. This could indicate:");
            _logger.LogWarning("  - No controllers are physically connected");
            _logger.LogWarning("  - Controllers are not properly recognized by the system");
            _logger.LogWarning("  - MonoGame/XInput is not working properly");
            _logger.LogWarning("  - Driver issues with the controllers");
        }
        else
        {
            _logger.LogInformation("At least one controller appears to be connected");
        }
    }

    private void LogControllerCapabilities(int index, GamePadCapabilities capabilities)
    {
        _logger.LogDebug("Controller {Index} capabilities:", index);
        _logger.LogDebug("  - IsConnected: {IsConnected}", capabilities.IsConnected);
        _logger.LogDebug("  - HasAButton: {HasA}", capabilities.HasAButton);
        _logger.LogDebug("  - HasBButton: {HasB}", capabilities.HasBButton);
        _logger.LogDebug("  - HasXButton: {HasX}", capabilities.HasXButton);
        _logger.LogDebug("  - HasYButton: {HasY}", capabilities.HasYButton);
        _logger.LogDebug("  - HasLeftTrigger: {HasLeftTrigger}", capabilities.HasLeftTrigger);
        _logger.LogDebug("  - HasRightTrigger: {HasRightTrigger}", capabilities.HasRightTrigger);
        _logger.LogDebug("  - HasLeftXThumbStick: {HasLeftX}", capabilities.HasLeftXThumbStick);
        _logger.LogDebug("  - HasLeftYThumbStick: {HasLeftY}", capabilities.HasLeftYThumbStick);
        _logger.LogDebug("  - HasRightXThumbStick: {HasRightX}", capabilities.HasRightXThumbStick);
        _logger.LogDebug("  - HasRightYThumbStick: {HasRightY}", capabilities.HasRightYThumbStick);
        _logger.LogDebug("  - GamePadType: {GamePadType}", capabilities.GamePadType);
    }

    private string GetControllerName(GamePadCapabilities capabilities, int index)
    {
        // Try to determine controller type based on capabilities
        if (capabilities.HasAButton && capabilities.HasBButton && capabilities.HasXButton && capabilities.HasYButton)
        {
            if (
                capabilities.HasLeftTrigger
                && capabilities.HasRightTrigger
                && capabilities.HasLeftXThumbStick
                && capabilities.HasLeftYThumbStick
                && capabilities.HasRightXThumbStick
                && capabilities.HasRightYThumbStick
            )
            {
                return $"Xbox Controller {index + 1}";
            }
            else if (capabilities.HasLeftXThumbStick && capabilities.HasLeftYThumbStick)
            {
                return $"PlayStation Controller {index + 1}";
            }
        }

        return $"Generic Controller {index + 1}";
    }

    private ControllerType DetermineControllerType(GamePadCapabilities capabilities)
    {
        if (capabilities.HasAButton && capabilities.HasBButton && capabilities.HasXButton && capabilities.HasYButton)
        {
            if (
                capabilities.HasLeftTrigger
                && capabilities.HasRightTrigger
                && capabilities.HasLeftXThumbStick
                && capabilities.HasLeftYThumbStick
                && capabilities.HasRightXThumbStick
                && capabilities.HasRightYThumbStick
            )
            {
                return ControllerType.Xbox;
            }
            else if (capabilities.HasLeftXThumbStick && capabilities.HasLeftYThumbStick)
            {
                return ControllerType.PlayStation;
            }
        }

        return ControllerType.Generic;
    }

    public void Update()
    {
        try
        {
            // Store previous states
            foreach (var kvp in _currentStates.ToList())
            {
                _previousStates[kvp.Key] = kvp.Value;
            }

            // Update current states for all connected controllers
            foreach (var controllerIndex in _controllers.Keys.ToList())
            {
                try
                {
                    var gamepadState = GamePad.GetState(controllerIndex);

                    if (gamepadState.IsConnected)
                    {
                        _currentStates[controllerIndex] = ConvertGamePadState(gamepadState);
                    }
                    else
                    {
                        // Controller disconnected during update
                        if (_controllers.TryGetValue(controllerIndex, out var info))
                        {
                            info.IsConnected = false;
                            _logger.LogInformation("Controller {Name} disconnected during update", info.Name);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating controller {Index}", controllerIndex);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating controller states");
        }
    }

    private ControllerState ConvertGamePadState(GamePadState gamepadState)
    {
        return new ControllerState
        {
            // Face buttons
            A = gamepadState.Buttons.A == ButtonState.Pressed,
            B = gamepadState.Buttons.B == ButtonState.Pressed,
            X = gamepadState.Buttons.X == ButtonState.Pressed,
            Y = gamepadState.Buttons.Y == ButtonState.Pressed,

            // Menu buttons
            Start = gamepadState.Buttons.Start == ButtonState.Pressed,
            Back = gamepadState.Buttons.Back == ButtonState.Pressed,

            // Shoulder buttons
            LeftShoulder = gamepadState.Buttons.LeftShoulder == ButtonState.Pressed,
            RightShoulder = gamepadState.Buttons.RightShoulder == ButtonState.Pressed,

            // Stick buttons
            LeftStick = gamepadState.Buttons.LeftStick == ButtonState.Pressed,
            RightStick = gamepadState.Buttons.RightStick == ButtonState.Pressed,

            // D-Pad
            DPadUp = gamepadState.DPad.Up == ButtonState.Pressed,
            DPadDown = gamepadState.DPad.Down == ButtonState.Pressed,
            DPadLeft = gamepadState.DPad.Left == ButtonState.Pressed,
            DPadRight = gamepadState.DPad.Right == ButtonState.Pressed,

            // Analog sticks (MonoGame uses -1 to 1, Y is already correct)
            LeftThumbstickX = gamepadState.ThumbSticks.Left.X,
            LeftThumbstickY = gamepadState.ThumbSticks.Left.Y,
            RightThumbstickX = gamepadState.ThumbSticks.Right.X,
            RightThumbstickY = gamepadState.ThumbSticks.Right.Y,

            // Triggers (MonoGame uses 0 to 1)
            LeftTrigger = gamepadState.Triggers.Left,
            RightTrigger = gamepadState.Triggers.Right,
        };
    }

    public bool IsButtonPressed(int controllerIndex, ControllerButton button)
    {
        if (
            !_currentStates.TryGetValue(controllerIndex, out var current) || !_previousStates.TryGetValue(controllerIndex, out var previous)
        )
        {
            return false;
        }

        return IsButtonDown(current, button) && !IsButtonDown(previous, button);
    }

    public bool IsButtonHeld(int controllerIndex, ControllerButton button)
    {
        if (!_currentStates.TryGetValue(controllerIndex, out var current))
        {
            return false;
        }

        return IsButtonDown(current, button);
    }

    public float GetAxisValue(int controllerIndex, AxisType axisType)
    {
        if (!_currentStates.TryGetValue(controllerIndex, out var state))
        {
            return 0.0f;
        }

        return axisType switch
        {
            AxisType.LeftThumbstickX => state.LeftThumbstickX,
            AxisType.LeftThumbstickY => state.LeftThumbstickY,
            AxisType.RightThumbstickX => state.RightThumbstickX,
            AxisType.RightThumbstickY => state.RightThumbstickY,
            AxisType.LeftTrigger => state.LeftTrigger,
            AxisType.RightTrigger => state.RightTrigger,
            _ => 0.0f,
        };
    }

    private bool IsButtonDown(ControllerState state, ControllerButton button)
    {
        return button switch
        {
            ControllerButton.A => state.A,
            ControllerButton.B => state.B,
            ControllerButton.X => state.X,
            ControllerButton.Y => state.Y,
            ControllerButton.Start => state.Start,
            ControllerButton.Back => state.Back,
            ControllerButton.LeftShoulder => state.LeftShoulder,
            ControllerButton.RightShoulder => state.RightShoulder,
            ControllerButton.LeftStick => state.LeftStick,
            ControllerButton.RightStick => state.RightStick,
            ControllerButton.DPadUp => state.DPadUp,
            ControllerButton.DPadDown => state.DPadDown,
            ControllerButton.DPadLeft => state.DPadLeft,
            ControllerButton.DPadRight => state.DPadRight,
            _ => false,
        };
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _logger.LogInformation("Unified controller service disposing");
        _controllers.Clear();
        _currentStates.Clear();
        _previousStates.Clear();
        _disposed = true;
    }

    /// <summary>
    /// Runs a comprehensive diagnostic on the controller detection system.
    /// This method provides detailed information useful for debugging controller detection issues.
    /// </summary>
    public void RunDiagnostics()
    {
        _logger.LogInformation("=== CONTROLLER DETECTION DIAGNOSTICS ===");

        // Platform information
        _logger.LogInformation("Platform: {Platform}", Environment.OSVersion);
        _logger.LogInformation("Runtime: {Runtime}", Environment.Version);
        _logger.LogInformation("64-bit OS: {Is64BitOS}", Environment.Is64BitOperatingSystem);
        _logger.LogInformation("64-bit Process: {Is64BitProcess}", Environment.Is64BitProcess);

        // MonoGame information
        _logger.LogInformation("MonoGame MaximumGamePadCount: {MaxCount}", GamePad.MaximumGamePadCount);
        _logger.LogInformation("MonoGame GamePadCapabilities type: {Type}", typeof(GamePadCapabilities).FullName);
        _logger.LogInformation("MonoGame GamePadState type: {Type}", typeof(GamePadState).FullName);

        // Check each controller slot
        _logger.LogInformation("Checking all controller slots...");
        for (int i = 0; i < GamePad.MaximumGamePadCount; i++)
        {
            try
            {
                var capabilities = GamePad.GetCapabilities(i);
                var state = GamePad.GetState(i);

                _logger.LogInformation(
                    "Slot {Index}: Capabilities.IsConnected={CapConnected}, State.IsConnected={StateConnected}",
                    i,
                    capabilities.IsConnected,
                    state.IsConnected
                );

                if (capabilities.IsConnected)
                {
                    _logger.LogInformation("  - Controller type: {Type}", DetermineControllerType(capabilities));
                    _logger.LogInformation("  - Name: {Name}", GetControllerName(capabilities, i));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking slot {Index}", i);
            }
        }

        // Current state
        _logger.LogInformation("Current tracked controllers: {Count}", _controllers.Count);
        foreach (var kvp in _controllers)
        {
            _logger.LogInformation("  - Index {Index}: {Name} ({Type})", kvp.Key, kvp.Value.Name, kvp.Value.ControllerType);
        }

        _logger.LogInformation("=== END DIAGNOSTICS ===");
    }
}
