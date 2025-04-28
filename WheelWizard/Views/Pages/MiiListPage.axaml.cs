using System.IO.Abstractions;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Testably.Abstractions;
using WheelWizard.Resources.Languages;
using WheelWizard.Services;
using WheelWizard.Services.Settings;
using WheelWizard.Shared.DependencyInjection;
using WheelWizard.Views.Components;
using WheelWizard.Views.Popups.Generic;
using WheelWizard.Views.Popups.MiiManagement;
using WheelWizard.WiiManagement;
using WheelWizard.WiiManagement.Domain.Mii;

namespace WheelWizard.Views.Pages;

public partial class MiiListPage : UserControlBase
{
    [Inject]
    private IMiiDbService MiiDbService { get; set; } = null!;

    [Inject]
    private IMiiRepositoryService MiiRepositoryService { get; set; } = null!;

    [Inject]
    private IFileSystem FileSystem { get; set; } = null!;

    [Inject]
    private IRandomSystem Random { get; set; } = null!;

    public MiiListPage()
    {
        InitializeComponent();
        DataContext = this;

        var miiDbExists = MiiDbService.Exists();
        if (!miiDbExists)
        {
            var sucess = MiiRepositoryService.ForceCreateDatabase();
            if (sucess.IsFailure)
            {
                ViewUtils.ShowSnackbar($"Failed to create Mii database '{sucess.Error.Message}'", ViewUtils.SnackbarType.Danger);
                VisibleWhenNoDb.IsVisible = !miiDbExists;
            }
        }

        if (miiDbExists)
        {
            VisibleWhenDb.IsVisible = true;
            ReloadMiiList();
        }
    }

    #region Multi and Single select

    private bool _isShiftPressed;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        // Subscribe to keyboard events to track Shift key state
        KeyDown += MiiListPage_KeyDown;
        KeyUp += MiiListPage_KeyUp;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        // Unsubscribe from keyboard events when control is detached
        KeyDown -= MiiListPage_KeyDown;
        KeyUp -= MiiListPage_KeyUp;

