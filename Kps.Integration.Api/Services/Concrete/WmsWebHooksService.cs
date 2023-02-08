using AutoMapper;
using Kps.Integration.Api.Models.Orders;
using Kps.Integration.Proxy.Magento;
using Kps.Integration.Proxy.Magento.Models.Orders;
using Refit;

namespace Kps.Integration.Api.Services.Concrete;

public class WmsWebHooksService : IWmsWebHooksService
{
    private readonly IMagentoProxy _magentoProxy;
    private readonly IMapper _mapper;
    private readonly ILogger<WmsWebHooksService> _logger;

    public WmsWebHooksService(
        IMagentoProxy magentoProxy,
        IMapper _mapper,
        ILogger<WmsWebHooksService> _logger)
    {
        _magentoProxy = magentoProxy;
        this._mapper = _mapper;
        this._logger = _logger;
    }

    public async Task Handler(WmsOrderStatusNotificationPayload payload, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Webhook | Receive WmsOrderStatusNotificationPayload: {@payload}", payload);
            var updateOrderRequest = new UpdateOrderRequest();
            updateOrderRequest.Entity = _mapper.Map<UpdateOrderEntity>(payload.Entity);
            await _magentoProxy.UpdateOrder(updateOrderRequest, cancellationToken);
            _logger.LogInformation("Webhook | Order Status updated for {entityId}", updateOrderRequest.Entity.EntityId);
        }
        catch (ApiException ex)
        {
            _logger.LogError("Webhook | Fail to process WmsOrderStatusNotificationPayload: {@payload}", payload);
        }
    }
}