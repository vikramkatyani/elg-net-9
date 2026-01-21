using ELG.DAL.DBEntitySA;
using ELG.Model.SuperAdmin;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.DAL.SuperAdminDAL
{
    public class ReportRep
    {
        #region Learner course progress report

        public CourseProgressReport GetLearningProgressReport(ProgressReportSearch searchCriteria)
        {
            try
            {
                CourseProgressReport progressReport = new CourseProgressReport();
                List<CourseProgressItem> progressRecord = new List<CourseProgressItem>();

                using (var context = new superadmindbEntities())
                {
                    var resultList = context.lms_superadmin_get_LearnerProgressReport(searchCriteria.SearchLearnerName, searchCriteria.SearchLearnerOrganisation, searchCriteria.SearchStatus, searchCriteria.Course, searchCriteria.SortCol, searchCriteria.SortColDir, searchCriteria.FromDate, searchCriteria.ToDate).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        progressReport.TotalRecords = resultList.Count();
                        var data = resultList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            CourseProgressItem progress = new CourseProgressItem();
                            progress.UserID = Convert.ToInt64(item.intContactID);
                            progress.FirstName = item.strFirstName;
                            progress.LastName = item.strSurname;
                            progress.EmailId = item.strEmail;
                            progress.Company = item.strOrganisation;
                            progress.Location = item.strLocation;
                            progress.Department = item.strDepartment;
                            progress.CourseName = item.strCourse;
                            progress.Course = Convert.ToInt64(item.intCourseId);
                            progress.RecordId = Convert.ToInt64(item.intRecordID);
                            progress.CourseStatus= item.strStatus;
                            progress.Score = Convert.ToInt32(item.intScore);
                            progress.SuspendDataString = item.strSuspendData;
                            progress.CompletionDate = item.dateCompletedOn == null ? "":(Convert.ToDateTime(item.dateCompletedOn)).ToString("dd-MMM-yyyy");
                            progress.AssignedOn = item.dateAssignedOn == null ? "":(Convert.ToDateTime(item.dateAssignedOn)).ToString("dd-MMM-yyyy");
                            progress.CompleteBy = item.coursecompleteby == null ? "":(Convert.ToDateTime(item.coursecompleteby)).ToString("dd-MMM-yyyy");
                            progressRecord.Add(progress);
                        }
                    }
                }
                progressReport.ProgressRecords = progressRecord;
                return progressReport;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Update learning record
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public int UpdateLearningProgress(CourseProgressItem record)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_update_learning_progress_record(record.RecordId, record.Score, Convert.ToDateTime(record.CompletionDate), record.CourseStatus, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        // download progress report
        public List<DownloadCourseProgressReport> DownloadLearningProgressReport(ProgressReportSearch searchCriteria)
        {
            try
            {
                List<DownloadCourseProgressReport> progressRecord = new List<DownloadCourseProgressReport>();

                using (var context = new superadmindbEntities())
                {
                    var resultList = context.lms_superadmin_get_LearnerProgressReport(searchCriteria.SearchLearnerName, searchCriteria.SearchLearnerOrganisation, searchCriteria.SearchStatus, searchCriteria.Course, searchCriteria.SortCol, searchCriteria.SortColDir, searchCriteria.FromDate, searchCriteria.ToDate).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        foreach (var item in resultList)
                        {
                            DownloadCourseProgressReport progress = new DownloadCourseProgressReport();
                            progress.FirstName = item.strFirstName;
                            progress.LastName = item.strSurname;
                            progress.EmailId = item.strEmail;
                            progress.Company = item.strOrganisation;
                            progress.Location = item.strLocation;
                            progress.Department = item.strDepartment;
                            progress.CourseName = item.strCourse;
                            progress.CourseStatus= item.strStatus;
                            progress.Score = Convert.ToInt32(item.intScore);
                            progress.CompletionDate = item.dateCompletedOn == null ? "":(Convert.ToDateTime(item.dateCompletedOn)).ToString("dd-MMM-yyyy");
                            progressRecord.Add(progress);
                        }
                    }
                }
                return progressRecord;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Learner Risk Assessment Report

        // Get learner Risk Assessment Responses
        public List<RiskAssessmentResult> GetLearnerRiskAssessmentResponses(Int64 riskAssId)
        {
            try
            {
                List<RiskAssessmentResult> raReport = new List<RiskAssessmentResult>();

                using (var context = new superadmindbEntities())
                {
                    var resultList = context.lms_superadmin_get_RiskAssessmentAnswerSheet(riskAssId).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        foreach (var item in resultList)
                        {
                            RiskAssessmentResult reportItem = new RiskAssessmentResult();
                            reportItem.RiskAssessmentId = Convert.ToInt64(item.intRiskAssessmentResultID);
                            reportItem.IssueCount = item.intIssueCount;
                            reportItem.UserNotes = item.strUserNotes;
                            reportItem.AdminNotes = item.strAdminNotes;
                            reportItem.DateSignedOff = item.datSignOff == null ? "" : (Convert.ToDateTime(item.datSignOff)).ToString("dd-MMM-yyyy");
                            reportItem.DateCompleted = item.datCompleted == null ? "" : (Convert.ToDateTime(item.datCompleted)).ToString("dd-MMM-yyyy");
                            reportItem.Question = item.strQuestion;
                            reportItem.GroupId = Convert.ToInt32(item.intGroupID);
                            reportItem.GroupName = item.strGroup;
                            reportItem.IsIssue = Convert.ToBoolean(item.blnIssue);
                            reportItem.Response = item.strQuestionOption;
                            reportItem.ResponseNote = String.IsNullOrWhiteSpace(item.strText) ? "": item.strText;
                            reportItem.SignedOfBy_FirstName = String.IsNullOrWhiteSpace(item.strFirstName) ? "": item.strFirstName;
                            reportItem.SignedOfBy_LastName = String.IsNullOrWhiteSpace(item.strSurname) ? "": item.strSurname;
                            reportItem.SignedOfBy_Email = String.IsNullOrWhiteSpace(item.strEmail) ? "": item.strEmail;
                            raReport.Add(reportItem);
                        }
                    }
                }
                return raReport;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get risk assessment progress report
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public CourseRAReport GetLearningRAReport(LearnerRAReportFilter searchCriteria )
        {
            try
            {
                CourseRAReport raReport = new CourseRAReport();
                List<CourseRAReportItem> recordItems = new List<CourseRAReportItem>();

                using (var context = new superadmindbEntities())
                {
                    var resultList = context.lms_superadmin_get_LearnerRAReport(searchCriteria.SearchLearnerName, searchCriteria.SearchLearnerOrganisation, searchCriteria.Course, searchCriteria.SignedOff, searchCriteria.Issue, searchCriteria.FromDate, searchCriteria.ToDate, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        raReport.TotalRecords = resultList.Count();
                        var data = resultList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            CourseRAReportItem reportItem = new CourseRAReportItem();
                            reportItem.RiskAssessmentId = Convert.ToInt64(item.intRiskAssessmentResultID);
                            reportItem.UserID = Convert.ToInt64(item.intContactID);
                            reportItem.FirstName = item.strFirstName;
                            reportItem.LastName = item.strSurname;
                            reportItem.EmailId = item.strEmail;
                            reportItem.Company = item.strOrganisation;
                            reportItem.Location = item.strLocation;
                            reportItem.Department = item.strDepartment;
                            reportItem.Course = Convert.ToInt64(item.intCourseId);
                            reportItem.CourseName = item.strCourse;
                            reportItem.Issue = item.intIssueCount;
                            reportItem.CompletionDate = item.datCompleted == null ? "" : (Convert.ToDateTime(item.datCompleted)).ToString("dd-MMM-yyyy");
                            reportItem.SignedOffDate = item.datSignoff == null ? "" : (Convert.ToDateTime(item.datSignoff)).ToString("dd-MMM-yyyy");
                            recordItems.Add(reportItem);
                        }
                    }
                }
                raReport.RiskAssessmentRecords = recordItems;
                return raReport;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // download Risk Assessment report
        public List<DownloadRAReport> DownloadRAReport(LearnerRAReportFilter searchCriteria)
        {
            try
            {
                List<DownloadRAReport> raRecord = new List<DownloadRAReport>();

                using (var context = new superadmindbEntities())
                {
                    var resultList = context.lms_superadmin_get_LearnerRAReport(searchCriteria.SearchLearnerName, searchCriteria.SearchLearnerOrganisation, searchCriteria.Course, searchCriteria.SignedOff, searchCriteria.Issue, searchCriteria.FromDate, searchCriteria.ToDate, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        foreach (var item in resultList)
                        {
                            DownloadRAReport raItem = new DownloadRAReport();
                            raItem.FirstName = item.strFirstName;
                            raItem.LastName = item.strSurname;
                            raItem.EmailId = item.strEmail;
                            raItem.Company = item.strOrganisation;
                            raItem.Location = item.strLocation;
                            raItem.Department = item.strDepartment;
                            raItem.CourseName = item.strCourse;
                            raItem.IssueCount = item.intIssueCount;
                            raItem.SignedOff = item.SignedOff;
                            raItem.CompletionDate = item.datCompleted == null ? "" : (Convert.ToDateTime(item.datCompleted)).ToString("dd-MMM-yyyy");
                            raItem.SignedOffDate = item.datSignoff == null ? "" : (Convert.ToDateTime(item.datSignoff)).ToString("dd-MMM-yyyy");
                            raRecord.Add(raItem);
                        }
                    }
                }
                return raRecord;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Update Risk assessment result for a user
        /// </summary>
        /// <param name="ra_id"></param>
        /// <param name="signedOff"></param>
        /// <param name="adminComments"></param>
        /// <returns></returns>
        public int UpdateLearnerRAStatus(Int64 ra_id, bool signedOff, string adminComments, Int64 adminId)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_update_RiskAssessmentResult(ra_id, signedOff, adminId, adminComments, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Certificate
        public ProgressCertificate GetCertificateRecord(Int64 recordid)
        {
            try
            {
                ProgressCertificate progress = new ProgressCertificate();

                using (var context = new superadmindbEntities())
                {
                    var item = context.lms_superadmin_get_certificate_record(recordid).FirstOrDefault();
                    if (item != null)
                    {
                        progress.FirstName = item.strFirstName;
                        progress.LastName = item.strSurname;
                        progress.CourseName = item.strCourse;
                        progress.RecordId = Convert.ToInt64(item.intRecordID);
                        progress.CourseStatus = item.strStatus;
                        progress.Score = Convert.ToInt32(item.intScore);
                        progress.CompletionDate = item.dateCompletedOn == null ? "" : (Convert.ToDateTime(item.dateCompletedOn)).ToString("dd-MMM-yyyy");
                        progress.Certificate = item.org_certificate;
                    }
                }
                return progress;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //public CompanyBusinessCertificate GetCompanyBusinessCertificate(Int64 companyId)
        //{
        //    try
        //    {
        //        CompanyBusinessCertificate details = new CompanyBusinessCertificate();

        //        using (var context = new ATF_DataEntities())
        //        {
        //            var item = context.atf_admin_get_company_details(companyId).FirstOrDefault();
        //            if (item != null)
        //            {
        //                details.CompanyName = item.strOrganisation;
        //                details.AssignedDate = item.datCreated == null ? "" : (Convert.ToDateTime(item.datCreated)).ToString("dd-MMM-yyyy");
        //                details.ExpiryDate = item.ul_datExpire == null ? "" : (Convert.ToDateTime(item.ul_datExpire)).ToString("dd-MMM-yyyy");
        //            }
        //        }
        //        return details;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}
        #endregion

        #region Company module report

        /// <summary>
        /// Return list of registered learner based on search criteria
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public OrganisationListing GetCompanyModuleReport(OrganisationModuleSearch searchCriteria)
        {
            try
            {
                OrganisationListing orgList = new OrganisationListing();
                List<OrganisationInfo> orgInfoList = new List<OrganisationInfo>();

                using (var context = new superadmindbEntities())
                {
                    var organisationList = context.lms_superadmin_get_companyListing_assigned_course(searchCriteria.Course, searchCriteria.OrganisationName, searchCriteria.Status, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (organisationList != null && organisationList.Count > 0)
                    {
                        orgList.TotalRecords = organisationList.Count();
                        var data = organisationList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            OrganisationInfo org = new OrganisationInfo();
                            org.OrgId = Convert.ToInt64(item.intOrganisationId);
                            org.OrgUId = Convert.ToString(item.org_uid);
                            org.OrganisationName = item.strOrganisation;
                            org.OrganisationNumber = item.ul_sequentialid;
                            org.ContractorName = item.strContractorName;
                            org.ContractorEmail = item.strContractorEmail;
                            org.Sector = item.strSector;
                            org.Rate = item.strRate;
                            org.Status = Convert.ToBoolean(item.ul_blnLive) ? "Active" : "In-active";
                            org.ExpiryDate = item.ul_datExpire == null ? "" : (Convert.ToDateTime(item.ul_datExpire)).ToString("dd-MMM-yyyy");
                            orgInfoList.Add(org);
                        }
                    }
                }
                orgList.OrganisationList = orgInfoList;
                return orgList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Download company module repor
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public List<DownloadOrganisationModuleReport> DownloadCompanyModuleReport(OrganisationModuleSearch searchCriteria)
        {
            try
            {
                List<DownloadOrganisationModuleReport> orgInfoList = new List<DownloadOrganisationModuleReport>();

                using (var context = new superadmindbEntities())
                {
                    var organisationList = context.lms_superadmin_get_companyListing_assigned_course(searchCriteria.Course, searchCriteria.OrganisationName, searchCriteria.Status, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (organisationList != null && organisationList.Count > 0)
                    {
                        foreach (var item in organisationList)
                        {
                            DownloadOrganisationModuleReport org = new DownloadOrganisationModuleReport();
                            org.OrganisationName = item.strOrganisation;
                            org.OrganisationNumber = item.ul_sequentialid;
                            org.ContractManagerName = item.strContractorName;
                            org.ContractManagerEmail = item.strContractorEmail;
                            org.Status = Convert.ToBoolean(item.ul_blnLive) ? "Active" : "In-active";
                            org.CourseStatus = Convert.ToBoolean(searchCriteria.Status) ? "Assigned" : "Not-Assigned";
                            //org.ExpiryDate = item.ul_datExpire == null ? "" : (Convert.ToDateTime(item.ul_datExpire)).ToString("dd-MMM-yyyy");
                            org.ExpiryDate = Convert.ToDateTime(item.ul_datExpire);
                            orgInfoList.Add(org);
                        }
                    }
                }
                return orgInfoList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Company module report

        /// <summary>
        /// Return list of registered learner based on search criteria
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public OrganisationListing GetCRMCompanyReport(CRMOrganisationFilter searchCriteria)
        {
            try
            {
                OrganisationListing orgList = new OrganisationListing();
                List<OrganisationInfo> orgInfoList = new List<OrganisationInfo>();

                using (var context = new superadmindbEntities())
                {
                    var organisationList = context.lms_superadmin_get_crm_companyListing(searchCriteria.Filter, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (organisationList != null && organisationList.Count > 0)
                    {
                        orgList.TotalRecords = organisationList.Count();
                        //var data = organisationList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();
                        var data = organisationList.ToList();

                        foreach (var item in data)
                        {
                            OrganisationInfo org = new OrganisationInfo();
                            org.OrgId = Convert.ToInt64(item.intOrganisationId);
                            org.OrgUId = Convert.ToString(item.org_uid);
                            org.OrganisationName = item.strOrganisation;
                            org.OrganisationNumber = item.ul_sequentialid;
                            org.ContractorName = item.strContractorName;
                            org.ContractorEmail = item.strContractorEmail;
                            org.Sector = item.strSector;
                            org.Rate = item.strRate;
                            org.Status = Convert.ToBoolean(item.ul_blnLive) ? "Active" : "In-active";
                            org.ExpiryDate = item.ul_datExpire == null ? "" : (Convert.ToDateTime(item.ul_datExpire)).ToString("dd-MMM-yyyy");
                            orgInfoList.Add(org);
                        }
                    }
                }
                orgList.OrganisationList = orgInfoList;
                return orgList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<OrganisationAdminInfo> GetCRMCompanyAdminReportList(string selectedOrgs, string selectedAdminTypes, int selectContractManager, int selectTrainer)
        {
            try
            {
                List<OrganisationAdminInfo> adminInfoList = new List<OrganisationAdminInfo>();

                using (var context = new superadmindbEntities())
                {
                    var adminList = context.lms_superadmin_get_crm_company_admin_Listing(selectedOrgs, selectedAdminTypes, selectContractManager, selectTrainer).ToList();
                    if (adminList != null && adminList.Count > 0)
                    {
                        foreach (var item in adminList)
                        {
                            OrganisationAdminInfo admin = new OrganisationAdminInfo();
                            admin.FirstName = item.FirstName;
                            admin.LastName = item.LastName;
                            admin.Email = item.Email;

                            adminInfoList.Add(admin);
                        }
                    }
                }
                return adminInfoList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion
    }
}
