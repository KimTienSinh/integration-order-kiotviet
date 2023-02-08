using System.Text.Json;
using AutoMapper;
using Kps.Integration.Api.Infra;
using Kps.Integration.Api.Services;
using Kps.Integration.Domain.Constants;
using Kps.Integration.Domain.Entities;
using Kps.Integration.Proxy.Magento;
using Order = Kps.Integration.Proxy.WMS.Models.Orders.Order;

namespace Kps.Integration.Api.Flows.Concrete;

public class ReSyncOrderFromMagentoToWmsIntegrationFlow : IReSyncOrderFromMagentoToWmsIntegrationFlow
{
    private readonly IWmsService _wmsService;
    private readonly IMagentoService _magentoService;
    private readonly KpsIntegrationContext _dbContext;
    private readonly ILogger<ReSyncOrderFromMagentoToWmsIntegrationFlow> _logger;

    public ReSyncOrderFromMagentoToWmsIntegrationFlow(
        IWmsService wmsService,
        IMagentoService magentoService,
        KpsIntegrationContext dbContext,
        ILogger<ReSyncOrderFromMagentoToWmsIntegrationFlow> logger)
    {
        _wmsService = wmsService;
        _magentoService = magentoService;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<bool> ExecuteAsync(int inputData)
    {
        var unSyncedOrders = _dbContext.WmsSyncLog.Where(x => x.Synced == false)
            .OrderBy(x => x.Updated).Take(inputData).ToList();

        foreach (var unSyncedOrder in unSyncedOrders)
        {
            if (string.IsNullOrEmpty(unSyncedOrder.Payload)) continue;
            var wmsOrder = JsonSerializer.Deserialize<Order>(unSyncedOrder.Payload!);
            if (wmsOrder is not null)
            {
                var (wmsDo, message) = await SyncToWms(wmsOrder);
                unSyncedOrder.Synced = string.IsNullOrEmpty(wmsDo);
                unSyncedOrder.Updated = DateTime.Now;
                unSyncedOrder.Message = message;
                _dbContext.Update(unSyncedOrder);
                // update magento db
                if (!string.IsNullOrEmpty(wmsDo))
                {
                    await _magentoService.UpdateSaleOrderSyncStatus(unSyncedOrder.OrderId, wmsDo);
                    await _magentoService.UpdateSaleOrderProcessing(unSyncedOrder.OrderId);
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        return true;
    }

    private async Task<(string, string)> SyncToWms(Order order)
    {
        try
        {
            _logger.LogInformation("Sync Order from Magento to WMS. ItemId: {No}. {@order}", order.DO_No, order);
            var wmsDo = await _wmsService.CreateOrder(order);
            _logger.LogInformation($"Sync Order from Magento to WMS succeed. ItemId: {order.DO_No}");

            return (wmsDo, string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError("Sync Order from Magento to WMS failed. ItemId: {No}. {@ex}",order.DO_No, ex );
            return (null, ex.Message);
        }
    }
}