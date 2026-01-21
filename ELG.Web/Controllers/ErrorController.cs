using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace ELG.Web.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        public new ActionResult NotFound()
        {
            return View();
        }
    }
}