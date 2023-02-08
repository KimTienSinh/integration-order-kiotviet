using System.Text.Json.Serialization;

namespace Kps.Integration.Api.Models.Reports;

public sealed class ReSyncResp :
    Models.Resp<bool>
{
    [JsonPropertyName("data")]
    public override bool Data { get; set; }
}
