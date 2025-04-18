using Avalonia;
using WheelWizard.Shared.DependencyInjection;
using WheelWizard.Views.Components;
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
        VisibleWhenNoDb.IsVisible = !miiDbExists;

        if (miiDbExists)
        {
            VisibleWhenDb.IsVisible = true;
            ReloadMiiList();
        }
    }

    private void ReloadMiiList()
    {
        var size = 90;
        var margin = new Thickness(8, 10);

        MiiList.Children.Clear();
        foreach (var mii in MiiDbService.GetAllMiis().ToList())
        {
            var miiBlock = new MiiBlock
            {
                Mii = mii,
                Width = size,
                Height = size,
                Margin = margin
            };
            MiiList.Children.Add(miiBlock);
        }

        var addBlock = new MiiBlock
        {
            Width = size,
            Height = size,
            Margin = margin
        };
        MiiList.Children.Add(addBlock);
    }
}
