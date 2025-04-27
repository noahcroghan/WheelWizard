using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace WheelWizard.Views.Popups.MiiManagement.MiiEditor;

public class MiiEditorBaseControl : UserControlBase
{
    protected MiiEditorWindow Editor { get; init; }

    protected MiiEditorBaseControl(MiiEditorWindow editor) => Editor = editor;

    protected void BackButton_OnClick(object? sender, EventArgs e)
    {
        BeforeBack();
        Editor.SetEditorPage(typeof(EditorStartPage));
    }

    protected void RefreshImage() => Editor.RefreshImage();

    protected virtual void BeforeBack() { }

    protected DrawingImage GetMiiIconData(string name)
    {
        // If exception is being thrown here, it means the resource is not found, either name is invalid,
        // OR you e.g. forgot to add the list of icons to the App.xaml.
        return (DrawingImage)Application.Current!.FindResource(name)!;
    }
}
