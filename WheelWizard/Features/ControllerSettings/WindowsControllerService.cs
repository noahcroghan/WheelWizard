using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using SharpDX.XInput;

namespace WheelWizard.ControllerSettings;

/// <summary>
/// Windows-specific controller service that uses XInput directly for better controller detection
/// </summary>
public class WindowsControllerService : IControllerDetectionService
{
    private readonly ILogger<WindowsControllerService> _logger;
    private readonly Controller[] _controllers;
    private readonly Dictionary<int, ControllerInfo> _controllerInfos;
    private readonly Dictionary<int, ControllerState> _currentStates;
    private readonly Dictionary<int, ControllerState> _previousStates;
    private bool _disposed;

    public WindowsControllerService(ILogger<WindowsControllerService> logger)
    {
        _logger = logger;
        _controllers = new Controller[4]; // XInput supports up to 4 controllers
        _controllerInfos = new Dictionary<int, ControllerInfo>();
        _currentStates = new Dictionary<int, ControllerState>();
        _previousStates = new Dictionary<int, ControllerState>();

        _logger.LogInformation("Windows controller service initializing...");

        // Initialize XInput controllers
        for (int i = 0; i < 4; i++)
        {
            _controllers[i] = new Controller((UserIndex)i);
        }

        RefreshControllers();
        _logger.LogInformation("Windows controller service initialized successfully");
    }

    public List<ControllerInfo> GetConnectedControllers()
    {
        var controllers = _controllerInfos.Values.Where(c => c.IsConnected).ToList();
        _logger.LogDebug("GetConnectedControllers called, found {Count} connected controllers", controllers.Count);
        return controllers;
    }

