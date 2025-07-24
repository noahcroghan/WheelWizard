using SDL2;

namespace WheelWizard.ControllerSettings;

public class UniplatformControllerService : IControllerService
{
    readonly object _lock = new();
    readonly Dictionary<int, IntPtr> _gamepads = new();
    readonly Dictionary<int, ControllerInfo> _infos = new();
    readonly Dictionary<int, ControllerState> _previous = new();
    readonly Dictionary<int, ControllerState> _current = new();
    Timer? _pollTimer;

    public event Action<ControllerInfo>? OnControllerConnected;
    public event Action<ControllerInfo>? OnControllerDisconnected;

    public UniplatformControllerService()
    {
        // Init SDL GameController + Joystick subsystems :contentReference[oaicite:1]{index=1}
        if (SDL.SDL_Init(SDL.SDL_INIT_GAMECONTROLLER | SDL.SDL_INIT_JOYSTICK) != 0)
            throw new InvalidOperationException($"SDL_Init failed: {SDL.SDL_GetError()}");

        // Open any already‑attached controllers
        var count = SDL.SDL_NumJoysticks();
        for (int i = 0; i < count; i++)
            if (SDL.SDL_IsGameController(i) == SDL.SDL_bool.SDL_TRUE)
                AddController(i);
        
        _pollTimer = new Timer(_ => PollEvents(), null, 0, 16);
    }

    void AddController(int sdlIndex)
    {
        var pad = SDL.SDL_GameControllerOpen(sdlIndex);
        if (pad == IntPtr.Zero)
            return;

        var joy = SDL.SDL_GameControllerGetJoystick(pad);
        int instanceId = SDL.SDL_JoystickInstanceID(joy);

        lock (_lock)
        {
            _gamepads[instanceId] = pad;
            var name = SDL.SDL_GameControllerName(pad) ?? "Unknown";
            var info = new ControllerInfo
            {
                Index = instanceId,
                Name = name,
                IsConnected = true,
                ControllerType = IdentifyType(name),
            };
            _infos[instanceId] = info;
            _previous[instanceId] = new ControllerState();
            _current[instanceId] = new ControllerState();
            OnControllerConnected?.Invoke(info);
        }
    }

    void RemoveController(int instanceId)
    {
        lock (_lock)
        {
            if (!_gamepads.TryGetValue(instanceId, out var pad))
                return;
            var info = _infos[instanceId];
            SDL.SDL_GameControllerClose(pad);
            _gamepads.Remove(instanceId);
            _infos.Remove(instanceId);
            _previous.Remove(instanceId);
            _current.Remove(instanceId);
            OnControllerDisconnected?.Invoke(info);
        }
    }

    void PollEvents()
    {
        SDL.SDL_PumpEvents();
        while (SDL.SDL_PollEvent(out var e) != 0)
        {
            switch (e.type)
            {
                case SDL.SDL_EventType.SDL_CONTROLLERDEVICEADDED:
                    AddController(e.cdevice.which);
                    break;
                case SDL.SDL_EventType.SDL_CONTROLLERDEVICEREMOVED:
                    RemoveController(e.cdevice.which);
                    break;
            }
        }
    }

    public void Update()
    {
        PollEvents();
        lock (_lock)
        {
            foreach (var kvp in _gamepads)
            {
                int id = kvp.Key;
                var pad = kvp.Value;

                // Slide current -> previous
                _previous[id] = new ControllerState(_current[id]);

                var st = _current[id];

                // Buttons
                st.A = SDL.SDL_GameControllerGetButton(pad, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_A) == 1;
                st.B = SDL.SDL_GameControllerGetButton(pad, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_B) == 1;
                st.X = SDL.SDL_GameControllerGetButton(pad, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_X) == 1;
                st.Y = SDL.SDL_GameControllerGetButton(pad, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_Y) == 1;
                st.LeftShoulder =
                    SDL.SDL_GameControllerGetButton(pad, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSHOULDER) == 1;
                st.RightShoulder =
                    SDL.SDL_GameControllerGetButton(pad, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSHOULDER) == 1;
                st.Back = SDL.SDL_GameControllerGetButton(pad, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_BACK) == 1;
                st.Start = SDL.SDL_GameControllerGetButton(pad, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_START) == 1;
                st.LeftStick = SDL.SDL_GameControllerGetButton(pad, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSTICK) == 1;
                st.RightStick = SDL.SDL_GameControllerGetButton(pad, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSTICK) == 1;
                st.DPadUp = SDL.SDL_GameControllerGetButton(pad, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_UP) == 1;
                st.DPadDown = SDL.SDL_GameControllerGetButton(pad, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_DOWN) == 1;
                st.DPadLeft = SDL.SDL_GameControllerGetButton(pad, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_LEFT) == 1;
                st.DPadRight = SDL.SDL_GameControllerGetButton(pad, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_RIGHT) == 1;

                // Axes (normalize from [-32768,32767] → [-1,1] or [0,1] for triggers)
                short lx = SDL.SDL_GameControllerGetAxis(pad, SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTX);
                short ly = SDL.SDL_GameControllerGetAxis(pad, SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTY);
                short rx = SDL.SDL_GameControllerGetAxis(pad, SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTX);
                short ry = SDL.SDL_GameControllerGetAxis(pad, SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTY);
                short lt = SDL.SDL_GameControllerGetAxis(pad, SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERLEFT);
                short rt = SDL.SDL_GameControllerGetAxis(pad, SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERRIGHT);

                st.LeftThumbstickX = lx / 32767f;
                st.LeftThumbstickY = ly / 32767f;
                st.RightThumbstickX = rx / 32767f;
                st.RightThumbstickY = ry / 32767f;
                st.LeftTrigger = Math.Max(0f, lt / 32767f);
                st.RightTrigger = Math.Max(0f, rt / 32767f);
            }
        }
    }

