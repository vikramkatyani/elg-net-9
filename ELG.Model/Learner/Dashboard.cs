using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.Model.Learner
{
    public class Dashboard
    {
    }

    public class DashboardHeader
    {
        public string LastLoginDate { get; set; }
        public int ModuleCount { get; set; }
        public int DocumentCount { get; set; }
        public int HSCount { get; set; }
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

    public class DashboardCourseCompletions
    {
        public string CourseName { get; set; }
        public string CourseStatus { get; set; }

    }
}
