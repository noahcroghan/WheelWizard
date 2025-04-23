using System.ComponentModel;
using WheelWizard.Views.Popups.Base;
using WheelWizard.Views.Popups.MiiManagement.MiiEditor;
using WheelWizard.WiiManagement.Domain.Mii;

namespace WheelWizard.Views.Popups.MiiManagement;

public partial class MiiEditorWindow : PopupContent, INotifyPropertyChanged
{
    private Mii _mii;
    public Mii Mii
    {
        get => _mii;
        private set
        {
            if (_mii != value)
            {
                _mii = value;
                OnPropertyChanged(nameof(Mii));
            }
        }
    }

    public MiiEditorWindow()
        : base(true, false, false, "Mii Editor")
    {
        InitializeComponent();
        DataContext = this;
        Carousel.MiiImageLoaded += (_, _) => MiiLoadingIcon.IsVisible = false;
    }

    protected override void BeforeOpen()
    {
        base.BeforeOpen();
        SetEditorPage(typeof(EditorStartPage));
    }

    public void SetEditorPage(Type pageType)
    {
        EditorPresenter.Content = Activator.CreateInstance(pageType, this)!;
    }

    public MiiEditorWindow SetMii(Mii miiToEdit)
    {
        Window.WindowTitle = $"Mii Editor - {miiToEdit.Name}";
        Mii = miiToEdit;
        return this;
    }

    #region PropertyChanged

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new(propertyName));
    }

    #endregion
}
