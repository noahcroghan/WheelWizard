using Refit;
using System.Text.Json;
using WheelWizard.Services;
using WheelWizard.WheelWizardData.Domain;

namespace WheelWizard.WheelWizardData;

public static class WhWzDataExtensions
{
    public static IServiceCollection AddWhWzData(this IServiceCollection services)
    {
        services.AddRefitClient<IWhWzDataApi>(new()
        {
            ContentSerializer = new SystemTextJsonContentSerializer(new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                Converters = { new BadgeVariantConverter() }
            })
        }).ConfigureHttpClient((sp, client) =>
        {
            client.ConfigureWheelWizardClient(sp);

            client.BaseAddress = new(Endpoints.WhWzDataBaseAddress);
        }).AddStandardResilienceHandler();

        services.AddSingleton<IWhWzDataSingletonService, WhWzDataSingletonService>();

        return services;
    }
}

