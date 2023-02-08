using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Refit;

namespace Kps.Integration.Proxy.CRM;

public sealed class GetflyProxySetup
{
    public readonly static JsonSerializerSettings JsonSerializerSetting;

    static GetflyProxySetup()
    {
        GetflyProxySetup.JsonSerializerSetting = new JsonSerializerSettings();

        GetflyProxySetup.JsonSerializerSetting.ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        };
    }

    public static IServiceCollection Add(IServiceCollection services, IConfiguration configuration)
    {
        var settings = new RefitSettings();
        settings.ContentSerializer = new NewtonsoftJsonContentSerializer(GetflyProxySetup.JsonSerializerSetting);

        var sEndpoint = configuration.GetValue<string>("Proxy:Getfly:Endpoint");

        var baseAddress = new Uri(sEndpoint);

        var mediaType =
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json");

        var sApiKey = configuration.GetValue<string>("Proxy:Getfly:ApiKey");

        services
            .AddRefitClient<IGetflyProxy>(settings)
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = baseAddress;

                c.DefaultRequestHeaders.Accept.Add(mediaType);

                c.DefaultRequestHeaders.Add("X-API-KEY", sApiKey);
            });

        return services;
    }
}
