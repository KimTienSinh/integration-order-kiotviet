using System.Text;
using Microsoft.Extensions.Options;

namespace Kps.Integration.Proxy.Magento;

public class MagentoAuthenticationHandler: DelegatingHandler
{
    private readonly MagentoOptions _options;

    public MagentoAuthenticationHandler(IOptions<MagentoOptions> options)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
    }
    
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
        => await base.SendAsync(HandleGetTokenRequest(request), cancellationToken).ConfigureAwait(false);

    private HttpRequestMessage HandleGetTokenRequest(HttpRequestMessage request)
    {
        request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {_options.ApiKey}");

        return request;
    }
}