namespace Kps.Integration.Proxy.WMS.Models;

public abstract class WmsRequestBase
{
    public WmsAuth Auth { get; set; }
}

public class WmsAuth
{
    public string Token { get; set; }
    public int Type_ID { get; set; }
    public int Prototype_ID { get; set; }
    public string Device_ID { get; set; }
}
