using Newtonsoft.Json;

namespace Kps.Integration.Proxy.CRM.Models.Orders;

public sealed class CreateOrderReq
{
    [JsonIgnore]
    public int MagentoOrderId { get; set; }

    public Order OrderInfo { get; set; }

    public OrderItem[] Products { get; set; }

    public string[] Terms { get; set; }
}

public sealed class CreateOrderResp
{
    [JsonIgnore]
    public bool HasExisted { get; set; }

    [JsonIgnore]
    public int MagentoOrderId { get; set; }

    public int OrderId { get; set; }

    [JsonIgnore]
    public CreateOrderReq RequestData { get; set; }

    [JsonIgnore]
    public string RequestBody { get; set; }
}
