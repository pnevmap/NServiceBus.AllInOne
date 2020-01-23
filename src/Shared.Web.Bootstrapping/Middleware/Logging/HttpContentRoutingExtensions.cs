using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Shared.Web.Bootstrapping.Middleware.Logging
{
    public static class HttpContentRoutingExtensions
    {
        private const string ControllerKey = ".route.controller";
        private const string ActionKey = ".route.action";

        public static void StoreCurrentRouteInfo(this ActionExecutingContext context)
        {
            try
            {
                context.HttpContext.Items[ControllerKey] = context.RouteData.Values["controller"];
                context.HttpContext.Items[ActionKey] = context.RouteData.Values["action"];

            }
            catch
            {
                context.HttpContext.Items[ControllerKey] = null;
                context.HttpContext.Items[ActionKey] = null;
            }
        }

        public static (string ControllerName, string ActionName) GetRouteInfo(this HttpContext context)
        {
            try
            {
                return (context.Items[ControllerKey] as string, context.Items[ActionKey] as string);
            }
            catch
            {
                return (null, null);
            }
        }
    }
}