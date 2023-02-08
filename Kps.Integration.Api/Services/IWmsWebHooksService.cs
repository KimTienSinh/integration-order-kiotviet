using Kps.Integration.Api.Models.Orders;

namespace Kps.Integration.Api.Services;

public interface IWmsWebHooksService
{
    Task Handler(WmsOrderStatusNotificationPayload payload, CancellationToken cancellationToken = default);
}