        base.OnDetachedFromVisualTree(e);
    }

    private void MiiListPage_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is not (Key.LeftShift or Key.RightShift or Key.LeftCtrl or Key.RightCtrl))
            return;

        if (!_isShiftPressed)
            ChangeBlockSelectionType(true);
        _isShiftPressed = true;
    }

    private void MiiListPage_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key is not (Key.LeftShift or Key.RightShift or Key.LeftCtrl or Key.RightCtrl))
            return;

        if (_isShiftPressed)
            ChangeBlockSelectionType(false);
        _isShiftPressed = false;
    }

    private void ChangeBlockSelectionType(bool multiSelect)
    {
        foreach (var miiBlock in MiiList.Children.OfType<MiiBlock>())
        {
            var groupName = multiSelect ? Guid.NewGuid().ToString() : "MiiListSingleSelect";
            miiBlock.GroupName = groupName;
        }
    }

    #endregion

    private void ReloadMiiList()
    {
        var size = 90;
        var margin = new Thickness(8, 10);

        MiiList.Children.Clear();
        foreach (var mii in MiiDbService.GetAllMiis().OrderByDescending(m => m.IsFavorite).ToList())
        {
            var miiBlock = new MiiBlock
            {
                Mii = mii,
                Width = size,
                Height = size,
                Margin = margin,
            };
            miiBlock.Click += (_, _) => ChangeTopButtons();

            miiBlock.ContextMenu = new ContextMenu();
            miiBlock.ContextMenu.Items.Add(new MenuItem { Header = Common.Action_Edit, Command = new MyCommand(() => EditMii(mii)) });
            miiBlock.ContextMenu.Items.Add(
                new MenuItem { Header = "Duplicate", Command = new MyCommand(() => ContextAction(mii, DuplicateMii)) }
            );
            miiBlock.ContextMenu.Items.Add(
                new MenuItem { Header = Common.Action_Delete, Command = new MyCommand(() => ContextAction(mii, DeleteMii)) }
            );
            miiBlock.ContextMenu.Items.Add(
                new MenuItem { Header = Common.Action_Export, Command = new MyCommand(() => ContextAction(mii, ExportMultipleMiiFiles)) }
            );
            MiiList.Children.Add(miiBlock);
        }

        var count = MiiList.Children.Count;
        ListItemCount.Text = count.ToString();

        ChangeTopButtons();
        if (count >= 100)
            return;

        var addBlock = new MiiBlock
        {
            Width = size,
            Height = size,
            Margin = margin,
        };
        addBlock.Click += (_, _) =>
        {
            foreach (var miiBlock in MiiList.Children.OfType<MiiBlock>())
            {
                miiBlock.IsChecked = false;
            }

            ChangeTopButtons();
            CreateNewMii();
        };

        MiiList.Children.Add(addBlock);
    }

    private async void DeleteMii_OnClick(object? sender, RoutedEventArgs e) => DeleteMii(GetSelectedMiis());

    private async void EditMii_OnClick(object? sender, RoutedEventArgs e) => EditMii(GetSelectedMiis()[0]);

    private async void ExportMii_OnClick(object? sender, RoutedEventArgs e) => ExportMultipleMiiFiles(GetSelectedMiis());

    private async void DuplicateMii_OnClick(object? sender, RoutedEventArgs e) => DuplicateMii(GetSelectedMiis());

    private async void ImportMii_OnClick(object? sender, RoutedEventArgs e)
    {
        var miiFiles = await FilePickerHelper.OpenFilePickerAsync(
            fileType: new FilePickerFileType("mii file") { Patterns = new[] { "*.mii" } },
            allowMultiple: true,
            title: "Select Mii file(s)"
        );
        if (miiFiles.Count == 0)
            return;
        foreach (var file in miiFiles)
        {
            FileSystem.File.Exists(file);
            //get raw bytes from file
            var stream = FileSystem.File.OpenRead(file);
            using var reader = new BinaryReader(stream);
            var miiData = reader.ReadBytes((int)stream.Length);
            stream.Close();
            var result = MiiSerializer.Deserialize(miiData);
            if (result.IsFailure)
            {
                ViewUtils.ShowSnackbar($"Failed to deserialize Mii '{result.Error.Message}'", ViewUtils.SnackbarType.Danger);
                return;
            }

            var mii = result.Value;

            //We duplicate to make sure it does not actually have the original MiiId
            var macAddress = (string)SettingsManager.MACADDRESS.Get();
            var saveResult = MiiDbService.AddToDatabase(mii, macAddress);
            if (saveResult.IsFailure)
            {
                ViewUtils.ShowSnackbar($"Failed to save Mii '{saveResult.Error.Message}'", ViewUtils.SnackbarType.Danger);
                return;
            }
        }
    }

    private async void ExportMultipleMiiFiles(Mii[] miis)
    {
        // TODO: This is not how we should do it, instead it should ask for a folder
        //   2 story points
        foreach (var mii in miis)
            ExportMiiAsFile(mii);
    }

    private async void ExportMiiAsFile(Mii mii)
    {
        var diaglog = await FilePickerHelper.SaveFileAsync(
            title: "Save Mii as file",
            fileTypes: new[] { new FilePickerFileType("Mii file") { Patterns = new[] { "*.mii" } } },
            defaultFileName: $"{mii.Name}"
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

    private async void DeleteMii(Mii[] miis)
    {
        if (miis.Length == 0)
        {
            ViewUtils.ShowSnackbar("It seems there where no Miis to delete", ViewUtils.SnackbarType.Warning);
            return;
        }

        // TODO: add a check that you cant remove a Mii that is in use by a lisence,
        // I have no idea how tho

        var mainText = $"Are you sure you want to delete {miis.Length} Miis?";
        var successMessage = $"Deleted {miis.Length} Miis";
        if (miis.Length == 1)
        {
            mainText = $"Are you sure you want to delete the Mii '{miis[0].Name}'?";
            successMessage = $"Deleted Mii '{miis[0].Name}'";
        }

        var result = await new YesNoWindow()
            .SetMainText(mainText)
            .SetExtraText("This action will permanently delete the Mii(s) and cannot be undone.")
            .AwaitAnswer();
        if (!result)
            return;

        foreach (var mii in miis)
        {
            MiiDbService.Remove(mii.MiiId);
        }
        ReloadMiiList();
        ViewUtils.ShowSnackbar(successMessage);
    }

    private async void EditMii(Mii mii)
    {
        var window = new MiiEditorWindow().SetMii(mii);
        var save = await window.AwaitAnswer();
        if (!save)
            return;

        var result = MiiDbService.Update(mii);
        if (result.IsFailure)
        {
            ViewUtils.ShowSnackbar($"Failed to update Mii '{result.Error.Message}'", ViewUtils.SnackbarType.Danger);
            return;
        }
        ReloadMiiList();
    }

    private async void CreateNewMii()
    {
        string[] presets =
        [
            "liwAZgByADMAZAAAAAAAAAAAAAAAAFYXiRPnfsJmn7skBGWAYIAociBsKEATSLCNEIoAiiUEAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
            "hDQAZwByAG8AbQBwAGEAAAAAAAAAAC8AiRPogsJmn7syxGkAGYCIoiyMCECESACNAIoIiiUEAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
            "xCAAZABhAG4AaQBlAGwAbABlAAAAAGYaiRPo48Jmn7sARAjAAQBokniNaEBjUHiOAIsGiiUEAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
            "0rIAZgBvAHoAaQBsAGwAYQAAAGUAAEBAiRPpIMJmn7sABBLAAUBooohsKECjSGiNAIoGiiUEAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
            "hCgAZgByADAAZAAAAAAAAAAAAAAAAEBAiRPoU8Jmn7sABHDAWYBokoCLKEB0QIiMAIkGiiUEAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
        ];

        var randomIndex = (int)(Random.Random.Shared.NextDouble() * presets.Length);
        var miiResult = MiiSerializer.Deserialize(presets[randomIndex]);
        if (miiResult.IsFailure)
        {
            ViewUtils.ShowSnackbar($"Failed to create a new Mii, Please try again, or message a developer", ViewUtils.SnackbarType.Danger);
            return;
        }

        var window = new MiiEditorWindow().SetMii(miiResult.Value);
        var save = await window.AwaitAnswer();
        if (!save)
            return;

        var result = MiiDbService.AddToDatabase(miiResult.Value, (string)SettingsManager.MACADDRESS.Get());
        if (result.IsFailure)
        {
            ViewUtils.ShowSnackbar($"Failed to create Mii '{result.Error.Message}'", ViewUtils.SnackbarType.Danger);
            return;
        }
        ReloadMiiList();
    }

    private void DuplicateMii(Mii[] miis)
    {
        //assuming the mac address is already set correctly
        var macAddress = (string)SettingsManager.MACADDRESS.Get();
        foreach (var mii in miis)
        {
            var result = MiiDbService.AddToDatabase(mii, macAddress);
            if (!result.IsFailure)
                continue;

            ViewUtils.ShowSnackbar($"Failed to duplicate Mii(s) '{result.Error.Message}'", ViewUtils.SnackbarType.Danger);
            return;
        }

        var successMessage = $"Created {miis.Length} duplicate Miis";
        if (miis.Length == 1)
            successMessage = $"Created duplicate Mii '{miis[0].Name}'";

        ReloadMiiList();
        ViewUtils.ShowSnackbar(successMessage);
    }

    private Mii[] GetSelectedMiis()
    {
        var selected = MiiList
            .Children.OfType<MiiBlock>()
            .Where(block => block is { IsChecked: true, Mii: not null })
            .Select(block => block.Mii!);
        return selected.ToArray();
    }

    private void ChangeTopButtons()
    {
        var selectedMiis = GetSelectedMiis();

        if (selectedMiis.Length == 0)
        {
            DeleteMiisButton.IsVisible = false;
            ExportMiisButton.IsVisible = false;
            EditMiisButton.IsVisible = false;
            DuplicateMiisButton.IsVisible = false;
            ImportMiiButton.IsVisible = true;
            return;
        }

        EditMiisButton.IsVisible = selectedMiis.Length == 1;
        ImportMiiButton.IsVisible = false;
        DeleteMiisButton.IsVisible = true;
        ExportMiisButton.IsVisible = true;
        DuplicateMiisButton.IsVisible = true;
    }

    #region Command

    private void ContextAction(Mii mii, Action<Mii[]> command)
    {
        var selectedMiis = GetSelectedMiis();
        // If the user right clicks and perform action on a selected Mii, that action applies for all the selected Miis
        // But if the user right clicks and perform actions on a mii that is not selected, than it only happens for that specific mii.
        command.Invoke(selectedMiis.Contains(mii) ? selectedMiis : [mii]);
    }

    // There must be a better way to do this. Since this seems absurd
    private class MyCommand(Action command) : ICommand
    {
        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => command.Invoke();

        public event EventHandler? CanExecuteChanged;
    }

    #endregion
}
