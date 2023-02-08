namespace Kps.Integration.Proxy.WMS.Models.Authen;

public class AuthenticationRequest : WmsRequestBase
{
    public  AuthenticationRequestData Data { get; set; }
}

public class AuthenticationRequestData
{
    public string Ma_Dang_Nhap { get; set; }
    public string Mat_Khau { get; set; }
}