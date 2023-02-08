using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Refit;

namespace Kps.Integration.Proxy.Magento;

public sealed class MagentoProxySetup
{
    public static IServiceCollection Add(IServiceCollection services, IConfiguration configuration)
    {
        var snakeCaseResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        };

        var jsonSerialSettings = new JsonSerializerSettings
        {
            ContractResolver = snakeCaseResolver
        };

        var settings = new RefitSettings();
        settings.ContentSerializer = new NewtonsoftJsonContentSerializer(jsonSerialSettings);

        var sEndpoint = configuration.GetValue<string>("Proxy:Magento:Endpoint");

        var baseAddress = new Uri(sEndpoint);

        var mediaType =
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json");

        string sApiKey;
        sApiKey = configuration.GetValue<string>("Proxy:Magento:ApiKey");
        sApiKey = $"Bearer {sApiKey}";

        services
            .AddRefitClient<IMagentoProxy>(settings)
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = baseAddress;

                c.DefaultRequestHeaders.Accept.Add(mediaType);

                c.DefaultRequestHeaders.Add("Authorization", sApiKey);
            });

        return services;
    }
}
