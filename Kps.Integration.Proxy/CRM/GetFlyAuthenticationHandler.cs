using Microsoft.Extensions.Options;

namespace Kps.Integration.Proxy.CRM;

public class GetFlyAuthenticationHandler: DelegatingHandler
{
    private readonly GetFlyOptions _options;

    public GetFlyAuthenticationHandler(IOptions<GetFlyOptions> options)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
    }
    
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
        => await base.SendAsync(HandleGetTokenRequest(request), cancellationToken).ConfigureAwait(false);

    private HttpRequestMessage HandleGetTokenRequest(HttpRequestMessage request)
    {
        request.Headers.TryAddWithoutValidation("X-API-KEY", $"{_options.ApiKey}");

        return request;
    }
}