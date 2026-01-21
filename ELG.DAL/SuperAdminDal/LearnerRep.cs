using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ELG.Model.SuperAdmin;
using ELG.DAL.Utilities;
using System.Data.Entity.Core.Objects;
using ELG.DAL.DBEntitySA;

namespace ELG.DAL.SuperAdminDAL
{
    public class LearnerRep
    {
        /// <summary>
        /// Return list of registered learner based on search criteria
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public OrganisationLearnerList GetRegisteredLearners(LearnerReportSearch searchCriteria)
        {
            try
            {
                OrganisationLearnerList orglearnerList = new OrganisationLearnerList();
                List<LearnerInfo> learnerInfoList = new List<LearnerInfo>();

                using (var context = new superadmindbEntities())
                {
                    var learnerList = context.lms_superadmin_get_RegisteredLearners(searchCriteria.SearchLearnerName, searchCriteria.SearchLearnerOrganisation, searchCriteria.SearchStatus, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (learnerList != null && learnerList.Count > 0)
                    {
                        orglearnerList.TotalLearners = learnerList.Count();
                        var data = learnerList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            LearnerInfo learner = new LearnerInfo();
                            learner.UserID = item.intContactID;
                            learner.FirstName = item.strFirstName;
                            learner.LastName = item.strSurname;
                            learner.EmailId = item.strEmail;
                            learner.Company = item.strOrganisation;
                            learner.Location = item.strLocation;
                            learner.Department = item.strDepartment;
                            learner.CompanyNumber = item.ul_sequentialid;
                            learner.IsDeactive = Convert.ToBoolean(item.blnCancelled);
                            learner.Status = Convert.ToBoolean(item.blnCancelled) ? "In-active" : "Active";
                            learnerInfoList.Add(learner);
                        }
                    }
                }
                orglearnerList.LearnerList = learnerInfoList;
                return orglearnerList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Download learner list
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public List<DownloadLearnerList> DownloadLearnerReport(LearnerReportSearch searchCriteria)
        {
            try
            {
                List<DownloadLearnerList> learnerRecord = new List<DownloadLearnerList>();

                using (var context = new superadmindbEntities())
                {
                    var learnerList = context.lms_superadmin_get_RegisteredLearners(searchCriteria.SearchLearnerName, searchCriteria.SearchLearnerOrganisation, searchCriteria.SearchStatus, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (learnerList != null && learnerList.Count > 0)
                    {
                        foreach (var item in learnerList)
                        {
                            DownloadLearnerList learner = new DownloadLearnerList();
                            learner.FirstName = item.strFirstName;
                            learner.LastName = item.strSurname;
                            learner.EmailId = item.strEmail;
                            learner.Company = item.strOrganisation;
                            learner.Location = item.strLocation;
                            learner.Department = item.strDepartment;
                            learner.Status = Convert.ToBoolean(item.blnCancelled) ? "In-active" : "Active";
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
        /// Reset learner's password to default
        /// </summary>
        /// <param name="intContactID"></param>
        /// <param name="usrpwd"></param>
        /// <param name="key"></param>
        public void UpdateUserPassword(Int64 intContactID, string usrpwd, string key)
        {
            try
            {
                using (var context = new superadmindbEntities())
                {
                    context.lms_superadmin_Reset_LearnerPassword(intContactID, CommonMethods.EncodePassword(usrpwd, key));
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get details of selected learner
        /// </summary>
        /// <param name="learnerID"></param>
        /// <returns></returns>
        public LearnerInfo GetLearnerInfo(Int64 learnerID)
        {
            try
            {
                LearnerInfo learner = new LearnerInfo();
                using (var context = new superadmindbEntities())
                {
                    var learnerInfo = context.lms_superadmin_get_LearnerInfo(learnerID).FirstOrDefault();
                    if (learnerInfo != null)
                    {
                        learner.UserID = learnerInfo.intContactID;
                        learner.FirstName = learnerInfo.strFirstName;
                        learner.LastName = learnerInfo.strSurname;
                        learner.EmailId = learnerInfo.strEmail;
                        learner.Company = learnerInfo.strOrganisation;
                        learner.Location = learnerInfo.strLocation;
                        learner.Department = learnerInfo.strDepartment;
                        learner.CompanyID = Convert.ToInt64(learnerInfo.intOrganisationID);
                        learner.LocationID = Convert.ToInt64(learnerInfo.intLocationID);
                        learner.DepartmentID = Convert.ToInt64(learnerInfo.intDepartmentID);
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
        /// Update details of a learner
        /// </summary>
        /// <param name="learner"></param>
        /// <returns></returns>
        public int UpdateLearnerInfo(LearnerInfo learner)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_update_LearnerInfo(learner.UserID, learner.FirstName, learner.LastName, learner.EmailId, learner.LocationID, learner.DepartmentID, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// update acitve status of learner; avtivate or deactivate leanre
        /// </summary>
        /// <param name="learnerID"></param>
        /// <param name="deactivate"></param>
        /// <returns></returns>
        public int UpdateLearnerActiveStatus(Int64 learnerID, bool deactivate)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_update_LearnerStatus(learnerID, deactivate, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get login info for a learner
        /// </summary>
        /// <param name="learnerID"></param>
        /// <returns></returns>
        public LearnerLoginReport GetLearnerLoginInfo(LearnerLogReportSearch searchCriteria)
        {
            try
            {
                LearnerLoginReport learnerLoginList = new LearnerLoginReport();
                List<LearnerLoginInfo> learnerInfoList = new List<LearnerLoginInfo>();
                using (var context = new superadmindbEntities())
                {
                    var learnerList = context.lms_superadmin_getLearnerLoginDetails(searchCriteria.LearnerId, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (learnerList != null && learnerList.Count > 0)
                    {
                        learnerLoginList.TotalRecords = learnerList.Count();
                        var data = learnerList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var learnerInfo in data)
                        {
                            LearnerLoginInfo learner = new LearnerLoginInfo();
                            learner.RecordId = learnerInfo.intRecordID;
                            //learner.LoginDate = Convert.ToString(learnerInfo.loginDate);
                            learner.LoginDate = learnerInfo.loginDate == null ? "" : (Convert.ToDateTime(learnerInfo.loginDate)).ToString("dd-MMM-yyyy HH:mm:ss");
                            learner.Browser = learnerInfo.Browser;
                            learner.BrowserVersion = learnerInfo.BrowserVersion;
                            learner.Device = learnerInfo.Device;
                            learner.IsMobileDevice = learnerInfo.isMobileDevice == true ? "Yes" : "No";
                            learner.OS = learnerInfo.OS;
                            learnerInfoList.Add(learner);
                        }
                    }
                }
                learnerLoginList.LoginLog = learnerInfoList;
                return learnerLoginList;
            }
            catch (Exception)
            {

                throw;
            }

        }

        /// <summary>
        /// Get course launch info for a learner
        /// </summary>
        /// <param name="learnerID"></param>
        /// <returns></returns>
        public LearnerLaunchReport GetLearnerCourseLaunchInfo(LearnerLogReportSearch searchCriteria)
        {
            try
            {
                LearnerLaunchReport learnerLoginList = new LearnerLaunchReport();
                List<LearnerLaunchInfo> learnerInfoList = new List<LearnerLaunchInfo>();
                using (var context = new superadmindbEntities())
                {
                    var learnerList = context.lms_superadmin_getCourseLauchDetails(searchCriteria.LearnerId, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (learnerList != null && learnerList.Count > 0)
                    {
                        learnerLoginList.TotalRecords = learnerList.Count();
                        var data = learnerList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var learnerInfo in data)
                        {
                            LearnerLaunchInfo learner = new LearnerLaunchInfo();
                            learner.RecordID = learnerInfo.intRecordID;
                            //learner.LaunchTime = Convert.ToString(learnerInfo.launchTime);
                            learner.LaunchTime = learnerInfo.launchTime == null ? "" : (Convert.ToDateTime(learnerInfo.launchTime)).ToString("dd-MMM-yyyy HH:mm:ss");
                            learner.Browser = learnerInfo.Browser;
                            learner.BrowserVersion = learnerInfo.BrowserVersion;
                            learner.Device = learnerInfo.Device;
                            learner.IsMobileDevice = learnerInfo.isMobileDevice == true ? "Yes" : "No";
                            learner.OS = learnerInfo.OS;
                            learner.CourseCloseTime = Convert.ToString(learnerInfo.courseCloseTime);
                            learner.CourseName = learnerInfo.strCourse;
                            learnerInfoList.Add(learner);
                        }
                    }
                }
                learnerLoginList.LaunchLog = learnerInfoList;
                return learnerLoginList;
            }
            catch (Exception)
            {

                throw;
            }

        }


        /// <summary>
        /// Get single course launch details for an user
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public LearnerLaunchReport GetLearnerCourseLaunchDetails(LearnerLogReportSearch searchCriteria)
        {
            try
            {
                LearnerLaunchReport learnerLoginList = new LearnerLaunchReport();
                List<LearnerLaunchInfo> learnerInfoList = new List<LearnerLaunchInfo>();
                using (var context = new superadmindbEntities())
                {
                    var learnerList = context.lms_superadmin_get_LearnerCourseLauchDetails(searchCriteria.LearnerId, searchCriteria.CourseId, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (learnerList != null && learnerList.Count > 0)
                    {
                        learnerLoginList.TotalRecords = learnerList.Count();
                        var data = learnerList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var learnerInfo in data)
                        {
                            LearnerLaunchInfo learner = new LearnerLaunchInfo();
                            learner.RecordID = learnerInfo.intRecordID;
                            learner.LaunchTime = learnerInfo.launchTime == null ? "" : (Convert.ToDateTime(learnerInfo.launchTime)).ToString("dd-MMM-yyyy HH:mm:ss");
                            learner.Browser = learnerInfo.Browser;
                            learner.BrowserVersion = learnerInfo.BrowserVersion;
                            learner.Device = learnerInfo.Device;
                            learner.IsMobileDevice = learnerInfo.isMobileDevice == true ? "Yes" : "No";
                            learner.OS = learnerInfo.OS;
                            learner.CourseCloseTime = Convert.ToString(learnerInfo.courseCloseTime);
                            learner.CourseName = learnerInfo.strCourse;
                            learnerInfoList.Add(learner);
                        }
                    }
                }
                learnerLoginList.LaunchLog = learnerInfoList;
                return learnerLoginList;
            }
            catch (Exception)
            {

                throw;
            }

        }

    }
}
