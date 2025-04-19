using System.IO.Abstractions;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using WheelWizard.Services;
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

    [Inject]
    private IFileSystem FileSystem { get; set; } = null!;

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
            miiBlock.ContextMenu.Items.Add(new MenuItem { Header = "Save Mii as file", Command = new MyCommand(() => SaveMiiAsFile(mii)) });
            MiiList.Children.Add(miiBlock);
        }

        ListItemCount.Text = MiiList.Children.Count.ToString();
        var addBlock = new MiiBlock
        {
            Width = size,
            Height = size,
            Margin = margin,
        };
        MiiList.Children.Add(addBlock);
    }

    private async void SaveMiiAsFile(Mii mii)
    {
        var diaglog = await FilePickerHelper.SaveFileAsync(
            title: "Save Mii as file",
            fileTypes: new[] { new FilePickerFileType("Mii file") { Patterns = new[] { "*.mii" } } },
            defaultFileName: "Mymii.mii"
        );
        if (diaglog == null)
            return;
        var result = MiiDbService.GetByAvatarId(mii.MiiId);
        if (result.IsFailure)
        {
            ViewUtils.ShowSnackbar($"Failed to get Mii '{result.Error.Message}'", ViewUtils.SnackbarType.Danger);
            return;
        }
        var miiData = MiiSerializer.Serialize(result.Value);
        if (miiData.IsFailure)
        {
            ViewUtils.ShowSnackbar($"Failed to serialize Mii '{miiData.Error.Message}'", ViewUtils.SnackbarType.Danger);
            return;
        }
        var file = FileSystem.FileInfo.New(diaglog);
        using var stream = file.Open(FileMode.Create, FileAccess.Write);
        using var writer = new BinaryWriter(stream);
        writer.Write(miiData.Value);
        writer.Flush();
        writer.Close();
        stream.Close();
        ViewUtils.ShowSnackbar($"Saved Mii '{mii.Name}' to file '{file.FullName}'");
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
