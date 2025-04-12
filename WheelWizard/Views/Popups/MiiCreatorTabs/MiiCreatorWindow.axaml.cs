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
    private readonly IMiiDbService _miiDbService;
    private TaskCompletionSource<Mii?> _tcs = new();
    private Mii _originalMii;
    private Mii _miiCloneClone;

    public Mii MiiClone
    {
        get => _miiCloneClone;
    }

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
    public bool IsValid => string.IsNullOrEmpty(NameValidationError) && string.IsNullOrEmpty(CreatorNameValidationError);

    public IEnumerable<MiiFavoriteColor> FavoriteColorOptions { get; }

    public MiiCreatorWindow(IMiiDbService miiDbService, Mii? existingMii = null)
        : base(true, false, true, existingMii == null ? "Create New Mii" : $"Edit Mii: {existingMii.Name}")
    {
        _miiDbService = miiDbService ?? throw new ArgumentNullException(nameof(miiDbService));

        if (existingMii != null)
        {
            _originalMii = existingMii;
            _miiCloneClone = CloneMii(existingMii);
            _miiNameString = existingMii.Name.ToString();
            _creatorNameString = existingMii.CreatorName.ToString();
        }
        else
        {
            _originalMii = new();
            _miiCloneClone = new();
            _miiNameString = "";
            _creatorNameString = "";
        }

        FavoriteColorOptions = Enum.GetValues<MiiFavoriteColor>();

        InitializeComponent();
        DataContext = this;
        LoadMiiPage("General");
        ValidateMiiName();
        ValidateCreatorName();
    }

    private void Navigation_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is RadioButton { Tag: string sectionName } radioButton && radioButton.IsChecked == true)
        {
            LoadMiiPage(sectionName);
        }
    }

    private void LoadMiiPage(string sectionName)
    {
        UserControl? page = sectionName switch
        {
            "General" => new MiiCreatorGeneralPage(),
            "Face" => new MiiCreatorFacePage(),
            "Hair" => new MiiCreatorHairPage(),
            "Eyes" => new MiiCreatorEyesPage(),
            "Eyebrows" => new MiiCreatorEyebrowsPage(),
            "Nose" => new MiiCreatorNosePage(),
            "Mouth" => new MiiCreatorMouthPage(),
            "FacialHair" => new MiiCreatorFacialHairPage(),
            "Mole" => new MiiCreatorMolePage(),
            "Glasses" => new MiiCreatorGlassesPage(),
            "Body" => new MiiCreatorBodyPage(),
            _ => new MiiCreatorGeneralPage(), // Default or handle error
        };
        page.DataContext = this; //
        MiiContent.Content = page;
    }

    private void ValidateMiiName()
    {
        var nameResult = MiiName.Create(MiiNameString);
        NameValidationError = nameResult.IsFailure ? nameResult.Error.Message : null;
        OnPropertyChanged(nameof(IsValid));
    }

    private void ValidateCreatorName()
    {
        if (string.IsNullOrWhiteSpace(CreatorNameString))
        {
            CreatorNameValidationError = null;
        }
        else
        {
            var nameResult = MiiName.Create(CreatorNameString);
            CreatorNameValidationError = nameResult.IsFailure ? nameResult.Error.Message : null;
        }
        OnPropertyChanged(nameof(IsValid));
    }

    public Task<Mii?> ShowDialogAsync()
    {
        _tcs = new();
        Show();
        return _tcs.Task;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        ValidateMiiName();
        ValidateCreatorName();
        if (!IsValid)
        {
            await new MessageBoxWindow()
                .SetMessageType(MessageBoxWindow.MessageType.Warning)
                .SetTitleText("Validation Error")
                .SetInfoText("Please fix the errors before saving.")
                .ShowDialog();
            return;
        }

        var nameResult = MiiName.Create(MiiNameString);
        if (nameResult.IsSuccess)
            _miiCloneClone.Name = nameResult.Value;

        var creatorNameResult = MiiName.Create(CreatorNameString);
        _miiCloneClone.CreatorName = string.IsNullOrWhiteSpace(CreatorNameString) ? new("") : creatorNameResult.Value;

        OperationResult result;
        bool isNewMii = _miiCloneClone.MiiId == 0 || _originalMii.MiiId == 0;
        if (isNewMii)
        {
            _miiCloneClone.MiiId = (uint)Random.Shared.Next(1, int.MaxValue);
            // TODO: Robust ID generation needed! Check for collisions.

            var macAddress = (string)SettingsManager.MACADDRESS.Get();
            result = _miiDbService.AddToDatabase(_miiCloneClone, macAddress);
            if (!result.IsSuccess && _miiCloneClone.MiiId != 0)
            {
                _miiCloneClone.MiiId = 0;
            }
        }
        else
        {
            _miiCloneClone.MiiId = _originalMii.MiiId;
            result = _miiDbService.Update(_miiCloneClone);
        }
        if (result.IsSuccess)
        {
            _tcs.TrySetResult(_miiCloneClone);
            Close();
        }
        else
        {
            await new MessageBoxWindow()
                .SetMessageType(MessageBoxWindow.MessageType.Error)
                .SetTitleText("Save Failed")
                .SetInfoText(result.Error.Message)
                .ShowDialog();
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    protected override void BeforeClose()
    {
        _tcs.TrySetResult(null);
        base.BeforeClose();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private Mii CloneMii(Mii? source)
    {
        if (source == null)
            return new Mii();
        var clone = new Mii
        {
            // Copy properties from the source Mii
            IsInvalid = source.IsInvalid,
            IsGirl = source.IsGirl,
            Date = source.Date,
            MiiFavoriteColor = source.MiiFavoriteColor,
            IsFavorite = source.IsFavorite,
            Name = source.Name,
            Height = source.Height,
            Weight = source.Weight,
            MiiId = source.MiiId,
            SystemId0 = source.SystemId0,
            SystemId1 = source.SystemId1,
            SystemId2 = source.SystemId2,
            SystemId3 = source.SystemId3,
            CreatorName = source.CreatorName,

            // Copy complex properties (assuming they are also cloneable or value types)
            // If these are reference types that can be modified, they need cloning too!
            // For now, assuming shallow copy is okay for these domain objects or they handle it internally.
            MiiFacial = source.MiiFacial, // Assuming struct or needs deep clone if class
            MiiHair = source.MiiHair, // Assuming struct or needs deep clone if class
            MiiEyebrows = source.MiiEyebrows, // Assuming struct or needs deep clone if class
            MiiEyes = source.MiiEyes, // Assuming struct or needs deep clone if class
            MiiNose = source.MiiNose, // Assuming struct or needs deep clone if class
            MiiLips = source.MiiLips, // Assuming struct or needs deep clone if class
            MiiGlasses = source.MiiGlasses, // Assuming struct or needs deep clone if class
            MiiFacialHair = source.MiiFacialHair, // Assuming struct or needs deep clone if class
            MiiMole = source.MiiMole, // Assuming struct or needs deep clone if class
        };
        return clone;
    }
}

// Create the actual files later as needed.
public class MiiCreatorFacePage : UserControl
{
    public MiiCreatorFacePage() =>
        Content = new TextBlock
        {
            Text = "Face Page - TODO",
            Margin = new(10),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
        };
}

public class MiiCreatorHairPage : UserControl
{
    public MiiCreatorHairPage() =>
        Content = new TextBlock
        {
            Text = "Hair Page - TODO",
            Margin = new(10),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
        };
}

public class MiiCreatorEyesPage : UserControl
{
    public MiiCreatorEyesPage() =>
        Content = new TextBlock
        {
            Text = "Eyes Page - TODO",
            Margin = new(10),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
        };
}

public class MiiCreatorEyebrowsPage : UserControl
{
    public MiiCreatorEyebrowsPage() =>
        Content = new TextBlock
        {
            Text = "Eyebrows Page - TODO",
            Margin = new(10),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
        };
}

public class MiiCreatorNosePage : UserControl
{
    public MiiCreatorNosePage() =>
        Content = new TextBlock
        {
            Text = "Nose Page - TODO",
            Margin = new(10),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
        };
}

public class MiiCreatorMouthPage : UserControl
{
    public MiiCreatorMouthPage() =>
        Content = new TextBlock
        {
            Text = "Mouth Page - TODO",
            Margin = new(10),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
        };
}

public class MiiCreatorFacialHairPage : UserControl
{
    public MiiCreatorFacialHairPage() =>
        Content = new TextBlock
        {
            Text = "Facial Hair Page - TODO",
            Margin = new(10),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
        };
}

public class MiiCreatorMolePage : UserControl
{
    public MiiCreatorMolePage() =>
        Content = new TextBlock
        {
            Text = "Mole Page - TODO",
            Margin = new(10),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
        };
}

public class MiiCreatorGlassesPage : UserControl
{
    public MiiCreatorGlassesPage() =>
        Content = new TextBlock
        {
            Text = "Glasses Page - TODO",
            Margin = new(10),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
        };
}

public class MiiCreatorBodyPage : UserControl
{
    public MiiCreatorBodyPage() =>
        Content = new TextBlock
        {
            Text = "Body Page - TODO",
            Margin = new(10),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
        };
}
