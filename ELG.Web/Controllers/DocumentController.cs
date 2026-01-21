using ELG.Model.Learner;
using OrgAdmin = ELG.Model.OrgAdmin;
using Microsoft.AspNetCore.Http;
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
    public class DocumentController : Controller
    {
        #region Category
        // GET: Category
        public ActionResult DocumentCategory()
        {
            return View("DocumentCategory");
        }

        // get report on applied filter
        [HttpPost]
        public ActionResult LoadDocCategoryData(OrgAdmin.DataTableDocFilter searchCriteria)
        {
            try
            {
                var docRep = new DocumentRep();

                searchCriteria.Organisation = SessionHelper.CompanyId;
                searchCriteria.Admin = SessionHelper.UserId;
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

                OrgAdmin.DocumentCategoryList docList = docRep.GetDocumentCategories(searchCriteria);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = docList.TotalCategories, recordsTotal = docList.TotalCategories, data = docList.CategoryList, listFor = SessionHelper.UserRole });

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("Learner");
            }
        }

        // function to add new category
        [HttpPost]
        public ActionResult CreateCategory([FromBody] OrgAdmin.DocumentCategory category)
        {
            try
            {
                if (category == null || String.IsNullOrEmpty(category.CategoryName))
                    return Json(new { success = -1 });

                category.Organisation = SessionHelper.CompanyId;
                var catRep = new DocumentRep();
                int result = catRep.CreateNewDocCategory(category);

                //if location admin or location supervisor; map new category to assigned location
                if(SessionHelper.UserRole == 3 || SessionHelper.UserRole == 8)
                {
                    int map = catRep.MapAdminCreatedCategory(result, SessionHelper.UserId, SessionHelper.UserRole);
                }

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }
        
        // function to update category
        [HttpPost]
        public ActionResult UpdateCategory([FromBody] OrgAdmin.DocumentCategory category)
        {
            try
            {
                if (category == null || String.IsNullOrEmpty(category.CategoryName))
                    return Json(new { success = -1 });

                category.Organisation = SessionHelper.CompanyId;
                var catRep = new DocumentRep();
                int result = catRep.UpdateDocumentCategory(category);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        // delete category
        [HttpPost]
        public ActionResult DeleteCategory([FromBody] OrgAdmin.DocumentCategory category)
        {
            try
            {
                if (category == null)
                    return Json(new { success = -1 });

                category.Organisation = SessionHelper.CompanyId;
                var catRep = new DocumentRep();
                int result = catRep.DeleteDocumentCategory(category);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        #endregion

        #region sub-category

        // function to add new sub category
        [HttpPost]
        public ActionResult CreateSubCategory([FromBody] OrgAdmin.DocumentSubCategory subCategory)
        {
            try
            {
                if (subCategory == null || String.IsNullOrEmpty(subCategory.SubCategoryName))
                    return Json(new { success = -1 });

                subCategory.SubCategoryCreatedBy = SessionHelper.UserId;
                var catRep = new DocumentRep();
                int result = catRep.CreateNewSubDocCategory(subCategory);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        // function to update sub-category
        [HttpPost]
        public ActionResult UpdateSubCategory([FromBody] OrgAdmin.DocumentSubCategory subCategory)
        {
            try
            {
                if (subCategory == null || String.IsNullOrEmpty(subCategory.SubCategoryName))
                    return Json(new { success = -1 });

                var catRep = new DocumentRep();
                int result = catRep.UpdateDocumentSubCategory(subCategory);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        // delete sub-category
        [HttpPost]
        public ActionResult DeleteSubCategory([FromBody] OrgAdmin.DocumentSubCategory subCategory)
        {
            try
            {
                if (subCategory == null)
                    return Json(new { success = -1 });

                var catRep = new DocumentRep();
                int result = catRep.DeleteDocumentSubCategory(subCategory);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }
        #endregion

        #region Document
        // GET: Document
        public ActionResult Documents()
        {
            return View("Documents");
        }

        // GET: Document
        public ActionResult ManageAccess()
        {
            return View("ManageAccess");
        }

        // get report on applied filter
        [HttpPost]
        public ActionResult LoadDocumentData(OrgAdmin.DataTableDocFilter searchCriteria)
        {
            try
            {
                var docRep = new DocumentRep();
                searchCriteria.Admin = SessionHelper.UserId;
                searchCriteria.AdminRole = SessionHelper.UserRole;

                searchCriteria.Organisation = Convert.ToInt64(SessionHelper.CompanyId);
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

                OrgAdmin.DocumentList docList = docRep.GetDocuments(searchCriteria);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = docList.TotalDocuments, recordsTotal = docList.TotalDocuments, data = docList.DocList, listFor = SessionHelper.UserRole });

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View();
            }
        }

        // function to add new document
        [HttpPost]
        public async Task<ActionResult> CreateDocument()
        {
            int status = 0;
            try
            {
                // Get the file from the request - it comes with the filename as the key
                var files = Request.Form.Files;
                IFormFile newDocFile = files.Count > 0 ? files[0] : null;
                
                //check if document upload is valid
                if (newDocFile != null && newDocFile.Length > 0)
                {
                    IFormFile document = newDocFile;

                    //validate file size (<= 5MB)  5*1024*1024 = 5242880
                    if (document.Length > 5242880)
                    {
                        return Json(new { success = "File too large", status });
                    }

                    // Build document object from form fields
                    var doc = new OrgAdmin.Document
                    {
                        CategoryId = Convert.ToInt64(Request.Form["CategoryId"]),
                        SubCategoryId = Convert.ToInt64(Request.Form["SubCategoryId"]),
                        DocumentName = Request.Form["DocumentName"],
                        DocumentDesc = Request.Form["DocumentDesc"],
                        Version = Request.Form["Version"],
                        DateOfPublish = Request.Form["DateOfPublish"],
                        DateOfReview = Request.Form["DateOfReview"]
                    };

                    var result = await AsyncUploadFile(document, doc);

                    doc.DocumentPath = result;
                    doc.Organisation = SessionHelper.CompanyId;
                    doc.DocumentType = "File";

                    var docRep = new DocumentRep();
                    status = docRep.CreateNewDocument(doc);

                    //string _folderName = Server.MapPath("~/ATF_Company_Doc/Doc_" + SessionHelper.CompanyId);

                    //// Determine whether the directory exists.
                    //if (!Directory.Exists(_folderName))
                    //{
                    //    // Try to create the directory.
                    //    DirectoryInfo di = Directory.CreateDirectory(_folderName);
                    //}

                    //HttpFileCollectionBase documentFiles = Request.Files;
                    //IFormFile document = documentFiles[0];

                    //string _FileName = Path.GetFileName(document.FileName);
                    //string _path = Path.Combine(_folderName, _FileName);
                    //document.SaveAs(_path);

                    //doc.Organisation = SessionHelper.CompanyId;
                    //var docRep = new DocumentRep();
                    //doc.DocumentPath = _FileName;
                    //result = docRep.CreateNewDocument(doc);
                    return Json(new { success = result, status});
                }
                     return Json(new { success = "No files", status });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = ex.Message, status });
            }

        }

        //public static async Task<string> AsyncUploadFile(IFormFile newDocFile, Document doc)
        //{
        //    // Get the connection string from app settings
        //    string connectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
        //    string shareName = ConfigurationManager.AppSettings["StorageShareName"];

        //    // Instantiate a ShareClient which will be used to create and manipulate the file share
        //    ShareClient share = new ShareClient(connectionString, shareName);

        //    // Create the share if it doesn't already exist
        //    await share.CreateIfNotExistsAsync();

        //    // Ensure that the share exists
        //    if (await share.ExistsAsync())
        //    {
        //        // Get a reference to the sample directory
        //        string directoryName = SessionHelper.CompanyNumber + "_doc";
        //        ShareDirectoryClient directory = share.GetDirectoryClient(directoryName);

        //        // Create the directory if it doesn't already exist
        //        await directory.CreateIfNotExistsAsync();

        //        // Ensure that the directory exists
        //        if (await directory.ExistsAsync())
        //        {
        //            string fileName = Guid.NewGuid() + Path.GetExtension(newDocFile.FileName);
        //            ShareFileClient file = directory.GetFileClient(fileName);
        //            if (!await file.ExistsAsync())
        //            {
        //                using (var stream = newDocFile.InputStream)
        //                {
        //                    await file.CreateAsync(stream.Length);
        //                    await file.UploadAsync(stream);
        //                }
                        
        //            }
        //            return file.Uri.AbsoluteUri;

        //            //// Get a reference to a file object
        //            //ShareFileClient file = directory.GetFileClient("Log1.txt");

        //                //// Ensure that the file exists
        //                //if (await file.ExistsAsync())
        //                //{
        //                //    Console.WriteLine($"File exists: {file.Name}");

        //                //    // Download the file
        //                //    ShareFileDownloadInfo download = await file.DownloadAsync();

        //                //    // Save the data to a local file, overwrite if the file already exists
        //                //    using (FileStream stream = File.OpenWrite(@"downloadedLog1.txt"))
        //                //    {
        //                //        await download.Content.CopyToAsync(stream);
        //                //        await stream.FlushAsync();
        //                //        stream.Close();

        //                //        // Display where the file was saved
        //                //        Console.WriteLine($"File downloaded: {stream.Name}");
        //                //    }
        //                //}
        //        }
        //    }
        //        return "";
        //}

        public static async Task<string> AsyncUploadFile(IFormFile newDocFile, OrgAdmin.Document doc)

        {
            var connectionString = ELG.Web.Helper.CommonHelper.GetAppSettingValue("AZStorageConnectionString");

            // create a client with the connection
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            // Use elg-learn container
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("elg-learn");

            IFormFile document = newDocFile;
            // Create blob path: company_number/document/filename
            string companyNumber = SessionHelper.CompanyNumber;
            string filename = Guid.NewGuid() + Path.GetExtension(newDocFile.FileName);
            string blobPath = $"{companyNumber}/document/{filename}";
            string filetype = (newDocFile.ContentType);

            BlobClient blobClient = containerClient.GetBlobClient(blobPath);

            var blobHttpHeader = new BlobHttpHeaders();

            blobHttpHeader.ContentType = filetype;

            using (var stream = newDocFile.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, blobHttpHeader);
            }

            return blobClient.Uri.AbsoluteUri;

            //// Name of the share, directory, and file we'll create
            //string shareName = "chsedocuments";
            //string dirName = SessionHelper.CompanyNumber + "_doc";
            //string fileName = Guid.NewGuid() + Path.GetExtension(newDocFile.FileName);

            //// Get a reference to a share and then create it
            //ShareClient share = new ShareClient(connectionString, shareName);
            //share.Create();

            //// Get a reference to a directory and create it
            //ShareDirectoryClient directory = share.GetDirectoryClient(dirName);
            //directory.Create();

            //// Get a reference to a file and upload it
            //ShareFileClient file = directory.GetFileClient(fileName);
            //using (var stream = newDocFile.InputStream)
            //{
            //    file.Create(stream.Length);
            //    file.UploadRange(
            //        new HttpRange(0, stream.Length),
            //        stream);
            //}

            //return fileName;
        }

        // function to update document
        [HttpPost]
        public ActionResult UpdateDocument(OrgAdmin.Document doc)
        {
            try
            {
                if (String.IsNullOrEmpty(doc.DocumentName))
                    return View();

                doc.Organisation = SessionHelper.CompanyId;
                var catRep = new DocumentRep();
                int result = catRep.UpdateDocument(doc);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View();
            }
        }

        // function to delete document
        [HttpPost]
        public ActionResult DeleteDocument(OrgAdmin.Document doc)
        {
            try
            {
                doc.Organisation = SessionHelper.CompanyId;
                var catRep = new DocumentRep();
                int result = catRep.DeleteDocument(doc);

                //delete document from blob
                if (result > 0 && !string.IsNullOrEmpty(doc.DocumentPath))
                {
                    var connectionString = ELG.Web.Helper.CommonHelper.GetAppSettingValue("AZStorageConnectionString");

                    BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
                    // Use elg-learn container
                    BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("elg-learn");

                    // Extract blob path from full URL (company_number/document/filename)
                    Uri uri = new Uri(doc.DocumentPath);
                    string fullPath = uri.AbsolutePath;
                    // Remove container name from path if present
                    string blobPath = fullPath.Contains("elg-learn/") 
                        ? fullPath.Substring(fullPath.IndexOf("elg-learn/") + "elg-learn/".Length)
                        : fullPath.TrimStart('/');

                    BlobClient blobClient = containerClient.GetBlobClient(blobPath);

                    // Delete the blob if it exists
                    blobClient.DeleteIfExists();
                }

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View();
            }
        }
        // function to delete document
        [HttpPost]
        public ActionResult ArchiveDocument(Int64 DocumentID)
        {
            try
            {
                Int64 adminId = SessionHelper.UserId;
                var docRep = new DocumentRep();
                int result = docRep.ArchiveDocument(DocumentID, adminId);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View();
            }
        }
        // function to delete document
        [HttpPost]
        public ActionResult ActivateDocument(Int64 DocumentID)
        {
            try
            {
                Int64 adminId = SessionHelper.UserId;
                var docRep = new DocumentRep();
                int result = docRep.ActivateDocument(DocumentID, adminId);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View();
            }
        }

        ////get url with token
        //public JsonResult GetCloudDocumentURLString(string docPath)
        //{
        //    //// Get a file SAS from the method created ealier
        //    //Uri fileSasUri = GetFileSasUri(docPath, DateTime.UtcNow.AddHours(24), ShareFileSasPermissions.Read);
        //    UriBuilder fileSasUri = new UriBuilder($"{docPath}{ConfigurationManager.AppSettings["FileAccessToken"]}");
        //    return Json(fileSasUri.Uri);
        //}

        ////-------------------------------------------------
        //// Create a SAS URI for a file
        ////-------------------------------------------------
        //public Uri GetFileSasUri(string docPath, DateTime expiration, ShareFileSasPermissions permissions)
        //{
        //    // Get the account details from app settings
        //    string shareName = ConfigurationManager.AppSettings["StorageShareName"];
        //    string accountName = ConfigurationManager.AppSettings["StorageAccountName"];
        //    string accountKey = ConfigurationManager.AppSettings["StorageAccountKey"];

        //    ShareSasBuilder fileSAS = new ShareSasBuilder()
        //    {
        //        ShareName = shareName,
        //        FilePath = docPath,
        //        StartsOn = DateTime.UtcNow,

        //        // Specify an Azure file resource
        //        Resource = "f",

        //        // Expires in 24 hours
        //        ExpiresOn = expiration
        //    };

        //    // Set the permissions for the SAS
        //    fileSAS.SetPermissions(permissions);

        //    // Create a SharedKeyCredential that we can use to sign the SAS token
        //    StorageSharedKeyCredential credential = new StorageSharedKeyCredential(accountName, accountKey);

        //    // Build a SAS URI
        //    UriBuilder fileSasUri = new UriBuilder($"{fileSAS.FilePath}");
        //    fileSasUri.Query = fileSAS.ToSasQueryParameters(credential).ToString();

        //    // Return the URI
        //    return fileSasUri.Uri;
        //}

        #endregion

        #region Document Allocation

        public ActionResult DocumentAllocation(string id)
        {
            return View("DocumentAllocation");
        }

        // get report on applied filter
        [HttpPost]
        public ActionResult LoadDepartmentForDocumentAllocation(OrgAdmin.DataTableFilter searchCriteria)
        {
            DepartmentListForDocumentAssignment departmentList = new DepartmentListForDocumentAssignment();
            departmentList.DepartmentList = new List<DepartmentForDocumentAssignment>();
            try
            {
                var docRep = new DocumentRep();

                searchCriteria.Company = Convert.ToInt64(SessionHelper.CompanyId);

                searchCriteria.Draw = Request.Form["draw"].FirstOrDefault();
                searchCriteria.Start = Request.Form["start"].FirstOrDefault();
                searchCriteria.Length = Request.Form["length"].FirstOrDefault();

                //Find Order Column
                searchCriteria.SortCol = Request.Form[$"columns[{Request.Form["order[0][column]"].FirstOrDefault()}][name]"].FirstOrDefault();
                searchCriteria.SortColDir = Request.Form["order[0][dir]"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                departmentList = docRep.GetDepartmentListForModuleAutoAssignment(searchCriteria);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = departmentList.TotalDepartments, recordsTotal = departmentList.TotalDepartments, data = departmentList.DepartmentList });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = departmentList.TotalDepartments, recordsTotal = departmentList.TotalDepartments, data = departmentList.DepartmentList });
            }
        }

        //get list of all Locations for document allocation
        [HttpPost]
        public ActionResult LoadAllLocationsForDocumentAllocation(OrgAdmin.DataTableFilter searchCriteria)
        {
            LocationListForDocumentAssignment locationList = new LocationListForDocumentAssignment();
            locationList.LocationList = new List<LocationForDocumentAssignment>();
            try
            {
                var docRep = new DocumentRep();


                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;

                searchCriteria.Draw = Request.Form["draw"].FirstOrDefault();
                searchCriteria.Start = Request.Form["start"].FirstOrDefault();
                searchCriteria.Length = Request.Form["length"].FirstOrDefault();

                //Find Order Column
                var orderColIndex = Request.Form["order[0][column]"].FirstOrDefault();
                if (!string.IsNullOrEmpty(orderColIndex))
                {
                    searchCriteria.SortCol = Request.Form[$"columns[{orderColIndex}][name]"].FirstOrDefault();
                }
                else
                {
                    searchCriteria.SortCol = null;
                }
                searchCriteria.SortColDir = Request.Form["order[0][dir]"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                locationList = docRep.GetAllLocationListForDocumentAssignment(searchCriteria);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
            return Json(new { draw = searchCriteria.Draw, recordsFiltered = locationList.TotalLocations, recordsTotal = locationList.TotalLocations, data = locationList.LocationList });
        }

        // get all departments of a company for licence auto allocation
        [HttpPost]
        public ActionResult LoadAllDepartmentForDocumentAllocation(OrgAdmin.DataTableFilter searchCriteria)
        {
            DepartmentListForDocumentAssignment departmentList = new DepartmentListForDocumentAssignment();
            departmentList.DepartmentList = new List<DepartmentForDocumentAssignment>();
            try
            {
                var docRep = new DocumentRep();

                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;

                searchCriteria.Draw = Request.Form["draw"].FirstOrDefault();
                searchCriteria.Start = Request.Form["start"].FirstOrDefault();
                searchCriteria.Length = Request.Form["length"].FirstOrDefault();

                //Find Order Column
                searchCriteria.SortCol = Request.Form[$"columns[{Request.Form["order[0][column]"].FirstOrDefault()}][name]"].FirstOrDefault();
                searchCriteria.SortColDir = Request.Form["order[0][dir]"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                departmentList = docRep.GetAllDepartmentListForDocumentAssignment(searchCriteria);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
            return Json(new { draw = searchCriteria.Draw, recordsFiltered = departmentList.TotalDepartments, recordsTotal = departmentList.TotalDepartments, data = departmentList.DepartmentList });
        }

        // remove department from document allocation 
        [HttpPost]
        public ActionResult RemoveDocumentAllocationForDepartment(OrgAdmin.DepartmentFilterForDocumentAssignment searchCriteria)
        {
            try
            {
                var docRep = new DocumentRep();
                int result = docRep.RemoveDepartmentFromDocumentAllocation(searchCriteria);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        // remove document allocation for a module from a location
        [HttpPost]
        public ActionResult RemoveDocumentAllocationFromLocationInOrg(OrgAdmin.LocationFilterForDocumentAssignment searchCriteria)
        {
            try
            {
                var docRep = new DocumentRep();
                int result = docRep.RemoveAllLocationsForDocumentAllocation(searchCriteria);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        // remove department allocation for a document
        [HttpPost]
        public ActionResult RemoveDocumentAllocationForDepartmentInOrg(OrgAdmin.DepartmentFilterForDocumentAssignment searchCriteria)
        {
            try
            {
                var docRep = new DocumentRep();
                int result = docRep.RemoveDepartmentFromAllLocationsForDocumentAllocation(searchCriteria);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        // remove document allocation from department
        [HttpPost]
        public ActionResult RemoveDocumentAllocationFromAllDepartments(OrgAdmin.DepartmentFilterForDocumentAssignment searchCriteria)
        {
            try
            {
                // get list of all departments in location and convert to comma seperated string
                var utilities = new CommonMethods();
                var departmentList = utilities.GetDepartmentsForLocation(SessionHelper.UserRole, SessionHelper.UserId, searchCriteria.Location);
                var departments = string.Join(",", departmentList.Select(x => x.DepartmentId));
                searchCriteria.Departments = departments;

                var docRep = new DocumentRep();
                int result = docRep.RemoveDepartmentFormDocumentAllocation(searchCriteria);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        // set department to license auto allocation for a module - selected location
        [HttpPost]
        public ActionResult SetDocumentAllocationForDepartmentInLocation(OrgAdmin.DepartmentFilterForDocumentAssignment searchCriteria)
        {
            try
            {
                var docRep = new DocumentRep();
                int result = docRep.SetDocumentAllocationForDepartmentInLocation(searchCriteria);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        // set department for document allocation - selected department in all locations
        [HttpPost]
        public ActionResult SetDocumentAllocationForAllDepartments(OrgAdmin.DepartmentFilterForDocumentAssignment searchCriteria, bool allSelected, Int64[] selectedDepartmentList, Int64[] unselectedDepartmentList)
        {
            try
            {
                // get list of all departments in location and convert to comma seperated string
                var utilities = new CommonMethods();
                var departmentList = utilities.GetDepartmentsForLocation(SessionHelper.UserRole, SessionHelper.UserId, searchCriteria.Location);
                string departments = "";

                if (allSelected)
                {
                    //remove if there are unselected departments
                    if (unselectedDepartmentList != null && unselectedDepartmentList.Length > 0)
                    {
                        foreach (var dep in unselectedDepartmentList)
                        {
                            var depToRemove = departmentList.Single(x => x.DepartmentId == dep);
                            departmentList.Remove(depToRemove);
                        }
                    }

                    // convert to string
                    departments = string.Join(",", departmentList.Select(x => x.DepartmentId));
                }
                else
                {
                    // if few are selected
                    if (selectedDepartmentList != null && selectedDepartmentList.Length > 0)
                    {
                        List<OrganisationDepartment> selectedDeps = new List<OrganisationDepartment>();
                        foreach (var dep in selectedDepartmentList)
                        {
                            OrganisationDepartment newDep = new OrganisationDepartment();
                            var depToAdd = departmentList.Single(x => x.DepartmentId == dep);
                            selectedDeps.Add(depToAdd);
                        }

                        // convert to string
                        departments = string.Join(",", selectedDeps.Select(x => x.DepartmentId));
                    }

                }

                searchCriteria.Departments = departments;
                var docRep = new DocumentRep();
                int result = docRep.SetDocumentAllocationForDepartments(searchCriteria);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        [HttpPost]
        public ActionResult SetDocumentAllocationForEntireOrg(OrgAdmin.DataTableFilter searchCriteria)
        {
            try
            {
                searchCriteria.Company = SessionHelper.CompanyId;
                var docRep = new DocumentRep();
                int result = docRep.SetDocumentAllocationForEntireOrg(searchCriteria);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        //set location to license auto allocation for a module - selected multiple locations in an organisation
        [HttpPost]
        public ActionResult SetDocumentAllocationForLocationInOrganisation(OrgAdmin.LocationFilterForDocumentAssignment searchCriteria, bool allSelected, Int64[] selectedLocationList, Int64[] unselectedLocationList)
        {
            try
            {
                // get list of all departments in location and convert to comma seperated string
                var utilities = new CommonMethods();
                var locationList = utilities.GetLocationsForCompany(SessionHelper.CompanyId);
                string locations = "";

                if (allSelected)
                {
                    //remove if there are unselected departments
                    if (unselectedLocationList != null && unselectedLocationList.Length > 0)
                    {
                        foreach (var loc in unselectedLocationList)
                        {
                            var locToRemove = locationList.Single(x => x.LocationId == loc);
                            locationList.Remove(locToRemove);
                        }
                    }

                    // convert to string
                    locations = string.Join(",", locationList.Select(x => x.LocationId));
                }
                else
                {
                    // if few are selected
                    if (selectedLocationList != null && selectedLocationList.Length > 0)
                    {
                        List<OrganisationLocation> selectedLocs = new List<OrganisationLocation>();
                        foreach (var loc in selectedLocationList)
                        {
                            OrganisationLocation newLoc = new OrganisationLocation();
                            var locToAdd = locationList.Single(x => x.LocationId == loc);
                            selectedLocs.Add(locToAdd);
                        }

                        // convert to string
                        locations = string.Join(",", selectedLocs.Select(x => x.LocationId));
                    }

                }

                searchCriteria.Locations = locations;
                var docRep = new DocumentRep();
                int result = docRep.SetDocumentAllocationForLocationInOrganisation(searchCriteria);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }


        //document allocation for selected department in company, all locations
        [HttpPost]
        public ActionResult SetDocumentAllocationForEntireOrgDepartment(OrgAdmin.DataTableFilter searchCriteria)
        {
            try
            {
                searchCriteria.Company = SessionHelper.CompanyId;
                var docRep = new DocumentRep();
                int result = docRep.SetDocumentAllocationForEntireOrgDepartment(searchCriteria);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        //set location to license auto allocation for a module - selected multiple locations in an organisation
        [HttpPost]
        public ActionResult SetDocumentAllocationForMultipleLocationInOrganisation(OrgAdmin.LocationFilterForDocumentAssignment searchCriteria, bool allSelected, Int64[] selectedLocationList, Int64[] unselectedLocationList)
        {
            try
            {
                // get list of all departments in location and convert to comma seperated string
                var utilities = new CommonMethods();
                var locationList = utilities.GetLocationsForCompany(SessionHelper.CompanyId);
                string locations = "";

                if (allSelected)
                {
                    //remove if there are unselected departments
                    if (unselectedLocationList != null && unselectedLocationList.Length > 0)
                    {
                        foreach (var loc in unselectedLocationList)
                        {
                            var locToRemove = locationList.Single(x => x.LocationId == loc);
                            locationList.Remove(locToRemove);
                        }
                    }

                    // convert to string
                    locations = string.Join(",", locationList.Select(x => x.LocationId));
                }
                else
                {
                    // if few are selected
                    if (selectedLocationList != null && selectedLocationList.Length > 0)
                    {
                        List<OrganisationLocation> selectedLocs = new List<OrganisationLocation>();
                        foreach (var loc in selectedLocationList)
                        {
                            OrganisationLocation newLoc = new OrganisationLocation();
                            var locToAdd = locationList.Single(x => x.LocationId == loc);
                            selectedLocs.Add(locToAdd);
                        }

                        // convert to string
                        locations = string.Join(",", selectedLocs.Select(x => x.LocationId));
                    }

                }

                searchCriteria.Locations = locations;
                var docRep = new DocumentRep();
                int result = docRep.SetDocumentAllocationForMultipleLocationInOrganisation(searchCriteria);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        //set department to license auto allocation for a module - selected multiple departments in all locations
        [HttpPost]
        public ActionResult SetDocumentAllocationForAllDepartmentsInOrganisation(OrgAdmin.DepartmentFilterForDocumentAssignment searchCriteria, bool allSelected, Int64[] selectedDepartmentList, Int64[] unselectedDepartmentList)
        {
            try
            {
                // get list of all departments in location and convert to comma seperated string
                var utilities = new CommonMethods();
                var departmentList = utilities.GetDepartmentsForOrganisation(SessionHelper.CompanyId);
                string departments = "";

                if (allSelected)
                {
                    //remove if there are unselected departments
                    if (unselectedDepartmentList != null && unselectedDepartmentList.Length > 0)
                    {
                        foreach (var dep in unselectedDepartmentList)
                        {
                            var depToRemove = departmentList.Single(x => x.DepartmentId == dep);
                            departmentList.Remove(depToRemove);
                        }
                    }

                    // convert to string
                    departments = string.Join(",", departmentList.Select(x => x.DepartmentId));
                }
                else
                {
                    // if few are selected
                    if (selectedDepartmentList != null && selectedDepartmentList.Length > 0)
                    {
                        List<OrganisationDepartment> selectedDeps = new List<OrganisationDepartment>();
                        foreach (var dep in selectedDepartmentList)
                        {
                            OrganisationDepartment newDep = new OrganisationDepartment();
                            var depToAdd = departmentList.Single(x => x.DepartmentId == dep);
                            selectedDeps.Add(depToAdd);
                        }

                        // convert to string
                        departments = string.Join(",", selectedDeps.Select(x => x.DepartmentId));
                    }

                }

                searchCriteria.Departments = departments;
                var docRep = new DocumentRep();
                int result = docRep.SetDocumentAllocationForAllDepartmentsInOrganisation(searchCriteria);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        #endregion

        #region Document Allocation - Individual

        public ActionResult DocumentAllocationIndividual(string id)
        {
            return View("DocumentAllocationIndividual");
        }

        // get report on applied filter
        [HttpPost]
        public ActionResult LoadLearnerDataToAllocateIndividalDocument(ELG.Model.OrgAdmin.DataTableFilter searchCriteria)
        {
            OrganisationLearnerList learnerList = new OrganisationLearnerList();
            learnerList.LearnerList = new List<ELG.Model.OrgAdmin.LearnerInfo>();
            try
            {
                var docRep = new DocumentRep();

                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;

                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                    searchCriteria.Location = 0;
                    searchCriteria.Department = 0;
                }

                searchCriteria.Draw = Request.Form["draw"].FirstOrDefault();
                searchCriteria.Start = Request.Form["start"].FirstOrDefault();
                searchCriteria.Length = Request.Form["length"].FirstOrDefault();
                //Find Order Column
                var orderColIndex = Request.Form["order[0][column]"].FirstOrDefault();
                if (!string.IsNullOrEmpty(orderColIndex))
                {
                    searchCriteria.SortCol = Request.Form[$"columns[{orderColIndex}][name]"].FirstOrDefault();
                }
                else
                {
                    searchCriteria.SortCol = null;
                }
                searchCriteria.SortColDir = Request.Form["order[0][dir]"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                if (searchCriteria.Course > 0)
                    learnerList = docRep.GetUsersWithDocumentStatus(searchCriteria);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = learnerList.TotalLearners, recordsTotal = learnerList.TotalLearners, data = learnerList.LearnerList });

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = learnerList.TotalLearners, recordsTotal = learnerList.TotalLearners, data = learnerList.LearnerList });
            }
        }
       
        // assign individual document Access
        [HttpPost]
        public ActionResult AllocateIndividualDocumentToLearner([FromBody] LearnerModuleFilter searchCriteria)
        {
            try
            {
                var docRep = new DocumentRep();
                searchCriteria.Company = Convert.ToInt64(SessionHelper.CompanyId);
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;
                int result;

                //if (SessionHelper.UserRole == 6 || SessionHelper.CompanyId == 1)
                //    result = moduleRep.AllocateModuleLicenseToResellerLearner(searchCriteria);
                //else
                    result = docRep.AssignIndividualDocumentAccess(searchCriteria);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        // revoke individual document Access
        [HttpPost]
        public ActionResult RevokeIndividualDocumentToLearner(LearnerModuleFilter searchCriteria)
        {
            try
            {
                var docRep = new DocumentRep();
                searchCriteria.Company = Convert.ToInt64(SessionHelper.CompanyId);
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;
                int result;

                //if (SessionHelper.UserRole == 6 || SessionHelper.CompanyId == 1)
                //    result = moduleRep.AllocateModuleLicenseToResellerLearner(searchCriteria);
                //else
                result = docRep.RevokeIndividualDocumentAccess(searchCriteria);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        [HttpPost]
        public ActionResult AllocateIndividualDocumentToLearner_Multiple(LearnerModuleFilter searchCriteria, bool allSelected, Int64[] selectedUserList, Int64[] unselectedUserList)
        {
            try
            {
                int result = 0;
                var docRep = new DocumentRep();
                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;

                string selectedLearners = "";
                string unSelectedLearners = "";

                if (allSelected)
                {
                    //remove unselected users
                    if (unselectedUserList != null && unselectedUserList.Length > 0)
                    {
                        unSelectedLearners = string.Join(",", unselectedUserList);
                    }
                }
                else
                {
                    // if few are selected
                    if (selectedUserList != null && selectedUserList.Length > 0)
                    {
                        selectedLearners = string.Join(",", selectedUserList);
                    }

                }
                result = docRep.AssignIndividualDocumentAccess_All(searchCriteria, allSelected, selectedLearners, unSelectedLearners);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }
        #endregion

        #region Category Location Mapping

        public ActionResult CategoryLocationMap(string id)
        {
            return View("CategoryLocationMap");
        }
        
        //get list of all Locations for category allocation
        [HttpPost]
        public ActionResult LoadAllLocationsForCategoryAllocation(OrgAdmin.DataTableFilter searchCriteria)
        {
            LocationListForDocumentAssignment locationList = new LocationListForDocumentAssignment();
            locationList.LocationList = new List<LocationForDocumentAssignment>();
            try
            {
                var docRep = new DocumentRep();


                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;

                searchCriteria.Draw = Request.Form["draw"].FirstOrDefault();
                searchCriteria.Start = Request.Form["start"].FirstOrDefault();
                searchCriteria.Length = Request.Form["length"].FirstOrDefault();

                //Find Order Column
                searchCriteria.SortCol = Request.Form[$"columns[{Request.Form["order[0][column]"].FirstOrDefault()}][name]"].FirstOrDefault();
                searchCriteria.SortColDir = Request.Form["order[0][dir]"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                locationList = docRep.GetAllLocationListForCategoryAssignment(searchCriteria);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
            return Json(new { draw = searchCriteria.Draw, recordsFiltered = locationList.TotalLocations, recordsTotal = locationList.TotalLocations, data = locationList.LocationList });
        }

        //map document category with location
        [HttpPost]
        public ActionResult SetCategoryAllocationForLocationInOrganisation(OrgAdmin.LocationFilterForDocumentAssignment searchCriteria, bool allSelected, Int64[] selectedLocationList, Int64[] unselectedLocationList)
        {
            try
            {
                // get list of all departments in location and convert to comma seperated string
                var utilities = new CommonMethods();
                var locationList = utilities.GetLocationsForCompany(SessionHelper.CompanyId);
                string locations = "";

                if (allSelected)
                {
                    //remove if there are unselected departments
                    if (unselectedLocationList != null && unselectedLocationList.Length > 0)
                    {
                        foreach (var loc in unselectedLocationList)
                        {
                            var locToRemove = locationList.Single(x => x.LocationId == loc);
                            locationList.Remove(locToRemove);
                        }
                    }

                    // convert to string
                    locations = string.Join(",", locationList.Select(x => x.LocationId));
                }
                else
                {
                    // if few are selected
                    if (selectedLocationList != null && selectedLocationList.Length > 0)
                    {
                        List<OrganisationLocation> selectedLocs = new List<OrganisationLocation>();
                        foreach (var loc in selectedLocationList)
                        {
                            OrganisationLocation newLoc = new OrganisationLocation();
                            var locToAdd = locationList.Single(x => x.LocationId == loc);
                            selectedLocs.Add(locToAdd);
                        }

                        // convert to string
                        locations = string.Join(",", selectedLocs.Select(x => x.LocationId));
                    }

                }

                searchCriteria.Locations = locations;
                var docRep = new DocumentRep();
                int result = docRep.SetDocumentCategoryAllocationForLocationInOrganisation(searchCriteria, SessionHelper.UserId);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        // remove category allocation for a module from a location
        [HttpPost]
        public ActionResult RemoveCategoryAllocationFromLocationInOrg(OrgAdmin.LocationFilterForDocumentAssignment searchCriteria)
        {
            try
            {
                var docRep = new DocumentRep();
                int result = docRep.RemoveAllLocationsForDocumentCategoryAllocation(searchCriteria, SessionHelper.UserId);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        //set location to license auto allocation for a module - selected multiple locations in an organisation
        [HttpPost]
        public ActionResult SetCategoryAllocationForMultipleLocationInOrganisation(OrgAdmin.LocationFilterForDocumentAssignment searchCriteria, bool allSelected, Int64[] selectedLocationList, Int64[] unselectedLocationList)
        {
            try
            {
                // get list of all departments in location and convert to comma seperated string
                var utilities = new CommonMethods();
                var locationList = utilities.GetLocationsForCompany(SessionHelper.CompanyId);
                string locations = "";

                if (allSelected)
                {
                    //remove if there are unselected departments
                    if (unselectedLocationList != null && unselectedLocationList.Length > 0)
                    {
                        foreach (var loc in unselectedLocationList)
                        {
                            var locToRemove = locationList.Single(x => x.LocationId == loc);
                            locationList.Remove(locToRemove);
                        }
                    }

                    // convert to string
                    locations = string.Join(",", locationList.Select(x => x.LocationId));
                }
                else
                {
                    // if few are selected
                    if (selectedLocationList != null && selectedLocationList.Length > 0)
                    {
                        List<OrganisationLocation> selectedLocs = new List<OrganisationLocation>();
                        foreach (var loc in selectedLocationList)
                        {
                            OrganisationLocation newLoc = new OrganisationLocation();
                            var locToAdd = locationList.Single(x => x.LocationId == loc);
                            selectedLocs.Add(locToAdd);
                        }

                        // convert to string
                        locations = string.Join(",", selectedLocs.Select(x => x.LocationId));
                    }

                }

                searchCriteria.Locations = locations;
                var docRep = new DocumentRep();
                int result = docRep.SetDocumentCategoryAllocationForMultipleLocationInOrganisation(searchCriteria, SessionHelper.UserId);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }


        [HttpPost]
        public ActionResult SetDocumentCategoryAllocationForEntireOrg(OrgAdmin.DataTableFilter searchCriteria)
        {
            try
            {
                searchCriteria.Company = SessionHelper.CompanyId;
                var docRep = new DocumentRep();
                int result = docRep.SetDocumentCategoryAllocationForEntireOrg(searchCriteria, SessionHelper.UserId);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        #endregion

        #region Document Group
        public ActionResult DocumentBundel()
        {
            return View("DocumentBundel");
        }
        // get report on applied filter
        [HttpPost]
        public ActionResult LoadDocGroupData(OrgAdmin.DataTableDocFilter searchCriteria)
        {
            DocumentGroupList docList = new DocumentGroupList();
            docList.GroupList = new List<DocumentGroup>();
            try
            {
                var docRep = new DocumentRep();

                searchCriteria.Organisation = SessionHelper.CompanyId;
                searchCriteria.Admin = SessionHelper.UserId;
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

                docList = docRep.GetDocumentGroups(searchCriteria);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = docList.TotalGroups, recordsTotal = docList.TotalGroups, data = docList.GroupList, listFor = SessionHelper.UserRole });

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = docList.TotalGroups, recordsTotal = docList.TotalGroups, data = docList.GroupList, listFor = SessionHelper.UserRole });
            }
        }
        // function to add new category
        [HttpPost]
        public ActionResult CreateDocGroup([FromBody] OrgAdmin.DocumentGroup group)
        {
            int result = -1;
            try
            {
                if (group == null || String.IsNullOrEmpty(group.GroupName))
                    return Json(new { success = result });

                group.GroupOrgId = SessionHelper.CompanyId;
                group.GroupCreatedBy = SessionHelper.UserId;
                var catRep = new DocumentRep();
                result = catRep.CreateNewDocGroup(group);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = result });
            }
        }
        public ActionResult MapGroupDocuments()
        {
            return View("MapGroupDocuments");
        }
        // get report on applied filter
        [HttpPost]
        public ActionResult LoadGroupDocumentData(OrgAdmin.DataTableDocFilter searchCriteria, Int64 groupId)
        {
            try
            {
                var docRep = new DocumentRep();
                searchCriteria.Admin = SessionHelper.UserId;
                searchCriteria.AdminRole = SessionHelper.UserRole;

                searchCriteria.Organisation = Convert.ToInt64(SessionHelper.CompanyId);
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

                MappedDocumentList docList = docRep.GetDocumentsForGroupMap(searchCriteria, groupId);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = docList.TotalDocuments, recordsTotal = docList.TotalDocuments, data = docList.DocumentList, listFor = SessionHelper.UserRole });

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View();
            }
        }

        // assign individual document Access
        [HttpPost]
        public ActionResult AssignDocumentToGroup([FromForm] string docIds, [FromForm] Int32 groupId)
        {
            try
            {
                var docRep = new DocumentRep();
                int result;

                result = docRep.AssignIndividualDocumentToGroup(groupId, docIds);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }
        // remove individual document Access
        [HttpPost]
        public ActionResult RemoveDocumentFromGroup([FromForm] Int32 docId, [FromForm] Int32 groupId)
        {
            try
            {
                var docRep = new DocumentRep();
                int result;

                result = docRep.RemoveIndividualDocumentToGroup(groupId, docId);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }
        #endregion
        #region Document Group Allocation to learners

        public ActionResult DocGroupUserAllocation(string id)
        {
            return View("DocGroupUserAllocation");
        }

        // get report on applied filter
        [HttpPost]
        public ActionResult LoadLearnerDataToAllocateIndividalDocumentGroup(ELG.Model.OrgAdmin.DataTableFilter searchCriteria)
        {
            OrganisationLearnerList learnerList = new OrganisationLearnerList();
            learnerList.LearnerList = new List<ELG.Model.OrgAdmin.LearnerInfo>();
            try
            {
                var docRep = new DocumentRep();

                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;

                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                    searchCriteria.Location = 0;
                    searchCriteria.Department = 0;
                }

                searchCriteria.Draw = Request.Form["draw"].FirstOrDefault();
                searchCriteria.Start = Request.Form["start"].FirstOrDefault();
                searchCriteria.Length = Request.Form["length"].FirstOrDefault();
                //Find Order Column
                var orderColIndex2 = Request.Form["order[0][column]"].FirstOrDefault();
                if (!string.IsNullOrEmpty(orderColIndex2))
                {
                    searchCriteria.SortCol = Request.Form[$"columns[{orderColIndex2}][name]"].FirstOrDefault();
                }
                else
                {
                    searchCriteria.SortCol = null;
                }
                searchCriteria.SortColDir = Request.Form["order[0][dir]"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                if (searchCriteria.Course > 0)
                    learnerList = docRep.GetUsersWithDocumentGroupStatus(searchCriteria);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = learnerList.TotalLearners, recordsTotal = learnerList.TotalLearners, data = learnerList.LearnerList });

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = learnerList.TotalLearners, recordsTotal = learnerList.TotalLearners, data = learnerList.LearnerList });
            }
        }

        // assign individual document Access
        [HttpPost]
        public ActionResult AllocateIndividualDocumentGroupToLearner([FromBody] LearnerModuleFilter searchCriteria, [FromQuery] Int64 groupID)
        {
            try
            {
                var docRep = new DocumentRep();
                searchCriteria.Company = Convert.ToInt64(SessionHelper.CompanyId);
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;
                int result;

                //if (SessionHelper.UserRole == 6 || SessionHelper.CompanyId == 1)
                //    result = moduleRep.AllocateModuleLicenseToResellerLearner(searchCriteria);
                //else
                result = docRep.AssignIndividualDocumentGroupAccess(searchCriteria, groupID);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        [HttpPost]
public ActionResult AllocateIndividualDocumentGroupToLearner_Multiple([FromBody] AllocateDocumentGroupMultipleRequest request, [FromQuery] Int64 groupID)
        {
            try
            {
                int result = 0;
                var docRep = new DocumentRep();
            var searchCriteria = new LearnerModuleFilter();
            searchCriteria.Company = SessionHelper.CompanyId;
            searchCriteria.AdminRole = SessionHelper.UserRole;
            searchCriteria.AdminUserId = SessionHelper.UserId;
            searchCriteria.SearchText = request.SearchText;
            searchCriteria.Location = request.Location;
            searchCriteria.Department = request.Department;

                string selectedLearners = "";
                string unSelectedLearners = "";

                if (request.AllSelected)
                {
                    //remove unselected users
                    if (request.UnselectedUserList != null && request.UnselectedUserList.Length > 0)
                    {
                        unSelectedLearners = string.Join(",", request.UnselectedUserList);
                    }
                }
                else
                {
                    // if few are selected
                    if (request.SelectedUserList != null && request.SelectedUserList.Length > 0)
                    {
                        selectedLearners = string.Join(",", request.SelectedUserList);
                    }

                }
                result = docRep.AssignIndividualDocumentGroupAccess_All(searchCriteria, groupID, request.AllSelected, selectedLearners, unSelectedLearners);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { success = -1 });
            }
        }

        public class AllocateDocumentGroupMultipleRequest
        {
            public bool AllSelected { get; set; }
            public long[] SelectedUserList { get; set; }
            public long[] UnselectedUserList { get; set; }
            public string SearchText { get; set; }
            public long Location { get; set; }
            public long Department { get; set; }
        }
        #endregion
    }
}