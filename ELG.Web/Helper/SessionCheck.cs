using ELG.Web.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ELG.Web.Helper
{
    public class SessionCheck : ActionFilterAttribute
    {
        /// <summary>
        /// On Action Executing
        /// </summary>
        /// <param name="filterContext">filter Context</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (SessionHelper.UserId == 0 || SessionHelper.CompanyId == 0)
            {
                // TODO: Sign out logic should be implemented using ASP.NET Core Identity.
                // For now, this simply redirects to login and sets 401 status.
                // ASP.NET Core does not have IsAjaxRequest; check X-Requested-With header instead
                var isAjax = filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
                if (isAjax)
                {
                    filterContext.HttpContext.Response.StatusCode = 401;
                }
                else
                {
                    filterContext.HttpContext.Response.StatusCode = 401;
                    filterContext.Result = new RedirectResult("~/Account/Login");
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}