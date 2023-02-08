namespace Kps.Integration.Api.Models.WmsReport;

public class WmsReportViewModel
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public bool Synced { get; set; }
    public DateTime Updated { get; set; }
    public string? Message { get; set; }
}