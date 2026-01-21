using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using ELG.Model.OrgAdmin;
using ELG.DAL.DBEntity;
using ELG.DAL.DBEntitySA;
using System.Data.Entity.Core.Objects;

namespace ELG.DAL.Utilities
{
    public class CommonMethods
    {
        private static Random random = new Random();

        /// <summary>
        /// Create Random Password
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GetRandomPassword(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_.?/!@#$%";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Encode Password
        /// </summary>
        /// <param name="value"></param>
        /// <param name="passKey"></param>
        /// <returns></returns>
        public static string EncodePassword(string value, string passKey)
        {
            using (var sha1 = new SHA1Managed())
            {
                return BitConverter.ToString(sha1.ComputeHash(Encoding.UTF8.GetBytes(value + passKey))).Replace("-", "");
            }
        }

        /// <summary>
        /// Create new location for a company
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public Int64 CreateNewLocation(Int64 Organisation, string LocationName)
        {
            Int64 locationId = 0;
            try
            {
                ObjectParameter newLocId = new ObjectParameter("newLocId", typeof(long));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_create_location(Organisation, LocationName, newLocId);
                    locationId = Convert.ToInt32(newLocId.Value);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return locationId;
        }

        /// <summary>
        /// Create new location for a company
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public Int64 CreateNewLocation(String Organisation, string LocationName)
        {
            Int64 locationId = 0;
            try
            {
                ObjectParameter newLocId = new ObjectParameter("newLocId", typeof(long));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_create_location(Organisation, LocationName, newLocId);
                    locationId = Convert.ToInt32(newLocId.Value);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return locationId;
        }

        /// <summary>
        /// Create new department and map with location
        /// </summary>
        /// <param name="department"></param>
        /// <returns></returns>
        public Int64 CreateNewLocationDepartment(Int64 Organiastion, Int64 LocationId, string DepartmentName)
        {
            Int64 departmentId = 0;
            try
            {
                ObjectParameter newDepId = new ObjectParameter("newDepId", typeof(long));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_create_location_department(Organiastion, LocationId, DepartmentName, newDepId);
                    departmentId = Convert.ToInt32(newDepId.Value);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return departmentId;
        }

        /// <summary>
        /// Create new department and map with location
        /// </summary>
        /// <param name="department"></param>
        /// <returns></returns>
        public Int64 CreateNewLocationDepartment(String Organiastion, Int64 LocationId, string DepartmentName)
        {
            Int64 departmentId = 0;
            try
            {
                ObjectParameter newDepId = new ObjectParameter("newDepId", typeof(long));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_create_location_department(Organiastion, LocationId, DepartmentName, newDepId);
                    departmentId = Convert.ToInt32(newDepId.Value);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return departmentId;
        }
        /// <summary>
        /// Get list of all locations in the company as per admin role
        /// </summary>
        /// <param name="company"></param>
        /// <returns></returns>
        public List<OrganisationLocation> GetLocationsForCompany(int adminRole, Int64 adminUserId, Int64 company )
        {
            try
            {
                List<OrganisationLocation> locationList = new List<OrganisationLocation>();
                using (var context = new lmsdbEntities())
                {
                    var locations = context.lms_admin_getAllLocations_perRole(adminRole, adminUserId, company).ToList();
                    if (locations != null && locations.Count > 0)
                    {
                        foreach (var location in locations)
                        {
                            OrganisationLocation loc = new OrganisationLocation();
                            loc.LocationId = location.intLocationID;
                            loc.LocationName = location.strLocation;
                            locationList.Add(loc);
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
        /// Get list of all locations in the company
        /// </summary>
        /// <param name="company"></param>
        /// <returns></returns>
        public List<OrganisationLocation> GetLocationsForCompany(Int64 company)
        {
            try
            {
                List<OrganisationLocation> locationList = new List<OrganisationLocation>();
                using (var context = new lmsdbEntities())
                {
                    var locations = context.lms_admin_getAllLocations(company).ToList();
                    if (locations != null && locations.Count > 0)
                    {
                        foreach (var location in locations)
                        {
                            OrganisationLocation loc = new OrganisationLocation();
                            loc.LocationId = location.intLocationID;
                            loc.LocationName = location.strLocation;
                            locationList.Add(loc);
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
        /// Get list of all locations in the company
        /// </summary>
        /// <param name="company"></param>
        /// <returns></returns>
        public List<ELG.Model.SuperAdmin.OrganisationLocation> GetLocationsForCompany(String companyUID)
        {
            try
            {
                List<ELG.Model.SuperAdmin.OrganisationLocation> locationList = new List<ELG.Model.SuperAdmin.OrganisationLocation>();
                using (var context = new superadmindbEntities())
                {
                    var locations = context.lms_superadmin_getAllCompnayLocations(companyUID,"").ToList();
                    if (locations != null && locations.Count > 0)
                    {
                        foreach (var location in locations)
                        {
                            ELG.Model.SuperAdmin.OrganisationLocation loc = new ELG.Model.SuperAdmin.OrganisationLocation();
                            loc.LocationId = location.intLocationID;
                            loc.LocationName = location.strLocation;
                            locationList.Add(loc);
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
        /// to get list of all departments for a location in the company
        /// </summary>
        /// <param name="company"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public List<OrganisationDepartment> GetDepartmentsForLocation(int adminRole, Int64 adminUserId, Int64 location)
        {
            try
            {
                List<OrganisationDepartment> departmentList = new List<OrganisationDepartment>();
                using (var context = new lmsdbEntities())
                {
                    var departments = context.lms_admin_getAllDepartmentsForLocation_perRole(adminRole, adminUserId, location).ToList();
                    if (departments != null && departments.Count > 0)
                    {
                        foreach (var department in departments)
                        {
                            OrganisationDepartment dep = new OrganisationDepartment();
                            dep.DepartmentId = Convert.ToInt64(department.intDepartmentID);
                            dep.DepartmentName = department.strDepartment;
                            departmentList.Add(dep);
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
        /// to get list of all departments for a location in the company for super admin
        /// </summary>
        /// <param name="company"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public List<ELG.Model.SuperAdmin.OrganisationDepartment> GetDepartmentsForLocation(Int64 location)
        {
            try
            {
                List<ELG.Model.SuperAdmin.OrganisationDepartment> departmentList = new List<ELG.Model.SuperAdmin.OrganisationDepartment>();
                using (var context = new superadmindbEntities())
                {
                    var departments = context.lms_superadmin_get_AllDepartmentsForLocation(location).ToList();
                    if (departments != null && departments.Count > 0)
                    {
                        foreach (var department in departments)
                        {
                            ELG.Model.SuperAdmin.OrganisationDepartment dep = new ELG.Model.SuperAdmin.OrganisationDepartment();
                            dep.DepartmentId = Convert.ToInt64(department.intDepartmentID);
                            dep.DepartmentName = department.strDepartment;
                            departmentList.Add(dep);
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
        /// to get list of all departments for a location in the company
        /// </summary>
        /// <param name="company"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public List<OrganisationDepartment> GetDepartmentsForLocation_Admin(Int64 location)
        {
            try
            {
                List<OrganisationDepartment> departmentList = new List<OrganisationDepartment>();
                using (var context = new lmsdbEntities())
                {
                    var departments = context.lms_admin_getAllDepartmentsForLocation(location).ToList();
                    if (departments != null && departments.Count > 0)
                    {
                        foreach (var department in departments)
                        {
                            OrganisationDepartment dep = new OrganisationDepartment();
                            dep.DepartmentId = Convert.ToInt64(department.intDepartmentID);
                            dep.DepartmentName = department.strDepartment;
                            departmentList.Add(dep);
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
        /// to get list of all departments for a location in the company
        /// </summary>
        /// <param name="company"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public List<ELG.Model.SuperAdmin.OrganisationDepartment> GetDepartmentsForLocation(Int64 location, string orgUID)
        {
            try
            {
                List<ELG.Model.SuperAdmin.OrganisationDepartment> departmentList = new List<ELG.Model.SuperAdmin.OrganisationDepartment>();
                using (var context = new superadmindbEntities())
                {
                    var departments = context.lms_superadmin_get_AllDepartmentsForLocation(location).ToList();
                    if (departments != null && departments.Count > 0)
                    {
                        foreach (var department in departments)
                        {
                            ELG.Model.SuperAdmin.OrganisationDepartment dep = new ELG.Model.SuperAdmin.OrganisationDepartment();
                            dep.DepartmentId = Convert.ToInt64(department.intDepartmentID);
                            dep.DepartmentName = department.strDepartment;
                            departmentList.Add(dep);
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
        /// to get list of all departments for a location in the company
        /// </summary>
        /// <param name="company"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public List<OrganisationDepartment> GetDepartmentsForOrganisation(Int64 companyId)
        {
            try
            {
                List<OrganisationDepartment> departmentList = new List<OrganisationDepartment>();
                using (var context = new lmsdbEntities())
                {
                    var departments = context.lms_admin_getAllDepartments(companyId).ToList();
                    if (departments != null && departments.Count > 0)
                    {
                        foreach (var department in departments)
                        {
                            OrganisationDepartment dep = new OrganisationDepartment();
                            dep.DepartmentId = Convert.ToInt64(department.intDepartmentID);
                            dep.DepartmentName = department.strDepartment;
                            departmentList.Add(dep);
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
        /// to get list of all courses for a company
        /// </summary>
        /// <param name="company"></param>
        /// <returns></returns>
        public List<OrganisationCourse> GetCoursesForOrganisation(Int64 company)
        {
            try
            {
                List<OrganisationCourse> courseList = new List<OrganisationCourse>();
                using (var context = new lmsdbEntities())
                {
                    var courses = context.lms_admin_getAllCourses(company).ToList();
                    if (courses != null && courses.Count > 0)
                    {
                        foreach (var course in courses)
                        {
                            OrganisationCourse c = new OrganisationCourse();
                            c.CourseId = Convert.ToInt64(course.intCourseID);
                            c.CourseName = course.strCourse;
                            courseList.Add(c);
                        }
                    }
                }
                return courseList;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public List<CourseSubModule> GetCourseSubModules(Int64 courseID)
        {
            try
            {
                List<CourseSubModule> subModuleList = new List<CourseSubModule>();
                using (var context = new lmsdbEntities())
                {
                    var subModules = context.lms_admin_get_course_submodules(courseID).ToList();
                    if (subModules != null && subModules.Count > 0)
                    {
                        foreach (var subModule in subModules)
                        {
                            CourseSubModule c = new CourseSubModule();
                            c.CourseId = Convert.ToInt64(subModule.courseId);
                            c.SubModuleId = Convert.ToInt64(subModule.smid);
                            c.SubModuleName = subModule.name;
                            subModuleList.Add(c);
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
        public List<OrganisationCourse> GetWidgetCoursesForOrganisation(Int64 company)
        {
            try
            {
                List<OrganisationCourse> courseList = new List<OrganisationCourse>();
                using (var context = new lmsdbEntities())
                {
                    var courses = context.lms_admin_get_org_WidgetCourses(company).ToList();
                    if (courses != null && courses.Count > 0)
                    {
                        foreach (var course in courses)
                        {
                            OrganisationCourse c = new OrganisationCourse();
                            c.CourseId = Convert.ToInt64(course.courseId);
                            c.CourseName = course.courseName;
                            courseList.Add(c);
                        }
                    }
                }
                return courseList;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public List<OrganisationCourse> GetAllCoursesOfCHSE()
        {
            try
            {
                List<OrganisationCourse> courseList = new List<OrganisationCourse>();
                using (var context = new superadmindbEntities())
                {
                    var courses = context.lms_superadmin_get_AllCourses().ToList();
                    if (courses != null && courses.Count > 0)
                    {
                        foreach (var course in courses)
                        {
                            OrganisationCourse c = new OrganisationCourse();
                            c.CourseId = Convert.ToInt64(course.intCourseID);
                            c.CourseName = course.strCourse;
                            courseList.Add(c);
                        }
                    }
                }
                return courseList;
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// to get list of all risk assessments for a company
        /// </summary>
        /// <param name="company"></param>
        /// <returns></returns>
        public List<OrganisationCourse> GetRiskAssessmnetsForOrganisation(Int64 company)
        {
            try
            {
                List<OrganisationCourse> courseList = new List<OrganisationCourse>();
                using (var context = new lmsdbEntities())
                {
                    var courses = context.lms_admin_getAllRiskAssessments(company).ToList();
                    if (courses != null && courses.Count > 0)
                    {
                        foreach (var course in courses)
                        {
                            OrganisationCourse c = new OrganisationCourse();
                            c.CourseId = Convert.ToInt64(course.intCourseID);
                            c.CourseName = course.strCourse;
                            courseList.Add(c);
                        }
                    }
                }
                return courseList;
            }
            catch (Exception)
            {

                throw;
            }
        }


        /// <summary>
        /// Get list of all risk assessments in the system
        /// </summary>
        /// <returns></returns>
        public List<OrganisationCourse> GetAllRiskAssessmnetsOfCHSE()
        {
            try
            {
                List<OrganisationCourse> courseList = new List<OrganisationCourse>();
                using (var context = new superadmindbEntities())
                {
                    var courses = context.lms_superadmin_get_AllRiskAssessments().ToList();
                    if (courses != null && courses.Count > 0)
                    {
                        foreach (var course in courses)
                        {
                            OrganisationCourse c = new OrganisationCourse();
                            c.CourseId = Convert.ToInt64(course.intCourseID);
                            c.CourseName = course.strCourse;
                            courseList.Add(c);
                        }
                    }
                }
                return courseList;
            }
            catch (Exception)
            {

                throw;
            }
        }
        /// <summary>
        /// get list of all classroom for an organisation
        /// </summary>
        /// <param name="company"></param>
        /// <returns></returns>
        public List<OrganisationCourse> GetClassroomForOrganisation(Int64 company)
        {
            try
            {
                List<OrganisationCourse> courseList = new List<OrganisationCourse>();
                using (var context = new lmsdbEntities())
                {
                    var courses = context.lms_admin_getAllClassroom(company).ToList();
                    if (courses != null && courses.Count > 0)
                    {
                        foreach (var course in courses)
                        {
                            OrganisationCourse c = new OrganisationCourse();
                            c.CourseId = Convert.ToInt64(course.intOCourseID);
                            c.CourseName = course.strTitle;
                            courseList.Add(c);
                        }
                    }
                }
                return courseList;
            }
            catch (Exception)
            {

                throw;
            }
        }



        /// <summary>
        /// Get details of company trainer
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public CompanyTrainerDetails GetCompanyTrainerDetails(Int64 companyId)
        {
            try
            {
                CompanyTrainerDetails trainer = new CompanyTrainerDetails();
                using (var context = new lmsdbEntities())
                {
                    var trainerInfo = context.lms_admin_getTrainingManagerDetails(companyId).FirstOrDefault();
                    if (trainerInfo != null)
                    {
                        trainer.Company = trainerInfo.strOrganisation;
                        trainer.Trainer = trainerInfo.strTrainerFirstName + " " + trainerInfo.strTrainerSurname;
                        trainer.Email = trainerInfo.strTrainerEmail;
                        trainer.Phone = trainerInfo.strTrainerPhNo;
                        trainer.Extension = trainerInfo.strTrainerExt;
                    }
                }
                return trainer;
            }
            catch (Exception)
            {

                throw;
            }

        }

        /// <summary>
        /// get list of all documents for an organisation
        /// </summary>
        /// <param name="company"></param>
        /// <returns></returns>
        public List<Document> DocumentsForOrganisation(Int64 company)
        {
            try
            {
                List<Document> documentList = new List<Document>();
                using (var context = new lmsdbEntities())
                {
                    var docList = context.lms_admin_getAllDocuments(null, 0, 0, null, null, company, 1, 1, 1).ToList();
                    if (docList != null && docList.Count > 0)
                    {
                        foreach (var item in docList)
                        {
                            Document doc = new Document();
                            doc.DocumentID = item.docId;
                            doc.DocumentName = item.docName;

                            documentList.Add(doc);
                        }
                    }
                }
                return documentList;
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// get list of all document categories for an organisation
        /// </summary>
        /// <param name="company"></param>
        /// <returns></returns>
        public List<DocumentCategory> DocumentCategoryForOrganisation(Int64 company, Int64 adminId, int adminRole)
        {
            try
            {
                List<DocumentCategory> catList = new List<DocumentCategory>();
                using (var context = new lmsdbEntities())
                {
                    var category = context.lms_admin_get_DocumentCategoryList("", "TX_NAME", "asc", company, adminId, adminRole).ToList();
                    if (category != null && category.Count > 0)
                    {
                        foreach (var cat in category)
                        {
                            DocumentCategory c = new DocumentCategory();
                            c.CategoryId = Convert.ToInt64(cat.PK_DOCUMENTID);
                            c.CategoryName = cat.TX_NAME;
                            catList.Add(c);
                        }
                    }
                }
                return catList;
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
        public ELG.Model.SuperAdmin.CHSERiskAssessmentGroupListing GetMasterRiskAssessmentGroupList(Int64 courseID)
        {
            try
            {
                ELG.Model.SuperAdmin.CHSERiskAssessmentGroupListing groupList = new ELG.Model.SuperAdmin.CHSERiskAssessmentGroupListing();
                List<ELG.Model.SuperAdmin.CHSERiskAssessmentGroup> groupInfoList = new List<ELG.Model.SuperAdmin.CHSERiskAssessmentGroup>();

                using (var context = new superadmindbEntities())
                {
                    var gList = context.lms_superadmin_get_GroupsOfRA(courseID,"").ToList();
                    if (gList != null && gList.Count > 0)
                    {
                        groupList.TotalRecords = gList.Count();

                        foreach (var item in gList)
                        {
                            ELG.Model.SuperAdmin.CHSERiskAssessmentGroup group = new ELG.Model.SuperAdmin.CHSERiskAssessmentGroup();
                            group.RAId = item.intRACourseID;
                            group.GroupId = item.intRAGroupID;
                            group.GroupName = item.strRAGroup;
                            groupInfoList.Add(group);
                        }
                    }
                }
                groupList.RAGroupList = groupInfoList;
                return groupList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get list of all resellers in the system
        /// </summary>
        /// <returns></returns>
        public List<ELG.Model.SuperAdmin.Organisation> GetAllResellersOfCHSE()
        {
            try
            {
                List<ELG.Model.SuperAdmin.Organisation> resellerList = new List<ELG.Model.SuperAdmin.Organisation>();
                using (var context = new superadmindbEntities())
                {
                    var resellers = context.lms_superadmin_get_AllResellers().ToList();
                    if (resellers != null && resellers.Count > 0)
                    {
                        foreach (var reseller in resellers)
                        {
                            ELG.Model.SuperAdmin.Organisation r = new ELG.Model.SuperAdmin.Organisation();
                            r.OrganisationId = Convert.ToInt64(reseller.resellerId);
                            r.OrganisationName = reseller.resellerName;
                            resellerList.Add(r);
                        }
                    }
                }
                return resellerList;
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// to get list of all sub categories for a category
        /// </summary>
        /// <param name="CategoryID"></param>
        /// <returns></returns>
        public List<DocumentSubCategory> GetSubCategoriesForCategory(Int64 CategoryID)
        {
            try
            {
                List<DocumentSubCategory> subCategoryList = new List<DocumentSubCategory>();
                using (var context = new lmsdbEntities())
                {
                    var subcategories = context.lms_admin_get_SubCategories_ForCategory(CategoryID).ToList();
                    if (subcategories != null && subcategories.Count > 0)
                    {
                        foreach (var subCategory in subcategories)
                        {
                            DocumentSubCategory scat = new DocumentSubCategory();
                            scat.SubCategoryId = Convert.ToInt64(subCategory.intSubCategoryId);
                            scat.SubCategoryName = subCategory.strSubCategory;
                            subCategoryList.Add(scat);
                        }
                    }
                }
                return subCategoryList;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
