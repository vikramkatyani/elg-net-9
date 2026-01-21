using ELG.DAL.DbEntityLearner;
using ELG.Model.Learner;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.DAL.LearnerDAL
{
    public class SCORMRep
    {
        public CourseProgress InitialiseCourse(Int64 course, Int64 learner)
        {
            try
            {
                CourseProgress progress = new CourseProgress();

                using (learnerDBEntities context = new learnerDBEntities())
                {
                    var result = context.lms_learner_initializeModule(learner, course).FirstOrDefault();
                    if (result != null)
                    {
                        progress.Bookmark = result.strBookMark;
                        progress.CourseId = result.intCourseID;
                        progress.ProgressStatus = result.strStatus;
                        progress.Score = Convert.ToInt32(result.intScore);
                        //progress.PassingScore = Convert.ToInt32(result.intPassmark);
                        progress.SuspendData = result.strSuspendData;
                        progress.UserFistName = result.strFirstName;
                        progress.UserLastName = result.strSurname;
                        progress.UserId = result.intContactID;
                    }
                }
                return progress;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //public string SaveProgressDetails(CourseProgressData SaveData)
        //{
        //    string success = "1";
        //    try
        //    {
        //        using (learnerDBEntities context = new learnerDBEntities())
        //        {
        //            var result = context.lms_learner_updateCourseProgressRecord(SaveData.UserId, SaveData.CourseId, SaveData.Action, SaveData.Value);
        //                success = "1";
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    return success;
        //}
        public CourseProgressResponse SaveProgressDetails(CourseProgress SaveData)
        {
            CourseProgressResponse response = new CourseProgressResponse();
            response.SendABSCertificate = 0;
            response.Success = "1";
            int score = (int)Math.Round(SaveData.Score);
            try
            {
                ObjectParameter sendITPCertificate = new ObjectParameter("sendITPCertificate", typeof(int));
                using (learnerDBEntities context = new learnerDBEntities())
                {
                    var result = context.lms_learner_updateCourseProgressRecord_withSession(SaveData.UserId, SaveData.CourseId, SaveData.Bookmark, SaveData.ProgressStatus, SaveData.SuspendData, score, SaveData.SessionTime, sendITPCertificate);
                    response.SendABSCertificate = Convert.ToInt32(sendITPCertificate.Value);
                    response.Success = "1";
                }
            }
            catch (Exception)
            {
                throw;
            }
            return response;
        }

        public string TrackCourseExit(CourseProgressData SaveData)
        {
            string success = "1";
            try
            {
                using (learnerDBEntities context = new learnerDBEntities())
                {
                    var result = context.lms_learner_courseSessionEndRecord(SaveData.UserId, SaveData.CourseId);
                        success = "1";
                }
            }
            catch (Exception)
            {
                throw;
            }
            return success;
        }

        public int TrackCourseLaunch(LearnerCourseLaunchRecord learner)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("insertRecordId", typeof(long));
                using (var context = new learnerDBEntities())
                {
                    var result = context.lms_learner_insert_course_launch_record(learner.UserID, learner.Browser, learner.BrowserVersion, learner.OS, learner.Device, learner.BrowserDetails, learner.IsMobileDevice, learner.CourseId, learner.CourseName, retVal);
                    success = Convert.ToInt32(retVal.Value);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return success;
        }

        public string PostCHSEScore(int userId, int moduleId, int passingMarks, int scoredMarks)
        {
            string success = "1";
            try
            {
                using (learnerDBEntities context = new learnerDBEntities())
                {
                    var result = context.lms_learner_update_CourseProgressScore(userId, moduleId, passingMarks, scoredMarks);
                    success = "1";
                }
            }
            catch (Exception)
            {
                throw;
            }
            return success;
        }
    }
}
