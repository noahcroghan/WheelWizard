using System.ComponentModel;
using Avalonia.Interactivity;
using WheelWizard.Views.Popups.Base;
using WheelWizard.Views.Popups.MiiCreatorTabs;
using WheelWizard.WiiManagement;
using WheelWizard.WiiManagement.Domain.Mii;

namespace WheelWizard.Views.Popups;

public partial class MiiSelectorPopup : PopupContent, INotifyPropertyChanged
{
    private TaskCompletionSource<Mii?> _tcs;
    private Mii? _selectedMii;

    public IEnumerable<Mii> AvailableMiis { get; }

    public Mii? SelectedMii
    {
        get => _selectedMii;
        set
        {
            if (_selectedMii != value)
            {
                _selectedMii = value;
                OnPropertyChanged(nameof(SelectedMii));
            }
        }
    }

    public Mii? Result { get; private set; }

    public MiiSelectorPopup(IEnumerable<Mii> availableMiis, Mii? currentMii)
        : base(true, false, true, "Select Mii")
    {
        InitializeComponent();
        AvailableMiis = availableMiis;
        SelectedMii = currentMii;
        DataContext = this;
    }

    private void SelectButton_Click(object? sender, RoutedEventArgs e)
    {
        Result = SelectedMii;
        _tcs?.TrySetResult(Result);
        Close();
    }

    private async void EditButton_Click(object? sender, RoutedEventArgs e)
    {
        var miiDbService = App.Services.GetRequiredService<IMiiDbService>();
        var popup = new MiiCreatorWindow(miiDbService, SelectedMii);
        await popup.ShowDialogAsync();
    }

    public async Task<Mii?> ShowDialogAsync()
    {
        _tcs = new TaskCompletionSource<Mii?>();
        Show();
        return await _tcs.Task;
    }

    protected override void BeforeClose()
    {
        _tcs?.TrySetResult(null);
        base.BeforeClose();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
