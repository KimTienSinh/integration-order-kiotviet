using Kps.Integration.Api.Flows;
using Microsoft.AspNetCore.Mvc;

namespace Kps.Integration.Api.Controllers;

[ApiController]
[Route("order")]
public sealed class OrderController : ControllerBase
{

    private const int MaxLimit = 4;

    [HttpGet("sync")]
    public async Task<ActionResult> Sync(
        [FromServices] ISyncMagentoOrderToCrmFlow flow,
        [FromQuery] int? limit = MaxLimit
    ) {
        int iLimit;

        if (limit == null)
        {
            iLimit = MaxLimit;
        }
        else if (limit < 1)
        {
            return BadRequest(new Models.Orders.SyncResp
            {
                Code = 400,
                Message = "Bad request (limit cannot less than 1)",
                Data = null
            });
        }
        else if (limit > MaxLimit)
        {
            return BadRequest(new Models.Orders.SyncResp
            {
                Code = 400,
                Message = $"Bad request (limit cannot greater than {MaxLimit})"
            });
        }
        else
        {
            iLimit = limit.Value;
        }

        await flow.ExecuteAsync(iLimit);
        return Ok();
    }
}
