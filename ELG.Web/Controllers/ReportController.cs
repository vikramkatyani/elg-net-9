using Microsoft.AspNetCore.Hosting;
using ELG.Model.OrgAdmin;
using Microsoft.AspNetCore.Http;
using ELG.Web.Helper;
using ELG.DAL.OrgAdminDAL;
using ELG.DAL.Utilities;
using ELG.Model.OrgAdmin;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;

namespace ELG.Web.Controllers
{
    [SessionCheck]
    public class ReportController : Controller
    {
        #region Learning Progress Report
        // GET: Learning Progress Report
        public ActionResult LearningProgress()
        {
            return View();
        }

        [HttpPost]
        //fetch learning progress records
        public ActionResult LoadLearningProgress(LearnerReportFilter searchCriteria)
        {
            ELG.Model.OrgAdmin.CourseProgressReport progressReport = new ELG.Model.OrgAdmin.CourseProgressReport();
            progressReport.ProgressRecords = new List<ELG.Model.OrgAdmin.CourseProgressItem>();
            try
            {
                var reportRep = new ReportRep();

                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;

                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                }

                if (!String.IsNullOrEmpty(Request.Form["From"]))
                    searchCriteria.FromDate = Convert.ToDateTime(Request.Form["From"]);

                if (!String.IsNullOrEmpty(Request.Form["To"]))
                    searchCriteria.ToDate = Convert.ToDateTime(Request.Form["To"]);

                searchCriteria.Draw = Request.Form["draw"];
                searchCriteria.Start = Request.Form["start"];
                searchCriteria.Length = Request.Form["length"];

                //Find Order Column
                var orderColumn = Request.Form["order[0][column]"];
                searchCriteria.SortCol = Request.Form[$"columns[{orderColumn}][name]"];
                searchCriteria.SortColDir = Request.Form["order[0][dir]"];

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                progressReport = reportRep.GetLearningProgressReport(searchCriteria);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = progressReport.TotalRecords, recordsTotal = progressReport.TotalRecords, data = progressReport.ProgressRecords });
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = progressReport.TotalRecords, recordsTotal = progressReport.TotalRecords, data = progressReport.ProgressRecords });
            }
        }

        //download learning progress records
        public ActionResult DownloadLearningProgress(LearnerReportFilter searchCriteria)
        {
            List<ELG.Model.OrgAdmin.DownloadCourseProgressReport> progressReport = new List<ELG.Model.OrgAdmin.DownloadCourseProgressReport>();
            try
            {
                var reportRep = new ReportRep();

                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;

                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                }

                string fromDate = Request.Query["From"].ToString();
                string toDate = Request.Query["To"].ToString();
                if (!String.IsNullOrEmpty(fromDate))
                    searchCriteria.FromDate = Convert.ToDateTime(fromDate);

                if (!String.IsNullOrEmpty(toDate))
                    searchCriteria.ToDate = Convert.ToDateTime(toDate);

                progressReport = reportRep.DownloadLearningProgressReport(searchCriteria);

                DataTable dtReport = CommonHelper.ListToDataTable(progressReport);
                string[] columns = { "FirstName", "LastName", "EmailId", "Location", "Department", "CourseName", "AssignedOn", "LastAccessedOn",  "CourseStatus","CompletionDate" };

                string[] columns_header = { SessionHelper.CompanySettings.strFirstNameDescription, SessionHelper.CompanySettings.strSurnameDescription, SessionHelper.CompanySettings.emailIdDescription, SessionHelper.CompanySettings.strLocationDescription, SessionHelper.CompanySettings.strDepartmentDescription, "Course", "Assigned On", "Last Accessed", "Status", "Completion Date" };

                byte[] filecontent = CommonHelper.ExportExcelWithHeader(dtReport, "Learning Report", false, columns_header, columns);
                return File(filecontent, CommonHelper.ExcelContentType, "LearningReport.xlsx");
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("LearningProgress");
            }
        }

        //update learning progress record
        [HttpPost]
        public ActionResult UpdateLearningProgress(ELG.Model.OrgAdmin.CourseProgressItem record)
        {
            try
            {
                var reportRep = new ReportRep();
                int result = reportRep.UpdateLearningProgress(record);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }
        #endregion

        #region Risk Assessment Report
        // GET: Learning Progress Report
        public ActionResult RiskAssessmentReport()
        {
            return View();
        }

        [HttpPost]
        //fetch risk assessment records
        public ActionResult LoadRiskAssessmentReport(ELG.Model.OrgAdmin.LearnerRAReportFilter searchCriteria)
        {
            CourseRAReport raReport = new CourseRAReport();
            raReport.RiskAssessmentRecords = new List<CourseRAReportItem>();
            try
            {
                var reportRep = new ReportRep();

                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;

                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                }

                if (!String.IsNullOrEmpty(Request.Form["From"]))
                    searchCriteria.FromDate = Convert.ToDateTime(Request.Form["From"]);

                if (!String.IsNullOrEmpty(Request.Form["To"]))
                    searchCriteria.ToDate = Convert.ToDateTime(Request.Form["To"]);

                searchCriteria.Draw = Request.Form["draw"];
                searchCriteria.Start = Request.Form["start"];
                searchCriteria.Length = Request.Form["length"];

                //Find Order Column
                var orderColumn = Request.Form["order[0][column]"];
                searchCriteria.SortCol = Request.Form[$"columns[{orderColumn}][name]"];
                searchCriteria.SortColDir = Request.Form["order[0][dir]"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                raReport = reportRep.GetLearningRAReport(searchCriteria);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = raReport.TotalRecords, recordsTotal = raReport.TotalRecords, data = raReport.RiskAssessmentRecords });
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = raReport.TotalRecords, recordsTotal = raReport.TotalRecords, data = raReport.RiskAssessmentRecords });
            }
        }

        //download Risk Assessment records
        public ActionResult DownloadRiskAssessmentReport(ELG.Model.OrgAdmin.LearnerRAReportFilter searchCriteria)
        {
            List<DownloadRAReport> raReport = new List<DownloadRAReport>();
            try
            {
                var reportRep = new ReportRep();

                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;

                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                }

                string fromDate = Request.Query["From"].ToString();
                string toDate = Request.Query["To"].ToString();
                if (!String.IsNullOrEmpty(fromDate))
                    searchCriteria.FromDate = Convert.ToDateTime(fromDate);

                if (!String.IsNullOrEmpty(toDate))
                    searchCriteria.ToDate = Convert.ToDateTime(toDate);

                raReport = reportRep.DownloadRAReport(searchCriteria);

                DataTable dtReport = CommonHelper.ListToDataTable(raReport);
                string[] columns = { "FirstName", "LastName", "EmailId", "Location", "Department", "CourseName", "AssignedOnDate", "IssueCount", "SignedOff", "SignedOffDate", "Status","CompletionDate" };
                byte[] filecontent = CommonHelper.ExportExcel(dtReport, "Risk Assessment Report", true, columns);
                return File(filecontent, CommonHelper.ExcelContentType, "RiskAssessmentReport.xlsx");
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("RiskAssessmentReport");
            }
        }
        
        //fetch risk assessment responses
        public ActionResult GetLearnerRiskAssessmentResponses([FromBody] RiskAssessmentRequest request)
        {
            List<RiskAssessmentResult> raReport = new List<RiskAssessmentResult>();
            try
            {
                var reportRep = new ReportRep();
                raReport = reportRep.GetLearnerRiskAssessmentResponses(request.RiskAssId);
                var firstItem = raReport.FirstOrDefault();
                string location = firstItem?.LocationName;
                string completionDate = firstItem?.DateCompleted;
                return Json(new { response = raReport, location, completionDate });
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { response = raReport });
            }
        }

        [HttpPost]
        public async Task<ActionResult> SaveAdminRANotes(Int64 ra_id, string adminComments, IFormFile newImageFile)
        {
            try
            {
                var reportRep = new ReportRep();
                int result = reportRep.UpdateLearnerRAStatus(ra_id, false, adminComments, SessionHelper.UserId);

                RAAdminNoteImage evidence = new RAAdminNoteImage();
                evidence.RAId = ra_id;
                await UploadRANoteImage(newImageFile, evidence);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        [HttpPost]
        public async Task<ActionResult> UpdateLearnerRAStatus(Int64 ra_id, string adminComments, IFormFile newImageFile)
        {
            try
            {
                var reportRep = new ReportRep();
                int result = reportRep.UpdateLearnerRAStatus(ra_id, true, adminComments, SessionHelper.UserId);

                RAAdminNoteImage evidence = new RAAdminNoteImage();
                evidence.RAId = ra_id;
                if(newImageFile != null)
                    await UploadRANoteImage(newImageFile, evidence);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        // function to upload image for RA Note
        [HttpPost]
        public async Task<ActionResult> UploadRANoteImage(IFormFile newRANoteFile, RAAdminNoteImage evidence)
        {
            int status = 0;
            try
            {
                IFormFile document = newRANoteFile;

                //validate file size (<= 10MB)  10*1024*1024 = 10485760
                if (document.Length > 10485760)
                {
                    return Json(new { success = "File too large", status });
                }

                var result = await AsyncUploadFile(document, evidence);

                evidence.ImagePath = result;

                var reportRep = new ReportRep();
                status = reportRep.SaveAdminRANoteImage(evidence);
                return Json(new { success = result, status });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = ex.Message, status });
            }

        }
        public static async Task<string> AsyncUploadFile(IFormFile newDocFile, RAAdminNoteImage evidence)

        {
            var connectionString = ELG.Web.Helper.CommonHelper.GetAppSettingValue("AZStorageConnectionString");

            // create a client with the connection
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(ELG.Web.Helper.CommonHelper.GetAppSettingValue("AZAdminRANoteContainer"));

            IFormFile document = newDocFile;
            string filename = "RA_Admin_Note_Image_" + evidence.RAId + Path.GetExtension(newDocFile.FileName);
            string filetype = (newDocFile.ContentType);

            BlobClient blobClient = containerClient.GetBlobClient(filename);

            var blobHttpHeader = new BlobHttpHeaders();

            blobHttpHeader.ContentType = filetype;

            using (var stream = newDocFile.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, blobHttpHeader);
            }

            return blobClient.Uri.AbsoluteUri;
        }
        
        [HttpPost]
        public ActionResult UpdateLearnerRAIssueFollowedUp(Int64 respId, string feedBack)
        {
            try
            {
                var reportRep = new ReportRep();
                int result = reportRep.UpdateLearnerRAIssueFollowedUp(respId, feedBack, SessionHelper.UserId);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }
        #endregion

        #region Classroom Report
        // GET: Learning Progress Report
        public ActionResult ClassroomReport()
        {
            return View();
        }

        [HttpPost]
        //fetch learning progress records
        public ActionResult LoadClassroomReport(ClassroomReportFilter searchCriteria)
        {
            ClassroomProgressReport report = new ClassroomProgressReport();
            report.ClassroomRecords = new List<ClassroomProgressItem>();
            try
            {
                var reportRep = new ReportRep();

                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;

                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                }

                if (!String.IsNullOrEmpty(Request.Form["From"]))
                    searchCriteria.FromDate = Convert.ToDateTime(Request.Form["From"]);

                if (!String.IsNullOrEmpty(Request.Form["To"]))
                    searchCriteria.ToDate = Convert.ToDateTime(Request.Form["To"]);

                searchCriteria.Draw = Request.Form["draw"];
                searchCriteria.Start = Request.Form["start"];
                searchCriteria.Length = Request.Form["length"];

                //Find Order Column
                var orderColumn = Request.Form["order[0][column]"];
                searchCriteria.SortCol = Request.Form[$"columns[{orderColumn}][name]"];
                searchCriteria.SortColDir = Request.Form["order[0][dir]"];

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                report = reportRep.GetClassroomReport(searchCriteria);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = report.TotalRecords, recordsTotal = report.TotalRecords, data = report.ClassroomRecords });
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = report.TotalRecords, recordsTotal = report.TotalRecords, data = report.ClassroomRecords });
            }
        }

        //download classroom records
        public ActionResult DownloadClassroomReport(ClassroomReportFilter searchCriteria)
        {
            List<DownloadClassroomReport> classroomReport = new List<DownloadClassroomReport>();
            try
            {
                var reportRep = new ReportRep();

                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;

                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                }

                string fromDate = Request.Query["From"].ToString();
                string toDate = Request.Query["To"].ToString();
                if (!String.IsNullOrEmpty(fromDate))
                    searchCriteria.FromDate = Convert.ToDateTime(fromDate);

                if (!String.IsNullOrEmpty(toDate))
                    searchCriteria.ToDate = Convert.ToDateTime(toDate);

                classroomReport = reportRep.DownloadClassroomReport(searchCriteria);

                DataTable dtReport = CommonHelper.ListToDataTable(classroomReport);
                string[] columns = { "FirstName", "LastName", "EmployeeNumber", "EmailId", "Location", "Department", "Class", "Venue", "AttendedDate" };
                byte[] filecontent = CommonHelper.ExportExcel(dtReport, "Classroom Report", true, columns);
                return File(filecontent, CommonHelper.ExcelContentType, "ClassroomReport.xlsx");
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("ClassroomReport");
            }
        }

        #endregion

        #region Document Report
        // GET: Document Report
        public ActionResult DocumentReport()
        {
            return View();
        }

        [HttpPost]
        //fetch document records
        public ActionResult LoadDocumentReport(DocumentReportFilter searchCriteria)
        {
            try
            {
                var reportRep = new ReportRep();

                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;

                if (searchCriteria.AssignmentStatus == -1)
                    searchCriteria.AssignmentStatus = null;

                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                }

                if (!String.IsNullOrEmpty(Request.Form["From"]))
                    searchCriteria.FromDate = Convert.ToDateTime(Request.Form["From"]);

                if (!String.IsNullOrEmpty(Request.Form["To"]))
                    searchCriteria.ToDate = Convert.ToDateTime(Request.Form["To"]);

                searchCriteria.Draw = Request.Form["draw"];
                searchCriteria.Start = Request.Form["start"];
                searchCriteria.Length = Request.Form["length"];

                //Find Order Column
                var orderColumn = Request.Form["order[0][column]"];
                searchCriteria.SortCol = Request.Form[$"columns[{orderColumn}][name]"];
                searchCriteria.SortColDir = Request.Form["order[0][dir]"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                DocumentReport report = reportRep.GetDocumentReport(searchCriteria);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = report.TotalRecords, recordsTotal = report.TotalRecords, data = report.DocumentReportRecords, UserRole = SessionHelper.UserRole });
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { });
            }
        }

        //download document records
        public ActionResult DownloadDocumentReport(DocumentReportFilter searchCriteria)
        {
            List<DownloadDocumentReport> docReport = new List<DownloadDocumentReport>();
            try
            {
                var reportRep = new ReportRep();

                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;

                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                }

                string fromDate = Request.Query["FromDate"].ToString();
                string toDate = Request.Query["ToDate"].ToString();
                if (!String.IsNullOrEmpty(fromDate))
                    searchCriteria.FromDate = Convert.ToDateTime(fromDate);

                if (!String.IsNullOrEmpty(toDate))
                    searchCriteria.ToDate = Convert.ToDateTime(toDate);

                docReport = reportRep.DownloadDocumentReport(searchCriteria);

                DataTable dtReport = CommonHelper.ListToDataTable(docReport);
                string[] columns = { "FirstName", "LastName", "EmailId", "Location", "Department", "Category", "File", "Status", "ViewedDate", "StatusUpdatedOn" };
                byte[] filecontent = CommonHelper.ExportExcel(dtReport, "Document Report", true, columns);
                return File(filecontent, CommonHelper.ExcelContentType, "DocumentReport.xlsx");
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("DocumentReport");
            }
        }

        [HttpPost]
        public ActionResult UpdateLearnerDocRecord(DocumentReportItem record)
        {
            try
            {
                var reportRep = new ReportRep();
                int result = reportRep.UpdateLearnerDocRecord(record);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        #endregion

        #region Announcement Report
        // GET: Announcement Report
        public ActionResult AnnouncementReport()
        {
            return View();
        }

        [HttpPost]
        //fetch Announcement records
        public ActionResult LoadAnnouncementReport(AnnouncementReportFilter searchCriteria)
        {
            try
            {
                var reportRep = new ReportRep();

                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;

                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                }

                if (!String.IsNullOrEmpty(Request.Form["From"]))
                    searchCriteria.FromDate = Convert.ToDateTime(Request.Form["From"]);

                if (!String.IsNullOrEmpty(Request.Form["To"]))
                    searchCriteria.ToDate = Convert.ToDateTime(Request.Form["To"]);

                searchCriteria.Draw = Request.Form["draw"].FirstOrDefault();
                searchCriteria.Start = Request.Form["start"].FirstOrDefault();
                searchCriteria.Length = Request.Form["length"].FirstOrDefault();

                //Find Order Column
                searchCriteria.SortCol = Request.Form[$"columns[{Request.Form["order[0][column]"].FirstOrDefault()}][name]"].FirstOrDefault();
                searchCriteria.SortColDir = Request.Form["order[0][dir]"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                AnnouncementReport report = reportRep.GetAnnouncementReport(searchCriteria);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = report.TotalRecords, recordsTotal = report.TotalRecords, data = report.AnnouncementReportRecords });
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { });
            }
        }

        //download classroom records
        public ActionResult DownloadAnnouncementReport(AnnouncementReportFilter searchCriteria)
        {
            List<DownloadAnnouncementReport> announcementReport = new List<DownloadAnnouncementReport>();
            try
            {
                var reportRep = new ReportRep();

                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;

                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                }

                string fromDate = Request.Query["From"].ToString();
                string toDate = Request.Query["To"].ToString();
                if (!String.IsNullOrEmpty(fromDate))
                    searchCriteria.FromDate = Convert.ToDateTime(fromDate);

                if (!String.IsNullOrEmpty(toDate))
                    searchCriteria.ToDate = Convert.ToDateTime(toDate);

                announcementReport = reportRep.DownloadAnnouncementReport(searchCriteria);

                DataTable dtReport = CommonHelper.ListToDataTable(announcementReport);
                string[] columns = { "Title", "FirstName", "LastName", "EmployeeNumber", "EmailId", "Location", "Department", "ViewedOn", "PublishedOn" };
                byte[] filecontent = CommonHelper.ExportExcel(dtReport, "Announcement Report", true, columns);
                return File(filecontent, CommonHelper.ExcelContentType, "AnnouncementReport.xlsx");
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("AnnouncementReport");
            }
        }
        #endregion

        #region Advance Compliancy Report
        // GET: Learning Progress Report
        public ActionResult AdvanceCompliancyReport()
        {
            return View();
        }

        [HttpPost]
        //fetch advance compliancy report
        public ActionResult AdvanceCompliancyReport(AdvanceCompliancyFilter searchCriteria)
        {
            List<AdvanceCompliancyItem> advCompliancy = new List<AdvanceCompliancyItem>();
            var model = new AdvCompliancyViewModel();
            try
            {
                var reportRep = new ReportRep();
                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;

                // get list of courses 
                var commmon = new CommonMethods();
                var courseList = commmon.GetCoursesForOrganisation(Convert.ToInt64(SessionHelper.CompanyId));

                if (searchCriteria.FromDate == null || searchCriteria.ToDate == null)
                {
                    return View("_CompanyAdvanceCompliancy", model);
                }

               else if(searchCriteria.Location > 0)
                {
                    // get list of all departments with in the location
                    var departmentList = commmon.GetDepartmentsForLocation(SessionHelper.UserRole, SessionHelper.UserId, searchCriteria.Location);
                    advCompliancy = reportRep.GetAdvanceCompliancyReportForLocation(searchCriteria);
                    model = new AdvCompliancyViewModel
                    {
                        Courses = courseList,
                        LocationCourseCompliancy =
                        from department in departmentList
                        select new LocationCourseViewModel
                        {
                            LocationID = searchCriteria.Location,
                            LocationName = department.DepartmentName,
                            CompliancyItems =
                                from course in courseList
                                select advCompliancy.FirstOrDefault(x =>
                                    x.CourseID == course.CourseId &&
                                    x.Department == department.DepartmentId
                                )
                        }
                    };
                }
                else
                {
                    // get list of all locations with in the company
                    var locationList = commmon.GetLocationsForCompany(SessionHelper.UserRole, SessionHelper.UserId, SessionHelper.CompanyId);
                    advCompliancy = reportRep.GetAdvanceCompliancyReportForCompany(searchCriteria);
                    model = new AdvCompliancyViewModel
                    {
                        Courses = courseList,
                        LocationCourseCompliancy =
                        from location in locationList
                        select new LocationCourseViewModel
                        {
                            LocationName = location.LocationName,
                            CompliancyItems =
                                from course in courseList
                                select advCompliancy.FirstOrDefault(x =>
                                    x.CourseID == course.CourseId &&
                                    x.Location == location.LocationId
                                )
                        }
                    };
                }
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }

            return View("_CompanyAdvanceCompliancy", model);
        }


        // get report on applied filter
        [HttpPost]
        public ActionResult LoadNonCompliantUserData(ELG.Model.OrgAdmin.DataTableFilter searchCriteria)
        {
            OrganisationLearnerList nonCompliantLearnerList = new OrganisationLearnerList();
            List<LearnerInfo> learnerInfoList = new List<LearnerInfo>();
            try
            {
                var reportRep = new ReportRep();

                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;

                searchCriteria.Draw = Request.Form["draw"].FirstOrDefault();
                searchCriteria.Start = Request.Form["start"].FirstOrDefault();
                searchCriteria.Length = Request.Form["length"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                nonCompliantLearnerList = reportRep.GetNonCompliantUserReport(searchCriteria);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }

            return Json(new { draw = searchCriteria.Draw, recordsFiltered = nonCompliantLearnerList.TotalLearners, recordsTotal = nonCompliantLearnerList.TotalLearners, data = nonCompliantLearnerList.LearnerList });

        }

        // get all learner in department with their progress
        [HttpPost]
        public ActionResult LoadAdvCompliantUserData(LearnerReportFilter searchCriteria)
        {
            ELG.Model.OrgAdmin.CourseProgressReport progressReport = new ELG.Model.OrgAdmin.CourseProgressReport();
            List<ELG.Model.OrgAdmin.CourseProgressItem> progressRecord = new List<ELG.Model.OrgAdmin.CourseProgressItem>();
            try
            {
                var reportRep = new ReportRep();

                searchCriteria.Company = Convert.ToInt64(SessionHelper.CompanyId);

                searchCriteria.Draw = Request.Form["draw"].FirstOrDefault();
                searchCriteria.Start = Request.Form["start"].FirstOrDefault();
                searchCriteria.Length = Request.Form["length"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                if(searchCriteria.Course > 0 && searchCriteria.Location > 0 && searchCriteria.Department > 0)
                progressReport = reportRep.GetAdvanceCompliancyUserReport(searchCriteria);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }

            return Json(new { draw = searchCriteria.Draw, recordsFiltered = progressReport.TotalRecords, recordsTotal = progressReport.TotalRecords, data = progressReport.ProgressRecords });

        }

        #endregion

        #region Overdue Course Report
        // GET: Learning Progress Report
        public ActionResult OverdueCourseReport()
        {
            return View();
        }

        [HttpPost]
        //fetch overdue records
        public ActionResult LoadOverdueCourseReport(LearnerReportFilter searchCriteria)
        {
            OverDueCourseReport progressReport = new OverDueCourseReport();
            progressReport.OverDueCourseRecords = new List<OverDueCourseItem>();
            try
            {
                var reportRep = new ReportRep();

                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;

                if (!String.IsNullOrEmpty(Request.Form["From"].FirstOrDefault()))
                    searchCriteria.FromDate = Convert.ToDateTime(Request.Form["From"].FirstOrDefault());

                if (!String.IsNullOrEmpty(Request.Form["From"].FirstOrDefault()))
                    searchCriteria.ToDate = Convert.ToDateTime(Request.Form["To"].FirstOrDefault());

                searchCriteria.Draw = Request.Form["draw"].FirstOrDefault();
                searchCriteria.Start = Request.Form["start"].FirstOrDefault();
                searchCriteria.Length = Request.Form["length"].FirstOrDefault();

                //Find Order Column
                searchCriteria.SortCol = Request.Form[$"columns[{Request.Form["order[0][column]"].FirstOrDefault()}][name]"].FirstOrDefault();
                searchCriteria.SortColDir = Request.Form["order[0][dir]"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                progressReport = reportRep.GetOverdueReport(searchCriteria);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = progressReport.TotalRecords, recordsTotal = progressReport.TotalRecords, data = progressReport.OverDueCourseRecords });
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = progressReport.TotalRecords, recordsTotal = progressReport.TotalRecords, data = progressReport.OverDueCourseRecords });
            }
        }


        //download overdue records
        public ActionResult DownloadOverDue(LearnerReportFilter searchCriteria)
        {
            List<OverDueCourseDownloadItem> overdueReport = new List<OverDueCourseDownloadItem>();
            try
            {
                var reportRep = new ReportRep();

                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;

                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                }

                string fromDate = Request.Query["From"].ToString();
                string toDate = Request.Query["To"].ToString();
                if (!String.IsNullOrEmpty(fromDate))
                    searchCriteria.FromDate = Convert.ToDateTime(fromDate);

                if (!String.IsNullOrEmpty(toDate))
                    searchCriteria.ToDate = Convert.ToDateTime(toDate);

                overdueReport = reportRep.DownloadOverDueReport(searchCriteria);

                DataTable dtReport = CommonHelper.ListToDataTable(overdueReport);
                string[] columns = { "FirstName", "LastName", "EmailId", "CourseName", "AssignedOn", "OverDueDate", "CompletionDate" };
                byte[] filecontent = CommonHelper.ExportExcel(dtReport, "Overdue Report", true, columns);
                return File(filecontent, CommonHelper.ExcelContentType, "OverdueReport.xlsx");
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("LearningProgress");
            }
        }

        #endregion

        #region Learning Progress Report For Manual Notification
        // GET: Learning Progress Report
        public ActionResult LearningProgressNotification()
        {
            return View();
        }

        [HttpPost]
        //fetch learning progress records
        public ActionResult LoadLearningProgressNotification(LearnerReportFilter searchCriteria)
        {
            CourseProgressReportForManualReminder progressReport = new CourseProgressReportForManualReminder();
            progressReport.ProgressRecords = new List<CourseProgressItemForManualReminder>();
            try
            {
                var reportRep = new ReportRep();

                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;

                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                }

                //if (!String.IsNullOrEmpty(Request.Form.GetValues("From").FirstOrDefault()))
                //    searchCriteria.FromDate = Convert.ToDateTime(Request.Form.GetValues("From").FirstOrDefault());

                //if (!String.IsNullOrEmpty(Request.Form.GetValues("From").FirstOrDefault()))
                //    searchCriteria.ToDate = Convert.ToDateTime(Request.Form.GetValues("To").FirstOrDefault());

                searchCriteria.Draw = Request.Form["draw"].FirstOrDefault();
                searchCriteria.Start = Request.Form["start"].FirstOrDefault();
                searchCriteria.Length = Request.Form["length"].FirstOrDefault();

                //Find Order Column
                searchCriteria.SortCol = Request.Form[$"columns[{Request.Form["order[0][column]"].FirstOrDefault()}][name]"].FirstOrDefault();
                searchCriteria.SortColDir = Request.Form["order[0][dir]"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                progressReport = reportRep.GetLearningProgressReportForManualNotification(searchCriteria);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = progressReport.TotalRecords, recordsTotal = progressReport.TotalRecords, data = progressReport.ProgressRecords });
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = progressReport.TotalRecords, recordsTotal = progressReport.TotalRecords, data = progressReport.ProgressRecords });
            }
        }

        private readonly IWebHostEnvironment _env;

        public ReportController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost]
        public ActionResult SendReminderNotificationToLearner(Int64[] selectedRecordList)
        {
            try
            {
                EmailUtility emailUti = new EmailUtility(_env);
                int result = 0;
                var reportRep = new ReportRep();

                string selectedRecords = "";

                // if few are selected
                if (selectedRecordList != null && selectedRecordList.Length > 0)
                {
                    selectedRecords = string.Join(",", selectedRecordList);
                }

                ELG.Model.OrgAdmin.CourseProgressReport progressReport = new ELG.Model.OrgAdmin.CourseProgressReport();
                progressReport.ProgressRecords = new List<ELG.Model.OrgAdmin.CourseProgressItem>();

                progressReport = reportRep.GetLearningProgressRecordsForManualNotification(SessionHelper.CompanyId, selectedRecords);

                //send email to each record
                if(progressReport.TotalRecords > 0)
                {
                    foreach (var record in progressReport.ProgressRecords)
                    {
                        try
                        {
                            string emailTemplate = emailUti.GetEmailTemplate("ReminderNotificationEmail.html");
                            emailTemplate = emailTemplate.Replace("{username}", record.FirstName + " " + record.LastName);
                            emailTemplate = emailTemplate.Replace("{coursename}", record.CourseName);
                            emailTemplate = emailTemplate.Replace("{overduedate}", record.CompletionDate);
                            emailUti.SendEmailUsingSendGrid(record.EmailId, "eLearning Gate - Course Completion Reminder", emailTemplate);
                            //emailUti.SendEMail(record.EmailId, "eLearning Gate - Course Completion Reminder", emailTemplate);
                            result++;

                            //log email sending
                            int log = reportRep.CreateCourseReminderLog(record.UserID, record.Course, SessionHelper.UserId);

                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex.Message, ex);
                        }
                    }
                }

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }


        #endregion

        #region Accident/Incident Report
        // GET: Learning Progress Report
        public ActionResult AccidentIncident()
        {
            return View();
        }

        [HttpPost]
        //fetch risk assessment records
        public ActionResult LoadAccidentIncidentReport(AccidentIncidentReportFilter searchCriteria)
        {
            try
            {
                var accidentRep = new AccidentIncidentRep();

                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;

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

                ReportedIncidentsList incidentList = accidentRep.GetReportedIncidentsInOrg(searchCriteria);

                return Json(new { draw = searchCriteria.Draw, recordsFiltered = incidentList.TotalIncidents, recordsTotal = incidentList.TotalIncidents, data = incidentList.IncidentList });

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("List");
            }
        }

        //download Risk Assessment records
        public ActionResult DownloadAccidentIncidentReport(AccidentIncidentReportFilter searchCriteria)
        {
            List<DownloadAccidentIncidentResponse> incidentList = new List<DownloadAccidentIncidentResponse>();
            try
            {
                var accidentRep = new AccidentIncidentRep();

                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;

                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                }

                incidentList = accidentRep.DownloadReportedIncidentsInOrg(searchCriteria);

                DataTable dtReport = CommonHelper.ListToDataTable(incidentList);
                string[] columns = { "Title", "ReportedBy", "IncidentOn", "ReportedOn", "Injured", "CreatedBy", "Employee", "Reportable", "SignedOff", "SignedOffDate", "Comment" };
                byte[] filecontent = CommonHelper.ExportExcel(dtReport, "Incident Report", true, columns);
                return File(filecontent, CommonHelper.ExcelContentType, "IncidentReport.xlsx");

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("AccidentIncident");
            }
        }

        [HttpPost]
        public ActionResult SignOffAccidentIncident(Int64 aiid, string adminComments)
        {
            try
            {
                var accidentRep = new AccidentIncidentRep();
                int result = accidentRep.SignOffAccidentIncident(aiid, adminComments, SessionHelper.UserId);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }
        #endregion

        #region Course Autoallocation Report
        public ActionResult AutoAllocationReport()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LoadAutoAllocationReport(ELG.Model.OrgAdmin.DataTableFilter searchCriteria)
        {
            CourseAutoAllocationReport allocationReport = new CourseAutoAllocationReport();
            allocationReport.AllocationRecords = new List<CourseAutoAllocationItem>();
            try
            {
                var reportRep = new ReportRep();

                searchCriteria.Company = SessionHelper.CompanyId;

                searchCriteria.Draw = Request.Form["draw"].FirstOrDefault();
                searchCriteria.Start = Request.Form["start"].FirstOrDefault();
                searchCriteria.Length = Request.Form["length"].FirstOrDefault();

                //Find Order Column
                var orderColIndex = Request.Form["order[0][column]"].FirstOrDefault();
                if (!string.IsNullOrEmpty(orderColIndex))
                {
                    searchCriteria.SortCol = Request.Form[$"columns[{orderColIndex}][name]"].FirstOrDefault();
                }
                else
                {
                    searchCriteria.SortCol = null;
                }
                searchCriteria.SortColDir = Request.Form["order[0][dir]"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                allocationReport = reportRep.GetCourseAutoAllocationReport(searchCriteria);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = allocationReport.TotalRecords, recordsTotal = allocationReport.TotalRecords, data = allocationReport.AllocationRecords });
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = allocationReport.TotalRecords, recordsTotal = allocationReport.TotalRecords, data = allocationReport.AllocationRecords });
            }
        }

        //download learning progress records
        public ActionResult DownloadAutoAllocationReport(ELG.Model.OrgAdmin.DataTableFilter searchCriteria)
        {
            List<CourseAutoAllocationDownloadItem> progressReport = new List<CourseAutoAllocationDownloadItem>();
            try
            {
                var reportRep = new ReportRep();

                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;

                progressReport = reportRep.DownloadCourseAutoAllocationReport(searchCriteria);

                DataTable dtReport = CommonHelper.ListToDataTable(progressReport);
                string[] columns = { "Course", "Location", "Department", "AutoAllocation" };
                byte[] filecontent = CommonHelper.ExportExcel(dtReport, "Auto Allocation Report", true, columns);
                return File(filecontent, CommonHelper.ExcelContentType, "AutoAllocationReport.xlsx");
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("LearningProgress");
            }
        }
        #endregion

        #region Training Card

        public ActionResult TrainingCard()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LoadTrainingCard(ELG.Model.OrgAdmin.DataTableFilter searchCriteria)
        {
            TrainingCard trainingCard = new TrainingCard();
            //trainingCard.Trainee = new TraineeInfo();
            //trainingCard.TestResult = new List<TraineeTestResult>();
            //trainingCard.DocResult = new List<TraineeDocResult>();
            try
            {
                var reportRep = new ReportRep();

                searchCriteria.Company = SessionHelper.CompanyId;

                trainingCard = reportRep.GetTrainingCard(searchCriteria);


                if (trainingCard.Trainee.TraineePic != null)
                {
                    string learnerProfileBase64Data = Convert.ToBase64String(trainingCard.Trainee.TraineePic);
                    trainingCard.Trainee.TraineePicURL = string.Format("data:image/png;base64,{0}", learnerProfileBase64Data);
                }
                else
                {
                    trainingCard.Trainee.TraineePicURL = "../Content/img/no-pic.png";
                }

                return Json(new { trainingCard });
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { trainingCard });
            }
        }
        #endregion

        #region Summary

        public ActionResult Summary()
        {
            return View();
        }

        public ActionResult LoadLearningStatistics()
        {
            List<CourseLearningStatistics> stats = new List<CourseLearningStatistics>();
            try
            {
                var reportRep = new ReportRep();

                stats = reportRep.GetLearningStatistics(SessionHelper.CompanyId);
                return Json(new { stats });
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { });
            }
        }
        public ActionResult LoadLearningStatisticsForLocations(int course)
        {
            List<CourseLearningStatistics_perLocation> stats = new List<CourseLearningStatistics_perLocation>();
            try
            {
                var reportRep = new ReportRep();

                stats = reportRep.GetLearningStatistics_perLocation(SessionHelper.CompanyId, course);
                return Json(new { stats });
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { });
            }
        }
        public ActionResult LoadLearningStatisticsForDepartments(int course, int location)
        {
            List<CourseLearningStatistics_perDepartment> stats = new List<CourseLearningStatistics_perDepartment>();
            try
            {
                var reportRep = new ReportRep();

                stats = reportRep.GetLearningStatistics_perDepartment(location, course);
                return Json(new { stats });
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { });
            }
        }
        #endregion

        #region widget report

        public ActionResult WidgetProgress()
        {
            return View();
        }

        [HttpPost]
        //fetch widget progress records
        public ActionResult LoadWidgetProgress(LearnerReportFilter searchCriteria)
        {
            WidgeteProgressReport progressReport = new WidgeteProgressReport();
            progressReport.ProgressRecords = new List<WidgetProgressItem>();
            try
            {
                var reportRep = new ReportRep();

                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;

                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                }

                //if (!String.IsNullOrEmpty(Request.Form.GetValues("From").FirstOrDefault()))
                //    searchCriteria.FromDate = Convert.ToDateTime(Request.Form.GetValues("From").FirstOrDefault());

                //if (!String.IsNullOrEmpty(Request.Form.GetValues("From").FirstOrDefault()))
                //    searchCriteria.ToDate = Convert.ToDateTime(Request.Form.GetValues("To").FirstOrDefault());

                searchCriteria.Draw = Request.Form["draw"].FirstOrDefault();
                searchCriteria.Start = Request.Form["start"].FirstOrDefault();
                searchCriteria.Length = Request.Form["length"].FirstOrDefault();

                //Find Order Column
                searchCriteria.SortCol = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                searchCriteria.SortColDir = Request.Form["order[0][dir]"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                progressReport = reportRep.GetWidgetReport(searchCriteria);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = progressReport.TotalRecords, recordsTotal = progressReport.TotalRecords, data = progressReport.ProgressRecords });
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = progressReport.TotalRecords, recordsTotal = progressReport.TotalRecords, data = progressReport.ProgressRecords });
            }
        }

        //download widget progress records
        public ActionResult DownloadWidgetProgress(LearnerReportFilter searchCriteria)
        {
            List<WidgetProgressItem> progressReport = new List<WidgetProgressItem>();
            try
            {
                var reportRep = new ReportRep();

                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;

                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                }

                //string fromDate = Request.QueryString["From"].ToString();
                //string toDate = Request.QueryString["To"].ToString();
                //if (!String.IsNullOrEmpty(fromDate))
                //    searchCriteria.FromDate = Convert.ToDateTime(fromDate);

                //if (!String.IsNullOrEmpty(toDate))
                //    searchCriteria.ToDate = Convert.ToDateTime(toDate);

                progressReport = reportRep.DownloadWidgetReport(searchCriteria);

                DataTable dtReport = CommonHelper.ListToDataTable(progressReport);
                string[] columns = { "FirstName", "LastName", "EmailId", "Location", "Department", "CourseName", "AssignedOn", "LastAccessedOn", "CourseStatus", "CompletionDate" };

                string[] columns_header = { SessionHelper.CompanySettings.strFirstNameDescription, SessionHelper.CompanySettings.strSurnameDescription, SessionHelper.CompanySettings.emailIdDescription, SessionHelper.CompanySettings.strLocationDescription, SessionHelper.CompanySettings.strDepartmentDescription, "Course", "Assigned On", "Last Accessed", "Status", "Completion Date" };

                byte[] filecontent = CommonHelper.ExportExcelWithHeader(dtReport, "Widget Progress Report", false, columns_header, columns);
                return File(filecontent, CommonHelper.ExcelContentType, "WidgetReport.xlsx");
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("WidgetProgress");
            }
        }
        #endregion

        #region Historic Learning Progress Report
        // GET: Learning Progress Report
        public ActionResult ArchiveReport()
        {
            var fileList = new List<string>();

            string connectionString = ELG.Web.Helper.CommonHelper.GetAppSettingValue("AZStorageConnectionString");
            string containerName = ELG.Web.Helper.CommonHelper.GetAppSettingValue("AZStorageDocumentContainer");
            //string folderName = "0DDB5ACD-5E46-421A-A464-682E13E300CD-archive-data-report/";// Convert.ToString(SessionHelper.CompanyId) + "-archive-data-report/";

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            //// List blobs with folder prefix
            //foreach (BlobItem blobItem in containerClient.GetBlobs(prefix: folderName))
            //{
            //    string fileName = blobItem.Name.Substring(folderName.Length);
            //    fileList.Add(fileName);
            //}
            foreach (BlobItem blobItem in containerClient.GetBlobs())
            {
                string fileName = blobItem.Name;
                fileList.Add(fileName);
            }


            return View(fileList);
        }
        [HttpPost]
        //public JsonResult GetExcelData(string fileName)
        //{
        //    string connectionString = ConfigurationManager.AppSettings["AZStorageConnectionString"];
        //    string containerName = ConfigurationManager.AppSettings["AZStorageDocumentContainer"];

        //    var blobServiceClient = new BlobServiceClient(connectionString);
        //    var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        //    var blobClient = containerClient.GetBlobClient(fileName);

        //    using (var stream = new MemoryStream())
        //    {
        //        blobClient.DownloadTo(stream);
        //        stream.Position = 0;

        //        using (var package = new OfficeOpenXml.ExcelPackage(stream))
        //        {
        //            var worksheet = package.Workbook.Worksheets.First();
        //            var rowCount = worksheet.Dimension.Rows;
        //            var colCount = worksheet.Dimension.Columns;

        //            var columns = new List<string>();
        //            for (int col = 1; col <= colCount; col++)
        //                columns.Add(worksheet.Cells[1, col].Text);

        //            var rows = new List<List<string>>();
        //            for (int row = 2; row <= rowCount; row++)
        //            {
        //                var rowData = new List<string>();
        //                for (int col = 1; col <= colCount; col++)
        //                    rowData.Add(worksheet.Cells[row, col].Text);
        //                rows.Add(rowData);
        //            }

        //            return Json(new { columns, rows });
        //        }
        //    }
        //}
        public JsonResult GetExcelData(string fileName)
        {
            string connectionString = ELG.Web.Helper.CommonHelper.GetAppSettingValue("AZStorageConnectionString");
            string containerName = ELG.Web.Helper.CommonHelper.GetAppSettingValue("AZStorageDocumentContainer");
            //string folderName = "0DDB5ACD-5E46-421A-A464-682E13E300CD-archive-data-report/";

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            //var blobClient = containerClient.GetBlobClient(folderName + fileName);
            var blobClient = containerClient.GetBlobClient(fileName);

            var dataTable = new DataTable();

            using (var stream = new MemoryStream())
            {
                blobClient.DownloadTo(stream);
                stream.Position = 0;

                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheets.FirstOrDefault();
                    if (worksheet != null)
                    {
                        var firstRowUsed = worksheet.FirstRowUsed();
                        var columnCount = firstRowUsed.CellsUsed().Count();

                        // Add columns from header row
                        foreach (var cell in firstRowUsed.CellsUsed())
                        {
                            dataTable.Columns.Add(cell.GetString());
                        }

                        // Add data rows
                        foreach (var row in worksheet.RowsUsed().Skip(1))
                        {
                            var dataRow = dataTable.NewRow();
                            for (int i = 0; i < columnCount; i++)
                            {
                                dataRow[i] = row.Cell(i + 1).GetValue<string>();
                            }
                            dataTable.Rows.Add(dataRow);
                        }
                    }
                }
            }

            var jsonData = new
            {
                columns = dataTable.Columns.Cast<DataColumn>().Select(c => new { title = c.ColumnName }),
                data = dataTable.Rows.Cast<DataRow>().Select(r => r.ItemArray)
            };

            return Json(jsonData);
        }

        public ActionResult HistoricRecords()
        {
            return View();
        }

        [HttpPost]
        //fetch learning progress records
        public ActionResult LoadHistoricLearningProgress(LearnerReportFilter searchCriteria)
        {
            ELG.Model.OrgAdmin.CourseProgressReport progressReport = new ELG.Model.OrgAdmin.CourseProgressReport();
            progressReport.ProgressRecords = new List<ELG.Model.OrgAdmin.CourseProgressItem>();
            try
            {
                var reportRep = new ReportRep();

                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;

                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                }

                if (!String.IsNullOrEmpty(Request.Form["From"].FirstOrDefault()))
                    searchCriteria.FromDate = Convert.ToDateTime(Request.Form["From"].FirstOrDefault());

                if (!String.IsNullOrEmpty(Request.Form["From"].FirstOrDefault()))
                    searchCriteria.ToDate = Convert.ToDateTime(Request.Form["To"].FirstOrDefault());

                searchCriteria.Draw = Request.Form["draw"].FirstOrDefault();
                searchCriteria.Start = Request.Form["start"].FirstOrDefault();
                searchCriteria.Length = Request.Form["length"].FirstOrDefault();

                //Find Order Column
                searchCriteria.SortCol = Request.Form[$"columns[{Request.Form["order[0][column]"].FirstOrDefault()}][name]"].FirstOrDefault();
                searchCriteria.SortColDir = Request.Form["order[0][dir]"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                progressReport = reportRep.GetLearningProgressReport_Historic(searchCriteria);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = progressReport.TotalRecords, recordsTotal = progressReport.TotalRecords, data = progressReport.ProgressRecords });
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = progressReport.TotalRecords, recordsTotal = progressReport.TotalRecords, data = progressReport.ProgressRecords });
            }
        }

        //download learning progress records
        public ActionResult DownloadHistoricRecords(LearnerReportFilter searchCriteria)
        {
            List<DownloadCourseProgressReport_Historic> progressReport = new List<DownloadCourseProgressReport_Historic>();
            try
            {
                var reportRep = new ReportRep();

                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;

                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                }

                string fromDate = Request.Query["From"].ToString();
                string toDate = Request.Query["To"].ToString();
                if (!String.IsNullOrEmpty(fromDate))
                    searchCriteria.FromDate = Convert.ToDateTime(fromDate);

                if (!String.IsNullOrEmpty(toDate))
                    searchCriteria.ToDate = Convert.ToDateTime(toDate);

                progressReport = reportRep.DownloadLearningProgressReport_historic(searchCriteria);

                DataTable dtReport = CommonHelper.ListToDataTable(progressReport);
                string[] columns = { "FirstName", "LastName", "EmailId", "Location", "Department", "CourseName", "AssignedOn", "LastAccessedOn", "CourseStatus", "CompletionDate", "MovedToHistoryOn" };

                string[] columns_header = { SessionHelper.CompanySettings.strFirstNameDescription, SessionHelper.CompanySettings.strSurnameDescription, SessionHelper.CompanySettings.emailIdDescription, SessionHelper.CompanySettings.strLocationDescription, SessionHelper.CompanySettings.strDepartmentDescription, "Course", "Assigned On", "Last Accessed", "Status", "Completion Date", "Moved to history on" };

                byte[] filecontent = CommonHelper.ExportExcelWithHeader(dtReport, "Historic Report", false, columns_header, columns);
                return File(filecontent, CommonHelper.ExcelContentType, "HistoricReport.xlsx");
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("HistoricRecords");
            }
        }

        #endregion

        #region Sub-Module Progress Report
        // GET: Sub-module Learning Progress Report
        public ActionResult SubModuleProgress()
        {
            return View();
        }

        [HttpPost]
        //fetch learning progress records
        public ActionResult LoadSubModuleProgress(LearnerSubModuleReportFilter searchCriteria)
        {
            CourseSubModuleProgressReport progressReport = new CourseSubModuleProgressReport();
            progressReport.ProgressRecords = new List<CourseSubModuleProgressItem>();
            try
            {
                var reportRep = new ReportRep();

                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;

                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                }

                if (!String.IsNullOrEmpty(Request.Form["From"].FirstOrDefault()))
                    searchCriteria.FromDate = Convert.ToDateTime(Request.Form["From"].FirstOrDefault());

                if (!String.IsNullOrEmpty(Request.Form["From"].FirstOrDefault()))
                    searchCriteria.ToDate = Convert.ToDateTime(Request.Form["To"].FirstOrDefault());

                searchCriteria.Draw = Request.Form["draw"].FirstOrDefault();
                searchCriteria.Start = Request.Form["start"].FirstOrDefault();
                searchCriteria.Length = Request.Form["length"].FirstOrDefault();

                //Find Order Column
                searchCriteria.SortCol = Request.Form[$"columns[{Request.Form["order[0][column]"].FirstOrDefault()}][name]"].FirstOrDefault();
                searchCriteria.SortColDir = Request.Form["order[0][dir]"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                progressReport = reportRep.GetSubModuleProgressReport(searchCriteria);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = progressReport.TotalRecords, recordsTotal = progressReport.TotalRecords, data = progressReport.ProgressRecords });
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = progressReport.TotalRecords, recordsTotal = progressReport.TotalRecords, data = progressReport.ProgressRecords });
            }
        }

        //download learning progress records
        public ActionResult DownloadSubModuleLearningProgress(LearnerReportFilter searchCriteria)
        {
            List<ELG.Model.OrgAdmin.DownloadCourseProgressReport> progressReport = new List<ELG.Model.OrgAdmin.DownloadCourseProgressReport>();
            try
            {
                var reportRep = new ReportRep();

                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;

                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                }

                string fromDate = Request.Query["From"].ToString();
                string toDate = Request.Query["To"].ToString();
                if (!String.IsNullOrEmpty(fromDate))
                    searchCriteria.FromDate = Convert.ToDateTime(fromDate);

                if (!String.IsNullOrEmpty(toDate))
                    searchCriteria.ToDate = Convert.ToDateTime(toDate);

                progressReport = reportRep.DownloadLearningProgressReport(searchCriteria);

                DataTable dtReport = CommonHelper.ListToDataTable(progressReport);
                string[] columns = { "FirstName", "LastName", "EmailId", "Location", "Department", "CourseName", "AssignedOn", "LastAccessedOn", "CourseStatus", "CompletionDate" };

                string[] columns_header = { SessionHelper.CompanySettings.strFirstNameDescription, SessionHelper.CompanySettings.strSurnameDescription, SessionHelper.CompanySettings.emailIdDescription, SessionHelper.CompanySettings.strLocationDescription, SessionHelper.CompanySettings.strDepartmentDescription, "Course", "Assigned On", "Last Accessed", "Status", "Completion Date" };

                byte[] filecontent = CommonHelper.ExportExcelWithHeader(dtReport, "Learning Report", false, columns_header, columns);
                return File(filecontent, CommonHelper.ExcelContentType, "LearningReport.xlsx");
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("LearningProgress");
            }
        }
        #endregion
    }

    // Request model for GetLearnerRiskAssessmentResponses
    public class RiskAssessmentRequest
    {
        public Int64 RiskAssId { get; set; }
    }
}