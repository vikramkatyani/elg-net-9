using ELG.Model.OrgAdmin;
using Microsoft.AspNetCore.Http;
using ELG.Web.Helper;
using ELG.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using ELG.DAL.Utilities;
using System.IO;
using System.Data;
using ClosedXML.Excel;
using System.Threading.Tasks;
using iText.StyledXmlParser.Jsoup.Safety;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using ELG.DAL.OrgAdminDAL;

namespace ELG.Web.Controllers
{
    public class UserManageRequest
    {
        public long learnerId { get; set; }
    }
    [SessionCheck]
    public class UserManagementController : Controller
    {
        private readonly EmailUtility emailUti;
        private readonly IWebHostEnvironment _env;

        public UserManagementController(IWebHostEnvironment env)
        {
            _env = env;
            emailUti = new EmailUtility(_env);
        }

        #region Manage Learner
        // GET: list of all registered learners
        public ActionResult Accounts()
        {
            return View("Accounts");
        }

        // get report on applied filter
        [HttpPost]
        public ActionResult LoadLearnerData(AdminLearnerFilter searchCriteria)
        {
            try
            {
                var learnerRep = new LearnerRep();

                searchCriteria.Company = Convert.ToInt64(SessionHelper.CompanyId);
                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                    searchCriteria.SearchLocation = 0;
                    searchCriteria.SearchDepartment = 0;
                    searchCriteria.SearchStatus = -1;
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

                OrganisationLearnerList learnerList = learnerRep.GetRegisteredLearners(searchCriteria, SessionHelper.UserId, SessionHelper.UserRole);

                return Json(new { draw = searchCriteria.Draw, recordsFiltered = learnerList.TotalLearners, recordsTotal = learnerList.TotalLearners, data = learnerList.LearnerList, adminPrev = SessionHelper.UserRole, ssoOrg = SessionHelper.IsSSOLogin });

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("Accounts");
            }
        }

        //download learning progress records
        public ActionResult DownloadLearners(AdminLearnerFilter searchCriteria)
        {
            List<DownloadLearnerList> learnerReport = new List<DownloadLearnerList>();
            try
            {
                var reportRep = new LearnerRep();

                searchCriteria.Company = SessionHelper.CompanyId;

                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                }

                learnerReport = reportRep.DownloadLearnerReport(searchCriteria, SessionHelper.UserId, SessionHelper.UserRole);

                DataTable dtReport = CommonHelper.ListToDataTable(learnerReport);
                string[] columns = { "FirstName", "LastName","EmailId", "Location", "Department", "EmployeeNumber", "Status" };
                byte[] filecontent = CommonHelper.ExportExcel(dtReport, "Learners", true, columns);
                return File(filecontent, CommonHelper.ExcelContentType, "Learners.xlsx");
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("Learner");
            }
        }

