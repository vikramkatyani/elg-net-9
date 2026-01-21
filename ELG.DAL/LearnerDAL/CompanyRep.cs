using System;
using System.Collections.Generic;
using System.Linq;
using ELG.Model.Learner;
using ELG.DAL.DbEntityLearner;
using ELG.DAL.Utilities;
using System.Data.Entity.Core.Objects;
using System.Globalization;

namespace ELG.DAL.LearnerDAL
{
    public class CompanyRep
    {
        /// <summary>
        /// Get user info by username/password
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public Company GetCompanyInfo(string compay)
        {
            try
            {
                Company company = new Company();

                using (learnerDBEntities context = new learnerDBEntities())
                {
                    var companyInfo= context.lms_learner_getCompanyInfo(compay).FirstOrDefault();
                    if (companyInfo != null)
                    {
                        company.CompanyId = companyInfo.intOrganisationID;
                        company.CompanyNumber = companyInfo.UL_sequentialId;
                        company.CompanyBrandName = companyInfo.orgBrandName;
                        company.CompanySupportMail = companyInfo.supportEmail;
                        company.CompanyBaseURL = companyInfo.orgBaseURL;
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
        /// 
        /// </summary>
        /// <param name="compay"></param>
        /// <returns></returns>
        public CreateLearnerFields GetCreateUserFields(Int64 compay)
        {
            try
            {
                CreateLearnerFields fields = new CreateLearnerFields();

                using (learnerDBEntities context = new learnerDBEntities())
                {
                    var companyInfo = context.lms_learner_GetAdminOrgRegSettings(compay).FirstOrDefault();
                    if (companyInfo != null)
                    {
                        fields.TitleDesc = companyInfo.strTitleDescription;
                        fields.FirstNameDesc = companyInfo.strFirstNameDescription;
                        fields.LastNameDesc = companyInfo.strSurnameDescription;
                        fields.EmailDesc = companyInfo.UL_emailIdDescription;
                        fields.LocationDes = companyInfo.strLocationDescription;
                        fields.DepartmentDesc = companyInfo.strDepartmentDescription;
                        fields.EmployeeNumberDesc = companyInfo.strEmployeeNumberDescription;
                    }
                }
                return fields;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public CompanyDomainDetails GetOrganizationFromHost(string host)
        {
            CompanyDomainDetails detalis = new CompanyDomainDetails();

            using (learnerDBEntities context = new learnerDBEntities())
            {
                var info = context.lms_global_getCompanyDomainInfo(host).FirstOrDefault();
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
