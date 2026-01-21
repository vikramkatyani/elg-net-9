using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ELG.Model.OrgAdmin;
using ELG.DAL.DBEntity;
using ELG.DAL.Utilities;
using System.Data.Entity.Core.Objects;

namespace ELG.DAL.OrgAdminDAL   
{
    public class AdminRep
    {
        #region Admin rights global

        /// <summary>
        /// To get list of all users having admin rights
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public AdminLearnerList GetAdminLearners(AdminLearnerListFilter searchCriteria)
        {
            try
            {
                AdminLearnerList adminList = new AdminLearnerList();
                List<AdminLearner> adminlearnerInfoList = new List<AdminLearner>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_getAllLearnersWithAdminRights(searchCriteria.SearchText, searchCriteria.Company, searchCriteria.AdminLevel).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        adminList.TotalAdmins = resultList.Count();
                        var data = resultList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            AdminLearner admin = new AdminLearner();
                            admin.UserID = Convert.ToInt64(item.intContactID);
                            admin.FirstName = item.strFirstName;
                            admin.LastName = item.strSurname;
                            admin.EmployeeNumber = item.strEmployeeNumber;
                            admin.EmailId = item.strEmail;
                            admin.AdminLevel = Convert.ToInt32(item.intAdminLevelID);
                            admin.AdminLevelName = item.strAdminLevel;
                            adminlearnerInfoList.Add(admin);
                        }
                    }
                }
                adminList.AdminList = adminlearnerInfoList;
                return adminList;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public AdminLearnerList GetAllAdminLearners(AdminLearnerListFilter searchCriteria)
        {
            try
            {
                AdminLearnerList adminList = new AdminLearnerList();
                List<AdminLearner> adminlearnerInfoList = new List<AdminLearner>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_getAllLearnersWithAdminRights(searchCriteria.SearchText, searchCriteria.Company, searchCriteria.AdminLevel).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        adminList.TotalAdmins = resultList.Count();
                        foreach (var item in resultList)
                        {
                            AdminLearner admin = new AdminLearner();
                            admin.UserID = Convert.ToInt64(item.intContactID);
                            admin.FirstName = item.strFirstName;
                            admin.LastName = item.strSurname;
                            admin.EmployeeNumber = item.strEmployeeNumber;
                            admin.EmailId = item.strEmail;
                            admin.AdminLevel = Convert.ToInt32(item.intAdminLevelID);
                            admin.AdminLevelName = item.strAdminLevel;
                            adminlearnerInfoList.Add(admin);
                        }
                    }
                }
                adminList.AdminList = adminlearnerInfoList;
                return adminList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public AdminLearnerEmailInfo GetUpgradedAdminEmailInfo(Int64 LearnerId, Int32 AdminRole)
        {
            try
            {
                AdminLearnerEmailInfo adminlearnerInfo = new AdminLearnerEmailInfo();

                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_get_emailInfo_upgradetoAdmin(LearnerId, AdminRole).FirstOrDefault();
                    if (result != null)
                    {
                        adminlearnerInfo.FirstName = result.strFirstName;
                        adminlearnerInfo.LastName = result.strSurname;
                        adminlearnerInfo.EmailId = result.strEmail;
                        adminlearnerInfo.AdminLevelName = result.strAdminLevel;
                        adminlearnerInfo.OrganisationName = result.strOrganisation;
                        adminlearnerInfo.OrganisationBrandName = result.orgBrandName;
                        adminlearnerInfo.SupportEmail = result.supportEmail;
                        adminlearnerInfo.BaseURL = result.orgBaseURL;
                    }
                }

                return adminlearnerInfo;
            }
            catch (Exception)
            {
                throw;
            }
        }


        // download learner list
        public List<DownloadAdminLearner> DownloadAdminReport(AdminLearnerListFilter searchCriteria)
        {
            try
            {
                List<DownloadAdminLearner> learnerRecord = new List<DownloadAdminLearner>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_getAllLearnersWithAdminRights_toDownload(searchCriteria.SearchText, searchCriteria.Company, searchCriteria.AdminLevel).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        foreach (var item in resultList)
                        {
                            DownloadAdminLearner learner = new DownloadAdminLearner();
                            learner.FirstName = item.strFirstName;
                            learner.LastName = item.strSurname;
                            learner.Email = item.strEmail;
                            learner.EmployeeNumber = item.strEmployeeNumber;
                            learner.AdminLevel = item.strAdminLevel;
                            learner.LocationAdminRights = item.LocationAdminRights;
                            learner.DepartmentAdminRights = item.DepartmentAdminRights;
                            learner.LocationSuperVisorRights = item.LocationSuperVisorRights;
                            learner.DepartmentSupervisorRights = item.DepartmentSupervisorRights;
                            learnerRecord.Add(learner);
                        }
                    }
                }
                return learnerRecord;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// function to assign Super Admin, report admin, user admin rights to a learner
        /// </summary>
        /// <param name="learner"></param>
        /// <param name="adminRight"></param>
        /// <returns></returns>
        public int AssignGlobalAdminRights(Int64 learner, int adminRight )
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("retval", typeof(long));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_assignAdminRights(learner, adminRight, retVal);
                    success = Convert.ToInt32(retVal.Value);
                }
                return success;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Function to remove global admin rights from a user
        /// </summary>
        /// <param name="learner"></param>
        /// <param name="adminRight"></param>
        /// <returns></returns>
        public int RemoveGlobalAdminRights(Int64 learner, int adminRight)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("retval", typeof(long));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_removeGlobalAdminRights(learner, adminRight, retVal);
                    success = Convert.ToInt32(retVal.Value);
                }
                return success;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// To get Get Admin Org Reg Settings
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public OrganizationInfo GetAdminOrgRegSettings(Int64 CompanyId)
        {
            try
            {
                OrganizationInfo OrgInfo = new OrganizationInfo();

                using (var context = new lmsdbEntities())
                {
                    var OrgaInfo = context.lms_admin_GetAdminOrgRegSettings(CompanyId).FirstOrDefault();
                    if (OrgaInfo != null)
                    {
                        OrgInfo.strTitleDescription = OrgaInfo.strTitleDescription;
                        OrgInfo.strFirstNameDescription = OrgaInfo.strFirstNameDescription;
                        OrgInfo.strSurnameDescription = OrgaInfo.strSurnameDescription;
                        OrgInfo.emailIdDescription = OrgaInfo.UL_emailIdDescription;
                        OrgInfo.strDepartmentDescription = OrgaInfo.strDepartmentDescription;
                        OrgInfo.strLocationDescription = OrgaInfo.strLocationDescription;
                        OrgInfo.strEmployeeNumberDescription = OrgaInfo.strEmployeeNumberDescription;
                        OrgInfo.PayrollMandatory = OrgaInfo.UL_PayrollMandatory;
                        OrgInfo.PayrollDescription = OrgaInfo.UL_PayrollDescription;
                        OrgInfo.JobtitleMandatory = OrgaInfo.UL_JobtitleMandatory;
                        OrgInfo.JobtitleDescription = OrgaInfo.UL_JobtitleDescription;
                        OrgInfo.RegSettings = OrgaInfo.Reg_Settings;
                        OrgInfo.UploadDocEnabled = Convert.ToBoolean(OrgaInfo.docUploadEnabled);
                        OrgInfo.AllowedUploadCount = Convert.ToInt32(OrgaInfo.allowedDocCount);
                        OrgInfo.CourseAssignMode = Convert.ToInt32(OrgaInfo.courseAssignMode);
                    }
                    return OrgInfo;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region location admin rights
        /// <summary>
        /// Function to return list of all location and indication for which learner has admin rights
        /// </summary>
        /// <param name="company"></param>
        /// <param name="learner"></param>
        /// <returns></returns>
        public List<LearnerLocationtWithAdminRights> GetLocationWithAdminRights(Int64 company, Int64 learner)
        {
            try
            {
                List<LearnerLocationtWithAdminRights> locationList = new List<LearnerLocationtWithAdminRights>();
                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_getAllLocationAdminRights_OfLeaner(learner, company).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        foreach (var item in resultList)
                        {
                            LearnerLocationtWithAdminRights location = new LearnerLocationtWithAdminRights();
                            location.LocationID = item.intLocationID;
                            location.LocationName = item.strLocation;
                            location.HasRights = Convert.ToBoolean(item.selected);
                            locationList.Add(location);
                        }
                    }
                }
                return locationList;

            }
            catch (Exception)
            {

                throw;
            }

        }

        /// <summary>
        /// Function return list of locations for which a learner has admin rights
        /// </summary>
        /// <param name="company"></param>
        /// <param name="learner"></param>
        /// <returns></returns>
        public List<LearnerLocationtWithAdminRights> GetLocationAssignedWithAdminRights(Int64 company, Int64 learner)
        {
            try
            {
                List<LearnerLocationtWithAdminRights> locationList = new List<LearnerLocationtWithAdminRights>();
                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_getLocationForLearnerWithAdminRights(learner, company).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        foreach (var item in resultList)
                        {
                            LearnerLocationtWithAdminRights location = new LearnerLocationtWithAdminRights();
                            location.LocationID = item.intLocationID;
                            location.LocationName = item.strLocation;
                            location.HasRights = true;
                            locationList.Add(location);
                        }
                    }
                }
                return locationList;

            }
            catch (Exception)
            {

                throw;
            }

        }

        /// <summary>
        /// Function to assign location admin rights to a user
        /// </summary>
        /// <param name="learner"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public int AssignLocationAdminRights(Int64 learner, Int64 location)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("retval", typeof(long));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_assignLocationAdminRights(learner, location, retVal);
                    success = Convert.ToInt32(retVal.Value);
                }
                return success;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Function to remove loaction admin rights from a user
        /// </summary>
        /// <param name="learner"></param>
        /// <param name="adminRight"></param>
        /// <returns></returns>
        public int RemoveLocationAdminRights(Int64 learner, Int64 location)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("retval", typeof(long));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_removeLocationAdminRights(learner, location, retVal);
                    success = Convert.ToInt32(retVal.Value);
                }
                return success;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion


        #region location supervisor rights
        /// <summary>
        /// Function to return list of all location and indication for which learner has supervisor rights
        /// </summary>
        /// <param name="company"></param>
        /// <param name="learner"></param>
        /// <returns></returns>
        public List<LearnerLocationtWithAdminRights> GetLocationWithSupervisorRights(Int64 company, Int64 learner)
        {
            try
            {
                List<LearnerLocationtWithAdminRights> locationList = new List<LearnerLocationtWithAdminRights>();
                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_getAllLocationSupervisorRights_OfLeaner(learner, company).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        foreach (var item in resultList)
                        {
                            LearnerLocationtWithAdminRights location = new LearnerLocationtWithAdminRights();
                            location.LocationID = item.intLocationID;
                            location.LocationName = item.strLocation;
                            location.HasRights = Convert.ToBoolean(item.selected);
                            locationList.Add(location);
                        }
                    }
                }
                return locationList;

            }
            catch (Exception)
            {

                throw;
            }

        }

        /// <summary>
        /// Function return list of locations for which a learner has admin rights
        /// </summary>
        /// <param name="company"></param>
        /// <param name="learner"></param>
        /// <returns></returns>
        public List<LearnerLocationtWithAdminRights> GetLocationAssignedWithSupervisorRights(Int64 company, Int64 learner)
        {
            try
            {
                List<LearnerLocationtWithAdminRights> locationList = new List<LearnerLocationtWithAdminRights>();
                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_getLocationForLearnerWithSupervisorRights(learner, company).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        foreach (var item in resultList)
                        {
                            LearnerLocationtWithAdminRights location = new LearnerLocationtWithAdminRights();
                            location.LocationID = item.intLocationID;
                            location.LocationName = item.strLocation;
                            location.HasRights = true;
                            locationList.Add(location);
                        }
                    }
                }
                return locationList;

            }
            catch (Exception)
            {

                throw;
            }

        }

        /// <summary>
        /// Function to assign location supervisor rights to a user
        /// </summary>
        /// <param name="learner"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public int AssignLocationSupervisorRights(Int64 learner, Int64 location)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("retval", typeof(long));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_assignLocationSupervisiorRights(learner, location, retVal);
                    success = Convert.ToInt32(retVal.Value);
                }
                return success;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Function to remove loaction supervisor rights from a user
        /// </summary>
        /// <param name="learner"></param>
        /// <param name="adminRight"></param>
        /// <returns></returns>
        public int RemoveLocationSupervisorRights(Int64 learner, Int64 location)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("retval", typeof(long));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_removeLocationSupervisorRights(learner, location, retVal);
                    success = Convert.ToInt32(retVal.Value);
                }
                return success;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region department admin rights
        /// <summary>
        /// Function to return list of all locations and indication for which learner has admin rights
        /// </summary>
        /// <param name="company"></param>
        /// <param name="location"></param>
        /// <param name="learner"></param>
        /// <returns></returns>
        public List<LearnerDepartmentWithAdminRights> GetDepartmentWithAdminRights(Int64 company, Int64 location, Int64 learner)
        {
            try
            {
                List<LearnerDepartmentWithAdminRights> departmentList = new List<LearnerDepartmentWithAdminRights>();
                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_getAllDepartmentAdminRights_OfLeaner(learner, location, company).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        foreach (var item in resultList)
                        {
                            LearnerDepartmentWithAdminRights department = new LearnerDepartmentWithAdminRights();
                            department.DepartmentID = item.intDepartmentID;
                            department.DepartmentName = item.strDepartment;
                            department.HasRights = Convert.ToBoolean(item.selected);
                            departmentList.Add(department);
                        }
                    }
                }
                return departmentList;

            }
            catch (Exception)
            {

                throw;
            }

        }

        /// <summary>
        /// Function return list of departments for which a learner has admin rights
        /// </summary>
        /// <param name="company"></param>
        /// <param name="location"></param>
        /// <param name="learner"></param>
        /// <returns></returns>
        public List<LearnerDepartmentWithAdminRights> GetDepartmentAssignedWithAdminRights(Int64 company, Int64 learner)
        {
            try
            {
                List<LearnerDepartmentWithAdminRights> departmentList = new List<LearnerDepartmentWithAdminRights>();
                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_getDepartmentForLearnerWithAdminRights(learner, company).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        foreach (var item in resultList)
                        {
                            LearnerDepartmentWithAdminRights department = new LearnerDepartmentWithAdminRights();
                            department.DepartmentID = item.intDepartmentID;
                            department.DepartmentName = item.strDepartment;
                            department.LocationID = Convert.ToInt64(item.intLocationID);
                            department.LocationName = item.strLocation;
                            department.HasRights = true;
                            departmentList.Add(department);
                        }
                    }
                }
                return departmentList;

            }
            catch (Exception)
            {

                throw;
            }

        }

        /// <summary>
        /// Function to assign department admin rights to a user
        /// </summary>
        /// <param name="learner"></param>
        /// <param name="location"></param>
        /// <param name="department"></param>
        /// <returns></returns>
        public int AssignDepartmentAdminRights(Int64 learner, Int64 location, Int64 department)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("retval", typeof(long));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_assignDepartmentAdminRights(learner, location, department, retVal);
                    success = Convert.ToInt32(retVal.Value);
                }
                return success;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// function remove department admin rights from a user
        /// </summary>
        /// <param name="learner"></param>
        /// <param name="location"></param>
        /// <param name="department"></param>
        /// <returns></returns>
        public int RemoveDepartmentAdminRights(Int64 learner, Int64 location, Int64 department)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("retval", typeof(long));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_removeDepartmentAdminRights(learner, location, department, retVal);
                    success = Convert.ToInt32(retVal.Value);
                }
                return success;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region department supervisor rights
        /// <summary>
        /// Function to return list of all locations and indication for which learner has supervisor rights
        /// </summary>
        /// <param name="company"></param>
        /// <param name="location"></param>
        /// <param name="learner"></param>
        /// <returns></returns>
        public List<LearnerDepartmentWithAdminRights> GetDepartmentWithSupervisorRights(Int64 company, Int64 location, Int64 learner)
        {
            try
            {
                List<LearnerDepartmentWithAdminRights> departmentList = new List<LearnerDepartmentWithAdminRights>();
                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_getAllDepartmentSupervisorRights_OfLeaner(learner, location, company).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        foreach (var item in resultList)
                        {
                            LearnerDepartmentWithAdminRights department = new LearnerDepartmentWithAdminRights();
                            department.DepartmentID = item.intDepartmentID;
                            department.DepartmentName = item.strDepartment;
                            department.HasRights = Convert.ToBoolean(item.selected);
                            departmentList.Add(department);
                        }
                    }
                }
                return departmentList;

            }
            catch (Exception)
            {

                throw;
            }

        }

        /// <summary>
        /// Function return list of departments for which a learner has admin rights
        /// </summary>
        /// <param name="company"></param>
        /// <param name="location"></param>
        /// <param name="learner"></param>
        /// <returns></returns>
        public List<LearnerDepartmentWithAdminRights> GetDepartmentAssignedWithSupervisorRights(Int64 company, Int64 learner)
        {
            try
            {
                List<LearnerDepartmentWithAdminRights> departmentList = new List<LearnerDepartmentWithAdminRights>();
                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_getDepartmentForLearnerWithSupervisiorRights(learner, company).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        foreach (var item in resultList)
                        {
                            LearnerDepartmentWithAdminRights department = new LearnerDepartmentWithAdminRights();
                            department.DepartmentID = item.intDepartmentID;
                            department.DepartmentName = item.strDepartment;
                            department.LocationID = Convert.ToInt64(item.intLocationID);
                            department.LocationName = item.strLocation;
                            department.HasRights = true;
                            departmentList.Add(department);
                        }
                    }
                }
                return departmentList;

            }
            catch (Exception)
            {

                throw;
            }

        }

        /// <summary>
        /// Function to assign department supervisor rights to a user
        /// </summary>
        /// <param name="learner"></param>
        /// <param name="location"></param>
        /// <param name="department"></param>
        /// <returns></returns>
        public int AssignDepartmentSupervisorRights(Int64 learner, Int64 location, Int64 department)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("retval", typeof(long));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_assignDepartmentSupervisiorRights(learner, location, department, retVal);
                    success = Convert.ToInt32(retVal.Value);
                }
                return success;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// function remove department supervisor rights from a user
        /// </summary>
        /// <param name="learner"></param>
        /// <param name="location"></param>
        /// <param name="department"></param>
        /// <returns></returns>
        public int RemoveDepartmentSupervisorRights(Int64 learner, Int64 location, Int64 department)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("retval", typeof(long));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_removeDepartmentSupervisorRights(learner, location, department, retVal);
                    success = Convert.ToInt32(retVal.Value);
                }
                return success;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region admin's privileges

        /// <summary>
        /// To get Get Admin Org Reg Settings
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public List<AdminPrivilege> GetAdminPrevelageSettings(Int64 AdminId)
        {
            try
            {
                List<AdminPrivilege> privilegeInfo = new List<AdminPrivilege>();
                List<DepartmentAdminRights> AdminDepartmentPrivilege = new List<DepartmentAdminRights>();
                List<DepartmentAdminRights> SpvDepartmentPrivilege = new List<DepartmentAdminRights>();
                List<int> AdminLocationPrivilege = new List<int>();
                List<int> SpvLocationPrivilege = new List<int>();

                using (var context = new lmsdbEntities())
                {
                    var pInfoList = context.lms_admin_getPrivileges(AdminId).ToList();
                    if (pInfoList != null && pInfoList.Count > 0)
                    {
                        int lastRole = 0;
                        AdminPrivilege privilege = new AdminPrivilege();
                        foreach(var pInfo in pInfoList)
                        {
                            if(lastRole != pInfo.adminLevel)
                            {
                                privilege = new AdminPrivilege();
                                privilege.AdminRole = pInfo.adminLevel;
                                lastRole = pInfo.adminLevel;
                            }
                            
                            //if has super admin access, no need of other admin rights
                            if(pInfo.adminLevel == 1)
                            {
                                privilegeInfo.Add(privilege);
                                break;
                            }
                            //if have admin rights other then Super Admin
                            else
                            {
                                switch(pInfo.adminLevel)
                                {
                                    
                                    case 2:// check for department admin roles
                                        DepartmentAdminRights adminDepList = new DepartmentAdminRights();
                                        if (pInfo.adminDepartmentLocationId != null && pInfo.adminDepartmentID != null)
                                        {
                                            privilege.DepartmentAccess = new List<DepartmentAdminRights>();
                                            adminDepList.LocationID = Convert.ToInt32(pInfo.adminDepartmentLocationId);
                                            adminDepList.DepartmentID = Convert.ToInt32(pInfo.adminDepartmentID);
                                            AdminDepartmentPrivilege.Add(adminDepList);
                                            privilege.DepartmentAccess = AdminDepartmentPrivilege;
                                            privilegeInfo.Add(privilege);
                                        }
                                        break;
                                        
                                    case 3:// check for location admin roles
                                        if (pInfo.adminLocationID != null)
                                        {
                                            privilege.LocationAccess = new List<int>();
                                            AdminLocationPrivilege.Add(Convert.ToInt32(pInfo.adminLocationID));
                                            privilege.LocationAccess = AdminLocationPrivilege;
                                            privilegeInfo.Add(privilege);
                                        }
                                        break;
                                    case 4:// check for report admin roles
                                        privilegeInfo.Add(privilege);
                                        break;
                                    case 5:// check for user admin roles
                                        privilegeInfo.Add(privilege);
                                        break;
                                    case 6:// check for reseller admin roles
                                        privilegeInfo.Add(privilege);
                                        break;
                                    case 8:// check for location supervisor roles
                                        if (pInfo.spvLocationId != null)
                                        {
                                            privilege.LocationSpvAccess = new List<int>();
                                            SpvLocationPrivilege.Add(Convert.ToInt32(pInfo.spvLocationId));
                                            privilege.LocationSpvAccess = SpvLocationPrivilege;
                                            privilegeInfo.Add(privilege);
                                        }
                                        break;
                                    case 9:// check for department supervisor roles
                                        DepartmentAdminRights supDepList = new DepartmentAdminRights();
                                        if (pInfo.spvLocationDepartmentID != null && pInfo.spvDepartmentID != null)
                                        {
                                            privilege.DepartmentAccess = new List<DepartmentAdminRights>();
                                            supDepList.LocationID = Convert.ToInt32(pInfo.spvLocationDepartmentID);
                                            supDepList.DepartmentID = Convert.ToInt32(pInfo.spvDepartmentID);
                                            SpvDepartmentPrivilege.Add(supDepList);
                                            privilege.DepartmentSpvAccess = SpvDepartmentPrivilege;
                                            privilegeInfo.Add(privilege);
                                        }
                                        break;
                                }
                            }
                            //privilege.DepartmentAccess = new List<DepartmentAdminRights>();
                            //privilege.LocationAccess = new List<int>();
                            //privilege.DepartmentAccess = AdminDepartmentPrivilege;
                            //privilege.LocationAccess = AdminLocationPrivilege;
                        }
                    }
                    privilegeInfo = privilegeInfo.Distinct().ToList();
                    return privilegeInfo;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
    }
}
