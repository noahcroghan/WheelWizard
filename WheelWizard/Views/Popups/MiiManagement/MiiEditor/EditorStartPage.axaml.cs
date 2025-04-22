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
    }

    private void PopupPageButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not PopupListButton button)
            return;

        var pageType = button.Tag as Type;
        if (pageType == null)
            return;

        Editor.SetEditorPage(pageType);
    }
}
