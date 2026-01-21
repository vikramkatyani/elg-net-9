using ELG.Web.Helper;
using ELG.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using ELG.Model.OrgAdmin;
using ELG.DAL.OrgAdminDAL;
using ELG.DAL.Utilities;
using System.Data;
using Microsoft.AspNetCore.Mvc;

namespace ELG.Web.Controllers
{
    public class CommonController : Controller
    {
        // get Compnay trainer info
        public ActionResult GetCompanyTrainerDetails()
        {
            CompanyTrainerDetails trainer = new CompanyTrainerDetails();
            try
            {
                var common = new CommonMethods();
                trainer = common.GetCompanyTrainerDetails(Convert.ToInt64(SessionHelper.CompanyId));
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
            return new ContentResult {
                Content = System.Text.Json.JsonSerializer.Serialize(new { trainer }),
                ContentType = "application/json"
            };
        }
        // get list of location in the organisation
        public ActionResult LocationList()
        {
            try
            {
                var locFilter = new CommonMethods();
                List<OrganisationLocation> locationList = new List<OrganisationLocation>();
                locationList = locFilter.GetLocationsForCompany(SessionHelper.UserRole, SessionHelper.UserId, SessionHelper.CompanyId);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { locationList }),
                    ContentType = "application/json"
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { locations = "" }),
                    ContentType = "application/json"
                };
            }
        }

        //Get list of all departments for a location
        public ActionResult DepartmentList(Int64 location)
        {
            try
            {
                var locFilter = new CommonMethods();
                List<OrganisationDepartment> departmentList = new List<OrganisationDepartment>();
                departmentList = locFilter.GetDepartmentsForLocation(SessionHelper.UserRole, SessionHelper.UserId, location);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { departmentList }),
                    ContentType = "application/json"
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { departments = "" }),
                    ContentType = "application/json"
                };
            }
        }

        //Get list of all departments in an organisation
        public ActionResult CompanyDepartmentList()
        {
            try
            {
                var locFilter = new CommonMethods();
                List<OrganisationDepartment> departmentList = new List<OrganisationDepartment>();
                departmentList = locFilter.GetDepartmentsForOrganisation(SessionHelper.CompanyId);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { departmentList }),
                    ContentType = "application/json"
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { departments = "" }),
                    ContentType = "application/json"
                };
            }
        }

        //Get list of all courses for a company
        public ActionResult CourseList()
        {
            try
            {
                var orgFilter = new CommonMethods();
                List<OrganisationCourse> courseList = new List<OrganisationCourse>();
                courseList = orgFilter.GetCoursesForOrganisation(Convert.ToInt64(SessionHelper.CompanyId));
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { courseList }),
                    ContentType = "application/json"
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { courses = "" }),
                    ContentType = "application/json"
                };
            }
        }
        //Get list of all submodules for a course
        public ActionResult SubModuleList(Int64 courseID)
        {
            try
            {
                var subModule = new CommonMethods();
                List<CourseSubModule> subModuleist = new List<CourseSubModule>();
                subModuleist = subModule.GetCourseSubModules(courseID);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { subModuleist }),
                    ContentType = "application/json"
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { departments = "" }),
                    ContentType = "application/json"
                };
            }
        }

        //get all widget courses 
        public ActionResult WidgetCourseList()
        {
            try
            {
                var orgFilter = new CommonMethods();
                List<OrganisationCourse> courseList = new List<OrganisationCourse>();
                courseList = orgFilter.GetWidgetCoursesForOrganisation(Convert.ToInt64(SessionHelper.CompanyId));
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { courseList }),
                    ContentType = "application/json"
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { courses = "" }),
                    ContentType = "application/json"
                };
            }
        }


        //Get list of all risk assessment for a company
        public ActionResult RiskAssessmentList()
        {
            try
            {
                var orgFilter = new CommonMethods();
                List<OrganisationCourse> courseList = new List<OrganisationCourse>();
                courseList = orgFilter.GetRiskAssessmnetsForOrganisation(Convert.ToInt64(SessionHelper.CompanyId));
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { courseList }),
                    ContentType = "application/json"
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { courses = "" }),
                    ContentType = "application/json"
                };
            }
        }

        //Get list of all classrooms for an organisation
        public ActionResult ClassroomList()
        {
            try
            {
                var orgFilter = new CommonMethods();
                List<OrganisationCourse> courseList = new List<OrganisationCourse>();
                courseList = orgFilter.GetClassroomForOrganisation(Convert.ToInt64(SessionHelper.CompanyId));
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { courseList }),
                    ContentType = "application/json"
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { courses = "" }),
                    ContentType = "application/json"
                };
            }
        }

        //Get list of all Documents for an organisation
        public ActionResult DocumentList()
        {
            try
            {
                var docFilter = new CommonMethods();
                List<Document> docList = new List<Document>();
                docList = docFilter.DocumentsForOrganisation(Convert.ToInt64(SessionHelper.CompanyId));
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { docList }),
                    ContentType = "application/json"
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { docList = "" }),
                    ContentType = "application/json"
                };
            }
        }


        //Get list of all Document Categories for an organisation
        public ActionResult DocumentCategoryList()
        {
            try
            {
                var catFilter = new CommonMethods();
                List<DocumentCategory> catList = new List<DocumentCategory>();
                catList = catFilter.DocumentCategoryForOrganisation(SessionHelper.CompanyId, SessionHelper.UserId, SessionHelper.UserRole);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { catList }),
                    ContentType = "application/json"
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { courses = "" }),
                    ContentType = "application/json"
                };
            }
        }
        //Get list of all Groups for a RA Course
        public ActionResult RAGroupList()
        {
            RiskAssessmentGroupList groupList = new RiskAssessmentGroupList();
            try
            {
                var groupFilter = new CommonMethods();
                groupList = groupFilter.GetRiskAssessmentGroupList(SessionHelper.CourseId);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
            return new ContentResult {
                Content = System.Text.Json.JsonSerializer.Serialize(new { groupList = groupList.RiskAssessmentGroups }),
                ContentType = "application/json"
            };
        }

        // get list of location and department map in the organisation
        public ActionResult LocationDepartmentMapList()
        {
            try
            {
                CompanyRep rep = new CompanyRep();
                List<CompanyStructuralComponent> map = new List<CompanyStructuralComponent>();
                map = rep.GetCompanyStructure(Convert.ToInt64(SessionHelper.CompanyId));
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { map }),
                    ContentType = "application/json"
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { map = "" }),
                    ContentType = "application/json"
                };
            }
        }

        //download company structure
        public ActionResult DownloadCompanyStructue()
        {
            List<CompanyStructuralComponent> structure = new List<CompanyStructuralComponent>();
            try
            {
                CompanyRep rep = new CompanyRep();
                structure = rep.GetCompanyStructure(Convert.ToInt64(SessionHelper.CompanyId));

                DataTable dtStruct = CommonHelper.ListToDataTable(structure);
                string[] columns = { SessionHelper.CompanySettings.strLocationDescription, SessionHelper.CompanySettings.strDepartmentDescription };
                string[] columns_toTake = { "LocationName", "DepartmentName" };
                byte[] filecontent = CommonHelper.ExportExcelWithHeader(dtStruct, "OrgStructure", false, columns, columns_toTake);
                return File(filecontent, CommonHelper.ExcelContentType, SessionHelper.CompanyName + "_structure.xlsx");
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { struc = "" }),
                    ContentType = "application/json"
                };
            }
        }

        //Get list of all departments for a location
        public ActionResult GetSubCategoriesForCategory([FromBody] CategoryRequest request)
        {
            try
            {
                var subCategoryFilter = new CommonMethods();
                List<DocumentSubCategory> subCategoryList = new List<DocumentSubCategory>();
                subCategoryList = subCategoryFilter.GetSubCategoriesForCategory(request?.CategoryID ?? 0);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { subCategoryList }),
                    ContentType = "application/json"
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { subCategoryList = "" }),
                    ContentType = "application/json"
                };
            }
        }
    }
}
