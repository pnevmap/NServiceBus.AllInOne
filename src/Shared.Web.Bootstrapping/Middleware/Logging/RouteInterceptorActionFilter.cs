using Microsoft.AspNetCore.Mvc.Filters;

namespace Shared.Web.Bootstrapping.Middleware.Logging
{
    public class RouteInterceptorActionFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            context.StoreCurrentRouteInfo();
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // nothing to do here
        }
    }
}