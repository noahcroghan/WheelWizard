using Avalonia;

namespace WheelWizard;

/// <summary>
/// This interface is used to define the entry point for the Avalonia designer.
/// </summary>
public interface IDesignerEntryPoint
{
    /// <summary>
    /// This method is used by the Avalonia designer to create the application instance.
    /// </summary>
    /// <param name="isDesigner">True if the designer is running, false otherwise.</param>
    /// <remarks>
    /// All the parameters should be optional as the designer may not provide them.
    /// </remarks>
    static abstract AppBuilder BuildAvaloniaApp(bool isDesigner = true);
}
