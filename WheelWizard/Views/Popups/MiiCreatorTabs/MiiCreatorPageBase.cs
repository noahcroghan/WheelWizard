using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using WheelWizard.WiiManagement.Domain.Mii;

namespace WheelWizard.Views.Popups.MiiCreatorTabs;

// Base class implementing INotifyPropertyChanged and holding the Mii reference
public abstract class MiiCreatorPageBase : UserControl, INotifyPropertyChanged
{
    private Mii _miiToEdit = null!; // Initialize with null! - will be set by SetMiiToEdit

    // Property to access the Mii being edited by derived pages
    public Mii MiiToEdit => _miiToEdit;

    // Method for the Window to pass the Mii clone
    public virtual void SetMiiToEdit(Mii mii)
    {
        _miiToEdit = mii ?? throw new ArgumentNullException(nameof(mii));
        // Optionally trigger property changes for bindings that depend directly on MiiToEdit
        OnPropertyChanged(nameof(MiiToEdit));
        // Or trigger specific property changes if needed after Mii is set
        LoadDataFromMii();
    }

    /// <summary>
    /// Called after SetMiiToEdit. Override this in derived classes
    /// to populate page-specific properties or controls from the Mii object.
    /// </summary>
    protected virtual void LoadDataFromMii() { }

    #region INotifyPropertyChanged Implementation
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
        field = value;
        OnPropertyChanged(propertyName);
        // Automatically notify dependent IsPageValid property if it exists
        if (GetType().GetProperty(nameof(IValidatableMiiPage.IsPageValid)) != null)
        {
            OnPropertyChanged(nameof(IValidatableMiiPage.IsPageValid));
        }
        if (GetType().GetProperty("IsValid") != null) // Also check for IsValid if used
        {
            OnPropertyChanged("IsValid");
        }
        return true;
    }
    #endregion
}
