using System.ComponentModel.DataAnnotations;

namespace Kps.Integration.Domain.Entities;

public sealed class ScheduleLogging
{
    public DateTime CreatedOn { get; set; }

    public DateTime LastOrderTime { get; set; }
    public string ApplicationName { get; set; }

    [Key]
    public int Id { get; set; }
}
