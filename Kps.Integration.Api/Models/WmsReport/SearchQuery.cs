namespace Kps.Integration.Api.Models.WmsReport;

public class SearchQuery
{
    public int RequestCount { get; set; } = 10;

    public int RequestPage { get; set; } = 1;

    public DateTime FromDate { get; set; } = DateTime.Now;

    public DateTime ToDate { get; set; } = DateTime.Now;

    public bool? Status { get; set; }
}