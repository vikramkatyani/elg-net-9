using ELG.Web.Helper;
using System;
using System.Linq;
using ELG.Model.OrgAdmin;
using ELG.DAL.OrgAdminDAL;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Files.Shares;
using Azure;
using Azure.Storage.Sas;
using Azure.Storage;
using System.Collections.Generic;
using ELG.DAL.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace ELG.Web.Controllers
{
    public class QueWidgetController : Controller
    {
        // GET: QueWidget
        public ActionResult Course()
        {
            return View();
        }
        // get report on applied filter
        [HttpPost]
        public ActionResult LoadWidgetCourseData(DataTableFilter searchCriteria)
        {
            try
            {
                var widgetRep = new WidgetRep();

                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminUserId = SessionHelper.UserId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                }

                searchCriteria.Draw = Request.Form["draw"].FirstOrDefault();
                searchCriteria.Start = Request.Form["start"].FirstOrDefault();
                searchCriteria.Length = Request.Form["length"].FirstOrDefault();
                //Find Order Column
                searchCriteria.SortCol = Request.Form[$"columns[{Request.Form["order[0][column]"].FirstOrDefault()}][name]"].FirstOrDefault();
                searchCriteria.SortColDir = Request.Form["order[0][dir]"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                WidgetCourseList courseList = widgetRep.GetWidgetCourses(searchCriteria);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = courseList.TotalCourses, recordsTotal = courseList.TotalCourses, data = courseList.CourseList });

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("Course");
            }
        }

        // function to add new course
        [HttpPost]
        public ActionResult CreateCourse(WidgetCourse course)
        {
            try
            {
                if (String.IsNullOrEmpty(course.CourseName))
                    return View("Course");

                course.CourseOrg = SessionHelper.CompanyId;
                var widgetRep = new WidgetRep();
                int result = widgetRep.CreateNewWidgetCourse(course);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("Course");
            }
        }


        public ActionResult CourseWidgets(string id)
        {
            return View();
        }
        // get report on applied filter
        [HttpPost]
        public ActionResult LoadCourseWidgetData(DataTableFilterWidgets searchCriteria)
        {
            try
            {
                var widgetRep = new WidgetRep();

                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminUserId = SessionHelper.UserId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                }

                searchCriteria.Draw = Request.Form["draw"].FirstOrDefault();
                searchCriteria.Start = Request.Form["start"].FirstOrDefault();
                searchCriteria.Length = Request.Form["length"].FirstOrDefault();
                //Find Order Column
                searchCriteria.SortCol = Request.Form["columns[" + Request.Form["order[0][column]"] + "][name]"].FirstOrDefault();
                searchCriteria.SortColDir = Request.Form["order[0][dir]"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                CourseWidgetList widgetList = widgetRep.GetCourseWidgets(searchCriteria);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = widgetList.TotalWidgets, recordsTotal = widgetList.TotalWidgets, data = widgetList.WidgetList });

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("Course");
            }
        }


        // function to add new widget
        [HttpPost]
        public ActionResult CreateWidget(CourseWidget widget)
        {
            try
            {
                if (String.IsNullOrEmpty(widget.QuesText))
                    return View("CourseWidgets");

                var widgetRep = new WidgetRep();
                int result = widgetRep.CreateNewCourseWidget(widget);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("CourseWidgets");
            }
        }
    }
}