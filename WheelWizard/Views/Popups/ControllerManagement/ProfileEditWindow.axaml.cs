using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.Logging;
using WheelWizard.ControllerSettings;
using WheelWizard.Dolphin;
using WheelWizard.Features.Dolphin;
using WheelWizard.Shared.DependencyInjection;
using WheelWizard.Shared.MessageTranslations;
using WheelWizard.Views.Popups.Base;
using WheelWizard.Views.Popups.Generic;

namespace WheelWizard.Views.Popups.ControllerManagement;

public partial class ProfileEditWindow : PopupContent, INotifyPropertyChanged
{
    [Inject]
    private DolphinControllerService DolphinControllerService { get; set; } = null!;

    [Inject]
    private ILogger<ProfileEditWindow> Logger { get; set; } = null!;

    private DolphinControllerProfile _originalProfile;
    private DolphinControllerProfile _editingProfile;
    private bool _hasChanges = false;
    private bool _isInitializing = true;

    // Properties for data binding
    private string _profileName = string.Empty;
    public string ProfileName
    {
        get => _profileName;
        set
        {
            if (SetField(ref _profileName, value) && !_isInitializing)
            {
                UpdateChangeStatus();
            }
        }
    }

    private ControllerType _controllerType;
    public ControllerType ControllerType
    {
        get => _controllerType;
        set => SetField(ref _controllerType, value);
    }

    private DateTime _createdAt;
    public DateTime CreatedAt
    {
        get => _createdAt;
        set => SetField(ref _createdAt, value);
    }

