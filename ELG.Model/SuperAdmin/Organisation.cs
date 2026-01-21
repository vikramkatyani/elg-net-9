using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.Model.SuperAdmin
{
   public class Organisation
    {
        public Int64 OrganisationId { get; set; }
        public String OrganisationName { get; set; }
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
        public int Mapped { get; set; }
    }

    public class OrganisationInfo
    {
        public Int64 OrgId { get; set; }
        public int MaxAllowedUserCount { get; set; }
        public String OrganisationBaseURL { get; set; }
        public String OrgUId { get; set; }
        public string OrganisationName { get; set; }
        public string OrganisationBrandName { get; set; }
        public string OrganisationNumber { get; set; }
        public string SupportEmail { get; set; }
        public string SupportPhone { get; set; }
        public Int64 ResellerId { get; set; }
        public string Sector { get; set; }
        public string Rate { get; set; }
        public string Notes { get; set; }
        public string ExpiryDate { get; set; }
        public string ContractorName { get; set; }
        public string ContractorEmail { get; set; }
        public string TrainingManagerFirstName { get; set; }
        public string TrainingManagerLastName { get; set; }
        public string TrainingManagerEmail { get; set; }
        public string TrainingManagerPhone { get; set; }
        public string TrainingManagerExtension { get; set; }
        public string OrganisationOwner { get; set; }
        public string Status { get; set; }
        public bool IsReseller { get; set; }
    }

    public class OrganisationInfo_dropOff
    {
        public Int64 OrgId { get; set; }
        public string OrganisationName { get; set; }
        public string OrganisationNumber { get; set; }
        public string CreationDate { get; set; }
        public string ExpiryDate { get; set; }
        public string ContractorName { get; set; }
        public string ContractorEmail { get; set; }
        public string Status { get; set; }
        public int Learners { get; set; }
        public string LastLoginDate { get; set; }
        public string LastCourseLaunchDate { get; set; }
    }

    public class OrganisationListing
    {
        public List<OrganisationInfo> OrganisationList { get; set; }
        public int TotalRecords { get; set; }
    }

    public class OrganisationListing_dropOff
    {
        public List<OrganisationInfo_dropOff> OrganisationList { get; set; }
        public int TotalRecords { get; set; }
    }

    public class OrganisationLocationListing
    {
        public List<OrganisationLocation> LocationList { get; set; }
        public int TotalRecords { get; set; }
    }

    public class OrganisationDepartmentListing
    {
        public List<OrganisationDepartment> DepartmentList { get; set; }
        public int TotalRecords { get; set; }
    }

    public class OrganisationImage
    {
        public string OrgUid { get; set; }
        public byte[] Image { get; set; }
    }

    public class OrganisationDetailedInfo: OrganisationInfo
    {
        public bool DocUploadEnabled { get; set; }
        public int AllowedUploadCount { get; set; }
        public int AllowedAccidentIncidentFeature { get; set; }
        public int TrainingRenewalMode { get; set; }
        public int CourseAssignMode { get; set; }
        public bool SelfRegistrationEnabled { get; set; }
        public string TitleDescription { get; set; }
        public string FirstNameDescription { get; set; }
        public string LastNameDescription { get; set; }
        public string EmailDescription { get; set; }
        public string LocationDescription { get; set; }
        public string DepartmentDescription { get; set; }
    }

    public class DownloadOrganisationList
    {
        public string OrganisationName { get; set; }
        public string OrganisationNumber { get; set; }
        public string ContractManagerName { get; set; }
        public string ContractManagerEmail { get; set; }
        public string Sector { get; set; }
        public string Rate { get; set; }
        public string Notes { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Status { get; set; }
        public string Owner { get; set; }
    }

    public class DownloadOrganisationList_dropOff
    {
        public string OrganisationName { get; set; }
        public string OrganisationNumber { get; set; }
        public string ContractManagerName { get; set; }
        public string ContractManagerEmail { get; set; }
        public int Learners { get; set; }
        public string Status { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime LastLogin { get; set; }
        public DateTime LastCourseLaunch { get; set; }
    }

    public class DownloadOrganisationModuleReport
    {
        public string OrganisationName { get; set; }
        public string OrganisationNumber { get; set; }
        public string ContractManagerName { get; set; }
        public string ContractManagerEmail { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Status { get; set; }
        public string Course { get; set; }
        public string CourseStatus { get; set; }
    }

    public class OrganisationAdminInfo
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }

}
