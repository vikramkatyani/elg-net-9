using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.Model.OrgAdmin
{
    public class SubModule
    {
        public Int64 SubModuleID { get; set; }
        public Int64 CourseId { get; set; }
        public Int64 CreatedById { get; set; }
        public string SubModuleName { get; set; }
        public string SubModuleDesc { get; set; }
        public string SubModulePath { get; set; }
        public int Sequence { get; set; }
        public Int64 RAID { get; set; }
    }
    public class Module
    {
        public Int64 ModuleID { get; set; }
        public string ModuleName { get; set; }
        public string ModuleDesc { get; set; }
        public string CourseLogo { get; set; }
        public string CoursePath { get; set; }
        public int PassingMarks { get; set; }
        public int Frequency { get; set; }
        public int RAFrequency { get; set; }
        public int CompletionDays { get; set; }
        public int SubModuleCount { get; set; }
    }

    public class OrgModuleList
    {
        public List<Module> ModuleList { get; set; }
        public int TotalModules { get; set; }
    }

    public class LearnerModuleFilter:DataTableFilter
    {
        public Int64 LearnerID { get; set; }
    }

    public class LearnerProgressResetRequest
    {
        public LearnerModuleFilter SearchCriteria { get; set; }
        public bool AllSelected { get; set; }
        public long[] SelectedRecordList { get; set; }
        public long[] UnselectedRecordList { get; set; }
    }

    public class ModuleLicenceSummary: Module
    {
        public int TotalLicenses { get; set; }
        public int AllocatedLicenses { get; set; }
        public int FreeLicenses { get; set; }
        public int UsedLicenses { get; set; }
        public int DeletedLicenses { get; set; }
        public int AvailableToRevokeLicenses { get; set; }
    }

    public class LicenseSummaryReport
    {
        public List<ModuleLicenceSummary> ModuleList { get; set; }
        public int TotalModules { get; set; }
    }

    public class DownloadLicenceSummary
    {
        public string Course { get; set; }
        public int TotalLicenses { get; set; }
        public int AllocatedLicenses { get; set; }
        public int AvailableLicenses { get; set; }
        public int UsedLicenses { get; set; }
        public int DeletedLicenses { get; set; }
        public int AvailableToRevokeLicenses { get; set; }
    }



    public class LicenseTransactionFilter : DataTableFilter
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class LicenseTransactionItem
    {
        public Int64 TransactionId { get; set; }
        public string ModuleName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Instructions { get; set; }
        public string StrAdditionalText { get; set; }
        public string Email { get; set; }
        public string Action { get; set; }
        public int LicenseCount { get; set; }
        public string ActionDate { get; set; }
        public string TransBy_FirstName { get; set; }
        public string TransBy_LastName { get; set; }
        public string TransBy_Email { get; set; }
    }


    public class DownloadLicenceTransactionReport
    {
        public string Date { get; set; }
        public string Course { get; set; }
        public string Action { get; set; }
        public int LicenseCount { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string TransBy_FirstName { get; set; }
        public string TransBy_LastName { get; set; }
        public string TransBy_Email { get; set; }
    }
    public class LicenseTransactionReport
    {
        public List<LicenseTransactionItem> TransactionSummary { get; set; }
        public int TotalItems { get; set; }
    }

    public class DepartmentFilterForLicenseAutoAssignment
    {
        public Int64 Course { get; set; }
        public Int64 Organisation { get; set; }
        public Int64 Location { get; set; }
        public string Departments { get; set; }
    }

    public class LocationFilterForLicenseAutoAssignment
    {
        public Int64 Course { get; set; }
        public Int64 Organisation { get; set; }
        public string Locations { get; set; }
    }

    public class DepartmentForLicenseAutoAssignment
    {
        public Int64 DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public bool Assigned { get; set; }
    }
    public class DepartmentListForModuleAutoAssignment
    {
        public List<DepartmentForLicenseAutoAssignment> DepartmentList { get; set; }
        public int TotalDepartments { get; set; }
    }

    public class LocationForLicenseAutoAssignment
    {
        public Int64 LocationId { get; set; }
        public string LocationName { get; set; }
        public bool Assigned { get; set; }
    }
    public class LocationListForModuleAutoAssignment
    {
        public List<LocationForLicenseAutoAssignment>LocationList { get; set; }
        public int TotalLocations { get; set; }
    }

    public class RiskAssessmentQuestion
    {
        public Int64 QuestionId { get; set; }
        public Int64 CourseId { get; set; }
        public Int64 GroupId { get; set; }
        public string QuestionText { get; set; }
        public string Instructions { get; set; }
        public int QuestionType { get; set; }
        public int Order { get; set; }
        public int GotoNO { get; set; }
        public int GotoYes { get; set; }
        public int NumberFrom { get; set; }
        public int NumberTo { get; set; }
        public int OptionCount { get; set; }
        public bool FreeText { get; set; }
        public bool MultiLine { get; set; }
        public bool End { get; set; }
        public bool BarChart { get; set; }
        public bool BaseQuestion { get; set; }
        public string Group { get; set; }
        public string CourseName { get; set; }
    }

    public class RiskAssessmentGroup
    {
        public Int64 GroupID { get; set; }
        public Int64 CourseID { get; set; }
        public string GroupName { get; set; }
    }

    public class RiskAssessmentGroupList
    {
        public List<RiskAssessmentGroup> RiskAssessmentGroups { get; set; }
        public int TotalGroups { get; set; }
    }

    public class RiskAssessmentQuestionOption
    {
        public Int64 QuestionOptionId { get; set; }
        public Int64 QuestionId { get; set; }
        public string OptionText { get; set; }
        public bool Issue { get; set; }
        public int Order { get; set; }
    }



    public class ResellerLicenceReportFilter : DataTableFilter
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class ResellerLicenseTransactionItem : LicenseTransactionItem
    {
        public int TransationType { get; set; }
        public Int64 OrganisationId { get; set; }
        public string OrganisationName{ get; set; }
    }
    public class ResellerLicenseTransactionReport
    {
        public List<ResellerLicenseTransactionItem> TransactionSummary { get; set; }
        public int TotalItems { get; set; }
    }
    public class ResellerLicenseTransactionDownloadItem
    {
        public string OrganisationName{ get; set; }
        public string ModuleName { get; set; }
        public string Action { get; set; }
        public int LicenseCount { get; set; }
        public string ActionDate { get; set; }
    }
}
