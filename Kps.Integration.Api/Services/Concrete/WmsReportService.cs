using System.Text.Json;
using AutoMapper;
using Kps.Integration.Api.Infra;
using Kps.Integration.Api.Models.WmsReport;
using Kps.Integration.Proxy.Magento;
using Kps.Integration.Proxy.Magento.Models.Products;
using Kps.Integration.Proxy.WMS.Models.Orders;
using Microsoft.EntityFrameworkCore;
using Refit;

namespace Kps.Integration.Api.Services.Concrete;

public class WmsReportService : IWmsReportService
{
    private readonly KpsIntegrationContext _dbContext;
    private readonly ILogger<WmsReportService> _logger;
    private readonly IWmsService _wmsService;
    private readonly IMagentoService _magentoService;
    private readonly IMapper _mapper;
    private readonly IMagentoProxy _magentoProxy;

    public WmsReportService(KpsIntegrationContext dbContext, ILogger<WmsReportService> logger, IWmsService wmsService,
        IMagentoService magentoService,
        IMapper mapper, IMagentoProxy magentoProxy)
    {
        _dbContext = dbContext;
        _logger = logger;
        _wmsService = wmsService;
        _magentoService = magentoService;
        _mapper = mapper;
        _magentoProxy = magentoProxy;
    }

    public async Task<(List<WmsReportViewModel>, int)> Search(SearchQuery searchQuery)
    {
        // filter Date range
        var query = _dbContext.WmsSyncLog
            .Where(t => t.Updated >= searchQuery.FromDate.Date && t.Updated <= searchQuery.ToDate.AddDays(1).Date);
        if (searchQuery.Status.HasValue)
            query = query.Where(t => t.Synced == searchQuery.Status);

        var total = await query.CountAsync();

        query = query
            .OrderByDescending(t=> t.Updated)
            .Skip(searchQuery.RequestCount * (searchQuery.RequestPage - 1))
            .Take(searchQuery.RequestCount);

        var items = await query.Select(t => new WmsReportViewModel
        {
            Id = t.Id,
            Synced = t.Synced,
            Updated = t.Updated,
            OrderId = t.OrderId,
            Message = t.Message
        }).ToListAsync();

        return (items, total);
    }

    public async Task<(bool, string)> ReSync(int id)
    {
        var unSyncedOrder = _dbContext.WmsSyncLog.Where(x => x.Id == id).FirstOrDefault();
        if (unSyncedOrder == null)
        {
            return (false, "");
        }

        Order? wmsOrder;
        if (string.IsNullOrEmpty(unSyncedOrder!.Payload))
        {
            var saleOrder = await GetMagentoOrder(unSyncedOrder.OrderId).ConfigureAwait(false);
            saleOrder.Items = saleOrder.Items.Where(t => t.ParentItem == null).ToArray();

            if (saleOrder is null)
            {
                return (false, "");
            }

            wmsOrder = _mapper.Map<Order>(saleOrder);
            wmsOrder.Transaction_Detail = _mapper.Map<List<OrderItem>>(saleOrder.Items);
            // update time calculator
            var shippingTimes = await GetProductShippingTimes(saleOrder.Items);
            wmsOrder.SetEstimationShippingTime(shippingTimes, saleOrder.CreatedAt);

            unSyncedOrder.Payload = JsonSerializer.Serialize(wmsOrder);
        }
        else
        {
            wmsOrder = JsonSerializer.Deserialize<Order>(unSyncedOrder!.Payload!);
        }

        if (wmsOrder is null)
        {
            return (false, "");
        }

        var (wmsDo, message)  = await SyncToWms(wmsOrder);

        unSyncedOrder.Synced = !string.IsNullOrEmpty(wmsDo);
        unSyncedOrder.Updated = DateTime.Now;
        unSyncedOrder.Message = message;
        // update magento db
        if (!string.IsNullOrEmpty(wmsDo))
        {
            await _magentoService.UpdateSaleOrderSyncStatus(unSyncedOrder.OrderId, wmsDo);
            await _magentoService.UpdateSaleOrderProcessing(unSyncedOrder.OrderId);
        }

        _dbContext.Update(unSyncedOrder);
        await _dbContext.SaveChangesAsync();

        return (unSyncedOrder.Synced, message);
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
    
    private async Task<List<ShippingTime>> GetProductShippingTimes(Proxy.Magento.Models.Orders.OrderItem[] orderItems)
    {
        var shippingTimes =  new List<ShippingTime>();
        foreach (var item  in orderItems)
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
        var product = await _magentoProxy.GetProduct(productId);
        if (product != null)
        {
            return product.GetShippingTime();
        }
    
        return null;
    }

}