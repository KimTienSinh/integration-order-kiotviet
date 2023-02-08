using Newtonsoft.Json;

namespace Kps.Integration.Proxy.CRM.Models.Products;

public sealed class CreateProductReq
{
    public string ProductCode { get; set; }

    public string ProductName { get; set; }
}

public sealed class CreateProductResp
{
    [JsonIgnore]
    public int MagenoProductId { get; set; }

    public int ProductId { get; set; }

    public string Message { get; set; }

    [JsonIgnore]
    public string ProductCode { get; set; }
}
