using Avalonia.Interactivity;
using WheelWizard.Views.Components;

namespace WheelWizard.Views.Popups.MiiManagement.MiiEditor;

public partial class EditorStartPage : MiiEditorBaseControl
{
    public EditorStartPage(MiiEditorWindow ew)
        : base(ew)
    {
        InitializeComponent();
        MiiName.Text = Editor.Mii.Name.ToString();
        if (Editor.Mii.IsFavorite)
            FavoriteButton.Classes.Add("favorite");
    }

    private void PopupPageButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not PopupListButton button)
            return;

        if (button.Type is not { } pageType)
            return;

        Editor.SetEditorPage(pageType);
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e) => Editor.Close();

    private void SaveButton_OnClick(object? sender, RoutedEventArgs e) => Editor.SignalSaveMii();

    private void FavoriteButton_OnClick(object? sender, EventArgs e)
    {
        Editor.Mii.IsFavorite = !Editor.Mii.IsFavorite;

        FavoriteButton.Classes.Clear();
        if (Editor.Mii.IsFavorite)
            FavoriteButton.Classes.Add("favorite");
    }
}
