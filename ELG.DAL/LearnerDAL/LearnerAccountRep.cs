using System;
using System.Collections.Generic;
using System.Linq;
using ELG.Model.Learner;
using ELG.DAL.DbEntityLearner;
using ELG.DAL.Utilities;
using System.Data.Entity.Core.Objects;

namespace ELG.DAL.LearnerDAL
{
    public class LearnerAccountRep
    {
        #region SSO Login

        /// <summary>
        /// Get user info by username/password
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public LearnerSSOInfo GetLearnerSSOInfo(string username)
        {
            try
            {
                LearnerSSOInfo learnerSSO = new LearnerSSOInfo();

                using (learnerDBEntities context = new learnerDBEntities())
                {
                    var item = context.lms_learner_get_LearnerSSOInfo(username).FirstOrDefault();
                    if (item != null)
                    {
                        //foreach (var item in ssoDetails)
                        //{
                        //LearnerSSOInfo learner = new LearnerSSOInfo();
                        learnerSSO.UserID = item.intContactID;
                        learnerSSO.OrgId = Convert.ToInt64(item.intOrganisationId);
                        learnerSSO.SSOURL = item.ssoURL;
                        learnerSSO.EntityId = item.entityID;
                        learnerSSO.EntityKey = item.entityKey;
                        learnerSSO.Cert = item.cert;
                        learnerSSO.ReturnURL = item.returnURL;

                            //learnerSSO.Add(learner);
                        //}
                    }
                }
                return learnerSSO;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        /// <summary>
        /// Get user info by username/password
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public List<LearnerInfo> GetLearnerInfoByUsernamePassword(Int64 orgId, string username, string password, string key, bool isMasterPwd)
        {
            try
            {
                var enc_password = CommonMethods.EncodePassword(password, key);
                List<LearnerInfo> learners = new List<LearnerInfo>();

                using (learnerDBEntities context = new learnerDBEntities())
                {
                    var learnerInfolst = context.lms_learner_getLoginDetails(orgId, username, enc_password, isMasterPwd).ToList();
                    if (learnerInfolst != null && learnerInfolst.Count > 0)
                    {
                        foreach (var item in learnerInfolst)
                        {
                            LearnerInfo learner = new LearnerInfo();
                            learner.UserID = item.intcontactid;
                            learner.FirstName = item.strFirstName;
                            learner.LastName = item.strSurname;
                            learner.EmailId = item.strEmail;
                            learner.ProfilePic = item.profilePic;
                            learner.CompanyId = Convert.ToInt64(item.intOrganisationID);
                            learner.CompanyNumber = item.UL_sequentialId;
                            learner.CompanyName = item.strOrganisation;
                            learner.CompanyLogo = item.org_banner;
                            learner.CompanyCertificate = item.org_certificate;
                            learner.IsCompanyActive = Convert.ToBoolean(item.ul_blnLive);
                            learner.IsDeactivated = Convert.ToBoolean(item.blnCancelled);
                            learner.IsPasswordReset = Convert.ToBoolean(item.IsRestPassword);
                            learner.AccidentIncidentFeature = Convert.ToInt32(item.incidentAccidentEnabled);
                            learner.MenuItems = item.learnerMenuSettings;

                            learners.Add(learner);
                        }
                    }
                }
                return learners;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// learner login info on user id
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public LearnerInfo GetLearnerInfoByUserID(int userid)
        {
            try
            {
                LearnerInfo learner = new LearnerInfo();

                using (learnerDBEntities context = new learnerDBEntities())
                {
                    var learnerInfo = context.lms_learner_getLoginDetails_byUserId(userid).FirstOrDefault();
                    if (learnerInfo != null)
                    {
                        learner.UserID = learnerInfo.intcontactid;
                        learner.FirstName = learnerInfo.strFirstName;
                        learner.LastName = learnerInfo.strSurname;
                        learner.EmailId = learnerInfo.strEmail;
                        learner.ProfilePic = learnerInfo.profilePic;
                        learner.CompanyId = Convert.ToInt64(learnerInfo.intOrganisationID);
                        learner.CompanyNumber = learnerInfo.UL_sequentialId;
                        learner.CompanyName = learnerInfo.strOrganisation;
                        learner.CompanyLogo = learnerInfo.org_banner;
                        learner.CompanyCertificate = learnerInfo.org_certificate;
                        learner.IsCompanyActive = Convert.ToBoolean(learnerInfo.ul_blnLive);
                        learner.IsDeactivated = Convert.ToBoolean(learnerInfo.blnCancelled);
                        learner.IsPasswordReset = Convert.ToBoolean(learnerInfo.IsRestPassword);
                        learner.AccidentIncidentFeature = Convert.ToInt32(learnerInfo.incidentAccidentEnabled);
                        learner.MenuItems = learnerInfo.learnerMenuSettings;

                    }
                }
                return learner;
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
        public LearnerInfo GetSSOLearnerInfoByUsernamePassword(string username)
        {
            try
            {
                LearnerInfo learner = new LearnerInfo();

                using (learnerDBEntities context = new learnerDBEntities())
                {
                    var learnerInfo = context.lms_learner_getSSOLoginDetails(username).FirstOrDefault();
                    if (learnerInfo != null)
                    {
                        learner.UserID = learnerInfo.intcontactid;
                        learner.FirstName = learnerInfo.strFirstName;
                        learner.LastName = learnerInfo.strSurname;
                        learner.EmailId = learnerInfo.strEmail;
                        learner.CompanyId = Convert.ToInt64(learnerInfo.intOrganisationID);
                        learner.CompanyNumber = learnerInfo.UL_sequentialId;
                        learner.CompanyName = learnerInfo.strOrganisation;
                        learner.CompanyLogo = learnerInfo.org_banner;
                        learner.CompanyCertificate = learnerInfo.org_certificate;
                        learner.IsCompanyActive = Convert.ToBoolean(learnerInfo.ul_blnLive);
                        learner.IsDeactivated = Convert.ToBoolean(learnerInfo.blnCancelled);
                        learner.IsPasswordReset = Convert.ToBoolean(learnerInfo.IsRestPassword);
                        learner.ProfilePic = learnerInfo.profilePic;
                    }
                }
                return learner;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public int CheckIfLearnerHasAdminRights(Int64 learnerId)
        {
                int hasAdminRight = 0;
            try
            {
                using (learnerDBEntities context = new learnerDBEntities())
                {
                    hasAdminRight = Convert.ToInt32(context.lms_learner_check_admin_rights(learnerId).FirstOrDefault());
                }
            }
            catch (Exception)
            {
                throw;
            }
                return hasAdminRight;
        }

        public LearnerInfo ValidateUserNameByCompany(string company, string username)
        {
            try
            {
                LearnerInfo learnerinfo = new LearnerInfo();
                using (var context = new learnerDBEntities())
                {
                    var learner = context.lms_learner_validateCompanyNumber(username, company).FirstOrDefault();
                    if (learner != null)
                    {
                        learnerinfo.UserID = learner.intcontactid;
                        learnerinfo.FirstName = learner.strFirstName;
                        learnerinfo.LastName = learner.strSurname;
                        learnerinfo.EmailId = learner.strEmail;
                        learnerinfo.CompanyId = Convert.ToInt64(learner.intOrganisationID);
                        learnerinfo.CompanyNumber = learner.UL_sequentialId;
                        learnerinfo.CompanyName = learner.strOrganisation;
                        learnerinfo.ProfilePic = learner.profilePic;
                    }
                }

                return learnerinfo;
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
                using (var context = new learnerDBEntities())
                {
                    context.lms_learner_ResetPasswordFlag(intContactID);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public LearnerInfo GetNewUserInfoOrResetPasswordInfo(int intContactID)
        {
            try
            {
                LearnerInfo learnerinfo = new LearnerInfo();
                using (var context = new learnerDBEntities())
                {
                    var learner = context.lms_learner_GetNewUserInfoOrResetPasswordInfo(intContactID).FirstOrDefault();
                    if (learner != null)
                    {
                        learnerinfo.UserID = learner.intContactID;
                        learnerinfo.FirstName = learner.strFirstName;
                        learnerinfo.LastName = learner.strSurname;
                        learnerinfo.EmailId = learner.strEmail;
                        learnerinfo.CompanyId = Convert.ToInt64(learner.intOrganisationID);
                    }
                }
                return learnerinfo;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public LearnerProfile GetSessionLearnerProfile(Int64 intContactID)
        {
            try
            {
                LearnerProfile profile = new LearnerProfile();
                using (var context = new learnerDBEntities())
                {
                    var learner = context.lms_learner_getSessionUserInfo(intContactID).FirstOrDefault();
                    if (learner != null)
                    {
                        profile.UserID = learner.intContactID;
                        profile.FirstName = learner.strFirstName;
                        profile.LastName = learner.strSurname;
                        profile.EmployeeNumber = learner.strEmployeeNumber;
                        profile.Email = learner.strEmail;
                        profile.CompanyName = learner.strOrganisation;
                        profile.CompanyNumber = learner.UL_sequentialId;
                        profile.Location = learner.strLocation;
                        profile.Department = learner.strDepartment;
                    }
                }
                return profile;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// Update details of learner in session
        /// </summary>
        /// <param name="learner"></param>
        /// <returns></returns>
        public int UpdateLearnerInfo(LearnerProfile learner)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new learnerDBEntities())
                {
                    var result = context.lms_learner_updateSessionUserInfo(learner.UserID, learner.FirstName, learner.LastName, learner.Email, learner.EmployeeNumber, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// remove learner profile pic
        /// </summary>
        /// <param name="Learner"></param>
        /// <param name="ProfilePic"></param>
        /// <returns></returns>
        public int RemoveLearnerProfilePic(Int64 Learner)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("update", typeof(int));
                using (var context = new learnerDBEntities())
                {
                    var result = context.lms_learner_update_profilePic(Learner, null, retVal);
                    success = Convert.ToInt32(retVal.Value);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return success;
        }

        /// <summary>
        /// update learner profile pic
        /// </summary>
        /// <param name="Learner"></param>
        /// <param name="ProfilePic"></param>
        /// <returns></returns>
        public int UpdateLearnerProfilePic(Int64 Learner, byte[] ProfilePic)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("update", typeof(int));
                using (var context = new learnerDBEntities())
                {
                    var result = context.lms_learner_update_profilePic(Learner, ProfilePic, retVal);
                    success = Convert.ToInt32(retVal.Value);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return success;
        }

        public void UpdateUserPassword(int intContactID, string usrpwd, string key)
        {
            try
            {
                using (var context = new learnerDBEntities())
                {
                    context.lms_learner_UpdatePassword(intContactID, CommonMethods.EncodePassword(usrpwd, key));
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public LearnerInfo GetUserShortDetailByUserID(long intContactID)
        {
            try
            {
                LearnerInfo learnerinfo = new LearnerInfo();
                using (var context = new learnerDBEntities())
                {
                    var learner = context.lms_learner_GetUserShortDetailByUserID(Convert.ToInt32(intContactID)).FirstOrDefault();
                    if (learner != null)
                    {
                        learnerinfo.UserID = learner.intContactID;
                        learnerinfo.FirstName = learner.strFirstName;
                        learnerinfo.LastName = learner.strSurname;
                        learnerinfo.EmailId = learner.strEmail;
                        learnerinfo.Password = learner.strPassword;
                        learnerinfo.CompanyId = Convert.ToInt64(learner.intOrganisationID);
                    }
                }
                return learnerinfo;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// Function to create new learner in an organisation
        /// </summary>
        /// <param name="learner"></param>
        /// <returns></returns>
        public NewLearner CreateNewLearner(LearnerProfile learner)
        {
            NewLearner Learner = new NewLearner();
            Learner.UserID = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("id", typeof(long));
                ObjectParameter assign = new ObjectParameter("retVal", typeof(int));
                ObjectParameter noLicence = new ObjectParameter("courseList", typeof(string));
                using (var context = new learnerDBEntities())
                {
                    var result = context.lms_learner_createLearner(learner.FirstName, learner.Title,  learner.LastName, learner.EmployeeNumber, learner.Email, learner.CompanyId, learner.LocationId, learner.DepartmentId, retVal);
                    Learner.UserID = Convert.ToInt32(retVal.Value);

                    // assign courses set for auto assignment
                    // ToDo: Update this logic remove decryption and make it simple
                    if (Learner.UserID > 0)
                    {
                        var courses = context.lms_learner_getCoursesForAutoAssignement(learner.LocationId, learner.DepartmentId).ToList();
                        if (courses != null && courses.Count > 0)
                        {
                            List<string> strValidCourses = new List<string>();
                            foreach (var course in courses)
                            {
                                strValidCourses.Add(Convert.ToString(course.intCourseID));
                            }
                            var assignResult = context.lms_learner_AssignCoursestoContact(Learner.UserID, String.Join(",", strValidCourses), assign, noLicence);
                            Learner.CourseWithNoLicences = Convert.ToString(noLicence.Value);
                        }
                    }

                }

            }
            catch (Exception)
            {
                throw;
            }
            return Learner;
        }



        /// <summary>
        /// To get details of the contractor
        /// </summary>
        /// <param name="Company"></param>
        /// <returns></returns>
        public CompanyContractor GetContractorInfo(Int64 Company)
        {
            try
            {
                CompanyContractor contractor = new CompanyContractor();
                using (var context = new learnerDBEntities())
                {
                    var contractorInfo = context.lms_learner_get_companyContractorInfo(Company).FirstOrDefault();
                    if (contractorInfo != null)
                    {
                        contractor.ContractorName = contractorInfo.strContractorName;
                        contractor.ContractorEmail = contractorInfo.strContractorEmail;
                        contractor.TrainerName = contractorInfo.strTrainerFirstName + " " + contractorInfo.strTrainerSurname;
                        contractor.TrainerEmail = contractorInfo.strTrainerEmail;
                    }
                }
                return contractor;
            }
            catch (Exception)
            {

                throw;
            }

        }

        /// <summary>
        /// TO track browser details at user login
        /// </summary>
        /// <param name="learner"></param>
        /// <returns></returns>
        public Int64 CreateLearnerLoginRecord(LearnerBrowserDetails learner)
        {
            Int64 success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("insertRecordId", typeof(long));
                using (var context = new learnerDBEntities())
                {
                    var result = context.lms_learner_insert_Login_Details(learner.UserID, learner.Browser, learner.BrowserVersion, learner.OS, learner.Device, learner.BrowserDetails, learner.IsMobileDevice, retVal);
                    success = Convert.ToInt64(retVal.Value);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return success;
        }
    }
}
