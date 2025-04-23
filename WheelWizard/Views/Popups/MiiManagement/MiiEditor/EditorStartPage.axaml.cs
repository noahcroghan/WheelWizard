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

        if (button.Type is not { } pageType)
            return;

        Editor.SetEditorPage(pageType);
    }
}
