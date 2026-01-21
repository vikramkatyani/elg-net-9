using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.Model.SuperAdmin
{
    public class LMS_COURSE
    {
        public Int64 CourseId { get; set; }
        public string CourseName { get; set; }
        public string CourseDesc { get; set; }
        public string Status { get; set; }
    }

    public class LMSCourseListing
    {
        public List<LMS_COURSE> CourseList { get; set; }
        public int TotalRecords { get; set; }
    }
    public class CHSEModule
    {
        public Int64 ModuleId { get; set; }
        public string ModuleName { get; set; }
        public string ModuleDescription { get; set; }
        public string ModuleSummary { get; set; }
        public string Path { get; set; }
        public string Status { get; set; }
    }

    public class CHSEModuleListing
    {
        public List<CHSEModule> ModuleList { get; set; }
        public int TotalRecords { get; set; }
    }

    public class CHSECompnayCourse
    {
        public Int64 ModuleId { get; set; }
        public Int64 MainModuleId { get; set; }
        public string ModuleName { get; set; }
        public bool IsRaModule { get; set; }
        public int AssignedLicenses { get; set; }
        public int ConsumedLicenses { get; set; }
        public string CourseType { get; set; }
        public int AssignedStatus { get; set; }
    }

    public class CHSECompnayCourseListing
    {
        public List<CHSECompnayCourse> ModuleList { get; set; }
        public int TotalRecords { get; set; }
        public int TotalAssignedLicences { get; set; }
        public int TotalConsumedLicences { get; set; }
    }

    public class DownloadModuleLicnceReport
    {
        public string Course { get; set; }
        public string Type { get; set; }
        public int AssignedLicenses { get; set; }
        public int ConsumedLicenses { get; set; }
    }

    public class CHSEAssignCourseLicence
    {
        public String OrgUID { get; set; }
        public Int64 CourseId { get; set; }
        public Int32 NewLicenceCount { get; set; }
        public Int32 RemoveLicenceCount { get; set; }
        public bool IsRaModule { get; set; }
    }
}
