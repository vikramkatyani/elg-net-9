using ELG.Web.Helper;
using System;
using System.Linq;
using ELG.Model.OrgAdmin;
using ELG.DAL.OrgAdminDAL;
using Microsoft.AspNetCore.Mvc;

namespace ELG.Web.Controllers
{
    public class ClassroomController : Controller
    {
        // GET: Document
        public ActionResult Classroom()
        {
            return View("Classroom");
        }

        // get report on applied filter
        [HttpPost]
        public ActionResult LoadClassroomData(ClassroomFilter searchCriteria)
        {
            try
            {
                var classroomRep = new ClassroomRep();

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

                ActiveClassroomList classroomList = classroomRep.GetClassroomList(searchCriteria);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = classroomList.TotalClassroom, recordsTotal = classroomList.TotalClassroom, data = classroomList.ClassroomList });

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("Classroom");
            }
        }

        // function to add new document
        [HttpPost]
        public ActionResult CreateClassroom(Classroom classroom)
        {
            try
            {
                if (String.IsNullOrEmpty(classroom.ClassroomName))
                    return View();

                classroom.Creator = SessionHelper.UserId;
                classroom.OrganisationId = SessionHelper.CompanyId;
                var classroomRep = new ClassroomRep();
                int result = classroomRep.CreateNewClassroom(classroom);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View();
            }
        }


        // to archive class
        [HttpPost]
        public ActionResult ArchiveClassroom(Classroom classroom)
        {
            try
            {
                var classroomRep = new ClassroomRep();
                classroom.Creator = SessionHelper.UserId;
                classroom.OrganisationId = SessionHelper.CompanyId;
                int result = classroomRep.ArchiveClassroom(classroom);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        // to get learner info
        public ActionResult GetClassInfo(Classroom classroom)
        {
            try
            {
                var classRep = new ClassroomRep();
                Classroom classroomInfo = classRep.GetClassroomInfo(classroom.ClassroomId);

                return Json(new { classroomInfo });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View();
            }
        }

        // to update class info
        [HttpPost]
        public ActionResult UpdateClassroom(Classroom classroom)
        {
            try
            {
                var classroomRep = new ClassroomRep();
                classroom.Creator = SessionHelper.UserId;
                classroom.OrganisationId = SessionHelper.CompanyId;
                int result = classroomRep.UpdateClassroom(classroom);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }


        // GET: Document
        public ActionResult PendingRequests()
        {
            return View("PendingRequests");
        }

        // get report on applied filter
        [HttpPost]
        public ActionResult LoadPendingRequestData(ClassroomProgressFilter searchCriteria)
        {
            try
            {
                var classroomRep = new ClassroomRep();

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

                ClassroomProgressReport classroomList = classroomRep.GetPendingRequests(searchCriteria);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = classroomList.TotalRecords, recordsTotal = classroomList.TotalRecords, data = classroomList.ClassroomRecords });

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("PendingRequests");
            }
        }

        // to accept request
        [HttpPost]
        public ActionResult AcceptClassroomRequest(ClassroomProgressItem classroom)
        {
            try
            {
                var classroomRep = new ClassroomRep();
                classroom.MarkedBy = SessionHelper.UserId;
                int result = classroomRep.AcceptClassroomRequest(classroom);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        // to accept request
        [HttpPost]
        public ActionResult RejectClassroomRequest(ClassroomProgressItem classroom)
        {
            try
            {
                var classroomRep = new ClassroomRep();
                int result = classroomRep.RejectClassroomRequest(classroom);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }



        // GET: Document
        public ActionResult ClassAttendance()
        {
            return View("ClassAttendance");
        }

        // get report on applied filter
        [HttpPost]
        public ActionResult LoadClassAttendanceData(ClassroomProgressFilter searchCriteria)
        {
            try
            {
                var classroomRep = new ClassroomRep();

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

                ClassroomProgressReport classroomList = classroomRep.GetAcceptedRequests(searchCriteria);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = classroomList.TotalRecords, recordsTotal = classroomList.TotalRecords, data = classroomList.ClassroomRecords });

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("ClassAttendance");
            }
        }
        // to accept request
        [HttpPost]
        public ActionResult MarkClassAttended(ClassroomProgressItem classroom)
        {
            try
            {
                var classroomRep = new ClassroomRep();
                classroom.MarkedBy = SessionHelper.UserId;
                int result = classroomRep.MarkClassAttended(classroom);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }
    }
}