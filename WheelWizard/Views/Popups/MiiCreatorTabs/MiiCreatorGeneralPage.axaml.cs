// For ArgumentNullException
// For IEnumerable
using System.Globalization; // Required for CultureInfo
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters; // Required for IValueConverter
// For OperationResult, adjust if needed
using WheelWizard.WiiManagement.Domain.Mii;

namespace WheelWizard.Views.Popups.MiiCreatorTabs
{
    // Inherit from base class and implement validation interface
    public partial class MiiCreatorGeneralPage : MiiCreatorPageBase, IValidatableMiiPage
    {
        // --- Properties for Binding ---

        private string _miiNameString = "";
        public string MiiNameString
        {
            get => _miiNameString;
            set
            {
                if (SetField(ref _miiNameString, value))
                {
                    ValidateMiiName(); // Validate on change
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
                    ValidateCreatorName(); // Validate on change
                }
            }
        }

        private string? _creatorNameValidationError;
        public string? CreatorNameValidationError
        {
            get => _creatorNameValidationError;
            private set => SetField(ref _creatorNameValidationError, value);
        }

        // Expose favorite colors passed from the main window
        // This needs to be set after instantiation, perhaps via a method or property.
        // For simplicity now, let's assume the main window's DataContext is accessible
        // ONLY IF the main window is set as the page's DataContext, which we are avoiding.
        // A better way: Pass it via SetMiiToEdit or a dedicated method/property.
        // We'll add a property for this.
        private IEnumerable<MiiFavoriteColor> _favoriteColorOptions = [];
        public IEnumerable<MiiFavoriteColor> FavoriteColorOptions
        {
            get => _favoriteColorOptions;
            set => SetField(ref _favoriteColorOptions, value ?? []);
        }

        // Helper property for the Male RadioButton binding
        public bool IsMale
        {
            get => MiiToEdit != null && !MiiToEdit.IsGirl;
            set
            {
                if (MiiToEdit != null && MiiToEdit.IsGirl == value) // Only change if value is different
                {
                    MiiToEdit.IsGirl = !value;
                    OnPropertyChanged(); // Notify IsMale changed
                    OnPropertyChanged(nameof(MiiToEdit)); // Notify MiiToEdit potentially changed (for Female RadioButton)
                }
            }
        }

        // Implementation of IValidatableMiiPage
        public bool IsPageValid => string.IsNullOrEmpty(NameValidationError) && string.IsNullOrEmpty(CreatorNameValidationError);

        public MiiCreatorGeneralPage()
        {
            InitializeComponent();
            // DataContext is set to 'this' by the MiiCreatorWindow when loaded
        }

        // Override SetMiiToEdit to also get FavoriteColorOptions
        public override void SetMiiToEdit(Mii mii)
        {
            base.SetMiiToEdit(mii); // Call base implementation

            // Retrieve options from parent window (less ideal, requires parent reference or static access)
            // A better approach: Pass options via a method or constructor injection.
            // Assuming MiiCreatorWindow exposes FavoriteColorOptions publicly:
            if (this.Parent is ContentControl cc && cc.Parent is MiiCreatorWindow parentWindow)
            {
                FavoriteColorOptions = parentWindow.FavoriteColorOptions;
            }

            // Initial population and validation
            LoadDataFromMii();
        }

        protected override void LoadDataFromMii()
        {
            if (MiiToEdit == null)
                return;

            // Initialize bound properties from the Mii object
            _miiNameString = MiiToEdit.Name?.ToString() ?? ""; // Handle potential null MiiName
            _creatorNameString = MiiToEdit.CreatorName?.ToString() ?? "";

            // Trigger property changed for bindings
            OnPropertyChanged(nameof(MiiNameString));
            OnPropertyChanged(nameof(CreatorNameString));
            OnPropertyChanged(nameof(IsMale)); // Update gender radio button state

            // Perform initial validation
            ValidatePage();
        }

        public void ValidatePage()
        {
            ValidateMiiName();
            ValidateCreatorName();
            // Notify that validity might have changed
            OnPropertyChanged(nameof(IsPageValid));
        }

        // Called by MiiCreatorWindow just before saving
        public void PrepareForSave()
        {
            // Ensure MiiToEdit has the latest valid data from the UI properties
            // This happens automatically if validation updates MiiToEdit properties,
            // otherwise, do final assignments here.

            // Re-apply validated names just in case
            var nameResult = MiiName.Create(MiiNameString);
            if (nameResult.IsSuccess)
                MiiToEdit.Name = nameResult.Value;

            var creatorNameResult = MiiName.Create(CreatorNameString);
            // Handle empty creator name correctly
            MiiToEdit.CreatorName = string.IsNullOrWhiteSpace(CreatorNameString)
                ? MiiName.Create("").Value // Assuming MiiName can handle empty string
                : (creatorNameResult.IsSuccess ? creatorNameResult.Value : MiiToEdit.CreatorName); // Keep old if invalid
        }

        private void ValidateMiiName()
        {
            var nameResult = MiiName.Create(MiiNameString); // Use the MiiName validation
            NameValidationError = nameResult.IsFailure ? nameResult.Error.Message : null;

            // Update the MiiClone only if the name is valid *and* different
            if (nameResult.IsSuccess && MiiToEdit.Name != nameResult.Value)
            {
                MiiToEdit.Name = nameResult.Value;
            }
            // Notify validity potentially changed
            OnPropertyChanged(nameof(IsPageValid));
        }

        private void ValidateCreatorName()
        {
            // Allow empty creator name
            if (string.IsNullOrWhiteSpace(CreatorNameString))
            {
                CreatorNameValidationError = null;
                if (MiiToEdit.CreatorName.ToString() != "") // Update if needed
                {
                    // Assuming MiiName can be created with an empty string
                    MiiToEdit.CreatorName = MiiName.Create("").Value;
                }
            }
            else
            {
                var nameResult = MiiName.Create(CreatorNameString);
                CreatorNameValidationError = nameResult.IsFailure ? nameResult.Error.Message : null;
                // Update the MiiClone only if the name is valid *and* different
                if (nameResult.IsSuccess && MiiToEdit.CreatorName != nameResult.Value)
                {
                    MiiToEdit.CreatorName = nameResult.Value;
                }
            }
            // Notify validity potentially changed
            OnPropertyChanged(nameof(IsPageValid));
        }
    }

    // --- Add Converter ---
    // (Put this in a Converters folder/namespace)
    public class NotConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return !b;
            }
            return AvaloniaProperty.UnsetValue; // Or return false/true based on desired default
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return !b;
            }
            return AvaloniaProperty.UnsetValue;
        }
    }
}
