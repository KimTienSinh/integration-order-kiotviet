using Newtonsoft.Json;

namespace Kps.Integration.Proxy.CRM.Models.Orders;

public sealed class Order
{
    [JsonIgnore]
    public bool HasAccountEmail { get; set; }

    [JsonIgnore]
    public bool HasAccountPhone { get; set; }

    [JsonIgnore]
    public bool HasMappedAccount { get; set; }

    public decimal Amount { get; set; }

    public decimal Discount { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal Installation { get; set; }

    public decimal InstallationAmount { get; set; }

    public decimal Transport { get; set; }

    public decimal TransportAmount { get; set; }

    public decimal Vat { get; set; }

    public decimal VatAmount { get; set; }

    [JsonIgnore]
    public int AccountId { get; set; }

    [JsonIgnore]
    public int CustomerId { get; set; }

    [JsonIgnore]
    public int Id { get; set; }

    public string AccountAddress { get; set; }

    public string AccountCode { get; set; }

    public string AccountEmail { get; set; }

    public string AccountName { get; set; }

    public string AccountPhone { get; set; }

    public string OrderCode { get; set; }

    public string OrderDate { get; set; }

    public string SourceName { get; set; }
}

public sealed class OrderItem
{
    public decimal Price { get; set; }

    public decimal ProductSaleOff { get; set; }

    public decimal Vat { get; set; }

    [JsonIgnore]
    public int MagentoOrderItemId { get; set; }

    [JsonIgnore]
    public int MagentoProductId { get; set; }

    [JsonIgnore]
    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public string CashDiscount { get; set; }

    public string ProductCode { get; set; }
}
