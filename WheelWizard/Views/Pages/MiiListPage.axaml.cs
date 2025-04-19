using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using WheelWizard.Shared.DependencyInjection;
using WheelWizard.Views.Components;
using WheelWizard.Views.Popups.Generic;
using WheelWizard.WiiManagement;
using WheelWizard.WiiManagement.Domain.Mii;

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
                Margin = margin,
            };

            miiBlock.ContextMenu = new ContextMenu();
            miiBlock.ContextMenu.Items.Add(new MenuItem { Header = "Edit Mii", Command = new MyCommand(() => EditMii(mii)) });
            miiBlock.ContextMenu.Items.Add(new MenuItem { Header = "Duplicate Mii", Command = new MyCommand(() => DuplicateMii(mii)) });
            miiBlock.ContextMenu.Items.Add(new MenuItem { Header = "Delete Mii", Command = new MyCommand(() => DeleteMii(mii)) });
            MiiList.Children.Add(miiBlock);
        }

        var addBlock = new MiiBlock
        {
            Width = size,
            Height = size,
            Margin = margin,
        };
        MiiList.Children.Add(addBlock);
    }

    private async void DeleteMii(Mii mii)
    {
        // TODO: add a check that you cant remove a Mii that is in use by a lisence,
        // I have no idea how tho

        var result = await new YesNoWindow()
            .SetMainText($"Are you sure you want to delete '{mii.Name}'?")
            .SetExtraText("This action will permanently delete the Mii and cannot be undone.")
            .AwaitAnswer();
        if (!result)
            return;

        MiiDbService.Remove(mii.MiiId);
        ReloadMiiList();
        ViewUtils.ShowSnackbar($"Deleted Mii '{mii.Name}'");
    }

    private void EditMii(Mii mii)
    {
        // TODO: Implement
        ViewUtils.ShowSnackbar($"Lol, you really thing I would let you edit '{mii.Name}'", ViewUtils.SnackbarType.Warning);
    }

    private void DuplicateMii(Mii mii)
    {
        //assuming the mac address is already set correctly
        var result = MiiDbService.Duplicate(mii);
        if (result.IsFailure)
        {
            ViewUtils.ShowSnackbar($"Failed to duplicate Mii '{result.Error.Message}'", ViewUtils.SnackbarType.Danger);
            return;
        }

        ReloadMiiList();
        ViewUtils.ShowSnackbar($"Created duplicate Mii '{mii.Name}'");
    }

    // There must be a better way to do this. Since this seems absurd
    private class MyCommand(Action command) : ICommand
    {
        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            command.Invoke();
        }

        public event EventHandler? CanExecuteChanged;
    }
}
