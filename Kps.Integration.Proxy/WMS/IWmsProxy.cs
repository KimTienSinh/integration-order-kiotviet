using Kps.Integration.Proxy.WMS.Models.Authen;
using Kps.Integration.Proxy.WMS.Models.Inventories;
using Kps.Integration.Proxy.WMS.Models.Orders;

namespace Kps.Integration.Proxy.WMS;

public interface IWmsProxy
{
    Task<string> Login(int type, int prototypeId);
    Task<string> CreateOrder(string token, Order order);

    /// <summary>
    /// Only one request within 30mins
    /// </summary>
    /// <returns></returns>
    Task<List<InventoryItem>> GetInventoryReport(string token, string ownerCode, int pageNum = 1, int pageLimit = 100);
}