using Microsoft.AspNetCore.Http;
using ELG.Web.Helper;
using ELG.DAL.OrgAdminDAL;
using ELG.Model.OrgAdmin;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ELG.Web.Controllers
{
    public class AccidentIncidentController : Controller
    {
        // GET: AccidentIncident
        public ActionResult List()
        {
            return View();
        }

        // get report on applied filter
        [HttpPost]
        public ActionResult LoadReportedIncidentData(DataTableFilter searchCriteria)
        {
            try
            {
                var accidentRep = new AccidentIncidentRep();

                searchCriteria.AdminUserId = Convert.ToInt64(SessionHelper.UserId);
                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                }

                searchCriteria.Draw = Request.Form["draw"].FirstOrDefault();
                searchCriteria.Start = Request.Form["start"].FirstOrDefault();
                searchCriteria.Length = Request.Form["length"].FirstOrDefault();
                //Find Order Column
                var orderColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
                searchCriteria.SortCol = Request.Form[$"columns[{orderColumnIndex}][name]"].FirstOrDefault();
                searchCriteria.SortColDir = Request.Form["order[0][dir]"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                ReportedIncidentsList incidentList = accidentRep.GetReportedIncidents(searchCriteria);

                return Json(new { draw = searchCriteria.Draw, recordsFiltered = incidentList.TotalIncidents, recordsTotal = incidentList.TotalIncidents, data = incidentList.IncidentList });

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("List");
            }
        }

        // GET: list of all question for a incident/accident
        [HttpPost]
        public ActionResult GetAccidentIncidentQuestions(Int64 aiID)
        {
            List<AccidentIncidentQuestion> quesList = new List<AccidentIncidentQuestion>();
            try
            {
                var accidentRep = new AccidentIncidentRep();
                quesList = accidentRep.GetAccidentIncidentQuestions(aiID);

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }

            return Json(new { quesList });
        }

        //save new accident/incident
        [HttpPost]
        public async Task<ActionResult> ReportNew(AccidentIncidentResponse ResponseDetails, string Response, IFormFile newImageFile)
        {
            try
            {
                var accidentRep = new AccidentIncidentRep();
                ResponseDetails.CreatorId = SessionHelper.UserId;
                ResponseDetails.Response = Response;

                int result = accidentRep.SaveAccidentIncident(ResponseDetails);

                //check if document upload is valid
                if (result > 0 && newImageFile != null && newImageFile.Length > 0)
                {
                    IncidentImage details = new IncidentImage();
                    details.ResponseId = result;
                    await UploadIncidentImage(newImageFile, details);
                }

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        // function to upload evidence
        [HttpPost]
        public async Task<ActionResult> UploadIncidentImage(IFormFile newDocFile, IncidentImage evidence)
        {
            int status = 0;
            try
            {
                IFormFile document = newDocFile;

                //validate file size (<= 10MB)  10*1024*1024 = 10485760
                if (document != null && document.Length > 10485760)
                {
                    return Json(new { success = "File too large", status });
                }

                var result = await AsyncUploadFile(document, evidence);

                evidence.ImagePath = result;

                var accidentRep = new AccidentIncidentRep();
                status = accidentRep.SaveIncidentImage(evidence);
                return Json(new { success = result, status });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = ex.Message, status });
            }

        }
        public static async Task<string> AsyncUploadFile(IFormFile newDocFile, IncidentImage evidence)

        {
            var connectionString = ELG.Web.Helper.CommonHelper.GetAppSettingValue("AZStorageConnectionString");

            // create a client with the connection
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(ELG.Web.Helper.CommonHelper.GetAppSettingValue("AZIncidentStorageContainer"));

            IFormFile document = newDocFile;
            string filename = "Incident_Image_" + evidence.ResponseId + Path.GetExtension(newDocFile.FileName);
            string filetype = (newDocFile.ContentType);

            BlobClient blobClient = containerClient.GetBlobClient(filename);

            var blobHttpHeader = new BlobHttpHeaders();

            blobHttpHeader.ContentType = filetype;

            using (var stream = newDocFile.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, blobHttpHeader);
            }

            return blobClient.Uri.AbsoluteUri;
        }
    }
}