using System.Text.Json;
using AutoMapper;
using Kps.Integration.Api.Infra;
using Kps.Integration.Api.Services;
using Kps.Integration.Api.Services.Models.Magento;
using Kps.Integration.Domain.Constants;
using Kps.Integration.Domain.Entities;
using Kps.Integration.Proxy.Magento;
using Kps.Integration.Proxy.Magento.Models.Products;
using Microsoft.EntityFrameworkCore;
using Refit;
using Order = Kps.Integration.Proxy.WMS.Models.Orders.Order;
using OrderItem = Kps.Integration.Proxy.WMS.Models.Orders.OrderItem;

namespace Kps.Integration.Api.Flows.Concrete;

public class SyncOrderFromMagentoToWmsIntegrationFlow : ISyncOrderFromMagentoToWmsIntegrationFlow
{
    private readonly ILogger<SyncOrderFromMagentoToWmsIntegrationFlow> _logger;

    private readonly IMagentoService _magentoService;
    private readonly IWmsService _wmsService;
    private readonly IMapper _mapper;
    private readonly IMagentoProxy _magentoProxy;
    private readonly KpsIntegrationContext _dbContext;

    public SyncOrderFromMagentoToWmsIntegrationFlow(
        ILogger<SyncOrderFromMagentoToWmsIntegrationFlow> logger,
        IMagentoService magentoService,
        IWmsService wmsService,
        IMapper mapper,
        IMagentoProxy magentoProxy,
        KpsIntegrationContext dbContext)
    {
        _logger = logger;
        _magentoService = magentoService;
        _wmsService = wmsService;
        _mapper = mapper;
        _magentoProxy = magentoProxy;
        _dbContext = dbContext;
    }

    public async Task<bool> ExecuteAsync(int inputData)
    {
        var unSyncedOrders = await GetUnSyncedOrder(inputData);

        foreach (var unSyncedOrder in unSyncedOrders.ToList())
        {
            var wmsSyncLog = await _dbContext.WmsSyncLog
                .FirstOrDefaultAsync(t => t.OrderId == unSyncedOrder.EntityId)
                .ConfigureAwait(false);

            if (wmsSyncLog == null)
            {
                wmsSyncLog = new WmsSyncLog()
                {
                    OrderId = unSyncedOrder.EntityId,
                    Synced = false,
                    Updated = DateTime.Now
                };
                await _dbContext.WmsSyncLog.AddAsync(wmsSyncLog).ConfigureAwait(false);
                await _dbContext.SaveChangesAsync().ConfigureAwait(false);
            }

            if (wmsSyncLog.Synced == true)
            {
                continue;
            }

            var saleOrder = await GetMagentoOrder(unSyncedOrder.EntityId).ConfigureAwait(false);
            saleOrder.Items = saleOrder.Items.Where(t => t.ParentItem == null).ToArray();

            if (saleOrder is null) continue;

            var wmsOrder = _mapper.Map<Order>(saleOrder);
            wmsOrder.Transaction_Detail = _mapper.Map<List<OrderItem>>(saleOrder.Items);

            // update time calculator
            var shippingTimes = await GetProductShippingTimes(saleOrder.Items);
            wmsOrder.SetEstimationShippingTime(shippingTimes, saleOrder.CreatedAt);

            var (wmsDo, message) = await SyncToWms(wmsOrder);
            wmsSyncLog.Synced = !string.IsNullOrEmpty(wmsDo);
            wmsSyncLog.Message = message;
            wmsSyncLog.Updated = DateTime.Now;

            wmsSyncLog.Payload = JsonSerializer.Serialize(wmsOrder);
            _dbContext.WmsSyncLog.Update(wmsSyncLog);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);

            // update magento db
            if (!string.IsNullOrEmpty(wmsDo))
            {
                await _magentoService.UpdateSaleOrderSyncStatus(unSyncedOrder.EntityId, wmsDo);
                await _magentoService.UpdateSaleOrderProcessing(unSyncedOrder.EntityId);
            }

        }

        var lastProcessedTime = unSyncedOrders.Length == 0 ? DateTime.Now : unSyncedOrders.Last().CreatedAt;
        await UpdateLastProcessedTime(lastProcessedTime);

        return true;
    }

    private async Task<Proxy.Magento.Models.Orders.Order?> GetMagentoOrder(int id)
    {
        try
        {
            var saleOrders = await _magentoProxy.GetOrder(id);
            return saleOrders;
        }
        catch (ApiException ex)
        {
            _logger.LogError("Có lỗi khi lấy Order từ Magento API. ItemId: {id}. {@ex}", id, ex);
            return null;
        }
    }

    private async Task<(string?, string)> SyncToWms(Order order)
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
            _logger.LogError("Sync Order from Magento to WMS failed. ItemId: {No}. {@ex}", order.DO_No, ex);
            return (null, ex.Message);
        }
    }

    private async Task<SalesOrder[]> GetUnSyncedOrder(int limit)
    {
        var scheduleLogging = await _dbContext.ScheduleLogging
            .FirstOrDefaultAsync(x => x.ApplicationName == ApplicationName.WMS);

        var lastProcessedTime = scheduleLogging?.LastOrderTime;
        return await _magentoService.GetOrderForWms(lastProcessedTime, limit);
    }

    private async Task UpdateLastProcessedTime(DateTime lastProcessedTime)
    {
        var scheduleLogging = new ScheduleLogging
        {
            CreatedOn = System.DateTime.Now,
            LastOrderTime = lastProcessedTime,
            ApplicationName = ApplicationName.WMS
        };

        _dbContext.ScheduleLogging.Add(scheduleLogging);
        await _dbContext.SaveChangesAsync();
    }

    private async Task<List<ShippingTime>> GetProductShippingTimes(Proxy.Magento.Models.Orders.OrderItem[] orderItems)
    {
        var shippingTimes = new List<ShippingTime>();
        foreach (var item in orderItems)
        {
            var shippingTime = await GetProductShippingTime(item.ProductId);
            shippingTime.Sku = item.Sku;
            if (shippingTime != null)
            {
                shippingTimes.Add(shippingTime);
            }
        }

        return shippingTimes;
    }

    private async Task<ShippingTime?> GetProductShippingTime(int productId)
    {
        try
        {
            var product = await _magentoProxy.GetProduct(productId);
            if (product != null)
            {
                return product.GetShippingTime();
            }
        }
        catch (ApiException ex)
        {
            _logger.LogError($"Cannot get product {productId} in magento. {ex.StatusCode}");
        }

        return null;
    }
}