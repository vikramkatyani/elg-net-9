using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.Model.OrgAdmin
{
    public class DataTableDocFilter
    {
        public string Draw { get; set; }
        public string Start { get; set; }
        public string Length { get; set; }
        public string SortCol { get; set; }
        public string SortColDir { get; set; }
        public int PageSize { get; set; }
        public int Skip { get; set; }
        public int RecordTotal { get; set; }

        public string SearchText { get; set; }
        public Int64 Organisation { get; set; }
        public Int64 Admin { get; set; }
        public int AdminRole { get; set; }
        public int Location { get; set; }
        public int Category { get; set; }
        public int SubCategory { get; set; }
        public int IsActive { get; set; }
    }

    public class DocumentCategory
    {
        public Int64 CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryDesc { get; set; }
        public Int64 Organisation { get; set; }
        public string CreatedOn { get; set; }
    }

    public class DocumentCategoryWithSubCategory : DocumentCategory
    {
        public List<DocumentSubCategory> SubCategoryList { get; set; }
    }

    public class DocumentCategoryList
    {
        public List<DocumentCategoryWithSubCategory> CategoryList { get; set; }
        public int TotalCategories { get; set; }
    }

    public class DocumentSubCategory : DocumentCategory
    {
        public Int64 SubCategoryId { get; set; }
        public string SubCategoryName { get; set; }
        public string SubCategoryDesc { get; set; }
        public string SubCategoryCreatedOn { get; set; }
        public Int64 SubCategoryCreatedBy { get; set; }
    }

    public class Document: DocumentSubCategory
    {
        public Int64 DocumentID { get; set; }
        public string DocumentName { get; set; }
        public string DocumentDesc { get; set; }
        public string DocumentPath { get; set; }
        public string DocumentType { get; set; }
        public string DocumentSequence { get; set; }
        public string Version { get; set; }
        public string DateOfPublish { get; set; }
        public string DateOfReview { get; set; }
        public bool DocIsArchived { get; set; }
    }

    public class DocumentList
    {
        public List<Document> DocList { get; set; }
        public int TotalDocuments { get; set; }
    }

    public class LocationFilterForDocumentAssignment
    {
        public Int64 Document { get; set; }
        public Int64 Organisation { get; set; }
        public string Locations { get; set; }
    }

    public class DepartmentFilterForDocumentAssignment
    {
        public Int64 Document { get; set; }
        public Int64 Organisation { get; set; }
        public Int64 Location { get; set; }
        public string Departments { get; set; }
    }

    public class DepartmentForDocumentAssignment
    {
        public Int64 DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public bool Assigned { get; set; }
    }
    public class DepartmentListForDocumentAssignment
    {
        public List<DepartmentForDocumentAssignment> DepartmentList { get; set; }
        public int TotalDepartments { get; set; }
    }

    public class LocationForDocumentAssignment
    {
        public Int64 LocationId { get; set; }
        public string LocationName { get; set; }
        public bool Assigned { get; set; }
    }
    public class LocationListForDocumentAssignment
    {
        public List<LocationForDocumentAssignment> LocationList { get; set; }
        public int TotalLocations { get; set; }
    }


    public class DocumentGroup
    {
        public Int64 GroupId { get; set; }
        public Int64 GroupLocationId { get; set; }
        public Int64 GroupOrgId { get; set; }
        public string GroupName { get; set; }
        public string GroupDesc { get; set; }
        public string GroupLocationName { get; set; }
        public int MappedDocuments { get; set; }
        public string GroupCreatedOn { get; set; }
        public Int64 GroupCreatedBy { get; set; }
    }

    public class DocumentGroupList
    {
        public List<DocumentGroup> GroupList { get; set; }
        public int TotalGroups { get; set; }
    }
    public class MappedDocuments: Document
    {
        public int Mapped { get; set; }
    }
    public class MappedDocumentList
    {
        public List<MappedDocuments> DocumentList { get; set; }
        public int TotalDocuments{ get; set; }
    }
}
