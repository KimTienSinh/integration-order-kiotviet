namespace Kps.Integration.Proxy.CRM.Constants;

public sealed class ResultCode
{
    public const int
        Succeed = 200,
        CreateSucceed = 201,
        BadRequest = 400,
        Unauthorized = 401,
        Existed = 402,
        Forbidden = 403,
        NotFound = 404,
        InternalError = 500;

    public static string GetDescription(int code)
    {
        switch (code)
        {
            case ResultCode.Succeed:
                return "Thành công";
            case ResultCode.CreateSucceed:
                return "Thêm mới thành công";
            case ResultCode.BadRequest:
                return "Bad request";
            case ResultCode.Unauthorized:
                return "Không xác thực yêu cầu, có thể do cung cấp API Key không hợp lệ";
            case ResultCode.Existed:
                return "Trùng email với khách hàng đã tồn tại";
            case ResultCode.Forbidden:
                return "Forbidden - Request Method not accepted";
            case ResultCode.NotFound:
                return "Not found - Không tìm thấy tài nguyên yêu cầu";
            case ResultCode.InternalError:
                return "Internal Server Error";
        }

        return string.Empty;
    }
}
