using ELG.DAL.DBEntitySA;
using ELG.Model.SuperAdmin;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.DAL.SuperAdminDAL
{
    public class DashboardRep
    {
        /// <summary>
        /// Get dashboard header items for super admin
        /// </summary>
        /// <returns></returns>
        public DashboardHeader GetAdminDashboardHeader()
        {
            try
            {
                DashboardHeader header = new DashboardHeader();
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_get_dash_HeaderInfo().FirstOrDefault();
                    if (result != null)
                    {
                        header.TotalModules = Convert.ToInt32(result.TotalModules);
                        header.LoggedInUsers = Convert.ToInt32(result.LatestLoggedInUsers);
                        header.ActiveLearners = Convert.ToInt32(result.TotalActiveUsers);
                        header.ActiveCompanies = Convert.ToInt32(result.TotalActiveCompanies);
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
        public List<DashboardYearlyData> GetAdminDashboardYearlyCompletion()
        {
            try
            {
                List<DashboardYearlyData> infoList = new List<DashboardYearlyData>();
                string[] MonthNames = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
                for(int i=0; i<12; i++)
                {
                    DashboardYearlyData infoInit = new DashboardYearlyData();
                    infoInit.Month = i + 1;
                    infoInit.MonthName = MonthNames[i];
                    infoInit.CompletionCount = 0;
                    infoList.Add(infoInit);
                }

                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_get_dash_YearlyCompletion().ToList();
                    if (result != null && result.Count > 0)
                    {
                        foreach(var item in result)
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
        /// <param name="OrganisationId"></param>
        /// <returns></returns>
        public DashboardWeeklyData GetAdminDashboardWeeklyProgress()
        {
            try
            {
                DashboardWeeklyData info = new DashboardWeeklyData();
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_get_dash_WeeklyProgress().FirstOrDefault();
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
    }
}
