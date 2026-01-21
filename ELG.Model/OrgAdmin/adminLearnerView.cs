using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.Model.OrgAdmin
{
    public class AdminLearnerView
    {
        public string SearchText { get; set; }
        public Int64 SearchLocation { get; set; }
        public Int64 SearchDepartment { get; set; }
        public int SearchStatus { get; set; }
        public Int64 Company { get; set; }
        public int PageNumber { get; set; }
    }

    public class AdminLearnerFilter: AdminLearnerView
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

    public class OrganisationCourse
    {
        public Int64 CourseId { get; set; }
        public string CourseName { get; set; }
    }
    public class CourseSubModule
    {
        public Int64 CourseId { get; set; }
        public Int64 SubModuleId { get; set; }
        public string SubModuleName { get; set; }
    }

    public class OrganisationLocation
    {
        public Int64 LocationId { get; set; }
        public String LocationName { get; set; }
    }

    public class OrganisationDepartment
    {
        public Int64 LocationId { get; set; }
        public Int64 DepartmentId { get; set; }
        public String DepartmentName { get; set; }
    }

}