    public IReadOnlyList<ControllerInfo> GetConnectedControllers() => new List<ControllerInfo>(_infos.Values);

    public bool IsButtonPressed(int idx, ControllerButton btn)
    {
        var curr = _current[idx];
        var prev = _previous[idx];
        return btn switch
        {
            ControllerButton.A => curr.A && !prev.A,
            ControllerButton.B => curr.B && !prev.B,
            ControllerButton.X => curr.X && !prev.X,
            ControllerButton.Y => curr.Y && !prev.Y,
            ControllerButton.LeftShoulder => curr.LeftShoulder && !prev.LeftShoulder,
            ControllerButton.RightShoulder => curr.RightShoulder && !prev.RightShoulder,
            ControllerButton.Back => curr.Back && !prev.Back,
            ControllerButton.Start => curr.Start && !prev.Start,
            ControllerButton.LeftStick => curr.LeftStick && !prev.LeftStick,
            ControllerButton.RightStick => curr.RightStick && !prev.RightStick,
            ControllerButton.DPadUp => curr.DPadUp && !prev.DPadUp,
            ControllerButton.DPadDown => curr.DPadDown && !prev.DPadDown,
            ControllerButton.DPadLeft => curr.DPadLeft && !prev.DPadLeft,
            ControllerButton.DPadRight => curr.DPadRight && !prev.DPadRight,
            _ => false,
        };
    }

    public bool IsButtonHeld(int idx, ControllerButton btn) =>
        btn switch
        {
            ControllerButton.A => _current[idx].A,
            ControllerButton.B => _current[idx].B,
            ControllerButton.X => _current[idx].X,
            ControllerButton.Y => _current[idx].Y,
            ControllerButton.LeftShoulder => _current[idx].LeftShoulder,
            ControllerButton.RightShoulder => _current[idx].RightShoulder,
            ControllerButton.Back => _current[idx].Back,
            ControllerButton.Start => _current[idx].Start,
            ControllerButton.LeftStick => _current[idx].LeftStick,
            ControllerButton.RightStick => _current[idx].RightStick,
            ControllerButton.DPadUp => _current[idx].DPadUp,
            ControllerButton.DPadDown => _current[idx].DPadDown,
            ControllerButton.DPadLeft => _current[idx].DPadLeft,
            ControllerButton.DPadRight => _current[idx].DPadRight,
            _ => false,
        };

    public float GetAxisValue(int idx, AxisType axis) =>
        axis switch
        {
            AxisType.LeftThumbstickX => _current[idx].LeftThumbstickX,
            AxisType.LeftThumbstickY => _current[idx].LeftThumbstickY,
            AxisType.RightThumbstickX => _current[idx].RightThumbstickX,
            AxisType.RightThumbstickY => _current[idx].RightThumbstickY,
            AxisType.LeftTrigger => _current[idx].LeftTrigger,
            AxisType.RightTrigger => _current[idx].RightTrigger,
            _ => 0f,
        };
    
    public void Dispose()
    {
        _pollTimer?.Dispose();
        foreach (var pad in _gamepads.Values)
            SDL.SDL_GameControllerClose(pad);
        SDL.SDL_Quit();
    }

    static ControllerType IdentifyType(string n)
    {
        var name = n.ToLowerInvariant();
        return name.Contains("xbox") ? ControllerType.Xbox
            : name.Contains("playstation") || name.Contains("ps") ? ControllerType.PlayStation
            : name.Contains("nintendo") ? ControllerType.Nintendo
            : ControllerType.Generic;
    }
}
