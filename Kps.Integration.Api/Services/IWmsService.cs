using Kps.Integration.Proxy.WMS.Models.Inventories;
using Kps.Integration.Proxy.WMS.Models.Orders;

namespace Kps.Integration.Api.Services;

public interface IWmsService
{
    Task<string> CreateOrder(Order order);
    Task<List<InventoryItem>?> GetInventoryReport(string ownerCode, int pageNum = 1, int pageLimit = 100);
}