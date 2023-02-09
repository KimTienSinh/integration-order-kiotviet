using Kps.Integration.Api.Flows;
using Kps.Integration.Api.Flows.Concrete;
using Kps.Integration.Api.Models.Options;
using Kps.Integration.Api.Services;
using Kps.Integration.Api.Services.Concrete;

namespace Kps.Integration.Api.Configs;

public static class AppSetup
{
    public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ApiSecretOptions>(configuration.GetSection("ApiSecretConfig"));

        services.AddTransient<IMagentoService>(
            p =>
            {
                var sConnectionString = configuration.GetConnectionString("Magento");

                return new MagentoService(sConnectionString);
            }
        );

        services.AddTransient<ISyncMagentoOrderToCrmFlow, SyncMagentoOrderToCrmFlow>();
        services.AddTransient<ISyncOrderFromMagentoToWmsIntegrationFlow, SyncOrderFromMagentoToWmsIntegrationFlow>();
        services.AddTransient<IReSyncOrderFromMagentoToWmsIntegrationFlow, ReSyncOrderFromMagentoToWmsIntegrationFlow>();
        services.AddTransient<ISyncInventoryToMagento, SyncInventoryToMagento>();

        services.AddTransient<IWmsService, WmsService>();

        services.AddScoped<ICrmOrderFlow, CrmOrderFlow>();
        services.AddScoped<IWmsReportService, WmsReportService>();
        services.AddScoped<IWmsWebHooksService, WmsWebHooksService>();
        
        return services;
    }
}
