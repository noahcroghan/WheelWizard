namespace WheelWizard.Views.Popups.MiiManagement.MiiEditor;

public class MiiEditorBaseControl : UserControlBase
{
    protected MiiEditorWindow Editor { get; init; }

    protected MiiEditorBaseControl(MiiEditorWindow editor) => Editor = editor;
}
