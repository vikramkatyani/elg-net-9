using Microsoft.AspNetCore.Hosting;
using ELG.Model.OrgAdmin;
using ELG.Web.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using ELG.DAL.OrgAdminDAL;
using ELG.DAL.Utilities;
using System.Data;
using Microsoft.AspNetCore.Mvc;

namespace ELG.Web.Controllers
{
    [SessionCheck]
    public class RoleManagementController : Controller
    {
        // GET: ManageAdmin
        public ActionResult Accounts()
        {
            return View("Accounts");
        }

        // to show view assign rights options.
        [HttpGet]
        public IActionResult AssignAdminRights(long? id)
        {
            try
            {
                Logger.Debug($"AssignAdminRights called with id={id}");

                var learnerRep = new LearnerRep();

                ViewBag.LearnerName = "Unknown Learner";
                ViewBag.ErrorMessage = null;

                if (!id.HasValue || id.Value <= 0)
                {
                    SessionHelper.CurrentUserId = 0;
                    return View();
                }

                // set session safely
                try
                {
                    SessionHelper.CurrentUserId = id.Value;
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to set SessionHelper.CurrentUserId", ex);
                    // continue — don't fail the whole page
                }

                LearnerAdminRights learnerAdmin = null;
                try
                {
                    learnerAdmin = learnerRep.GetLearnerAdminRights(id.Value);
                }
                catch (Exception ex)
                {
                    Logger.Error($"GetLearnerAdminRights failed for id={id.Value}", ex);
                    ViewBag.ErrorMessage = "Unable to load learner details.";
                    return View();
                }

                if (learnerAdmin == null || string.IsNullOrWhiteSpace(learnerAdmin.FirstName))
                {
                    ViewBag.LearnerName = "Unknown Learner";
                    ViewBag.ErrorMessage = "Learner not found or has no admin rights.";
                    return View();
                }

                ViewBag.LearnerName = $"{learnerAdmin.FirstName} {learnerAdmin.LastName}";
                return View();
            }
            catch (Exception ex)
            {
                Logger.Error("Unhandled exception in AssignAdminRights", ex);
                ViewBag.ErrorMessage = "An error occurred while loading the page.";
                return View();
            }
        }

        #region global admin rights

        // get list of users with admin rights; on applied filter
        [HttpPost]
        public ActionResult LoadAdminData(AdminLearnerListFilter searchCriteria)
        {
            try
            {
                var adminRep = new AdminRep();

                searchCriteria.Company = Convert.ToInt64(SessionHelper.CompanyId);

                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                }

                searchCriteria.Draw = Request.Form["draw"].FirstOrDefault();
                searchCriteria.Start = Request.Form["start"].FirstOrDefault();
                searchCriteria.Length = Request.Form["length"].FirstOrDefault();

                //Find Order Column
                searchCriteria.SortCol = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                searchCriteria.SortColDir = Request.Form["order[0][dir]"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                AdminLearnerList adminList = adminRep.GetAdminLearners(searchCriteria);
                return Content(Newtonsoft.Json.JsonConvert.SerializeObject(new { draw = searchCriteria.Draw, recordsFiltered = adminList.TotalAdmins, recordsTotal = adminList.TotalAdmins, data = adminList.AdminList }), "application/json");
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("Admins");
            }
        }

        //download learning progress records
        public ActionResult DownloadAdmins(AdminLearnerListFilter searchCriteria)
        {
            List<DownloadAdminLearner> adminReport = new List<DownloadAdminLearner>();
            try
            {
                var adminRep = new AdminRep();

                searchCriteria.Company = Convert.ToInt64(SessionHelper.CompanyId);

                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                }

                adminReport = adminRep.DownloadAdminReport(searchCriteria);

                DataTable dtReport = CommonHelper.ListToDataTable(adminReport);
                string[] columns = { "FirstName", "LastName", "Email", "EmployeeNumber", "AdminLevel", "LocationAdminRights", "DepartmentAdminRights", "LocationSuperVisorRights", "DepartmentSupervisorRights" };
                byte[] filecontent = CommonHelper.ExportExcel(dtReport, "AdminList", true, columns);
                return File(filecontent, CommonHelper.ExcelContentType, "AdminList.xlsx");
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("Learner");
            }
        }

        // to assign admin rights.
        private readonly IWebHostEnvironment _env;

