namespace WheelWizard.ControllerSettings;

public class ControllerInfo
{
    public int Index { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsConnected { get; set; }
    public ControllerType ControllerType { get; set; }

    // Note: VendorID and ProductID are often not available through the MonoGame API.
    // They are kept for potential future compatibility with other backends.
    public ushort VendorId { get; set; }
    public ushort ProductId { get; set; }
}

/// <summary>
/// Represents the complete input state of a controller at a single point in time.
/// </summary>
public class ControllerState
{
    // Buttons
    public bool A { get; set; }
    public bool B { get; set; }
    public bool X { get; set; }
    public bool Y { get; set; }
    public bool Start { get; set; }
    public bool Back { get; set; }
    public bool LeftShoulder { get; set; }
    public bool RightShoulder { get; set; }
    public bool LeftStick { get; set; }
    public bool RightStick { get; set; }

    // D-Pad
    public bool DPadUp { get; set; }
    public bool DPadDown { get; set; }
    public bool DPadLeft { get; set; }
    public bool DPadRight { get; set; }

    // Thumbsticks (-1.0 to 1.0)
    public float LeftThumbstickX { get; set; }
    public float LeftThumbstickY { get; set; }
    public float RightThumbstickX { get; set; }
    public float RightThumbstickY { get; set; }

    // Triggers (0.0 to 1.0)
    public float LeftTrigger { get; set; }
    public float RightTrigger { get; set; }

    /// <summary>
    /// Default constructor.
    /// </summary>
    public ControllerState() { }

    /// <summary>
    /// Copy constructor to create a deep copy of another state.
    /// </summary>
    public ControllerState(ControllerState other)
    {
        A = other.A;
        B = other.B;
        X = other.X;
        Y = other.Y;
        Start = other.Start;
        Back = other.Back;
        LeftShoulder = other.LeftShoulder;
        RightShoulder = other.RightShoulder;
        LeftStick = other.LeftStick;
        RightStick = other.RightStick;
        DPadUp = other.DPadUp;
        DPadDown = other.DPadDown;
        DPadLeft = other.DPadLeft;
        DPadRight = other.DPadRight;
        LeftThumbstickX = other.LeftThumbstickX;
        LeftThumbstickY = other.LeftThumbstickY;
        RightThumbstickX = other.RightThumbstickX;
        RightThumbstickY = other.RightThumbstickY;
        LeftTrigger = other.LeftTrigger;
        RightTrigger = other.RightTrigger;
    }
}

public enum ControllerType
{
    Unknown,
    Xbox,
    PlayStation,
    Nintendo,
    Generic,
}

public enum ControllerButton
{
    A,
    B,
    X,
    Y,
    Start,
    Back,
    LeftShoulder,
    RightShoulder,
    LeftStick,
    RightStick,
    DPadUp,
    DPadDown,
    DPadLeft,
    DPadRight,
}

public enum AxisType
{
    LeftThumbstickX,
    LeftThumbstickY,
    RightThumbstickX,
    RightThumbstickY,
    LeftTrigger,
    RightTrigger,
}
