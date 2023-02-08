using System.ComponentModel.DataAnnotations;

namespace Kps.Integration.Domain.Entities;

public sealed class Order
{
    public byte[]? GetflyRequestBody { get; set; }

    public byte[]? MagentoPayload { get; set; }

    public DateTime CreatedOn { get; set; } = System.DateTime.Now;

    public DateTime? LastRetriedOn { get; set; }

    [Key]
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int RetryCount { get; set; } = 0;

    public int? CustomerId { get; set; }

    public int? GetflyOrderId { get; set; }

    public string? GetflyCustomerCode { get; set; }

    public string? RetriedBy { get; set; }

    public string Status { get; set; }

    public DateTime? OrderCreatedOn { get; set; }

    public DateTime? SyncedOn { get; set; }
}
