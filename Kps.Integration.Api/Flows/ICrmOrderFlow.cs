using Microsoft.AspNetCore.Identity;

namespace Kps.Integration.Api.Flows;

public interface ICrmOrderFlow
{
    Models.Reports.SearchResp Search(Models.Reports.SearchReq req);

    Task<bool> ReSync(IdentityUser user, int id);
}
