using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework.Input;

namespace WheelWizard.ControllerSettings;

public class MonogameControllerService : IControllerService
{
    private readonly ILogger<MonogameControllerService> _logger;
    private readonly Dictionary<int, ControllerInfo> _connectedControllers = new();
    private readonly Dictionary<int, ControllerState> _currentStates = new();
    private readonly Dictionary<int, ControllerState> _previousStates = new();
    private bool _disposed;

    public event Action<ControllerInfo> OnControllerConnected;
    public event Action<ControllerInfo> OnControllerDisconnected;

    public MonogameControllerService(ILogger<MonogameControllerService> logger)
    {
        _logger = logger;
        _logger.LogInformation("MonoGame Controller Service initializing...");
        _logger.LogInformation("Platform: {Platform}", Environment.OSVersion);
        _logger.LogInformation("MonoGame MaximumGamePadCount: {MaxCount}", GamePad.MaximumGamePadCount);
        Update();
    }

    public void Update()
    {
        if (_disposed)
            return;

        // First, move current states to previous states for all known controllers
        foreach (var kvp in _currentStates)
        {
            // We can reuse the object to avoid allocations, but creating a new one is safer
            // if the state object were to be passed around.
            _previousStates[kvp.Key] = new ControllerState(kvp.Value);
        }

        // Scan all possible controller slots to detect changes and update states
        for (int i = 0; i < GamePad.MaximumGamePadCount; i++)
        {
            var state = GamePad.GetState(i);
            bool isCurrentlyConnected = _connectedControllers.ContainsKey(i);

            if (state.IsConnected)
            {
                if (!isCurrentlyConnected)
                {
                    ConnectController(i);
                }
                _currentStates[i] = ConvertGamePadState(state);
            }
            else
            {
                if (isCurrentlyConnected)
                {
                    // --- CONTROLLER DISCONNECTED ---
                    DisconnectController(i);
                }
            }
        }
    }

    private void ConnectController(int index)
    {
        var capabilities = GamePad.GetCapabilities(index);
        var controllerInfo = new ControllerInfo
        {
            Index = index,
            IsConnected = true,
            // Use the more reliable Identifier, fallback to a generic name
            Name = !string.IsNullOrWhiteSpace(capabilities.Identifier) ? capabilities.Identifier : $"Generic Controller {index + 1}",
            ControllerType = DetermineControllerType(capabilities),
        };

        _connectedControllers[index] = controllerInfo;
        _currentStates[index] = new ControllerState();
        _previousStates[index] = new ControllerState();

        _logger.LogInformation(
            "Controller connected: [{Index}] {Name} (Type: {Type})",
            index,
            controllerInfo.Name,
            controllerInfo.ControllerType
        );
        OnControllerConnected?.Invoke(controllerInfo);
    }

    private void DisconnectController(int index)
    {
        if (_connectedControllers.TryGetValue(index, out var controllerInfo))
        {
            _connectedControllers.Remove(index);
            _currentStates.Remove(index);
            _previousStates.Remove(index);

            _logger.LogInformation("Controller disconnected: [{Index}] {Name}", index, controllerInfo.Name);
            OnControllerDisconnected?.Invoke(controllerInfo);
        }
    }

    public IReadOnlyList<ControllerInfo> GetConnectedControllers()
    {
        return _connectedControllers.Values.ToList().AsReadOnly();
    }

    public bool IsButtonPressed(int controllerIndex, ControllerButton button)
    {
        return _currentStates.TryGetValue(controllerIndex, out var current)
            && _previousStates.TryGetValue(controllerIndex, out var previous)
            && IsButtonDown(current, button)
            && !IsButtonDown(previous, button);
    }

    public bool IsButtonHeld(int controllerIndex, ControllerButton button)
    {
        return _currentStates.TryGetValue(controllerIndex, out var current) && IsButtonDown(current, button);
    }

