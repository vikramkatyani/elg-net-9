using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.Model.OrgAdmin
{
    public class LearnerInfo
    {
        public Int64 UserID { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
        public string EmployeeNumber { get; set; }
        public string Location { get; set; }
        public string Department { get; set; }
        public Int64 CompanyID { get; set; }
        public Int64 LocationID { get; set; }
        public Int64 DepartmentID { get; set; }
        public Boolean IsDeactive { get; set; }
        public String Status { get; set; }
        public Boolean IsCourseExpired { get; set; }
        public Boolean IsRASignedOff { get; set; }
    }

    public class NewLearner
    {
        public Int64 UserID { get; set; }
        public Int32 AssignementMode { get; set; }
        public string CourseWithNoLicences { get; set; }
    }

    public class CompanyContractor
    {
        public string ContractorName { get; set; }
        public string ContractorEmail { get; set; }
        public string TrainerName { get; set; }
        public string TrainerEmail { get; set; }
    }

    public class DownloadLearnerList
    {
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
        public string Location { get; set; }
        public string Department { get; set; }
        public string EmployeeNumber { get; set; }
        public String Status { get; set; }
    }

    public class OrganisationLearnerList
    {
        public List<LearnerInfo> LearnerList { get; set; }
        public int TotalLearners { get; set; }
    }

    public class LearnerAdminRights
    {
        public Int64 UserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<int> AdminLevels { get; set; }
    }

    public class LearnerLocationtWithAdminRights
    {
        public Int64 LocationID { get; set; }
        public string LocationName { get; set; }
        public bool HasRights { get; set; }
    }

    public class LearnerDepartmentWithAdminRights
    {
        public Int64 DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public Int64 LocationID { get; set; }
        public string LocationName { get; set; }
        public bool HasRights { get; set; }
    }
}
