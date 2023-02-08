using Newtonsoft.Json;

namespace Kps.Integration.Proxy.CRM.Models.Products;

public sealed class Product
{
    [JsonIgnore]
    public int MagentoProductId { get; set; }

    public int ProductId { get; set; }

    public string ProductCode { get; set; }

    public string ProductName { get; set; }
}
