using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ELG.Model.OrgAdmin;
using ELG.DAL.DBEntity;
using ELG.DAL.Utilities;

namespace ELG.DAL.OrgAdminDAL
{
    
    public class OrgAdminAccountRep
    {
        #region SSO Login

        /// <summary>
        /// Get user info by username/password
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public OrdAdminSSOInfo GetAdminSSOInfo(string username)
        {
            try
            {
                OrdAdminSSOInfo adminSSO = new OrdAdminSSOInfo();

                using (var context = new lmsdbEntities())
                {
                    var item = context.lms_admin_get_LearnerSSOInfo(username).FirstOrDefault();
                    if (item != null)
                    {
                        //foreach (var item in ssoDetails)
                        //{
                        //LearnerSSOInfo learner = new LearnerSSOInfo();
                        adminSSO.UserID = item.intContactID;
                        adminSSO.OrgId = Convert.ToInt64(item.intOrganisationId);
                        adminSSO.SSOURL = item.adminSSOURL;
                        adminSSO.EntityId = item.entityID;
                        adminSSO.EntityKey = item.entityKey;
                        adminSSO.Cert = item.cert;
                        adminSSO.ReturnURL = item.adminReturnURL;

                        //learnerSSO.Add(learner);
                        //}
                    }
                }
                return adminSSO;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        /// <summary>
        /// Get user info by username
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public List<OrgAdminInfo> GetAdmin(string username, string password, string key, Boolean masterPwd)
        {
            try
            {
                var enc_password = CommonMethods.EncodePassword(password, key);
                List<OrgAdminInfo> admins = new List<OrgAdminInfo>();
                using (var context = new lmsdbEntities())
                {
                    var adminList = context.lms_admin_getAdminLoginDetails(username, enc_password, masterPwd).ToList();
                    if (adminList != null && adminList.Count > 0)
                    {
                        foreach (var item in adminList)
                        {
                            OrgAdminInfo admin = new OrgAdminInfo();
                            admin.UserID = item.intcontactid;
                            admin.FirstName = item.strFirstName;
                            admin.LastName = item.strSurname;
                            admin.EmailId = item.strEmail;
                            admin.CompanyId = Convert.ToInt64(item.intOrganisationID);
                            admin.CompanyNumber = item.UL_sequentialId;
                            admin.CompanyName = item.strOrganisation;
                            admin.CompanyLogo = item.org_banner;
                            admin.IsCompanyActive = Convert.ToBoolean(item.UL_blnLive);
                            admin.CompanyCertificate = item.org_certificate;
                            admin.IsPasswordReset = Convert.ToBoolean(item.IsRestPassword);
                            admin.ProfilePic = item.profilePic;
                            admin.AccidentIncidentFeature = Convert.ToInt32(item.incidentAccidentEnabled);
                            admin.TrainingRenewalMode = Convert.ToInt32(item.trainingResetType);
                            admin.CourseAssignMode = Convert.ToInt32(item.courseAssignMode);
                            admin.MenuItems = item.adminMenuSettings;

                            admins.Add(admin);
                        }
                    }
                }
                return admins;
            }
            catch (Exception)
            {
                throw;
            }
        }
        
        public OrgAdminInfo GetOrgAdmin(string company, string username)
        {
            try
            {
                OrgAdminInfo admin = new OrgAdminInfo();
                using (var context = new lmsdbEntities())
                {
                    var orgAdmin = context.lms_admin_validateCompanyNumber(username, company).FirstOrDefault();
                    if (orgAdmin != null)
                    {
                        admin.UserID = orgAdmin.intcontactid;
                        admin.FirstName = orgAdmin.strFirstName;
                        admin.LastName = orgAdmin.strSurname;
                        admin.EmailId = orgAdmin.strEmail;
                        admin.CompanyId = Convert.ToInt64(orgAdmin.intOrganisationID);
                        admin.CompanyNumber = orgAdmin.UL_sequentialId;
                        admin.CompanyName = orgAdmin.strOrganisation;
                        admin.ProfilePic = orgAdmin.profilePic;
                        admin.AccidentIncidentFeature = Convert.ToInt32(orgAdmin.incidentAccidentEnabled);
                        admin.TrainingRenewalMode = Convert.ToInt32(orgAdmin.trainingResetType);
                    }
                }

                return admin;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// get sso user login
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public OrgAdminInfo GetSSOAdminInfoByUsernamePassword(string username)
        {
            try
            {
                OrgAdminInfo admin = new OrgAdminInfo();

                using (var context = new lmsdbEntities())
                {
                    var adminInfo = context.lms_admin_getSSOLoginDetails(username).FirstOrDefault();
                    if (adminInfo != null)
                    {
                        admin.UserID = adminInfo.intcontactid;
                        admin.FirstName = adminInfo.strFirstName;
                        admin.LastName = adminInfo.strSurname;
                        admin.EmailId = adminInfo.strEmail;
                        admin.CompanyId = Convert.ToInt64(adminInfo.intOrganisationID);
                        admin.CompanyNumber = adminInfo.UL_sequentialId;
                        admin.CompanyName = adminInfo.strOrganisation;
                        admin.CompanyLogo = adminInfo.org_banner;
                        admin.CompanyCertificate = adminInfo.org_certificate;
                        admin.IsCompanyActive = Convert.ToBoolean(adminInfo.ul_blnLive);
                        admin.IsDeactivated = Convert.ToBoolean(adminInfo.blnCancelled);
                        admin.IsPasswordReset = Convert.ToBoolean(adminInfo.IsRestPassword);
                        admin.ProfilePic = adminInfo.profilePic;
                        admin.AccidentIncidentFeature = Convert.ToInt32(adminInfo.incidentAccidentEnabled);
                        admin.TrainingRenewalMode = Convert.ToInt32(adminInfo.trainingResetType);
                    }
                }
                return admin;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void ResetPasswordFlag(int intContactID)
        {
            try
            {
                using (var context = new lmsdbEntities())
                {
                    context.lms_admin_ResetPasswordFlag(intContactID);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public OrgAdminInfo GetNewUserInfoOrResetPasswordInfo(int intContactID)
        {
            try
            {
                OrgAdminInfo info = new OrgAdminInfo();
                using (var context = new lmsdbEntities())
                {
                    var admin = context.lms_admin_GetNewUserInfoOrResetPasswordInfo(intContactID).FirstOrDefault();
                    if (admin != null)
                    {
                        info.UserID = admin.intContactID;
                        info.FirstName = admin.strFirstName;
                        info.LastName = admin.strSurname;
                        info.EmailId = admin.strEmail;
                        info.CompanyId = Convert.ToInt64(admin.intOrganisationID);
                    }
                }
                return info;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void UpdateUserPassword(Int64 intContactID, string usrpwd, string key)
        {
            try
            {
                using (var context = new lmsdbEntities())
                {
                    context.lms_admin_UpdatePassword(intContactID, CommonMethods.EncodePassword(usrpwd, key));
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void UpdateLearnerPasswordByAdmin(Int64 intContactID, string usrpwd, string key)
        {
            try
            {
                using (var context = new lmsdbEntities())
                {
                    context.lms_admin_update_learner_Password(intContactID, CommonMethods.EncodePassword(usrpwd, key));
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void UpdateAdminPassword(int intContactID, string usrpwd, string key)
        {
            try
            {
                using (var context = new lmsdbEntities())
                {
                    context.lms_admin_UpdatePassword(intContactID, CommonMethods.EncodePassword(usrpwd, key));
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public OrgAdminInfo GetUserShortDetailByUserID(long intContactID)
        {
            try
            {
                OrgAdminInfo info = new OrgAdminInfo();
                using (var context = new lmsdbEntities())
                {
                    var admin = context.lms_admin_GetUserShortDetailByUserID(Convert.ToInt32(intContactID)).FirstOrDefault();
                    if (admin != null)
                    {
                        info.UserID = admin.intContactID;
                        info.FirstName = admin.strFirstName;
                        info.LastName = admin.strSurname;
                        info.EmailId = admin.strEmail;
                        info.Password = admin.strPassword;
                        info.CompanyId = Convert.ToInt64(admin.intOrganisationID);
                    }
                }
                return info;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
