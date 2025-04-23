using System.ComponentModel;
using Avalonia.Threading;
using WheelWizard.Views.Popups.Base;
using WheelWizard.Views.Popups.MiiManagement.MiiEditor;
using WheelWizard.WiiManagement.Domain.Mii;

namespace WheelWizard.Views.Popups.MiiManagement;

public partial class MiiEditorWindow : PopupContent, INotifyPropertyChanged
{
    // whether or not you want to save the Mii
    public bool Result { get; private set; } = false;
    private TaskCompletionSource<bool> _tcs;

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

    public void SignalSaveMii()
    {
        Result = true;
        _tcs.TrySetResult(true);
        Close();
    }

    protected override void BeforeClose()
    {
        // If you want to return something different, then to the TrySetResult before you close it
        _tcs.TrySetResult(false);
    }

    public async Task<bool> AwaitAnswer()
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            return await Dispatcher.UIThread.InvokeAsync(() => AwaitAnswer());
        }
        _tcs = new();
        Show(); // Or ShowDialog(parentWindow) if you need it to be modal
        return await _tcs.Task;
    }

    #region PropertyChanged

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new(propertyName));
    }

    #endregion
}
