using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.Logging;
using WheelWizard.ControllerSettings;
using WheelWizard.Dolphin;
using WheelWizard.Features.Dolphin;
using WheelWizard.Shared.DependencyInjection;
using WheelWizard.Views.Components;
using WheelWizard.Views.Popups.ControllerManagement;

namespace WheelWizard.Views.Pages;

public partial class ControllerPage : UserControlBase, INotifyPropertyChanged
{
    [Inject]
    private IControllerService ControllerService { get; set; } = null!;

    [Inject]
    private DolphinControllerService DolphinControllerService { get; set; } = null!;

    [Inject]
    private ILogger<ControllerPage> Logger { get; set; } = null!;

    private ObservableCollection<ControllerInfo> _controllers;
    private ObservableCollection<DolphinControllerProfile> _profiles;
    private DispatcherTimer _updateTimer;

    public ObservableCollection<ControllerInfo> Controllers
    {
        get => _controllers;
        set => SetField(ref _controllers, value);
    }

    public ObservableCollection<DolphinControllerProfile> Profiles
    {
        get => _profiles;
        set => SetField(ref _profiles, value);
    }

    public ControllerPage()
    {
        InitializeComponent();
        _controllers = new ObservableCollection<ControllerInfo>();
        _profiles = new ObservableCollection<DolphinControllerProfile>();

        _updateTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _updateTimer.Tick += UpdateTimer_Tick;

        DataContext = this;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        RefreshControllers();
        RefreshProfiles();
        _updateTimer.Start();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        Logger.LogInformation("ControllerPage unloaded - stopping controller detection");
        _updateTimer.Stop();
    }

    private void UpdateTimer_Tick(object? sender, EventArgs e)
    {
        ControllerService.Update();
        RefreshControllers();
    }

