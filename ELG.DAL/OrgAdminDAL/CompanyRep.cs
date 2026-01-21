using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ELG.Model.OrgAdmin;
using ELG.DAL.DBEntity;
using ELG.DAL.Utilities;
using System.Data.Entity.Core.Objects;
using ELG.DAL.DbEntityLearner;

namespace ELG.DAL.OrgAdminDAL
{
    public class CompanyRep
    {
       /// <summary>
       /// get company info
       /// </summary>
       /// <param name="compay"></param>
       /// <returns></returns>
        public Company GetCompanyInfo(Int64 compay)
        {
            try
            {
                Company company = new Company();

                using (var context = new lmsdbEntities())
                {
                    var companyInfo= context.lms_admin_getCompanyInfo(compay).FirstOrDefault();
                    if (companyInfo != null)
                    {
                        company.CompanyId = companyInfo.intOrganisationID;
                        company.CompanyNumber = companyInfo.UL_sequentialId;
                        company.CompanyName = companyInfo.strOrganisation;
                        company.CompanyBaseURL = companyInfo.orgBaseURL;
                        company.CompanyBrandName = companyInfo.orgBrandName;
                        company.CompanySupportEmail = companyInfo.supportEmail;
                        company.Cancelled = Convert.ToBoolean(companyInfo.blnCancelled);
                        company.Live = Convert.ToBoolean(companyInfo.UL_blnLive);
                        company.Status = companyInfo.UL_Status;
                        company.Settings = companyInfo.Reg_Settings;
                        company.AllowedSelfRegistration = Convert.ToBoolean(companyInfo.SelfRegistrationEnabled);
                    }
                }
                return company;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get test reminder info for a company
        /// </summary>
        /// <param name="company"></param>
        /// <returns></returns>
        public CompanyReminderConfiguration GetCompanyTestReminderInfo(Int64 company)
        {
            try
            {
                CompanyReminderConfiguration configuration = new CompanyReminderConfiguration();

                using (var context = new lmsdbEntities())
                {
                    var companyInfo = context.lms_admin_get_test_reminder_configuration(company).FirstOrDefault();
                    if (companyInfo != null)
                    {
                        configuration.CompanyId = companyInfo.intOrganisationId;
                        configuration.DaysBefore = (companyInfo.intTestReminder_DaysBefore == null) ? 0 : Convert.ToInt32(companyInfo.intTestReminder_DaysBefore);
                        configuration.Frequency = (companyInfo.intTestReminderFrequency == null) ? 0 : Convert.ToInt32(companyInfo.intTestReminderFrequency);
                        configuration.MaxReminders = (companyInfo.intMaxTestReminderCount == null) ? 0 : Convert.ToInt32(companyInfo.intMaxTestReminderCount);
                    }
                }
                return configuration;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// Update test reminder configuration for a company
        /// </summary>
        /// <param name="compay"></param>
        /// <returns></returns>
        public int UpdateCompanyTestReminderInfo(CompanyReminderConfiguration info)
        {
            int success = 0;
            try
            {
                ObjectParameter result = new ObjectParameter("result", typeof(long));
                using (var context = new lmsdbEntities())
                {
                    var response = context.lms_admin_update_test_reminder_configuration(info.CompanyId, info.DaysBefore, info.Frequency, info.MaxReminders, result);
                    success = Convert.ToInt32(result.Value);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return success;
        }

        /// <summary>
        /// get company reminder configuration
        /// </summary>
        /// <param name="compay"></param>
        /// <returns></returns>
        public CompanyReminderConfiguration GetCompanyRAReminderInfo(Int64 compay)
        {
            try
            {
                CompanyReminderConfiguration configuration = new CompanyReminderConfiguration();

                using (var context = new lmsdbEntities())
                {
                    var companyInfo = context.lms_admin_get_ra_reminder_configuration(compay).FirstOrDefault();
                    if (companyInfo != null)
                    {
                        configuration.CompanyId = companyInfo.intOrganisationId;
                        configuration.DaysBefore = (companyInfo.intRAReminder_DaysBefore == null) ? 0 : Convert.ToInt32(companyInfo.intRAReminder_DaysBefore);
                        configuration.Frequency = (companyInfo.intRAReminderFrequency == null) ? 0 : Convert.ToInt32(companyInfo.intRAReminderFrequency);
                        configuration.MaxReminders = (companyInfo.intMaxRAReminderCount == null) ? 0 : Convert.ToInt32(companyInfo.intMaxRAReminderCount);
                    }
                }
                return configuration;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Update Risk Assessment reminder configuration of a company
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public int UpdateCompanyRAReminderInfo(CompanyReminderConfiguration info)
        {
            int success = 0;
            try
            {
                ObjectParameter result = new ObjectParameter("result", typeof(long));
                using (var context = new lmsdbEntities())
                {
                    var response = context.lms_admin_update_ra_reminder_configuration(info.CompanyId, info.DaysBefore, info.Frequency, info.MaxReminders, result);
                    success = Convert.ToInt32(result.Value);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return success;
        }

        /// <summary>
        /// Get notification settings for a company
        /// </summary>
        /// <param name="compay"></param>
        /// <returns></returns>
        public CompanyNotificationSettings GetCompanyNotificationSettingsInfo(Int64 compay)
        {
            try
            {
                CompanyNotificationSettings configuration = new CompanyNotificationSettings();

                using (var context = new lmsdbEntities())
                {
                    var companyInfo = context.lms_admin_get_notification_settings(compay).FirstOrDefault();
                    if (companyInfo != null)
                    {
                        configuration.CompanyId = companyInfo.intOrganisationId;
                        configuration.SendCoursePassedToLocationAdmin = Convert.ToBoolean(companyInfo.sendPassedTo_LocationAdmin);
                        configuration.SendCoursePassedToDepartmentAdmin = Convert.ToBoolean(companyInfo.sendPassedTo_DepartmentAdmin);
                        configuration.SendCourseFailedToLocationAdmin = Convert.ToBoolean(companyInfo.sendCourseFailedTo_LocationAdmin);
                        configuration.SendCourseFailedToDepartmentAdmin = Convert.ToBoolean(companyInfo.sendCourseFailedTo_DepartmentAdmin);
                        configuration.SendRACompletionToLocationAdmin = Convert.ToBoolean(companyInfo.sendRACompletionTo_locationAdmin);
                        configuration.SendRACompletionToDepartmentAdmin = Convert.ToBoolean(companyInfo.sendRACompletionTo_DepartmentAdmin);
                        configuration.SendCourseOverdueToLocationAdmin = Convert.ToBoolean(companyInfo.sendOverDueTo_locationAdmin);
                        configuration.SendCourseOverdueToDepartmentAdmin = Convert.ToBoolean(companyInfo.sendOverDueTo_DepartmentAdmin);
                        configuration.SendRAOverDueToLocationAdmin = Convert.ToBoolean(companyInfo.sendRAOverdueTo_LocationAdmin);
                        configuration.SendRAOverDueToDepartmentAdmin = Convert.ToBoolean(companyInfo.sendRAOverdueTo_DepartmentAdmin);
                    }
                }
                return configuration;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get notification settings for a company
        /// </summary>
        /// <param name="compay"></param>
        /// <returns></returns>
        public CompanyModuleAssigmentNotificationSettings GetCompanyModuleAssignmentNotificationSettingsInfo(Int64 compay)
        {
            try
            {
                CompanyModuleAssigmentNotificationSettings configuration = new CompanyModuleAssigmentNotificationSettings();

                using (var context = new lmsdbEntities())
                {
                    var companyInfo = context.lms_admin_get_courseAssignementEmail_settings(compay).FirstOrDefault();
                    if (companyInfo != null)
                    {
                        configuration.SendCourseAssignmentMailToLearner =  Convert.ToBoolean(companyInfo.Value);
                    }
                }
                return configuration;
            }
            catch (Exception)
            {
                throw;
            }
        }



        /// <summary>
        /// update notification settings for a company
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public int UpdateCourseAssignmentEmailSettings(CompanyModuleAssigmentNotificationSettings info)
        {
            int success = 0;
            try
            {
                ObjectParameter result = new ObjectParameter("result", typeof(long));
                using (var context = new lmsdbEntities())
                {
                    var response = context.lms_admin_update_courseAssignementEmail_settings(info.CompanyId,
                        info.SendCourseAssignmentMailToLearner,
                        result);
                    success = Convert.ToInt32(result.Value);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return success;
        }


        /// <summary>
        /// update notification settings for a company
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public int UpdateCompanyNotificationSettings(CompanyNotificationSettings info)
        {
            int success = 0;
            try
            {
                ObjectParameter result = new ObjectParameter("result", typeof(long));
                using (var context = new lmsdbEntities())
                {
                    var response = context.lms_admin_update_notification_settings(info.CompanyId, 
                        info.SendCoursePassedToLocationAdmin, 
                        info.SendCoursePassedToDepartmentAdmin,
                        info.SendCourseOverdueToLocationAdmin, 
                        info.SendRAOverDueToDepartmentAdmin,
                        info.SendRACompletionToLocationAdmin,
                        info.SendRACompletionToDepartmentAdmin,
                        info.SendRAOverDueToLocationAdmin,
                        info.SendRAOverDueToDepartmentAdmin,
                        info.SendCourseFailedToLocationAdmin,
                        info.SendCourseFailedToDepartmentAdmin, 
                        result);
                    success = Convert.ToInt32(result.Value);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return success;
        }

        /// <summary>
        /// Get Company structure
        /// </summary>
        /// <param name="compay"></param>
        /// <returns></returns>
        public List<CompanyStructuralComponent> GetCompanyStructure(Int64 compay)
        {
            try
            {
                List<CompanyStructuralComponent> compStruc = new List<CompanyStructuralComponent>();

                using (var context = new lmsdbEntities())
                {
                    var companyStructure = context.lms_admin_get_company_structure(compay).ToList();
                    if (companyStructure != null && companyStructure.Count > 0)
                    {
                        foreach (var item in companyStructure)
                        {
                            CompanyStructuralComponent component = new CompanyStructuralComponent();
                            component.LocationId = item.intLocationID;
                            component.LocationName = item.strLocation;
                            component.DepartmentId = Convert.ToInt64(item.intDepartmentID);
                            component.DepartmentName = item.strDepartment;
                            compStruc.Add(component);
                        }
                    }
                }
                return compStruc;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get list of companies associated to a reseller
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public ResellerCompanyList GetResellerCompanies(DataTableFilter searchCriteria)
        {
            try
            {
                ResellerCompanyList resellerCompnayList = new ResellerCompanyList();
                List<ResellerCompany> compnayList = new List<ResellerCompany>();

                using (var context = new lmsdbEntities())
                {
                    var resellerCompanies = context.lms_reseller_get_company_list(searchCriteria.Company, searchCriteria.SearchText, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (resellerCompanies != null && resellerCompanies.Count > 0)
                    {
                        resellerCompnayList.TotalRecords = resellerCompanies.Count();
                        var data = resellerCompanies.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            ResellerCompany company = new ResellerCompany();
                            company.CompanyId = item.intOrganisationID;
                            company.AdminFirstname = item.strFirstName;
                            company.AdminSurname = item.strSurname;
                            company.AdminEmail = item.strEmail;
                            company.CompanyName = item.strOrganisation;
                            company.Cancelled = Convert.ToBoolean(item.blnCancelled);
                            company.Status = Convert.ToBoolean(item.blnCancelled) ? "In-active" : "Active";
                            company.CreationDate = item.datCreated == null ? "" : (Convert.ToDateTime(item.datCreated)).ToString("dd-MMM-yyyy");
                            company.ExpiryDate = item.UL_datExpire == null ? "" : (Convert.ToDateTime(item.UL_datExpire)).ToString("dd-MMM-yyyy");
                            compnayList.Add(company);
                        }
                    }
                }
                resellerCompnayList.CompanyList = compnayList;
                return resellerCompnayList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get course list for a company with assigned and consumend licences
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public ResellerCompanyLicenceList GetLicencesForCompany(Int64 CompanyId)
        {
            try
            {
                ResellerCompanyLicenceList companyLicenceList = new ResellerCompanyLicenceList();
                List<ResellerCompanyLicence> courseList = new List<ResellerCompanyLicence>();

                using (var context = new lmsdbEntities())
                {
                    var companyCourses = context.lms_reseller_get_company_licenceUsage(CompanyId).ToList();
                    if (companyCourses != null && companyCourses.Count > 0)
                    {
                        companyLicenceList.TotalRecords = companyCourses.Count();

                        foreach (var item in companyCourses)
                        {
                            ResellerCompanyLicence course = new ResellerCompanyLicence();
                            course.CourseName = item.strCourse;
                            course.AssignedLicences = item.AssignedLicenses;
                            course.ConsumedLicences = item.ConsumedLicenses;
                            courseList.Add(course);
                        }
                    }
                }
                companyLicenceList.LicenceList = courseList;
                return companyLicenceList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get domain details
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public CompanyDomainDetails GetOrganizationFromHost(string host)
        {
            CompanyDomainDetails detalis = new CompanyDomainDetails();

            using (var context = new lmsdbEntities())
            {
                var info = context.lms_admin_getCompanyDomainInfo(host).FirstOrDefault();
                if (info != null)
                {
                    detalis.CompanyId = Convert.ToInt64(info.lms_org_id);
                    detalis.Domain = info.domain;
                    detalis.CSS = info.css_path;
                    detalis.Favicon = info.favicon_path;
                    detalis.TitleText = info.title_text;
                    detalis.LogoPath = info.logo_path;
                }
            }
            return detalis;
        }
    }
}
