using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using ELG.DAL.LearnerDAL;
using ELG.DAL.Utilities;
using ELG.Web.Helper;
using ELG.Model.Learner;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ELG.Web.Areas.Learner.Controllers
{
    public class DocManageRequest
    {
        public long DocumentId { get; set; }
        public string Status { get; set; }
    }
    [Area("Learner")]
    public class DocumentController : Controller
    {
        // GET: Learner/Document/List
        public ActionResult List()
        {
            ViewBag.Title = "Documents";
            return View();
        }

        // GET: Learner/Document/Preview
        public ActionResult Preview(int id)
        {
            try
            {
                var docRep = new DocumentRep();
                Document doc = docRep.GetDocumentDetails(id, SessionHelper.UserId);
                string ext = Path.GetExtension(doc.DocumentPath);
                bool previewAvailable = false;
                if (ext == ".pdf")
                    previewAvailable = true;

                // Generate SAS token for Azure Blob access
                string documentPathWithSas = GenerateBlobSasUrl(doc.DocumentPath);

                ViewBag.DocID = doc.DocumentID;
                ViewBag.DocName = doc.DocumentName;
                ViewBag.DocumentPath = documentPathWithSas;
                ViewBag.PreviewAvailable = previewAvailable;
                ViewBag.DocStatus = doc.DocumentStatus;
                return View("Preview");

            }
            catch (Exception ex)
            {
                ViewBag.DocPath = "";
                ViewBag.DocName = "";
                ViewBag.PreviewAvailable = false;
                Logger.Error(ex.Message, ex);
                return View("Preview");
            }
        }

        // POST: get report on applied filter
        [HttpPost]
        public ActionResult LoadDocumentData(DataTableDocFilter searchCriteria)
        {
            try
            {
                var docRep = new DocumentRep();

                searchCriteria.Learner = Convert.ToInt64(SessionHelper.UserId);
                searchCriteria.Organisation = Convert.ToInt64(SessionHelper.CompanyId);
                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                }

                Microsoft.Extensions.Primitives.StringValues drawValues;
                Request.Form.TryGetValue("draw", out drawValues);
                searchCriteria.Draw = drawValues.FirstOrDefault();
                Microsoft.Extensions.Primitives.StringValues startValues;
                Request.Form.TryGetValue("start", out startValues);
                searchCriteria.Start = startValues.FirstOrDefault();
                Microsoft.Extensions.Primitives.StringValues lengthValues;
                Request.Form.TryGetValue("length", out lengthValues);
                searchCriteria.Length = lengthValues.FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                DocumentList docList = docRep.GetDocuments(searchCriteria);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = docList.TotalDocuments, recordsTotal = docList.TotalDocuments, data = docList.DocList });

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { error = ex.Message });
            }
        }

        // POST: mark document read
        [HttpPost]
        public ActionResult MarkDocumentRead([FromBody] DocManageRequest request)
        {
            try
            {
                Int64 docid = request?.DocumentId ?? 0;
                var docRep = new DocumentRep();
                int result = docRep.MarkDocumentRead(SessionHelper.UserId, docid, SessionHelper.CompanyId);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        // POST: set document status
        [HttpPost]
        public ActionResult SetDocumentStatus([FromBody] DocManageRequest request)
        {
            try
            {
                Int64 docid = request?.DocumentId ?? 0;
                string status = request?.Status ?? "";
                var docRep = new DocumentRep();
                int result = docRep.UpdateDocumentStatus(SessionHelper.UserId, docid, status);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        /// <summary>
        /// Generates a SAS URL for Azure Blob Storage with read permissions valid for 1 hour
        /// </summary>
        private string GenerateBlobSasUrl(string blobUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(blobUrl))
                    return blobUrl;

                // Check if it's an Azure blob URL
                if (!blobUrl.Contains(".blob.core.windows.net"))
                    return blobUrl;

                var connectionString = CommonHelper.GetAppSettingValue("AZStorageConnectionString");
                BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

                // Parse the blob URL to extract container and blob path
                Uri uri = new Uri(blobUrl);
                string containerName = uri.Segments[1].TrimEnd('/');
                string blobPath = string.Join("", uri.Segments.Skip(2));

                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                BlobClient blobClient = containerClient.GetBlobClient(blobPath);

                // Check if the blob client can generate SAS tokens
                if (blobClient.CanGenerateSasUri)
                {
                    // Create SAS token valid for 1 hour with read permissions
                    BlobSasBuilder sasBuilder = new BlobSasBuilder()
                    {
                        BlobContainerName = containerName,
                        BlobName = blobPath,
                        Resource = "b",
                        StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                        ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
                    };
                    sasBuilder.SetPermissions(BlobSasPermissions.Read);

                    Uri sasUri = blobClient.GenerateSasUri(sasBuilder);
                    return sasUri.ToString();
                }
                else
                {
                    // If cannot generate SAS (account key not available), return original URL
                    return blobUrl;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error generating SAS token: {ex.Message}", ex);
                return blobUrl;
            }
        }
    }
}
