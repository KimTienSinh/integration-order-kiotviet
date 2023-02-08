using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kps.Integration.Proxy.WMS;

public static class WmsProxySetup
{
    public static IServiceCollection AddWmsProxies(this IServiceCollection services, 
        IConfiguration configuration)
    {
        var apiServer = configuration.GetValue<string>("Proxy:WMS:ApiServer");

        services.Configure<WmsOptions>(configuration.GetSection("Proxy:WMS"));

        services.AddHttpClient<IWmsProxy, WmsProxy>(c =>
        {
            c.BaseAddress = new Uri(apiServer);
        });
        return services;
    }
}