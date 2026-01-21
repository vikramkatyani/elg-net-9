using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.Model.OrgAdmin
{
    public class OrgAdminInfo
    {
        public Int64 UserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
        public string Password { get; set; }
        public string CompanyNumber { get; set; }
        public Int64 CompanyId { get; set; }
        public String CompanyName { get; set; }
        public byte[] ProfilePic { get; set; }
        public byte[] CompanyLogo { get; set; }
        public byte[] CompanyCertificate { get; set; }
        public bool IsDeactivated { get; set; }
        public bool IsCompanyActive { get; set; }
        public String Roles { get; set; }
        public bool IsPasswordReset { get; set; }
        public int AccidentIncidentFeature { get; set; }
        public int TrainingRenewalMode { get; set; }
        public int CourseAssignMode { get; set; }
        public string MenuItems { get; set; }
    }

    public class OrdAdminSSOInfo
    {
        public Int64 UserID { get; set; }
        public Int64 OrgId { get; set; }
        public string SSOURL { get; set; }
        public int SSOType { get; set; }
        public string EntityId { get; set; }
        public string EntityKey { get; set; }
        public string Cert { get; set; }
        public string ReturnURL { get; set; }
    }

    public class AdminLearnerEmailInfo
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
        public string AdminLevelName { get; set; }
        public string OrganisationName { get; set; }
        public string OrganisationBrandName { get; set; }
        public string SupportEmail { get; set; }
        public string BaseURL { get; set; }
    }
    public class AdminLearner
    {
        public Int64 UserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmployeeNumber { get; set; }
        public string EmailId { get; set; }
        public int AdminLevel { get; set; }
        public String AdminLevelName { get; set; }
    }

    public class DownloadAdminLearner
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string EmployeeNumber { get; set; }
        public string AdminLevel { get; set; }
        public string LocationAdminRights { get; set; }
        public string DepartmentAdminRights { get; set; }
        public string LocationSuperVisorRights { get; set; }
        public string DepartmentSupervisorRights { get; set; }
    }

    public class AdminLearnerList
    {
        public List<AdminLearner> AdminList { get; set; }
        public int TotalAdmins { get; set; }
    }

    public class AdminLearnerListFilter
    {
        public string SearchText { get; set; }
        public Int64 Company { get; set; }
        public int AdminLevel { get; set; }
        public string Draw { get; set; }
        public string Start { get; set; }
        public string Length { get; set; }
        public string SortCol { get; set; }
        public string SortColDir { get; set; }
        public int PageSize { get; set; }
        public int Skip { get; set; }
        public int RecordTotal { get; set; }
    }

    public class OrganizationInfo
    {
        public int OrgId { get; set; }
        public string strTitleDescription { get; set; }
        public string strFirstNameDescription { get; set; }
        public string strSurnameDescription { get; set; }
        public string emailIdDescription { get; set; }
        public string strDepartmentDescription { get; set; }
        public string strLocationDescription { get; set; }
        public string strEmployeeNumberDescription { get; set; }
        public bool? PayrollMandatory { get; set; }
        public string PayrollDescription { get; set; }
        public bool? JobtitleMandatory { get; set; }
        public string JobtitleDescription { get; set; }
        public string RegSettings { get; set; }
        public bool UploadDocEnabled { get; set; }
        public int AllowedUploadCount { get; set; }
        public int CourseAssignMode { get; set; }
    }

    public class AdminPrivilege
    {
        public int AdminRole { get; set; }
        public List<int> LocationAccess { get; set; }
        public List<int> LocationSpvAccess { get; set; }
        public List<DepartmentAdminRights> DepartmentAccess { get; set; }
        public List<DepartmentAdminRights> DepartmentSpvAccess { get; set; }
    }

    public class DepartmentAdminRights
    {
        public int LocationID { get; set; }
        public int DepartmentID { get; set; }
    }

    public class CompanyDomainDetails
    {
        public Int64 CompanyId { get; set; }
        public string Domain { get; set; }
        public string Favicon { get; set; }
        public string CSS { get; set; }
        public string TitleText { get; set; }
        public string LogoPath { get; set; }
    }
}
