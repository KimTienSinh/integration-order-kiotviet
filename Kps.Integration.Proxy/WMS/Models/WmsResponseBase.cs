namespace Kps.Integration.Proxy.WMS.Models;

public abstract class WmsResponseBase
{
    public MessageResponse Message { get; set; }
}

public class MessageResponse
{
    public int Message_Code { get; set; }
    public string Message_Desc { get; set; }
}