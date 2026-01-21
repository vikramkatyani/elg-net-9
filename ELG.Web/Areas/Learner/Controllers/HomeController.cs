using ELG.DAL.LearnerDAL;
using ELG.DAL.Utilities;
using ELG.Web.Helper;
using ELG.Web.Models;
using ELG.Model.Learner;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ELG.Model.OrgAdmin;

namespace ELG.Web.Areas.Learner.Controllers
{
    [Area("Learner")]
    public class HomeController : Controller
    {
        private static readonly string ScormShimContent = ScormShimSource.ScormShim;
        // GET: Learner/Home/Dashboard
        public IActionResult Dashboard()
        {
            ViewBag.Title = "Learner Dashboard";
            return View();
        }

        // GET: Learner/Home/Index
        public IActionResult Index()
        {
            ViewBag.Title = "Learner Home";
            return RedirectToAction("Dashboard");
        }

        // GET: Learner/Home/MyCourses
        public IActionResult MyCourses()
        {
            ViewBag.Title = "My Courses";
            return View();
        }

        // POST: Get learner courses
        [HttpPost]
        public IActionResult GetCourses([FromBody] GetCoursesRequest request)
        {
            string course = request?.Course ?? "";
            string sort = request?.Sort ?? "";
            List<ELG.Model.Learner.LearnerAssignedCourse> courses = new List<ELG.Model.Learner.LearnerAssignedCourse>();
            try
            {
                var learnerRep = new LearnerCourseRep();
                Int64 UserID = Convert.ToInt64(SessionHelper.UserId);
                courses = learnerRep.GetLearnerCourses(course, UserID, sort);
                
                // Generate SAS URLs for Azure Storage access
                string azureConnectionString = ELG.Web.Helper.CommonHelper.GetAppSettingValue("AZStorageConnectionString");
                string thumbnailContainer = ELG.Web.Helper.CommonHelper.GetAppSettingValue("AzureStorage:ThumbnailContainer") ?? "elg-content";
                string courseContentContainer = ELG.Web.Helper.CommonHelper.GetAppSettingValue("AzureStorage:CourseContentContainer") ?? "elg-learn";
                
                if (!string.IsNullOrEmpty(azureConnectionString))
                {
                    var azureStorage = new ELG.DAL.Utilities.AzureStorageUtility(azureConnectionString, courseContentContainer, thumbnailContainer);
                    
                    foreach (var c in courses)
                    {
                        // Thumbnail container is public; use direct blob URL without SAS token
                        if (!string.IsNullOrEmpty(c.CourseLogo) && !c.CourseLogo.StartsWith("http"))
                        {
                            // Build public blob URL: https://{account}.blob.core.windows.net/{container}/{blob}
                            c.CourseLogo = $"https://elgdocstorage.blob.core.windows.net/{thumbnailContainer}/{c.CourseLogo}";
                        }
                        
                        // Generate SAS URLs for course content (isThumbnail = false, 8 hour expiration for course launch)
                        if (!string.IsNullOrEmpty(c.CoursePath))
                        {
                            c.CoursePath = azureStorage.GenerateSasUrl(c.CoursePath, 480, isThumbnail: false);
                        }

                        // Generate SAS URLs for submodules (use course content container)
                        if (c.SubModuleList != null)
                        {
                            foreach (var sm in c.SubModuleList)
                            {
                                if (!string.IsNullOrEmpty(sm.SubModulePath))
                                {
                                    sm.SubModulePath = azureStorage.GenerateSasUrl(sm.SubModulePath, 480, isThumbnail: false);
                                }
                            }
                        }
                    }
                }
                
                return Json(new { courses });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { courses });
            }
        }

        // GET: Learner/Home/MyProfile
        public IActionResult MyProfile()
        {
            ViewBag.Title = "My Profile";
            return View();
        }

