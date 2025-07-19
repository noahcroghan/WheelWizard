using Microsoft.Extensions.Logging;

namespace WheelWizard.ControllerSettings;

public class ControllerService : IDisposable
{
    private readonly IControllerDetectionService _detectionService;
    private readonly ILogger<ControllerService> _logger;
    private bool _disposed;

    public ControllerService(ILogger<ControllerService> logger, ILoggerFactory loggerFactory)
    {
        _logger = logger;
        _logger.LogInformation("ControllerService initializing...");

        // Choose the appropriate controller detection service based on platform
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            _logger.LogInformation("Running on Windows - using XInput controller detection");
            var windowsLogger = loggerFactory.CreateLogger<WindowsControllerService>();
            _detectionService = new WindowsControllerService(windowsLogger);
        }
        else
        {
            _logger.LogInformation("Running on non-Windows platform - using MonoGame controller detection");
            var unifiedLogger = loggerFactory.CreateLogger<UnifiedControllerService>();
            _detectionService = new UnifiedControllerService(unifiedLogger);
        }

        _logger.LogInformation("ControllerService initialized successfully");
    }

    public List<ControllerInfo> GetConnectedControllers()
    {
        var controllers = _detectionService.GetConnectedControllers();
        return controllers;
    }

    public void Update()
    {
        _logger.LogTrace("ControllerService.Update() called");
        _detectionService.Update();
    }

    public void RefreshControllers()
    {
        _logger.LogInformation("ControllerService.RefreshControllers() called");
        _detectionService.RefreshControllers();
    }

    public bool IsButtonPressed(int controllerIndex, ControllerButton button)
    {
        var result = _detectionService.IsButtonPressed(controllerIndex, button);
        if (result)
        {
            _logger.LogDebug("Button {Button} pressed on controller {Index}", button, controllerIndex);
        }
        return result;
    }

    public bool IsButtonHeld(int controllerIndex, ControllerButton button)
    {
        return _detectionService.IsButtonHeld(controllerIndex, button);
    }

    public float GetAxisValue(int controllerIndex, AxisType axisType)
    {
        var value = _detectionService.GetAxisValue(controllerIndex, axisType);
        _logger.LogTrace("Axis {Axis} value on controller {Index}: {Value}", axisType, controllerIndex, value);
        return value;
    }

    /// <summary>
    /// Runs comprehensive diagnostics on the controller detection system.
    /// This is useful for debugging controller detection issues.
    /// </summary>
    public void RunDiagnostics()
    {
        _logger.LogInformation("Running controller diagnostics...");
        _detectionService.RunDiagnostics();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _logger.LogInformation("ControllerService disposing");
        _detectionService?.Dispose();
        _disposed = true;
    }
}

/// <summary>
/// Interface for controller detection services to allow platform-specific implementations
/// </summary>
public interface IControllerDetectionService : IDisposable
{
    List<ControllerInfo> GetConnectedControllers();
    void Update();
    void RefreshControllers();
    bool IsButtonPressed(int controllerIndex, ControllerButton button);
    bool IsButtonHeld(int controllerIndex, ControllerButton button);
    float GetAxisValue(int controllerIndex, AxisType axisType);
    void RunDiagnostics();
}

public class ControllerInfo
{
    public int Index { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsConnected { get; set; }
    public ControllerType ControllerType { get; set; }
    public ushort VendorId { get; set; }
    public ushort ProductId { get; set; }
}

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
