using Avalonia.Controls;
using System;
using System.Collections.Generic;

namespace WheelWizard.Views;

public abstract class BaseWindow : Window
{
    protected static List<BaseWindow> WindowLayers = new();
    private int _disableCount = 0;
    
    protected abstract Control InteractionOverlay { get; } // Just the visual part of the interaction
    protected abstract Control InteractionContent { get; } // The content that will be disabled when the overlay is shown
    
    protected bool AllowLayoutInteraction = false;

    public void AddLayer()
    {
    }
    
    public void RemoveLayer()
    {
    }
    
    public void SetInteractable(bool value)
    {
        if (!value)
            _disableCount++;
        else if (_disableCount > 0)
            _disableCount--;
        
        if (_disableCount != 0 && value) return;
        
        InteractionOverlay.IsVisible = !value;
        InteractionContent.IsEnabled = value;
    }
}
