using Avalonia.Interactivity;
using WheelWizard.Views.Popups.Base;

namespace WheelWizard.Views.Popups;

public partial class ThreeDTestWindow : PopupContent
{
    public ThreeDTestWindow()
        : base(true, true, true, "3dTestWindow")
    {
        InitializeComponent();
    }

    private void Close_OnClick(object? sender, RoutedEventArgs e) => Close();
}
