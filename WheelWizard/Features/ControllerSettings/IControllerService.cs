namespace WheelWizard.ControllerSettings;

using System;
using System.Collections.Generic;

/// <summary>
/// A service for managing and retrieving input from game controllers.
/// Provides event-driven notifications for controller connections/disconnections.
/// </summary>
public interface IControllerService : IDisposable
{
    /// <summary>
    /// Fired when a new controller is connected and detected by the service.
    /// </summary>
    event Action<ControllerInfo> OnControllerConnected;

    /// <summary>
    /// Fired when a previously connected controller is disconnected.
    /// </summary>
    event Action<ControllerInfo> OnControllerDisconnected;

    void Update();

    /// <summary>
    /// Gets a list of all currently connected controllers.
    /// </summary>
    /// <returns>A list of ControllerInfo objects for connected controllers.</returns>
    IReadOnlyList<ControllerInfo> GetConnectedControllers();

    /// <summary>
    /// Checks if a specific button was just pressed in the latest Update() call.
    /// </summary>
    /// <param name="controllerIndex">The index of the controller.</param>
    /// <param name="button">The button to check.</param>
    /// <returns>True if the button was pressed in the last frame, false otherwise.</returns>
    bool IsButtonPressed(int controllerIndex, ControllerButton button);

    /// <summary>
    /// Checks if a specific button is currently being held down.
    /// </summary>
    /// <param name="controllerIndex">The index of the controller.</param>
    /// <param name="button">The button to check.</param>
    /// <returns>True if the button is held down, false otherwise.</returns>
    bool IsButtonHeld(int controllerIndex, ControllerButton button);

    /// <summary>
    /// Gets the current value of a controller's analog axis.
    /// </summary>
    /// <param name="controllerIndex">The index of the controller.</param>
    /// <param name="axis">The axis to read.</param>
    /// <returns>A float value, typically between -1.0 and 1.0 for thumbsticks, and 0.0 to 1.0 for triggers.</returns>
    float GetAxisValue(int controllerIndex, AxisType axis);

    /// <summary>
    /// Runs comprehensive diagnostics on the controller detection system.
    /// Logs detailed information for debugging purposes.
    /// </summary>
    void RunDiagnostics();
}
