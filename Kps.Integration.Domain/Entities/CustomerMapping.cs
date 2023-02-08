using System.ComponentModel.DataAnnotations;

namespace Kps.Integration.Domain.Entities;

public sealed class CustomerMapping
{
    public int CustomerId { get; set; }

    [Key]
    public int Id { get; set; }

    public int? GetflyCustomerId { get; set; }

    public string? CustomerEmail { get; set; }

    public string? CustomerPhone { get; set; }

    public string GetflyCustomerCode { get; set; }

    public DateTime? CreatedOn { get; set; } = System.DateTime.Now;
}
