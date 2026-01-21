using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.Model.OrgAdmin
{
    public class Dashboard
    {
    }

    public class DashboardHeader
    {
        public string RenewalDate { get; set; }
        public int ModuleCount { get; set; }
        public int DocumentCount { get; set; }
        public int HSCount { get; set; }
        public int AcciInciCount { get; set; }
        public int CPDScore { get; set; }
        public int MaxUsers { get; set; }
        public int MaxLocationCount { get; set; }
        public int MaxCourseCount { get; set; }
        public int TotalUsers { get; set; }
        public int TotalLocations { get; set; }
    }

    public class DashboardYearlyData
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; }
        public int CompletionCount { get; set; }
        public int AssignmentCount { get; set; }
        public int UserCount { get; set; }
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

    public class DashboardCourseCompletion
    {
        public string CourseName { get; set; }
        public int TotalAssignment { get; set; }
        public int Completed { get; set; }
        public int CompletionPercentage { get; set; }

    }

    public class DashboardCourseAssignment
    {
        public string CourseName { get; set; }
        public int TotalAssignment { get; set; }
        public int Completed { get; set; }
        public int TotalUsers { get; set; }
        public int AssignmentPercentage { get; set; }

    }

    public class LocationUserQuota
    {
        public int LocationId { get; set; }
        public string LocationName { get; set; }
        public int UsedUsers { get; set; }
        public int RemainingUsers { get; set; }
        public int TotalQuota { get; set; }
    }
}
