using Kps.Integration.Proxy.CRM;
using Kps.Integration.Proxy.Magento;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Kps.Integration.Proxy.WMS;
namespace Kps.Integration.Proxy
{
    public static class ProxySetup
    {

    public static IServiceCollection AddProxies(this IServiceCollection services, IConfiguration configuration)
    {
        CRM.GetflyProxySetup.Add(services, configuration);

        Magento.MagentoProxySetup.Add(services, configuration);
        services.AddWmsProxies(configuration);
            return services;
        }
    }
}
