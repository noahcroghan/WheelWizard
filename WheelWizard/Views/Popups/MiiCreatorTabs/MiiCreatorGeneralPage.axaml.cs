using Avalonia.Controls;
using WheelWizard.WiiManagement.Domain.Mii;

namespace WheelWizard.Views.Popups.MiiCreatorTabs;

public partial class MiiCreatorGeneralPage : MiiCreatorPageBase, IValidatableMiiPage
{
    private string _miiNameString = "";
    public string MiiNameString
    {
        get => _miiNameString;
        set
        {
            if (SetField(ref _miiNameString, value))
            {
                ValidateMiiName();
            }
        }
    }

    private string? _nameValidationError;
    public string? NameValidationError
    {
        get => _nameValidationError;
        private set => SetField(ref _nameValidationError, value);
    }

    private string _creatorNameString = "";
    public string CreatorNameString
    {
        get => _creatorNameString;
        set
        {
            if (SetField(ref _creatorNameString, value))
            {
                ValidateCreatorName();
            }
        }
    }

    private string? _creatorNameValidationError;
    public string? CreatorNameValidationError
    {
        get => _creatorNameValidationError;
        private set => SetField(ref _creatorNameValidationError, value);
    }

    private IEnumerable<MiiFavoriteColor> _favoriteColorOptions = [];
    public IEnumerable<MiiFavoriteColor> FavoriteColorOptions
    {
        get => _favoriteColorOptions;
        set => SetField(ref _favoriteColorOptions, value ?? []);
    }

    public bool IsMale
    {
        get => !MiiToEdit.IsGirl;
        set
        {
            if (MiiToEdit.IsGirl != value) // Only change if value is different
                return;

            MiiToEdit.IsGirl = !value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(MiiToEdit));
        }
    }

    public bool IsPageValid => string.IsNullOrEmpty(NameValidationError) && string.IsNullOrEmpty(CreatorNameValidationError);

    public MiiCreatorGeneralPage()
    {
        InitializeComponent();
        DataContext = this;
    }

    public override void SetMiiToEdit(Mii mii)
    {
        base.SetMiiToEdit(mii);
        LoadDataFromMii();
        SetFavoriteColorOptions();
    }

    public void SetFavoriteColorOptions()
    {
        // Assuming MiiFavoriteColor is an enum, assign all its values:
        FavoriteColorOptions = Enum.GetValues(typeof(MiiFavoriteColor)).Cast<MiiFavoriteColor>().ToList();
    }

    protected override void LoadDataFromMii()
    {
        _miiNameString = MiiToEdit.Name.ToString();
        _creatorNameString = MiiToEdit.CreatorName.ToString();

        OnPropertyChanged(nameof(MiiNameString));
        OnPropertyChanged(nameof(CreatorNameString));
        OnPropertyChanged(nameof(IsMale));
        ValidatePage();
    }

    public void ValidatePage()
    {
        ValidateMiiName();
        ValidateCreatorName();
        OnPropertyChanged(nameof(IsPageValid));
    }

    public void PrepareForSave()
    {
        var nameResult = MiiName.Create(MiiNameString);
        if (nameResult.IsSuccess)
            MiiToEdit.Name = nameResult.Value;

        var creatorNameResult = MiiName.Create(CreatorNameString);
        MiiToEdit.CreatorName = string.IsNullOrWhiteSpace(CreatorNameString)
            ? MiiName.Create("").Value
            : (creatorNameResult.IsSuccess ? creatorNameResult.Value : MiiToEdit.CreatorName);
    }

    private void ValidateMiiName()
    {
        var nameResult = MiiName.Create(MiiNameString);
        NameValidationError = nameResult.IsFailure ? nameResult.Error.Message : null;

        if (nameResult.IsSuccess && MiiToEdit.Name != nameResult.Value)
        {
            MiiToEdit.Name = nameResult.Value;
        }
        OnPropertyChanged(nameof(IsPageValid));
    }

    private void ValidateCreatorName()
    {
        // Allow empty creator name
        if (string.IsNullOrWhiteSpace(CreatorNameString))
        {
            CreatorNameValidationError = null;
            if (MiiToEdit.CreatorName.ToString() != "")
            {
                MiiToEdit.CreatorName = MiiName.Create("").Value;
            }
        }
        else
        {
            var nameResult = MiiName.Create(CreatorNameString);
            CreatorNameValidationError = nameResult.IsFailure ? nameResult.Error.Message : null;
            if (nameResult.IsSuccess && MiiToEdit.CreatorName != nameResult.Value)
            {
                MiiToEdit.CreatorName = nameResult.Value;
            }
        }
        OnPropertyChanged(nameof(IsPageValid));
    }
}
