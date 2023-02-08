using System.Linq.Expressions;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Kps.Integration.Proxy.WMS.Models;
using Kps.Integration.Proxy.WMS.Models.Authen;
using Kps.Integration.Proxy.WMS.Models.Inventories;
using Kps.Integration.Proxy.WMS.Models.Orders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kps.Integration.Proxy.WMS;

public class WmsProxy : IWmsProxy
{
    private readonly WmsOptions _options;
    private readonly HttpClient _httpClient;
    private readonly ILogger<WmsProxy> _logger;

    public WmsProxy(IOptions<WmsOptions> options, HttpClient httpClient, ILogger<WmsProxy> logger)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> Login(int type, int prototypeId)
    {
        AuthenticationRequest loginRequest = new AuthenticationRequest()
        {
            Data = new AuthenticationRequestData()
            {
                Mat_Khau = _options.LoginSecret,
                Ma_Dang_Nhap = _options.LoginId
            },
            Auth = new WmsAuth()
            {
                Token = "",
                Device_ID = "",
                Prototype_ID = prototypeId,
                Type_ID = type
            }
        };
        _logger.LogInformation($"Start login to wms with user {_options.LoginId} {_options.LoginSecret}, {JsonSerializer.Serialize(loginRequest)}");

        var payload = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            MediaTypeNames.Application.Json);

        var response = await _httpClient.PostAsync("", payload);
        response.EnsureSuccessStatusCode();

        var authResponse = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();

        if (authResponse == null) throw new Exception("Có lỗi trong quá trình xử lý");

        if (authResponse.Message.Message_Code == 200)
        {
            return authResponse.Data.Token;
        }

        throw new Exception(authResponse.Message.Message_Desc);
    }

    public async Task<string> CreateOrder(string token, Order order)
    {
        var orderRequest = new CreateOrderRequest()
        {
            Auth = new WmsAuth()
            {
                Token = token,
                Device_ID = "",
                Prototype_ID = 2, // 2: adding
                Type_ID = 410 // for order
            },
            Data = order
        };

        var payload = new StringContent(
            JsonSerializer.Serialize(orderRequest),
            Encoding.UTF8,
            MediaTypeNames.Application.Json);

        var response = await _httpClient.PostAsync("", payload);
        response.EnsureSuccessStatusCode();

        var createOrderResponse = await response.Content.ReadFromJsonAsync<CreateOrderResponse>();

        if (createOrderResponse == null) throw new Exception("Có lỗi trong quá trình xử lý");

        if (createOrderResponse.Message.Message_Code != 200)
        {
            throw new Exception($"{createOrderResponse.Message.Message_Code} - {createOrderResponse.Message.Message_Desc}");
        }

        return createOrderResponse.Data;
    }

    public async Task<List<InventoryItem>> GetInventoryReport(string token, string ownerCode, int pageNum = 1, int pageLimit = 100)
    {
        var getReportRequest = new GetInventoryReportRequest()
        {
            Auth = new WmsAuth()
            {
                Token = token,
                Device_ID = "",
                Prototype_ID = 1,
                Type_ID = 903
            },
            Data = new GetInventoryReportQuery()
            {
                Owner_Code = ownerCode,
                Page_Limit = pageLimit,
                Page_Num = pageNum
            }
        };

        var payload = new StringContent(
            JsonSerializer.Serialize(getReportRequest),
            Encoding.UTF8,
            MediaTypeNames.Application.Json);

        var response = await _httpClient.PostAsync("", payload);
        response.EnsureSuccessStatusCode();

        var createOrderResponse = await response.Content.ReadFromJsonAsync<GetInventoryReportResponse>();

        if (createOrderResponse == null) throw new Exception("Có lỗi trong quá trình xử lý");

        if (createOrderResponse.Message.Message_Code != 200)
        {
            throw new Exception(createOrderResponse.Message.Message_Desc);
        }

        return createOrderResponse.Data;
    }
}