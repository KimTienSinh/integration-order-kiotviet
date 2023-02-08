using Kps.Integration.Proxy.Magento.Models.Orders;
using Kps.Integration.Proxy.Magento.Models.Products;
using Refit;

namespace Kps.Integration.Proxy.Magento;

public interface IMagentoProxy
{
    [Get("/rest/all/V1/orders/{id}")]
    Task<Order> GetOrder(int id, CancellationToken cancellation = default);

    [Get("/rest/V1/mobileapi/products/id/{id}")]
    Task<GetProductResponse> GetProduct(int id, CancellationToken cancellation = default);

    [Post("/rest/all/V1/orders")]
    Task<Order> UpdateOrder(UpdateOrderRequest request, CancellationToken cancellation = default);
}