        public RoleManagementController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost]
        public ActionResult AssignAdminRights(Int32 adminRight)
        {
            try
            {
                Int64 learnerId = SessionHelper.CurrentUserId;
                AdminRep adminRep = new AdminRep();
                int result = adminRep.AssignGlobalAdminRights(learnerId, adminRight);

                if (result > 0)
                {
                    AdminLearnerEmailInfo info = adminRep.GetUpgradedAdminEmailInfo(learnerId, adminRight);
                    //send email to the user
                    EmailUtility emailUti = new EmailUtility(_env);
                    string subject = $"{info.OrganisationBrandName}: {info.AdminLevelName} access.";
                    string link = $"{info.BaseURL}/manage";
                    string emailTemplate = emailUti.GetEmailTemplate("AdminUpgrade.html");
                    emailTemplate = emailTemplate.Replace("{username}", $"{info.FirstName} {info.LastName}");
                    emailTemplate = emailTemplate.Replace("{tenantbrandname}", info.OrganisationBrandName);
                    emailTemplate = emailTemplate.Replace("{accesslevel}", info.AdminLevelName);
                    emailTemplate = emailTemplate.Replace("{tenantname}", info.OrganisationName);
                    emailTemplate = emailTemplate.Replace("{tenantcontactemail}", info.SupportEmail);
                    emailTemplate = emailTemplate.Replace("{loginLink}", link);
                    emailTemplate = emailTemplate.Replace("{useremail}", info.EmailId);

                    emailUti.SendEmailUsingSendGrid(info.EmailId, subject, emailTemplate);
                }

                return Content(Newtonsoft.Json.JsonConvert.SerializeObject(new { success = result }), "application/json");
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View();
            }
        }

        // remove global admin rights to the user
        [HttpPost]
        public ActionResult RemoveGlobalAdminRights(Int64 learner, int adminLevel)
        {
            try
            {
                var adminRep = new AdminRep();
                int result = adminRep.RemoveGlobalAdminRights(learner, adminLevel);
                return Content(System.Text.Json.JsonSerializer.Serialize(new { success = result }), "application/json");
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View();
            }
        }

        #endregion

        #region location admin rights

        // to get list of all location and locations with admin rights
        [HttpPost]
        public ActionResult LoadAllLocationToAssignAdminRights()
        {
            try
            {
                AdminRep adminRep = new AdminRep();

                Int64 company = SessionHelper.CompanyId;
                Int64 learner = SessionHelper.CurrentUserId;

                var locationList = adminRep.GetLocationWithAdminRights(company, learner);
                return Content(System.Text.Json.JsonSerializer.Serialize(new { recordsFiltered = locationList.Count, recordsTotal = locationList.Count, data = locationList }), "application/json");
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { });
            }
        }

        // to get list of all location for which user have admin rights
        [HttpPost]
        public ActionResult LoadAllLocationWithAdminRights(Int64 learner)
        {
            try
            {
                AdminRep adminRep = new AdminRep();

                Int64 company = SessionHelper.CompanyId;

                var locationList = adminRep.GetLocationAssignedWithAdminRights(company, learner);
                return Json(new { recordsFiltered = locationList.Count, recordsTotal = locationList.Count, data = locationList });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { });
            }
        }

        // to assign location admin rights to the user
        [HttpPost]
        public ActionResult AssignLocationAdminRights(Int64 location)
        {
            try
            {
                var adminRep = new AdminRep();
                Int64 learner = SessionHelper.CurrentUserId;
                int result = adminRep.AssignLocationAdminRights(learner, location);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View();
            }
        }

        // remove location admin rights to the user
        [HttpPost]
        public ActionResult RemoveLocationAdminRights(Int64 learner, Int64 location)
        {
            try
            {
                var adminRep = new AdminRep();
                int result = adminRep.RemoveLocationAdminRights(learner, location);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View();
            }
        }

        #endregion

        #region location supervisor rights

        // to get list of all location and locations with supervisor rights
        [HttpPost]
        public ActionResult LoadAllLocationToAssignSupervisorRights()
        {
            try
            {
                AdminRep adminRep = new AdminRep();

                Int64 company = SessionHelper.CompanyId;
                Int64 learner = SessionHelper.CurrentUserId;

                var locationList = adminRep.GetLocationWithSupervisorRights(company, learner);
                return Json(new { recordsFiltered = locationList.Count, recordsTotal = locationList.Count, data = locationList });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { });
            }
        }

        // to get list of all location for which user have supervisor rights
        [HttpPost]
        public ActionResult LoadAllLocationWithSupervisorRights(Int64 learner)
        {
            try
            {
                AdminRep adminRep = new AdminRep();

                Int64 company = SessionHelper.CompanyId;

                var locationList = adminRep.GetLocationAssignedWithSupervisorRights(company, learner);
                return Json(new { recordsFiltered = locationList.Count, recordsTotal = locationList.Count, data = locationList });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { });
            }
        }

        // to assign location supervisor rights to the user
        [HttpPost]
        public ActionResult AssignLocationSupervisorRights(Int64 location)
        {
            try
            {
                var adminRep = new AdminRep();
                Int64 learner = SessionHelper.CurrentUserId;
                int result = adminRep.AssignLocationSupervisorRights(learner, location);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View();
            }
        }

        // remove location supervisor rights to the user
        [HttpPost]
        public ActionResult RemoveLocationSupervisorRights(Int64 learner, Int64 location)
        {
            try
            {
                var adminRep = new AdminRep();
                int result = adminRep.RemoveLocationSupervisorRights(learner, location);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View();
            }
        }

        #endregion

        #region department admin rights

        // to get list of all departments and depatments with admin rights
        [HttpPost]
        public ActionResult LoadAllDepartmentToAssignAdminRights(Int64 location)
        {
            try
            {
                AdminRep adminRep = new AdminRep();

                Int64 company = SessionHelper.CompanyId;
                Int64 learner = SessionHelper.CurrentUserId;

                var departmentList = adminRep.GetDepartmentWithAdminRights(company, location, learner);
                return Json(new { recordsFiltered = departmentList.Count, recordsTotal = departmentList.Count, data = departmentList });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { });
            }
        }

        // to get list of all departments for which user have admin rights
        [HttpPost]
        public ActionResult LoadAllDepartmentWithAdminRights(Int64 learner)
        {
            try
            {
                AdminRep adminRep = new AdminRep();

                Int64 company = SessionHelper.CompanyId;

                var departmentList = adminRep.GetDepartmentAssignedWithAdminRights(company, learner);
                return Json(new { recordsFiltered = departmentList.Count, recordsTotal = departmentList.Count, data = departmentList });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { });
            }
        }

        // to assign department admin rights to the user
        [HttpPost]
        public ActionResult AssignDepartmentAdminRights(Int64 location, Int64 department)
        {
            try
            {
                var adminRep = new AdminRep();
                Int64 learner = SessionHelper.CurrentUserId;
                int result = adminRep.AssignDepartmentAdminRights(learner, location, department);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View();
            }
        }

        // remove department admin rights to the user
        [HttpPost]
        public ActionResult RemoveDepartmentAdminRights(Int64 learner, Int64 location, Int64 department)
        {
            try
            {
                var adminRep = new AdminRep();
                int result = adminRep.RemoveDepartmentAdminRights(learner, location, department);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View();
            }
        }

        #endregion

        #region department Supervisor rights

        // to get list of all departments and depatments with admin rights
        [HttpPost]
        public ActionResult LoadAllDepartmentToAssignSupervisorRights(Int64 location)
        {
            try
            {
                AdminRep adminRep = new AdminRep();

                Int64 company = SessionHelper.CompanyId;
                Int64 learner = SessionHelper.CurrentUserId;

                var departmentList = adminRep.GetDepartmentWithSupervisorRights(company, location, learner);
                return Json(new { recordsFiltered = departmentList.Count, recordsTotal = departmentList.Count, data = departmentList });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { });
            }
        }

        // to get list of all departments for which user have Supervisor rights
        [HttpPost]
        public ActionResult LoadAllDepartmentWithSupervisorRights(Int64 learner)
        {
            try
            {
                AdminRep adminRep = new AdminRep();

                Int64 company = SessionHelper.CompanyId;

                var departmentList = adminRep.GetDepartmentAssignedWithSupervisorRights(company, learner);
                return Json(new { recordsFiltered = departmentList.Count, recordsTotal = departmentList.Count, data = departmentList });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { });
            }
        }

        // to assign department Supervisor rights to the user
        [HttpPost]
        public ActionResult AssignDepartmentSupervisorRights(Int64 location, Int64 department)
        {
            try
            {
                var adminRep = new AdminRep();
                Int64 learner = SessionHelper.CurrentUserId;
                int result = adminRep.AssignDepartmentSupervisorRights(learner, location, department);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View();
            }
        }

        // remove department admin rights to the user
        [HttpPost]
        public ActionResult RemoveDepartmentSUpervisorRights(Int64 learner, Int64 location, Int64 department)
        {
            try
            {
                var adminRep = new AdminRep();
                int result = adminRep.RemoveDepartmentSupervisorRights(learner, location, department);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View();
            }
        }

        #endregion
    }
}