using Kps.Integration.Api.Models.Inventories;
using Kps.Integration.Proxy.Magento.Models.Orders;

namespace Kps.Integration.Api.Services
{
    using Kps.Integration.Api.Services.Models.Magento;

    public interface IMagentoService
    {
        Task<SalesOrder[]> GetOrderList(DateTime? lastOrderOn = null, int limit = 100);
        Task<int> UpdateInventoryQuantity(string sku, double quantity);
        Task<int> AddInventory(UpdateInventoryParams request);
        Task<int> SubtractInventory(UpdateInventoryParams request);
        Task<double> GetOrderedQtyByOrderStatus(string sku, List<string>? statuses = null);
        Task<double> GetOrderedQtyTransaction(string sku, List<string>? statuses = null);
        Task<SalesOrder[]> GetOrderForWms(DateTime? lastOrderOn = null, int limit = 50);
        Task UpdateSaleOrderSyncStatus(int entityId, string wmsDo);
        Task UpdateSaleOrderProcessing(int entityId);
    }
}
