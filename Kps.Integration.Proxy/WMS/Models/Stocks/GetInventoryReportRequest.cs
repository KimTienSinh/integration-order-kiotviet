namespace Kps.Integration.Proxy.WMS.Models.Inventories;

public class GetInventoryReportRequest : WmsRequestBase
{
    public GetInventoryReportQuery Data { get; set; }
}

public  class GetInventoryReportQuery
{
    public string Owner_Code { get; set; }
    public int Page_Num { get; set; } = 1;
    public int Page_Limit { get; set; } = 100;
}