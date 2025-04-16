using System.Windows.Input;
using Avalonia;
using Avalonia.Data;
using WheelWizard.WiiManagement.Domain.Mii;

namespace WheelWizard.Views.BehaviorComponent;

public partial class MiiSelectorGrid : UserControlBase
{
    #region Properties

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
        defaultBindingMode: BindingMode.TwoWay,
        coerce: CoerceSelectedItem
    );

    public Mii? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public static readonly StyledProperty<ICommand?> SelectionChangedCommandProperty = AvaloniaProperty.Register<
        MiiSelectorGrid,
        ICommand?
    >(nameof(SelectionChangedCommand));

    public ICommand? SelectionChangedCommand
    {
        get => GetValue(SelectionChangedCommandProperty);
        set => SetValue(SelectionChangedCommandProperty, value);
    }

    #endregion

    public MiiSelectorGrid()
    {
        InitializeComponent();
        // DataContext = this;
        // I am not sure why it breaks when setting this, anyways, this has to be set eventually, otherwise it's not setup correctly
    }

    private static Mii? CoerceSelectedItem(AvaloniaObject d, Mii? value)
    {
        if (d is not MiiSelectorGrid grid)
            return value;

        var command = grid.SelectionChangedCommand;
        if (command?.CanExecute(value) ?? false)
            command.Execute(value);

        return value;
    }
}
