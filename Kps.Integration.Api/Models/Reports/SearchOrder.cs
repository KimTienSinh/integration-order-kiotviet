using System.Text.Json.Serialization;

namespace Kps.Integration.Api.Models.Reports;

public sealed class SearchReq
{
    [JsonPropertyName("requestCount")]
    public int RequestCount { get; set; }

    [JsonPropertyName("requestPage")]
    public int RequestPage { get; set; }

    [JsonPropertyName("fromDate")]
    public string? FromDate { get; set; }

    [JsonPropertyName("toDate")]
    public string? ToDate { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

public sealed class SearchResp :
    Models.Resp<Models.Reports.SearchRespData>
{
    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public override SearchRespData? Data { get; set; } = null;
}

public sealed class SearchRespData
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("records")]
    public SearchRespItem[] Records { get; set; }
}

public sealed class SearchRespItem
{
    [JsonPropertyName("canReSync")]
    public bool CanReSync { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("orderId")]
    public int OrderId { get; set; }

    [JsonPropertyName("retryCount")]
    public int RetryCount { get; set; }

    [JsonPropertyName("getflyOrderId")]
    public int? GetflyOrderId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("createdOn")]
    public DateTime CreatedOn { get; set; }

    [JsonPropertyName("orderCreatedOn")]
    public DateTime OrderCreatedOn { get; set; }

    [JsonPropertyName("lastRetriedOn")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? LastRetriedOn { get; set; }

    [JsonPropertyName("syncedOn")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? SyncedOn { get; set; }
}
