using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ELG.Model.OrgAdmin;
using ELG.DAL.DBEntity;
using ELG.DAL.Utilities;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Data;

namespace ELG.DAL.OrgAdminDAL
{
    public class LearnerRep
    {
        /// <summary>
        /// Function to create new learner in an organisation
        /// </summary>
        /// <param name="learner"></param>
        ///// <returns></returns>
        //public NewLearner CreateNewLearner(LearnerInfo learner, int mode)
        //{
        //    NewLearner Learner = new NewLearner();
        //    Learner.UserID = 0;
        //    try
        //    {
        //        ObjectParameter retVal = new ObjectParameter("id", typeof(long));
        //        ObjectParameter assign = new ObjectParameter("retval", typeof(int));
        //        ObjectParameter noLicence = new ObjectParameter("courseList", typeof(string));
        //        using (var context = new lmsdbEntities())
        //        {
        //            var result = context.lms_admin_createLearner(learner.Title, learner.FirstName, learner.LastName, learner.EmployeeNumber, learner.EmailId, learner.CompanyID, learner.LocationID, learner.DepartmentID, retVal);
        //            Learner.UserID = Convert.ToInt32(retVal.Value);

        //            // assign courses set for auto assignment
        //            if (Learner.UserID > 0)
        //            {
        //                List<string> strValidCourses = new List<string>();
        //                if (mode == 1)
        //                {
        //                    //assign all courses
        //                    var courses = context.lms_admin_getCoursesForOrganisation(learner.CompanyID).ToList();
        //                    if (courses != null && courses.Count > 0)
        //                    {
        //                        foreach (var course in courses)
        //                        {
        //                            strValidCourses.Add(Convert.ToString(course.intCourseID));
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    //assign courses set for auto allocation
        //                    var courses = context.lms_admin_getCoursesForAutoAssignement(learner.LocationID, learner.DepartmentID).ToList();
        //                    if (courses != null && courses.Count > 0)
        //                    {
        //                        foreach (var course in courses)
        //                        {
        //                            strValidCourses.Add(Convert.ToString(course.intCourseID));
        //                        }
        //                    }
        //                }

        //                //var courses = context.lms_admin_getCoursesForOrganisation(learner.CompanyID).ToList();
        //                //if(courses != null && courses.Count > 0)
        //                //{
        //                //    List<string> strValidCourses = new List<string>();
        //                //    foreach (var course in courses)
        //                //    {
        //                //        strValidCourses.Add(Convert.ToString(course.intCourseID));
        //                //    }
        //                //    string courseList = String.Join(",", strValidCourses);
        //                //    var assignResult = context.lms_admin_AssignCoursestoContact(0, Learner.UserID, Learner.UserID, courseList, assign, noLicence);
        //                //    Learner.CourseWithNoLicences = Convert.ToString(noLicence.Value);
        //                //}

        //                string courseList = String.Join(",", strValidCourses);
        //                if (!String.IsNullOrEmpty(courseList))
        //                {
        //                    var assignResult = context.lms_admin_AssignCoursestoContact(0, Learner.UserID, Learner.UserID, courseList, assign, noLicence);
        //                    Learner.CourseWithNoLicences = Convert.ToString(noLicence.Value);
        //                }

        //            }

