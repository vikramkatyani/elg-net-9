using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace ELG.Web.Controllers
{
    public class ProfileController : Controller
    {
        // GET: Profile
        public ActionResult ManageProfile()
        {
            return View();
        }
        public ActionResult AssignProfile()
        {
            return View();
        }
        public ActionResult ProfileAutoAssign()
        {
            return View();
        }
        public ActionResult RenewProfile()
        {
            return View();
        }
    }
}