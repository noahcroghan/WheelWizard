using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using WheelWizard.Models.MiiImages;
using WheelWizard.Services.Settings;
using WheelWizard.Shared.DependencyInjection;
using WheelWizard.Views.Components;
using WheelWizard.Views.Components.MiiImages;
using WheelWizard.Views.Popups;
using WheelWizard.Views.Popups.Generic;
using WheelWizard.WiiManagement;
using WheelWizard.WiiManagement.Domain.Mii;

namespace WheelWizard.Views.BehaviorComponent;

public partial class DetailedProfileBox : UserControlBase, INotifyPropertyChanged
{
    #region Properties

    [Inject]
    private IGameDataSingletonService GameDataService { get; set; } = null!;

    public static readonly StyledProperty<Mii?> MiiProperty = AvaloniaProperty.Register<DetailedProfileBox, Mii?>(nameof(Mii));

    public Mii? Mii
    {
        get => GetValue(MiiProperty);
        set
        {
            SetValue(MiiProperty, value);
            OnPropertyChanged(nameof(Mii));
        }
    }

    public static readonly StyledProperty<Bitmap?> MiiImageProperty = AvaloniaProperty.Register<DetailedProfileBox, Bitmap?>(
        nameof(MiiImage)
    );

    public Bitmap? MiiImage
    {
        get => GetValue(MiiImageProperty);
        set => SetValue(MiiImageProperty, value);
    }

    public static readonly StyledProperty<bool> IsOnlineProperty = AvaloniaProperty.Register<DetailedProfileBox, bool>(nameof(IsOnline));

    public bool IsOnline
    {
        get => GetValue(IsOnlineProperty);
        set => SetValue(IsOnlineProperty, value);
    }

    public static readonly StyledProperty<string> TotalWonProperty = AvaloniaProperty.Register<DetailedProfileBox, string>(
        nameof(TotalWon)
    );

    public string TotalWon
    {
        get => GetValue(TotalWonProperty);
        set => SetValue(TotalWonProperty, value);
    }

    public static readonly StyledProperty<string> TotalRacesProperty = AvaloniaProperty.Register<DetailedProfileBox, string>(
        nameof(TotalRaces)
    );

    public string TotalRaces
    {
        get => GetValue(TotalRacesProperty);
        set => SetValue(TotalRacesProperty, value);
    }

    public static readonly StyledProperty<string> VrProperty = AvaloniaProperty.Register<DetailedProfileBox, string>(nameof(Vr));

    public string Vr
    {
        get => GetValue(VrProperty);
        set => SetValue(VrProperty, value);
    }

    public static readonly StyledProperty<string> BrProperty = AvaloniaProperty.Register<DetailedProfileBox, string>(nameof(Br));

    public string Br
    {
        get => GetValue(BrProperty);
        set => SetValue(BrProperty, value);
    }

    public static readonly StyledProperty<string> FriendCodeProperty = AvaloniaProperty.Register<DetailedProfileBox, string>(
        nameof(FriendCode)
    );

    public string FriendCode
    {
        get => GetValue(FriendCodeProperty);
        set => SetValue(FriendCodeProperty, value);
    }

    public static readonly StyledProperty<string> UserNameProperty = AvaloniaProperty.Register<DetailedProfileBox, string>(
        nameof(UserName)
    );

    public string UserName
    {
        get => GetValue(UserNameProperty);
        set => SetValue(UserNameProperty, value);
    }

    public static readonly StyledProperty<bool> IsCheckedProperty = AvaloniaProperty.Register<DetailedProfileBox, bool>(nameof(IsChecked));

