using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.Model.SuperAdmin
{

    public class ReportFilter
    {
        public string Draw { get; set; }
        public string Start { get; set; }
        public string Length { get; set; }
        public string SortCol { get; set; }
        public string SortColDir { get; set; }
        public int PageSize { get; set; }
        public int Skip { get; set; }
        public int RecordTotal { get; set; }
    }

    public class LearnerReportSearch: ReportFilter
    {
        public string SearchLearnerName { get; set; }
        public string SearchLearnerOrganisation { get; set; }
        public int SearchStatus { get; set; }
        public int PageNumber { get; set; }
    }

    public class ProgressReportSearch : ReportFilter
    {
        public string SearchLearnerOrganisation { get; set; }
        public string SearchLearnerName { get; set; }
        public string SearchStatus { get; set; }
        public int Course { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; }
    }

    public class LearnerRAReportFilter : ReportFilter
    {
        public string SearchLearnerOrganisation { get; set; }
        public string SearchLearnerName { get; set; }
        public int Course { get; set; }
        public int Issue { get; set; }
        public int SignedOff { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class OrganisationListingSearch : ReportFilter
    {
        public int Reseller { get; set; }
        public string OrganisationName { get; set; }
        public string OrganisationOwner { get; set; }
        public string OrganisationSector { get; set; }
        public int Status { get; set; }
    }

    public class SearchListing : ReportFilter
    {
        public string UID { get; set; }
        public string SearchText { get; set; }
    }

    public class CourseListingSearch : ReportFilter
    {
        public string CourseName { get; set; }
        public int Status { get; set; }
    }

    public class ModuleListingSearch : ReportFilter
    {
        public string ModuleName { get; set; }
        public int Status { get; set; }
    }

    public class LearnerLogReportSearch: ReportFilter
    {
        public Int64 LearnerId { get; set; }
        public Int64 CourseId { get; set; }
        public int PageNumber { get; set; }
    }

    public class DepartmentMapSearchListing : ReportFilter
    {
        public Int64 LocationId { get; set; }
        public string SearchText { get; set; }
    }

    public class CompnayCourseListingSearch : ReportFilter
    {
        public string OrgUID { get; set; }
        public string ModuleName { get; set; }
    }

    public class RAGroupListingSearch : ReportFilter
    {
        public string GroupName { get; set; }
        public Int64 RAId { get; set; }
    }

    public class RAQOptionListingSearch : ReportFilter
    {
        public string Option { get; set; }
        public Int64 RAId { get; set; }
        public Int64 QueId { get; set; }
    }

    public class OrganisationModuleSearch : ReportFilter
    {
        public int Reseller { get; set; }
        public string OrganisationName { get; set; }
        public int Status { get; set; }
        public Int64 Course { get; set; }
    }

    public class CRMOrganisationSearch : ReportFilter
    {
        public int Reseller { get; set; }
        public string OrganisationName { get; set; }
        public string Sector { get; set; }
        public int OrganisationStatus { get; set; }
        public string Course { get; set; }
        public int CourseStatus { get; set; }
        public int SelfRegisration { get; set; }
        public int DocUpload { get; set; }
        public int AcciInci { get; set; }
    }

    public class CRMOrganisationFilter : ReportFilter
    {
        public string Filter { get; set; }
    }
}
