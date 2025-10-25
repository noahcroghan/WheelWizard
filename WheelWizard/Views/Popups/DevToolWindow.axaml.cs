﻿using Avalonia.Interactivity;
using Avalonia.Threading;
using Microsoft.Extensions.Caching.Memory;
using WheelWizard.Helpers;
using WheelWizard.Services.Launcher.Helpers;
using WheelWizard.Services.LiveData;
using WheelWizard.Shared.DependencyInjection;
using WheelWizard.Shared.MessageTranslations;
using WheelWizard.Utilities;
using WheelWizard.Utilities.RepeatedTasks;
using WheelWizard.Views.Components;
using WheelWizard.Views.Popups.Base;
using WheelWizard.Views.Popups.Generic;

namespace WheelWizard.Views.Popups;

public partial class DevToolWindow : PopupContent, IRepeatedTaskListener
{
    [Inject]
    private IMemoryCache Cache { get; set; } = null!;

    public DevToolWindow()
        : base(true, true, true, "Dev Tool")
    {
        InitializeComponent();
        AppStateMonitor.Instance.Subscribe(this);
        LoadSettings();
    }

    protected override void BeforeClose()
    {
        AppStateMonitor.Instance.Unsubscribe(this);
        base.BeforeClose();
    }

    // Yes, it would absolutely be more optimized to insteadof every x seconds refreshing, to just refresh when something changes
    // However, We explicitly do it this way, so all code in the codebase can stay unchanged. The idea is that you can remove the AppStateMonitor and everything will still work
    // This is indeed also possible if you make it if you make everything an observer pattern, where-ever you want to monitor something.
    // However, the problem with that is that we will everything an observer pattern, but something like the MiiImageManager has no reason
    // to be an observer pattern besides this, and it would make the codebase more complex for no reason.
    public void OnUpdate(RepeatedTaskManager sender)
    {
        RrRefreshTimeLeft.Text = RRLiveRooms.Instance.TimeUntilNextTick.Seconds.ToString();
        MiiImagesCashed.Text = ((MemoryCache)Cache).Count.ToString();
    }

    private void LoadSettings()
    {
        WhWzTopMost.IsChecked = ViewUtils.GetLayout().Topmost;
        HttpHelperOff.IsChecked = !HttpClientHelper.FakeConnectionToInternet;
    }

    private void WhWzTopMost_OnClick(object sender, RoutedEventArgs e) => ViewUtils.GetLayout().Topmost = WhWzTopMost.IsChecked == true;

    private void HttpHelperOff_OnClick(object sender, RoutedEventArgs e) =>
        HttpClientHelper.FakeConnectionToInternet = HttpHelperOff.IsChecked != true;

    private void ForceEnableLayout_OnClick(object sender, RoutedEventArgs e) => ViewUtils.GetLayout().SetInteractable(true);

    private void ClearCache_OnClick(object sender, RoutedEventArgs e) => ((MemoryCache)Cache).Clear();

    private void MiiChannel_OnClick(object? sender, RoutedEventArgs e) => DolphinLaunchHelper.LaunchDolphin(" -b -n 0001000248414341");

    private void Open3DTestWindow_OnClick(object? sender, RoutedEventArgs e)
    {
        new ThreeDTestWindow().Show();
    }

    #region Popup Tests

    private async void TestProgressPopup_OnClick(object sender, RoutedEventArgs e)
    {
        ProgressPopupTest.IsEnabled = false;
        var progressWindow = new ProgressWindow("test progress !!");
        progressWindow.SetGoal("Setting a goal!");
        progressWindow.Show();

        for (var i = 0; i < 5; i++)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                progressWindow.UpdateProgress(i * 20);
                progressWindow.SetExtraText($"This is information for iteration {i}");
                if (i == 3)
                    progressWindow.SetGoal($"Changed the Goal");
            });
            await Task.Delay(1000);
        }

        Dispatcher.UIThread.Invoke(() =>
        {
            progressWindow.Close();
            ProgressPopupTest.IsEnabled = true;
        });
    }

    private void TestMessagePopups_OnClick(object sender, RoutedEventArgs e)
    {
        new MessageBoxWindow()
            // .SetMessageType(MessageBoxWindow.MessageType.Message) // Default, so you dont have to type this
            .SetTitleText("Saved Successfully!")
            .SetTag("Tag")
            .SetInfoText("The name you entered has successfully saved in the system")
            .Show();

        new MessageBoxWindow()
            .SetMessageType(MessageBoxWindow.MessageType.Warning)
            .SetTitleText("Invalid license.")
            .SetInfoText(
                "This license has no Mii data or is incomplete.\n" + "Please use the Mii Channel to create a Mii first. \n \n \n more text"
            )
            .Show();

        MessageTranslationHelper.ShowMessage(MessageTranslation.Error_StanderdError);
    }

    private async void YesNoPopup_OnClick(object sender, RoutedEventArgs e)
    {
        var yesNoWindow = await new YesNoWindow()
            .SetExtraText("text for some extra information")
            .SetMainText("Do you click yes or no")
            .AwaitAnswer();

        YesNoPopupButton.Variant = yesNoWindow ? Button.ButtonsVariantType.Primary : Button.ButtonsVariantType.Danger;
    }

    private async void OptionsPopup_OnClick(object sender, RoutedEventArgs e)
    {
        // You can do things in the action with the options.
        // However, you can also read the button click based on the title
        var optionsWindow = await new OptionsWindow()
            .AddOption("PersonMale", "Boy!", () => Console.WriteLine("Option Boy!"))
            .AddOption("PersonFemale", "Girl!", () => Console.WriteLine("Option Girl!"))
            .AddOption("Banana", "Not an Option", () => { }, false)
            .AwaitAnswer();

        OptionsPopupButton.Variant = optionsWindow != null ? Button.ButtonsVariantType.Warning : Button.ButtonsVariantType.Danger;
        OptionsPopupButton.Text = optionsWindow ?? "Clicked away";
    }

    #endregion
}
