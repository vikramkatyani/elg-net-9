using ELG.DAL.DbEntityLearner;
using ELG.Model.Learner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.DAL.LearnerDAL
{
    public class DashboardRep
    {
        /// <summary>
        /// Get dashboard header info for learner
        /// </summary>
        /// <param name="OrganisationId"></param>
        /// <param name="Learner"></param>
        /// <returns></returns>
        public DashboardHeader GetLearnerDashboardHeader(Int64 OrganisationId, Int64 Learner)
        {
            try
            {
                DashboardHeader header = new DashboardHeader();
                using (var context = new learnerDBEntities())
                {
                    var result = context.lms_learner_get_dash_HeaderInfo(OrganisationId, Learner).FirstOrDefault();
                    if (result != null)
                    {
                        header.DocumentCount = Convert.ToInt32(result.TotalDocuments);
                        header.HSCount = Convert.ToInt32(result.HSECount);
                        header.ModuleCount = Convert.ToInt32(result.OverdueCourses);
                        header.LastLoginDate = result.LastLoginDate == null ? "" : (Convert.ToDateTime(result.LastLoginDate)).ToString("dd-MMM-yyyy");
                    }
                }
                return header;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Yearly completion records
        /// </summary>
        /// <param name="Learner"></param>
        /// <returns></returns>
        public List<DashboardYearlyData> GetLearnerDashboardYearlyCompletion(Int64 Learner)
        {
            try
            {
                List<DashboardYearlyData> infoList = new List<DashboardYearlyData>();
                string[] MonthNames = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
                for (int i = 0; i < 12; i++)
                {
                    DashboardYearlyData infoInit = new DashboardYearlyData();
                    infoInit.Month = i + 1;
                    infoInit.MonthName = MonthNames[i];
                    infoInit.CompletionCount = 0;
                    infoList.Add(infoInit);
                }

                using (var context = new learnerDBEntities())
                {
                    var result = context.lms_learner_get_dash_YearlyCompletion(Learner).ToList();
                    if (result != null && result.Count > 0)
                    {
                        foreach (var item in result)
                        {
                            infoList.First(x => x.Month == item.Months).CompletionCount = Convert.ToInt32(item.CompletionCount);
                        }
                    }
                }
                return infoList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Weekly progress records
        /// </summary>
        /// <param name="Learner"></param>
        /// <returns></returns>
        public DashboardWeeklyData GetLearnerDashboardWeeklyProgress(Int64 Learner)
        {
            try
            {
                DashboardWeeklyData info = new DashboardWeeklyData();
                using (var context = new learnerDBEntities())
                {
                    var result = context.lms_learner_get_dash_WeeklyProgress(Learner).FirstOrDefault();
                    if (result != null)
                    {
                        info.WeeklyCompleted = Convert.ToInt32(result.CompletedThisWeek);
                        info.WeeklyInProgress = Convert.ToInt32(result.InProgressThisWeek);
                        info.WeeklyNotStarted = Convert.ToInt32(result.NotStarted);
                    }
                }
                return info;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// get list of all modules with course progress
        /// </summary>
        /// <param name="Learner"></param>
        /// <returns></returns>
        public List<DashboardCourseCompletions> GetDashboardCourseCompletions(Int64 Learner)
        {
            try
            {
                List<DashboardCourseCompletions> progressRecord = new List<DashboardCourseCompletions>();

                using (var context = new learnerDBEntities())
                {
                    var resultList = context.lms_learner_get_dash_course_completion(Learner).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        foreach (var item in resultList)
                        {
                            DashboardCourseCompletions info = new DashboardCourseCompletions();
                            info.CourseName = item.strCourse;
                            info.CourseStatus = item.strStatus;
                            progressRecord.Add(info);
                        }
                    }
                }
                return progressRecord;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get ATF's notification
        /// </summary>
        /// <returns></returns>
        public string GetDashboardNotification()
        {
            try
            {
                string notification = "";

                using (var context = new learnerDBEntities())
                {
                    notification = context.lms_learner_get_dash_notification().FirstOrDefault();
                }
                return notification;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
