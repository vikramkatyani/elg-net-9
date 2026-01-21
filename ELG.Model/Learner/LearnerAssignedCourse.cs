using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.Model.Learner
{
    public class LearnerAssignedCourse
    {
        public Int64 CourseId { get; set; }
        public string CourseName { get; set; }
        public string CourseDesc { get; set; }
        public string CourseSummary { get; set; }
        public string CourseLogo { get; set; }
        public string CoursePath { get; set; }
        public bool IsCourseRA { get; set; }
        public int TestFrequency { get; set; }
        public Int64 ProgressRecordId { get; set; }
        public string ProgressStatus { get; set; }
        public int Score { get; set; }
        public string StartedOn { get; set; }
        public string LastAccessedOn { get; set; }
        public string CompletedOn { get; set; }
        public string AssignedOn { get; set; }
        public string CourseCompleteBy { get; set; }
        public string CourseResetOn { get; set; }
        public string CourseExpiryDate { get; set; }
        public int IsExpired { get; set; }
        public bool SelfCourseResetEnabled { get; set; }
        public int SubModuleCount { get; set; }
        public List<SubModule> SubModuleList { get; set; }
    }
    public class SubModule
    {
        public Int64 SubModuleID { get; set; }
        public Int64 CourseId { get; set; }
        public Int64 RAID { get; set; }
        public string SubModuleName { get; set; }
        public string SubModuleDesc { get; set; }
        public string SubModulePath { get; set; }
        public string SubModuleStatus { get; set; }
        public string SubModuleAccessDate { get; set; }
        public int Sequence { get; set; }
    }

    public class LearnerRACourse
    {
        public Int64 CourseId { get; set; }
        public string CourseName { get; set; }
        public string LocationName { get; set; }
        public int RAFrequency { get; set; }
        public int RAResultID { get; set; }
        public string RAStartedOn { get; set; }
        public string RADueDate { get; set; }
        public string RACompletedOn { get; set; }
        public int IssueCount { get; set; }
        public string RASignOffDate { get; set; }
    }

    public class LearnerRACourseList
    {
        public List<LearnerRACourse> RAList { get; set; }
        public int TotalCount { get; set; }
        public int IncompleteRACount { get; set; }
    }

    public class LearnerCertificateRecord
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CourseName { get; set; }
        public Int64 RecordId { get; set; }
        public string CourseStatus { get; set; }
        public int Score { get; set; }
        public int CPDScore { get; set; }
        public string CompletionDate { get; set; }
        public string CertificateNumber { get; set; }
    }
}
