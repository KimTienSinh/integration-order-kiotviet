using Kps.Integration.Api.Models.Orders;
using Kps.Integration.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Kps.Integration.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class WmsWebHookController : ControllerBase
{
    [HttpPost("UpdateOrderStatus")]
    public async Task<IActionResult> UpdateOrderStatus(
        [FromBody] WmsOrderStatusNotificationPayload payload,
        [FromServices] IWmsWebHooksService webHooksService)
    {
        await webHooksService.Handler(payload, CancellationToken.None);
        return Ok();
    }
}