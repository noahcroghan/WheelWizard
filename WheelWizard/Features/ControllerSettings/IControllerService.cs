namespace WheelWizard.ControllerSettings;

using System;
using System.Collections.Generic;

public interface IControllerService : IDisposable
{
    void Update();
    IReadOnlyList<ControllerInfo> GetConnectedControllers();
    
    bool IsButtonPressed(int controllerIndex, ControllerButton button);
    
    bool IsButtonHeld(int controllerIndex, ControllerButton button);

    float GetAxisValue(int controllerIndex, AxisType axis);

}
