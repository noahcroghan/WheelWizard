using Avalonia.Controls;
using Avalonia.Interactivity;

namespace WheelWizard.Views.Popups.MiiManagement.MiiEditor;

public partial class EditorGeneral : MiiEditorBaseControl
{
    private bool _hasMiiNameError;
    private bool _hasCreatorNameError;

    public EditorGeneral(MiiEditorWindow ew)
        : base(ew)
    {
        InitializeComponent();
        DataContext = this;

        MiiName.Text = Editor.Mii.Name.ToString();
        CreatorName.Text = Editor.Mii.CreatorName.ToString();
        GirlToggle.IsChecked = Editor.Mii.IsGirl;
    }

    protected override void BeforeBack()
    {
        // Rather than constantly making a new MiiName, we can also just set it when we return back to the start page
        if (!_hasMiiNameError)
            Editor.Mii.Name = new(MiiName.Text);
        if (!_hasCreatorNameError)
            Editor.Mii.CreatorName = new(CreatorName.Text);
    }

    // We only have to check if it's a female, since if it's not, we already know the other option is going to be the male
    private void IsGirl_OnIsCheckedChanged(object? sender, RoutedEventArgs e) => Editor.Mii.IsGirl = GirlToggle.IsChecked == true;

    private void Name_TextChanged(object sender, TextChangedEventArgs e)
    {
        // MiiName
        var validationMiiNameResult = ValidateMiiName(MiiName.Text);
        _hasMiiNameError = validationMiiNameResult.IsFailure;
        MiiName.ErrorMessage = validationMiiNameResult.Error?.Message ?? "";

        // CreatorName
        var validationCreatorNameResult = ValidateCreatorName(CreatorName.Text);
        _hasCreatorNameError = validationCreatorNameResult.IsFailure;
        CreatorName.ErrorMessage = validationCreatorNameResult.Error?.Message ?? "";
    }

    private OperationResult ValidateMiiName(string newName)
    {
        if (newName.Length is > 10 or < 3)
            return Fail("Name must be between 3 and 10 characters long.");

        return Ok();
    }

    private OperationResult ValidateCreatorName(string newName)
    {
        if (newName.Length > 10)
            return Fail("Creator name must be less than 10 characters long.");

        return Ok();
    }
}
