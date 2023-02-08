namespace Kps.Integration.Proxy.Magento.Models.Orders;

public class UpdateOrderRequest
{
    public UpdateOrderEntity Entity { get; set; }
}

public class UpdateOrderEntity
{
    public int EntityId { get; set; }
    public string State { get; set; }
    public string Status { get; set; }
}