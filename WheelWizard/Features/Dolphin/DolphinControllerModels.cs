using WheelWizard.ControllerSettings;

namespace WheelWizard.Dolphin;

public class DolphinControllerProfile
{
    public string Name { get; set; } = string.Empty;
    public ControllerType ControllerType { get; set; }
    public DolphinControllerMapping Mapping { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public string Description => $"{ControllerType} controller profile for {Name}";
}

public class DolphinControllerMapping
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, string> ButtonMappings { get; set; } = new();

    public string GetMappingForButton(string dolphinButton)
    {
        return ButtonMappings.TryGetValue(dolphinButton, out var mapping) ? mapping : "";
    }

    public void SetMappingForButton(string dolphinButton, string inputMapping)
    {
        ButtonMappings[dolphinButton] = inputMapping;
    }
}

public class ControllerMappingPreset
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ControllerType TargetControllerType { get; set; }
    public DolphinControllerMapping Mapping { get; set; } = new();
    public bool IsBuiltIn { get; set; }
}

public enum DolphinControllerType
{
    None = 0,
    StandardController = 6,
    Keyboard = 7,
    SteeringWheel = 8,
    DanceMat = 9,
    DKBongos = 10,
    GBAIntegrated = 13,
}

public enum WiimoteSource
{
    None = 0,
    Emulated = 1,
    Real = 2,
}
