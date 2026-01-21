using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ELG.DAL.Utilities;
using ELG.Web.Helper;
using ELG.Web.Models;
using ELG.Model.OrgAdmin;
using ELG.DAL.OrgAdminDAL;
using Microsoft.AspNetCore.Mvc;

namespace ELG.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult MyProfile()
        {
            return View();
        }

        #region change password
        //[Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        //[Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordViewModel pwddetail)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var acc = new OrgAdminAccountRep();
                    OrgAdminInfo admin = acc.GetUserShortDetailByUserID(SessionHelper.UserId);
                    if (admin.Password != CommonMethods.EncodePassword(pwddetail.OldPassword, CommonHelper.GetAppSettingValue("LMS_PasswordEncryptionKey")))
                    {
                        ModelState.AddModelError(string.Empty, "Your old password is not correct, Please enter correct old password.");
                        return View(pwddetail);
                    }
                    if (admin.Password == CommonMethods.EncodePassword(pwddetail.NewPassword, CommonHelper.GetAppSettingValue("LMS_PasswordEncryptionKey")))
                    {
                        ModelState.AddModelError(string.Empty, "Your new password can't be same as your old password, Please change your password.");
                        return View(pwddetail);
                    }

                    acc.GetUserShortDetailByUserID(SessionHelper.UserId);
                    acc.UpdateUserPassword(Convert.ToInt32(SessionHelper.UserId), pwddetail.NewPassword, CommonHelper.GetAppSettingValue("LMS_PasswordEncryptionKey"));

                    ViewBag.Message = "Your password has been updated.";
                    return View();
                }
                else
                {
                    return View(pwddetail);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(pwddetail);
            }

        }

        #endregion

        #region change admin roles
        public ActionResult DepartmentAdmin()
        {
            if (CommonHelper.EnsureAdminRole(2))
            {
                SessionHelper.UserRole = 2;
                return View("Dashboard");
            }
            else
            {
                return RedirectToAction("Logout", "Account");
            }
        }
        public ActionResult LocationAdmin()
        {
            if (CommonHelper.EnsureAdminRole(3))
            {
                SessionHelper.UserRole = 3;
                return View("Dashboard");
            }
            else
            {
                return RedirectToAction("Logout", "Account");
            }
        }
        public ActionResult DepartmentSupervisor()
        {
            if (CommonHelper.EnsureAdminRole(9))
            {
                SessionHelper.UserRole = 9;
                return View("Dashboard");
            }
            else
            {
                return RedirectToAction("Logout", "Account");
            }
        }
        public ActionResult LocationSupervisor()
        {
            if (CommonHelper.EnsureAdminRole(8))
            {
                SessionHelper.UserRole = 8;
                return View("Dashboard");
            }
            else
            {
                return RedirectToAction("Logout", "Account");
            }
        }
        public ActionResult ReportAdmin()
        {
            if (CommonHelper.EnsureAdminRole(4))
            {
                SessionHelper.UserRole = 4;
                return View("Dashboard");
            }
            else
            {
                return RedirectToAction("Logout", "Account");
            }
        }
        public ActionResult UserAdmin()
        {
            if (CommonHelper.EnsureAdminRole(5))
            {
                SessionHelper.UserRole = 5;
                return View("Dashboard");
            }
            else
            {
                return RedirectToAction("Logout", "Account");
            }
        }
        #endregion

        #region Dashboard
        [SessionCheck]
        public ActionResult Dashboard()
        {
            return View();
        }


        [HttpPost]
        //fetch admin dashoard info
        public ActionResult LoadDashboardHeaderInfo()
        {
            DashboardHeader header = new DashboardHeader();
            try
            {
                var dashRep = new DashboardRep();
                header = dashRep.GetAdminDashboardHeader(SessionHelper.UserRole, SessionHelper.UserId, SessionHelper.CompanyId);
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
            return Json(new { header });

        }

        [HttpPost]
        //fetch yearly completions
        public ActionResult LoadDashboardYearlyCompletion()
        {
            List<DashboardYearlyData> info = new List<DashboardYearlyData>();
            try
            {
                var dashRep = new DashboardRep();
                info = dashRep.GetAdminDashboardYearlyCompletion(SessionHelper.UserRole, SessionHelper.UserId, SessionHelper.CompanyId);
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
            return Json(new { info });

        }

        [HttpPost]
        //fetch yearly completions
        public ActionResult LoadDashboardUserQuotaPerLocation()
        {
            List<LocationUserQuota> info = new List<LocationUserQuota>();
            try
            {
                if (SessionHelper.UserRole == 1)
                {
                    var dashRep = new DashboardRep();
                    info = dashRep.GetAdminDashboardUserQuotaPerLocation(SessionHelper.CompanyId);
                }
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
            return Json(new { info });

        }

        [HttpPost]
        //fetch course completions
        public ActionResult LoadCourseCompletionData()
        {
            DashboardCourseCompletion info = new DashboardCourseCompletion();
            try
            {
                var dashRep = new DashboardRep();
                info = dashRep.GetAdminDashboardCourseCompletions(SessionHelper.UserRole, SessionHelper.UserId, SessionHelper.CompanyId);
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
            return Json(new { info });

        }        

        //[HttpPost]
        ////fetch weekly completions
        //public ActionResult LoadDashboardWeeklyData()
        //{
        //    DashboardWeeklyData info = new DashboardWeeklyData();
        //    try
        //    {
        //        var dashRep = new DashboardRep();
        //        info = dashRep.GetAdminDashboardWeeklyProgress(SessionHelper.UserRole, SessionHelper.UserId, SessionHelper.CompanyId);
        //    }

        //    catch (Exception ex)
        //    {
        //        Logger.Error(ex.Message, ex);
        //    }
        //    return Json(new { info });

        //}
        [HttpPost]
        //fetch all courses and licence consumption
        public ActionResult GetDashboardLicenseUsage()
        {
            List<DashboardLicenceUsage> info = new List<DashboardLicenceUsage>();
            try
            {
                var dashRep = new DashboardRep();
                info = dashRep.GetDashboardLicenseUsage(Convert.ToInt64(SessionHelper.CompanyId));
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
            return Json(new { info });

        }

        [HttpPost]
        //fetch all courses and licence consumption
        public ActionResult GetDashboardCourseCompletion()
        {
            List<DashboardCourseCompletion> info = new List<DashboardCourseCompletion>();
            try
            {
                var dashRep = new DashboardRep();
                info = dashRep.GetDashboardCourseCompletion(Convert.ToInt64(SessionHelper.CompanyId));
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
            return Json(new { info });

        }

        [HttpPost]
        //fetch all courses and licence consumption
        public ActionResult GetDashboardCourseAssignments()
        {
            List<DashboardCourseAssignment> info = new List<DashboardCourseAssignment>();
            try
            {
                var dashRep = new DashboardRep();
                info = dashRep.GetDashboardCourseAssignments(Convert.ToInt64(SessionHelper.CompanyId));
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
            return Json(new { info });

        }

        [HttpPost]
        //fetch atf notification
        public ActionResult GetDashboardNotification()
        {
            string notification = "";
            try
            {
                var dashRep = new DashboardRep();
                notification = dashRep.GetDashboardNotification();
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
            return Json(new { notification });

        }
        #endregion

        #region HELP
        //[Authorize]
        public ActionResult Contact()
        {
            return View();
        }
        #endregion

        #region Communication

        [SessionCheck]
        public ActionResult ModuleNotification()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LoadModuleNotificationData()
        {
            try
            {
                var companyRep = new CompanyRep();

                CompanyNotificationSettings config = companyRep.GetCompanyNotificationSettingsInfo(SessionHelper.CompanyId);
                return Json(new { config });

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("ModuleNotification");
            }
        }

        [HttpPost]
        public ActionResult LoadModuleAssignmentData()
        {
            try
            {
                var companyRep = new CompanyRep();

                CompanyModuleAssigmentNotificationSettings config = companyRep.GetCompanyModuleAssignmentNotificationSettingsInfo(SessionHelper.CompanyId);
                return Json(new { config });

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("ModuleNotification");
            }
        }

        // to update ra reminder info
        [HttpPost]
        public ActionResult UpdateNotificationSettings(CompanyNotificationSettings config)
        {
            try
            {
                var rep = new CompanyRep();
                config.CompanyId = SessionHelper.CompanyId;
                int result = rep.UpdateCompanyNotificationSettings(config);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        // to update ra reminder info
        [HttpPost]
        public ActionResult UpdateCourseAssignmentEmailSettings(CompanyModuleAssigmentNotificationSettings config)
        {
            try
            {
                var rep = new CompanyRep();
                config.CompanyId = SessionHelper.CompanyId;
                int result = rep.UpdateCourseAssignmentEmailSettings(config);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        [SessionCheck]
        public ActionResult ModuleReminder()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LoadTestReminderData()
        {
            try
            {
                var companyRep = new CompanyRep();

                CompanyReminderConfiguration config = companyRep.GetCompanyTestReminderInfo(SessionHelper.CompanyId);
                return Json(new { config });

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("ModuleReminder");
            }
        }

        // to update test reminder info
        [HttpPost]
        public ActionResult UpdateTestReminder(CompanyReminderConfiguration config)
        {
            try
            {
                var rep = new CompanyRep();
                config.CompanyId = SessionHelper.CompanyId;
                int result = rep.UpdateCompanyTestReminderInfo(config);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        [SessionCheck]
        public ActionResult RAReminder()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LoadRAReminderData()
        {
            try
            {
                var companyRep = new CompanyRep();

                CompanyReminderConfiguration config = companyRep.GetCompanyRAReminderInfo(SessionHelper.CompanyId);
                return Json(new { config });

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("RAReminder");
            }
        }

        // to update ra reminder info
        [HttpPost]
        public ActionResult UpdateRAReminder(CompanyReminderConfiguration config)
        {
            try
            {
                var rep = new CompanyRep();
                config.CompanyId = SessionHelper.CompanyId;
                int result = rep.UpdateCompanyRAReminderInfo(config);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }



        #endregion

        #region Resources/Business certificate

        public ActionResult Resources()
        {
            return View();
        }

        #endregion

        // Add error action for global error handling
        public IActionResult Error()
        {
            return View();
        }

        // Add status code error action for status code handling
        public IActionResult StatusErrorCode(int code)
        {
            return View("StatusErrorCode", code);
        }

    }
}