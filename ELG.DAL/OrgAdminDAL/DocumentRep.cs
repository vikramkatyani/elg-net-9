using System;
using System.Collections.Generic;
using System.Linq;
using ELG.Model.OrgAdmin;
using ELG.DAL.DBEntity;
using System.Data.Entity.Core.Objects;

namespace ELG.DAL.OrgAdminDAL
{
    public class DocumentRep
    {
        /// <summary>
        /// Function to create new document category in an organisation
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public int CreateNewDocCategory(DocumentCategory category)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("id", typeof(long));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_createDocumentCategory(category.CategoryName, category.CategoryDesc, category.Organisation, retVal);
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
        /// Update details of a document category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public int UpdateDocumentCategory(DocumentCategory category)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_updateDocumentCategory(category.CategoryName, category.CategoryDesc, category.CategoryId, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// delete a document category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public int DeleteDocumentCategory(DocumentCategory category)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_deleteDocumentCategory(category.CategoryId, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Return list of all document categories within an organisation
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public DocumentCategoryList GetDocumentCategories(DataTableDocFilter searchCriteria)
        {
            try
            {
                DocumentCategoryList docCategoryList = new DocumentCategoryList();
                List<DocumentCategoryWithSubCategory> categoryInfoList = new List<DocumentCategoryWithSubCategory>();
                List<DocumentSubCategory> subCategoryInfoList = new List<DocumentSubCategory>();

                using (var context = new lmsdbEntities())
                {
                    var catList = context.lms_admin_getAllDocumentCategories(searchCriteria.SearchText, searchCriteria.SortCol, searchCriteria.SortColDir, searchCriteria.Organisation, searchCriteria.Admin, searchCriteria.AdminRole).ToList();
                    if (catList != null && catList.Count > 0)
                    {

                        var groupedByMainCategory = catList.AsEnumerable().GroupBy(item => item.PK_DOCUMENTID);
                        var data = groupedByMainCategory.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var mainCategory in data)
                        {
                            DocumentCategoryWithSubCategory category = new DocumentCategoryWithSubCategory();
                            List<DocumentSubCategory> subCategoryList = new List<DocumentSubCategory>();
                            foreach (var item in mainCategory)
                            {
                                category.CategoryId = item.PK_DOCUMENTID;
                                category.CategoryName = item.TX_NAME;
                                category.CategoryDesc = item.TX_DESCRIPTION;
                                category.CreatedOn = item.DT_CREATEDON == null ? "" : (Convert.ToDateTime(item.DT_CREATEDON)).ToString("dd-MMM-yyyy");

                                if(Convert.ToInt64(item.intSubCategoryId) > 0)
                                {
                                    DocumentSubCategory subCategory = new DocumentSubCategory();
                                    subCategory.SubCategoryId = Convert.ToInt64(item.intSubCategoryId);
                                    subCategory.SubCategoryName = item.strSubCategory;
                                    subCategory.SubCategoryDesc = item.strSubCategoryDesc;
                                    subCategoryList.Add(subCategory);
                                }                                
                            }
                            category.SubCategoryList = subCategoryList;
                            categoryInfoList.Add(category);
                        } 
                        
                        docCategoryList.TotalCategories = groupedByMainCategory.ToList().Count();

                    }
                }
                docCategoryList.CategoryList = categoryInfoList;
                return docCategoryList;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// Function to create new document sub-category in an organisation
        /// </summary>
        /// <param name="subCategory"></param>
        /// <returns></returns>
        public int CreateNewSubDocCategory(DocumentSubCategory subCategory)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("id", typeof(long));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_createDocumentSubCategory(subCategory.SubCategoryName, subCategory.SubCategoryDesc, subCategory.CategoryId, subCategory.SubCategoryCreatedBy, retVal);
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
        /// Update sub category details
        /// </summary>
        /// <param name="subCategory"></param>
        /// <returns></returns>
        public int UpdateDocumentSubCategory(DocumentSubCategory subCategory)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_update_DocumentSubCategory(subCategory.SubCategoryName, subCategory.SubCategoryDesc, subCategory.SubCategoryId, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// delete document sub category
        /// </summary>
        /// <param name="subCategory"></param>
        /// <returns></returns>
        public int DeleteDocumentSubCategory(DocumentSubCategory subCategory)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_delete_documentSubCategory(subCategory.SubCategoryId, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Function to create new document category in an organisation
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public int CreateNewDocument(Document document)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("id", typeof(long));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_createDocument(document.Organisation, document.DocumentName, document.DocumentDesc, document.DocumentPath, document.CategoryId, document.SubCategoryId, document.DocumentType, document.Version, document.DateOfPublish, document.DateOfReview, retVal);
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
        /// Update details of a document category
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public int UpdateDocument(Document document)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_updateDocument(document.DocumentName, document.DocumentDesc, document.DocumentPath, document.CategoryId, document.SubCategoryId, document.DocumentID, document.Version, document.DateOfPublish, document.DateOfReview, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }
        
        /// <summary>
        /// delete document
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public int DeleteDocument(Document document)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_deleteDocument(document.DocumentID, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Archive document
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="adminId"></param>
        /// <returns></returns>
        public int ArchiveDocument(Int64 documentId, Int64 adminId)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_archiveDocument(documentId, adminId, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// activate document
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="adminId"></param>
        /// <returns></returns>
        public int ActivateDocument(Int64 documentId, Int64 adminId)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_activateDocument(documentId, adminId, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Return list of all document categories within an organisation
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public DocumentList GetDocuments(DataTableDocFilter searchCriteria)
        {
            try
            {
                DocumentList documentList = new DocumentList();
                List<Document> docInfoList = new List<Document>();
                string companyFolder = "Doc_" + searchCriteria.Organisation + "/";

                using (var context = new lmsdbEntities())
                {
                    var docList = context.lms_admin_getAllDocuments(searchCriteria.SearchText, searchCriteria.Category, searchCriteria.SubCategory, searchCriteria.SortCol, searchCriteria.SortColDir, searchCriteria.Organisation, searchCriteria.AdminRole, searchCriteria.Admin, searchCriteria.IsActive).ToList();
                    if (docList != null && docList.Count > 0)
                    {
                        documentList.TotalDocuments = docList.Count();
                        var data = docList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            Document doc = new Document();
                            doc.Organisation = Convert.ToInt64(item.organisation);
                            doc.CategoryId = Convert.ToInt64(item.catId);
                            doc.CategoryName = item.catName;
                            doc.SubCategoryId = Convert.ToInt64(item.subCategoryID);
                            doc.SubCategoryName = item.subCategory;
                            doc.Version = item.version;
                            doc.DocumentID = item.docId;
                            doc.DocumentName = item.docName;
                            doc.DocumentDesc = item.docDesc;
                            doc.DocumentPath = item.docPath;
                            doc.DocumentSequence = item.docSequence == null ? "" : (Convert.ToDateTime(item.docSequence)).ToString("dd-MMM-yyyy");
                            doc.DateOfPublish = item.dateOfPublish == null ? "" : (Convert.ToDateTime(item.dateOfPublish)).ToString("dd-MMM-yyyy");
                            doc.DateOfReview = item.dateOfReview == null ? "" : (Convert.ToDateTime(item.dateOfReview)).ToString("dd-MMM-yyyy");
                            doc.DocIsArchived = Convert.ToBoolean(item.isArchived);
                            docInfoList.Add(doc);
                        }
                    }
                }
                documentList.DocList = docInfoList;
                return documentList;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// funtion to fetch list of Departments For Document Assignment
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public DepartmentListForDocumentAssignment GetDepartmentListForModuleAutoAssignment(DataTableFilter searchCriteria)
        {
            try
            {
                DepartmentListForDocumentAssignment departmentList = new DepartmentListForDocumentAssignment();
                List<DepartmentForDocumentAssignment> departmentInfoList = new List<DepartmentForDocumentAssignment>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_get_DocumentAllocation_Departments(searchCriteria.Course, searchCriteria.Company, searchCriteria.Location, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        departmentList.TotalDepartments = resultList.Count();
                        var data = resultList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            DepartmentForDocumentAssignment department = new DepartmentForDocumentAssignment();
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
        /// Get list of all Locations for document assignment
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public LocationListForDocumentAssignment GetAllLocationListForDocumentAssignment(DataTableFilter searchCriteria)
        {
            try
            {
                LocationListForDocumentAssignment locationList = new LocationListForDocumentAssignment();
                List<LocationForDocumentAssignment> locationInfoList = new List<LocationForDocumentAssignment>();
                if (searchCriteria.Course > 0)
                {
                    using (var context = new lmsdbEntities())
                    {
                        var resultList = context.lms_admin_get_DocumentAllocation_Map(searchCriteria.Company, searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.Course, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                        if (resultList != null && resultList.Count > 0)
                        {
                            //group by location
                            var groupedByLocation = resultList.GroupBy(loc => loc.intLocationID);
                            foreach (var group in groupedByLocation)
                            {
                                Int64 locid = 0;
                                string locName = "";
                                int assigned = 1;

                                foreach (lms_admin_get_DocumentAllocation_Map_Result row in group)
                                {
                                    locid = Convert.ToInt64(row.intLocationID);
                                    locName = row.strLocation;
                                    assigned = Convert.ToInt32(row.autoSet);
                                    if (assigned == 0)
                                        break;
                                }
                                LocationForDocumentAssignment location = new LocationForDocumentAssignment();
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
                    locationList.LocationList = new List<LocationForDocumentAssignment>();
                }
                return locationList;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// Get list of all Departments for document assignment
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public DepartmentListForDocumentAssignment GetAllDepartmentListForDocumentAssignment(DataTableFilter searchCriteria)
        {
            try
            {
                DepartmentListForDocumentAssignment departmentList = new DepartmentListForDocumentAssignment();
                List<DepartmentForDocumentAssignment> departmentInfoList = new List<DepartmentForDocumentAssignment>();
                if (searchCriteria.Course > 0)
                {
                    using (var context = new lmsdbEntities())
                    {
                        var resultList = context.lms_admin_get_DocumentAllocation_AllDepartments(searchCriteria.Company, searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.Course, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                        if (resultList != null && resultList.Count > 0)
                        {
                            //group by department
                            var groupedByDepartment = resultList.GroupBy(loc => loc.intDepartmentID);
                            foreach (var group in groupedByDepartment)
                            {
                                Int64 depid = 0;
                                string depName = "";
                                int assigned = 1;

                                foreach (lms_admin_get_DocumentAllocation_AllDepartments_Result row in group)
                                {
                                    depid = Convert.ToInt64(row.intDepartmentID);
                                    depName = row.strDepartment;
                                    assigned = Convert.ToInt32(row.autoSet);
                                    if (row.autoSet == null || row.autoSet == 0)
                                        break;
                                }
                                DepartmentForDocumentAssignment department = new DepartmentForDocumentAssignment();
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
                    departmentList.DepartmentList = new List<DepartmentForDocumentAssignment>();
                }
                return departmentList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Function to remove department from document allocation
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        /// 
        public int RemoveDepartmentFromDocumentAllocation(DepartmentFilterForDocumentAssignment searchCriteria)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_remove_DocumentAllocation_Departments(searchCriteria.Document, searchCriteria.Location, searchCriteria.Departments, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// To remove document allocation from location of an organisation
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public int RemoveAllLocationsForDocumentAllocation(LocationFilterForDocumentAssignment searchCriteria)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_remove_DocumentAllocation_EntireCompany_Locations(searchCriteria.Document, searchCriteria.Locations, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// To remove document allocation from departments in all locations of an organisation
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public int RemoveDepartmentFromAllLocationsForDocumentAllocation(DepartmentFilterForDocumentAssignment searchCriteria)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_remove_DocumentAllocation_EntireCompany_Departments(searchCriteria.Document, searchCriteria.Departments, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// Function to remove department from document allocation
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        /// 
        public int RemoveDepartmentFormDocumentAllocation(DepartmentFilterForDocumentAssignment searchCriteria)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_remove_DocumentAllocation_DepartmentsForLocation(searchCriteria.Document, searchCriteria.Location, searchCriteria.Departments, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Function to set department for document allocation - in selected location
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        /// 
        public int SetDocumentAllocationForDepartmentInLocation(DepartmentFilterForDocumentAssignment searchCriteria)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_Set_DocumentAllocation_ForDepartmentInLocation(searchCriteria.Document, searchCriteria.Location, searchCriteria.Departments, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Function to set department for document allocation - in selected departments
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        /// 
        public int SetDocumentAllocationForDepartments(DepartmentFilterForDocumentAssignment searchCriteria)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_Set_DocumentAllocation_ForDepartments(searchCriteria.Document, searchCriteria.Location, searchCriteria.Departments, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Function to document allocation to entire organisation
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        /// 
        public int SetDocumentAllocationForEntireOrg(DataTableFilter searchCriteria)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_Set_DocumentAllocation_ForEntireOrg(searchCriteria.Course, searchCriteria.Company, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Function to set document allocation to entire location
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        /// 
        public int SetDocumentAllocationForLocationInOrganisation(LocationFilterForDocumentAssignment searchCriteria)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_Set_DocumentAllocation_ForLocationInOrganisation(searchCriteria.Document, searchCriteria.Organisation, searchCriteria.Locations, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// function to document allocation to selected department in all location of a company - single department
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public int SetDocumentAllocationForEntireOrgDepartment(DataTableFilter searchCriteria)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_Set_DocumentAllocation_ForEntireOrgDepartment(searchCriteria.Course, searchCriteria.Department, searchCriteria.Company, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Function to set document allocation for selected location(s) in an organisation
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        /// 
        public int SetDocumentAllocationForMultipleLocationInOrganisation(LocationFilterForDocumentAssignment searchCriteria)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_Set_DocumentAllocation_ForMultipleLocationInOrganisation(searchCriteria.Document, searchCriteria.Organisation, searchCriteria.Locations, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// set document allocation for department(s) in all locations in an organisation - multiple departments
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public int SetDocumentAllocationForAllDepartmentsInOrganisation(DepartmentFilterForDocumentAssignment searchCriteria)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_Set_DocumentAllocation_ForAllDepartmentsInOrganisation(searchCriteria.Document, searchCriteria.Organisation, searchCriteria.Departments, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get location list for mapped category
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public LocationListForDocumentAssignment GetAllLocationListForCategoryAssignment(DataTableFilter searchCriteria)
        {
            try
            {
                LocationListForDocumentAssignment locationList = new LocationListForDocumentAssignment();
                List<LocationForDocumentAssignment> locationInfoList = new List<LocationForDocumentAssignment>();
                if (searchCriteria.Course > 0)
                {
                    using (var context = new lmsdbEntities())
                    {
                        var resultList = context.lms_admin_get_CategoryLocation_Map(searchCriteria.Company, searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.Course, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                        if (resultList != null && resultList.Count > 0)
                        {
                            //group by location
                            var groupedByLocation = resultList.GroupBy(loc => loc.intLocationID);
                            foreach (var group in groupedByLocation)
                            {
                                Int64 locid = 0;
                                string locName = "";
                                int assigned = 1;

                                foreach (lms_admin_get_CategoryLocation_Map_Result row in group)
                                {
                                    locid = Convert.ToInt64(row.intLocationID);
                                    locName = row.strLocation;
                                    assigned = Convert.ToInt32(row.assigned);
                                    if (assigned == 0)
                                        break;
                                }
                                LocationForDocumentAssignment location = new LocationForDocumentAssignment();
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
                    locationList.LocationList = new List<LocationForDocumentAssignment>();
                }
                return locationList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Function to set document allocation to entire location
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        /// 
        public int SetDocumentCategoryAllocationForLocationInOrganisation(LocationFilterForDocumentAssignment searchCriteria, Int64 admin)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_Set_Document_Category_Allocation_ForLocationInOrganisation(searchCriteria.Document, searchCriteria.Organisation, admin, searchCriteria.Locations, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// To remove document category allocation from location of an organisation
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public int RemoveAllLocationsForDocumentCategoryAllocation(LocationFilterForDocumentAssignment searchCriteria, Int64 admin)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_remove_Document_Category_Allocation_EntireCompany_Locations(searchCriteria.Document, admin, searchCriteria.Locations, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Function to set document allocation for selected location(s) in an organisation
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        /// 
        public int SetDocumentCategoryAllocationForMultipleLocationInOrganisation(LocationFilterForDocumentAssignment searchCriteria, Int64 admin)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_Set_Document_Category_Allocation_ForMultipleLocationInOrganisation(admin, searchCriteria.Document, searchCriteria.Organisation, searchCriteria.Locations, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Function to document allocation to entire organisation
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        /// 
        public int SetDocumentCategoryAllocationForEntireOrg(DataTableFilter searchCriteria, Int64 admin)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_Set_Document_Category_Allocation_ForEntireOrg(admin, searchCriteria.Course, searchCriteria.Company, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Function to set document allocation to entire location
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        /// 
        public int MapAdminCreatedCategory(int category, Int64 admin, int adminRole )
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_Set_Document_Category_Allocation_ForLocation_ByLocAdmin(category, admin, adminRole, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region allocated documents
        /// <summary>
        /// function to get list of all learners with document assignment status
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public OrganisationLearnerList GetUsersWithDocumentStatus(DataTableFilter searchCriteria)
        {
            try
            {
                OrganisationLearnerList orglearnerList = new OrganisationLearnerList();
                List<LearnerInfo> learnerInfoList = new List<LearnerInfo>();

                using (var context = new lmsdbEntities())
                {
                    var learnerList = context.lms_admin_get_LearnersForDocumentAssignment(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.Course, searchCriteria.SearchText, searchCriteria.Location, searchCriteria.Department, searchCriteria.Company, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
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
                            learner.IsCourseExpired = item.assigned == 1 ? true : false;
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
        /// function to revoke access of a document for learer
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public int AssignIndividualDocumentAccess(LearnerModuleFilter searchCriteria)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    context.lms_admin_assign_docAccess_toLearner_(searchCriteria.LearnerID, searchCriteria.Course, searchCriteria.AdminUserId, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// function to revoke access of a document for learer
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public int RevokeIndividualDocumentAccess(LearnerModuleFilter searchCriteria)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    context.lms_admin_revoke_docAccess_fromLearner_(searchCriteria.LearnerID, searchCriteria.Course, searchCriteria.AdminUserId, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Allocate document to multiple individual learners
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <param name="allSelected"></param>
        /// <param name="selectedUserList"></param>
        /// <param name="unselectedUserList"></param>
        /// <returns></returns>
        public int AssignIndividualDocumentAccess_All(LearnerModuleFilter searchCriteria, bool allSelected, string selectedUserList, string unselectedUserList)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_allocate_document_to_all(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.Course, searchCriteria.SearchText, searchCriteria.Location, searchCriteria.Department, searchCriteria.Company, selectedUserList, unselectedUserList, allSelected, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Document Group


        /// <summary>
        /// Return list of all document categories within an organisation
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public DocumentGroupList GetDocumentGroups(DataTableDocFilter searchCriteria)
        {
            try
            {
                DocumentGroupList docGroupList = new DocumentGroupList();
                List<DocumentGroup> groupInfoList = new List<DocumentGroup>();

                using (var context = new lmsdbEntities())
                {
                    var gList = context.lms_admin_get_doc_groups(searchCriteria.Organisation, searchCriteria.Location, searchCriteria.SearchText, searchCriteria.SortCol, searchCriteria.SortColDir, searchCriteria.Admin, searchCriteria.AdminRole).ToList();
                    if (gList != null && gList.Count > 0)
                    {
                        docGroupList.TotalGroups = gList.Count();
                        var data = gList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            DocumentGroup grp = new DocumentGroup();
                            grp.GroupId = item.intGroupId;
                            grp.GroupName = item.strGroupName;
                            grp.GroupDesc = item.strGroupDesc;
                            grp.GroupCreatedOn = item.dateCreatedOn == null ? "" : (Convert.ToDateTime(item.dateCreatedOn)).ToString("dd-MMM-yyyy");
                            grp.MappedDocuments = Convert.ToInt32(item.DocumentCount);
                            grp.GroupLocationName = item.strLocation;
                            groupInfoList.Add(grp);
                        }
                    }
                }
                docGroupList.GroupList = groupInfoList;
                return docGroupList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int CreateNewDocGroup(DocumentGroup group)
        {
            int success = 0;
            try
            {
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_create_doc_group(group.GroupOrgId, group.GroupLocationId, group.GroupName, group.GroupDesc, group.GroupCreatedBy);
                    var scalarValue = result.FirstOrDefault(); // Extract the first value from the result  
                    success = scalarValue.HasValue ? Convert.ToInt32(scalarValue.Value) : 0;
                }

            }
            catch (Exception)
            {
                throw;
            }
            return success;
        }

        /// <summary>
        /// Return list of all document categories within an organisation
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public MappedDocumentList GetDocumentsForGroupMap(DataTableDocFilter searchCriteria, Int64 groupId)
        {
            try
            {
                MappedDocumentList documentList = new MappedDocumentList();
                List<MappedDocuments> docInfoList = new List<MappedDocuments>();

                using (var context = new lmsdbEntities())
                {
                    var docList = context.lms_admin_get_AllDocuments_group(searchCriteria.SearchText, searchCriteria.Category, searchCriteria.SubCategory, searchCriteria.SortCol, searchCriteria.SortColDir, searchCriteria.Organisation, searchCriteria.AdminRole, searchCriteria.Admin, searchCriteria.IsActive, groupId).ToList();
                    if (docList != null && docList.Count > 0)
                    {
                        documentList.TotalDocuments = docList.Count();
                        var data = docList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            MappedDocuments doc = new MappedDocuments();
                            doc.Organisation = Convert.ToInt64(item.organisation);
                            doc.CategoryId = Convert.ToInt64(item.catId);
                            doc.CategoryName = item.catName;
                            doc.SubCategoryId = Convert.ToInt64(item.subCategoryID);
                            doc.SubCategoryName = item.subCategory;
                            doc.Version = item.version;
                            doc.DocumentID = item.docId;
                            doc.DocumentName = item.docName;
                            doc.DocumentDesc = item.docDesc;
                            doc.DocumentPath = item.docPath;
                            doc.DocumentSequence = item.docSequence == null ? "" : (Convert.ToDateTime(item.docSequence)).ToString("dd-MMM-yyyy");
                            doc.DateOfPublish = item.dateOfPublish == null ? "" : (Convert.ToDateTime(item.dateOfPublish)).ToString("dd-MMM-yyyy");
                            doc.DateOfReview = item.dateOfReview == null ? "" : (Convert.ToDateTime(item.dateOfReview)).ToString("dd-MMM-yyyy");
                            doc.DocIsArchived = Convert.ToBoolean(item.isArchived);
                            doc.Mapped = Convert.ToInt32(item.isMappedWithGroup);
                            docInfoList.Add(doc);
                        }
                    }
                }
                documentList.DocumentList = docInfoList;
                return documentList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Assign individual document to a group
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="docIds"></param>
        /// <returns></returns>
        public int AssignIndividualDocumentToGroup(Int32 groupId, string docIds)
        {
            int success = 0;
            try
            {
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_create_doc_group_map(groupId, docIds); 
                    var scalarValue = result.FirstOrDefault(); // Extract the first value from the result  
                    success = scalarValue.HasValue ? Convert.ToInt32(scalarValue.Value) : 0;
                }
                return success;
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Remove individual document from a group
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="docId"></param>
        /// <returns></returns>
        public int RemoveIndividualDocumentToGroup(Int32 groupId, Int32 docId)
        {
            int success = 0;
            try
            {
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_remove_doc_group_map(groupId, docId);
                    var scalarValue = result.FirstOrDefault(); // Extract the first value from the result  
                    success = scalarValue.HasValue ? Convert.ToInt32(scalarValue.Value) : 0;
                }
                return success;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Function to get list of all learners with document group assignment status
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public OrganisationLearnerList GetUsersWithDocumentGroupStatus(DataTableFilter searchCriteria)
        {
            try
            {
                OrganisationLearnerList orglearnerList = new OrganisationLearnerList();
                List<LearnerInfo> learnerInfoList = new List<LearnerInfo>();

                using (var context = new lmsdbEntities())
                {
                    var learnerList = context.lms_admin_get_LearnersForGroupAssignment(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.Course, searchCriteria.SearchText, searchCriteria.Location, searchCriteria.Department, searchCriteria.Company, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
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
                            learner.IsCourseExpired = item.isMapped == 1 ? true : false;
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
        public int AssignIndividualDocumentGroupAccess(LearnerModuleFilter searchCriteria, Int64 groupID)
        {
            int success = 0;
            try
            {
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_assign_docGroupAccess_toLearner(searchCriteria.LearnerID, groupID, searchCriteria.AdminUserId);
                    var scalarValue = result.FirstOrDefault(); // Extract the first value from the result  
                    success = scalarValue.HasValue ? Convert.ToInt32(scalarValue.Value) : 0;
                }
                return success;
            }
            catch (Exception)
            {
                throw;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <param name="allSelected"></param>
        /// <param name="selectedUserList"></param>
        /// <param name="unselectedUserList"></param>
        /// <returns></returns>
        public int AssignIndividualDocumentGroupAccess_All(LearnerModuleFilter searchCriteria, Int64 groupID, bool allSelected, string selectedUserList, string unselectedUserList)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_assign_docGroupAccess_toMultipleLearners(searchCriteria.AdminRole, searchCriteria.AdminUserId, groupID, searchCriteria.SearchText, searchCriteria.Location, searchCriteria.Department, searchCriteria.Company, selectedUserList, unselectedUserList, allSelected, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
    }
}
