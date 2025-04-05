using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace WheelWizard.Views.Pages.Settings;

public partial class AppInfo : UserControl
{
    public AppInfo()
    {
        InitializeComponent();
    }

    private void OpenLick_OnClick(object? sender, EventArgs e)
    {
        if (sender is not TemplatedControl control)
            return;
        if (control.Tag == null)
            return;
        
        ViewUtils.OpenLink(control.Tag.ToString()!);
    }
}
