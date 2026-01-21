using ELG.DAL.DbEntityLearner;
using ELG.Model.Learner;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.DAL.LearnerDAL
{
    public class DocumentRep
    {

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
                string companyFolder = "Doc_" + searchCriteria.Organisation+"/";

                using (var context = new learnerDBEntities())
                {
                    var docList = context.lms_learner_getAllDocuments(searchCriteria.SearchText, searchCriteria.Category, searchCriteria.Organisation, searchCriteria.Learner, searchCriteria.IsActive).ToList();
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
                            doc.DocumentID = item.docId;
                            doc.DocumentName = item.docName;
                            doc.DocumentDesc = item.docDesc;
                            doc.DocumentPath =  item.docPath;
                            doc.DocumentStatus = item.readStatus;
                            doc.DocumentViewed = item.viewed == "1" ? true : false;
                            doc.DocumentSequence = item.docSequence == null ? "" : (Convert.ToDateTime(item.docSequence)).ToString("dd-MMM-yyyy");
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


        public Document GetDocumentDetails(int docid, Int64 learnerid)
        {
            try
            {
                Document doc = new Document();

                using (var context = new learnerDBEntities())
                {
                    var item = context.lms_learner_getDocument_info(docid, learnerid).FirstOrDefault();
                    if (item != null )
                    {
                        doc.Organisation = Convert.ToInt64(item.organisation);
                        doc.CategoryId = Convert.ToInt64(item.catId);
                        doc.CategoryName = item.catName;
                        doc.DocumentID = item.docId;
                        doc.DocumentName = item.docName;
                        doc.DocumentDesc = item.docDesc;
                        doc.DocumentPath = item.docPath;
                        doc.DocumentStatus = item.readStatus;
                        doc.DocumentViewed = item.viewed == "1" ? true : false;
                        doc.DocumentSequence = item.docSequence == null ? "" : (Convert.ToDateTime(item.docSequence)).ToString("dd-MMM-yyyy");
                    }
                }
                return doc;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// Update document status for a learner
        /// </summary>
        /// <param name="learnerID"></param>
        /// <param name="docId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public int UpdateDocumentStatus(Int64 learnerID, Int64 docId, string status)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new learnerDBEntities())
                {
                    var result = context.lms_learner_setDocStatus(learnerID, docId, status, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// mark document read
        /// </summary>
        /// <param name="learnerID"></param>
        /// <param name="docId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public int MarkDocumentRead(Int64 learnerID, Int64 docId, Int64 organisation)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new learnerDBEntities())
                {
                    var result = context.lms_learner_setDocRead(learnerID, docId, organisation, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
