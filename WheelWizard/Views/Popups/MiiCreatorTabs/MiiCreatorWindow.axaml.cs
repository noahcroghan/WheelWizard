using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Interactivity;
using WheelWizard.Services.Settings;
using WheelWizard.Views.Popups.Generic;
using WheelWizard.WiiManagement;
using WheelWizard.WiiManagement.Domain.Mii;

namespace WheelWizard.Views.Popups.MiiCreatorTabs;

public partial class MiiCreatorWindow : PopupContent, INotifyPropertyChanged
{
    private IGameLicenseSingletonService gameLicenseService = App.Services.GetService<IGameLicenseSingletonService>();

    private readonly IMiiDbService _miiDbService;
    private TaskCompletionSource<Mii?> _tcs = new();
    private readonly Mii _originalMii;
    private readonly Mii _miiClone;
    public Mii MiiClone => _miiClone;

    private bool _isCurrentPageValid = true;
    public bool IsCurrentPageValid
    {
        get => _isCurrentPageValid;
        private set => SetField(ref _isCurrentPageValid, value, nameof(IsSaveEnabled)); // Notify IsSaveEnabled depends on this
    }

    public bool IsSaveEnabled => IsCurrentPageValid;

    public IEnumerable<MiiFavoriteColor> FavoriteColorOptions { get; }

    public MiiCreatorWindow(IMiiDbService miiDbService, Mii? existingMii = null)
        : base(true, false, true, existingMii == null ? "Create New Mii" : $"Edit Mii: {existingMii?.Name}") // Use null conditional
    {
        _miiDbService = miiDbService ?? throw new ArgumentNullException(nameof(miiDbService));

        if (existingMii != null)
        {
            _originalMii = existingMii;
            _miiClone = CloneMii(existingMii);
        }
        else
        {
            _originalMii = new();
            _miiClone = new();
        }

        FavoriteColorOptions = Enum.GetValues<MiiFavoriteColor>();

        InitializeComponent();
        DataContext = this;
        LoadMiiPage("General");
        UpdateValidationState();
    }

