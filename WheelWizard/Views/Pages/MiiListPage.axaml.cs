using WheelWizard.Shared.DependencyInjection;
using WheelWizard.WiiManagement;

namespace WheelWizard.Views.Pages;

public partial class MiiListPage : UserControlBase
{
    [Inject]
    private IMiiDbService MiiDbService { get; set; } = null!;

    public MiiListPage()
    {
        InitializeComponent();
        DataContext = this;

        var miiDbExists = MiiDbService.Exists();
        VisibleWhenNoDolphin.IsVisible = !miiDbExists;
    }
}
