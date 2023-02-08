using System.Text.Json.Serialization;

namespace Kps.Integration.Api.Models;

public abstract class Resp<T>
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    public abstract T? Data { get; set; }
}
