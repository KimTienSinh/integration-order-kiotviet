using System.ComponentModel.DataAnnotations;

namespace Kps.Integration.Domain.Entities;

public class WmsSyncLog
{
    [Key]
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string? Payload { get; set; }
    public bool Synced { get; set; }
    public DateTime Updated { get; set; }
    public string? Message { get; set; }
}