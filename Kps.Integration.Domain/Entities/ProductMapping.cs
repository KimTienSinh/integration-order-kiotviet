using System.ComponentModel.DataAnnotations;

namespace Kps.Integration.Domain.Entities;

public sealed class ProductMapping
{
    [Key]
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int GetflyProductId { get; set; }

    public string GetflyProductCode { get; set; }

    public DateTime? CreatedOn { get; set; } = System.DateTime.Now;
}
