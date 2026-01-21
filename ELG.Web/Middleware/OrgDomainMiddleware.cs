using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ELG.Web.Middleware
{
    public class OrgDomainMiddleware
    {
        private readonly RequestDelegate _next;

        public OrgDomainMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var host = context.Request.Host.Host;
            var sessionDomainDetails = ELG.Web.Helper.SessionHelper.OrgDomainDetails;

            try
            {
                if (sessionDomainDetails == null || host != sessionDomainDetails.Domain)
                {
                    var rep = new ELG.DAL.OrgAdminDAL.CompanyRep();
                    var organization = rep.GetOrganizationFromHost(host);
                    ELG.Web.Helper.SessionHelper.OrgDomainDetails = organization;
                }
            }
            catch (Exception ex)
            {
                // Log the error (replace with your logger as needed)
                System.Diagnostics.Debug.WriteLine($"[OrgDomainMiddleware] DB error: {ex.Message}");
                // Optionally, set OrgDomainDetails to null or a default value
                ELG.Web.Helper.SessionHelper.OrgDomainDetails = null;
            }

            await _next(context);
        }
    }
}
