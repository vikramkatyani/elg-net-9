using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.Model.SuperAdmin
{
    public class Dashboard
    {
    }

    public class DashboardHeader
    {
        public int ActiveCompanies { get; set; }
        public int ActiveLearners { get; set; }
        public int LoggedInUsers { get; set; }
        public int TotalModules { get; set; }
    }

    public class DashboardYearlyData
    {
        public int Month { get; set; }
        public string MonthName { get; set; }
        public int CompletionCount { get; set; }
    }

    public class DashboardWeeklyData
    {
        public int WeeklyCompleted { get; set; }
        public int WeeklyInProgress { get; set; }
        public int WeeklyNotStarted { get; set; }
    }

    public class DashboardLicenceUsage
    {
        public string CourseName { get; set; }
        public int TotalLicences { get; set; }
        public int ConsumedLicences { get; set; }
        public int UsagePercentage { get; set; }

    }
}
