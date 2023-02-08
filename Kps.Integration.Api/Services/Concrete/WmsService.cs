using Kps.Integration.Proxy.WMS;
using Kps.Integration.Proxy.WMS.Models.Authen;
using Kps.Integration.Proxy.WMS.Models.Inventories;
using Kps.Integration.Proxy.WMS.Models.Orders;
using Microsoft.Extensions.Options;

namespace Kps.Integration.Api.Services.Concrete;

public class WmsService : IWmsService
{
    private readonly IWmsProxy _wmsProxy;
    private readonly ILogger<WmsService> _logger;

    public WmsService(IWmsProxy wmsProxy, ILogger<WmsService> logger)
    {
        _wmsProxy = wmsProxy;
        _logger = logger;
    }

    public async Task<string> CreateOrder(Order order)
    {
        var token = await _wmsProxy.Login(100, 2);
        return await _wmsProxy.CreateOrder(token, order);
    }

    public async Task<List<InventoryItem>?> GetInventoryReport(string ownerCode, int pageNum = 1, int pageLimit = 100)
    {
        try
        {
            var token = await _wmsProxy.Login(100, 2);

            return await _wmsProxy.GetInventoryReport(token, ownerCode, pageNum, pageLimit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return null;
        }
    }
}