using Microsoft.AspNetCore.Authorization;

namespace Kps.Integration.Api;

[Authorize]
public class ReportModel :
    Microsoft.AspNetCore.Mvc.RazorPages.PageModel
{
    private readonly ILogger<ReportModel> _logger;

    public ReportModel(ILogger<ReportModel> logger)
    {
        _logger = logger;
    }
}
