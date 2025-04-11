using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace PadelInDubai.Attributes
{
    public class LocalhostOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var remoteIp = context.HttpContext.Connection.RemoteIpAddress;
            var localIp = context.HttpContext.Connection.LocalIpAddress;

            if (remoteIp?.ToString() != "127.0.0.1" &&
                remoteIp?.ToString() != "::1" &&
                remoteIp?.ToString() != localIp?.ToString())
            {
                context.Result = new StatusCodeResult(403);
            }
        }
    }
}
