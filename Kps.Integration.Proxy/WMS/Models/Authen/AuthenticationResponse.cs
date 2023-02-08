namespace Kps.Integration.Proxy.WMS.Models.Authen;

public class AuthenticationResponse: WmsResponseBase
{
    public AuthenticationResponseData Data { get; set; }
}

public class AuthenticationResponseData
{
    public int Auto_ID { get; set; }
    public string Token { get; set; }
}
