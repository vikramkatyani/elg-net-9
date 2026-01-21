using ELG.Web.Helper;
using ELG.DAL.OrgAdminDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ELG.Model.OrgAdmin;
using ELG.DAL.Utilities;
using System.Collections.Concurrent;
using System.IO;

namespace ELG.Web.Controllers
{
    [SessionCheck]
    public class RiskAssessmentController : Controller
    {
        private static readonly ConcurrentDictionary<string, RiskAssessmentExcelParser.RAParsed> _drafts = new ConcurrentDictionary<string, RiskAssessmentExcelParser.RAParsed>();
        public ActionResult AllocateRiskAssessment()
        {
            return View("AllocateRiskAssessment");
        }

        // GET: Risk Assessment Auto Allocation
        public ActionResult RiskAssessmentAutoAllocation()
        {
            return View("RiskAssessmentAutoAllocation");
        }

        // GET: Risk Assessment Auto Allocation
        public ActionResult RevokeRiskAssessment()
        {
            return View("RevokeRiskAssessment");
        }

        // GET: Risk Assessment Auto Allocation
        public ActionResult ConfigureRiskAssessment()
        {
            return View("ConfigureRiskAssessment");
        }

        // update frequency of risk assessment
        [HttpPost]
        public ActionResult UpdateFrequency(Module module)
        {
            try
            {
                var moduleRep = new ModuleRep();
                int result = moduleRep.UpdateRAFrequency(module.ModuleID, module.RAFrequency, module.CompletionDays);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        #region Risk Assessment
        // GET: list of all learners to whom the selected module is not assigned
        public ActionResult ManageRiskAssessment()
        {
            return View("ManageRiskAssessment");
        }

        // get report on applied filter
        [HttpPost]
        public ActionResult LoadRAModuleList(DataTableFilter searchCriteria)
        {
            OrgModuleList orgModuleList = new OrgModuleList();
            orgModuleList.ModuleList = new List<Module>();
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
                var orderColIndex = Request.Form["order[0][column]"].FirstOrDefault();
                searchCriteria.SortCol = Request.Form[$"columns[{orderColIndex}][name]"].FirstOrDefault();
                searchCriteria.SortColDir = Request.Form["order[0][dir]"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                orgModuleList = moduleRep.GetRiskAssessmentModules(searchCriteria);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = orgModuleList.TotalModules, recordsTotal = orgModuleList.TotalModules, data = orgModuleList.ModuleList });

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = orgModuleList.TotalModules, recordsTotal = orgModuleList.TotalModules, data = orgModuleList.ModuleList });
            }
        }

        #endregion

        #region Create RA (Org Admin)
        [HttpPost]
        public async Task<ActionResult> CreateRiskAssessment([FromForm] string Title, [FromForm] string Description, [FromForm] IFormFile Thumbnail)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Title))
                {
                    return Json(new { success = 0, message = "Risk Assessment title is required." });
                }

                var moduleRep = new ModuleRep();

                // Upload thumbnail if provided
                string connectionString = CommonHelper.GetAppSettingValue("AZStorageConnectionString");
                string courseContentContainer = CommonHelper.GetAppSettingValue("AzureStorage:CourseContentContainer") ?? "elg-learn";
                string thumbnailContainer = CommonHelper.GetAppSettingValue("AzureStorage:ThumbnailContainer") ?? "elg-content";

                string thumbnailPath = string.Empty;
                if (!string.IsNullOrEmpty(connectionString) && Thumbnail != null && Thumbnail.Length > 0)
                {
                    try
                    {
                        var azureStorage = new AzureStorageUtility(connectionString, courseContentContainer, thumbnailContainer);
                        string extension = System.IO.Path.GetExtension(Thumbnail.FileName);
                        extension = string.IsNullOrWhiteSpace(extension) ? string.Empty : extension;
                        string id = Guid.NewGuid().ToString();
                        thumbnailPath = $"thumbnails/thumbnail_ra_{id}{extension}";

                        using (var stream = Thumbnail.OpenReadStream())
                        {
                            await azureStorage.UploadBlobAsync(thumbnailPath, stream, Thumbnail.ContentType, isThumbnail: true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("RA thumbnail upload failed: " + ex.Message, ex);
                        // continue without thumbnail
                    }
                }

                int companyId = (int)SessionHelper.CompanyId;
                int courseId = moduleRep.CreateRiskAssessmentCourse(companyId, Title, Description ?? string.Empty, thumbnailPath);

                if (courseId > 0)
                {
                    return Json(new { success = 1, message = "Risk Assessment created successfully.", id = courseId });
                }
                else
                {
                    return Json(new { success = 0, message = "Failed to create Risk Assessment." });
                }
            }
            catch (Exception ex)
            {
                Logger.Error("CreateRiskAssessment error: " + ex.Message, ex);
                return Json(new { success = -1, message = "An error occurred: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult> UploadDraft([FromForm] IFormFile File)
        {
            try
            {
                if (File == null || File.Length == 0)
                {
                    return Json(new { success = 0, message = "Please select a valid file." });
                }

                using (var ms = new MemoryStream())
                {
                    await File.CopyToAsync(ms);
                    ms.Position = 0;
                    var parsed = RiskAssessmentExcelParser.Parse(ms);
                    if (parsed == null || parsed.Sections.Count == 0)
                    {
                        return Json(new { success = 0, message = "No questions found in the file. Ensure the file has correct headers and at least one question row." });
                    }
                    string key = Guid.NewGuid().ToString("N");
                    _drafts[key] = parsed;
                    return Json(new { success = 1, draftKey = key, data = parsed });
                }
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error("UploadDraft validation error: " + ex.Message, ex);
                return Json(new { success = 0, message = ex.Message });
            }
            catch (Exception ex)
            {
                Logger.Error("UploadDraft error: " + ex.Message, ex);
                return Json(new { success = -1, message = "Failed to parse file. Ensure it is a valid Excel (.xlsx) or CSV file with required columns: Section, Question, Instructions, Option1-Option6, Issue." });
            }
        }

        [HttpPost]
        public ActionResult DiscardDraft([FromForm] string draftKey)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(draftKey))
                    return Json(new { success = 0, message = "Invalid draft key." });

                _drafts.TryRemove(draftKey, out _);
                return Json(new { success = 1 });
            }
            catch (Exception ex)
            {
                Logger.Error("DiscardDraft error: " + ex.Message, ex);
                return Json(new { success = -1, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult> PublishDraft([FromForm] string draftKey, [FromForm] long parentCourseId, [FromForm] string Title, [FromForm] string Description, [FromForm] IFormFile Thumbnail)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(draftKey) || !_drafts.TryGetValue(draftKey, out var parsed))
                {
                    return Json(new { success = 0, message = "Draft not found. Please re-upload the file." });
                }
                if (string.IsNullOrWhiteSpace(Title))
                {
                    return Json(new { success = 0, message = "Risk Assessment title is required." });
                }
                if (parentCourseId <= 0)
                {
                    return Json(new { success = 0, message = "Invalid parent course." });
                }

                var moduleRep = new ModuleRep();

                // Upload thumbnail if provided
                string connectionString = CommonHelper.GetAppSettingValue("AZStorageConnectionString");
                string courseContentContainer = CommonHelper.GetAppSettingValue("AzureStorage:CourseContentContainer") ?? "elg-learn";
                string thumbnailContainer = CommonHelper.GetAppSettingValue("AzureStorage:ThumbnailContainer") ?? "elg-content";

                string thumbnailPath = string.Empty;
                if (!string.IsNullOrEmpty(connectionString) && Thumbnail != null && Thumbnail.Length > 0)
                {
                    try
                    {
                        var azureStorage = new AzureStorageUtility(connectionString, courseContentContainer, thumbnailContainer);
                        string extension = System.IO.Path.GetExtension(Thumbnail.FileName);
                        extension = string.IsNullOrWhiteSpace(extension) ? string.Empty : extension;
                        string id = Guid.NewGuid().ToString();
                        thumbnailPath = $"thumbnails/thumbnail_ra_{id}{extension}";

                        using (var stream = Thumbnail.OpenReadStream())
                        {
                            await azureStorage.UploadBlobAsync(thumbnailPath, stream, Thumbnail.ContentType, isThumbnail: true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("PublishDraft thumbnail upload failed: " + ex.Message, ex);
                        // proceed without thumbnail
                    }
                }

                int companyId = (int)SessionHelper.CompanyId;
                int raCourseId = moduleRep.CreateRiskAssessmentCourse(companyId, Title, Description ?? string.Empty, thumbnailPath);
                if (raCourseId <= 0)
                {
                    return Json(new { success = 0, message = "Failed to create Risk Assessment course." });
                }

                // Persist groups/questions/options
                int savedGroups = 0;
                int savedQuestions = 0;
                int savedOptions = 0;
                foreach (var section in parsed.Sections)
                {
                    var group = new ELG.Model.OrgAdmin.RiskAssessmentGroup
                    {
                        CourseID = raCourseId,
                        GroupName = section.Name
                    };
                    int groupId = moduleRep.CreateNewRAGroup(group);
                    if (groupId <= 0) continue;
                    savedGroups++;

                    foreach (var ques in section.Questions)
                    {
                        var q = new ELG.Model.OrgAdmin.RiskAssessmentQuestion
                        {
                            GroupId = groupId,
                            Group = section.Name,
                            QuestionText = ques.Question,
                            CourseId = raCourseId
                        };
                        int questionId = moduleRep.CreateNewRAQuestion(q);
                        if (questionId <= 0) continue;
                        savedQuestions++;

                        foreach (var opt in ques.Options)
                        {
                            var o = new ELG.Model.OrgAdmin.RiskAssessmentQuestionOption
                            {
                                QuestionId = questionId,
                                OptionText = opt.Text,
                                Issue = opt.Issue,
                                Order = opt.Order
                            };
                            var optId = moduleRep.CreateNewRAQuestionOption(o);
                            if (optId > 0) savedOptions++;
                        }
                    }
                }

                // Map RA as submodule to the parent course
                var sm = new ELG.Model.OrgAdmin.SubModule
                {
                    CourseId = parentCourseId,
                    SubModuleName = Title,
                    SubModuleDesc = Description,
                    SubModulePath = string.Empty,
                    CreatedById = Convert.ToInt64(SessionHelper.UserId),
                    RAID = raCourseId
                };
                int mapResult = moduleRep.CreateNewSubModule(sm);

                // cleanup draft
                _drafts.TryRemove(draftKey, out _);

                if (mapResult > 0)
                {
                    return Json(new { success = 1, message = "Risk Assessment published and mapped successfully.", id = raCourseId, savedGroups, savedQuestions, savedOptions });
                }
                else
                {
                    return Json(new { success = 0, message = "Risk Assessment created but mapping failed.", id = raCourseId, savedGroups, savedQuestions, savedOptions });
                }
            }
            catch (Exception ex)
            {
                Logger.Error("PublishDraft error: " + ex.Message, ex);
                return Json(new { success = -1, message = ex.Message });
            }
        }
        #endregion

        #region Edit Risk Assessment
        public ActionResult EditRiskAssessment(Int64? id)
        {
            try
            {
                SessionHelper.CourseId = Convert.ToInt64(id);
                return View();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View();
            }
        }

        // GET: list of all risk assessment question for a course
        [HttpPost]
        public ActionResult GetRiskAssessmentQuestion()
        {
            List<RiskAssessmentQuestion> quesList = new List<RiskAssessmentQuestion>();
            try
            {
                var moduleRep = new ModuleRep();
                quesList = moduleRep.GetModuleRiskAssessmentQuestion(SessionHelper.CourseId);

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }

            return Json(new { quesList });
        }

        // GET: list of all option of risk assessment question for a course
        [HttpPost]
        public ActionResult GetRiskAssessmentQuestionOptions(DataTableFilter searchCriteria)
        {
            List<RiskAssessmentQuestionOption> optionList = new List<RiskAssessmentQuestionOption>();
            try
            {

                searchCriteria.Draw = Request.Form["draw"].FirstOrDefault();
                searchCriteria.Start = Request.Form["start"].FirstOrDefault();
                searchCriteria.Length = Request.Form["length"].FirstOrDefault();

                var moduleRep = new ModuleRep();
                optionList = moduleRep.GetRAQuestionOptionList(searchCriteria.Course);

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }

            return Json(new { draw = searchCriteria.Draw, recordsFiltered = optionList.Count, recordsTotal = optionList.Count, data = optionList });
        }

        #endregion

        #region Risk Assessment Group
        // create Risk assessment course group
        [HttpPost]
        public ActionResult CreateRAGroup(RiskAssessmentGroup group)
        {
            try
            {
                if (String.IsNullOrEmpty(group.GroupName))
                    return View();

                var raRep = new ModuleRep();
                group.CourseID = SessionHelper.CourseId;
                int result = raRep.CreateNewRAGroup(group);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View();
            }
        }
        #endregion

        #region Risk Assessment Question
        // Create Risk assessment course question
        [HttpPost]
        public ActionResult CreateRAQuestion(Int64 GroupId, string Group, string Question)
        {
            try
            {
                if (String.IsNullOrEmpty(Question))
                    return View();

                var raRep = new ModuleRep();
                RiskAssessmentQuestion raQue = new RiskAssessmentQuestion();
                raQue.GroupId = GroupId;
                raQue.Group = Group;
                raQue.QuestionText = Question;
                raQue.CourseId = SessionHelper.CourseId;
                int result = raRep.CreateNewRAQuestion(raQue);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View();
            }
        }

        // get ra question info
        [HttpPost]
        public ActionResult GetRAQuestionInfo(Int64 QuestionId)
        {
            RiskAssessmentQuestion que = new RiskAssessmentQuestion();
            try
            {
                var raRep = new ModuleRep();
                que = raRep.GetRiskAssessmentQuestionInfo(QuestionId);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }

            return Json(new { que });
        }

        // create option for question
        [HttpPost]
        public ActionResult CreateRAQuestionOption(Int64 QuestionId, string Option, bool Issue, int Order)
        {
            try
            {
                if (String.IsNullOrEmpty(Option))
                    return View();

                var raRep = new ModuleRep();
                RiskAssessmentQuestionOption option = new RiskAssessmentQuestionOption();
                option.QuestionId = QuestionId;
                option.OptionText = Option;
                option.Issue = Issue;
                option.Order = Order;
                int result = raRep.CreateNewRAQuestionOption(option);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View();
            }
        }

        // delete ra question
        [HttpPost]
        public ActionResult RemoveRAQuestion(Int64 QuestionId)
        {
            int result = 0;
            try
            {
                var raRep = new ModuleRep();
                result = raRep.RemoveRiskAssessmentQuestion(QuestionId);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }

            return Json(new { success = result });
        }

        //delete ra question option
        [HttpPost]
        public ActionResult DeleteOption(Int64 optionID)
        {
            int result = 0;
            try
            {
                var raRep = new ModuleRep();
                result = raRep.RemoveRiskAssessmentQuestionOption(optionID);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }

            return Json(new { success = result });
        }

        //update option details
        [HttpPost]
        public ActionResult UpdateOption(RiskAssessmentQuestionOption option)
        {
            int result = 0;
            try
            {
                var raRep = new ModuleRep();
                result = raRep.UpdateRiskAssessmentQuestionOption(option);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }

            return Json(new { success = result });
        }

        [HttpPost]
        public ActionResult UpdateRAQuestion(RiskAssessmentQuestion question)
        {
            int result = 0;
            try
            {
                var raRep = new ModuleRep();
                question.CourseId = SessionHelper.CourseId;
                result = raRep.UpdateRiskAssessmentQuestion(question);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }

            return Json(new { success = result });
        }
        #endregion

    }
}