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
}
