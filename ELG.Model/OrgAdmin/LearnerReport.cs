using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.Model.OrgAdmin
{
    public class LearnerReport
    {
    }

    public class SessionAdminDetails
    {
        public int AdminRole { get; set; }
        public Int64 AdminUserId { get; set; }
    }

    public class DataTableFilter : SessionAdminDetails
    {
        public string Draw { get; set; }
        public string Start { get; set; }
        public string Length { get; set; }
        public string SortCol { get; set; }
        public string SortColDir { get; set; }
        public int PageSize { get; set; }
        public int Skip { get; set; }
        public int RecordTotal { get; set; }

        public string SearchText { get; set; }
        public Int64 Company { get; set; }
        public Int64 Location { get; set; }
        public Int64 Department { get; set; }
        public Int64 Course { get; set; }
    }

    public class DownloadReport
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
        public string EmployeeNumber { get; set; }
        public string Location { get; set; }
        public string Department { get; set; }
    }

    public class LearnerReportFilter : DataTableFilter
    {
        public int UserStatus { get; set; }
        public int AccessStatus { get; set; }
        public string Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
    public class LearnerSubModuleReportFilter : LearnerReportFilter
    {
        public Int64 SubModuleId { get; set; }
    }

    public class CourseProgressItem : LearnerInfo
    {
        public Int64 RecordId { get; set; }
        public Int64 Course { get; set; }
        public String CourseName { get; set; }
        public string AssignedOn { get; set; }
        public string CourseStatus { get; set; }
        public int Score { get; set; }
        public int CPDScore { get; set; }
        public string StartDate { get; set; }
        public string LastAccessedDate { get; set; }
        public string CompletionDate { get; set; }
        public string CertificateNumber { get; set; }
    }
    public class CourseSubModuleProgressItem: CourseProgressItem
    {
        public Int64 SubModuleId { get; set; }
        public String SubModuleName { get; set; }
    }

    public class WidgetProgressItem : LearnerInfo
    {
        public string CourseName { get; set; }
        public int QuesType { get; set; }
        public string Question { get; set; }
        public string AfterQuestion { get; set; }
        public string Response { get; set; }
        public string AfterResponse { get; set; }
        public string Response_1 { get; set; }
        public string Response_2 { get; set; }
        public string Response_3 { get; set; }
        public string FeedBackResponse { get; set; }
        public string FeedBackResponseText { get; set; }
    }

    public class CourseProgressItemForManualReminder : LearnerInfo
    {
        public Int64 RecordId { get; set; }
        public Int64 Course { get; set; }
        public String CourseName { get; set; }
        public string AssignedOn { get; set; }
        public string CourseStatus { get; set; }
        public string LatestReminderOn { get; set; }
    }

    public class CourseProgressReport
    {
        public List<CourseProgressItem> ProgressRecords { get; set; }
        public int TotalRecords { get; set; }
    }
    public class CourseSubModuleProgressReport
    {
        public List<CourseSubModuleProgressItem> ProgressRecords { get; set; }
        public int TotalRecords { get; set; }
    }
    public class CourseProgressItem_historic : CourseProgressItem
    {
        public string MovedToHistoryOn { get; set; }
    }
    public class CourseProgressReport_historic
    {
        public List<CourseProgressItem_historic> ProgressRecords { get; set; }
        public int TotalRecords { get; set; }
    }

    public class WidgeteProgressReport
    {
        public List<WidgetProgressItem> ProgressRecords { get; set; }
        public int TotalRecords { get; set; }
    }

    public class CourseProgressReportForManualReminder
    {
        public List<CourseProgressItemForManualReminder> ProgressRecords { get; set; }
        public int TotalRecords { get; set; }
    }

    public class DownloadCourseProgressReport
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
        public string EmployeeNumber { get; set; }
        public string Location { get; set; }
        public string Department { get; set; }
        public String CourseName { get; set; }
        public string AssignedOn { get; set; }
        public string LastAccessedOn { get; set; }
        public string CourseStatus { get; set; }
        public string CompletionDate { get; set; }
    }
    public class DownloadCourseProgressReport_Historic: DownloadCourseProgressReport
    {
        public string MovedToHistoryOn { get; set; }
    }

    public class LearnerRAReportFilter : DataTableFilter
    {
        public int Issue { get; set; }
        public int SignedOff { get; set; }
        public int RAStatus { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class CourseRAReportItem : LearnerInfo
    {
        public Int64 RiskAssessmentId { get; set; }
        public Int64 Course { get; set; }
        public String CourseName { get; set; }
        public String RAStatus { get; set; }
        public int Issue { get; set; }
        public string SignedOff { get; set; }
        public string AssignedOnDate { get; set; }
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
        public string Location { get; set; }
        public string Department { get; set; }
        public String CourseName { get; set; }
        public string AssignedOnDate { get; set; }
        public String RAStatus { get; set; }
        public int IssueCount { get; set; }
        public string SignedOff { get; set; }
        public string SignedOffDate { get; set; }
        public string CompletionDate { get; set; }
    }


    public class AnnouncementReportFilter : DataTableFilter
    {
        public int Read { get; set; }
        public String Title { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class AnnouncementReportItem : LearnerInfo
    {
        public String AnnouncementTitle { get; set; }
        public String ReadStatus { get; set; }
        public string ExpiryDare { get; set; }
        public string PublishedOnDate { get; set; }
        public string ViewedOnDate { get; set; }
    }

    public class AnnouncementReport
    {
        public List<AnnouncementReportItem> AnnouncementReportRecords { get; set; }
        public int TotalRecords { get; set; }
    }

    public class DownloadAnnouncementReport : DownloadReport
    {
        public string Title { get; set; }
        public string ViewedDate { get; set; }
        public string PublishedDate { get; set; }
        public string ReadStatus { get; set; }
    }

    public class DocumentReportFilter : DataTableFilter
    {
        public int Category { get; set; }
        public int Status { get; set; }
        public int? AssignmentStatus { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class DocumentReportItem : LearnerInfo
    {
        public Int64 DocID { get; set; }
        public string DocumentStatus { get; set; }
        public string DocumentAssignmentStatus { get; set; }
        public string ViewedOnDate { get; set; }
        public string StatusUpdatedOn { get; set; }
        public string FileName { get; set; }
        public int Category { get; set; }
        public string CategoryName { get; set; }
    }

    public class DocumentReport
    {
        public List<DocumentReportItem> DocumentReportRecords { get; set; }
        public int TotalRecords { get; set; }
    }

    public class DownloadDocumentReport
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
        public string EmployeeNumber { get; set; }
        public string Location { get; set; }
        public string Department { get; set; }
        public string Category { get; set; }
        public string File { get; set; }
        public string Issued { get; set; }
        public string Status { get; set; }
        public string ViewedDate { get; set; }
        public string StatusUpdatedOn { get; set; }
    }

    public class AdvanceCompliancyFilter : SessionAdminDetails
    {
        public Int64 Company { get; set; }
        public Int64 Location { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class AdvanceCompliancyItem
    {
        public Int64 Location { get; set; }
        public Int64 Department { get; set; }
        public Int64 CourseID { get; set; }
        public Int64 TotalUsers { get; set; }
        public Int64 AssignedTo { get; set; }
        public Int64 CompletedBy { get; set; }
    }

    public class AdvCompliancyViewModel
    {
        public IEnumerable<OrganisationCourse> Courses { get; set; }
        public IEnumerable<LocationCourseViewModel> LocationCourseCompliancy { get; set; }
    }

    public class LocationCourseViewModel
    {
        public Int64 LocationID { get; set; }
        public string LocationName { get; set; }
        public IEnumerable<AdvanceCompliancyItem> CompliancyItems { get; set; }
    }

    public class RiskAssessmentResult
    {
        public Int64 RiskAssessmentId { get; set; }
        public Int64 IssueCount { get; set; }
        public string UserNotes { get; set; }
        public string AdminNotes { get; set; }
        public string AdminNoteImagePath { get; set; }
        public string DateSignedOff { get; set; }
        public string AssignedOn { get; set; }
        public string DateCompleted { get; set; }
        public string Question { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public bool IsIssue { get; set; }
        public Int64 ResponseId { get; set; }
        public string Response { get; set; }
        public string ResponseNote { get; set; }
        public string Evidence { get; set; }
        public string FollowUpFeedback { get; set; }
        public string FollowedUpOn { get; set; }
        public string SignedOfBy_FirstName { get; set; }
        public string SignedOfBy_LastName { get; set; }
        public string SignedOfBy_Email { get; set; }
        public string LocationName { get; set; }
    }

    public class OverDueCourseItem : LearnerInfo
    {
        public Int64 Course { get; set; }
        public String CourseName { get; set; }
        public int Frequency { get; set; }
        public string AssignedOn { get; set; }
        public string OverDueDate { get; set; }
        public string CompletionDate { get; set; }
    }

    public class OverDueCourseReport
    {
        public List<OverDueCourseItem> OverDueCourseRecords { get; set; }
        public int TotalRecords { get; set; }
    }

    public class OverDueCourseDownloadItem
    {
        public Int64 UserID { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
        public Int64 Course { get; set; }
        public String CourseName { get; set; }
        public int Frequency { get; set; }
        public string AssignedOn { get; set; }
        public string OverDueDate { get; set; }
        public string CompletionDate { get; set; }
    }

    public class AccidentIncidentReportFilter : DataTableFilter
    {
        public int SignedOff { get; set; }
        public int IsEmployee { get; set; }
        public int IsPermitted { get; set; }
    }

    public class RAAdminNoteImage
    {
        public Int64 RAId { get; set; }
        public string ImagePath { get; set; }
    }

    public class CourseAutoAllocationDownloadItem
    {
        public String Course{ get; set; }
        public String Location{ get; set; }
        public String Department{ get; set; }
        public String AutoAllocation{ get; set; }
    }

    public class CourseAutoAllocationItem
    {
        public Int64 Course { get; set; }
        public String CourseName { get; set; }
        public Int64 Location { get; set; }
        public String LocationName { get; set; }
        public Int64 Department { get; set; }
        public String DepartmentName { get; set; }
        public String IsAutoAllocationOn { get; set; }
    }

    public class CourseAutoAllocationReport
    {
        public List<CourseAutoAllocationItem> AllocationRecords { get; set; }
        public int TotalRecords { get; set; }
    }

    public class TraineeInfo
    {
        public Int64 TraineeID { get; set; }
        public string TraineeName { get; set; }
        public string TraineeEmail { get; set; }
        public string TraineeLocation { get; set; }
        public string TraineeDepartment { get; set; }
        public string TraineePicURL { get; set; }
        public byte[] TraineePic { get; set; }
    }
     public class TraineeTestResult
    {
        public string CourseName { get; set; }
        public string LastAccessDate { get; set; }
        public string CompletionDate { get; set; }
        public string Status { get; set; }
        public int Score { get; set; }
        public bool IsRA { get; set; }
    }

    public class LearnerRACourse
    {
        public Int64 CourseId { get; set; }
        public string CourseName { get; set; }
        public int RAFrequency { get; set; }
        public int RAResultID { get; set; }
        public string RAStartedOn { get; set; }
        public string RADueDate { get; set; }
        public string RACompletedOn { get; set; }
        public int IssueCount { get; set; }
        public string RASignOffDate { get; set; }
    }

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
    }
    public class TraineeDocResult
    {
        public string DocName { get; set; }
        public string ViewedOn { get; set; }
        public string Comment { get; set; }
    }

    public class TrainingCard
    {
        public TraineeInfo Trainee { get; set; }
        public List<LearnerAssignedCourse> TestResult { get; set; }
        public List<LearnerRACourse> RAResult { get; set; }
        public List<TraineeDocResult> DocResult { get; set; }
    }


    public class AssignedCourseItem : LearnerInfo
    {
        public Int64 Course { get; set; }
        public Int64 ProgressRecordID { get; set; }
        public String CourseName { get; set; }
        public string AssignedOn { get; set; }
        public string CourseStatus { get; set; }
        public string CompletionDate { get; set; }
    }

    public class AssignedCourseReport
    {
        public List<AssignedCourseItem> AssignedRecords { get; set; }
        public int TotalRecords { get; set; }
    }

    public class CourseLearningStatistics
    {
        public Int64 CourseID { get; set; }
        public string Course { get; set; }
        public int Assigned { get; set; }
        public int Completed {  get; set; }
        public int Inprogress { get; set; }
        public int Notstarted { get; set; }
    }
    public class SummaryReportDownloadItem
    {
        public int LocationId { get; set; }
        public string Location { get; set; }
        public int CourseId { get; set; }
        public string Course { get; set; }
        public int TotalAssignments { get; set; }
        public int TotalCompleted { get; set; }
        public int TotalInProgress { get; set; }
        public int TotalNotStarted { get; set; }
    }
    public class CourseLearningStatistics_perLocation
    {
        public Int64 LocationId { get; set; }   
        public string Location { get; set; }
        public int Assigned { get; set; }
        public int Completed { get; set; }
    }
    public class CourseLearningStatistics_perDepartment
    {
        public string Department { get; set; }
        public int Assigned { get; set; }
        public int Completed { get; set; }
    }
}