    private void Navigation_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is RadioButton { Tag: string sectionName, IsChecked: true })
        {
            LoadMiiPage(sectionName);
        }
    }

    private void LoadMiiPage(string sectionName)
    {
        UserControl? page = sectionName switch
        {
            "General" => new MiiCreatorGeneralPage(),
            //"Face" => new MiiCreatorFacePage(),
            //"Hair" => new MiiCreatorHairPage(),
            //"Eyes" => new MiiCreatorEyesPage(),
            //"Eyebrows" => new MiiCreatorEyebrowsPage(),
            //"Nose" => new MiiCreatorNosePage(),
            //"Mouth" => new MiiCreatorMouthPage(),
            //"FacialHair" => new MiiCreatorFacialHairPage(),
            //"Mole" => new MiiCreatorMolePage(),
            //"Glasses" => new MiiCreatorGlassesPage(),
            //"Body" => new MiiCreatorBodyPage(),
            _ => new MiiCreatorGeneralPage(), // Default or handle error
        };
        if (page is MiiCreatorPageBase miiPage) // Use a base class or check type
        {
            miiPage.SetMiiToEdit(_miiClone); // Pass the clone
            //Subscribe to the page's validity changes
            if (miiPage is INotifyPropertyChanged notifyingPage)
            {
                // Unsubscribe from previous page if any
                if (MiiContent.Content is INotifyPropertyChanged oldPage)
                {
                    oldPage.PropertyChanged -= Page_PropertyChanged;
                }
                notifyingPage.PropertyChanged += Page_PropertyChanged;
            }
        }
        // Set the page's DataContext to itself to enable bindings within the page
        page.DataContext = page;

        MiiContent.Content = page;
        UpdateValidationState(); // Check validity of the newly loaded page
    }

    private void Page_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Check if the property that changed signifies a validity update
        // (e.g., IsPageValid or specific validation error properties)
        if (
            e.PropertyName == nameof(IValidatableMiiPage.IsPageValid)
            || e.PropertyName?.Contains("ValidationError") == true
            || e.PropertyName == "IsValid"
        )
        {
            UpdateValidationState();
        }
    }

    private void UpdateValidationState()
    {
        if (MiiContent.Content is IValidatableMiiPage validatablePage)
        {
            IsCurrentPageValid = validatablePage.IsPageValid;
        }
        else if (MiiContent.Content is MiiCreatorGeneralPage generalPage)
        {
            IsCurrentPageValid = generalPage.IsPageValid;
        }
        else
        {
            IsCurrentPageValid = true;
        }
        OnPropertyChanged(nameof(IsSaveEnabled));
    }

    public Task<Mii?> ShowDialogAsync()
    {
        _tcs = new();
        Show();
        return _tcs.Task;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (MiiContent.Content is IValidatableMiiPage currentValidatablePage)
        {
            currentValidatablePage.ValidatePage(); // Ensure latest validation runs
            if (!currentValidatablePage.IsPageValid)
            {
                await ShowValidationErrorDialog("Please fix the errors on the current page before saving.");
                return;
            }
            currentValidatablePage.PrepareForSave();
        }
        // todo: Add loop here to check ALL pages if cross-page validation is needed
        // This requires storing references to all created pages. For now, we only check current.

        // 2. Perform Save Operation (logic moved from specific validation)
        OperationResult result;
        // Determine if it's a new Mii based on whether an existing one was passed *or* if the clone still has default ID.
        bool isNewMii = _originalMii.MiiId == 0; // Simpler check: was the original Mii a new one?

        if (isNewMii)
        {
            // Ensure essential fields like Name are valid *before* generating ID/saving
            // The page should have already validated and updated MiiClone.Name
            if (string.IsNullOrWhiteSpace(_miiClone.Name.ToString()) || _miiClone.Name.ToString() == "no name")
            {
                await ShowValidationErrorDialog("Mii Name cannot be empty. Please go to the General tab.");
                // Optionally navigate to the General tab here.
                return;
            }
            _miiClone.Date = DateOnly.FromDateTime(DateTime.Now); // Set creation date

            var macAddress = (string)SettingsManager.MACADDRESS.Get();
            result = _miiDbService.AddToDatabase(_miiClone, macAddress);
            if (!result.IsSuccess && _miiClone.MiiId != 0)
            {
                _miiClone.MiiId = 0; // Reset ID on failure
            }
        }
        else
        {
            // Ensure Mii ID is correctly set from the original for update
            _miiClone.MiiId = _originalMii.MiiId;
            // Ensure essential fields like Name are valid
            if (string.IsNullOrWhiteSpace(_miiClone.Name.ToString()) || _miiClone.Name.ToString() == "no name")
            {
                await ShowValidationErrorDialog("Mii Name cannot be empty. Please go to the General tab.");
                // Optionally navigate to the General tab here.
                return;
            }
            result = _miiDbService.Update(_miiClone);
        }

        // 3. Handle Result
        if (result.IsSuccess)
        {
            _tcs.TrySetResult(_miiClone);
            Close();
        }
        else
        {
            // Keep Mii ID if it was successfully generated but DB save failed,
            // allowing retry without new ID unless it was the cause of failure.
            await new MessageBoxWindow()
                .SetMessageType(MessageBoxWindow.MessageType.Error)
                .SetTitleText("Save Failed")
                .SetInfoText(result.Error.Message) // Make sure OperationResult has a meaningful error message
                .ShowDialog();
        }
        gameLicenseService.LoadLicense();
    }

    private async Task ShowValidationErrorDialog(string message)
    {
        await new MessageBoxWindow()
            .SetMessageType(MessageBoxWindow.MessageType.Warning)
            .SetTitleText("Validation Error")
            .SetInfoText(message)
            .ShowDialog();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    protected override void BeforeClose()
    {
        if (MiiContent.Content is INotifyPropertyChanged notifyingPage)
        {
            notifyingPage.PropertyChanged -= Page_PropertyChanged;
        }

        _tcs.TrySetResult(null);
        base.BeforeClose();
    }

    #region INotifyPropertyChanged Implementation
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private bool SetField<T>(ref T field, T value, params string[] additionalProperties)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
        field = value;
        // Notify the property itself (CallerMemberName gets this)
        OnPropertyChanged(null); // Use null to rely on CallerMemberName
        // Notify dependent properties
        foreach (var propName in additionalProperties)
        {
            OnPropertyChanged(propName);
        }
        return true;
    }
    #endregion

    private Mii CloneMii(Mii source)
    {
        var clonedName = source.Name;
        var clonedCreator = source.CreatorName;

        var clone = new Mii
        {
            IsInvalid = source.IsInvalid,
            IsGirl = source.IsGirl,
            Date = source.Date,
            MiiFavoriteColor = source.MiiFavoriteColor,
            IsFavorite = source.IsFavorite,
            Height = source.Height,
            Weight = source.Weight,
            MiiId = source.MiiId,
            SystemId0 = source.SystemId0,
            SystemId1 = source.SystemId1,
            SystemId2 = source.SystemId2,
            SystemId3 = source.SystemId3,
            Name = clonedName,
            CreatorName = clonedCreator,
            MiiFacial = source.MiiFacial,
            MiiHair = source.MiiHair,
            MiiEyebrows = source.MiiEyebrows,
            MiiEyes = source.MiiEyes,
            MiiNose = source.MiiNose,
            MiiLips = source.MiiLips,
            MiiGlasses = source.MiiGlasses,
            MiiFacialHair = source.MiiFacialHair,
            MiiMole = source.MiiMole,
        };
        return clone;
    }
}
