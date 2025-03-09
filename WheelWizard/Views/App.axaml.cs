using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using WheelWizard.AutoUpdating;

namespace WheelWizard.Views;

public class App : Application
{
    /// <summary>
    /// Gets the service provider configured for this application.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the application is not initialized yet.</exception>
    public static IServiceProvider Services =>
        (Current as App)?._serviceProvider ?? throw new InvalidOperationException("The application is not initialized yet.");

    private IServiceProvider? _serviceProvider;

    public override void Initialize()
    {
        var services = new ServiceCollection();
        services.AddWheelWizardServices();

        _serviceProvider = services.BuildServiceProvider();

        AvaloniaXamlLoader.Load(this);
    }

    private static async void OnInitializedAsync()
    {
        try
        {
            var updateService = Services.GetRequiredService<IAutoUpdaterSingletonService>();

            await updateService.CheckForUpdatesAsync();
        }
        catch (Exception e)
        {
            // TODO: Better logging using ILogger<T> and Serilog package
            Console.WriteLine($"Failed to initialize application: {e.Message}");
        }
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new Layout();

            OnInitializedAsync();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
