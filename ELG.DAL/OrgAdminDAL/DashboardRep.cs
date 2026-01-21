using ELG.DAL.DBEntity;
using ELG.Model.OrgAdmin;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.DAL.OrgAdminDAL
{
    public class DashboardRep
    {
        /// <summary>
        /// Get Dashboard header info for company admin
        /// </summary>
        /// <param name="OrganisationId"></param>
        /// <returns></returns>
        public DashboardHeader GetAdminDashboardHeader(Int32 AdminRole, Int64 AdminId, Int64 OrganisationId)
        {
            try
            {
                DashboardHeader header = new DashboardHeader();
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_get_dash_HeaderInfo(AdminRole, AdminId, OrganisationId).FirstOrDefault();
                    if (result != null)
                    {
                        header.DocumentCount = Convert.ToInt32(result.TotalDocuments);
                        header.HSCount = Convert.ToInt32(result.HSECount);
                        header.ModuleCount = Convert.ToInt32(result.TotalCourses);
                        header.AcciInciCount = Convert.ToInt32(result.TotalAcciInci);
                        header.CPDScore = Convert.ToInt32(result.CPDscore);
                        header.MaxUsers = Convert.ToInt32(result.MaxUsers);
                        header.MaxLocationCount = Convert.ToInt32(result.MaxLocations);
                        header.MaxCourseCount = Convert.ToInt32(result.MaxCourses);
                        header.TotalUsers = Convert.ToInt32(result.TotalUsers);
                        header.TotalLocations = Convert.ToInt32(result.LocationCount);
                        header.RenewalDate = result.RenewalDate == null ? "" : (Convert.ToDateTime(result.RenewalDate)).ToString("dd-MMM-yyyy");
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
        /// <param name="OrganisationId"></param>
        /// <returns></returns>
        //public List<DashboardYearlyData> GetAdminDashboardYearlyCompletion(Int32 AdminRole, Int64 AdminId, Int64 OrganisationId)
        //{
        //    try
        //    {
        //        List<DashboardYearlyData> infoList = new List<DashboardYearlyData>();
        //        string[] MonthNames = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        //        for(int i=0; i<12; i++)
        //        {
        //            DashboardYearlyData infoInit = new DashboardYearlyData();
        //            infoInit.Month = i + 1;
        //            infoInit.MonthName = MonthNames[i];
        //            infoInit.CompletionCount = 0;
        //            infoList.Add(infoInit);
        //        }

        //        using (var context = new lmsdbEntities())
        //        {
        //            var result = context.lms_admin_get_dash_YearlyCompletion(AdminRole, AdminId, OrganisationId).ToList();
        //            if (result != null && result.Count > 0)
        //            {
        //                foreach(var item in result)
        //                {
        //                    infoList.First(x => x.Month == item.Months).CompletionCount = Convert.ToInt32(item.CompletionCount);
        //                }
        //            }
        //        }
        //        return infoList;
        //    }
        //    catch (Exception)   
        //    {
        //        throw;
        //    }
        //}
        public List<DashboardYearlyData> GetAdminDashboardYearlyCompletion(int AdminRole, long AdminId, long OrganisationId)
        {
            try
            {
                List<DashboardYearlyData> infoList = new List<DashboardYearlyData>();
                string[] MonthNames = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

                DateTime start = DateTime.Now.AddMonths(-8);
                for (int i = 0; i < 12; i++)
                {
                    DateTime refMonth = start.AddMonths(i);
                    infoList.Add(new DashboardYearlyData
                    {
                        Year = refMonth.Year,
                        Month = refMonth.Month,
                        MonthName = MonthNames[refMonth.Month - 1] + " " + refMonth.Year,
                        CompletionCount = 0,
                        AssignmentCount = 0,
                        UserCount = 0
                    });
                }

                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_get_dash_YearlyCompletion(AdminRole, AdminId, OrganisationId).ToList();
                    if (result != null && result.Count > 0)
                    {
                        foreach (var item in result)
                        {
                            var match = infoList.FirstOrDefault(x => x.Year == item.Year && x.Month == item.Month);
                            if (match != null)
                            {
                                match.CompletionCount = Convert.ToInt32(item.CompletionCount);
                                match.AssignmentCount = Convert.ToInt32(item.AssignmentCount);
                                match.UserCount = Convert.ToInt32(item.UserCount);
                            }
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
        /// <param name="OrganisationId"></param>
        /// <returns></returns>
        public DashboardWeeklyData GetAdminDashboardWeeklyProgress(Int32 AdminRole, Int64 AdminId, Int64 OrganisationId)
        {
            try
            {
                DashboardWeeklyData info = new DashboardWeeklyData();
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_get_dash_WeeklyProgress(AdminRole, AdminId, OrganisationId).FirstOrDefault();
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
        /// Weekly progress records
        /// </summary>
        /// <param name="OrganisationId"></param>
        /// <returns></returns>
        public DashboardCourseCompletion GetAdminDashboardCourseCompletions(Int32 AdminRole, Int64 AdminId, Int64 OrganisationId)
        {
            try
            {
                DashboardCourseCompletion info = new DashboardCourseCompletion();
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_get_dash_courseCompletionData(AdminRole, AdminId, OrganisationId).FirstOrDefault();
                    if (result != null)
                    {
                        info.TotalAssignment = Convert.ToInt32(result.TotalAssignments);
                        info.Completed = Convert.ToInt32(result.TotalCompletions);
                       // info.CompletionPercentage = (result.TotalAssignments > 0) ? Convert.ToInt32((result.TotalCompletions * 100) / result.TotalAssignments) : 0;
                    }
                }
                return info;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<LocationUserQuota> GetAdminDashboardUserQuotaPerLocation(long OrganisationId)
        {
            try
            {
                List<LocationUserQuota> records = new List<LocationUserQuota>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_get_location_user_distribution(OrganisationId).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        foreach (var item in resultList)
                        {
                            LocationUserQuota info = new LocationUserQuota
                            {
                                LocationId = item.intLocationId,
                                LocationName = item.strLocation,
                                UsedUsers = item.UsedUsers ?? 0,
                                TotalQuota = item.TotalQuota ?? 0,
                                RemainingUsers = Math.Max(0, (item.TotalQuota ?? 0) - (item.UsedUsers ?? 0))
                            };
                            records.Add(info);
                        }
                    }
                }

                return records;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// get list of all modules with license usage
        /// </summary>
        /// <param name="organisation"></param>
        /// <returns></returns>
        public List<DashboardLicenceUsage> GetDashboardLicenseUsage(Int64 organisation)
        {
            try
            {
                List<DashboardLicenceUsage> licensesRecord = new List<DashboardLicenceUsage>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_get_dash_licenceUsage(organisation).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        foreach (var item in resultList)
                        {
                            DashboardLicenceUsage info = new DashboardLicenceUsage();
                            info.CourseName = item.strCourse;
                            info.TotalLicences = item.AssignedLicenses;
                            info.ConsumedLicences = item.ConsumedLicenses;
                            info.UsagePercentage = (item.AssignedLicenses > 0) ? Convert.ToInt32((item.ConsumedLicenses  * 100)/ item.AssignedLicenses) : 0;
                            licensesRecord.Add(info);
                        }
                    }
                }
                return licensesRecord;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<DashboardCourseCompletion> GetDashboardCourseCompletion(Int64 organisation)
        {
            try
            {
                List<DashboardCourseCompletion> completionRecord = new List<DashboardCourseCompletion>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_get_dash_course_completion(organisation).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        foreach (var item in resultList)
                        {
                            DashboardCourseCompletion info = new DashboardCourseCompletion();
                            info.CourseName = item.strCourse;
                            info.TotalAssignment = Convert.ToInt32(item.TotalAssignments);
                            info.Completed = Convert.ToInt32(item.TotalCompletions);
                            info.CompletionPercentage = (item.TotalAssignments > 0) ? Convert.ToInt32((item.TotalCompletions * 100)/ item.TotalAssignments) : 0;
                            completionRecord.Add(info);
                        }
                    }
                }
                return completionRecord;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<DashboardCourseAssignment> GetDashboardCourseAssignments(Int64 organisation)
        {
            try
            {
                List<DashboardCourseAssignment> assignedRecord = new List<DashboardCourseAssignment>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_get_dash_course_assignments(organisation).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        foreach (var item in resultList)
                        {
                            DashboardCourseAssignment info = new DashboardCourseAssignment();
                            info.CourseName = item.strCourse;
                            info.TotalAssignment = Convert.ToInt32(item.TotalAssignments);
                            info.Completed = Convert.ToInt32(item.TotalCompletions);
                            info.TotalUsers = Convert.ToInt32(item.maxUsers);
                            info.AssignmentPercentage = (item.TotalAssignments > 0) ? Convert.ToInt32((item.TotalAssignments * 100) / item.maxUsers) : 0;
                            assignedRecord.Add(info);
                        }
                    }
                }
                return assignedRecord;
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

                using (var context = new lmsdbEntities())
                {
                    notification = context.lms_admin_get_dash_notification().FirstOrDefault();
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
