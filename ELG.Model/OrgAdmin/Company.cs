using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.Model.OrgAdmin
{
    public class Company
    {
        public Int64 CompanyId { get; set; }
        public string CompanyNumber { get; set; }
        public string CompanyName { get; set; }
        public string CompanyBrandName { get; set; }
        public string CompanyBaseURL { get; set; }
        public string CompanySupportEmail { get; set; }
        public bool Cancelled { get; set; }
        public bool Live { get; set; }
        public string Status { get; set; }
        public string Settings { get; set; }
        public bool AllowedSelfRegistration { get; set; }
        //public List<CompanyStructuralComponent> CompanyStructure { get; set; }
    }

    public class CompanyReminderConfiguration
    {
        public Int64 CompanyId { get; set; }
        public int DaysBefore { get; set; }
        public int Frequency { get; set; }
        public int MaxReminders { get; set; }
    }

    public class CompanyNotificationSettings
    {
        public Int64 CompanyId { get; set; }
        public bool SendCoursePassedToLocationAdmin { get; set; }
        public bool SendCoursePassedToDepartmentAdmin { get; set; }
        public bool SendCourseFailedToLocationAdmin { get; set; }
        public bool SendCourseFailedToDepartmentAdmin { get; set; }
        public bool SendRACompletionToLocationAdmin { get; set; }
        public bool SendRACompletionToDepartmentAdmin { get; set; }
        public bool SendCourseOverdueToLocationAdmin { get; set; }
        public bool SendCourseOverdueToDepartmentAdmin { get; set; }
        public bool SendRAOverDueToLocationAdmin { get; set; }
        public bool SendRAOverDueToDepartmentAdmin { get; set; }
    }

    public class CompanyModuleAssigmentNotificationSettings
    {
        public Int64 CompanyId { get; set; }
        public bool SendCourseAssignmentMailToLearner { get; set; }
    }

    public class CompanyStructuralComponent
    {
        public Int64 LocationId { get; set; }
        public String LocationName { get; set; }
        public Int64 DepartmentId { get; set; }
        public String DepartmentName { get; set; }
    }

    public class CompanyBusinessCertificate
    {
        public string CompanyName { get; set; }
        public string AssignedDate { get; set; }
        public string ExpiryDate { get; set; }
    }

    public class ResellerCompany: Company
    {
        public string AdminFirstname { get; set; }
        public string AdminSurname { get; set; }
        public string AdminEmail { get; set; }
        public string CreationDate { get; set; }
        public string ExpiryDate { get; set; }
    }

    public class ResellerCompanyList
    {
        public List<ResellerCompany> CompanyList { get; set; }
        public int TotalRecords { get; set; }
    }

    public class ResellerCompanyLicence
    {
        public string CourseName { get; set; }
        public int AssignedLicences { get; set; }
        public int ConsumedLicences { get; set; }
    }

    public class ResellerCompanyLicenceList
    {
        public List<ResellerCompanyLicence> LicenceList { get; set; }
        public int TotalRecords { get; set; }
    }
}
