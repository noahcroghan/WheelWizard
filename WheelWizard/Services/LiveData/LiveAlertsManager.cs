using WheelWizard.Utilities.RepeatedTasks;
using WheelWizard.Views;
using WheelWizard.WheelWizardData;
using WheelWizard.WheelWizardData.Domain;

namespace WheelWizard.Services.LiveData;

public class LiveAlertsManager : RepeatedTaskManager
{
    public WhWzStatus? Status { get; private set; }
    
    private static LiveAlertsManager? _instance;
    public static LiveAlertsManager Instance => _instance ??= new LiveAlertsManager();

    private LiveAlertsManager() : base(90) { }

    protected override async Task ExecuteTaskAsync()
    {
        var whWzDataService = App.Services.GetRequiredService<IWhWzDataSingletonService>();
        Status = await whWzDataService.GetStatusAsync();
    }
}