    private void RefreshControllers()
    {
        try
        {
            var connectedControllers = ControllerService.GetConnectedControllers();
            if (connectedControllers.Count == 0)
                return;

            Controllers.Clear();
            foreach (var controller in connectedControllers)
            {
                Controllers.Add(controller);
            }
            ControllersList.ItemsSource = connectedControllers;

            // Update controller count text
            var count = Controllers.Count;
            ControllerCountText.Text = $"{count} connected";

            // Update UI visibility
            NoControllersInfo.IsVisible = Controllers.Count == 0;
            ControllersList.IsVisible = Controllers.Count > 0;

            if (Controllers.Count == 0)
            {
                Logger.LogWarning("No controllers detected in UI - this may indicate a detection issue");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error refreshing controllers in UI");
        }
    }

    private void RefreshProfiles()
    {
        try
        {
            var profiles = DolphinControllerService.GetProfiles();

            //todo: can probably do this better
            Profiles.Clear();
            foreach (var profile in profiles)
            {
                Profiles.Add(profile);
            }

            ProfilesList.ItemsSource = profiles;

            NoProfilesInfo.IsVisible = (Profiles.Count == 0);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error refreshing controller profiles in UI");
        }
    }

    private async void CreateProfileButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (Controllers.Count == 0)
            {
                await ShowMessageDialog("No Controllers", "Please connect a controller before creating a profile.");
                return;
            }

            await ShowCreateProfileDialog();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating controller profile");
            await ShowMessageDialog("Error", "Failed to create controller profile. Please try again.");
        }
    }

    private async void ImportProfileButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            await ShowImportProfileDialog();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error importing controller profile");
            await ShowMessageDialog("Error", "Failed to import controller profile. Please try again.");
        }
    }

    private async void ActivateProfileButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not Avalonia.Controls.Button button || button.CommandParameter is not string profileName)
                return;

            var success = DolphinControllerService.SetActiveProfile(profileName);
            if (success)
            {
                RefreshProfiles();
            }
            else
            {
                await ShowMessageDialog("Error", $"Failed to activate profile '{profileName}'. Please try again.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error activating controller profile");
            await ShowMessageDialog("Error", "Failed to activate controller profile. Please try again.");
        }
    }

    private async void EditProfileButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not Avalonia.Controls.Button button || button.CommandParameter is not string profileName)
                return;

            await ShowEditProfileDialog(profileName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error editing controller profile");
            await ShowMessageDialog("Error", "Failed to edit controller profile. Please try again.");
        }
    }

    private async void DeleteProfileButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not Avalonia.Controls.Button button || button.CommandParameter is not string profileName)
                return;

            var result = await ShowConfirmDialog(
                "Delete Profile",
                $"Are you sure you want to delete the controller profile '{profileName}'? This action cannot be undone."
            );

            if (!result)
                return;
            var success = DolphinControllerService.DeleteProfile(profileName);

            if (success)
            {
                RefreshProfiles();
            }
            else
            {
                await ShowMessageDialog("Error", $"Failed to delete profile '{profileName}'. Please try again.");
            }
        }
        catch (Exception ex)
        {
            await ShowMessageDialog("Error", "Failed to delete controller profile. Please try again.");
        }
    }

    private async Task ShowCreateProfileDialog()
    {
        var controller = Controllers.First();
        var profileName = $"{controller.ControllerType} Profile {DateTime.Now:HHmm}";

        var mapping = DolphinControllerService.GetMappingForControllerType(controller.ControllerType);
        var success = DolphinControllerService.CreateProfile(profileName, controller, mapping);

        if (success)
        {
            RefreshProfiles();
            await ShowMessageDialog("Success", $"Controller profile '{profileName}' has been created.");
        }
        else
        {
            await ShowMessageDialog("Error", "Failed to create controller profile. Please try again.");
        }
    }

    private async Task ShowImportProfileDialog()
    {
        // TODO: Implement file picker for importing Dolphin controller profiles
        await ShowMessageDialog("Import Profile", "Profile import functionality will be available in a future update.");
    }

    private async Task ShowEditProfileDialog(string profileName)
    {
        try
        {
            Logger.LogInformation("Opening profile editor for: {ProfileName}", profileName);

            var profileEditWindow = new ProfileEditWindow();
            var result = await profileEditWindow.ShowDialog(profileName);

            if (result)
            {
                // Refresh the profiles list to show any changes
                RefreshProfiles();
                Logger.LogInformation("Profile '{ProfileName}' was successfully edited", profileName);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error opening profile editor for '{ProfileName}'", profileName);
            await ShowMessageDialog("Error", "Failed to open profile editor. Please try again.");
        }
    }

    private async Task CreateQuickProfile(string baseName, ControllerType controllerType, string? mappingKey = null)
    {
        var controller = Controllers.FirstOrDefault(c => c.ControllerType == controllerType) ?? Controllers.First();
        var profileName = $"{baseName} - {DateTime.Now:MMdd HHmm}";

        var mapping =
            mappingKey != null
                ? DolphinControllerService.GetMappingForControllerType(ControllerType.Generic) // Use generic for special mappings
                : DolphinControllerService.GetMappingForControllerType(controller.ControllerType);

        var success = DolphinControllerService.CreateProfile(profileName, controller, mapping);

        if (success)
        {
            // Automatically activate the new profile
            DolphinControllerService.SetActiveProfile(profileName);
            RefreshProfiles();
            await ShowMessageDialog("Success", $"Controller profile '{profileName}' has been created and activated for Mario Kart Wii.");
        }
        else
        {
            await ShowMessageDialog("Error", "Failed to create controller profile. Please try again.");
        }
    }

    private async Task ShowMessageDialog(string title, string message)
    {
        // TODO: Implement proper dialog system
        Logger.LogInformation("Dialog - {Title}: {Message}", title, message);

        // For now, we'll just log the message
        // In a real implementation, you'd show a proper dialog
    }

    private async Task<bool> ShowConfirmDialog(string title, string message)
    {
        // TODO: Implement proper confirmation dialog
        Logger.LogInformation("Confirm Dialog - {Title}: {Message}", title, message);

        // For now, return true (confirm)
        // In a real implementation, you'd show a proper confirmation dialog
        return true;
    }

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
        return true;
    }
}

// Helper class to hold all the button elements for a controller
