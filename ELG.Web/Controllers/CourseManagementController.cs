using Microsoft.AspNetCore.Hosting;
using ELG.Model.OrgAdmin;
using Microsoft.AspNetCore.Http;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ELG.Web.Helper;
using ELG.Web.Models;
using ELG.DAL.OrgAdminDAL;
using ELG.DAL.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ELG.Web.Controllers
{
    [SessionCheck]
    public class CourseManagementController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public CourseManagementController(IWebHostEnvironment env)
        {
            _env = env;
        }
        #region allocate license and module access
        // GET: list of all learners to whom the selected module is not assigned
        public ActionResult Enrollment()
        {
            return View("Enrollment");
        }

        // Search risk assessments for mapping (organisation admin)
        [HttpPost]
        public ActionResult SearchRiskAssessments(long courseId, string searchText)
        {
            try
            {
                var moduleRep = new ModuleRep();
                DataTableFilter filter = new DataTableFilter();
                filter.Company = SessionHelper.CompanyId;
                filter.SearchText = searchText ?? String.Empty;
                filter.Course = courseId;

                // Get RA modules from moduleRep (should return RA list with info whether mapped)
                var raList = moduleRep.GetRiskAssessmentSubModulesForCourse(filter);

                return Json(new { data = raList.ModuleList });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { data = new List<object>() });
            }
        }

        // Map RA to course by creating a submodule entry
        [HttpPost]
        public ActionResult MapRiskAssessmentToCourse(long courseId, long raId)
        {
            try
            {
                // fetch RA info
                var moduleRep = new ModuleRep();
                // ra info 
                var ra = moduleRep.GetCourseDetails(raId);
                if (ra == null)
                    return Json(new { success = 0 });

                // ensure not already mapped
                var existing = moduleRep.GetCourseSubModules(courseId).Any(s => s.RAID == ra.ModuleID);
                if (existing)
                    return Json(new { success = 0 });

                SubModule sm = new SubModule();
                sm.CourseId = courseId;
                sm.SubModuleName = ra.ModuleName;
                sm.SubModuleDesc = ra.ModuleDesc;
                sm.SubModulePath = string.Empty;
                sm.RAID = raId;
                sm.CreatedById = SessionHelper.UserId;

                int res = moduleRep.CreateNewSubModule(sm);
                return Json(new { success = res });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        // get report on applied filter
        [HttpPost]
        public ActionResult LoadLearnerDataToAllocateModule(DataTableFilter searchCriteria)
        {
            OrganisationLearnerList learnerList = new OrganisationLearnerList();
            learnerList.LearnerList = new List<LearnerInfo>();
            try
            {
                var moduleRep = new ModuleRep();

                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;

                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                    searchCriteria.Location = 0;
                    searchCriteria.Department = 0;
                }

                searchCriteria.Draw = Request.Form["draw"].FirstOrDefault();
                searchCriteria.Start = Request.Form["start"].FirstOrDefault();
                searchCriteria.Length = Request.Form["length"].FirstOrDefault();
                //Find Order Column
                var orderColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
                searchCriteria.SortCol = Request.Form[$"columns[{orderColumnIndex}][name]"].FirstOrDefault();
                searchCriteria.SortColDir = Request.Form["order[0][dir]"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                if (searchCriteria.Course > 0)
                    learnerList = moduleRep.GetUsersWithoutModule(searchCriteria);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = learnerList.TotalLearners, recordsTotal = learnerList.TotalLearners, data = learnerList.LearnerList });

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = learnerList.TotalLearners, recordsTotal = learnerList.TotalLearners, data = learnerList.LearnerList });
            }
        }

        // reassign Module Access
        [HttpPost]
        public ActionResult ReassignLearnerModuleAccess(LearnerModuleFilter searchCriteria)
        {
            try
            {
                var moduleRep = new ModuleRep();
                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;
                int result = moduleRep.ReAssignModuleAccess(searchCriteria);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        // reassign Module Access
        [HttpPost]
        public ActionResult AllocateModuleLicenseToLearner([FromBody] LearnerModuleFilter searchCriteria)
        {
            try
            {
                var moduleRep = new ModuleRep();
                searchCriteria.Company = Convert.ToInt64(SessionHelper.CompanyId);
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;
                int result;

                if(SessionHelper.UserRole == 6 || SessionHelper.CompanyId == 1)
                    result = moduleRep.AllocateModuleLicenseToResellerLearner(searchCriteria);
                else
                    result = moduleRep.AllocateModuleLicenseToLearner(searchCriteria);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }


        [HttpPost]
        public ActionResult AllocateModuleLicenseToLearner_Multiple(LearnerModuleFilter searchCriteria, bool allSelected, long[] selectedUserList, long[] unselectedUserList)
        {
            try
            {
                int result = 0;
                var moduleRep = new ModuleRep();
                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;

                string selectedLearners = "";
                string unSelectedLearners = "";

                if (allSelected)
                {
                    //remove unselected users
                    if(unselectedUserList != null && unselectedUserList.Length > 0)
                    {
                        unSelectedLearners = string.Join(",", unselectedUserList);
                    }
                }
                else
                {
                    // if few are selected
                    if(selectedUserList != null && selectedUserList.Length > 0)
                    {
                        selectedLearners = string.Join(",", selectedUserList);
                    }

                }
                result = moduleRep.AllocateModuleLicenseToLearner_All(searchCriteria,allSelected,selectedLearners,unSelectedLearners);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        // get list of users with started modules(consumed license); on applied filter
        [HttpPost]
        public ActionResult LoadConsumedLicenseLearnerData(DataTableFilter searchCriteria)
        {
            OrganisationLearnerList learnerList = new OrganisationLearnerList();
            learnerList.LearnerList = new List<LearnerInfo>();
            try
            {
                var moduleRep = new ModuleRep();

                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;

                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                    searchCriteria.Location = 0;
                    searchCriteria.Department = 0;
                }

                searchCriteria.Draw = Request.Form["draw"].FirstOrDefault();
                searchCriteria.Start = Request.Form["start"].FirstOrDefault();
                searchCriteria.Length = Request.Form["length"].FirstOrDefault();

                //Find Order Column
                var orderIndex = Request.Form["order[0][column]"].FirstOrDefault();
                searchCriteria.SortCol = Request.Form[$"columns[{orderIndex}][name]"] .FirstOrDefault();
                searchCriteria.SortColDir = Request.Form["order[0][dir]"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                learnerList = moduleRep.GetLearnerWithConsumedLicense(searchCriteria);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = learnerList.TotalLearners, recordsTotal = learnerList.TotalLearners, data = learnerList.LearnerList });
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = learnerList.TotalLearners, recordsTotal = learnerList.TotalLearners, data = learnerList.LearnerList });
            }
        }
        #endregion

        #region revoke module access
        // GET: list of all learners to whom the selected module is not assigned
        public ActionResult RestictAccess()
        {
            return View("RestictAccess");
        }

        // get report on applied filter
        [HttpPost]
        public ActionResult LoadLearnerDataToRevokeModule(DataTableFilter searchCriteria)
        {
            OrganisationLearnerList learnerList = new OrganisationLearnerList();
            learnerList.LearnerList = new List<LearnerInfo>();
            try
            {
                var moduleRep = new ModuleRep();

                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;
                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                    searchCriteria.Location = 0;
                    searchCriteria.Department = 0;
                }

                searchCriteria.Draw = Request.Form["draw"].FirstOrDefault();
                searchCriteria.Start = Request.Form["start"].FirstOrDefault();
                searchCriteria.Length = Request.Form["length"].FirstOrDefault();
                //Find Order Column
                var orderIndex = Request.Form["order[0][column]"].FirstOrDefault();
                searchCriteria.SortCol = Request.Form[$"columns[{orderIndex}][name]"].FirstOrDefault();
                searchCriteria.SortColDir = Request.Form["order[0][dir]"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                if(searchCriteria.Course > 0)
                    learnerList = moduleRep.GetUsersWithModule(searchCriteria);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = learnerList.TotalLearners, recordsTotal = learnerList.TotalLearners, data = learnerList.LearnerList });

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = learnerList.TotalLearners, recordsTotal = learnerList.TotalLearners, data = learnerList.LearnerList });
            }
        }


        // revoke Module Access
        [HttpPost]
        public ActionResult RevokeLearnerModuleAccess(LearnerModuleFilter searchCriteria)
        {
            try
            {
                var moduleRep = new ModuleRep();
                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;
                int result = moduleRep.RevokeModuleAccess(searchCriteria);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        [HttpPost]
        public ActionResult RevokeLearnerModuleAccess_Multiple(LearnerModuleFilter searchCriteria, bool allSelected, long[] selectedUserList, long[] unselectedUserList)
        {
            try
            {
                int result = 0;
                var moduleRep = new ModuleRep();
                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;
                OrganisationLearnerList learnerList = new OrganisationLearnerList();

                string selectedLearners = "";
                string unSelectedLearners = "";

                if (allSelected)
                {
                    //remove unselected users
                    if (unselectedUserList != null && unselectedUserList.Length > 0)
                    {
                        unSelectedLearners = string.Join(",", unselectedUserList);
                    }
                }
                else
                {
                    // if few are selected
                    if (selectedUserList != null && selectedUserList.Length > 0)
                    {
                        selectedLearners = string.Join(",", selectedUserList);
                    }

                }

                result = moduleRep.RevokeModuleAccess_Multiple(searchCriteria, selectedLearners, unSelectedLearners, allSelected);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        // get list of users with started modules(consumed license); on applied filter
        [HttpPost]
        public ActionResult LoadFreeLicenseLearnerData(DataTableFilter searchCriteria)
        {
            OrganisationLearnerList learnerList = new OrganisationLearnerList();
            learnerList.LearnerList = new List<LearnerInfo>();
            try
            {
                var moduleRep = new ModuleRep();

                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;

                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                    searchCriteria.Location = 0;
                    searchCriteria.Department = 0;
                }

                searchCriteria.Draw = Request.Form["draw"].FirstOrDefault();
                searchCriteria.Start = Request.Form["start"].FirstOrDefault();
                searchCriteria.Length = Request.Form["length"].FirstOrDefault();

                //Find Order Column
                var orderIndex = Request.Form["order[0][column]"].FirstOrDefault();
                searchCriteria.SortCol = Request.Form[$"columns[{orderIndex}][name]"].FirstOrDefault();
                searchCriteria.SortColDir = Request.Form["order[0][dir]"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                learnerList = moduleRep.GetLearnerWithAvailableLicense(searchCriteria);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = learnerList.TotalLearners, recordsTotal = learnerList.TotalLearners, data = learnerList.LearnerList });
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = learnerList.TotalLearners, recordsTotal = learnerList.TotalLearners, data = learnerList.LearnerList });
            }
        }
        #endregion

        #region configure module
        public ActionResult Courses()
        {
            return View("Courses");
        }

        // GET: list of all learners to whom the selected module is not assigned
        public ActionResult Setup()
        {
            return View("Setup");
        }

        // get report on applied filter
        [HttpPost]
        public ActionResult LoadModuleData(DataTableFilter searchCriteria)
        {
            OrgModuleList orgModuleList = new OrgModuleList();
            orgModuleList.ModuleList = new List<ELG.Model.OrgAdmin.Module>();
            try
            {
                var moduleRep = new ModuleRep();

                searchCriteria.Company = Convert.ToInt64(SessionHelper.CompanyId);
                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                }

                searchCriteria.Draw = Request.Form["draw"].FirstOrDefault();
                searchCriteria.Start = Request.Form["start"].FirstOrDefault();
                searchCriteria.Length = Request.Form["length"].FirstOrDefault();
                //Find Order Column
                var orderIndex = Request.Form["order[0][column]"].FirstOrDefault();
                searchCriteria.SortCol = Request.Form[$"columns[{orderIndex}][name]"].FirstOrDefault();
                searchCriteria.SortColDir = Request.Form["order[0][dir]"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                orgModuleList = moduleRep.GetModules(searchCriteria);

                // Process thumbnail URLs for public container access
                string thumbnailContainer = ELG.Web.Helper.CommonHelper.GetAppSettingValue("AzureStorage:ThumbnailContainer") ?? "elg-content";
                if (orgModuleList != null && orgModuleList.ModuleList != null)
                {
                    foreach (var module in orgModuleList.ModuleList)
                    {
                        // Thumbnail container is public; use direct blob URL without SAS token
                        if (!string.IsNullOrEmpty(module.CourseLogo) && !module.CourseLogo.StartsWith("http"))
                        {
                            module.CourseLogo = $"https://elgdocstorage.blob.core.windows.net/{thumbnailContainer}/{module.CourseLogo}";
                        }
                    }
                }

                return Json(new { draw = searchCriteria.Draw, recordsFiltered = orgModuleList.TotalModules, recordsTotal = orgModuleList.TotalModules, data = orgModuleList.ModuleList });

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = orgModuleList.TotalModules, recordsTotal = orgModuleList.TotalModules, data = orgModuleList.ModuleList });
            }
        }

        // Update course details with optional file uploads
        [HttpPost]
        public async Task<ActionResult> UpdateCourse(
            [FromForm] long CourseId,
            [FromForm] string CourseName,
            [FromForm] string CourseDesc,
            [FromForm] string OldThumbnailPath,
            [FromForm] IFormFile ThumbnailFile,
            [FromForm] IFormFile CoursePackage)
        {
            try
            {
                if (CourseId <= 0)
                {
                    return Json(new { status = 0, message = "Invalid course ID." });
                }

                var moduleRep = new ModuleRep();
                
                // Get existing course details
                var existingCourse = moduleRep.GetCourseDetails(CourseId);
                if (existingCourse == null)
                {
                    return Json(new { status = 0, message = "Course not found." });
                }

                // Get Azure storage configuration
                string connectionString = CommonHelper.GetAppSettingValue("AZStorageConnectionString");
                string courseContentContainer = CommonHelper.GetAppSettingValue("AzureStorage:CourseContentContainer") ?? "elg-learn";
                string thumbnailContainer = CommonHelper.GetAppSettingValue("AzureStorage:ThumbnailContainer") ?? "elg-content";
                
                if (string.IsNullOrEmpty(connectionString))
                {
                    return Json(new { status = 0, message = "Azure Storage configuration is missing." });
                }

                var azureStorage = new AzureStorageUtility(connectionString, courseContentContainer, thumbnailContainer);
                
                // Get company number for folder structure
                var companyNumber = SessionHelper.CompanyNumber;
                if (string.IsNullOrEmpty(companyNumber))
                {
                    return Json(new { status = 0, message = "Company information not found." });
                }
                
                string newThumbnailPath = OldThumbnailPath;
                string newCoursePath = null; // Will be populated if package is uploaded

                // Handle thumbnail upload if new file provided
                if (ThumbnailFile != null && ThumbnailFile.Length > 0)
                {
                    // Delete old thumbnail from Azure if exists
                    if (!string.IsNullOrEmpty(OldThumbnailPath))
                    {
                        try
                        {
                            var oldThumbBlobName = azureStorage.ExtractBlobName(OldThumbnailPath, isThumbnail: true);
                            if (!string.IsNullOrEmpty(oldThumbBlobName))
                            {
                                await azureStorage.DeleteBlobAsync(oldThumbBlobName, isThumbnail: true);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("Failed to delete old thumbnail: " + ex.Message, ex);
                        }
                    }

                    // Upload new thumbnail
                    string extension = Path.GetExtension(ThumbnailFile.FileName);
                    extension = string.IsNullOrWhiteSpace(extension) ? string.Empty : extension;
                    string uniqueId = Guid.NewGuid().ToString();
                    newThumbnailPath = $"thumbnails/thumbnail_{uniqueId}{extension}";

                    using (var stream = ThumbnailFile.OpenReadStream())
                    {
                        await azureStorage.UploadBlobAsync(newThumbnailPath, stream, ThumbnailFile.ContentType, isThumbnail: true);
                    }
                }

                // Handle SCORM package upload if new file provided
                if (CoursePackage != null && CoursePackage.Length > 0)
                {
                    try
                    {
                        // Delete old course package folder from Azure if it exists
                        if (!string.IsNullOrEmpty(existingCourse.CoursePath))
                        {
                            try
                            {
                                // Extract courseId from the existing path (e.g., "companyNumber/course/courseId" or "companyNumber/course/courseId/imsmanifest.xml")
                                string[] pathParts = existingCourse.CoursePath.Split(new[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);
                                if (pathParts.Length >= 3 && pathParts[1].ToLower() == "course")
                                {
                                    string oldCourseId = pathParts[2];
                                    await azureStorage.DeleteScormPackageAsync(companyNumber, oldCourseId);
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Error("Failed to delete old SCORM package: " + ex.Message, ex);
                                // Continue with new upload even if old deletion fails
                            }
                        }
                        
                        string uniqueCourseId = Guid.NewGuid().ToString();
                        
                        // Extract SCORM package and get start path
                        string startPath = await azureStorage.ExtractScormPackageAsync(
                            CoursePackage,
                            companyNumber,
                            uniqueCourseId);

                        // Build course launch path
                        var trimmedStart = (startPath ?? string.Empty).TrimStart('/', '\\');
                        newCoursePath = string.IsNullOrEmpty(trimmedStart)
                            ? $"{companyNumber}/course/{uniqueCourseId}"
                            : $"{companyNumber}/course/{uniqueCourseId}/{trimmedStart}";
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("SCORM package extraction failed: " + ex.Message, ex);
                        return Json(new { status = 0, message = "Failed to extract SCORM package: " + ex.Message });
                    }
                }

                // Update course in database
                var result = moduleRep.UpdateCourseDetails(CourseId, CourseName, CourseDesc, newThumbnailPath, newCoursePath);
                
                if (result > 0)
                {
                    return Json(new { status = 1, message = "Course updated successfully." });
                }
                else
                {
                    return Json(new { status = 0, message = "Failed to update course in database." });
                }
            }
            catch (Exception ex)
            {
                Logger.Error("UpdateCourse error: " + ex.Message, ex);
                return Json(new { status = -1, message = "An error occurred: " + ex.Message });
            }
        }

        // update minimum passing score for module
        [HttpPost]
        public ActionResult UpdateFrequency(ELG.Model.OrgAdmin.Module module)
        {
            try
            {
                var moduleRep = new ModuleRep();
                int result = moduleRep.UpdateModulePassingMarks(module.ModuleID, module.Frequency, module.CompletionDays);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }
        #endregion

        #region Course Catalogue
        // GET: list of all modules
        public ActionResult CourseCatalogue()
        {
            return View("CourseCatalogue");
        }
        #endregion

        #region revoke access / retake risk assessment

        // get report on applied filter
        [HttpPost]
        public ActionResult LoadLearnerDataToRevokeRA(DataTableFilter searchCriteria)
        {
            OrganisationLearnerList learnerList = new OrganisationLearnerList();
            learnerList.LearnerList = new List<LearnerInfo>();
            try
            {
                var moduleRep = new ModuleRep();

                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;
                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                    searchCriteria.Location = 0;
                    searchCriteria.Department = 0;
                }

                searchCriteria.Draw = Request.Form["draw"].FirstOrDefault();
                searchCriteria.Start = Request.Form["start"].FirstOrDefault();
                searchCriteria.Length = Request.Form["length"].FirstOrDefault();
                //Find Order Column
                var orderIndex = Request.Form["order[0][column]"].FirstOrDefault();
                searchCriteria.SortCol = Request.Form[$"columns[{orderIndex}][name]"].FirstOrDefault();
                searchCriteria.SortColDir = Request.Form["order[0][dir]"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                if (searchCriteria.Course > 0)
                    learnerList = moduleRep.GetUsersWithRA(searchCriteria);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = learnerList.TotalLearners, recordsTotal = learnerList.TotalLearners, data = learnerList.LearnerList });

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = learnerList.TotalLearners, recordsTotal = learnerList.TotalLearners, data = learnerList.LearnerList });
            }
        }

        // retake risk assessment
        [HttpPost]
        public ActionResult RetakeLearnerRAAccess(LearnerModuleFilter searchCriteria)
        {
            try
            {
                var moduleRep = new ModuleRep();
                int result = moduleRep.RetakeRAAccess(searchCriteria);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }
        #endregion

        #region reset training - fixed calendar

        public ActionResult ResetDateSetup()
        {
            return View("ResetDateSetup");
        }

        public ActionResult GetTrainingResetDate()
        {
            var reportRep = new ReportRep();
            var resetDate = reportRep.GetTrainingResetDate(SessionHelper.CompanyId);
            string resetDateString = (Convert.ToDateTime(resetDate)).ToString("dd-MMM-yyyy");

            return Json(new { resetDateString, resetDate });
        }


        //Send OTP
        public ActionResult SendTrainingResetDateOTP()
        {
            string otp_txn = "";
            try
            {
                string otp = GetOTPString(6);
                var learnerRep = new LearnerRep();
                LearnerInfo learnerInfo = learnerRep.GetLearnerInfo(SessionHelper.UserId);

                var moduleRep = new ModuleRep();
                otp_txn = moduleRep.CreateOTPTransaction(otp, "reset training calendar", learnerInfo.EmailId);
                if (!String.IsNullOrEmpty(otp_txn))
                {
                    EmailUtility emailUti = new EmailUtility(_env);

                    string emailTemplate = emailUti.GetEmailTemplate("resetTrainingDateOTP.html");
                    emailTemplate = emailTemplate.Replace("{username}", $"{learnerInfo.FirstName} {learnerInfo.LastName}");
                    emailTemplate = emailTemplate.Replace("{OTP}", otp);

                   emailUti.SendEmailUsingSendGrid(learnerInfo.EmailId, "eLearning Gate - Reset Training renewal date OTP", emailTemplate);
                    // emailUti.SendEMail(learnerInfo.EmailId, "eLearning Gate - Reset Training renewal date OTP", emailTemplate);

                    return Json(new { otp_txn });
                }
                else
                {
                    return Json(new { otp_txn });
                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { otp_txn });
            }
        }

        private static Random random = new Random();
        public static string GetOTPString(int length)
        {
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [HttpPost]
        public ActionResult UpdateTrainingResetDate(string resetDate, string otp, Guid otp_txn)
        {
            try
            {
                var reportRep = new ReportRep();
                int result = reportRep.UpdateTrainingResetDate(SessionHelper.CompanyId, resetDate, otp, otp_txn);

                //send notification to admins
                if(result == 1)
                {
                    SendResetDateNotoficationToAdmins(resetDate);
                }


                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        private void SendResetDateNotoficationToAdmins(string resetDate)
        {
            try
            {
                AdminLearnerListFilter filter = new AdminLearnerListFilter();
                filter.SearchText = "";
                filter.AdminLevel = 1;
                filter.Company = SessionHelper.CompanyId;

                var adminRep = new AdminRep();
                AdminLearnerList adminLearnerList = adminRep.GetAllAdminLearners(filter);

                if (adminLearnerList != null && adminLearnerList.TotalAdmins > 0)
                {
                    EmailUtility emailUti = new EmailUtility(_env);
                    string emailTemplate = emailUti.GetEmailTemplate("resetTrainingDateNotification.html");
                    foreach (var admin in adminLearnerList.AdminList)
                    {
                        string notificationEmail = emailTemplate;

                        notificationEmail = notificationEmail.Replace("{username}", $"{admin.FirstName} {admin.LastName}");
                        notificationEmail = notificationEmail.Replace("{name}", $"{SessionHelper.UserName}");
                        notificationEmail = notificationEmail.Replace("{resetDate}", resetDate);
                        emailUti.SendEmailUsingSendGrid(admin.EmailId, "eLearning Gate - Training renewal date reset", notificationEmail);
                        //emailUti.SendEMail(admin.EmailId, "eLearning Gate - Training renewal date reset", notificationEmail);
                    }

                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
        }

        #endregion

        #region reset training - Manual

        public ActionResult TrainingRefresher()
        {
            return View("TrainingRefresher");
        }

        // get report on applied filter
        [HttpPost]
        public ActionResult LoadLearnerAssignedModuleData(LearnerReportFilter searchCriteria)
        {
            AssignedCourseReport assignedCourseReport = new AssignedCourseReport();
            assignedCourseReport.AssignedRecords = new List<AssignedCourseItem>();
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

                searchCriteria.Draw = Request.Form["draw"].FirstOrDefault();
                searchCriteria.Start = Request.Form["start"].FirstOrDefault();
                searchCriteria.Length = Request.Form["length"].FirstOrDefault();

                //Find Order Column
                var orderIndex = Request.Form["order[0][column]"].FirstOrDefault();
                searchCriteria.SortCol = Request.Form[$"columns[{orderIndex}][name]"].FirstOrDefault();
                searchCriteria.SortColDir = Request.Form["order[0][dir]"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                assignedCourseReport = reportRep.GetLearnerAssignedCourseReport(searchCriteria);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = assignedCourseReport.TotalRecords, recordsTotal = assignedCourseReport.TotalRecords, data = assignedCourseReport.AssignedRecords });
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = assignedCourseReport.TotalRecords, recordsTotal = assignedCourseReport.TotalRecords, data = assignedCourseReport.AssignedRecords });
            }
        }

        // revoke Module Access
        [HttpPost]
        public ActionResult RefreshLearnerModuleProgress(LearnerModuleFilter searchCriteria)
        {
            try
            {
                var moduleRep = new ModuleRep();
                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;
                int result = moduleRep.RefreshLearerModuleProgress(searchCriteria);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        [HttpPost]
        public ActionResult RefreshLearnerModuleProgress_Multiple(LearnerModuleFilter searchCriteria, bool allSelected, long[] selectedRecordList, long[] unselectedRecordList)
        {
            try
            {
                int result = 0;
                var moduleRep = new ModuleRep();
                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;

                string selectedRecords = "";
                string unSelectedRecords = "";

                if (allSelected)
                {
                    //remove unselected users
                    if (unselectedRecordList != null && unselectedRecordList.Length > 0)
                    {
                        unSelectedRecords = string.Join(",", unselectedRecordList);
                    }
                }
                else
                {
                    // if few are selected
                    if (selectedRecordList != null && selectedRecordList.Length > 0)
                    {
                        selectedRecords = string.Join(",", selectedRecordList);
                    }

                }

                result = moduleRep.RefreshLearerModuleProgress_Multiple(searchCriteria, selectedRecords, unSelectedRecords, allSelected);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }
        #endregion

        #region sub modules


        // get report on applied filter
        [HttpPost]
        public ActionResult LoadSubmoduleData(long courseId)
        {
            try
            {
                var moduleRep = new ModuleRep();
                List<SubModule> subModules = moduleRep.GetCourseSubModules(courseId);

                return Json(new
                {
                    draw = 1, // This should come from the request
                    recordsTotal = subModules.Count,
                    recordsFiltered = subModules.Count,
                    data = subModules
                });


            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new
                {
                    draw = 1,
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<SubModule>()
                });  // Ensure empty array on failure
            }
        }
        // function to validate file extensions  
        private bool ValidateFileExtension(IFormFile file)
        {
            // Allowed extensions  
            var allowedExtensions = new List<string> { ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".mp4", ".xls", ".xlsx", ".zip" };

            // Get file extension  
            var fileExtension = Path.GetExtension(file.FileName)?.ToLower();

            // Check if the extension is valid  
            return allowedExtensions.Contains(fileExtension);
        }

        // Updated CreateSubmodule method with extension validation  
        [HttpPost]
        public async Task<ActionResult> CreateSubmodule(IFormFile newDocFile, SubModule module)
        {
            int status = 0;
            try
            {
                // Use the newDocFile param or files in the request
                IFormFile document = newDocFile;
                if ((document == null || document.Length == 0) && Request?.Form?.Files?.Count > 0)
                {
                    document = Request.Form.Files[0];
                }

                if (document != null)
                {
                    // Validate file extension  
                    if (!ValidateFileExtension(document))
                    {
                        return Json(new { success = "Invalid file extension", status });
                    }

                    // Validate file size (<= 5MB) 5*1024*1024 = 5242880  
                    if (document.Length > 5242880)
                    {
                        return Json(new { success = "File too large", status });
                    }

                    var result = await AsyncUploadSubModuleFile(document, module);

                    module.SubModulePath = result;
                    module.CreatedById = SessionHelper.UserId;

                    var moduleRep = new ModuleRep();
                    status = moduleRep.CreateNewSubModule(module);
                    return Json(new { success = result, status });
                }
                return Json(new { success = "No files", status });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = ex.Message, status });
            }
        }
        public static async Task<string> AsyncUploadSubModuleFile(IFormFile newDocFile, SubModule module)
        {
            var connectionString = ELG.Web.Helper.CommonHelper.GetAppSettingValue("AZStorageConnectionString");

            // Create a client with the connection
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            // Get course content container from configuration
            string courseContentContainer = ELG.Web.Helper.CommonHelper.GetAppSettingValue("AzureStorage:CourseContentContainer") ?? "elg-learn";
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(courseContentContainer);

            // Path inside container: {CompanyNumber}/submodule/{filename}
            string folderName = SessionHelper.CompanyNumber + "/submodule";
            string uniqueFileName = Guid.NewGuid() + Path.GetExtension(newDocFile.FileName);
            string filename = folderName + "/" + uniqueFileName;
            string filetype = newDocFile.ContentType;

            BlobClient blobClient = containerClient.GetBlobClient(filename);
            var blobHttpHeader = new BlobHttpHeaders { ContentType = filetype };

            // Upload the file to the designated folder
            using (var stream = newDocFile.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, blobHttpHeader);
            }

            return blobClient.Uri.AbsoluteUri;
        }

        #endregion

        #region SCORM Package Upload
        /// <summary>
        /// Display SCORM package upload page
        /// </summary>
        [HttpGet]
        public ActionResult UploadScormPackage()
        {
            try
            {
                var moduleRep = new ModuleRep();
                
                // Get current course count and max allowed
                int currentCourseCount = moduleRep.GetCourseCountByOrganization((int)SessionHelper.CompanyId);
                int? maxAllowedCoursesNullable = moduleRep.GetMaxAllowedCourseCount((int)SessionHelper.CompanyId);
                int maxAllowedCourses = maxAllowedCoursesNullable ?? 0;
                bool quotaExhausted = (maxAllowedCourses > 0) && (currentCourseCount >= maxAllowedCourses);
                
                // Pass quota info to view
                ViewBag.CurrentCourseCount = currentCourseCount;
                ViewBag.MaxAllowedCourses = maxAllowedCourses;
                ViewBag.QuotaExhausted = quotaExhausted;
                
                // Debug logging
                System.Diagnostics.Debug.WriteLine($"SCORM Upload Page - CompanyId: {SessionHelper.CompanyId}, CurrentCount: {currentCourseCount}, MaxAllowed: {maxAllowedCourses}, QuotaExhausted: {quotaExhausted}");
                
                return View();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View();
            }
        }

        [HttpPost]
        [RequestSizeLimit(600L * 1024L * 1024L)]
        [RequestFormLimits(MultipartBodyLengthLimit = 600L * 1024L * 1024L)]
        public async Task<ActionResult> UploadScormPackage([FromForm] ScormPackageUploadViewModel model)
        {
            ELG.Model.OrgAdmin.ControllerResponse response = new ELG.Model.OrgAdmin.ControllerResponse();
            try
            {
                // Validate quota before proceeding
                var moduleRep = new ModuleRep();
                int currentCourseCount = moduleRep.GetCourseCountByOrganization((int)SessionHelper.CompanyId);
                int? maxAllowedCoursesNullable = moduleRep.GetMaxAllowedCourseCount((int)SessionHelper.CompanyId);
                int maxAllowedCourses = maxAllowedCoursesNullable ?? 0;
                
                System.Diagnostics.Debug.WriteLine($"SCORM Upload POST - CompanyId: {SessionHelper.CompanyId}, CurrentCount: {currentCourseCount}, MaxAllowed: {maxAllowedCourses}");
                
                if (maxAllowedCourses > 0 && currentCourseCount >= maxAllowedCourses)
                {
                    System.Diagnostics.Debug.WriteLine($"QUOTA EXCEEDED: Current {currentCourseCount} >= Max {maxAllowedCourses}");
                    response.Err = 1;
                    response.Message = $"Course quota exhausted. You have reached the maximum limit of {maxAllowedCourses} courses.";
                    return Json(response);
                }

                // Validate model
                if (string.IsNullOrEmpty(model.CourseTitle) || 
                    model.ScormPackage == null || model.ScormPackage.Length == 0)
                {
                    response.Err = 1;
                    response.Message = "Course title and SCORM package are required.";
                    return Json(response);
                }

                // Validate file types
                var allowedZipMimeTypes = new[] { "application/zip", "application/x-zip-compressed", "application/octet-stream" };
                if (!allowedZipMimeTypes.Contains(model.ScormPackage.ContentType, StringComparer.OrdinalIgnoreCase))
                {
                    response.Err = 1;
                    response.Message = "Only ZIP files are allowed for SCORM packages.";
                    return Json(response);
                }

                // Validate thumbnail if provided
                if (model.Thumbnail != null && model.Thumbnail.Length > 0)
                {
                    var allowedImageTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                    if (!allowedImageTypes.Contains(model.Thumbnail.ContentType))
                    {
                        response.Err = 1;
                        response.Message = "Only image files (JPEG, PNG, GIF, WebP) are allowed for thumbnails.";
                        return Json(response);
                    }
                }

                // Get company info for folder path
                string companyNumber = SessionHelper.CompanyNumber;
                if (string.IsNullOrEmpty(companyNumber))
                {
                    response.Err = 1;
                    response.Message = "Company information not found.";
                    return Json(response);
                }

                // Generate unique course ID
                string uniqueCourseId = Guid.NewGuid().ToString();

                // Get Azure storage connection string
                string connectionString = CommonHelper.GetAppSettingValue("AZStorageConnectionString");
                if (string.IsNullOrEmpty(connectionString))
                {
                    response.Err = 1;
                    response.Message = "Azure Storage configuration is missing.";
                    return Json(response);
                }

                // Get container names from configuration
                string courseContentContainer = CommonHelper.GetAppSettingValue("AzureStorage:CourseContentContainer") ?? "elg-learn";
                string thumbnailContainer = CommonHelper.GetAppSettingValue("AzureStorage:ThumbnailContainer") ?? "elg-content";

                // Upload to Azure
                var azureStorage = new AzureStorageUtility(connectionString, courseContentContainer, thumbnailContainer);
                
                // Extract SCORM package and get start path (relative inside course folder)
                string startPath = await azureStorage.ExtractScormPackageAsync(
                    model.ScormPackage, 
                    companyNumber, 
                    uniqueCourseId);

                // Build course launch path to persist
                var trimmedStart = (startPath ?? string.Empty).TrimStart('/', '\\');
                string courseLaunchPath = string.IsNullOrEmpty(trimmedStart)
                    ? $"{companyNumber}/course/{uniqueCourseId}"
                    : $"{companyNumber}/course/{uniqueCourseId}/{trimmedStart}";

                // Upload thumbnail separately if provided and capture its storage path
                string thumbnailPath = string.Empty;
                if (model.Thumbnail != null && model.Thumbnail.Length > 0)
                {
                    string extension = Path.GetExtension(model.Thumbnail.FileName);
                    extension = string.IsNullOrWhiteSpace(extension) ? string.Empty : extension;
                    thumbnailPath = $"thumbnails/thumbnail_{uniqueCourseId}{extension}";

                    var thumbUploadOk = await azureStorage.UploadScormPackageAsync(
                        null,
                        model.Thumbnail,
                        companyNumber,
                        uniqueCourseId,
                        model.CourseTitle,
                        startPath);

                    if (!thumbUploadOk)
                    {
                        response.Err = 1;
                        response.Message = "Thumbnail upload failed. Please try again.";
                        return Json(response);
                    }
                }

                // Save course details to database
                var courseRep = new ModuleRep();
                int courseId = courseRep.CreateScormCourse(
                    (int)SessionHelper.CompanyId,
                    model.CourseTitle,
                    model.CourseDescription ?? "",
                    thumbnailPath,
                    courseLaunchPath,
                    uniqueCourseId);
                
                if (courseId > 0)
                {
                    response.Err = 0;
                    response.Message = "SCORM package uploaded successfully.";
                    response.Url = Url.Action("Courses", "CourseManagement");
                    return Json(response);
                }
                else
                {
                    response.Err = 1;
                    response.Message = "Failed to save course details to database.";
                    return Json(response);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                response.Err = 2;
                response.Message = $"Error uploading SCORM package: {ex.Message}";
                return Json(response);
            }
        }

        #endregion
    }
}