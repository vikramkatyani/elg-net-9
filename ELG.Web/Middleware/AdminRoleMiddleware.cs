using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace ELG.Web.Middleware
{
    public class AdminRoleMiddleware
    {
        private readonly RequestDelegate _next;

        public AdminRoleMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            //// Only check for authenticated users and paths under /Admin or as needed
            //var path = context.Request.Path.Value?.ToLower();
            //if (path != null && path.StartsWith("/announcement"))
            //{
            //    // Example: SessionHelper.UserRole > 0 means admin
            //    var userRole = ELG.Web.Helper.SessionHelper.UserRole;
            //    if (userRole <= 0)
            //    {
            //        context.Response.Redirect("/Account/LogOut");
            //        return;
            //    }
            //}

            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<Microsoft.AspNetCore.Authorization.IAllowAnonymous>() != null)
            {
                await _next(context);
                return;
            }

            // Paths to skip
            var path = context.Request.Path;
            if (path.StartsWithSegments("/Account/Login", StringComparison.OrdinalIgnoreCase)
                || path.StartsWithSegments("/Account/LogOut", StringComparison.OrdinalIgnoreCase)
                || path.StartsWithSegments("/css", StringComparison.OrdinalIgnoreCase)
                || path.StartsWithSegments("/js", StringComparison.OrdinalIgnoreCase)
                || path.StartsWithSegments("/favicon.ico", StringComparison.OrdinalIgnoreCase)
                || path.StartsWithSegments("/lib", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            var userRole = ELG.Web.Helper.SessionHelper.UserRole;
            if (userRole <= 0)
            {
                context.Response.Redirect("/Account/Login");
                return;
            }

            await _next(context);
        }
    }
}