    private bool _isActive;
    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (SetField(ref _isActive, value) && !_isInitializing)
            {
                UpdateChangeStatus();
            }
        }
    }

    public bool HasChanges
    {
        get => _hasChanges;
        set => SetField(ref _hasChanges, value);
    }

    // Display properties
    public string ControllerTypeDisplay => ControllerType.ToString();
    public string CreatedAtDisplay => CreatedAt.ToString("MMM dd, yyyy 'at' HH:mm");

    public ProfileEditWindow()
        : base(true, false, true, "Profile Editor")
    {
        InitializeComponent();
        DataContext = this;
    }

    public async Task<bool> ShowDialog(string profileName)
    {
        try
        {
            Logger.LogInformation("Opening profile editor for: {ProfileName}", profileName);

            // Load the original profile
            var profile = DolphinControllerService.GetProfile(profileName);
            if (profile == null)
            {
                Logger.LogWarning("Profile '{ProfileName}' not found", profileName);
                await ShowErrorMessage("Profile not found", $"The profile '{profileName}' could not be found.");
                return false;
            }

            // Create a copy for editing
            _originalProfile = profile;
            _editingProfile = new DolphinControllerProfile
            {
                Name = profile.Name,
                ControllerType = profile.ControllerType,
                Mapping = new DolphinControllerMapping
                {
                    Name = profile.Mapping.Name,
                    Description = profile.Mapping.Description,
                    ButtonMappings = new Dictionary<string, string>(profile.Mapping.ButtonMappings),
                },
                CreatedAt = profile.CreatedAt,
                IsActive = profile.IsActive,
            };

            // Initialize the UI
            _isInitializing = true;
            ProfileName = _editingProfile.Name;
            ControllerType = _editingProfile.ControllerType;
            CreatedAt = _editingProfile.CreatedAt;
            IsActive = _editingProfile.IsActive;
            _isInitializing = false;

            // Load mappings into the editor
            MappingEditor.LoadMappings(_editingProfile.Mapping.ButtonMappings);

            // Update window title
            Window.WindowTitle = $"Profile Editor - {ProfileName}";

            // Show the dialog
            var result = await ShowDialog<bool>();
            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error opening profile editor for '{ProfileName}'", profileName);
            await ShowErrorMessage("Error", "Failed to open profile editor. Please try again.");
            return false;
        }
    }

    private void ProfileNameTextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (!_isInitializing)
        {
            UpdateChangeStatus();
        }
    }

    private void ActiveToggle_Changed(object? sender, RoutedEventArgs e)
    {
        if (!_isInitializing)
        {
            UpdateChangeStatus();
        }
    }

    private void UpdateChangeStatus()
    {
        var nameChanged = ProfileName != _originalProfile.Name;
        var activeChanged = IsActive != _originalProfile.IsActive;
        var mappingsChanged = MappingEditor.HasChanges();

        HasChanges = nameChanged || activeChanged || mappingsChanged;

        // Update status message
        if (HasChanges)
        {
            var changes = new List<string>();
            if (nameChanged)
                changes.Add("name");
            if (activeChanged)
                changes.Add("active status");
            if (mappingsChanged)
                changes.Add("button mappings");

            StatusMessage.Text = $"Changes detected: {string.Join(", ", changes)}";
            StatusMessage.IsVisible = true;
        }
        else
        {
            StatusMessage.IsVisible = false;
        }
    }

    private async void ResetButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            Logger.LogInformation("Resetting mappings for profile: {ProfileName}", ProfileName);

            var result = await new YesNoWindow()
                .SetMainText("Reset Button Mappings")
                .SetExtraText("Are you sure you want to reset all button mappings to their default values? This action cannot be undone.")
                .SetButtonText("Cancel", "Reset")
                .ShowDialog<bool>();

            if (result)
            {
                MappingEditor.ResetToDefaults();
                UpdateChangeStatus();
                Logger.LogInformation("Mappings reset for profile: {ProfileName}", ProfileName);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error resetting mappings for profile '{ProfileName}'", ProfileName);
            await ShowErrorMessage("Error", "Failed to reset mappings. Please try again.");
        }
    }

    private async void SaveButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            Logger.LogInformation("Saving changes for profile: {ProfileName}", ProfileName);

            // Validate profile name
            if (string.IsNullOrWhiteSpace(ProfileName))
            {
                await ShowErrorMessage("Invalid Name", "Profile name cannot be empty.");
                return;
            }

            // Check for name conflicts (if name changed)
            if (ProfileName != _originalProfile.Name)
            {
                var existingProfile = DolphinControllerService.GetProfile(ProfileName);
                if (existingProfile != null)
                {
                    await ShowErrorMessage(
                        "Name Conflict",
                        $"A profile named '{ProfileName}' already exists. Please choose a different name."
                    );
                    return;
                }
            }

            // Get updated mappings
            var updatedMappings = MappingEditor.GetMappings();

            // Update the editing profile
            _editingProfile.Name = ProfileName;
            _editingProfile.IsActive = IsActive;
            _editingProfile.Mapping.ButtonMappings = updatedMappings;

            // Save to Dolphin
            var success = DolphinControllerService.UpdateProfile(_originalProfile.Name, _editingProfile);
            if (!success)
            {
                await ShowErrorMessage("Save Failed", "Failed to save profile changes. Please try again.");
                return;
            }

            Logger.LogInformation("Successfully saved profile: {ProfileName}", ProfileName);

            // Close dialog with success
            Close();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving profile '{ProfileName}'", ProfileName);
            await ShowErrorMessage("Error", "Failed to save profile changes. Please try again.");
        }
    }

    private async Task ShowErrorMessage(string title, string message)
    {
        await new MessageBoxWindow()
            .SetTitleText(title)
            .SetInfoText(message)
            .SetMessageType(MessageBoxWindow.MessageType.Error)
            .ShowDialog();
    }

    protected override void BeforeClose()
    {
        // If there are unsaved changes, ask for confirmation
        if (HasChanges)
        {
            // Note: In a real implementation, you might want to show a confirmation dialog here
            // For now, we'll just log the fact that changes were discarded
            Logger.LogInformation("Profile editor closed with unsaved changes for: {ProfileName}", ProfileName);
        }
    }

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
        return true;
    }

    #endregion
}