        // to delete learner and all associated records.
        [HttpPost]
        public ActionResult DeleteLearner(int learnerId)
        {
            try
            {
                if (SessionHelper.UserRole == 8 || SessionHelper.UserRole == 9)
                    return Json(new { success = -2 });
                else
                {
                    var learnerRep = new LearnerRep();
                    int result = learnerRep.DeleteLearner(learnerId);
                    return Json(new { success = result });
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }
        
        // to reset learner's password.
        [HttpPost]
        public ActionResult ResetLearnerPassword(UserManageRequest request)
        {
            try
            {
                Int64 learnerId = request?.learnerId ?? 0;
                var acc = new OrgAdminAccountRep();
                acc.UpdateLearnerPasswordByAdmin(learnerId, CommonHelper.GetAppSettingValue("LMS_DefaultPassword"), CommonHelper.GetAppSettingValue("LMS_PasswordEncryptionKey"));
                return Json(new { success = 1 });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }
        
        // to resend learner's activation email.
        [HttpPost]
        public ActionResult ResendLearnerActivationEmail(UserManageRequest request)
        {
            try
            {
                Int64 learnerId = request?.learnerId ?? 0;
                var learnerRep = new LearnerRep();
                LearnerInfo learnerInfo = learnerRep.GetLearnerInfo(learnerId);

                var companyRep = new CompanyRep();
                var org = companyRep.GetCompanyInfo(SessionHelper.CompanyId);

                string subject = $"{org.CompanyBrandName}: Account Activation Link Inside";

                string link = emailUti.CreateLoginLinkToBeSendInMail(org.CompanyBaseURL, learnerId, true);
                string emailTemplate = emailUti.GetEmailTemplate("NewUserEmail.html");
                emailTemplate = emailTemplate.Replace("{username}", $"{learnerInfo.FirstName} {learnerInfo.LastName}");
                emailTemplate = emailTemplate.Replace("{tenantbrandname}", org.CompanyBrandName);
                emailTemplate = emailTemplate.Replace("{tenantname}", org.CompanyName);
                emailTemplate = emailTemplate.Replace("{tenantcontactemail}", org.CompanySupportEmail);
                emailTemplate = emailTemplate.Replace("{loginLink}", link);
                emailTemplate = emailTemplate.Replace("{useremail}", learnerInfo.EmailId);

                emailUti.SendEmailUsingSendGrid(learnerInfo.EmailId, subject, emailTemplate);

                return Json(new { success = 1 });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        // to activate or deactivate learner status.
        [HttpPost]
        public ActionResult UpdateActiveStatus(LearnerInfo learner)
        {
            try
            {
                var learnerRep = new LearnerRep();
                int result = learnerRep.UpdateLearnerActiveStatus(learner.UserID, learner.IsDeactive);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }


        // to get learner info
        public ActionResult GetLearnerInfo(LearnerInfo learner)
        {
            try
            {
                var learnerRep = new LearnerRep();
                LearnerInfo learnerInfo = learnerRep.GetLearnerInfo(learner.UserID);

                // get list of all locations for user info and department for users' location
                var loc_dep_Filter = new CommonMethods();
                List<OrganisationLocation> locationList = new List<OrganisationLocation>();
                List<OrganisationDepartment> departmentList = new List<OrganisationDepartment>();

                locationList = loc_dep_Filter.GetLocationsForCompany(SessionHelper.UserRole, SessionHelper.UserId, SessionHelper.CompanyId);
                departmentList = loc_dep_Filter.GetDepartmentsForLocation(SessionHelper.UserRole, SessionHelper.UserId, learnerInfo.LocationID);

                return Json(new { learnerInfo, locationList, departmentList });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View();
            }
        }

        // to get session learner info
        public ActionResult GetSessionLearnerInfo(LearnerInfo learner)
        {
            try
            {
                var learnerRep = new LearnerRep();
                LearnerInfo learnerInfo = learnerRep.GetLearnerInfo(Convert.ToInt64(SessionHelper.UserId));

                // get list of all locations for user info and department for users' location
                var loc_dep_Filter = new CommonMethods();
                List<OrganisationLocation> locationList = new List<OrganisationLocation>();
                List<OrganisationDepartment> departmentList = new List<OrganisationDepartment>();

                locationList = loc_dep_Filter.GetLocationsForCompany(SessionHelper.UserRole, SessionHelper.UserId, SessionHelper.CompanyId);
                departmentList = loc_dep_Filter.GetDepartmentsForLocation(SessionHelper.UserRole, SessionHelper.UserId, learnerInfo.LocationID);

                return Json(new { learnerInfo, locationList, departmentList });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View();
            }
        }

        // to update learner info.
        [HttpPost]
        public ActionResult UpdateSessionLearnerInfo(LearnerInfo learner)
        {
            try
            {
                var learnerRep = new LearnerRep();
                learner.UserID = Convert.ToInt64(SessionHelper.UserId);
                int result = learnerRep.UpdateLearnerInfo(learner);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View();
            }
        }

        //upload session learner profile pic
        public ActionResult UploadProfilePic()
        {
            int uploaded = 0;
            try
            {
                var file = Request.Form.Files.FirstOrDefault();
                if (file != null && file.Length > 0)
                {
                    var profilePic = CommonHelper.ConvertToBytes(file);
                    var learnerRep = new LearnerRep();
                    uploaded = learnerRep.UpdateLearnerProfilePic(SessionHelper.UserId, profilePic);

                    //update profile pic in session
                    if (uploaded == 1)
                    {
                        string learnerProfileBase64Data = Convert.ToBase64String(profilePic);
                        SessionHelper.ProfilePic = string.Format("data:image/png;base64,{0}", learnerProfileBase64Data);
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }

            return Json(new { uploaded });
        }

        //remove profile pic
        public ActionResult RemoveProfilePic()
        {
            int uploaded = 0;
            try
            {
                var learnerRep = new LearnerRep();
                uploaded = learnerRep.RemoveLearnerProfilePic(SessionHelper.UserId);

                //update profile pic in session
                if (uploaded == 1)
                {
                    SessionHelper.ProfilePic = null;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }

            return Json(new { uploaded });
        }

        // to update learner info.
        [HttpPost]
        public ActionResult UpdateLearnerInfo(LearnerInfo learner)
        {
            try
            {
                var learnerRep = new LearnerRep();
                int result = learnerRep.UpdateLearnerInfo(learner);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View();
            }
        }
        #endregion

        #region Create Learner
        // create new learner view
        public ActionResult CreateAccount()
        {
            return View();
        }

        // function to add new user
        [HttpPost]
        public ActionResult CreateAccount([FromBody] LearnerInfo learner)
        {
            try
            {

                learner.CompanyID = SessionHelper.CompanyId;
                learner.Title = learner.Title ?? "";
                learner.EmployeeNumber = learner.EmployeeNumber ?? "";
                if (String.IsNullOrEmpty(learner.EmailId) || learner.CompanyID <= 0)
                {
                    return View();
                }
                var learnerRep = new LearnerRep();
                NewLearner Learner = learnerRep.CreateNewLearner(learner, SessionHelper.CompanyCourseAssignmentMode, SessionHelper.UserRole, SessionHelper.UserId);

                //send account creation email if user is created
                if(Learner.UserID > 1)
                {

                    
                    var companyRep = new CompanyRep();
                    var org = companyRep.GetCompanyInfo(SessionHelper.CompanyId);

                    string subject = $"{org.CompanyBrandName}: Account Activation Link Inside";
                    string learnerEmail = learner.EmailId;
                    string link = "";
                    string emailTemplate = "";
                    if (SessionHelper.IsSSOLogin)
                    {
                        emailTemplate = emailUti.GetEmailTemplate("NewSSOUserEmail.html");
                        //link = ConfigurationManager.AppSettings["LMS_LearnerBaseURL"] + "ssoaccount";
                        string hostPath = $"{Request.Scheme}://{Request.Host.Value}";
                        link = hostPath + "/ssoaccount";
                    }

                    else
                    {
                        emailTemplate = emailUti.GetEmailTemplate("NewUserEmail.html");
                        link = emailUti.CreateLoginLinkToBeSendInMail(org.CompanyBaseURL, Learner.UserID, true);
                    }
                    emailTemplate = emailTemplate.Replace("{username}", $"{learner.FirstName} {learner.LastName}");
                    emailTemplate = emailTemplate.Replace("{tenantbrandname}", org.CompanyBrandName);
                    emailTemplate = emailTemplate.Replace("{tenantname}", org.CompanyName);
                    emailTemplate = emailTemplate.Replace("{tenantcontactemail}", org.CompanySupportEmail);
                    emailTemplate = emailTemplate.Replace("{loginLink}", link);
                    emailTemplate = emailTemplate.Replace("{useremail}", learnerEmail);

                    emailUti.SendEmailUsingSendGrid(learnerEmail, subject, emailTemplate);
                }

                return Json(new { success = Learner.UserID });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { error = ex.Message });
            }
        }

        // create new learner view
        public ActionResult OpenEnrollment()
        {
            return View();            
        }

        [HttpPost]
        public ActionResult LoadSelfRegisterData()
        {
            var cr = new CompanyRep();
            Company company = cr.GetCompanyInfo(SessionHelper.CompanyId);

            // check if company allows self registration
            if (company.AllowedSelfRegistration || (!String.IsNullOrEmpty(company.Settings) && company.Settings.Contains("regdeny:False")))
            {
                //allow self registration
                return Json(new { selfregister = 1 });
            }
            else
            {
                // not allow self registration
                return Json(new { selfregister = 0 });
            }
        }

        #endregion

        #region Upload Bulk User

        // create new learner view
        public ActionResult ImportAccounts()
        {
            return View();
        }

        public ActionResult DownloadUserExcelTemplate()
        {
            try
            {
                UploadDownloadUserTemplate template = new UploadDownloadUserTemplate(_env);

                List<String> headerList = template.CreateHeaders(Convert.ToInt32(SessionHelper.CompanyId));
                DataTable table = new DataTable();
                foreach (var item in headerList)
                {
                    table.Columns.Add(item);
                }

                string[] columns = headerList.ToArray<string>();
                byte[] filecontent = CommonHelper.ExportExcelWithHeader(table, "Accounts", false, columns, columns);
                return File(filecontent, CommonHelper.ExcelContentType, "ImportAccountsTemplate.xlsx");

                //using (XLWorkbook wb = new XLWorkbook())
                //{
                //    wb.Worksheets.Add(table, "UserTemplate");

                //    Response.Clear();
                //    Response.Buffer = true;
                //    Response.Charset = "";
                //    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                //    Response.AddHeader("content-disposition", "attachment;filename=UserTemplate.xlsx");
                //    using (MemoryStream MyMemoryStream = new MemoryStream())
                //    {
                //        wb.SaveAs(MyMemoryStream);
                //        MyMemoryStream.WriteTo(Response.OutputStream);
                //        Response.Flush();
                //        Response.End();
                //    }
                //}

                //return View("ImportAccounts");
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("ImportAccounts");
            }
        }

        [HttpPost]
        public async Task<ActionResult> UploadUserTemplate(IFormFile myPostedFile)
        {
            try
            {
                string fileExtension = "";

                if (myPostedFile == null)
                {
                    ViewBag.UploadStatus = "Please select users data file.";
                    return View("ImportAccounts");
                }

                if (myPostedFile != null && myPostedFile.Length > 0)
                {
                    FileInfo finfo = new FileInfo(myPostedFile.FileName);
                    fileExtension = finfo.Extension.ToLower();
                    if (fileExtension != ".xls" && fileExtension != ".xlsx" && fileExtension != ".csv")
                    {
                        ViewBag.UploadStatus = "Please select valid file.";
                        return View("ImportAccounts");
                    }
                }
                if(SessionHelper.CompanyId <= 0)
                {
                    ViewBag.UploadStatus = "Session Timed Out. Please login again and retry.";
                    return View("ImportAccounts");
                }

                string strUploadPath = Path.Combine(_env.WebRootPath, "Upload");

                if (fileExtension == ".csv")
                {
                    string strFilePath = "";
                    if (!Directory.Exists(strUploadPath))
                        Directory.CreateDirectory(strUploadPath);

                    strFilePath = Path.Combine(_env.WebRootPath, "Upload");

                    // Only want the filename
                    string strClientFile = myPostedFile.FileName.Substring(myPostedFile.FileName.LastIndexOf("\\") + 1);

                    // Must have at least one character + extension
                    if (strClientFile.Length > 4)
                    {
                        // Save Uploaded file to server
                        string strFileName = "IC" + SessionHelper.CompanyId.ToString() + DateTime.Now.ToString("dd_MMM_yy_hh_mm_ss") + fileExtension;
                        strUploadPath = strUploadPath + strFileName;

                        if (myPostedFile.Length <= Convert.ToInt32(ELG.Web.Helper.CommonHelper.GetAppSettingValue("LMS_MaxUploadSize")))
                        {
                            if (myPostedFile.Length > 10)
                            { // Check file > 10 bytes
                              // Do the Upload
                                try
                                {
                                    using (var stream = new FileStream(strUploadPath, FileMode.Create))
                                    {
                                        await myPostedFile.OpenReadStream().CopyToAsync(stream);
                                    }
                                }
                                catch
                                {
                                    ViewBag.UploadStatus = "File not uploaded.";
                                    return View("ImportAccounts");
                                }
                            }
                        }
                    }
                }

                UploadDownloadUserTemplate template = new UploadDownloadUserTemplate(_env);
                UploadContactResult uploadResult = new UploadContactResult();
                //uploadResult = await template.AsynchImportContact(SessionHelper.CompanyId, SessionHelper.UserRole, SessionHelper.UserId, myPostedFile, fileExtension, strUploadPath);
                uploadResult = await template.AsynchImportContact(SessionHelper.CompanyId, SessionHelper.IsSSOLogin, SessionHelper.UserId, myPostedFile, fileExtension, strUploadPath);

                string message = "";
                {
                    switch (uploadResult.Error)
                    {
                        case 0:  message = "<h5>Upload Completed</h5>";
                                message += "<ul class='list-group'>";
                                message += "<li class='list-group-item d-flex justify-content-between align-items-center'><span>Total users uploaded <span class='badge badge-success badge-pill'>" + uploadResult.UploadedCount+"</span></li>";
                            if(uploadResult.ExistingCount > 0)
                                message += "<li class='list-group-item d-flex justify-content-between align-items-center'><span>Total number of duplicate users <span class='badge badge-warning badge-pill'>" + uploadResult.ExistingCount + "</span></li>";
                            if(uploadResult.EmptyCount > 0)
                                message += "<li class='list-group-item d-flex justify-content-between align-items-center'><span>Total number of empty rows <span class='badge badge-warning badge-pill'>" + uploadResult.EmptyCount + "</span></li>";
                            if(uploadResult.FailedCount > 0)
                                message += "<li class='list-group-item d-flex justify-content-between align-items-center'><span>Total users failed to upload <span class='badge badge-error badge-pill'>" + uploadResult.FailedCount + "</span></li>";
                            message += "</ul>";
                            break;
                        case 1:
                            message = "<div class='alert alert-danger'>Empty excel, No data to upload</div>";
                            break;
                        case 2:
                            message = "<div class='alert alert-danger'>In-correct file format. Please use the template downladed from the portal.</div>";
                            break;
                    }
                }
                ViewBag.UploadStatus = message;
                return View("ImportAccounts");

                //if (lngRetVal < 0)
                //{
                //    if (ConfigurationManager.AppSettings["Error" + lngRetVal.ToString()] != null)
                //    {
                //        ViewBag.UploadStatus = ConfigurationManager.AppSettings["Error" + lngRetVal.ToString()].ToString();
                //    }
                //    else
                //    {
                //        ViewBag.UploadStatus = "There was an error with this operation : " + lngRetVal.ToString();
                //    }
                //    return View("ImportAccounts");
                //}
                //else if (lngRetVal > 0)
                //{
                //    // Feedback
                //    string msg = "The upload completed successfully.";
                //    msg += "<BR>" + template.intUploadedContactCount.ToString() + " contacts were uploaded to the database.";
                //    if (template.intExistingContactCount > 0)
                //    {
                //        msg += "<BR>" + template.intExistingContactCount.ToString() + " contacts were already in the database.";
                //    }
                //    if (template.intUploadedEmptyContactCount > 0)
                //    {
                //        msg += "<BR>" + template.intUploadedEmptyContactCount.ToString() + " contacts were not containing required info in uploaded file like title or firstname or email. Please check general setting for required details.";
                //    }

                //    ViewBag.UploadStatus = msg;
                //    return View("ImportAccounts");
                //}
                //else
                //{
                //    ViewBag.UploadStatus = "The upload completed successfully.";
                //    return View("ImportAccounts");
                //}
            }
            catch (Exception ex)
            {
                ViewBag.UploadStatus = "<div class='alert alert-danger'>Something went wrong. Please try again later<div>";
                Logger.Error(ex.Message, ex);
                return View("ImportAccounts");
            }
        }
        #endregion
    }
}
