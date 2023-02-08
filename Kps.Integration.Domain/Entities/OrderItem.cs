using System.ComponentModel.DataAnnotations;

namespace Kps.Integration.Domain.Entities;

public sealed class OrderItem
{
    [Key]
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int OrderItemId { get; set; }

    public int ProductId { get; set; }

    public int? GetflyProductId { get; set; }

    public string? GetflyProductCode { get; set; }

    public DateTime? CreatedOn { get; set; } = System.DateTime.Now;
}
