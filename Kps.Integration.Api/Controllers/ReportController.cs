using Kps.Integration.Api.Flows;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Kps.Integration.Api.Controllers;

[Authorize]
[ApiController]
public sealed class ReportController :
    Microsoft.AspNetCore.Mvc.Controller
{
    private readonly ILogger<ReportController> _logger;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public ReportController(
        ILogger<ReportController> logger,
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager)
    {
        _logger = logger;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpPost("report/order/resync")]
    public async Task<IActionResult> ReSyncOrder(
        [FromServices] ICrmOrderFlow flow, [FromQuery] int id)
    {
        try
        {
            if (_signInManager.IsSignedIn(User) == false)
            {
                return Unauthorized();
            }
            
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            var resp = new Models.Reports.ReSyncResp();

            resp.Data = await flow.ReSync(user, id);

            if (resp.Data == true)
            {
                resp.Message = "Succeed!";
            }
            else
            {
                resp.Message = "Failed, please try again!";
            }

            return Ok(resp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Occured erros when Resync Order by Report");

            var resp = new Models.Reports.ReSyncResp
            {
                Code = -1,
                Message = "Occured errors"
            };
        
            return Ok(resp);
        }
    }

    [HttpPost("report/order/search")]
    public IActionResult SearchOrder(
        [FromServices] ICrmOrderFlow flow, [FromBody] Models.Reports.SearchReq req)
    {
        Models.Reports.SearchResp resp;

        try
        {
            resp = flow.Search(req);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Occured erros when search Order by Report");

            resp = new Models.Reports.SearchResp
            {
                Code = -1,
                Message = "Occured errors"
            };
        }

        return Ok(resp);
    }
}
