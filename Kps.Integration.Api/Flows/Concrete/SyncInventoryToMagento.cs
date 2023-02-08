using AutoMapper;
using Kps.Integration.Api.Services;
using Kps.Integration.Proxy.WMS.Models.Inventories;
using Org.BouncyCastle.Math.EC;

namespace Kps.Integration.Api.Flows.Concrete;

public class SyncInventoryToMagento : ISyncInventoryToMagento
{
    private readonly IWmsService _wmsService;
    // private readonly IMapper _mapper;
    private readonly IMagentoService _magentoService;
    private readonly ILogger<SyncOrderFromMagentoToWmsIntegrationFlow> _logger;

    private const int PageLimit = 100;

    public SyncInventoryToMagento(
        IWmsService wmsService,
        // IMapper mapper,
        IMagentoService magentoService,
        ILogger<SyncOrderFromMagentoToWmsIntegrationFlow> logger)
    {
        _wmsService = wmsService;
        // _mapper = mapper;
        _magentoService = magentoService;
        _logger = logger;
    }

    public async Task<bool> ExecuteAsync(string inputData = "KPS")
    {
        try
        {
            int pageNumber = 1;
            do
            {
                var inventoryItems = await _wmsService.GetInventoryReport(inputData, pageNumber, PageLimit);

                if (inventoryItems == null || inventoryItems.Count <= 0)
                {
                    break;
                }
                _logger.LogInformation("InventoryReport page {page}, items count {itemCount}", pageNumber, inventoryItems.Count);

                await UpdateInventoryItemsQuantity(inventoryItems);

                pageNumber++;
            } while (true);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return false;
        }
    }

    private async Task UpdateInventoryItemsQuantity(List<InventoryItem> items)
    {
        foreach (var item in items)
        {
            try
            {
                var sku = item.Item_Code + "-00";
                //var orderedQty = await _magentoService.GetOrderedQtyByOrderStatus(sku, new List<string> { "'new'", "'confirmed'", "'processing'" });
                //var expectedSaleableQty = item.Qty - orderedQty;
                var currentOrderedWithAllStatus = await _magentoService.GetOrderedQtyByOrderStatus(sku);
                //var expectedQty = expectedSaleableQty + currentOrderedWithAllStatus;
                var orderedQtyTransation = await _magentoService.GetOrderedQtyTransaction(sku);
                var expectedQty = item.Qty - orderedQtyTransation;
                _logger.LogInformation($"Expect Saleable in magento is {item.Qty} for {sku} has transaction = {orderedQtyTransation}. " +
                                       $"Ordered in magento {currentOrderedWithAllStatus} " +
                                       $"Expected qty = {expectedQty}");

                _logger.LogInformation("Update inventory {@item}", item);
                var result = await _magentoService.UpdateInventoryQuantity(sku, expectedQty);
                if (result > 0)
                {
                    _logger.LogInformation($"Inventory Updated for {sku}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while Updating inventory {@item}. {Message} - {Trace}", item, ex.Message, ex.StackTrace);
            }
        }
    }
}