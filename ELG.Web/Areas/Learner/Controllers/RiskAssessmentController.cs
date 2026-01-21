using ELG.DAL.LearnerDAL;
using ELG.Web.Helper;
using ELG.Model.Learner;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace ELG.Web.Areas.Learner.Controllers
{
    [Area("Learner")]
    [Route("Learner/RiskAssessment")]
    public class RiskAssessmentController : Controller
    {
        private readonly IConfiguration Configuration;

        public RiskAssessmentController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // GET: Risk Assessment
        [Route("")]
        [Route("Assessment")]
        public ActionResult Assessment()
        {
            ViewBag.Title = "Risk Assessment";
            return View("Assessment");
        }

        [Route("LoadRAReport/{courseId:int}/{subModuleId:int}")]
        public ActionResult LoadRAReport(int courseId, int subModuleId)
        {
            ViewBag.Title = "Risk Assessment";
            ViewBag.CourseId = courseId;
            ViewBag.SubModuleId = subModuleId;
            try
            {
                var moduleRep = new ELG.DAL.OrgAdminDAL.ModuleRep();
                var common = new ELG.DAL.Utilities.CommonMethods();

                var course = moduleRep.GetCourseDetails(courseId);
                var subs = common.GetCourseSubModules(courseId);
                var raSub = subs?.FirstOrDefault(s => s.SubModuleId == subModuleId);

                ViewBag.CourseName = course?.ModuleName ?? "";
                ViewBag.RATitle = raSub?.SubModuleName ?? "";
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
            return View("AssessmentReport");
        }

        // get report on applied filter
        [HttpPost]
        [Route("LoadRAData")]
        public ActionResult LoadRAData(DataTableFilter searchCriteria)
        {
            try
            {
                var courseRep = new LearnerCourseRep();

                searchCriteria.Organisation = Convert.ToInt64(SessionHelper.CompanyId);
                searchCriteria.Learner = Convert.ToInt64(SessionHelper.UserId);
                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                }

                if (Request.Form.TryGetValue("draw", out var drawValues))
                {
                    searchCriteria.Draw = drawValues.FirstOrDefault();
                }
                if (Request.Form.TryGetValue("start", out var startValues))
                {
                    searchCriteria.Start = startValues.FirstOrDefault();
                }
                if (Request.Form.TryGetValue("length", out var lengthValues))
                {
                    searchCriteria.Length = lengthValues.FirstOrDefault();
                }

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                LearnerRACourseList raList = courseRep.GetLearnerRA(searchCriteria);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = raList.TotalCount, recordsTotal = raList.TotalCount, data = raList.RAList });

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("Assessment");
            }
        }

        // get report on applied filter
        [HttpPost]
        [Route("LoadRAAttemptData")]
        public ActionResult LoadRAAttemptData(RAAttemptFilter searchCriteria)
        {
            try
            {
                var courseRep = new LearnerCourseRep();

                searchCriteria.Organisation = Convert.ToInt64(SessionHelper.CompanyId);
                searchCriteria.Learner = Convert.ToInt64(SessionHelper.UserId);
                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                }

                if (Request.Form.TryGetValue("draw", out var drawValues))
                {
                    searchCriteria.Draw = drawValues.FirstOrDefault();
                }
                if (Request.Form.TryGetValue("start", out var startValues))
                {
                    searchCriteria.Start = startValues.FirstOrDefault();
                }
                if (Request.Form.TryGetValue("length", out var lengthValues))
                {
                    searchCriteria.Length = lengthValues.FirstOrDefault();
                }

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                LearnerRACourseList raList = courseRep.GetLearnerRAAttempts(searchCriteria);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = raList.TotalCount, recordsTotal = raList.TotalCount, data = raList.RAList, incompleteRA = raList.IncompleteRACount });

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("AssessmentReport");
            }
        }

        // GET: list of all risk assessment question for a course
        [HttpPost]
        [Route("GetRiskAssessmentQuestion")]
        public ActionResult GetRiskAssessmentQuestion(RiskAssessment learnerRA)
        {
            List<RiskAssessmentQuestion> quesList = new List<RiskAssessmentQuestion>();
            try
            {
                var moduleRep = new LearnerCourseRep();
                learnerRA.LearnerID = SessionHelper.UserId;
                quesList = moduleRep.GetLearnerRAQuestions(learnerRA);

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }

            return Json(new { quesList });
        }

        // GET: list of all risk assessment question for a course
        [HttpPost]
        [Route("GetSubModuleRiskAssessmentQuestion")]
        public IActionResult GetSubModuleRiskAssessmentQuestion([FromBody] RiskAssessmentRecord learnerRA)
        {
            Int64 rarid = 0;
            List<RiskAssessmentQuestion> quesList = new List<RiskAssessmentQuestion>();
            try
            {
                var moduleRep = new LearnerCourseRep();
                learnerRA.LearnerID = SessionHelper.UserId;

                if (learnerRA.SubModuleId <= 0)
                {
                    Logger.Warn($"GetSubModuleRiskAssessmentQuestion called with invalid SubModuleId={learnerRA.SubModuleId} for Learner={learnerRA.LearnerID}");
                    return new JsonResult(new { quesList, rarid }, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = null });
                }

                quesList = moduleRep.GetLearnerSubModuleRAQuestions(learnerRA);
                rarid = quesList.FirstOrDefault()?.RiskAssessmentResultID ?? 0;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }

            return new JsonResult(new { quesList, rarid }, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = null });
        }

        // GET: list of all risk assessment question for a course
        [HttpPost]
        [Route("GetRiskAssessmentResponses")]
        public ActionResult GetRiskAssessmentResponses(RAResponseFilter filter)
        {
            List<RiskAssessmentResponseReview> resList = new List<RiskAssessmentResponseReview>();
            try
            {
                var moduleRep = new LearnerCourseRep();
                filter.Learner = SessionHelper.UserId;
                resList = moduleRep.GetLearnerRAResponses(filter);

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }

            return Json(new { draw = filter.Draw, recordsFiltered = resList.Count, recordsTotal = resList.Count, data = resList });
        }

        // GET: list of all risk assessment question for a course
        [HttpPost]
        [Route("GetLearnerSubModuleRAResponses")]
        public ActionResult GetLearnerSubModuleRAResponses(RAResponseFilter filter)
        {
            List<RiskAssessmentResponseReview> resList = new List<RiskAssessmentResponseReview>();
            try
            {
                var moduleRep = new LearnerCourseRep();
                filter.Learner = SessionHelper.UserId;
                resList = moduleRep.GetLearnerSubModuleRAResponses(filter);

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }

            return Json(new { draw = filter.Draw, recordsFiltered = resList.Count, recordsTotal = resList.Count, data = resList });
        }

        // start risk assessment
        [HttpPost]
        [Route("StartRiskAssessment")]
        public ActionResult StartRiskAssessment(RiskAssessment learnerRA)
        {
            try
            {
                var raRep = new LearnerCourseRep();
                learnerRA.LearnerID = SessionHelper.UserId;
                int result = raRep.StartRiskAssessment(learnerRA);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        [HttpPost]
        [Route("StartSubModuleRiskAssessment")]
        public ActionResult StartSubModuleRiskAssessment([FromBody] RiskAssessment learnerRA)
        {
            try
            {
                var learnerRep = new LearnerCourseRep();
                var raRep = new LearnerCourseRep();
                learnerRA.LearnerID = SessionHelper.UserId;
                var success = learnerRep.UpdateSubModuleProgress(learnerRA.LearnerID, learnerRA.CourseId);
                int result = raRep.StartRiskAssessment(learnerRA);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        //save ra response
        [HttpPost]
        [Route("SaveRAResponse")]
        public ActionResult SaveRAResponse(RiskAssessmentResponse response)
        {
            try
            {
                var raRep = new LearnerCourseRep();
                int result = raRep.SaveRAResponse(response);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        [HttpPost]
        [Route("SaveSubModuleRAResponse")]
        public ActionResult SaveSubModuleRAResponse([FromBody] RiskAssessmentResponse response)
        {
            try
            {
                var raRep = new LearnerCourseRep();
                int result = raRep.SaveRAResponse(response);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        // function to upload evidence
        [HttpPost]
        [Route("UploadEvidence")]
        public async Task<ActionResult> UploadEvidence(IFormFile newDocFile, [FromForm] RiskAssessmentEvidence evidence)
        {
            int status = 0;
            try
            {
                //check if document upload is valid
                if (newDocFile != null && newDocFile.Length > 0)
                {
                    //validate file size (<= 10MB)  10*1024*1024 = 10485760
                    if (newDocFile.Length > 10485760)
                    {
                        return Json(new { success = "File too large", status });
                    }

                    var result = await AsyncUploadFile(newDocFile, evidence);

                    evidence.EvidencePath = result;

                    var raRep = new LearnerCourseRep();
                    status = raRep.SaveRAEvidence(evidence);
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

        public async Task<string> AsyncUploadFile(IFormFile newDocFile, RiskAssessmentEvidence evidence)
        {
            var connectionString = Configuration["AppSettings:AZStorageConnectionString"];

            // create a client with the connection
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(Configuration["AppSettings:AZStorageContainer"]);

            string filename = "RA_Evidence_" + evidence.AnswerId + Path.GetExtension(newDocFile.FileName);
            string filetype = newDocFile.ContentType;

            BlobClient blobClient = containerClient.GetBlobClient(filename);

            var blobHttpHeader = new BlobHttpHeaders
            {
                ContentType = filetype
            };

            using (var stream = newDocFile.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, blobHttpHeader);
            }

            return blobClient.Uri.AbsoluteUri;
        }

        //set ra completed
        [HttpPost]
        [Route("SetRACompleted")]
        public ActionResult SetRACompleted(RiskAssessment learnerRA)
        {
            try
            {
                var raRep = new LearnerCourseRep();
                learnerRA.LearnerID = SessionHelper.UserId;
                int result = raRep.FinishtRiskAssessment(learnerRA);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        [HttpPost]
        [Route("SetSubModuleRACompleted")]
        public ActionResult SetSubModuleRACompleted([FromBody] RiskAssessment learnerRA)
        {
            try
            {
                var raRep = new LearnerCourseRep();
                learnerRA.LearnerID = SessionHelper.UserId;
                if (string.IsNullOrWhiteSpace(learnerRA.StrLocationName))
                {
                    learnerRA.StrLocationName = SessionHelper.CompanyName;
                }
                int result = raRep.FinishSubModuleRiskAssessment(learnerRA);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }
    }
}
