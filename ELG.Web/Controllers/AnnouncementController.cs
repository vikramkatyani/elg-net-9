using ELG.Web.Helper;
using System;
using System.Linq;
using ELG.Model.OrgAdmin;
using ELG.DAL.OrgAdminDAL;
using Microsoft.AspNetCore.Mvc;

namespace ELG.Web.Controllers
{
    public class AnnouncementController : Controller
    {
        // GET: Document
        public ActionResult Announcements()
        {
            return View("Announcements");
        }

        // get report on applied filter
        [HttpPost]
        public ActionResult LoadAnnouncementData(AnnouncementFilter searchCriteria)
        {
            try
            {
                var annRep = new AnnouncementRep();

                searchCriteria.Company = Convert.ToInt64(SessionHelper.CompanyId);
                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                }

                searchCriteria.Draw = Request.Form["draw"].FirstOrDefault();
                searchCriteria.Start = Request.Form["start"].FirstOrDefault();
                searchCriteria.Length = Request.Form["length"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                AnnouncementList annList = annRep.GetAnnouncementList(searchCriteria);
                return new Microsoft.AspNetCore.Mvc.JsonResult(new { draw = searchCriteria.Draw, recordsFiltered = annList.TotalAnnouncements, recordsTotal = annList.TotalAnnouncements, data = annList.AnnouncementRecords });

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("Announcements");
            }
        }

        // function to add new announcement
        [HttpPost]
        public ActionResult CreateAnnouncement(Announcement announcement)
        {
            try
            {
                if (String.IsNullOrEmpty(announcement.Title) || String.IsNullOrEmpty(announcement.Summary))
                    return View();

                announcement.AnnouncementOrganisation = SessionHelper.CompanyId;
                var annRep = new AnnouncementRep();
                int result = annRep.CreateNewAnnouncement(announcement);

                return Content(System.Text.Json.JsonSerializer.Serialize(new { success = result }), "application/json");
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View();
            }
        }

        // to archive announcement
        [HttpPost]
        public ActionResult ArchiveAnnouncement(Announcement announcement)
        {
            try
            {
                var annRep = new AnnouncementRep();
                announcement.AnnouncementCancelled = 1;
                int result = annRep.ArchiveAnnouncement(announcement);
                return Content(System.Text.Json.JsonSerializer.Serialize(new { success = result }), "application/json");
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Content(System.Text.Json.JsonSerializer.Serialize(new { success = -1 }), "application/json");
            }
        }

        //// to get announcement info
        //public ActionResult GetAnnouncementInfo(Announcement announcement)
        //{
        //    try
        //    {
        //        var classRep = new ClassroomRep();
        //        Classroom classroomInfo = classRep.GetClassroomInfo(classroom.ClassroomId);

        //        return Json(new { classroomInfo });
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error(ex.Message, ex);
        //        return View();
        //    }
        //}

        // to update announcement info
        [HttpPost]
        public ActionResult UpdateAnnouncement(Announcement announcement)
        {
            try
            {
                var annRep = new AnnouncementRep();
                int result = annRep.UpdateAnnouncement(announcement);
                return Content(System.Text.Json.JsonSerializer.Serialize(new { success = result }), "application/json");
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Content(System.Text.Json.JsonSerializer.Serialize(new { success = -1 }), "application/json");
            }
        }
    }
}