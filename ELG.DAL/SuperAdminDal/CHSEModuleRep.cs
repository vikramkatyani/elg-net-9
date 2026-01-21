using ELG.DAL.DBEntitySA;
using ELG.Model.SuperAdmin;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;

namespace ELG.DAL.SuperAdminDAL
{
    public class CHSEModuleRep
    {
        /// <summary>
        /// Get listing of all modules in the system
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public CHSEModuleListing GetCHSEModuleListing(ModuleListingSearch searchCriteria)
        {
            try
            {
                CHSEModuleListing moduleList = new CHSEModuleListing();
                List<CHSEModule> moduleInfoList = new List<CHSEModule>();

                using (var context = new superadmindbEntities())
                {
                    var moduleListData = context.lms_superadmin_get_moduleListing(searchCriteria.ModuleName, searchCriteria.Status, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (moduleListData != null && moduleListData.Count > 0)
                    {
                        moduleList.TotalRecords = moduleListData.Count();
                        var data = moduleListData.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            CHSEModule mod = new CHSEModule();
                            mod.ModuleId = Convert.ToInt64(item.intCourseID);
                            mod.ModuleName = item.strCourse;
                            mod.Path = item.Path;
                            mod.Status = Convert.ToBoolean(item.isArchived) ? "Archived" : "Active";
                            moduleInfoList.Add(mod);
                        }
                    }
                }
                moduleList.ModuleList = moduleInfoList;
                return moduleList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// get module info
        /// </summary>
        /// <param name="modId"></param>
        /// <returns></returns>
        public CHSEModule GetModuleInfo(Int64 modId)
        {
            try
            {
                CHSEModule mod = new CHSEModule();
                using (var context = new superadmindbEntities())
                {
                    var modInfo = context.lms_superadmin_get_CourseInfo(modId).FirstOrDefault();
                    if (modInfo != null)
                    {
                        mod.ModuleName = modInfo.strCourse;
                        mod.ModuleSummary = modInfo.strCourseSummary;
                        mod.ModuleDescription = modInfo.strCourseDescription;
                    }
                }
                return mod;
            }
            catch (Exception)
            {

                throw;
            }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="raid"></param>
        /// <returns></returns>
        public int UpdateModuleInfo(CHSEModule mod)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_update_CourseInfo(mod.ModuleId, mod.ModuleName, mod.ModuleSummary, mod.ModuleDescription, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Get list of all modules with assigned licence count
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public CHSECompnayCourseListing GetCHSECompnayCourseListing(CompnayCourseListingSearch searchCriteria)
        {
            try
            {
                CHSECompnayCourseListing moduleList = new CHSECompnayCourseListing();
                List<CHSECompnayCourse> moduleInfoList = new List<CHSECompnayCourse>();

                moduleList.TotalAssignedLicences = 0;
                moduleList.TotalConsumedLicences = 0;

                using (var context = new superadmindbEntities())
                {
                    var moduleListData = context.lms_superadmin_get_company_course_licences(searchCriteria.OrgUID, searchCriteria.ModuleName, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (moduleListData != null && moduleListData.Count > 0)
                    {
                        moduleList.TotalRecords = moduleListData.Count();
                        var data = moduleListData.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            CHSECompnayCourse mod = new CHSECompnayCourse();
                            mod.MainModuleId = Convert.ToInt64(item.MainModuleId);
                            mod.ModuleId = Convert.ToInt64(item.ModuleId);
                            mod.IsRaModule = Convert.ToBoolean(item.isRAModule);
                            mod.CourseType = Convert.ToBoolean(item.isRAModule) ? "Risk Assessment" : "Course";
                            mod.ModuleName = item.strCourse;
                            mod.AssignedLicenses = Convert.ToInt32(item.AssignedLicenses);
                            mod.ConsumedLicenses = Convert.ToInt32(item.ConsumedLicenses);
                            mod.AssignedStatus = Convert.ToInt32(item.assignedCourse);
                            moduleInfoList.Add(mod);

                            moduleList.TotalAssignedLicences = moduleList.TotalAssignedLicences + mod.AssignedLicenses;
                            moduleList.TotalConsumedLicences = moduleList.TotalConsumedLicences + mod.ConsumedLicenses;
                        }
                    }
                }
                moduleList.ModuleList = moduleInfoList;
                return moduleList;
            }
            catch (Exception)
            {
                throw;
            }
        }


        // download classroom report
        public List<DownloadModuleLicnceReport> DownloadAssignedLicenceReport(CompnayCourseListingSearch searchCriteria)
        {
            try
            {
                List<DownloadModuleLicnceReport> licenceRecord = new List<DownloadModuleLicnceReport>();

                using (var context = new superadmindbEntities())
                {
                    var moduleListData = context.lms_superadmin_get_company_course_licences(searchCriteria.OrgUID, searchCriteria.ModuleName, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (moduleListData != null && moduleListData.Count > 0)
                    {
                        foreach (var item in moduleListData)
                        {
                            DownloadModuleLicnceReport reportItem = new DownloadModuleLicnceReport();
                            reportItem.Type = Convert.ToBoolean(item.isRAModule) ? "Risk Assessment" : "Course";
                            reportItem.Course = item.strCourse;
                            reportItem.AssignedLicenses = Convert.ToInt32(item.AssignedLicenses);
                            reportItem.ConsumedLicenses = Convert.ToInt32(item.ConsumedLicenses);
                            licenceRecord.Add(reportItem);
                        }
                    }
                }
                return licenceRecord;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Assing course to company
        /// </summary>
        /// <param name="CourseLicence"></param>
        /// <returns></returns>
        public Int64 AssignAndGetCourseIdForCompany(CHSEAssignCourseLicence CourseLicence)
        {
            Int64 organisationCourseId = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("intRetCourseId", typeof(long));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_assign_new_course_to_company(CourseLicence.OrgUID, CourseLicence.CourseId, retVal);
                    organisationCourseId = Convert.ToInt32(retVal.Value);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return organisationCourseId;
        }

        /// <summary>
        /// Assing course licence to company
        /// </summary>
        /// <param name="CourseLicence"></param>
        /// <returns></returns>
        public int AssignCourseLicence(CHSEAssignCourseLicence CourseLicence)
        {
            int success = 0;
            try
            {
                Int64 compnayCourseId = AssignAndGetCourseIdForCompany(CourseLicence);

                ObjectParameter retVal = new ObjectParameter("retval", typeof(long));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_assign_company_new_licence(CourseLicence.OrgUID, compnayCourseId, CourseLicence.NewLicenceCount, CourseLicence.RemoveLicenceCount, retVal);
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
        /// Reassign / Revoke module access for a company
        /// </summary>
        /// <param name="courseId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public int UpdateCompanyModuleActiveStatus(Int64 courseId, bool status)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_update_CompanyModuleAccess(courseId, status, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Refresh course assignment
        /// </summary>
        /// <param name="courseId"></param>
        /// <returns></returns>
        public int RefreshCourseAssignment(Int64 courseId)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_refresh_module_assignment(courseId, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