        //        }

        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    return Learner;
        //}
        public NewLearner CreateNewLearner(LearnerInfo learner, int mode, int adminRole, Int64 adminId)
        {
            NewLearner Learner = new NewLearner();
            Learner.UserID = 0;

            try
            {
                using (var context = new lmsdbEntities())
                {
                    // Define output parameter for learner ID
                    var idParam = new SqlParameter("@id", SqlDbType.BigInt)
                    {
                        Direction = ParameterDirection.Output
                    };

                    // Execute stored procedure
                    context.Database.ExecuteSqlCommand(
                        "EXEC lms_admin_createLearner @title, @firstname, @lastname, @employeeNumber, @email, @organisation, @location, @department, @id OUTPUT",
                        new SqlParameter("@title", learner.Title),
                        new SqlParameter("@firstname", learner.FirstName),
                        new SqlParameter("@lastname", learner.LastName),
                        new SqlParameter("@employeeNumber", learner.EmployeeNumber),
                        new SqlParameter("@email", learner.EmailId),
                        new SqlParameter("@organisation", learner.CompanyID),
                        new SqlParameter("@location", learner.LocationID),
                        new SqlParameter("@department", learner.DepartmentID),
                        idParam
                    );

                    // Retrieve the output parameter value
                    Learner.UserID = Convert.ToInt32(idParam.Value);

                    // Assign courses set for auto assignment
                    if (Learner.UserID > 0)
                    {
                        List<string> strValidCourses = new List<string>();
                        if (mode == 1)
                        {
                            // Assign all courses
                            var courses = context.lms_admin_getCoursesForOrganisation(learner.CompanyID).ToList();
                            if (courses != null && courses.Count > 0)
                            {
                                foreach (var course in courses)
                                {
                                    strValidCourses.Add(Convert.ToString(course.intCourseID));
                                }
                            }
                        }
                        else
                        {
                            // Assign courses set for auto allocation
                            var courses = context.lms_admin_getCoursesForAutoAssignement(learner.LocationID, learner.DepartmentID).ToList();
                            if (courses != null && courses.Count > 0)
                            {
                                foreach (var course in courses)
                                {
                                    strValidCourses.Add(Convert.ToString(course.intCourseID));
                                }
                            }
                        }

                        string courseList = String.Join(",", strValidCourses);
                        if (!String.IsNullOrEmpty(courseList))
                        {
                            //var assignParam = new SqlParameter("@retval", SqlDbType.Int) { Direction = ParameterDirection.Output };
                            //var noLicenceParam = new SqlParameter("@courseList", SqlDbType.VarChar, 1000) { Direction = ParameterDirection.Output };

                            //context.Database.ExecuteSqlCommand(
                            //    "EXEC lms_admin_AssignCoursestoContact @adminRole, @adminUserId, @contactid, @courseids, @retval OUTPUT, @courseList OUTPUT",
                            //    new SqlParameter("@adminRole", 0),
                            //    new SqlParameter("@adminUserId", Learner.UserID),
                            //    new SqlParameter("@contactid", Learner.UserID),
                            //    new SqlParameter("@courseids", courseList),
                            //    assignParam,
                            //    noLicenceParam
                            //);

                            //Learner.CourseWithNoLicences = Convert.ToString(noLicenceParam.Value);
                            ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                            ObjectParameter noLicence = new ObjectParameter("courseList", typeof(string));
                            using (var context2 = new lmsdbEntities())
                            {
                                var result = context.lms_admin_AssignCoursestoContact(adminRole, adminId, Learner.UserID, courseList, retVal, noLicence);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating learner: " + ex.Message, ex);
            }

            return Learner;
        }


        /// <summary>
        /// Return list of registered learner based on search criteria
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public OrganisationLearnerList GetRegisteredLearners(AdminLearnerFilter searchCriteria, Int64 adminId, int adminRole)
        {
            try
            {
                OrganisationLearnerList orglearnerList = new OrganisationLearnerList();
                List<LearnerInfo> learnerInfoList = new List<LearnerInfo>();

                using (var context = new lmsdbEntities())
                {
                    var learnerList = context.lms_admin_getRegisteredLearners_perRole(adminRole, adminId, searchCriteria.SearchText, searchCriteria.SearchLocation, searchCriteria.SearchDepartment, searchCriteria.SearchStatus, searchCriteria.Company, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
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
                            learner.EmployeeNumber = item.strEmployeeNumber;
                            learner.EmailId = item.strEmail;
                            learner.Location = item.strLocation;
                            learner.Department = item.strDepartment;
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

        // download learner list
        public List<DownloadLearnerList> DownloadLearnerReport(AdminLearnerFilter searchCriteria, Int64 adminId, int adminRole)
        {
            try
            {
                List<DownloadLearnerList> learnerRecord = new List<DownloadLearnerList>();

                using (var context = new lmsdbEntities())
                {
                    var learnerList = context.lms_admin_getRegisteredLearners_perRole(adminRole, adminId, searchCriteria.SearchText, searchCriteria.SearchLocation, searchCriteria.SearchDepartment, searchCriteria.SearchStatus, searchCriteria.Company, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (learnerList != null && learnerList.Count > 0)
                    {
                        foreach (var item in learnerList)
                        {
                            DownloadLearnerList learner = new DownloadLearnerList();
                            learner.FirstName = item.strFirstName;
                            learner.LastName = item.strSurname;
                            learner.EmailId = item.strEmail;
                            learner.Location = item.strLocation;
                            learner.Department = item.strDepartment;
                            learner.EmployeeNumber = item.strEmployeeNumber;
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
        /// To get details of the contractor
        /// </summary>
        /// <param name="Company"></param>
        /// <returns></returns>
        public CompanyContractor GetContractorInfo(Int64 Company)
        {
            try
            {
                CompanyContractor contractor = new CompanyContractor();
                using (var context = new lmsdbEntities())
                {
                    var contractorInfo = context.lms_admin_get_companyContractorInfo(Company).FirstOrDefault();
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
        /// Get details of selected learner
        /// </summary>
        /// <param name="learnerID"></param>
        /// <returns></returns>
        public LearnerInfo GetLearnerInfo(Int64 learnerID)
        {
            try
            {
                LearnerInfo learner = new LearnerInfo();
                using (var context = new lmsdbEntities())
                {
                    var learnerInfo = context.lms_admin_getLearnerInfo(learnerID).FirstOrDefault();
                    if (learnerInfo != null)
                    {
                        learner.UserID = learnerInfo.intContactID;
                        learner.FirstName = learnerInfo.strFirstName;
                        learner.LastName = learnerInfo.strSurname;
                        learner.EmployeeNumber = learnerInfo.strEmployeeNumber;
                        learner.EmailId = learnerInfo.strEmail;
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
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_updateLearnerInfo(learner.UserID, learner.FirstName, learner.LastName, learner.EmployeeNumber, learner.EmailId, learner.LocationID, learner.DepartmentID, retVal);
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
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_changeLearnerStatus(learnerID, deactivate, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Delete selected learner with all associated records
        /// </summary>
        /// <param name="learnerID"></param>
        /// <returns></returns>
        //public int DeleteLearner(Int64 learnerID)
        //{
        //    int res = 0;
        //    try
        //    {
        //        ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
        //        using (var context = new lmsdbEntities())
        //        {
        //            var result = context.lms_admin_deleteLearner(learnerID, retVal);
        //            res = Convert.ToInt32(retVal.Value);
        //        }
        //        return res;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}
        public int DeleteLearner(Int64 learnerID)
        {
            int res = 0;

            try
            {
                using (var context = new lmsdbEntities())
                {
                    // Define output parameter
                    var retvalParam = new SqlParameter("@retval", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };

                    // Execute stored procedure
                    context.Database.ExecuteSqlCommand("EXEC lms_admin_deleteLearner @learner, @retval OUTPUT",
                        new SqlParameter("@learner", learnerID),
                        retvalParam);

                    // Get the output parameter value
                    res = (int)retvalParam.Value;
                }
            }
            catch (Exception ex)
            {
                // Log exception if needed
                throw new Exception("Error deleting learner: " + ex.Message, ex);
            }

            return res;
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
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_update_profilePic(Learner, null, retVal);
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
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_update_profilePic(Learner, ProfilePic, retVal);
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
        /// get all assigned admin rights for a learner
        /// </summary>
        /// <param name="learner"></param>
        /// <returns></returns>
        public LearnerAdminRights GetLearnerAdminRights(Int64 learner)
        {
            try
            {
                LearnerAdminRights learnerAdmin = new LearnerAdminRights();
                List<int> adminRights = new List<int>();
                using (var context = new lmsdbEntities())
                {
                    var adminInfo = context.lms_admin_getUserAdminRights(learner).ToList();
                    if (adminInfo != null && adminInfo.Count > 0)
                    {
                        learnerAdmin.UserID = adminInfo[0].intContactID;
                        learnerAdmin.FirstName = adminInfo[0].strFirstName;
                        learnerAdmin.LastName = adminInfo[0].strSurname;
                        foreach(var admin in adminInfo)
                        {
                            adminRights.Add(Convert.ToInt32(admin.intAdminLevelID));
                        }
                        learnerAdmin.AdminLevels = adminRights;
                    }
                }
                return learnerAdmin;

            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}