    public void RefreshControllers()
    {
        try
        {
            _logger.LogInformation("Starting Windows controller refresh...");

            var previousControllers = new Dictionary<int, ControllerInfo>(_controllerInfos);
            _controllerInfos.Clear();

            // Check all 4 XInput controller slots
            for (int i = 0; i < 4; i++)
            {
                _logger.LogDebug("Checking XInput controller slot {Index}", i);

                try
                {
                    var controller = _controllers[i];
                    var isConnected = controller.IsConnected;

                    _logger.LogDebug("Controller {Index} - IsConnected: {Connected}", i, isConnected);

                    if (isConnected)
                    {
                        var capabilities = controller.GetCapabilities(DeviceQueryType.Any);

                        var controllerInfo = new ControllerInfo
                        {
                            Index = i,
                            Name = GetControllerName(capabilities, i),
                            IsConnected = true,
                            ControllerType = DetermineControllerType(capabilities),
                            VendorId = 0, // XInput doesn't expose these directly
                            ProductId = 0,
                        };

                        _controllerInfos[i] = controllerInfo;

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
                        _logger.LogDebug("Controller {Index} not connected", i);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking XInput controller slot {Index}", i);
                }
            }

            // Log disconnected controllers
            foreach (var kvp in previousControllers)
            {
                if (!_controllerInfos.ContainsKey(kvp.Key))
                {
                    _currentStates.Remove(kvp.Key);
                    _previousStates.Remove(kvp.Key);
                    _logger.LogInformation("Controller disconnected: {Name} at index {Index}", kvp.Value.Name, kvp.Key);
                }
            }

            _logger.LogInformation("Windows controller refresh completed. Found {Count} connected controllers", _controllerInfos.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing Windows controllers");
        }
    }

    private void LogControllerCapabilities(int index, Capabilities capabilities)
    {
        _logger.LogDebug("Controller {Index} capabilities:", index);
        _logger.LogDebug("  - Type: {Type}", capabilities.Type);
        _logger.LogDebug("  - SubType: {SubType}", capabilities.SubType);
        _logger.LogDebug("  - Flags: {Flags}", capabilities.Flags);
        _logger.LogDebug("  - Gamepad: {Gamepad}", capabilities.Gamepad);
        _logger.LogDebug("  - Vibration: {Vibration}", capabilities.Vibration);
    }

    private string GetControllerName(Capabilities capabilities, int index)
    {
        return capabilities.Type switch
        {
            DeviceType.Gamepad => $"Xbox Controller {index + 1}",
            _ => $"Generic Controller {index + 1}",
        };
    }

    private ControllerType DetermineControllerType(Capabilities capabilities)
    {
        return capabilities.Type switch
        {
            DeviceType.Gamepad => ControllerType.Xbox,
            _ => ControllerType.Generic,
        };
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
            foreach (var controllerIndex in _controllerInfos.Keys.ToList())
            {
                try
                {
                    var controller = _controllers[controllerIndex];
                    var state = controller.GetState();

                    if (state.PacketNumber > 0) // Valid state received
                    {
                        var newState = ConvertXInputState(state);
                        var oldState = _currentStates[controllerIndex];

                        // Log button state changes for debugging
                        if (newState.A != oldState.A)
                            _logger.LogDebug("Controller {Index} A button: {Pressed}", controllerIndex, newState.A);
                        if (newState.B != oldState.B)
                            _logger.LogDebug("Controller {Index} B button: {Pressed}", controllerIndex, newState.B);
                        if (newState.X != oldState.X)
                            _logger.LogDebug("Controller {Index} X button: {Pressed}", controllerIndex, newState.X);
                        if (newState.Y != oldState.Y)
                            _logger.LogDebug("Controller {Index} Y button: {Pressed}", controllerIndex, newState.Y);
                        if (newState.Start != oldState.Start)
                            _logger.LogDebug("Controller {Index} Start button: {Pressed}", controllerIndex, newState.Start);
                        if (newState.Back != oldState.Back)
                            _logger.LogDebug("Controller {Index} Back button: {Pressed}", controllerIndex, newState.Back);
                        if (newState.LeftShoulder != oldState.LeftShoulder)
                            _logger.LogDebug("Controller {Index} LeftShoulder button: {Pressed}", controllerIndex, newState.LeftShoulder);
                        if (newState.RightShoulder != oldState.RightShoulder)
                            _logger.LogDebug("Controller {Index} RightShoulder button: {Pressed}", controllerIndex, newState.RightShoulder);
                        if (newState.DPadUp != oldState.DPadUp)
                            _logger.LogDebug("Controller {Index} DPadUp button: {Pressed}", controllerIndex, newState.DPadUp);
                        if (newState.DPadDown != oldState.DPadDown)
                            _logger.LogDebug("Controller {Index} DPadDown button: {Pressed}", controllerIndex, newState.DPadDown);
                        if (newState.DPadLeft != oldState.DPadLeft)
                            _logger.LogDebug("Controller {Index} DPadLeft button: {Pressed}", controllerIndex, newState.DPadLeft);
                        if (newState.DPadRight != oldState.DPadRight)
                            _logger.LogDebug("Controller {Index} DPadRight button: {Pressed}", controllerIndex, newState.DPadRight);

                        _currentStates[controllerIndex] = newState;
                    }
                    else
                    {
                        // Controller disconnected during update
                        if (_controllerInfos.TryGetValue(controllerIndex, out var info))
                        {
                            info.IsConnected = false;
                            _logger.LogInformation("Controller {Name} disconnected during update", info.Name);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating XInput controller {Index}", controllerIndex);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating XInput controller states");
        }
    }

    private ControllerState ConvertXInputState(SharpDX.XInput.State xinputState)
    {
        var gamepad = xinputState.Gamepad;

        return new ControllerState
        {
            // Face buttons
            A = (gamepad.Buttons & GamepadButtonFlags.A) != 0,
            B = (gamepad.Buttons & GamepadButtonFlags.B) != 0,
            X = (gamepad.Buttons & GamepadButtonFlags.X) != 0,
            Y = (gamepad.Buttons & GamepadButtonFlags.Y) != 0,

            // Menu buttons
            Start = (gamepad.Buttons & GamepadButtonFlags.Start) != 0,
            Back = (gamepad.Buttons & GamepadButtonFlags.Back) != 0,

            // Shoulder buttons
            LeftShoulder = (gamepad.Buttons & GamepadButtonFlags.LeftShoulder) != 0,
            RightShoulder = (gamepad.Buttons & GamepadButtonFlags.RightShoulder) != 0,

            // Stick buttons
            LeftStick = (gamepad.Buttons & GamepadButtonFlags.LeftThumb) != 0,
            RightStick = (gamepad.Buttons & GamepadButtonFlags.RightThumb) != 0,

            // D-Pad
            DPadUp = (gamepad.Buttons & GamepadButtonFlags.DPadUp) != 0,
            DPadDown = (gamepad.Buttons & GamepadButtonFlags.DPadDown) != 0,
            DPadLeft = (gamepad.Buttons & GamepadButtonFlags.DPadLeft) != 0,
            DPadRight = (gamepad.Buttons & GamepadButtonFlags.DPadRight) != 0,

            // Analog sticks (XInput uses -32768 to 32767, convert to -1 to 1)
            LeftThumbstickX = gamepad.LeftThumbX / 32768.0f,
            LeftThumbstickY = gamepad.LeftThumbY / 32768.0f,
            RightThumbstickX = gamepad.RightThumbX / 32768.0f,
            RightThumbstickY = gamepad.RightThumbY / 32768.0f,

            // Triggers (XInput uses 0 to 255, convert to 0 to 1)
            LeftTrigger = gamepad.LeftTrigger / 255.0f,
            RightTrigger = gamepad.RightTrigger / 255.0f,
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

        _logger.LogInformation("Windows controller service disposing");
        _controllerInfos.Clear();
        _currentStates.Clear();
        _previousStates.Clear();
        _disposed = true;
    }

    /// <summary>
    /// Runs a comprehensive diagnostic on the XInput controller detection system.
    /// </summary>
    public void RunDiagnostics()
    {
        _logger.LogInformation("=== XINPUT CONTROLLER DETECTION DIAGNOSTICS ===");

        // Platform information
        _logger.LogInformation("Platform: {Platform}", Environment.OSVersion);
        _logger.LogInformation("Runtime: {Runtime}", Environment.Version);
        _logger.LogInformation("64-bit OS: {Is64BitOS}", Environment.Is64BitOperatingSystem);
        _logger.LogInformation("64-bit Process: {Is64BitProcess}", Environment.Is64BitProcess);

        // XInput information
        _logger.LogInformation("XInput controllers available: {Count}", _controllers.Length);

        // Check each XInput controller slot
        _logger.LogInformation("Checking all XInput controller slots...");
        for (int i = 0; i < _controllers.Length; i++)
        {
            try
            {
                var controller = _controllers[i];
                var isConnected = controller.IsConnected;

                _logger.LogInformation("Slot {Index}: IsConnected={Connected}", i, isConnected);

                if (isConnected)
                {
                    try
                    {
                        var capabilities = controller.GetCapabilities(DeviceQueryType.Any);
                        _logger.LogInformation("  - Controller type: {Type}", DetermineControllerType(capabilities));
                        _logger.LogInformation("  - Name: {Name}", GetControllerName(capabilities, i));
                        _logger.LogInformation("  - SubType: {SubType}", capabilities.SubType);
                        _logger.LogInformation("  - Flags: {Flags}", capabilities.Flags);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error getting capabilities for slot {Index}", i);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking slot {Index}", i);
            }
        }

        // Current state
        _logger.LogInformation("Current tracked controllers: {Count}", _controllerInfos.Count);
        foreach (var kvp in _controllerInfos)
        {
            _logger.LogInformation("  - Index {Index}: {Name} ({Type})", kvp.Key, kvp.Value.Name, kvp.Value.ControllerType);
        }

        _logger.LogInformation("=== END XINPUT DIAGNOSTICS ===");
    }
}