    public bool IsChecked
    {
        get => GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public static readonly StyledProperty<EventHandler<RoutedEventArgs>?> OnCheckedProperty = AvaloniaProperty.Register<
        DetailedProfileBox,
        EventHandler<RoutedEventArgs>?
    >(nameof(OnChecked));

    public EventHandler<RoutedEventArgs>? OnChecked
    {
        get => GetValue(OnCheckedProperty);
        set => SetValue(OnCheckedProperty, value);
    }

    public static readonly StyledProperty<EventHandler?> OnRenameProperty = AvaloniaProperty.Register<DetailedProfileBox, EventHandler?>(
        nameof(OnRename)
    );

    public EventHandler? OnRename
    {
        get => GetValue(OnRenameProperty);
        set => SetValue(OnRenameProperty, value);
    }

    public static readonly StyledProperty<Action<string>?> ViewRoomActionProperty = AvaloniaProperty.Register<
        FriendsListItem,
        Action<string>?
    >(nameof(ViewRoomAction));

    public Action<string>? ViewRoomAction
    {
        get => GetValue(ViewRoomActionProperty);
        set => SetValue(ViewRoomActionProperty, value);
    }

    #endregion

    public DetailedProfileBox()
    {
        InitializeComponent();
        DataContext = this;

        if ((bool)SettingsManager.ENABLE_ANIMATIONS.Get())
        {
            // We set them all at least one, just to make sure the request is being send.
            // sometimes this still works goofy though, for some reason
            MiiFaceImageLoader.ImageVariant = MiiImageVariants.Variant.SLIGHT_SIDE_PROFILE_HOVER;
            MiiFaceImageLoader.ImageVariant = MiiImageVariants.Variant.SLIGHT_SIDE_PROFILE_INTERACT;
            MiiFaceImageLoader.ImageVariant = MiiImageVariants.Variant.SLIGHT_SIDE_PROFILE_DEFAULT;

            MiiFaceImageLoader.PointerEntered += (_, _) =>
                MiiFaceImageLoader.ImageVariant = MiiImageVariants.Variant.SLIGHT_SIDE_PROFILE_HOVER;
            MiiFaceImageLoader.PointerExited += (_, _) =>
                MiiFaceImageLoader.ImageVariant = MiiImageVariants.Variant.SLIGHT_SIDE_PROFILE_DEFAULT;
            MiiFaceImageLoader.PointerPressed += (_, _) =>
                MiiFaceImageLoader.ImageVariant = MiiImageVariants.Variant.SLIGHT_SIDE_PROFILE_INTERACT;
            MiiFaceImageLoader.PointerReleased += (_, _) =>
                MiiFaceImageLoader.ImageVariant = MiiImageVariants.Variant.SLIGHT_SIDE_PROFILE_HOVER;
        }
    }

    public void ViewRoom_OnClick(object? sender, RoutedEventArgs e) => ViewRoomAction.Invoke(FriendCode);

    public void RenameMii_OnClick(object? obj, EventArgs e) => OnRename.Invoke(obj, e);

    private void CopyFriendCode_OnClick(object? obj, EventArgs e)
    {
        TopLevel.GetTopLevel(this)?.Clipboard?.SetTextAsync(FriendCode);
    }

    public async void OpenMiiEditor_Click(object? sender, RoutedEventArgs e)
    {
        var miiDbService = App.Services.GetRequiredService<IMiiDbService>();

        // 2. Get the list of available Miis (adjust logic as needed)
        // Assuming GetAvailableMiis() returns the list you want to show
        IEnumerable<Mii> availableMiis;
        try
        {
            availableMiis = miiDbService.GetAllMiis().ToList();
        }
        catch (Exception ex)
        {
            // Handle potential errors fetching Miis
            new MessageBoxWindow()
                .SetTitleText("Error Loading Miis")
                .SetInfoText($"Could not load available Miis: {ex.Message}")
                .SetMessageType(MessageBoxWindow.MessageType.Error)
                .Show();
            return;
        }

        if (!availableMiis.Any())
        {
            new MessageBoxWindow()
                .SetTitleText("No Miis Found")
                .SetInfoText("There are no other Miis available to select.")
                .SetMessageType(MessageBoxWindow.MessageType.Warning)
                .Show();
            return;
        }

        // 3. Create and show the new selector popup
        var selectorPopup = new MiiSelectorPopup(availableMiis, this.Mii);
        var selectedMiiResult = await selectorPopup.ShowDialogAsync();

        // 4. Handle the result
        if (selectedMiiResult == null)
        {
            // User closed the popup without selecting (or clicked Cancel/X)
            Console.WriteLine("Mii selection cancelled.");
            return;
        }

        // User clicked "Select Mii" and chose a Mii
        this.Mii = selectedMiiResult; // Update the Mii property bound to the UI

        // Optional: Notify parent or trigger other logic if needed
        Console.WriteLine($"Mii '{selectedMiiResult.Name}' selected for profile '{this.UserName}'.");

        // TODO: Add logic here to actually SAVE the selected Mii association
        // to the license/profile this DetailedProfileBox represents.
        // This usually involves calling a service method.
        new MessageBoxWindow()
            .SetTitleText("Selection Applied (Visually)")
            .SetInfoText(
                $"Mii '{selectedMiiResult.Name}' has been selected. Saving the change to the actual license data is not yet implemented."
            )
            .Show();
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new(propertyName));
    }

    private void CheckBox_OnChecked(object? sender, RoutedEventArgs e)
    {
        OnChecked.Invoke(sender, e);
    }
}
