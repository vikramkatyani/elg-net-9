using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ELG.Model.OrgAdmin;
using ELG.DAL.DBEntity;
using ELG.DAL.Utilities;
using ELG.DAL.DbEntityLearner;
using ELG.DAL.DBEntitySA;
using System.Data.Entity.Core.Objects;

namespace ELG.DAL.OrgAdminDAL
{
    public class ModuleRep
    {
        #region allocated modules
        /// <summary>
        /// function to get list of all learners to whom the selected module is not assigned
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public OrganisationLearnerList GetUsersWithoutModule(DataTableFilter searchCriteria)
        {
            try
            {
                OrganisationLearnerList orglearnerList = new OrganisationLearnerList();
                List<LearnerInfo> learnerInfoList = new List<LearnerInfo>();

                using (var context = new lmsdbEntities())
                {
                    var learnerList = context.lms_admin_getLearnersForModuleNotAssigned(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.Course, searchCriteria.SearchText, searchCriteria.Location, searchCriteria.Department, searchCriteria.Company, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
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
                            if (item.accessExpiryDate != null && Convert.ToDateTime(item.accessExpiryDate).Date < DateTime.UtcNow.Date)
                                learner.IsCourseExpired = true;
                            else
                                learner.IsCourseExpired = false;
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
        /// function to revoke access of a module for learer
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public int RevokeModuleAccess(LearnerModuleFilter searchCriteria)
        {
            try
            {
                int result = 0;
                using (var context = new lmsdbEntities())
                {
                    context.lms_admin_RevokeLearnerModuleAccess(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.Course, searchCriteria.SearchText, searchCriteria.Location, searchCriteria.Department, searchCriteria.Company, searchCriteria.LearnerID);
                    result = 1;
                }
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Function to revoke access of module from multiple users
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <param name="selectedLearnerList"></param>
        /// <param name="unselectedLearnerList"></param>
        /// <param name="isAllSelected"></param>
        /// <returns></returns>
        public int RevokeModuleAccess_Multiple(LearnerModuleFilter searchCriteria,string selectedLearnerList, string unselectedLearnerList, bool isAllSelected)
        {
            try
            {
                int result = 0;
                using (var context = new lmsdbEntities())
                {
                    context.lms_admin_RevokeLearnerModuleAccess_Multiple(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.Course, searchCriteria.SearchText, searchCriteria.Location, searchCriteria.Department, searchCriteria.Company, selectedLearnerList, unselectedLearnerList, isAllSelected);
                    result = 1;
                }
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }


        #endregion

        #region manage module access
        /// <summary>
        /// function to get list of all learners to whom the selected module is assigned
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public OrganisationLearnerList GetUsersWithModule(DataTableFilter searchCriteria)
        {
            try
            {
                OrganisationLearnerList orglearnerList = new OrganisationLearnerList();
                List<LearnerInfo> learnerInfoList = new List<LearnerInfo>();

                using (var context = new lmsdbEntities())
                {
                    var learnerList = context.lms_admin_getLearnersToRevokeModule(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.Course, searchCriteria.SearchText, searchCriteria.Location, searchCriteria.Department, searchCriteria.Company, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (learnerList != null && learnerList.Count > 0)
                    {
                        orglearnerList.TotalLearners = learnerList.Count();
                        var data = learnerList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            LearnerInfo learner = new LearnerInfo();
                            learner.UserID = Convert.ToInt64(item.intContactID);
                            learner.FirstName = item.strFirstName;
                            learner.LastName = item.strSurname;
                            learner.EmployeeNumber = item.strEmployeeNumber;
                            learner.EmailId = item.strEmail;
                            learner.Location = item.strLocation;
                            learner.Department = item.strDepartment;
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
        /// Function to Allocate Module License To Learner
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        /// 
        public int AllocateModuleLicenseToLearner(LearnerModuleFilter searchCriteria)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                ObjectParameter noLicence = new ObjectParameter("courseList", typeof(string));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_AssignCoursestoContact(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.LearnerID, Convert.ToString(searchCriteria.Course), retVal, noLicence);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int AllocateModuleLicenseToResellerLearner(LearnerModuleFilter searchCriteria)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_reseller_AssignCoursestoContact(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.LearnerID, Convert.ToString(searchCriteria.Course), retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Function to Allocate Module License To Learner
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        /// 
        //public int AllocateModuleLicenseToLearner_All(OrganisationLearnerList learnerList, Int64 Course)
        //{
        //    try
        //    {
        //        if(learnerList != null && learnerList.TotalLearners > 0)
        //        {
        //            ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
        //            ObjectParameter noLicence = new ObjectParameter("courseList", typeof(string));
        //            using (var context = new lmsdbEntities())
        //            {
        //                foreach(var learner in learnerList.LearnerList)
        //                {
        //                    var result = context.lms_admin_AssignCoursestoContact(learner.UserID, Convert.ToString(Course), retVal, noLicence);
        //                }
        //            }
        //            return Convert.ToInt32(retVal.Value);
        //        }
        //        else
        //        {
        //            return 0;
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}
        public int AllocateModuleLicenseToLearner_All(LearnerModuleFilter searchCriteria, bool allSelected, string selectedUserList, string unselectedUserList)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_allocate_assign_module_to_all(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.Course, searchCriteria.SearchText, searchCriteria.Location, searchCriteria.Department, searchCriteria.Company, selectedUserList, unselectedUserList, allSelected, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Function to Allocate Module License To Learner
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        /// 
        public int AllocateModuleLicenseToLearner_Multiple(Int64[] learnerList, Int64 Course)
        {
            try
            {
                if (learnerList != null && learnerList.Length > 0)
                {
                    ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                    ObjectParameter noLicence = new ObjectParameter("courseList", typeof(string));
                    using (var context = new lmsdbEntities())
                    {
                        foreach (var learner in learnerList)
                        {
                            var result = context.lms_admin_AssignCoursestoContact(0, 0, learner, Convert.ToString(Course), retVal, noLicence);
                        }
                    }
                    return Convert.ToInt32(retVal.Value);
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Function to Allocate Module License To Learner
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        /// 
        public int AllocateModuleLicenseToLearner_Multiple(LearnerModuleFilter searchCriteria, Int64[] selectedLearners, Int64[] unselectedLearners, bool selectAll)
        {
            try
            {
                if (selectAll)
                {

                }

                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                ObjectParameter noLicence = new ObjectParameter("courseList", typeof(string));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_AssignCoursestoContact(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.LearnerID, Convert.ToString(searchCriteria.Course), retVal, noLicence);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Function to reassign module access to filtered users
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public int ReAssignModuleAccess(LearnerModuleFilter searchCriteria)
        {
            try
            {
                int result = 0;
                using (var context = new lmsdbEntities())
                {
                    context.lms_admin_ReassignLearnerModuleAccess(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.Course, searchCriteria.SearchText, searchCriteria.Location, searchCriteria.Department, searchCriteria.Company, searchCriteria.LearnerID);
                    result = 1;
                }
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Module configuration
        /// <summary>
        /// Get list of all modules to configure details
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public OrgModuleList GetModules(DataTableFilter searchCriteria)
        {
            try
            {
                OrgModuleList orgModuleList = new OrgModuleList();
                List<Module> moduleInfoList = new List<Module>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_getOrganisationModules(searchCriteria.SearchText, searchCriteria.Course, searchCriteria.Company, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        orgModuleList.TotalModules = resultList.Count();
                        var data = resultList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            Module module = new Module();
                            module.ModuleID = item.intCourseID;
                            module.ModuleName = item.strCourse;
                            module.PassingMarks = Convert.ToInt32(item.intPassmark);
                            module.Frequency = item.inttutorialfrequency;
                            module.CompletionDays = Convert.ToInt32(item.intDaysForCompletion);
                            module.SubModuleCount = Convert.ToInt32(item.subModuleCount);
                            module.SubModuleCount = Convert.ToInt32(item.subModuleCount);
                            module.CourseLogo = item.strCourseLogo;
                            module.ModuleDesc = item.strCourseDescription;
                            module.CoursePath = item.strCourseTutorialPath;
                            moduleInfoList.Add(module);
                        }
                    }
                }
                orgModuleList.ModuleList = moduleInfoList;
                return orgModuleList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Update passing marks for a module
        /// </summary>
        /// <param name="moduleId"></param>
        /// <param name="marks"></param>
        /// <returns></returns>
        public int UpdateModulePassingMarks(Int64 moduleId, int frequency, int cmpDays)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                int marks = 80;
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_updateMinimumPassingMarks(moduleId, marks, frequency, cmpDays, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region License Summary
        public LicenseSummaryReport GetLicenseSummary(DataTableFilter searchCriteria)
        {
            try
            {
                LicenseSummaryReport licenseSummaryReport = new LicenseSummaryReport();
                List<ModuleLicenceSummary> moduleLicenceSummary = new List<ModuleLicenceSummary>();

                using (var context = new lmsdbEntities())
                {
                    var moduleList = context.lms_admin_getOrganisationLicenseSummary(searchCriteria.SearchText, searchCriteria.Course, searchCriteria.Company, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (moduleList != null && moduleList.Count > 0)
                    {
                        licenseSummaryReport.TotalModules = moduleList.Count();
                        var data = moduleList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            ModuleLicenceSummary module = new ModuleLicenceSummary();
                            module.ModuleID = Convert.ToInt64(item.intCourseID);
                            module.ModuleName = item.strCourse;
                            module.TotalLicenses = Convert.ToInt32(item.TotalLicenses);
                            module.AllocatedLicenses = Convert.ToInt32(item.ConsumedLicenses);
                            module.FreeLicenses = Convert.ToInt32(item.TotalLicenses) - Convert.ToInt32(item.ConsumedLicenses);
                            module.UsedLicenses = Convert.ToInt32(item.UsedLicences);
                            module.DeletedLicenses = Convert.ToInt32(item.DeletedLicences);
                            module.AvailableToRevokeLicenses = Convert.ToInt32(item.UnUsedLicences);
                            moduleLicenceSummary.Add(module);
                        }
                    }
                }
                licenseSummaryReport.ModuleList = moduleLicenceSummary;
                return licenseSummaryReport;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get licence transaction report
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public List<DownloadLicenceSummary> DownloadLicenceSummaryReport(DataTableFilter searchCriteria)
        {
            try
            {
                List<DownloadLicenceSummary> report = new List<DownloadLicenceSummary>();

                using (var context = new lmsdbEntities())
                {
                    var moduleList = context.lms_admin_getOrganisationLicenseSummary(searchCriteria.SearchText, searchCriteria.Course, searchCriteria.Company, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (moduleList != null && moduleList.Count > 0)
                    {
                        foreach (var item in moduleList)
                        {
                            DownloadLicenceSummary module = new DownloadLicenceSummary();
                            module.Course = item.strCourse;
                            module.TotalLicenses = Convert.ToInt32(item.TotalLicenses);
                            module.AllocatedLicenses = Convert.ToInt32(item.ConsumedLicenses);
                            module.AvailableLicenses = Convert.ToInt32(item.TotalLicenses) - Convert.ToInt32(item.ConsumedLicenses);
                            module.UsedLicenses = Convert.ToInt32(item.UsedLicences);
                            module.DeletedLicenses = Convert.ToInt32(item.DeletedLicences);
                            module.AvailableToRevokeLicenses = Convert.ToInt32(item.UnUsedLicences);
                            report.Add(module);
                        }
                    }
                }
                return report;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public LicenseTransactionReport GetLicenseTransactionSummary(LicenseTransactionFilter searchCriteria)
        {
            try
            {
                LicenseTransactionReport transactionSummaryReport = new LicenseTransactionReport();
                List<LicenseTransactionItem> moduleLicenceSummary = new List<LicenseTransactionItem>();

                using (var context = new lmsdbEntities())
                {
                    var moduleList = context.lms_admin_getLicenseTransactionReport(searchCriteria.Company, searchCriteria.SearchText, searchCriteria.Course, searchCriteria.SortCol, searchCriteria.SortColDir, searchCriteria.FromDate, searchCriteria.ToDate).ToList();
                    if (moduleList != null && moduleList.Count > 0)
                    {
                        transactionSummaryReport.TotalItems = moduleList.Count();
                        var data = moduleList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            LicenseTransactionItem module = new LicenseTransactionItem();
                            module.ModuleName = item.strCourse;
                            module.FirstName = item.strfirstname ?? "";
                            module.LastName = item.strsurname ?? "";
                            module.Email = item.stremail ?? "";
                            module.TransBy_FirstName = item.TransBy_fnmae ?? "";
                            module.TransBy_LastName = item.TransBy_lnmae ?? "";
                            module.TransBy_Email = item.TransBy_email ?? "";
                            module.LicenseCount = Convert.ToInt32(item.TransactionLicenseCount);
                            module.ActionDate = item.TransactionDate == null ? "" : (Convert.ToDateTime(item.TransactionDate)).ToString("dd-MMM-yyyy");
                            module.Action = item.LicenseTransactionDescription;
                            moduleLicenceSummary.Add(module);
                        }
                    }
                }
                transactionSummaryReport.TransactionSummary = moduleLicenceSummary;
                return transactionSummaryReport;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// Get licence transaction report
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public List<DownloadLicenceTransactionReport> DownloadLicenseTransactions(LicenseTransactionFilter searchCriteria)
        {
            try
            {
                List<DownloadLicenceTransactionReport> report = new List<DownloadLicenceTransactionReport>();

                using (var context = new lmsdbEntities())
                {
                    var moduleList = context.lms_admin_getLicenseTransactionReport(searchCriteria.Company, searchCriteria.SearchText, searchCriteria.Course, searchCriteria.SortCol, searchCriteria.SortColDir, searchCriteria.FromDate, searchCriteria.ToDate).ToList();
                    if (moduleList != null && moduleList.Count > 0)
                    {
                        foreach (var item in moduleList)
                        {
                            DownloadLicenceTransactionReport module = new DownloadLicenceTransactionReport();
                            module.Course = item.strCourse;
                            module.FirstName = item.strfirstname ?? "";
                            module.LastName = item.strsurname ?? "";
                            module.Email = item.stremail ?? "";
                            module.TransBy_FirstName = item.TransBy_fnmae ?? "";
                            module.TransBy_LastName = item.TransBy_lnmae ?? "";
                            module.TransBy_Email = item.TransBy_email ?? "";
                            module.LicenseCount = Convert.ToInt32(item.TransactionLicenseCount);
                            module.Date = item.TransactionDate == null ? "" : (Convert.ToDateTime(item.TransactionDate)).ToString("dd-MMM-yyyy");
                            module.Action = item.LicenseTransactionDescription;

                            report.Add(module);
                        }
                    }
                }
                return report;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Manage License allocation

        /// <summary>
        /// To get list of all users who have started the assigned module
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public OrganisationLearnerList GetLearnerWithConsumedLicense(DataTableFilter searchCriteria)
        {
            try
            {
                OrganisationLearnerList learnerList = new OrganisationLearnerList();
                List<LearnerInfo> learnerInfoList = new List<LearnerInfo>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_getLearnersWithStatrtedAssignedModule(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.SearchText, searchCriteria.Course, searchCriteria.Location, searchCriteria.Department, searchCriteria.Company, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        learnerList.TotalLearners = resultList.Count();
                        var data = resultList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            LearnerInfo learner = new LearnerInfo();
                            learner.UserID = Convert.ToInt64(item.intContactID);
                            learner.FirstName = item.strFirstName;
                            learner.LastName = item.strSurname;
                            learner.EmployeeNumber = item.strEmployeeNumber;
                            learner.EmailId = item.strEmail;
                            learner.Location = item.strLocation;
                            learner.Department = item.strDepartment;
                            learnerInfoList.Add(learner);
                        }
                    }
                }
                learnerList.LearnerList = learnerInfoList;
                return learnerList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// To get list of all users who haven't started the assigned module
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public OrganisationLearnerList GetLearnerWithAvailableLicense(DataTableFilter searchCriteria)
        {
            try
            {
                OrganisationLearnerList learnerList = new OrganisationLearnerList();
                List<LearnerInfo> learnerInfoList = new List<LearnerInfo>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_getLearnersWithNotStatrtedAssignedModule(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.SearchText, searchCriteria.Course, searchCriteria.Location, searchCriteria.Department, searchCriteria.Company, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        learnerList.TotalLearners = resultList.Count();
                        var data = resultList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            LearnerInfo learner = new LearnerInfo();
                            learner.UserID = Convert.ToInt64(item.intContactID);
                            learner.FirstName = item.strFirstName;
                            learner.LastName = item.strSurname;
                            learner.EmployeeNumber = item.strEmployeeNumber;
                            learner.EmailId = item.strEmail;
                            learner.Location = item.strLocation;
                            learner.Department = item.strDepartment;
                            learnerInfoList.Add(learner);
                        }
                    }
                }
                learnerList.LearnerList = learnerInfoList;
                return learnerList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// funtion to fetch list of Departments ForModule Auto Assignment
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public DepartmentListForModuleAutoAssignment GetDepartmentListForModuleAutoAssignment(DataTableFilter searchCriteria)
        {
            try
            {
                DepartmentListForModuleAutoAssignment departmentList = new DepartmentListForModuleAutoAssignment();
                List<DepartmentForLicenseAutoAssignment> departmentInfoList = new List<DepartmentForLicenseAutoAssignment>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_getDepartmentsForAutoLicenseAllocation(searchCriteria.Course, searchCriteria.Company, searchCriteria.Location, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        departmentList.TotalDepartments = resultList.Count();
                        var data = resultList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            DepartmentForLicenseAutoAssignment department = new DepartmentForLicenseAutoAssignment();
                            department.DepartmentId = Convert.ToInt64(item.intDepartmentID);
                            department.DepartmentName = item.strDepartment;
                            department.Assigned = Convert.ToBoolean(item.assigned);
                            departmentInfoList.Add(department);
                        }
                    }
                }
                departmentList.DepartmentList = departmentInfoList;
                return departmentList;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// funtion to fetch list of All Departments in Organisation ForModule Auto Assignment
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public DepartmentListForModuleAutoAssignment GetAllDepartmentListForModuleAutoAssignment(DataTableFilter searchCriteria)
        {
            try
            {
                DepartmentListForModuleAutoAssignment departmentList = new DepartmentListForModuleAutoAssignment();
                List<DepartmentForLicenseAutoAssignment> departmentInfoList = new List<DepartmentForLicenseAutoAssignment>();
                if (searchCriteria.Course > 0)
                {
                    using (var context = new lmsdbEntities())
                    {
                        var resultList = context.lms_admin_get_AutoAllocationMap(searchCriteria.Company, searchCriteria.Course, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                        if (resultList != null && resultList.Count > 0)
                        {
                            //group by department
                            var groupedByDepartment = resultList.GroupBy(loc => loc.intDepartmentID);
                            foreach (var group in groupedByDepartment)
                            {
                                Int64 depid = 0;
                                string depName = "";
                                int assigned = 1;

                                foreach (lms_admin_get_AutoAllocationMap_Result row in group)
                                {
                                    depid = Convert.ToInt64(row.intDepartmentID);
                                    depName = row.strDepartment;
                                    assigned = Convert.ToInt32(row.autoSet);
                                    if (row.autoSet == null || row.autoSet == 0)
                                        break;
                                }
                                DepartmentForLicenseAutoAssignment department = new DepartmentForLicenseAutoAssignment();
                                department.DepartmentId = depid;
                                department.DepartmentName = depName;
                                department.Assigned = assigned >= 1 ? true : false;
                                departmentInfoList.Add(department);
                            }

                            departmentList.TotalDepartments = departmentInfoList.Count();
                            var data = departmentInfoList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                            departmentList.DepartmentList = data;
                        }
                    }
                }
                else
                {
                    departmentList.TotalDepartments = 0;
                    departmentList.DepartmentList = new List<DepartmentForLicenseAutoAssignment>();
                }
                return departmentList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get list of all Locations for licence auto assignment
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public LocationListForModuleAutoAssignment GetAllLocationListForModuleAutoAssignment(DataTableFilter searchCriteria)
        {
            try
            {
                LocationListForModuleAutoAssignment locationList = new LocationListForModuleAutoAssignment();
                List<LocationForLicenseAutoAssignment> locationInfoList = new List<LocationForLicenseAutoAssignment>();
                if (searchCriteria.Course > 0)
                {
                    using (var context = new lmsdbEntities())
                    {
                        var resultList = context.lms_admin_get_AutoAllocationMap(searchCriteria.Company, searchCriteria.Course, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                        if (resultList != null && resultList.Count > 0)
                        {
                            //group by location
                            var groupedByLocation = resultList.GroupBy(loc => loc.intLocationID);
                            foreach (var group in groupedByLocation)
                            {
                                Int64 locid = 0;
                                string locName = "";
                                int assigned = 1;

                                foreach (lms_admin_get_AutoAllocationMap_Result row in group)
                                {
                                    locid = row.intLocationID;
                                    locName = row.strLocation;
                                    assigned = Convert.ToInt32(row.autoSet);
                                    if (row.autoSet == null || row.autoSet == 0)
                                        break;
                                }
                                LocationForLicenseAutoAssignment location = new LocationForLicenseAutoAssignment();
                                location.LocationId = locid;
                                location.LocationName = locName;
                                location.Assigned = assigned >= 1 ? true : false;
                                locationInfoList.Add(location);
                            }

                            locationList.TotalLocations = locationInfoList.Count();
                            var data = locationInfoList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                            locationList.LocationList = data;
                        }
                    }
                }
                else
                {
                    locationList.TotalLocations = 0;
                    locationList.LocationList = new List<LocationForLicenseAutoAssignment>();
                }
                return locationList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Function to set department on module license auto allocation - in selected location
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        /// 
        public int SetDepartmentForAutoAllocationOfModule(DepartmentFilterForLicenseAutoAssignment searchCriteria)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_setDepartmentsForAutoLicenseAllocation(searchCriteria.Course, searchCriteria.Location, searchCriteria.Departments, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// set auto allocation for departmetns in all locations in an organisation - multiple departments
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public int SetAllOrganisationDepartmentsForAutoAllocationOfModule(DepartmentFilterForLicenseAutoAssignment searchCriteria)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_setDepartmentsForAutoLicenseAllocationInOrganisation(searchCriteria.Course, searchCriteria.Organisation, searchCriteria.Departments, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// function to set auto allocation to selected department in all location of a company - single department
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public int SetLicenseAutoAllocationForEntireOrgDepartment(DataTableFilter searchCriteria)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_setAutoLicenseAllocation_entire_org_departments(searchCriteria.Course, searchCriteria.Department, searchCriteria.Company, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Function to set department on module license auto allocation
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        /// 
        public int SetAutoAllocationOfModuleForEntireOrg(DataTableFilter searchCriteria)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_setAutoLicenseAllocation_entire_org(searchCriteria.Course, searchCriteria.Company, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Function to set location on module license auto allocation
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        /// 
        public int SetAutoAllocationOfModuleForEntireLocationInOrg(LocationFilterForLicenseAutoAssignment searchCriteria)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_setLocationsForAutoLicenseAllocationInOrganisation(searchCriteria.Course, searchCriteria.Organisation, searchCriteria.Locations, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Function to remove department from module license auto allocation
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        /// 
        public int RemoveDepartmentForAutoAllocationOfModule(DepartmentFilterForLicenseAutoAssignment searchCriteria)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_removeDepartmentsForAutoLicenseAllocation(searchCriteria.Course, searchCriteria.Location, searchCriteria.Departments, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// To remove auto allocation from departments in all locations of an organisation
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public int RemoveDepartmentFromAllLocationsForAutoAllocationOfModule(DepartmentFilterForLicenseAutoAssignment searchCriteria)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_remove_EntireCompany_Departments_ForAutoLicenseAllocation(searchCriteria.Course, searchCriteria.Departments, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// To remove auto allocation from location of an organisation
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public int RemoveAllLocationsForAutoAllocationOfModule(LocationFilterForLicenseAutoAssignment searchCriteria)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_remove_EntireCompany_Locations_ForAutoLicenseAllocation(searchCriteria.Course, searchCriteria.Locations, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Function to free up unused licences from list of users
        /// </summary>
        /// <param name="Course"></param>
        /// <param name="userList"></param>
        /// <returns></returns>
        public int FreeUpUnusedLicenses(Int64 Course, String userList, int count)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_FreeUpUnusedLicenses(Course, userList, count, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }

        }


        /// <summary>
        /// Function to free up unused licences from list of users
        /// </summary>
        /// <param name="Course"></param>
        /// <param name="userList"></param>
        /// <returns></returns>
        public int FreeUpUnusedLicenses_Multiple(LearnerModuleFilter searchCriteria, String selectedLearners, String unselectedLearners, bool selectAll, int count)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_FreeUpUnusedLicensesMultiple(searchCriteria.Course, searchCriteria.SearchText, searchCriteria.Location, searchCriteria.Department, searchCriteria.Company, selectedLearners, unselectedLearners, selectAll, count, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }

        }
        #endregion

        #region Risk Assessment

        public OrgModuleList GetRiskAssessmentModules(DataTableFilter searchCriteria)
        {
            try
            {
                OrgModuleList orgModuleList = new OrgModuleList();
                List<Module> moduleInfoList = new List<Module>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_getAllRACourses(searchCriteria.Company, searchCriteria.SearchText, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        orgModuleList.TotalModules = resultList.Count();
                        var data = resultList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            Module module = new Module();
                            module.ModuleID = item.intCourseID;
                            module.ModuleName = item.strCourse;
                            module.RAFrequency = item.intRAFrequency;
                            module.CompletionDays = Convert.ToInt32(item.intDaysForCompletion);
                            moduleInfoList.Add(module);
                        }
                    }
                }
                orgModuleList.ModuleList = moduleInfoList;
                return orgModuleList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<RiskAssessmentQuestion> GetModuleRiskAssessmentQuestion(Int64 module)
        {
            List<RiskAssessmentQuestion> questionList = new List<RiskAssessmentQuestion>();
            try
            {
                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_getAllRACourseQuestions(module).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        foreach (var item in resultList)
                        {
                            RiskAssessmentQuestion ques = new RiskAssessmentQuestion();
                            ques.QuestionId = item.intQuestionID;
                            ques.QuestionText = item.strQuestion;
                            ques.QuestionType = item.intQuestionTypeID;
                            ques.Order = Convert.ToInt32(item.intOrder);
                            ques.GotoNO = Convert.ToInt32(item.intGotoNo);
                            ques.GotoYes = Convert.ToInt32(item.intGotoYes);
                            ques.NumberFrom = Convert.ToInt32(item.intNumberFrom);
                            ques.NumberTo = Convert.ToInt32(item.intNumberTo);
                            ques.FreeText = item.blnFreeText;
                            ques.MultiLine = item.blnMultiLine;
                            ques.End = item.blnEnd;
                            ques.BarChart = item.blnBarChart;
                            ques.BaseQuestion = item.blnBaseQuestion;
                            ques.Group = item.strGroup;
                            ques.CourseName = item.strCourse;
                            ques.OptionCount = Convert.ToInt32(item.OptionCount);

                            questionList.Add(ques);
                        }
                    }
                }
                return questionList;
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// Create a new group for risk assessment course
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public int CreateNewRAGroup(RiskAssessmentGroup group)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("id", typeof(long));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_createRAQueGroup(group.GroupName, group.CourseID, retVal);
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
        /// To get list of all group for a RA course
        /// </summary>
        /// <param name="courseID"></param>
        /// <returns></returns>
        public RiskAssessmentGroupList GetRiskAssessmentGroupList(Int64 courseID)
        {
            try
            {
                RiskAssessmentGroupList groupList = new RiskAssessmentGroupList();
                List<RiskAssessmentGroup> groupInfoList = new List<RiskAssessmentGroup>();

                using (var context = new lmsdbEntities())
                {
                    var gList = context.lms_admin_getAllRAGroups(courseID).ToList();
                    if (gList != null && gList.Count > 0)
                    {
                        groupList.TotalGroups = gList.Count();

                        foreach (var item in gList)
                        {
                            RiskAssessmentGroup group = new RiskAssessmentGroup();
                            group.CourseID = item.intCourseID;
                            group.GroupID = item.intGroupID;
                            group.GroupName = item.strGroup;
                            groupInfoList.Add(group);
                        }
                    }
                }
                groupList.RiskAssessmentGroups = groupInfoList;
                return groupList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// To get list of all group for a RA course
        /// </summary>
        /// <param name="courseID"></param>
        /// <returns></returns>
        public List<RiskAssessmentQuestionOption> GetRAQuestionOptionList(Int64 queId)
        {
            try
            {
                List<RiskAssessmentQuestionOption> optionList = new List<RiskAssessmentQuestionOption>();

                using (var context = new lmsdbEntities())
                {
                    var oList = context.lms_admin_getAllOptionsForQuestion(queId).ToList();
                    if (oList != null && oList.Count > 0)
                    {
                        foreach (var item in oList)
                        {
                            RiskAssessmentQuestionOption option = new RiskAssessmentQuestionOption();
                            option.Issue = item.blnIssue;
                            option.OptionText = item.strQuestionOption;
                            option.Order = item.intOrder;
                            option.QuestionId = queId;
                            option.QuestionOptionId = item.intQuestionOptionID;
                            optionList.Add(option);
                        }
                    }
                }
                return optionList;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// Create new question for Risk assessment course
        /// </summary>
        /// <param name="question"></param>
        /// <returns></returns>
        public int CreateNewRAQuestion(RiskAssessmentQuestion question)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("id", typeof(long));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_createRAQue(question.QuestionText, question.GroupId, question.Group, question.CourseId, retVal);
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
        /// Create new Risk assessment question option
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public int CreateNewRAQuestionOption(RiskAssessmentQuestionOption option)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("id", typeof(long));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_createRAQueOption(option.OptionText, option.QuestionId, option.Order, option.Issue, retVal);
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
        /// remove risk assessment question
        /// </summary>
        /// <param name="question"></param>
        /// <returns></returns>
        public int RemoveRiskAssessmentQuestion(Int64 question)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("id", typeof(long));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_removeRAQuestion(question, retVal);
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
        /// get risk assessment question details
        /// </summary>
        /// <param name="question"></param>
        /// <returns></returns>
        public RiskAssessmentQuestion GetRiskAssessmentQuestionInfo(Int64 question)
        {
            RiskAssessmentQuestion que = new RiskAssessmentQuestion();
            try
            {
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_getQuestionInfo(question).FirstOrDefault();
                    if (result != null )
                    {
                        que.QuestionId = result.intQuestionID;
                        que.QuestionText = result.strQuestion;
                        que.Order = Convert.ToInt32(result.intOrder);
                        que.GroupId = Convert.ToInt32(result.intGroupID);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return que;
        }

        /// <summary>
        /// delete option from ra question
        /// </summary>
        /// <param name="optionId"></param>
        /// <returns></returns>
        public int RemoveRiskAssessmentQuestionOption(Int64 optionId)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("id", typeof(long));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_deleteRAQuestionOption(optionId, retVal);
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
        /// Update risk assessment question option
        /// </summary>
        /// <param name="ques"></param>
        /// <returns></returns>
        public int UpdateRiskAssessmentQuestionOption(RiskAssessmentQuestionOption option)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(long));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_updateRAQuestionOption(option.QuestionOptionId, option.OptionText, option.Order, option.Issue, retVal);
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
        /// Update risk assessment question option
        /// </summary>
        /// <param name="ques"></param>
        /// <returns></returns>
        public int UpdateRiskAssessmentQuestion(RiskAssessmentQuestion question)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(long));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_updateRAQuestion(question.QuestionText, question.QuestionId, question.GroupId, question.Group, question.CourseId, question.Order, retVal);
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
        /// Update frequency of Risk Assessment
        /// </summary>
        /// <param name="raId"></param>
        /// <param name="frequency"></param>
        /// <returns></returns>
        public int UpdateRAFrequency(Int64 raId, int frequency, int cmpDays)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_update_RA_Frequency(raId, frequency, cmpDays, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion


        #region Risk Assessment access and retake
        /// <summary>
        /// function to get list of all learners to whom the selected RA is assigned
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public OrganisationLearnerList GetUsersWithRA(DataTableFilter searchCriteria)
        {
            try
            {
                OrganisationLearnerList orglearnerList = new OrganisationLearnerList();
                List<LearnerInfo> learnerInfoList = new List<LearnerInfo>();

                using (var context = new lmsdbEntities())
                {
                    var learnerList = context.lms_admin_getLearnersToRevokeRetakeRA(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.Course, searchCriteria.SearchText, searchCriteria.Location, searchCriteria.Department, searchCriteria.Company, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (learnerList != null && learnerList.Count > 0)
                    {
                        orglearnerList.TotalLearners = learnerList.Count();
                        var data = learnerList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            LearnerInfo learner = new LearnerInfo();
                            learner.UserID = Convert.ToInt64(item.intContactID);
                            learner.FirstName = item.strFirstName;
                            learner.LastName = item.strSurname;
                            learner.EmployeeNumber = item.strEmployeeNumber;
                            learner.EmailId = item.strEmail;
                            learner.Location = item.strLocation;
                            learner.Department = item.strDepartment;
                            learner.IsRASignedOff = Convert.ToBoolean(item.SignedOff);
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
        /// function to set new risk assessment (retake) for learner
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public int RetakeRAAccess(LearnerModuleFilter searchCriteria)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_set_LearnersToRetakeRA(searchCriteria.LearnerID, searchCriteria.Course, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Reseller Licence Transaction Report

        public ResellerLicenseTransactionReport GetResellerTransactionReport(ResellerLicenceReportFilter searchCriteria)
        {
            try
            {
                ResellerLicenseTransactionReport transactionReport = new ResellerLicenseTransactionReport();
                List<ResellerLicenseTransactionItem> transactionRecord = new List<ResellerLicenseTransactionItem>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_reseller_get_LicenceTransactionReport(searchCriteria.Company, searchCriteria.SortCol, searchCriteria.SortColDir, searchCriteria.FromDate, searchCriteria.ToDate).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        transactionReport.TotalItems = resultList.Count();
                        var data = resultList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            ResellerLicenseTransactionItem transaction = new ResellerLicenseTransactionItem();
                            transaction.Action = item.LicenseTransactionDescription;
                            transaction.ActionDate = item.TransactionDate == null ? "" : (Convert.ToDateTime(item.TransactionDate)).ToString("dd-MMM-yyyy");
                            transaction.LicenseCount = Convert.ToInt32(item.TransactionLicenseCount);
                            transaction.ModuleName = item.strCourse;
                            transaction.OrganisationId = Convert.ToInt64(item.intOrganisationID);
                            transaction.OrganisationName = item.strOrganisation;
                            transaction.TransactionId = item.TransactionId;
                            transaction.TransationType = item.TransactionType;
                            transactionRecord.Add(transaction);
                        }
                    }
                }
                transactionReport.TransactionSummary = transactionRecord;
                return transactionReport;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // download progress report
        public List<ResellerLicenseTransactionDownloadItem> DownloadResellerTransactionReport(ResellerLicenceReportFilter searchCriteria)
        {
            try
            {
                List<ResellerLicenseTransactionDownloadItem> transactionRecord = new List<ResellerLicenseTransactionDownloadItem>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_reseller_get_LicenceTransactionReport(searchCriteria.Company, searchCriteria.SortCol, searchCriteria.SortColDir, searchCriteria.FromDate, searchCriteria.ToDate).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        foreach (var item in resultList)
                        {
                            ResellerLicenseTransactionDownloadItem transaction = new ResellerLicenseTransactionDownloadItem();
                            transaction.Action = item.LicenseTransactionDescription;
                            transaction.ActionDate = item.TransactionDate == null ? "" : (Convert.ToDateTime(item.TransactionDate)).ToString("dd-MMM-yyyy");
                            transaction.LicenseCount = Convert.ToInt32(item.TransactionLicenseCount);
                            transaction.ModuleName = item.strCourse;
                            transaction.OrganisationName = item.strOrganisation;
                            transactionRecord.Add(transaction);
                        }
                    }
                }
                return transactionRecord;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region refresh training - fixed calendar
        public string CreateOTPTransaction(string otp, string otpFor, string otpTo)
        {
            try
            {
                ObjectParameter txnid = new ObjectParameter("txnid", typeof(Guid));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_create_otp_transaction(otp, otpFor, otpTo, txnid);
                }
                return Convert.ToString(txnid.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region refresh learning manual

        public int RefreshLearerModuleProgress(LearnerModuleFilter searchCriteria)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_resetLearningProgress_manually(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.LearnerID, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int RefreshLearerModuleProgress_Multiple(DataTableFilter searchCriteria, string selectedRecordList, string unselectedRecordList, bool isAllSelected)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_resetLearningProgress_manually_multiple(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.SearchText, searchCriteria.Location, searchCriteria.Department, searchCriteria.Course, searchCriteria.Company, selectedRecordList, unselectedRecordList, isAllSelected, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region sub modules
        public List<SubModule> GetCourseSubModules(long courseId)
        {
            try
            {
                List<SubModule> subModuleList = new List<SubModule>();
                if (courseId > 0)
                {
                    using (var context = new lmsdbEntities())
                    {
                        var resultList = context.lms_admin_get_course_submodules(courseId).ToList();
                        if (resultList != null && resultList.Count > 0)
                        {
                            foreach (var item in resultList)
                            {
                                SubModule sm = new SubModule();
                                sm.SubModuleID = item.smid;
                                sm.SubModuleName = item.name;
                                sm.SubModuleDesc = item.descrip;
                                sm.Sequence = Convert.ToInt32(item.seq);
                                subModuleList.Add(sm);
                            }
                        }
                    }
                }
                return subModuleList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int CreateNewSubModule(SubModule sm)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("id", typeof(long));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_create_submodule(sm.CourseId, sm.SubModuleName, sm.SubModuleDesc, sm.SubModulePath, sm.CreatedById, sm.RAID, retVal);
                    success = Convert.ToInt32(retVal.Value);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return success;
        }

        public OrgModuleList GetRiskAssessmentSubModulesForCourse(DataTableFilter searchCriteria)
        {
            try
            {
                OrgModuleList orgModuleList = new OrgModuleList();
                List<Module> moduleInfoList = new List<Module>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_getRAasSubModulesForCourse(searchCriteria.Company, searchCriteria.Course, searchCriteria.SearchText).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        orgModuleList.TotalModules = resultList.Count();
                        var data = resultList;//.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            Module module = new Module();
                            module.ModuleID = item.intCourseID;
                            module.ModuleName = item.strCourse;
                            moduleInfoList.Add(module);
                        }
                    }
                }
                orgModuleList.ModuleList = moduleInfoList;
                return orgModuleList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Course details

        public Module GetCourseDetails(Int64 courseid)
        {
            try
            {
                Module module = new Module();
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_get_CourseDetails(courseid).FirstOrDefault();
                    if (result != null)
                    {
                        module.ModuleID = result.intCourseID;
                        module.ModuleName = result.strCourse;
                        module.ModuleDesc = result.strCourseDescription;
                    }
                    
                    // Get course path from tbCourse table
                    var coursePathQuery = "SELECT strCourseTutorialPath FROM tbCourse WHERE intCourseID = @p0";
                    var coursePath = context.Database.SqlQuery<string>(coursePathQuery, courseid).FirstOrDefault();
                    if (!string.IsNullOrEmpty(coursePath))
                    {
                        module.CoursePath = coursePath;
                    }
                }
                return module;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Update course details including name, description, thumbnail, and optionally course path
        /// </summary>
        public int UpdateCourseDetails(Int64 courseId, string courseName, string courseDesc, string thumbnailPath, string coursePath = null)
        {
            try
            {
                using (var context = new lmsdbEntities())
                {
                    // Build parameterized SQL query
                    string query;
                    if (string.IsNullOrEmpty(coursePath))
                    {
                        query = @"UPDATE tbCourse 
                                 SET strCourse = @p0, 
                                     strCourseDescription = @p1, 
                                     strCourseLogo = @p2,
                                     datModified = @p3
                                 WHERE intCourseID = @p4";
                        return context.Database.ExecuteSqlCommand(query, courseName, courseDesc ?? "", thumbnailPath ?? "", DateTime.Now, courseId);
                    }
                    else
                    {
                        query = @"UPDATE tbCourse 
                                 SET strCourse = @p0, 
                                     strCourseDescription = @p1, 
                                     strCourseLogo = @p2,
                                     strCourseTutorialPath = @p3,
                                     datModified = @p4
                                 WHERE intCourseID = @p5";
                        return context.Database.ExecuteSqlCommand(query, courseName, courseDesc ?? "", thumbnailPath ?? "", coursePath, DateTime.Now, courseId);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Create a new SCORM course
        /// </summary>
        public int CreateScormCourse(int companyId, string courseTitle, string courseDescription, 
            string courseLogo, string startPath, string uniqueCourseId)
        {
            try
            {
                using (var context = new ELG.DAL.DbEntityLearner.learnerDBEntities())
                {
                    var newCourse = new tbCourse
                    {
                        intOrganisationID = companyId,
                        intCourseIdentifier = uniqueCourseId,
                        strCourse = courseTitle,
                        strCourseDescription = courseDescription,
                        strCourseLogo = courseLogo,
                        strCourseTutorialPath = startPath,
                        blnCourseActive = true,
                        blnCancelled = false,
                        blnHTML5 = true,
                        blnCourseRA = false,
                        datCreated = DateTime.UtcNow,
                        datModified = DateTime.UtcNow,
                        intTestFrequency = 0,
                        blnTestReminders = false,
                        intRAFrequency = 0,
                        blnRAReminder = false,
                        intTutorialFrequency = 0,
                        blnTutorialReminder = false,
                        intTestReminderFreq = 0,
                        intRAReminderFreq = 0,
                        intTutorialReminderFreq = 0,
                        intMaxTestReminders = 0,
                        intMaxRAReminders = 0,
                        intMaxTutorialReminders = 0,
                        intRADaysBefore = 0,
                        intTestDaysBefore = 0,
                        intTutorialDaysBefore = 0,
                        blnDeptSendRAIssue = false,
                        blnDeptSendRAComplete = false,
                        blnDeptSendTestPass = false,
                        blnDeptSendTestFail = false,
                        blnDeptSendRANotCompleted = false,
                        blnDeptSendTestNotCompleted = false,
                        blnLocSendRAIssue = false,
                        blnLocSendRAComplete = false,
                        blnLocSendTestPass = false,
                        blnLocSendTestFail = false,
                        blnLocSendRANotCompleted = false,
                        blnLocSendTestNotCompleted = false,
                        blnContactSendTest = false,
                        blnContactSendRA = false
                    };

                    context.tbCourses.Add(newCourse);
                    context.SaveChanges();

                    return newCourse.intCourseID;
                }
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        /// <summary>
        /// Create a new Risk Assessment course for an organization admin
        /// Inserts into tbCourse with blnCourseRA = 1
        /// </summary>
        public int CreateRiskAssessmentCourse(int companyId, string courseTitle, string courseDescription, string courseLogo)
        {
            try
            {
                using (var context = new ELG.DAL.DbEntityLearner.learnerDBEntities())
                {
                    var newCourse = new tbCourse
                    {
                        intOrganisationID = companyId,
                        intCourseIdentifier = Guid.NewGuid().ToString(),
                        strCourse = courseTitle,
                        strCourseDescription = courseDescription,
                        strCourseLogo = courseLogo,
                        // No tutorial/test path for RA
                        strCourseTutorialPath = string.Empty,
                        strCourseTestPath = string.Empty,
                        blnCourseActive = true,
                        blnCancelled = false,
                        blnHTML5 = true,
                        blnCourseRA = true,
                        datCreated = DateTime.UtcNow,
                        datModified = DateTime.UtcNow,
                        intTestFrequency = 0,
                        blnTestReminders = false,
                        intRAFrequency = 0,
                        blnRAReminder = false,
                        intTutorialFrequency = 0,
                        blnTutorialReminder = false,
                        intTestReminderFreq = 0,
                        intRAReminderFreq = 0,
                        intTutorialReminderFreq = 0,
                        intMaxTestReminders = 0,
                        intMaxRAReminders = 0,
                        intMaxTutorialReminders = 0,
                        intRADaysBefore = 0,
                        intTestDaysBefore = 0,
                        intTutorialDaysBefore = 0,
                        blnDeptSendRAIssue = false,
                        blnDeptSendRAComplete = false,
                        blnDeptSendTestPass = false,
                        blnDeptSendTestFail = false,
                        blnDeptSendRANotCompleted = false,
                        blnDeptSendTestNotCompleted = false,
                        blnLocSendRAIssue = false,
                        blnLocSendRAComplete = false,
                        blnLocSendTestPass = false,
                        blnLocSendTestFail = false,
                        blnLocSendRANotCompleted = false,
                        blnLocSendTestNotCompleted = false,
                        blnContactSendTest = false,
                        blnContactSendRA = false
                    };

                    context.tbCourses.Add(newCourse);
                    context.SaveChanges();

                    return newCourse.intCourseID;
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Get total course count for an organization
        /// </summary>
        public int GetCourseCountByOrganization(int companyId)
        {
            try
            {
                using (var context = new ELG.DAL.DbEntityLearner.learnerDBEntities())
                {
                    return context.tbCourses.Where(c => c.intOrganisationID == companyId && c.blnCourseActive != false).Count();
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Get max allowed course count for an organization
        /// Returns null if no limit is set (unlimited courses allowed)
        /// Can be overridden with environment variable COURSE_QUOTA_LIMIT for testing
        /// </summary>
        public int? GetMaxAllowedCourseCount(int companyId)
        {
            try
            {
                // Check for environment override for testing
                string envQuota = System.Environment.GetEnvironmentVariable("COURSE_QUOTA_LIMIT");
                if (!string.IsNullOrEmpty(envQuota) && int.TryParse(envQuota, out int testQuota))
                {
                    return testQuota;
                }

                // Get max allowed courses from stored procedure (same source as dashboard)
                using (var context = new lmsdbEntities())
                {
                    // Use admin role 0 to get organization info regardless of user role
                    var result = context.lms_admin_get_dash_HeaderInfo(0, 0, companyId).FirstOrDefault();
                    if (result != null && result.MaxCourses.HasValue)
                    {
                        return result.MaxCourses.Value;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting max allowed course count: {ex.Message}");
                return null;
            }
        }

        #endregion
    }
}
