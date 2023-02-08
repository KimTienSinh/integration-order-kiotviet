using Kps.Integration.Proxy.CRM.Models.Accounts;
using Kps.Integration.Proxy.CRM.Models.Orders;
using Kps.Integration.Proxy.CRM.Models.Products;
using Refit;

namespace Kps.Integration.Proxy.CRM;

public interface IGetflyProxy
{
    [Post("/api/v3/account")]
    Task<CreateAccountResp> CreateAccountAsync(
        [Body] CreateAccountReq account, CancellationToken cancellationToken = default);

    [Post("/api/v3/products")]
    Task<CreateProductResp> CreateProductAsync(
        [Body] CreateProductReq product, CancellationToken cancellationToken = default);

    [Get("/api/v3/accounts")]
    Task<GetAccountListResp> GetAccountListAsync(
        string q = null, CancellationToken cancellationToken = default);

    [Post("/api/v3/orders")]
    Task<int> CreateOrderAsync(
        [Body] CreateOrderReq order, CancellationToken cancellationToken = default);

    [Get("/api/v3/products")]
    Task<GetProductListResp> GetProductListAsync(
        string product_code = null, CancellationToken cancellationToken = default);
}
