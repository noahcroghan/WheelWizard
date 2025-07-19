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
using WheelWizard.Features.Dolphin;
using WheelWizard.Shared.DependencyInjection;
using WheelWizard.Views.Components;

namespace WheelWizard.Views.Pages;

public partial class ControllerPage : UserControlBase, INotifyPropertyChanged
{
    [Inject]
    private ControllerService ControllerService { get; set; } = null!;

    [Inject]
    private DolphinControllerService DolphinControllerService { get; set; } = null!;

    [Inject]
    private ILogger<ControllerPage> Logger { get; set; } = null!;

    private ObservableCollection<ControllerInfo> _controllers;
    private ObservableCollection<DolphinControllerProfile> _profiles;
    private DispatcherTimer _updateTimer;
    private Dictionary<int, Border> _controllerButtonElements;

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
        _controllerButtonElements = new Dictionary<int, Border>();

        // Set up timer to periodically refresh controller detection
        _updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16), // Update at ~60fps for responsive button feedback
        };
        _updateTimer.Tick += UpdateTimer_Tick;

        DataContext = this;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Logger.LogInformation("ControllerPage loaded - starting controller detection");
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

    private void RefreshButton_Click(object? sender, RoutedEventArgs e)
    {
        Logger.LogInformation("Manual refresh button clicked");
        ControllerService.RefreshControllers();
        RefreshControllers();
    }

    private void DiagnosticButton_Click(object? sender, RoutedEventArgs e)
    {
        Logger.LogInformation("Diagnostic button clicked - running comprehensive controller diagnostics");
        ControllerService.RunDiagnostics();

        // Also refresh the UI to show current state
        RefreshControllers();

        Logger.LogInformation("Diagnostics completed - check the log file for detailed information");
    }

    private void UpdateTimer_Tick(object? sender, EventArgs e)
    {
        ControllerService.Update();
        UpdateControllerButtonStates();

        // Periodically refresh controllers (every 2 seconds) to detect new ones
        if (DateTime.Now.Ticks % 125 == 0) // ~2 seconds at 60fps
        {
            RefreshControllers();
        }
    }

    private void UpdateControllerButtonStates()
    {
        try
        {
            foreach (var controller in Controllers)
            {
                UpdateControllerButtonState(controller.Index);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating controller button states");
        }
    }

    private void UpdateControllerButtonState(int controllerIndex)
    {
        try
        {
            // Find the controller item container by searching through the visual tree
            var itemContainer = FindControllerItemContainer(controllerIndex);
            if (itemContainer == null)
            {
                Logger.LogDebug("Could not find controller item container for index {Index}", controllerIndex);
                return;
            }

            // Find the button elements within this controller's template
            var buttonElements = FindButtonElements(itemContainer);
            if (buttonElements == null)
            {
                Logger.LogDebug("Could not find button elements for controller index {Index}", controllerIndex);
                return;
            }

            // Check button states and log if any are pressed
            var aPressed = ControllerService.IsButtonHeld(controllerIndex, ControllerButton.A);
            var bPressed = ControllerService.IsButtonHeld(controllerIndex, ControllerButton.B);
            var xPressed = ControllerService.IsButtonHeld(controllerIndex, ControllerButton.X);
            var yPressed = ControllerService.IsButtonHeld(controllerIndex, ControllerButton.Y);

            if (aPressed || bPressed || xPressed || yPressed)
            {
                Logger.LogDebug(
                    "Controller {Index} face buttons - A:{A} B:{B} X:{X} Y:{Y}",
                    controllerIndex,
                    aPressed,
                    bPressed,
                    xPressed,
                    yPressed
                );
            }

            // Update button states
            UpdateButtonVisualState(buttonElements.AButton, aPressed);
            UpdateButtonVisualState(buttonElements.BButton, bPressed);
            UpdateButtonVisualState(buttonElements.XButton, xPressed);
            UpdateButtonVisualState(buttonElements.YButton, yPressed);

            UpdateButtonVisualState(buttonElements.StartButton, ControllerService.IsButtonHeld(controllerIndex, ControllerButton.Start));
            UpdateButtonVisualState(buttonElements.BackButton, ControllerService.IsButtonHeld(controllerIndex, ControllerButton.Back));

            UpdateButtonVisualState(
                buttonElements.LeftShoulder,
                ControllerService.IsButtonHeld(controllerIndex, ControllerButton.LeftShoulder)
            );
            UpdateButtonVisualState(
                buttonElements.RightShoulder,
                ControllerService.IsButtonHeld(controllerIndex, ControllerButton.RightShoulder)
            );

            UpdateButtonVisualState(buttonElements.DPadUp, ControllerService.IsButtonHeld(controllerIndex, ControllerButton.DPadUp));
            UpdateButtonVisualState(buttonElements.DPadDown, ControllerService.IsButtonHeld(controllerIndex, ControllerButton.DPadDown));
            UpdateButtonVisualState(buttonElements.DPadLeft, ControllerService.IsButtonHeld(controllerIndex, ControllerButton.DPadLeft));
            UpdateButtonVisualState(buttonElements.DPadRight, ControllerService.IsButtonHeld(controllerIndex, ControllerButton.DPadRight));

            // Update analog stick positions
            UpdateStickPosition(
                buttonElements.LeftStickIndicator,
                buttonElements.LeftStickText,
                ControllerService.GetAxisValue(controllerIndex, AxisType.LeftThumbstickX),
                ControllerService.GetAxisValue(controllerIndex, AxisType.LeftThumbstickY)
            );

            UpdateStickPosition(
                buttonElements.RightStickIndicator,
                buttonElements.RightStickText,
                ControllerService.GetAxisValue(controllerIndex, AxisType.RightThumbstickX),
                ControllerService.GetAxisValue(controllerIndex, AxisType.RightThumbstickY)
            );

            // Update trigger bars
            UpdateTriggerBar(
                buttonElements.LeftTriggerBar,
                buttonElements.LeftTriggerText,
                ControllerService.GetAxisValue(controllerIndex, AxisType.LeftTrigger)
            );

            UpdateTriggerBar(
                buttonElements.RightTriggerBar,
                buttonElements.RightTriggerText,
                ControllerService.GetAxisValue(controllerIndex, AxisType.RightTrigger)
            );
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating controller {Index} button state", controllerIndex);
        }
    }

    private void UpdateButtonVisualState(Border? button, bool isPressed)
    {
        if (button == null)
            return;

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            try
            {
                // Get the brush resources
                var neutralBrush = this.FindResource("Neutral600") as SolidColorBrush;
                var primaryBrush = this.FindResource("Primary400") as SolidColorBrush;

                if (neutralBrush != null && primaryBrush != null)
                {
                    button.Background = isPressed ? primaryBrush : neutralBrush;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating button visual state");
            }
        });
    }

    private void UpdateStickPosition(Border? stickIndicator, TextBlock? stickText, float x, float y)
    {
        if (stickIndicator == null || stickText == null)
            return;

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            try
            {
                // Update the stick indicator position using margins
                // The container is 60x60, so max offset from center is 20 (30 - 10 for the 20x20 indicator)
                var maxOffset = 20.0;
                var offsetX = x * maxOffset;
                var offsetY = -y * maxOffset; // Invert Y for screen coordinates

                // Use margins to position the stick indicator relative to center
                stickIndicator.Margin = new Thickness(offsetX, offsetY, 0, 0);

                // Update the text
                stickText.Text = $"({x:F2}, {y:F2})";
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating stick position");
            }
        });
    }

    private void UpdateTriggerBar(Border? triggerBar, TextBlock? triggerText, float value)
    {
        if (triggerBar == null || triggerText == null)
            return;

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            try
            {
                // Update the trigger bar width (assuming 30px max width)
                var maxWidth = 30.0;
                var width = value * maxWidth;
                triggerBar.Width = Math.Max(0, Math.Min(maxWidth, width));

                // Update the text
                triggerText.Text = $"{value:F2}";
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating trigger bar");
            }
        });
    }

    private Control? FindControllerItemContainer(int controllerIndex)
    {
        try
        {
            // Search through the visual tree to find the controller item container
            var controller = Controllers.FirstOrDefault(c => c.Index == controllerIndex);
            if (controller == null)
            {
                Logger.LogDebug("Controller with index {Index} not found in Controllers collection", controllerIndex);
                return null;
            }

            // Get all items in the ControllerTestList
            var items = ControllerTestList.GetRealizedContainers();
            foreach (var item in items)
            {
                if (item.DataContext == controller)
                {
                    return item;
                }
            }

            Logger.LogDebug(
                "Could not find controller container for index {Index} - ItemsControl may not have realized items yet",
                controllerIndex
            );
            return null;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error finding controller item container for index {Index}", controllerIndex);
            return null;
        }
    }

    private ControllerButtonElements? FindButtonElements(Control itemContainer)
    {
        try
        {
            // Find the button elements by name within the item container using recursive search
            var aButton = FindControlRecursive<Border>(itemContainer, "AButton");
            var bButton = FindControlRecursive<Border>(itemContainer, "BButton");
            var xButton = FindControlRecursive<Border>(itemContainer, "XButton");
            var yButton = FindControlRecursive<Border>(itemContainer, "YButton");

            var startButton = FindControlRecursive<Border>(itemContainer, "StartButton");
            var backButton = FindControlRecursive<Border>(itemContainer, "BackButton");

            var leftShoulder = FindControlRecursive<Border>(itemContainer, "LeftShoulder");
            var rightShoulder = FindControlRecursive<Border>(itemContainer, "RightShoulder");

            var dPadUp = FindControlRecursive<Border>(itemContainer, "DPadUp");
            var dPadDown = FindControlRecursive<Border>(itemContainer, "DPadDown");
            var dPadLeft = FindControlRecursive<Border>(itemContainer, "DPadLeft");
            var dPadRight = FindControlRecursive<Border>(itemContainer, "DPadRight");

            var leftStickIndicator = FindControlRecursive<Border>(itemContainer, "LeftStickIndicator");
            var rightStickIndicator = FindControlRecursive<Border>(itemContainer, "RightStickIndicator");
            var leftStickText = FindControlRecursive<TextBlock>(itemContainer, "LeftStickText");
            var rightStickText = FindControlRecursive<TextBlock>(itemContainer, "RightStickText");

            var leftTriggerBar = FindControlRecursive<Border>(itemContainer, "LeftTriggerBar");
            var rightTriggerBar = FindControlRecursive<Border>(itemContainer, "RightTriggerBar");
            var leftTriggerText = FindControlRecursive<TextBlock>(itemContainer, "LeftTriggerText");
            var rightTriggerText = FindControlRecursive<TextBlock>(itemContainer, "RightTriggerText");
            return new ControllerButtonElements
            {
                AButton = aButton,
                BButton = bButton,
                XButton = xButton,
                YButton = yButton,
                StartButton = startButton,
                BackButton = backButton,
                LeftShoulder = leftShoulder,
                RightShoulder = rightShoulder,
                DPadUp = dPadUp,
                DPadDown = dPadDown,
                DPadLeft = dPadLeft,
                DPadRight = dPadRight,
                LeftStickIndicator = leftStickIndicator,
                RightStickIndicator = rightStickIndicator,
                LeftStickText = leftStickText,
                RightStickText = rightStickText,
                LeftTriggerBar = leftTriggerBar,
                RightTriggerBar = rightTriggerBar,
                LeftTriggerText = leftTriggerText,
                RightTriggerText = rightTriggerText,
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error finding button elements");
            return null;
        }
    }

    private T? FindControlRecursive<T>(Control parent, string name)
        where T : Control
    {
        try
        {
            // Check if the current control matches
            if (parent is T targetControl && parent.Name == name)
            {
                return targetControl;
            }

            // Search through visual children
            if (parent is Visual visual)
            {
                var visualChildren = visual.GetVisualChildren();
                foreach (var child in visualChildren)
                {
                    if (child is Control childControl)
                    {
                        var result = FindControlRecursive<T>(childControl, name);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in recursive control search for {Name}", name);
            return null;
        }
    }

    private void RefreshControllers()
    {
        try
        {
            var connectedControllers = ControllerService.GetConnectedControllers();

            // Update the observable collection
            Controllers.Clear();
            foreach (var controller in connectedControllers)
            {
                Controllers.Add(controller);
            }

            // Set the ItemsControl items source
            ControllersList.ItemsSource = connectedControllers;
            ControllerTestList.ItemsSource = connectedControllers;

            // Update controller count text
            var count = Controllers.Count;
            ControllerCountText.Text = count == 1 ? "1 connected" : $"{count} connected";

            // Update UI visibility
            NoControllersInfo.IsVisible = Controllers.Count == 0;
            ControllersList.IsVisible = Controllers.Count > 0;
            TestingSection.IsVisible = Controllers.Count > 0;

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
            Logger.LogDebug("RefreshProfiles called - found {Count} controller profiles", profiles.Count);

            // Update the observable collection
            Profiles.Clear();
            foreach (var profile in profiles)
            {
                Profiles.Add(profile);
                Logger.LogDebug("Added profile to UI: {Name}", profile.Name);
            }

            // Set the ItemsControl items source
            ProfilesList.ItemsSource = profiles;

            // Update UI visibility
            NoProfilesInfo.IsVisible = Profiles.Count == 0;

            Logger.LogInformation("Refreshed {Count} controller profiles", Profiles.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error refreshing controller profiles in UI");
        }
    }

    // Event handlers for new controller profile functionality
    private void TestControllerButton_Click(object? sender, RoutedEventArgs e)
    {
        Logger.LogInformation("Test controller button clicked - showing testing section");
        TestingSection.IsVisible = true;
    }

    private void HideTestingButton_Click(object? sender, RoutedEventArgs e)
    {
        Logger.LogInformation("Hide testing button clicked - hiding testing section");
        TestingSection.IsVisible = false;
    }

    private async void CreateProfileButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            Logger.LogInformation("Create profile button clicked");

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
            Logger.LogInformation("Import profile button clicked");
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
            if (sender is Avalonia.Controls.Button button && button.CommandParameter is string profileName)
            {
                Logger.LogInformation("Activating profile: {ProfileName}", profileName);

                var success = DolphinControllerService.SetActiveProfile(profileName);
                if (success)
                {
                    RefreshProfiles();
                    await ShowMessageDialog("Success", $"Controller profile '{profileName}' has been activated for Mario Kart Wii.");
                }
                else
                {
                    await ShowMessageDialog("Error", $"Failed to activate profile '{profileName}'. Please try again.");
                }
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
            if (sender is Avalonia.Controls.Button button && button.CommandParameter is string profileName)
            {
                Logger.LogInformation("Editing profile: {ProfileName}", profileName);
                await ShowEditProfileDialog(profileName);
            }
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
            if (sender is Avalonia.Controls.Button button && button.CommandParameter is string profileName)
            {
                Logger.LogInformation("Deleting profile: {ProfileName}", profileName);

                var result = await ShowConfirmDialog(
                    "Delete Profile",
                    $"Are you sure you want to delete the controller profile '{profileName}'? This action cannot be undone."
                );

                if (result)
                {
                    var success = DolphinControllerService.DeleteProfile(profileName);
                    if (success)
                    {
                        RefreshProfiles();
                        await ShowMessageDialog("Success", $"Controller profile '{profileName}' has been deleted.");
                    }
                    else
                    {
                        await ShowMessageDialog("Error", $"Failed to delete profile '{profileName}'. Please try again.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting controller profile");
            await ShowMessageDialog("Error", "Failed to delete controller profile. Please try again.");
        }
    }

    private async void SetupGameCubeProfileButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            Logger.LogInformation("Setup GameCube profile button clicked");

            if (Controllers.Count == 0)
            {
                await ShowMessageDialog("No Controllers", "Please connect a controller before setting up a profile.");
                return;
            }

            await CreateQuickProfile("GameCube Controller", ControllerType.Xbox);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error setting up GameCube profile");
            await ShowMessageDialog("Error", "Failed to setup GameCube profile. Please try again.");
        }
    }

    private async void SetupWiiRemoteProfileButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            Logger.LogInformation("Setup Wii Remote profile button clicked");

            if (Controllers.Count == 0)
            {
                await ShowMessageDialog("No Controllers", "Please connect a controller before setting up a profile.");
                return;
            }

            await CreateQuickProfile("Wii Remote + Nunchuk", ControllerType.Generic, "Wiimote");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error setting up Wii Remote profile");
            await ShowMessageDialog("Error", "Failed to setup Wii Remote profile. Please try again.");
        }
    }

    // Helper methods for dialogs and profile creation
    private async Task ShowCreateProfileDialog()
    {
        // For now, create a simple profile with the first available controller
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
        // TODO: Implement profile editing dialog
        await ShowMessageDialog("Edit Profile", $"Profile editing for '{profileName}' will be available in a future update.");
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
public class ControllerButtonElements
{
    public Border? AButton { get; set; }
    public Border? BButton { get; set; }
    public Border? XButton { get; set; }
    public Border? YButton { get; set; }
    public Border? StartButton { get; set; }
    public Border? BackButton { get; set; }
    public Border? LeftShoulder { get; set; }
    public Border? RightShoulder { get; set; }
    public Border? DPadUp { get; set; }
    public Border? DPadDown { get; set; }
    public Border? DPadLeft { get; set; }
    public Border? DPadRight { get; set; }
    public Border? LeftStickIndicator { get; set; }
    public Border? RightStickIndicator { get; set; }
    public TextBlock? LeftStickText { get; set; }
    public TextBlock? RightStickText { get; set; }
    public Border? LeftTriggerBar { get; set; }
    public Border? RightTriggerBar { get; set; }
    public TextBlock? LeftTriggerText { get; set; }
    public TextBlock? RightTriggerText { get; set; }
}
