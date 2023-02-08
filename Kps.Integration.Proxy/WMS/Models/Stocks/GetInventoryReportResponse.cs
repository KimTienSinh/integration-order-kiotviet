namespace Kps.Integration.Proxy.WMS.Models.Inventories;

public class GetInventoryReportResponse : WmsResponseBase
{
    public GetInventoryReportResponse()
    {
        this.Data = new List<InventoryItem>();
    }
    public List<InventoryItem> Data { get; set; }
}

public class InventoryItem
{
    public string Item_Code { get; set; }
    public string Item_Name { get; set; }
    public string Lot_No { get; set; }
    public double Qty { get; set; }
}