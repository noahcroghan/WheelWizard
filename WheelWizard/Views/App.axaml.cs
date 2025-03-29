using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Logging;
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

    /// <summary>
    /// Sets the service provider for this application.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the service provider has already been set.</exception>
    public void SetServiceProvider(IServiceProvider serviceProvider)
    {
        if (_serviceProvider != null)
            throw new InvalidOperationException("The service provider has already been set.");

        _serviceProvider = serviceProvider;
    }

    public override void Initialize()
    {
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
            var logger = Services.GetRequiredService<ILogger<App>>();
            logger.LogError(e, "Failed to initialize application: {Message}", e.Message);
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