        // POST: Reset course progress
        [HttpPost]
        public IActionResult ResetProgress(Int64 Course, Int64 RecordId)
        {
            int reset = 0;
            try
            {
                var learnerRep = new LearnerCourseRep();
                Int64 UserID = Convert.ToInt64(SessionHelper.UserId);
                reset = learnerRep.ResetLearningProgress(UserID, Course, RecordId);
                return Json(new { reset });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { reset });
            }
        }

        // POST: Launch submodule
        [HttpPost]
        public IActionResult LaunchSubModule([FromBody] SubModuleRequest request)
        {
            int success = 0;
            try
            {
                var learnerRep = new LearnerCourseRep();
                Int64 UserID = Convert.ToInt64(SessionHelper.UserId);
                success = learnerRep.UpdateSubModuleProgress(UserID, request.subModuleId);
                return Json(new { success });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success });
            }
        }

        // POST: Ensure scorm_api.js shim exists in the course folder (supports azure-hosted cross-origin content)
        [HttpPost]
        public async Task<IActionResult> EnsureScormShim([FromBody] EnsureShimRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.url))
                {
                    return Json(new { ensured = false, reason = "No URL" });
                }

                string azureConnectionString = ELG.Web.Helper.CommonHelper.GetAppSettingValue("AZStorageConnectionString");
                string courseContentContainer = ELG.Web.Helper.CommonHelper.GetAppSettingValue("AzureStorage:CourseContentContainer") ?? "elg-learn";
                string thumbnailContainer = ELG.Web.Helper.CommonHelper.GetAppSettingValue("AzureStorage:ThumbnailContainer") ?? "elg-content";
                if (string.IsNullOrEmpty(azureConnectionString))
                {
                    return Json(new { ensured = false, reason = "No Azure connection" });
                }

                var azure = new ELG.DAL.Utilities.AzureStorageUtility(azureConnectionString, courseContentContainer, thumbnailContainer);

                // Derive directory path for the start file and target blob for shim
                var uri = new Uri(request.url);
                var pathNoQuery = uri.GetLeftPart(UriPartial.Path);
                var lastSlash = pathNoQuery.LastIndexOf('/')
                    ;
                if (lastSlash <= 0)
                {
                    return Json(new { ensured = false, reason = "Invalid path" });
                }
                var dirUrl = pathNoQuery.Substring(0, lastSlash + 1); // keep trailing slash
                var shimUrl = dirUrl + "scorm_api.js";

                // If it doesn't exist, upload our shim
                bool exists = await azure.BlobExistsAsync(shimUrl, isThumbnail: false);
                if (!exists)
                {
                    await azure.UploadTextAsync(shimUrl, ScormShimContent, isThumbnail: false, contentType: "application/javascript");
                }

                return Json(new { ensured = true });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { ensured = false });
            }
        }

        // GET: Check for auto courses with no licences
        [HttpPost]
        public IActionResult CheckForAutoCoursesWithNoLicences()
        {
            bool courseWithNoLicences = false;
            try
            {
                var learnerRep = new LearnerCourseRep();
                Int64 UserID = Convert.ToInt64(SessionHelper.UserId);
                courseWithNoLicences = learnerRep.GetAutoCoursesWithNoLicence(UserID);
                return Json(new { courseWithNoLicences });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { courseWithNoLicences });
            }
        }

        // POST: Get course history list
        [HttpPost]
        public IActionResult CourseHistoryList(ELG.Model.Learner.DataTableFilter searchCriteria)
        {
            CourseProgressHistoryList progressRecords = new CourseProgressHistoryList();
            try
            {
                var historyRep = new LearnerCourseRep();
                searchCriteria.Learner = Convert.ToInt64(SessionHelper.UserId);
                progressRecords = historyRep.GetLearnerCourseHistory(searchCriteria);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
            return Json(new { draw = searchCriteria.Draw, recordsFiltered = progressRecords.TotalRecords, recordsTotal = progressRecords.TotalRecords, data = progressRecords.History });
        }

        // POST: Get session learner info
        [HttpPost]
        public ActionResult GetSessionLearnerInfo()
        {
            dynamic learnerInfo = null;
            try
            {
                var learnerRep = new LearnerAccountRep();
                learnerInfo = learnerRep.GetSessionLearnerProfile(Convert.ToInt64(SessionHelper.UserId));
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
            return Json(new { learnerInfo });
        }

        // POST: Update learner info
        [HttpPost]
        public ActionResult UpdateLearnerInfo(LearnerProfile learner)
        {
            int result = 0;
            try
            {
                var learnerRep = new LearnerAccountRep();
                learner.UserID = Convert.ToInt64(SessionHelper.UserId);
                result = learnerRep.UpdateLearnerInfo(learner);
                if (result == 1)
                {
                    SessionHelper.UserName = learner.FirstName;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
            return Json(new { success = result });
        }

        // POST: Upload profile picture
        [HttpPost]
        public async Task<IActionResult> UploadProfilePic()
        {
            int uploaded = 0;
            try
            {
                var file = Request.Form.Files[0];
                if (file != null && file.Length > 0)
                {
                    byte[] profilePic;
                    using (var ms = new MemoryStream())
                    {
                        await file.CopyToAsync(ms);
                        profilePic = ms.ToArray();
                    }
                    var learnerRep = new LearnerAccountRep();
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

        // POST: Remove profile picture
        [HttpPost]
        public ActionResult RemoveProfilePic()
        {
            int uploaded = 0;
            try
            {
                var learnerRep = new LearnerAccountRep();
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

        // GET: Learner/Home/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        // POST: Learner/Home/ChangePassword
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordViewModel pwddetail)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var acc = new LearnerAccountRep();
                    ELG.Model.Learner.LearnerInfo learner = acc.GetUserShortDetailByUserID(SessionHelper.UserId);
                    if (learner.Password != CommonMethods.EncodePassword(pwddetail.OldPassword, CommonHelper.GetAppSettingValue("LMS_PasswordEncryptionKey")))
                    {
                        ModelState.AddModelError(string.Empty, "Your old password is not correct, Please enter correct old password.");
                        return View(pwddetail);
                    }
                    if (learner.Password == CommonMethods.EncodePassword(pwddetail.NewPassword, CommonHelper.GetAppSettingValue("LMS_PasswordEncryptionKey")))
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
            catch(Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(pwddetail);
            }
        }
    }

    public class SubModuleRequest
    {
        public long subModuleId { get; set; }
    }

    public class GetCoursesRequest
    {
        public string Course { get; set; }
        public string Sort { get; set; }
    }

    public class EnsureShimRequest
    {
        public string url { get; set; }
    }

    // Centralized shim content served to Azure when missing
    internal static class ScormShimSource
    {
        // Lightweight shim mirrored from wwwroot/js/SCORM/shims/scorm_api.js
        public const string ScormShim = @"(function(){\n    var _cache = {};\n    var _pending = {};\n    var _reqId = 0;\n    function nextId(){ return (++_reqId) + '_' + Date.now(); }\n    function hasOpener(){ try { return !!window.opener && !window.opener.closed; } catch(e){ return false; } }\n    function post(method, args){\n        if(!hasOpener()) return null;\n        var id = nextId();\n        try {\n            window.opener.postMessage({ type: 'SCORM_REQUEST', method: method, args: args || [], requestId: id }, '*');\n        } catch(e) { }\n        return id;\n    }\n    window.addEventListener('message', function(event){\n        var data = event.data || {};\n        if(data.type !== 'SCORM_RESPONSE' || !data.requestId) return;\n        var id = data.requestId;\n        if(_pending[id]){ try { _pending[id](data); } catch(e){} delete _pending[id]; }\n        if(data.values && typeof data.values === 'object'){ for(var k in data.values){ _cache[k] = data.values[k]; } }\n    });\n    var API = {\n        LMSInitialize: function(param){\n            var id = post('LMSInitialize', [param||'' ]);\n            if(id){ _pending[id] = function(resp){ if(resp && resp.values){ for(var k in resp.values){ _cache[k] = resp.values[k]; } } }; }\n            return 'true';\n        },\n        LMSGetValue: function(element){ var v = _cache[element]; return (v === undefined || v === null) ? '' : v; },\n        LMSSetValue: function(element, value){ _cache[element] = (value == null ? '' : value); post('LMSSetValue', [element, value]); return 'true'; },\n        LMSCommit: function(param){ post('LMSCommit', [param||'' ]); return 'true'; },\n        LMSFinish: function(param){ post('LMSFinish', [param||'' ]); return 'true'; },\n        LMSGetLastError: function(){ return 0; }, LMSGetErrorString: function(){ return ''; }, LMSGetDiagnostic: function(){ return ''; }\n    };\n    var SCORM = { initialized: false, init: function(){ var r = API.LMSInitialize(''); this.initialized = (r === 'true'); return this.initialized; }, get: function(p){ return API.LMSGetValue(p); }, set: function(p, v){ return (API.LMSSetValue(p, v) === 'true'); }, save: function(){ return (API.LMSCommit('') === 'true'); }, quit: function(){ return (API.LMSFinish('') === 'true'); } };\n    window.API = window.API || API;\n    window.SCORM = window.SCORM || SCORM;\n})();";
    }

}