    public float GetAxisValue(int controllerIndex, AxisType axisType)
    {
        if (!_currentStates.TryGetValue(controllerIndex, out var state))
            return 0.0f;

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

    public void RunDiagnostics()
    {
        _logger.LogInformation("--- CONTROLLER DIAGNOSTICS ---");
        _logger.LogInformation("Platform: {Platform}", Environment.OSVersion);
        _logger.LogInformation("MonoGame Max GamePads: {MaxCount}", GamePad.MaximumGamePadCount);

        for (int i = 0; i < GamePad.MaximumGamePadCount; i++)
        {
            var caps = GamePad.GetCapabilities(i);
            var state = GamePad.GetState(i);
            _logger.LogInformation(
                "Slot {Index} | State Connected: {StateConn} | Caps Connected: {CapsConn} | Identifier: {ID}",
                i,
                state.IsConnected,
                caps.IsConnected,
                caps.Identifier ?? "N/A"
            );
        }

        _logger.LogInformation("Currently tracked controllers: {Count}", _connectedControllers.Count);
        foreach (var controller in _connectedControllers.Values)
        {
            _logger.LogInformation(" -> [{Index}] {Name} ({Type})", controller.Index, controller.Name, controller.ControllerType);
        }

        if (_connectedControllers.Count == 0)
        {
            _logger.LogWarning("No controllers detected. Ensure they are plugged in and recognized by your OS.");
        }
        _logger.LogInformation("--- END DIAGNOSTICS ---");
    }

    #region Helper and Conversion Methods

    private static ControllerType DetermineControllerType(GamePadCapabilities capabilities)
    {
        string id = capabilities.Identifier?.ToLowerInvariant() ?? "";

        if (id.Contains("xbox") || id.Contains("xinput"))
            return ControllerType.Xbox;
        if (id.Contains("dualsense") || id.Contains("dualshock"))
            return ControllerType.PlayStation;
        if (id.Contains("pro controller") || id.Contains("joy-con"))
            return ControllerType.Nintendo;

        // Fallback to capability-based guessing
        if (capabilities.HasAButton && capabilities.HasBButton && capabilities.HasXButton && capabilities.HasYButton)
        {
            return ControllerType.Xbox; // Most generic controllers follow the Xbox layout
        }
        return ControllerType.Generic;
    }

    private static ControllerState ConvertGamePadState(GamePadState gs)
    {
        return new ControllerState
        {
            A = gs.Buttons.A == ButtonState.Pressed,
            B = gs.Buttons.B == ButtonState.Pressed,
            X = gs.Buttons.X == ButtonState.Pressed,
            Y = gs.Buttons.Y == ButtonState.Pressed,
            Start = gs.Buttons.Start == ButtonState.Pressed,
            Back = gs.Buttons.Back == ButtonState.Pressed,
            LeftShoulder = gs.Buttons.LeftShoulder == ButtonState.Pressed,
            RightShoulder = gs.Buttons.RightShoulder == ButtonState.Pressed,
            LeftStick = gs.Buttons.LeftStick == ButtonState.Pressed,
            RightStick = gs.Buttons.RightStick == ButtonState.Pressed,
            DPadUp = gs.DPad.Up == ButtonState.Pressed,
            DPadDown = gs.DPad.Down == ButtonState.Pressed,
            DPadLeft = gs.DPad.Left == ButtonState.Pressed,
            DPadRight = gs.DPad.Right == ButtonState.Pressed,
            LeftThumbstickX = gs.ThumbSticks.Left.X,
            LeftThumbstickY = gs.ThumbSticks.Left.Y,
            RightThumbstickX = gs.ThumbSticks.Right.X,
            RightThumbstickY = gs.ThumbSticks.Right.Y,
            LeftTrigger = gs.Triggers.Left,
            RightTrigger = gs.Triggers.Right,
        };
    }

    private static bool IsButtonDown(ControllerState state, ControllerButton button)
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

    #endregion

    public void Dispose()
    {
        if (_disposed)
            return;
        _logger.LogInformation("MonoGame Controller Service disposing.");
        _connectedControllers.Clear();
        _currentStates.Clear();
        _previousStates.Clear();
        OnControllerConnected = null;
        OnControllerDisconnected = null;
        _disposed = true;
    }
}
