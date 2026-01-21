using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.Model.Learner
{
    public class DataTableFilter
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
        public Int64 Learner { get; set; }
        public Int64 Course { get; set; }
    }

    public class DataTableDocFilter : DataTableFilter
    {
        public int Category { get; set; }
        public int SubCategory { get; set; }
        public int IsActive { get; set; }
    }

    public class RAResponseFilter : DataTableFilter
    {
        public int RACourseID { get; set; }
        public int RAResultID { get; set; }
    }
    public class RAAttemptFilter : DataTableFilter
    {
        public int RACourseID { get; set; }
        public int RASubModuleID { get; set; }
    }

    public class DocumentCategory
    {
        public Int64 CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryDesc { get; set; }
        public Int64 Organisation { get; set; }
        public string CreatedOn { get; set; }
    }

    public class DocumentCategoryList
    {
        public List<DocumentCategory> CategoryList { get; set; }
        public int TotalCategories { get; set; }
    }

    public class Document : DocumentCategory
    {
        public Int64 DocumentID { get; set; }
        public string DocumentName { get; set; }
        public string DocumentDesc { get; set; }
        public string DocumentPath { get; set; }
        public string DocumentType { get; set; }
        public string DocumentSequence { get; set; }
        public string DocumentStatus { get; set; }
        public bool DocumentViewed { get; set; }
        public string DocumentViewedOn { get; set; }
        public string DocumentStatusUpdatedOn { get; set; }
    }

    public class DocumentList
    {
        public List<Document> DocList { get; set; }
        public int TotalDocuments { get; set; }
    }

    public class DocumentPreview 
    {
        public string DocumentPath { get; set; }
        public bool PreviewAvailable { get; set; }
    }
}
