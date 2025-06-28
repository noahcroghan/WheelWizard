using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Testably.Abstractions;
using WheelWizard.Models.Enums;
using WheelWizard.Resources.Languages;
using WheelWizard.Services.Launcher;
using WheelWizard.Services.Launcher.Helpers;
using WheelWizard.Services.Settings;
using WheelWizard.Views.Components;
using WheelWizard.Views.Pages.Settings;
using Button = WheelWizard.Views.Components.Button;

namespace WheelWizard.Views.Pages;

public partial class HomePage : UserControlBase
{
    private static ILauncher currentLauncher => _launcherTypes[_launcherIndex];
    private static int _launcherIndex = 0; // Make sure this index never goes over the list index
    private WheelTrail[] _trails = [];

    private static List<ILauncher> _launcherTypes =
    [
        new RrLauncher(),
        //GoogleLauncher.Instance
    ];

    private WheelWizardStatus _status;
    private MainButtonState currentButtonState => buttonStates[_status];

    private static Dictionary<WheelWizardStatus, MainButtonState> buttonStates = new()
    {
        { WheelWizardStatus.Loading, new(Common.State_Loading, Button.ButtonsVariantType.Default, "Spinner", null, false) },
        { WheelWizardStatus.NoServer, new(Common.State_NoServer, Button.ButtonsVariantType.Danger, "RoadError", null, true) },
        {
            WheelWizardStatus.NoServerButInstalled,
            new(Common.Action_PlayOffline, Button.ButtonsVariantType.Warning, "Play", LaunchGame, true)
        },
        { WheelWizardStatus.NoDolphin, new("Dolphin not setup", Button.ButtonsVariantType.Warning, "Settings", NavigateToSettings, false) },
        {
            WheelWizardStatus.ConfigNotFinished,
            new(Common.State_ConfigNotFinished, Button.ButtonsVariantType.Warning, "Settings", NavigateToSettings, true)
        },
        { WheelWizardStatus.NotInstalled, new(Common.Action_Install, Button.ButtonsVariantType.Warning, "Download", Download, true) },
        { WheelWizardStatus.OutOfDate, new(Common.Action_Update, Button.ButtonsVariantType.Warning, "Download", Update, true) },
        { WheelWizardStatus.Ready, new(Common.Action_Play, Button.ButtonsVariantType.Primary, "Play", LaunchGame, true) },
    };

    public HomePage()
    {
        InitializeComponent();
        PopulateGameModeDropdown();
        UpdatePage();

        _trails = [HomeTrail1, HomeTrail2, HomeTrail3, HomeTrail4, HomeTrail5];
        App.Services.GetService<IRandomSystem>()?.Random.Shared.Shuffle(_trails);
        // We have to do it like `App.Service.GetService`. We cant make use of `private IRandomSystem Random { get; set; } = null!;` here
        // This is because this HomePage is always loaded first
    }

    private void UpdatePage()
    {
        GameTitle.Text = currentLauncher.GameTitle;
        UpdateActionButton();
    }

    private void DolphinButton_OnClick(object? sender, RoutedEventArgs e)
    {
        DolphinLaunchHelper.LaunchDolphin();
        DisableAllButtonsTemporarily();
    }

    private static void LaunchGame() => currentLauncher.Launch();

    private static void NavigateToSettings() => NavigationManager.NavigateTo<SettingsPage>();

    private static async void Download()
    {
        ViewUtils.GetLayout().SetInteractable(false);
        await currentLauncher.Install();
        ViewUtils.GetLayout().SetInteractable(true);
        NavigationManager.NavigateTo<HomePage>();
    }

    private static async void Update()
    {
        ViewUtils.GetLayout().SetInteractable(false);
        await currentLauncher.Update();
        ViewUtils.GetLayout().SetInteractable(true);
        NavigationManager.NavigateTo<HomePage>();
    }

    private void PlayButton_Click(object? sender, RoutedEventArgs e)
    {
        currentButtonState?.OnClick?.Invoke();

        UpdateActionButton();
        DisableAllButtonsTemporarily();
    }

    private void GameModeDropdown_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _launcherIndex = GameModeDropdown.SelectedIndex;
        UpdatePage();
    }

    private void PopulateGameModeDropdown()
    {
        // If there is only 1 option, we don't want to confuse the player with that option
        GameModeOption.IsVisible = _launcherTypes.Count > 1;
        if (!GameModeOption.IsVisible)
            return;

        foreach (var launcherType in _launcherTypes)
        {
            GameModeDropdown.Items.Add(launcherType.GameTitle);
        }

        GameModeDropdown.SelectedIndex = _launcherIndex;
    }

    private async void UpdateActionButton()
    {
        _status = WheelWizardStatus.Loading;
        SetButtonState(currentButtonState);
        _status = await currentLauncher.GetCurrentStatus();
        SetButtonState(currentButtonState);
    }

    private void DisableAllButtonsTemporarily()
    {
        CompleteGrid.IsEnabled = false;
        //wait 5 seconds before re-enabling the buttons
        Task.Delay(2000)
            .ContinueWith(_ =>
            {
                Dispatcher.UIThread.InvokeAsync(() => CompleteGrid.IsEnabled = true);
            });
    }

    private void SetButtonState(MainButtonState state)
    {
        PlayButton.Text = state.Text;
        PlayButton.Variant = state.Type;
        PlayButton.IsEnabled = state.OnClick != null;
        if (Application.Current != null && Application.Current.FindResource(state.IconName) is Geometry geometry)
            PlayButton.IconData = geometry;
        DolphinButton.IsEnabled = state.SubButtonsEnabled && SettingsHelper.PathsSetupCorrectly();

        UpdateWheelTrails();
    }

    private async void UpdateWheelTrails()
    {
        if (_status == WheelWizardStatus.Ready && (bool)SettingsManager.ENABLE_ANIMATIONS.Get())
        {
            foreach (var t in _trails)
            {
                t.Classes.Add("EntranceTrail");
                await Task.Delay(80);
            }

            await Task.Delay(500);
            foreach (var t in _trails)
            {
                t.Classes.Remove("EntranceTrail");
                t.Classes.Add("StaticTrail");
                await Task.Delay(80);
            }
        }
    }

    private void HomeTrail_OnGotFocus(object? sender, GotFocusEventArgs e)
    {
        if (sender is not WheelTrail trail)
            return;

        if (trail.Classes.Contains("ExcitedTrail"))
            return;
        trail.Classes.Add("ExcitedTrail");
    }

    private void HomeTrail_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (sender is not WheelTrail trail)
            return;

        trail.Classes.Remove("ExcitedTrail");
    }

    public class MainButtonState
    {
        public MainButtonState(string text, Button.ButtonsVariantType type, string iconName, Action? onClick, bool subButtonsEnables) =>
            (Text, Type, IconName, OnClick, SubButtonsEnabled) = (text, type, iconName, onClick, subButtonsEnables);

        public string Text { get; set; }
        public Button.ButtonsVariantType Type { get; set; }
        public string IconName { get; set; }
        public Action? OnClick { get; set; }
        public bool SubButtonsEnabled { get; set; }
    }

    private async void PlayButton_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        foreach (var t in _trails)
        {
            if (!t.Classes.Contains("ExcitedTrail"))
                t.Classes.Add("ExcitedTrail");
            await Task.Delay(180);
        }
    }

    private void PlayButton_OnPointerExit(object? sender, PointerEventArgs e)
    {
        foreach (var t in _trails)
        {
            t.Classes.Remove("ExcitedTrail");
        }
    }
}
