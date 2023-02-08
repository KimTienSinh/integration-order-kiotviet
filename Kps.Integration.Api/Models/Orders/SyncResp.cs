using System.Text.Json.Serialization;

namespace Kps.Integration.Api.Models.Orders;

public sealed class SyncResp :
    Models.Resp<Models.Orders.SyncRespData>
{
    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public override SyncRespData? Data { get; set; } = null;
}

public sealed class SyncRespData
{
    [JsonPropertyName("lastOrderId")]
    public int LastOrderId { get; set; }

    [JsonPropertyName("lastOrderOn")]
    public string LastOrderOn { get; set; }
}

