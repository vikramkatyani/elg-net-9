using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.Model.SuperAdmin
{
    class Learner
    {
    }

    public class LearnerInfo
    {
        public Int64 UserID { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
        public string Company { get; set; }
        public string CompanyNumber { get; set; }
        public string Location { get; set; }
        public string Department { get; set; }
        public Int64 CompanyID { get; set; }
        public Int64 LocationID { get; set; }
        public Int64 DepartmentID { get; set; }
        public Boolean IsDeactive { get; set; }
        public String Status { get; set; }
        public Boolean IsCourseExpired { get; set; }
    }

    public class DownloadLearnerList
    {
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
        public string Company { get; set; }
        public string Location { get; set; }
        public string Department { get; set; }
        public String Status { get; set; }
    }

    public class OrganisationLearnerList
    {
        public List<LearnerInfo> LearnerList { get; set; }
        public int TotalLearners { get; set; }
    }


    public class CourseProgressItem : LearnerInfo
    {
        public Int64 RecordId { get; set; }
        public Int64 Course { get; set; }
        public String CourseName { get; set; }
        public string CourseStatus { get; set; }
        public int Score { get; set; }
        public string StartDate { get; set; }
        public string CompletionDate { get; set; }
        public string AssignedOn { get; set; }
        public string CompleteBy { get; set; }
        public string SuspendDataString { get; set; }
    }

    public class ProgressCertificate: CourseProgressItem
    {
        public byte[] Certificate { get; set; }
    }

    public class CourseProgressReport
    {
        public List<CourseProgressItem> ProgressRecords { get; set; }
        public int TotalRecords { get; set; }
    }

    public class DownloadCourseProgressReport
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
        public string Company { get; set; }
        public string Location { get; set; }
        public string Department { get; set; }
        public String CourseName { get; set; }
        public string CourseStatus { get; set; }
        public int Score { get; set; }
        public string CompletionDate { get; set; }
    }

    public class CourseRAReportItem : LearnerInfo
    {
        public Int64 RiskAssessmentId { get; set; }
        public Int64 Course { get; set; }
        public String CourseName { get; set; }
        public int Issue { get; set; }
        public string SignedOff { get; set; }
        public string SignedOffDate { get; set; }
        public string CompletionDate { get; set; }
    }

    public class CourseRAReport
    {
        public List<CourseRAReportItem> RiskAssessmentRecords { get; set; }
        public int TotalRecords { get; set; }
    }

    public class DownloadRAReport
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
        public string Company { get; set; }
        public string Location { get; set; }
        public string Department { get; set; }
        public String CourseName { get; set; }
        public int IssueCount { get; set; }
        public string SignedOff { get; set; }
        public string SignedOffDate { get; set; }
        public string CompletionDate { get; set; }
    }

    public class RiskAssessmentResult
    {
        public Int64 RiskAssessmentId { get; set; }
        public Int64 IssueCount { get; set; }
        public string UserNotes { get; set; }
        public string AdminNotes { get; set; }
        public string DateSignedOff { get; set; }
        public string DateCompleted { get; set; }
        public string Question { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public bool IsIssue { get; set; }
        public string Response { get; set; }
        public string ResponseNote { get; set; }
        public string SignedOfBy_FirstName { get; set; }
        public string SignedOfBy_LastName { get; set; }
        public string SignedOfBy_Email { get; set; }
    }

    public class LearnerLoginInfo
    {
        public Int64 RecordId { get; set; }
        public string LoginDate { get; set; }
        public string Browser { get; set; }
        public string BrowserVersion { get; set; }
        public string Device { get; set; }
        public string IsMobileDevice { get; set; }
        public string OS { get; set; }
    }
    public class LearnerLoginReport
    {
        public List<LearnerLoginInfo> LoginLog { get; set; }
        public int TotalRecords { get; set; }
    }

    public class LearnerLaunchInfo
    {
        public long RecordID { get; set; }
        public string LaunchTime { get; set; }
        public string Browser { get; set; }
        public string BrowserVersion { get; set; }
        public string Device { get; set; }
        public string IsMobileDevice { get; set; }
        public string CourseCloseTime { get; set; }
        public string OS { get; set; }
        public string CourseName { get; set; }
    }
    public class LearnerLaunchReport
    {
        public List<LearnerLaunchInfo> LaunchLog { get; set; }
        public int TotalRecords { get; set; }
    }
}
