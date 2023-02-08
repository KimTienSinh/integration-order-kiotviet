using Kps.Integration.Api.Models.WmsReport;
using Kps.Integration.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kps.Integration.Api.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class WmsReportController : ControllerBase
{
    private readonly IWmsReportService _report;

    public WmsReportController(IWmsReportService report)
    {
        _report = report;
    }

    [HttpPost("resync")]
    public async Task<IActionResult> Sync(int id)
    {
        var (result, message) = await _report.ReSync(id);
        if (!result)
        {
            return BadRequest(new {Message = message});
        }

        return Ok(new {Message = "Success"});
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] SearchQuery query)
    {
        var (items, total) = await _report.Search(query);
        return Ok(new {items, total});
    }
}