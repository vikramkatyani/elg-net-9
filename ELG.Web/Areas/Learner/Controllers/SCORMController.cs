using ELG.DAL.LearnerDAL;
using ELG.DAL.Utilities;
using ELG.Web.Helper;
using ELG.Model.Learner;
using Microsoft.AspNetCore.Mvc;
using System;

namespace ELG.Web.Areas.Learner.Controllers
{
    [Area("Learner")]
    public class SCORMController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult LMSInitialize([FromBody] InitCourse course)
        {
            SessionHelper.CourseId = course.CourseId;

            Int64 contactid = Convert.ToInt64(SessionHelper.UserId);
            Int64 courseid = Convert.ToInt64(SessionHelper.CourseId);

            CourseProgress progress = new CourseProgress();

            if (contactid > 0 && courseid > 0)
            {
                try
                {
                    var progressRep = new SCORMRep();
                    progress = progressRep.InitialiseCourse(courseid, contactid);

                    // Track browser details
                    LearnerCourseLaunchRecord record = GetLearnerLaunchDetails(courseid, contactid);
                    progressRep.TrackCourseLaunch(record);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message, ex);
                }
            }

            return Json(new { progress });
        }

        private LearnerCourseLaunchRecord GetLearnerLaunchDetails(Int64 courseid, Int64 contactid)
        {
            var userAgent = Request.Headers["User-Agent"].ToString();
            string browserDetails = "Browser Capabilities\n"
                + "User-Agent = " + userAgent + "\n";

            LearnerCourseLaunchRecord learner = new LearnerCourseLaunchRecord
            {
                UserID = contactid,
                Browser = "",
                BrowserVersion = "",
                OS = "",
                Device = Request.Headers["User-Agent"].ToString(),
                BrowserDetails = browserDetails,
                IsMobileDevice = false,
                CourseId = courseid
            };

            return learner;
        }

        [HttpPost]
        public IActionResult LMSSaveData([FromBody] CourseProgress DataValue)
        {
            CourseProgressResponse response = new CourseProgressResponse
            {
                Success = "0",
                SendABSCertificate = 0
            };

            if (DataValue != null)
            {
                DataValue.UserId = Convert.ToInt64(SessionHelper.UserId);
                DataValue.CourseId = Convert.ToInt64(SessionHelper.CourseId);

                if (DataValue.UserId > 0 && DataValue.CourseId > 0)
                {
                    try
                    {
                        var progressRep = new SCORMRep();
                        response = progressRep.SaveProgressDetails(DataValue);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex.Message, ex);
                    }
                }
                else
                {
                    // Session not available
                    response.Success = "-1";
                }
            }

            return Json(new { success = response.Success });
        }

        [HttpPost]
        public IActionResult LMSFinish([FromBody] CourseProgressData DataValue)
        {
            string success = "0";

            if (DataValue != null)
            {
                DataValue.UserId = Convert.ToInt64(SessionHelper.UserId);
                DataValue.CourseId = Convert.ToInt64(SessionHelper.CourseId);

                if (DataValue.UserId > 0 && DataValue.CourseId > 0)
                {
                    try
                    {
                        var progressRep = new SCORMRep();
                        success = progressRep.TrackCourseExit(DataValue);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex.Message, ex);
                    }
                }
                else
                {
                    // Session not available
                    success = "-1";
                }
            }

            return Json(new { success });
        }

        [HttpPost]
        public IActionResult PostCHSEScore(int userId, int moduleId, int passingMarks, int scoredMarks)
        {
            string success = "0";

            if (userId > 0 && moduleId > 0)
            {
                try
                {
                    var progressRep = new SCORMRep();
                    success = progressRep.PostCHSEScore(userId, moduleId, passingMarks, scoredMarks);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message, ex);
                }
            }

            return Json(new { success });
        }
    }
}
