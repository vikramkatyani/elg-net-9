using System;

namespace ELG.Model.Learner
{
    public class LearnerInfo
    {
        public Int64 UserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
        public string Password { get; set; }
        public string CompanyNumber { get; set; }
        public byte[] CompanyLogo { get; set; }
        public byte[] CompanyCertificate { get; set; }
        public byte[] ProfilePic { get; set; }
        public Int64 CompanyId { get; set; }
        public string CompanyName { get; set; }
        public bool IsDeactivated { get; set; }
        public bool IsCompanyActive { get; set; }
        public bool IsPasswordReset { get; set; }
        public bool HasAdminRights { get; set; }
        public int AccidentIncidentFeature { get; set; }
        public string MenuItems { get; set; }
    }
    public class LearnerSSOInfo
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


    public class NewLearner
    {
        public Int64 UserID { get; set; }
        public string CourseWithNoLicences { get; set; }
    }

    public class CompanyContractor
    {
        public string ContractorName { get; set; }
        public string ContractorEmail { get; set; }
        public string TrainerName { get; set; }
        public string TrainerEmail { get; set; }
    }

    public class LearnerProfile
    {
        public Int64 UserID { get; set; }
        public string EmployeeNumber { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public Int64 CompanyId { get; set; }
        public string CompanyNumber { get; set; }
        public string CompanyName { get; set; }
        public Int64 LocationId { get; set; }
        public string Location { get; set; }
        public Int64 DepartmentId { get; set; }
        public string Department { get; set; }
    }

    public class CreateLearnerFields
    {
        public string TitleDesc { get; set; }
        public string FirstNameDesc { get; set; }
        public string LastNameDesc { get; set; }
        public string EmailDesc { get; set; }
        public string LocationDes { get; set; }
        public string DepartmentDesc { get; set; }
        public string EmployeeNumberDesc { get; set; }
    }


    public class LearnerBrowserDetails
    {
        public Int64 UserID { get; set; }
        public string Browser { get; set; }
        public string BrowserVersion { get; set; }
        public string OS { get; set; }
        public string Device { get; set; }
        public string BrowserDetails { get; set; }
        public bool IsMobileDevice { get; set; }
        public DateTime ActivityDate { get; set; }
    }

    public class LearnerCourseLaunchRecord : LearnerBrowserDetails
    {
        public Int64 CourseId { get; set; }
        public String CourseName { get; set; }
    }
}
