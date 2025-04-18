using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using WheelWizard.Views.Popups.MiiCreatorTabs;
using WheelWizard.WiiManagement;
using WheelWizard.WiiManagement.Domain.Mii;

namespace WheelWizard.Views.BehaviorComponent;

public partial class MiiSelectorGrid : UserControl
{
    public static readonly StyledProperty<IEnumerable<Mii>> ItemsSourceProperty = AvaloniaProperty.Register<
        MiiSelectorGrid,
        IEnumerable<Mii>
    >(nameof(ItemsSource));

    public IEnumerable<Mii> ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly StyledProperty<Mii?> SelectedItemProperty = AvaloniaProperty.Register<MiiSelectorGrid, Mii?>(
        nameof(SelectedItem),
        defaultValue: null,
        defaultBindingMode: BindingMode.TwoWay,
        enableDataValidation: false,
        coerce: null
    );

    public Mii? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public MiiSelectorGrid()
    {
        InitializeComponent();
        AddMiiButton.Click += AddMiiButton_Click;
    }

    private async void AddMiiButton_Click(object? sender, RoutedEventArgs e)
    {
        var miiDb = App.Services.GetRequiredService<IMiiDbService>();
        var miiCreatorWindow = new MiiCreatorWindow(miiDb, new());
        await miiCreatorWindow.ShowDialogAsync();
    }
}
