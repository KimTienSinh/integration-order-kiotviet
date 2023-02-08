using Kps.Integration.Api.Models.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace Kps.Integration.Api.Extensions.Filters;

public class ApiAuthenticationAttribute : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        string reqMethod = context.HttpContext.Request.Method;

        // don't process OPTIONS to supply CORS
        if (HttpMethods.IsOptions(reqMethod))
        {
            await next();
            return;
        }

        var apiSecretOptions = (IOptions<ApiSecretOptions>) context.HttpContext
            .RequestServices.GetService(typeof(IOptions<ApiSecretOptions>))!;

        var isSuccess = context.HttpContext.Request.Headers.TryGetValue("X-ApiKey", out var apiKey);
        if (!isSuccess)
        {
            context.Result = new UnauthorizedObjectResult("API Key is missing");
            return;
        }

        var app = apiSecretOptions!.Value.ApiSecrets.FirstOrDefault(x => x.Key == apiKey.ToString());
        if (app == null)
        {
            context.Result = new UnauthorizedObjectResult("API Key Invalid");
            return;
        }

        await next();
    }
}