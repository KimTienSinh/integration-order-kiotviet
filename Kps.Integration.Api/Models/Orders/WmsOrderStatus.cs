namespace Kps.Integration.Api.Models.Orders;

public class WmsOrderStatusNotificationPayload
{
    public OrderEntityNotification Entity { get; set; }
}

public class OrderEntityNotification
{
    public int Entity_Id { get; set; }
    public string State { get; set; }
    public string Status { get; set; }
}