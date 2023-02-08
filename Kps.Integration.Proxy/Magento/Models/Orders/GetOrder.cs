namespace Kps.Integration.Proxy.Magento.Models.Orders;

public sealed class GetOrderResp
{
    public int OrderId { get; set; }

    public Order Order { get; set; }

    public string ResponsePayload { get; set; }
}

public sealed class Order
{
    public decimal DiscountAmount { get; set; }

    public decimal? GrandTotal { get; set; }

    public decimal ShippingAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public ExtensionAttribute ExtensionAttributes { get; set; }

    public int EntityId { get; set; }

    public int CustomerId { get; set; }

    public OrderItem[] Items { get; set; }

    public DateTime CreatedAt { get; set; }

    public string CustomerEmail { get; set; }

    public string CustomerFirstname { get; set; }
    public string CustomerLastname { get; set; }
    public  string IncrementId { get; set; }
    public  Payment Payment { get; set; }
    public List<StatusHistory> StatusHistories { get; set; }

}

public sealed class OrderItem
{
    public string Sku { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }

    public decimal TaxPercent { get; set; }

    public int ItemId { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public int QtyOrdered { get; set; }
    public int DiscountAmount { get; set; }
    public OrderItemParentItem? ParentItem { get; set; }
}

public sealed class ExtensionAttribute
{
    public ShippingAssignment[] ShippingAssignments { get; set; }
}

public sealed class ShippingAssignment
{
    public Shipping Shipping { get; set; }
}

public sealed class Shipping
{
    public ShippingAddress Address { get; set; }
}

public sealed class ShippingAddress
{
    public string Email { get; set; }

    public string Firstname { get; set; }

    public string Lastname { get; set; }

    public string Telephone { get; set; }
    public string City { get; set; }
    public string District { get; set; }

    public string[] Street { get; set; }
    public string Ward { get; set; }
}

public class Payment
{
    // public string AccountStatus { get; set; }
    // public string[] AdditionalInformation { get; set; }
    // public double AmountOrdered { get; set; }
// public double base_amount_ordered{ get; set; }
// public double base_shipping_amount{ get; set; }
// public string cc_exp_year{ get; set; }
// public string cc_last4{ get; set; }
// public string cc_ss_start_month{ get; set; }
// public string cc_ss_start_year{ get; set; }
// public int entity_id{ get; set; }
    public string Method { get; set; }
// public int parent_id{ get; set; }
// public double shipping_amount{ get; set; }
}

public class StatusHistory
{
    public string Comment { get; set; }
    public string Status { get; set; }
    public int EntityId { get; set; }
    public DateTime CreatedAt { get; set; }

    public DateTime GetCreatedAtLocalDateTime()
    {
        var vnTimeZone = TimeZoneInfo.GetSystemTimeZones().Any(x => x.Id == "SE Asia Standard Time")
            ? TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
            : TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
        
        return TimeZoneInfo.ConvertTimeFromUtc(CreatedAt, vnTimeZone);
    }
}

public class OrderItemParentItem
{
    public string Sku { get; set; }
    public string Name { get; set; }
}