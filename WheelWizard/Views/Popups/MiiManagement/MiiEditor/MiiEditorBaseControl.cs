using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using WheelWizard.Views.Components;

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

    protected void SetButtons(string type, int count, UniformGrid addTo, Action<int, MultiIconRadioButton> modify)
    {
        for (var i = 0; i <= count; i++)
        {
            var index = i;
            var indexStr = index < 10 ? $"0{i}" : index.ToString();
            var button = new MultiIconRadioButton() { Margin = new(6), IconData = GetMiiIconData($"{type}{indexStr}") };
            modify(index, button);
            addTo.Children.Add(button);
        }
    }
}
