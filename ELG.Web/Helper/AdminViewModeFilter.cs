using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace ELG.Web.Helper
{
    // Global result filter: when admin view mode is modern, render *Modern view if it exists.
    public class AdminViewModeFilter : IAsyncResultFilter
    {
        private readonly ICompositeViewEngine _viewEngine;

        public AdminViewModeFilter(ICompositeViewEngine viewEngine)
        {
            _viewEngine = viewEngine;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context?.Result is not ViewResult viewResult)
            {
                await next();
                return;
            }

            if (!ShouldAttemptModernView(context, viewResult))
            {
                await next();
                return;
            }

            var routeValues = context.RouteData.Values;
            var actionName = routeValues["action"]?.ToString();
            if (string.IsNullOrWhiteSpace(actionName))
            {
                await next();
                return;
            }

            var currentViewName = string.IsNullOrWhiteSpace(viewResult.ViewName) ? actionName : viewResult.ViewName;
            if (string.IsNullOrWhiteSpace(currentViewName) ||
                currentViewName.EndsWith("Modern", StringComparison.OrdinalIgnoreCase) ||
                currentViewName.Contains("/"))
            {
                await next();
                return;
            }

            var modernViewName = currentViewName + "Modern";
            var modernView = _viewEngine.FindView(context, modernViewName, isMainPage: true);
            if (modernView.Success)
            {
                viewResult.ViewName = modernViewName;
            }

            await next();
        }

        private static bool ShouldAttemptModernView(ResultExecutingContext context, ViewResult viewResult)
        {
            var req = context.HttpContext.Request;
            if (!HttpMethods.IsGet(req.Method))
            {
                return false;
            }

            var isAjax = string.Equals(req.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
            if (isAjax)
            {
                return false;
            }

            if (SessionHelper.IsLearnerUser)
            {
                return false;
            }

            if (!string.Equals(SessionHelper.AdminViewMode, "modern", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (viewResult.ViewData != null &&
                viewResult.ViewData.ContainsKey("DisableModernView") &&
                Convert.ToBoolean(viewResult.ViewData["DisableModernView"]))
            {
                return false;
            }

            return true;
        }
    }
}
