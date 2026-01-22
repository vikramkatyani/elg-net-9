using ELG.DAL.DBEntity;
using ELG.Model.OrgAdmin;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.DAL.OrgAdminDAL
{
    public class ReportRep
    {
        #region Learner course progress report

        public CourseProgressReport GetLearningProgressReport(LearnerReportFilter searchCriteria)
        {
            try
            {
                CourseProgressReport progressReport = new CourseProgressReport();
                List<CourseProgressItem> progressRecord = new List<CourseProgressItem>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_getLearnerProgressReport(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.SearchText, searchCriteria.UserStatus, searchCriteria.Location, searchCriteria.Department, searchCriteria.Status, searchCriteria.Course, searchCriteria.Company, searchCriteria.SortCol, searchCriteria.SortColDir, searchCriteria.FromDate, searchCriteria.ToDate, searchCriteria.AccessStatus).ToList();
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
                            progress.EmployeeNumber = item.strEmployeeNumber;
                            progress.EmailId = item.strEmail;
                            progress.Location = item.strLocation;
                            progress.Department = item.strDepartment;
                            progress.CourseName = item.strCourse;
                            progress.Course = Convert.ToInt64(item.intCourseId);
                            progress.RecordId = Convert.ToInt64(item.intRecordID);
                            progress.CourseStatus = item.strStatus;
                            progress.Score = Convert.ToInt32(item.intScore);
                            progress.AssignedOn = item.dateAssignedOn == null ? "" : (Convert.ToDateTime(item.dateAssignedOn)).ToString("dd-MMM-yyyy");
                            progress.CompletionDate = item.dateCompletedOn == null ? "" : (Convert.ToDateTime(item.dateCompletedOn)).ToString("dd-MMM-yyyy");
                            progress.LastAccessedDate = item.dateLastStarted == null ? "" : (Convert.ToDateTime(item.dateLastStarted)).ToString("dd-MMM-yyyy");
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
        public CourseProgressReport GetLearningProgressReport_Historic(LearnerReportFilter searchCriteria)
        {
            try
            {
                CourseProgressReport progressReport = new CourseProgressReport();
                List<CourseProgressItem> progressRecord = new List<CourseProgressItem>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_getLearnerProgressReport_historic(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.SearchText, searchCriteria.Status, searchCriteria.Course, searchCriteria.Company, searchCriteria.SortCol, searchCriteria.SortColDir, searchCriteria.FromDate, searchCriteria.ToDate).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        progressReport.TotalRecords = resultList.Count();
                        var data = resultList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            CourseProgressItem_historic progress = new CourseProgressItem_historic();
                            progress.UserID = Convert.ToInt64(item.intContactID);
                            progress.FirstName = item.strFirstName;
                            progress.LastName = item.strSurname;
                            progress.EmployeeNumber = item.strEmployeeNumber;
                            progress.EmailId = item.strEmail;
                            progress.Location = item.strLocation;
                            progress.Department = item.strDepartment;
                            progress.CourseName = item.strCourse;
                            progress.Course = Convert.ToInt64(item.intCourseId);
                            progress.RecordId = Convert.ToInt64(item.intRecordID);
                            progress.CourseStatus= item.strStatus;
                            progress.Score = Convert.ToInt32(item.intScore);
                            progress.AssignedOn = item.dateAssignedOn == null ? "":(Convert.ToDateTime(item.dateAssignedOn)).ToString("dd-MMM-yyyy");
                            progress.CompletionDate = item.dateCompletedOn == null ? "":(Convert.ToDateTime(item.dateCompletedOn)).ToString("dd-MMM-yyyy");
                            progress.LastAccessedDate = item.dateLastStarted == null ? "":(Convert.ToDateTime(item.dateLastStarted)).ToString("dd-MMM-yyyy");
                            progress.MovedToHistoryOn = item.movedToHistoryOn == null ? "":(Convert.ToDateTime(item.movedToHistoryOn)).ToString("dd-MMM-yyyy");
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
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_update_learning_progress_record(record.RecordId, record.Score, Convert.ToDateTime(record.CompletionDate), record.CourseStatus, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }
        // download progress report
        public List<DownloadCourseProgressReport> DownloadLearningProgressReport(LearnerReportFilter searchCriteria)
        {
            try
            {
                List<DownloadCourseProgressReport> progressRecord = new List<DownloadCourseProgressReport>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_getLearnerProgressReport(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.SearchText, searchCriteria.UserStatus, searchCriteria.Location, searchCriteria.Department,searchCriteria.Status, searchCriteria.Course, searchCriteria.Company, searchCriteria.SortCol, searchCriteria.SortColDir, searchCriteria.FromDate, searchCriteria.ToDate, searchCriteria.AccessStatus).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        foreach (var item in resultList)
                        {
                            DownloadCourseProgressReport progress = new DownloadCourseProgressReport();
                            progress.FirstName = item.strFirstName;
                            progress.LastName = item.strSurname;
                            progress.EmailId = item.strEmail;
                            progress.EmployeeNumber = item.strEmployeeNumber;
                            progress.Location = item.strLocation;
                            progress.Department = item.strDepartment;
                            progress.CourseName = item.strCourse;
                            progress.CourseStatus= item.strStatus;
                            progress.AssignedOn = item.dateAssignedOn == null ? "" : (Convert.ToDateTime(item.dateAssignedOn)).ToString("dd-MMM-yyyy");
                            progress.CompletionDate = item.dateCompletedOn == null ? "":(Convert.ToDateTime(item.dateCompletedOn)).ToString("dd-MMM-yyyy");
                            progress.LastAccessedOn = item.dateLastStarted == null ? "" : (Convert.ToDateTime(item.dateLastStarted)).ToString("dd-MMM-yyyy");
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

        // download progress report historic
        public List<DownloadCourseProgressReport_Historic> DownloadLearningProgressReport_historic(LearnerReportFilter searchCriteria)
        {
            try
            {
                List<DownloadCourseProgressReport_Historic> progressRecord = new List<DownloadCourseProgressReport_Historic>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_getLearnerProgressReport_historic(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.SearchText, searchCriteria.Status, searchCriteria.Course, searchCriteria.Company, searchCriteria.SortCol, searchCriteria.SortColDir, searchCriteria.FromDate, searchCriteria.ToDate).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        foreach (var item in resultList)
                        {
                            DownloadCourseProgressReport_Historic progress = new DownloadCourseProgressReport_Historic();
                            progress.FirstName = item.strFirstName;
                            progress.LastName = item.strSurname;
                            progress.EmailId = item.strEmail;
                            progress.EmployeeNumber = item.strEmployeeNumber;
                            progress.Location = item.strLocation;
                            progress.Department = item.strDepartment;
                            progress.CourseName = item.strCourse;
                            progress.CourseStatus= item.strStatus;
                            progress.AssignedOn = item.dateAssignedOn == null ? "" : (Convert.ToDateTime(item.dateAssignedOn)).ToString("dd-MMM-yyyy");
                            progress.CompletionDate = item.dateCompletedOn == null ? "":(Convert.ToDateTime(item.dateCompletedOn)).ToString("dd-MMM-yyyy");
                            progress.LastAccessedOn = item.dateLastStarted == null ? "" : (Convert.ToDateTime(item.dateLastStarted)).ToString("dd-MMM-yyyy");
                            progress.MovedToHistoryOn = item.movedToHistoryOn == null ? "" : (Convert.ToDateTime(item.movedToHistoryOn)).ToString("dd-MMM-yyyy");
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

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_getRiskAssessmentAnswerSheet(riskAssId).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        foreach (var item in resultList)
                        {
                            RiskAssessmentResult reportItem = new RiskAssessmentResult();
                            reportItem.RiskAssessmentId = Convert.ToInt64(item.intRiskAssessmentResultID);
                            reportItem.IssueCount = item.intIssueCount;
                            reportItem.UserNotes = item.strUserNotes;
                            reportItem.AdminNotes = item.strAdminNotes;
                            reportItem.AdminNoteImagePath = String.IsNullOrWhiteSpace(item.strPathAdminNoteImage) ? "" : item.strPathAdminNoteImage;
                            reportItem.DateSignedOff = item.datSignOff == null ? "" : (Convert.ToDateTime(item.datSignOff)).ToString("dd-MMM-yyyy");
                            reportItem.DateCompleted = item.datCompleted == null ? "" : (Convert.ToDateTime(item.datCompleted)).ToString("dd-MMM-yyyy");
                            reportItem.Question = item.strQuestion;
                            reportItem.GroupId = Convert.ToInt32(item.intGroupID);
                            reportItem.GroupName = item.strGroup;
                            reportItem.IsIssue = Convert.ToBoolean(item.blnIssue);
                            reportItem.ResponseId = Convert.ToInt64(item.intAnswerID);
                            reportItem.Response = item.strQuestionOption;
                            reportItem.ResponseNote = String.IsNullOrWhiteSpace(item.strText) ? "": item.strText;
                            reportItem.Evidence = String.IsNullOrWhiteSpace(item.strEvidencePath) ? "" : item.strEvidencePath;
                            reportItem.SignedOfBy_FirstName = String.IsNullOrWhiteSpace(item.strFirstName) ? "": item.strFirstName;
                            reportItem.SignedOfBy_LastName = String.IsNullOrWhiteSpace(item.strSurname) ? "": item.strSurname;
                            reportItem.SignedOfBy_Email = String.IsNullOrWhiteSpace(item.strEmail) ? "": item.strEmail;
                            reportItem.FollowUpFeedback = String.IsNullOrWhiteSpace(item.strFollowUpFeedback) ? "" : item.strFollowUpFeedback;
                            reportItem.FollowedUpOn = item.dateFollowedUpOn == null ? "" : (Convert.ToDateTime(item.dateFollowedUpOn)).ToString("dd-MMM-yyyy");
                            reportItem.LocationName = item.strLocationName;
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
        public CourseRAReport GetLearningRAReport(LearnerRAReportFilter searchCriteria )
        {
            try
            {
                CourseRAReport raReport = new CourseRAReport();
                List<CourseRAReportItem> recordItems = new List<CourseRAReportItem>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_getLearnerRAReport(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.SearchText, searchCriteria.Location, searchCriteria.Department, searchCriteria.Course, searchCriteria.SignedOff, searchCriteria.RAStatus, searchCriteria.Issue, searchCriteria.Company, searchCriteria.FromDate, searchCriteria.ToDate, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
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
                            reportItem.EmployeeNumber = item.strEmployeeNumber;
                            reportItem.EmailId = item.strEmail;
                            reportItem.Location = item.strLocation;
                            reportItem.Department = item.strDepartment;
                            reportItem.Course = Convert.ToInt64(item.intCourseId);
                            reportItem.CourseName = item.strCourse;
                            reportItem.Issue = item.intIssueCount;
                            reportItem.RAStatus = item.RAStatus;
                            reportItem.CompletionDate = item.datCompleted == null ? "" : (Convert.ToDateTime(item.datCompleted)).ToString("dd-MMM-yyyy");
                            reportItem.SignedOffDate = item.datSignoff == null ? "" : (Convert.ToDateTime(item.datSignoff)).ToString("dd-MMM-yyyy");
                            reportItem.AssignedOnDate = item.dateAssignedOn == null ? "" : (Convert.ToDateTime(item.dateAssignedOn)).ToString("dd-MMM-yyyy");
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

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_getLearnerRAReport(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.SearchText, searchCriteria.Location, searchCriteria.Department, searchCriteria.Course, searchCriteria.SignedOff, searchCriteria.RAStatus, searchCriteria.Issue, searchCriteria.Company, searchCriteria.FromDate, searchCriteria.ToDate, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        foreach (var item in resultList)
                        {
                            DownloadRAReport raItem = new DownloadRAReport();
                            raItem.FirstName = item.strFirstName;
                            raItem.LastName = item.strSurname;
                            raItem.EmailId = item.strEmail;
                            raItem.Location = item.strLocation;
                            raItem.Department = item.strDepartment;
                            raItem.CourseName = item.strCourse;
                            raItem.IssueCount = item.intIssueCount;
                            raItem.SignedOff = item.SignedOff;
                            raItem.RAStatus = item.RAStatus;
                            raItem.CompletionDate = item.datCompleted == null ? "" : (Convert.ToDateTime(item.datCompleted)).ToString("dd-MMM-yyyy");
                            raItem.SignedOffDate = item.datSignoff == null ? "" : (Convert.ToDateTime(item.datSignoff)).ToString("dd-MMM-yyyy");
                            raItem.AssignedOnDate = item.dateAssignedOn == null ? "" : (Convert.ToDateTime(item.dateAssignedOn)).ToString("dd-MMM-yyyy");
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
        /// To save RA note image
        /// </summary>
        /// <param name="evidence"></param>
        /// <returns></returns>
        public int SaveAdminRANoteImage(RAAdminNoteImage evidence)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("retval", typeof(long));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_create_RA_NOTE_Evidence(evidence.RAId, evidence.ImagePath, retVal);
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
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_updateRiskAssessmentResult(ra_id, signedOff, adminId, adminComments, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int UpdateLearnerRAIssueFollowedUp(Int64 respId, string feedBack, Int64 adminId)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_update_ra_response_followup(respId, feedBack, adminId, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Classroom Report
        public ClassroomProgressReport GetClassroomReport(ClassroomReportFilter searchCriteria)
        {
            try
            {
                ClassroomProgressReport report = new ClassroomProgressReport();
                List<ClassroomProgressItem> recordItems = new List<ClassroomProgressItem>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_getClassroomProgressReport(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.SearchText, searchCriteria.Location, searchCriteria.Department, searchCriteria.Status, searchCriteria.Course, searchCriteria.Company, searchCriteria.FromDate, searchCriteria.ToDate, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        report.TotalRecords = resultList.Count();
                        var data = resultList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            ClassroomProgressItem reportItem = new ClassroomProgressItem();
                            reportItem.UserID = Convert.ToInt64(item.intContactID);
                            reportItem.FirstName = item.strFirstName;
                            reportItem.LastName = item.strSurname;
                            reportItem.EmployeeNumber = item.strEmployeeNumber;
                            reportItem.EmailId = item.strEmail;
                            reportItem.Location = item.strLocation;
                            reportItem.Department = item.strDepartment;
                            reportItem.Course = Convert.ToInt64(item.intOCourseId);
                            reportItem.CourseName = item.strTitle;
                            reportItem.Comment = item.strComments;
                            reportItem.TeacherName = item.strTeacherName;
                            reportItem.Venue = item.venue;
                            reportItem.AttendedOn = item.datAttended == null ? "" : (Convert.ToDateTime(item.datAttended)).ToString("dd-MMM-yyyy");
                            recordItems.Add(reportItem);
                        }
                    }
                }
                report.ClassroomRecords = recordItems;
                return report;
            }
            catch (Exception)
            {
                throw;
            }
        }
        
        // download classroom report
        public List<DownloadClassroomReport> DownloadClassroomReport(ClassroomReportFilter searchCriteria)
        {
            try
            {
                List<DownloadClassroomReport> progressRecord = new List<DownloadClassroomReport>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_getClassroomProgressReport(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.SearchText, searchCriteria.Location, searchCriteria.Department, searchCriteria.Status, searchCriteria.Course, searchCriteria.Company, searchCriteria.FromDate, searchCriteria.ToDate, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        foreach (var item in resultList)
                        {
                            DownloadClassroomReport reportItem = new DownloadClassroomReport();
                            reportItem.FirstName = item.strFirstName;
                            reportItem.LastName = item.strSurname;
                            reportItem.EmployeeNumber = item.strEmployeeNumber;
                            reportItem.EmailId = item.strEmail;
                            reportItem.Location = item.strLocation;
                            reportItem.Department = item.strDepartment;
                            reportItem.ClassName = item.strTitle;
                            reportItem.Venue = item.venue;
                            reportItem.AttendedDate = item.datAttended == null ? "" : (Convert.ToDateTime(item.datAttended)).ToString("dd-MMM-yyyy");
                            progressRecord.Add(reportItem);
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

        #region Learner Document Report
        public DocumentReport GetDocumentReport(DocumentReportFilter searchCriteria)
        {
            try
            {
                DocumentReport docReport = new DocumentReport();
                List<DocumentReportItem> docItems = new List<DocumentReportItem>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_get_LearnerDocumentReport(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.SearchText, searchCriteria.Location, searchCriteria.Department, searchCriteria.Category, searchCriteria.Status, searchCriteria.AssignmentStatus, searchCriteria.Company, searchCriteria.SortCol, searchCriteria.SortColDir, searchCriteria.FromDate, searchCriteria.ToDate).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        docReport.TotalRecords = resultList.Count();
                        var data = resultList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            DocumentReportItem reportItem = new DocumentReportItem();
                            reportItem.UserID = Convert.ToInt64(item.intContactID);
                            reportItem.FirstName = item.strFirstName;
                            reportItem.LastName = item.strSurname;
                            reportItem.EmployeeNumber = item.strEmployeeNumber;
                            reportItem.EmailId = item.strEmail;
                            reportItem.Location = item.strLocation;
                            reportItem.Department = item.strDepartment;
                            reportItem.Status = item.strUserStatus;
                            reportItem.CategoryName = item.Category;
                            reportItem.FileName = item.FileName;
                            reportItem.ViewedOnDate = item.dtOfViewed == null ? "" : (Convert.ToDateTime(item.dtOfViewed)).ToString("dd-MMM-yyyy");
                            reportItem.StatusUpdatedOn = item.statusUpdatedOn == null ? "" : (Convert.ToDateTime(item.statusUpdatedOn)).ToString("dd-MMM-yyyy");
                            reportItem.DocumentAssignmentStatus = item.assigned;
                            reportItem.DocumentStatus = item.strUserStatus;
                            reportItem.DocID = Convert.ToInt64(item.PK_DOCREFID);
                            docItems.Add(reportItem);
                        }
                    }
                }
                docReport.DocumentReportRecords = docItems;
                return docReport;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // download classroom report
        public List<DownloadDocumentReport> DownloadDocumentReport(DocumentReportFilter searchCriteria)
        {
            try
            {
                List<DownloadDocumentReport> progressRecord = new List<DownloadDocumentReport>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_get_LearnerDocumentReport(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.SearchText, searchCriteria.Location, searchCriteria.Department, searchCriteria.Category, searchCriteria.Status, searchCriteria.AssignmentStatus, searchCriteria.Company, searchCriteria.SortCol, searchCriteria.SortColDir, searchCriteria.FromDate, searchCriteria.ToDate).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        foreach (var item in resultList)
                        {
                            DownloadDocumentReport reportItem = new DownloadDocumentReport();
                            reportItem.FirstName = item.strFirstName;
                            reportItem.LastName = item.strSurname;
                            reportItem.EmployeeNumber = item.strEmployeeNumber;
                            reportItem.EmailId = item.strEmail;
                            reportItem.Location = item.strLocation;
                            reportItem.Department = item.strDepartment;
                            reportItem.Status = item.strUserStatus;
                            reportItem.Category = item.Category;
                            reportItem.File = item.FileName;
                            reportItem.ViewedDate = item.dtOfViewed == null ? "" : (Convert.ToDateTime(item.dtOfViewed)).ToString("dd-MMM-yyyy");
                            reportItem.StatusUpdatedOn = item.statusUpdatedOn == null ? "" : (Convert.ToDateTime(item.statusUpdatedOn)).ToString("dd-MMM-yyyy");
                            reportItem.Issued = item.assigned;
                            reportItem.Status = item.strUserStatus;
                            progressRecord.Add(reportItem);
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
        public int UpdateLearnerDocRecord(DocumentReportItem record)
        {
            try
            {
                // Gracefully parse optional dates; allow nulls to avoid FormatException and let the proc decide defaults
                DateTime? statusUpdatedOn = ParseNullableDate(record?.StatusUpdatedOn);
                DateTime? viewedOn = ParseNullableDate(record?.ViewedOnDate);

                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    context.lms_admin_setLearnerDocStatus(record?.UserID, record?.DocID, record?.DocumentStatus, statusUpdatedOn, viewedOn, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static DateTime? ParseNullableDate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            // Accept the formats we send from the UI and common fallbacks
            string[] formats = { "dd-MMM-yyyy", "dd-MMM-yy", "dd-MMM-yyyy HH:mm:ss", "yyyy-MM-dd", "MM/dd/yyyy" };
            if (DateTime.TryParseExact(value.Trim(), formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime parsed))
            {
                return parsed;
            }

            // Last-resort parse with current culture
            if (DateTime.TryParse(value, out parsed))
            {
                return parsed;
            }

            return null;
        }
        #endregion

        #region Learner Announcement Report
        public AnnouncementReport GetAnnouncementReport(AnnouncementReportFilter searchCriteria)
        {
            try
            {
                AnnouncementReport announcementReport = new AnnouncementReport();
                List<AnnouncementReportItem> announcementItems = new List<AnnouncementReportItem>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_getLearnerAnnouncementReport(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.SearchText, searchCriteria.Title, searchCriteria.Location, searchCriteria.Department, searchCriteria.Read, searchCriteria.Company, searchCriteria.SortCol, searchCriteria.SortColDir, searchCriteria.FromDate, searchCriteria.ToDate).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        announcementReport.TotalRecords = resultList.Count();
                        var data = resultList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            AnnouncementReportItem reportItem = new AnnouncementReportItem();
                            reportItem.Title = item.strTitle;
                            reportItem.UserID = Convert.ToInt64(item.intContactID);
                            reportItem.FirstName = item.strFirstName;
                            reportItem.LastName = item.strSurname;
                            reportItem.EmployeeNumber = item.strEmployeeNumber;
                            reportItem.EmailId = item.strEmail;
                            reportItem.Location = item.strLocation;
                            reportItem.Department = item.strDepartment;
                            reportItem.ReadStatus = item.ReadStatus;
                            reportItem.ViewedOnDate = item.datViewed == null ? "" : (Convert.ToDateTime(item.datViewed)).ToString("dd-MMM-yyyy");
                            reportItem.PublishedOnDate = item.datPublish == null ? "" : (Convert.ToDateTime(item.datPublish)).ToString("dd-MMM-yyyy");
                            announcementItems.Add(reportItem);
                        }
                    }
                }
                announcementReport.AnnouncementReportRecords = announcementItems;
                return announcementReport;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // download classroom report
        public List<DownloadAnnouncementReport> DownloadAnnouncementReport(AnnouncementReportFilter searchCriteria)
        {
            try
            {
                List<DownloadAnnouncementReport> progressRecord = new List<DownloadAnnouncementReport>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_getLearnerAnnouncementReport(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.SearchText, searchCriteria.Title, searchCriteria.Location, searchCriteria.Department, searchCriteria.Read, searchCriteria.Company, searchCriteria.SortCol, searchCriteria.SortColDir, searchCriteria.FromDate, searchCriteria.ToDate).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        foreach (var item in resultList)
                        {
                            DownloadAnnouncementReport reportItem = new DownloadAnnouncementReport();
                            reportItem.FirstName = item.strFirstName;
                            reportItem.LastName = item.strSurname;
                            reportItem.EmployeeNumber = item.strEmployeeNumber;
                            reportItem.EmailId = item.strEmail;
                            reportItem.Location = item.strLocation;
                            reportItem.Department = item.strDepartment;
                            reportItem.ReadStatus = item.ReadStatus;
                            reportItem.ViewedDate = item.datViewed == null ? "" : (Convert.ToDateTime(item.datViewed)).ToString("dd-MMM-yyyy");
                            reportItem.PublishedDate = item.datPublish == null ? "" : (Convert.ToDateTime(item.datPublish)).ToString("dd-MMM-yyyy");
                            progressRecord.Add(reportItem);
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

        #region Advance Compliancy report
        public List<AdvanceCompliancyItem> GetAdvanceCompliancyReportForCompany(AdvanceCompliancyFilter searchCriteria)
        {
            try
            {
                List<AdvanceCompliancyItem> advCompliancy = new List<AdvanceCompliancyItem>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_getCompliancyForCompany(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.Company, searchCriteria.FromDate, searchCriteria.ToDate).ToList();

                    if (resultList != null && resultList.Count > 0)
                    {
                        foreach (var item in resultList)
                        {
                            AdvanceCompliancyItem adv = new AdvanceCompliancyItem();
                            adv.Location = Convert.ToInt64(item.intlocationid);
                            adv.CourseID = Convert.ToInt64(item.intCourseID);
                            adv.TotalUsers = Convert.ToInt64(item.totalUsers);
                            adv.AssignedTo = Convert.ToInt64(item.assignedto);
                            adv.CompletedBy = Convert.ToInt64(item.completedBy);
                            advCompliancy.Add(adv);
                        }
                    }
                }
                return advCompliancy;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<AdvanceCompliancyItem> GetAdvanceCompliancyReportForLocation(AdvanceCompliancyFilter searchCriteria)
        {
            try
            {
                List<AdvanceCompliancyItem> advCompliancy = new List<AdvanceCompliancyItem>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_getCompliancyForLocation(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.Company, searchCriteria.Location, searchCriteria.FromDate, searchCriteria.ToDate).ToList();

                    if (resultList != null && resultList.Count > 0)
                    {
                        foreach (var item in resultList)
                        {
                            AdvanceCompliancyItem adv = new AdvanceCompliancyItem();
                            adv.Department = Convert.ToInt64(item.intDepartmentID);
                            adv.CourseID = Convert.ToInt64(item.intCourseID);
                            adv.TotalUsers = Convert.ToInt64(item.totalUsers);
                            adv.AssignedTo = Convert.ToInt64(item.assignedto);
                            adv.CompletedBy = Convert.ToInt64(item.completedBy);
                            advCompliancy.Add(adv);
                        }
                    }
                }
                return advCompliancy;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// To get list of all users with the completion record
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public CourseProgressReport GetAdvanceCompliancyUserReport(LearnerReportFilter searchCriteria)
        {
            try
            {
                CourseProgressReport progressReport = new CourseProgressReport();
                List<CourseProgressItem> progressRecord = new List<CourseProgressItem>();

                using (var context = new lmsdbEntities())
                {
                    var learnerList = context.lms_admin_get_advrep_users(searchCriteria.Company, searchCriteria.Course, searchCriteria.Location, searchCriteria.Department, searchCriteria.FromDate, searchCriteria.ToDate).ToList();
                    if (learnerList != null && learnerList.Count > 0)
                    {
                        progressReport.TotalRecords = learnerList.Count();
                        var data = learnerList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            CourseProgressItem progress = new CourseProgressItem();
                            progress.UserID = Convert.ToInt64(item.intContactID);
                            progress.FirstName = item.strFirstName;
                            progress.LastName = item.strSurname;
                            progress.EmailId = item.strEmail;
                            progress.Location = item.strLocation;
                            progress.Department = item.strDepartment;
                            progress.CourseStatus = item.strStatus;
                            progress.CompletionDate = item.dateCompletedOn == null ? "" : (Convert.ToDateTime(item.dateCompletedOn)).ToString("dd-MMM-yyyy");
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

        #endregion

        #region Learner overdue course report

        public OverDueCourseReport GetOverdueReport(LearnerReportFilter searchCriteria)
        {
            try
            {
                OverDueCourseReport progressReport = new OverDueCourseReport();
                List<OverDueCourseItem> progressRecord = new List<OverDueCourseItem>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_getOverdueCourseReport(searchCriteria.AdminRole,searchCriteria.AdminUserId, searchCriteria.SearchText, searchCriteria.Course, searchCriteria.Company, searchCriteria.FromDate, searchCriteria.ToDate, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        progressReport.TotalRecords = resultList.Count();
                        var data = resultList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            OverDueCourseItem progress = new OverDueCourseItem();
                            progress.UserID = Convert.ToInt64(item.intContactID);
                            progress.FirstName = item.strFirstname;
                            progress.LastName = item.strSurname;
                            progress.EmployeeNumber = item.strEmployeeNumber;
                            progress.EmailId = item.strEmail;
                            progress.CourseName = item.strCourse;
                            progress.Course = Convert.ToInt64(item.intcourseID);
                            progress.Frequency = Convert.ToInt32(item.intTestFrequency);
                            progress.AssignedOn = item.dateAssignedOn == null ? "" : (Convert.ToDateTime(item.dateAssignedOn)).ToString("dd-MMM-yyyy");
                            progress.CompletionDate = item.dateCompletedOn == null ? "" : (Convert.ToDateTime(item.dateCompletedOn)).ToString("dd-MMM-yyyy");
                            progress.OverDueDate = item.overDue == null ? "" : (Convert.ToDateTime(item.overDue)).ToString("dd-MMM-yyyy");
                            progressRecord.Add(progress);
                        }
                    }
                }
                progressReport.OverDueCourseRecords = progressRecord;
                return progressReport;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // download progress report
        public List<OverDueCourseDownloadItem> DownloadOverDueReport(LearnerReportFilter searchCriteria)
        {
            try
            {
                List<OverDueCourseDownloadItem> progressRecord = new List<OverDueCourseDownloadItem>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_getOverdueCourseReport(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.SearchText, searchCriteria.Course, searchCriteria.Company, searchCriteria.FromDate, searchCriteria.ToDate, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        foreach (var item in resultList)
                        {
                            OverDueCourseDownloadItem progress = new OverDueCourseDownloadItem();
                            progress.FirstName = item.strFirstname;
                            progress.LastName = item.strSurname;
                            progress.EmailId = item.strEmail;
                            progress.CourseName = item.strCourse;
                            progress.AssignedOn = item.dateAssignedOn == null ? "" : (Convert.ToDateTime(item.dateAssignedOn)).ToString("dd-MMM-yyyy");
                            progress.OverDueDate = item.overDue == null ? "" : (Convert.ToDateTime(item.overDue)).ToString("dd-MMM-yyyy");
                            progress.CompletionDate = item.dateCompletedOn == null ? "" : (Convert.ToDateTime(item.dateCompletedOn)).ToString("dd-MMM-yyyy");
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

        #region Non compliant learner report
        /// <summary>
        /// function to get list of all learners who haven't completed the module
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public OrganisationLearnerList GetNonCompliantUserReport(DataTableFilter searchCriteria)
        {
            try
            {
                OrganisationLearnerList orglearnerList = new OrganisationLearnerList();
                List<LearnerInfo> learnerInfoList = new List<LearnerInfo>();

                using (var context = new lmsdbEntities())
                {
                    var learnerList = context.lms_admin_getNonCompliantRecords(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.Company, searchCriteria.Course, searchCriteria.Department).ToList();
                    if (learnerList != null && learnerList.Count > 0)
                    {
                        orglearnerList.TotalLearners = learnerList.Count();
                        var data = learnerList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            LearnerInfo learner = new LearnerInfo();
                            learner.UserID = Convert.ToInt64(item.intContactID);
                            learner.FirstName = item.strFirstName;
                            learner.LastName = item.strSurname;
                            learner.EmployeeNumber = item.strEmployeeNumber;
                            learner.EmailId = item.strEmail;
                            learner.Location = item.strLocation;
                            learner.Department = item.strDepartment;
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
        /// Function to Allocate Module License To Learner
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        /// 
        public int AllocateModuleLicenseToLearner(LearnerModuleFilter searchCriteria)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                ObjectParameter noLicence = new ObjectParameter("courseList", typeof(string));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_AssignCoursestoContact(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.LearnerID, Convert.ToString(searchCriteria.Course), retVal, noLicence);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Function to reassign module access to filtered users
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public int ReAssignModuleAccess(LearnerModuleFilter searchCriteria)
        {
            try
            {
                int result = 0;
                using (var context = new lmsdbEntities())
                {
                    context.lms_admin_ReassignLearnerModuleAccess(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.Course, searchCriteria.SearchText, searchCriteria.Location, searchCriteria.Department, searchCriteria.Company, searchCriteria.LearnerID);
                    result = 1;
                }
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion


        #region Certificate
        public CourseProgressItem GetCertificateRecord(Int64 recordid)
        {
            try
            {
                CourseProgressItem progress = new CourseProgressItem();

                using (var context = new lmsdbEntities())
                {
                    var item = context.lms_admin_get_certificate_record(recordid).FirstOrDefault();
                    if (item != null)
                    {
                        progress.FirstName = item.strFirstName;
                        progress.LastName = item.strSurname;
                        progress.CourseName = item.strCourse;
                        progress.RecordId = Convert.ToInt64(item.intRecordID);
                        progress.CourseStatus = item.strStatus;
                        progress.Score = Convert.ToInt32(item.intScore);
                        progress.CPDScore = Convert.ToInt32(item.intcpdscore);
                        progress.CompletionDate = item.dateCompletedOn == null ? "" : (Convert.ToDateTime(item.dateCompletedOn)).ToString("dd-MMM-yyyy");
                        progress.CertificateNumber = item.certGUID == null ? "" : Convert.ToString(item.certGUID);
                    }
                }
                return progress;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public CompanyBusinessCertificate GetCompanyBusinessCertificate(Int64 companyId)
        {
            try
            {
                CompanyBusinessCertificate details = new CompanyBusinessCertificate();

                using (var context = new lmsdbEntities())
                {
                    var item = context.lms_admin_get_company_details(companyId).FirstOrDefault();
                    if (item != null)
                    {
                        details.CompanyName = item.strOrganisation;
                        details.AssignedDate = item.datCreated == null ? "" : (Convert.ToDateTime(item.datCreated)).ToString("dd-MMM-yyyy");
                        details.ExpiryDate = item.ul_datExpire == null ? "" : (Convert.ToDateTime(item.ul_datExpire)).ToString("dd-MMM-yyyy");
                    }
                }
                return details;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Manual Notification

        public CourseProgressReportForManualReminder GetLearningProgressReportForManualNotification(LearnerReportFilter searchCriteria)
        {
            try
            {
                CourseProgressReportForManualReminder progressReport = new CourseProgressReportForManualReminder();
                List<CourseProgressItemForManualReminder> progressRecord = new List<CourseProgressItemForManualReminder>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_get_LearnerProgressReportForManualNotification(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.SearchText, searchCriteria.Location, searchCriteria.Department, searchCriteria.Status, searchCriteria.Course, searchCriteria.Company, searchCriteria.SortCol, searchCriteria.SortColDir, searchCriteria.FromDate, searchCriteria.ToDate).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        progressReport.TotalRecords = resultList.Count();
                        var data = resultList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            CourseProgressItemForManualReminder progress = new CourseProgressItemForManualReminder();
                            progress.UserID = Convert.ToInt64(item.intContactID);
                            progress.FirstName = item.strFirstName;
                            progress.LastName = item.strSurname;
                            progress.EmployeeNumber = item.strEmployeeNumber;
                            progress.EmailId = item.strEmail;
                            progress.Location = item.strLocation;
                            progress.Department = item.strDepartment;
                            progress.CourseName = item.strCourse;
                            progress.Course = Convert.ToInt64(item.intCourseId);
                            progress.RecordId = Convert.ToInt64(item.intRecordID);
                            progress.CourseStatus = item.strStatus;
                            progress.AssignedOn = item.dateAssignedOn == null ? "" : (Convert.ToDateTime(item.dateAssignedOn)).ToString("dd-MMM-yyyy");
                            progress.LatestReminderOn = item.latestReminderOn == null ? "" : (Convert.ToDateTime(item.latestReminderOn)).ToString("dd-MMM-yyyy");
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

        //get progress records with overdue date for sending notification
        public CourseProgressReport GetLearningProgressRecordsForManualNotification(Int64 company, string selectedRecordList)
        {
            try
            {
                CourseProgressReport progressReport = new CourseProgressReport();
                List<CourseProgressItem> progressRecord = new List<CourseProgressItem>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_get_progressRecordWithOverdueDate(company, selectedRecordList).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        progressReport.TotalRecords = resultList.Count();

                        foreach (var item in resultList)
                        {
                            CourseProgressItem progress = new CourseProgressItem();
                            progress.UserID = Convert.ToInt64(item.intContactID);
                            progress.FirstName = item.strFirstName;
                            progress.LastName = item.strSurname;
                            progress.EmailId = item.strEmail;
                            progress.Course = Convert.ToInt64(item.intcourseId);
                            progress.CourseName = item.strCourse;
                            progress.RecordId = Convert.ToInt64(item.intRecordID);
                            progress.CompletionDate = item.CompletionDate == null ? "" : (Convert.ToDateTime(item.CompletionDate)).ToString("dd-MMM-yyyy");
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
        /// Function to create new document category in an organisation
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public int CreateCourseReminderLog(Int64 learner, Int64 course, Int64 admin)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("id", typeof(long));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_create_reminderNotificationLog(learner, course, admin, retVal);
                    success = Convert.ToInt32(retVal.Value);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return success;
        }
        #endregion

        #region Auto Allocation Report
        public CourseAutoAllocationReport GetCourseAutoAllocationReport(DataTableFilter searchCriteria)
        {
            try
            {
                CourseAutoAllocationReport courseAutoAllocationReport = new CourseAutoAllocationReport();
                List<CourseAutoAllocationItem> courseAutoAllocationRecord = new List<CourseAutoAllocationItem>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_get_CourseAutoAllocationReport(searchCriteria.Location, searchCriteria.Department, searchCriteria.Course, searchCriteria.Company, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        courseAutoAllocationReport.TotalRecords = resultList.Count();
                        var data = resultList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            CourseAutoAllocationItem allocationRecord = new CourseAutoAllocationItem();
                            allocationRecord.Course = Convert.ToInt32(item.intcourseid);
                            allocationRecord.Location = Convert.ToInt32(item.intLocationID);
                            allocationRecord.Department = Convert.ToInt32(item.intDepartmentID);
                            allocationRecord.CourseName = item.strcourse;
                            allocationRecord.LocationName = item.strLocation;
                            allocationRecord.DepartmentName = item.strDepartment;
                            allocationRecord.IsAutoAllocationOn = item.AutoAssigned;
                            courseAutoAllocationRecord.Add(allocationRecord);
                        }
                    }
                }
                courseAutoAllocationReport.AllocationRecords = courseAutoAllocationRecord;
                return courseAutoAllocationReport;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<CourseAutoAllocationDownloadItem> DownloadCourseAutoAllocationReport(DataTableFilter searchCriteria)
        {
            try
            {
                List<CourseAutoAllocationDownloadItem> courseAutoAllocationRecord = new List<CourseAutoAllocationDownloadItem>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_get_CourseAutoAllocationReport(searchCriteria.Location, searchCriteria.Department, searchCriteria.Course, searchCriteria.Company, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        foreach (var item in resultList)
                        {
                            CourseAutoAllocationDownloadItem allocationRecord = new CourseAutoAllocationDownloadItem();
                            allocationRecord.Course = item.strcourse;
                            allocationRecord.Location = item.strLocation;
                            allocationRecord.Department = item.strDepartment;
                            allocationRecord.AutoAllocation = item.AutoAssigned;
                            courseAutoAllocationRecord.Add(allocationRecord);
                        }
                    }
                }
                return courseAutoAllocationRecord;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Training Card
        public TrainingCard GetTrainingCard(DataTableFilter searchCriteria)
        {
            try
            {
                TrainingCard trainingCard = new TrainingCard();
                TraineeInfo trainee = new TraineeInfo();
                List<LearnerAssignedCourse> TestResultRecord = new List<LearnerAssignedCourse>();
                List<LearnerRACourse> RAResultRecord = new List<LearnerRACourse>();
                List<TraineeDocResult> DocResultRecord = new List<TraineeDocResult>();

                using (var context = new lmsdbEntities())
                {
                    var traineeDetails = context.lms_admin_get_trainingCard_TraineeDetails(searchCriteria.SearchText, searchCriteria.Company).ToList();
                    if (traineeDetails != null && traineeDetails.Count > 0)
                    {
                        trainee.TraineeID = traineeDetails[0].TraineeId;
                        trainee.TraineeName = traineeDetails[0].Trainee;
                        trainee.TraineeEmail = traineeDetails[0].TraineeEmail;
                        trainee.TraineeLocation = traineeDetails[0].Location;
                        trainee.TraineeDepartment = traineeDetails[0].Department;
                        trainee.TraineePic = traineeDetails[0].TraineePic;
                    }
                }
                trainingCard.Trainee = trainee;
                //get test results
                if (trainingCard.Trainee != null && trainingCard.Trainee.TraineeID > 0)
                {

                    using (var context = new lmsdbEntities())
                    {
                        var resultList = context.lms_admin_get_trainingCard_TraineeTestResults(trainingCard.Trainee.TraineeID, searchCriteria.Company).ToList();
                        var raRecordList = context.lms_admin_get_trainingCard_TraineeRAResults(trainingCard.Trainee.TraineeID, searchCriteria.Company).ToList();
                        var docResultList = context.lms_admin_get_trainingCard_TraineeDocResults(trainingCard.Trainee.TraineeID, searchCriteria.Company).ToList();

                        if (resultList != null && resultList.Count > 0)
                        {
                            foreach (var item in resultList)
                            {
                                LearnerAssignedCourse course = new LearnerAssignedCourse();
                                course.CourseId = item.intcourseid;
                                course.ProgressRecordId = Convert.ToInt64(item.intrecordid);
                                course.CourseName = item.strcourse;
                                course.CourseDesc = item.strcoursedescription;
                                course.CourseSummary = item.strcoursesummary;
                                course.CourseLogo = $"{item.strcourselogo}.jpg";
                                course.CoursePath = item.strcoursetutorialpath;
                                course.IsCourseRA = item.blncoursera;
                                course.TestFrequency = item.inttestfrequency;
                                course.IsExpired = item.iscourseexpired;

                                if (item.strstatus == null)
                                    course.ProgressStatus = "NOT STARTED";
                                else if (item.strstatus.ToLower() == "started")
                                    course.ProgressStatus = "IN-PROGRESS";
                                else if (item.strstatus.ToLower() == "incomplete")
                                    course.ProgressStatus = "IN-PROGRESS";
                                else
                                    course.ProgressStatus = item.strstatus.ToUpper();

                                course.Score = Convert.ToInt32(item.intscore);
                                course.StartedOn = item.datestartedon == null ? "" : (Convert.ToDateTime(item.datestartedon)).ToString("dd-MMM-yyyy");
                                course.LastAccessedOn = item.datelaststarted == null ? "" : (Convert.ToDateTime(item.datelaststarted)).ToString("dd-MMM-yyyy");
                                course.CompletedOn = item.datecompletedon == null ? "" : (Convert.ToDateTime(item.datecompletedon)).ToString("dd-MMM-yyyy");
                                course.AssignedOn = item.dateassignedon == null ? "" : (Convert.ToDateTime(item.dateassignedon)).ToString("dd-MMM-yyyy");
                                course.CourseExpiryDate = item.datcourseexpiry == null ? "" : (Convert.ToDateTime(item.datcourseexpiry)).ToString("dd-MMM-yyyy");
                                course.CourseCompleteBy = item.coursecompleteby == null ? "" : (Convert.ToDateTime(item.coursecompleteby)).ToString("dd-MMM-yyyy");
                                course.CourseResetOn = item.courseResetOn == null ? "" : (Convert.ToDateTime(item.courseResetOn)).ToString("dd-MMM-yyyy");

                                TestResultRecord.Add(course);
                            }
                            trainingCard.TestResult = TestResultRecord;
                        }

                        if (raRecordList != null && raRecordList.Count > 0)
                        {
                            foreach (var item in raRecordList)
                            {
                                LearnerRACourse ra = new LearnerRACourse();
                                ra.CourseId = Convert.ToInt64(item.intCourseID);
                                ra.CourseName = item.strCourse;
                                ra.IssueCount = Convert.ToInt32(item.intIssueCount);
                                ra.RACompletedOn = item.datCompleted == null ? "" : (Convert.ToDateTime(item.datCompleted)).ToString("dd-MMM-yyyy");
                                ra.RAStartedOn = item.datBegun == null ? "" : (Convert.ToDateTime(item.datBegun)).ToString("dd-MMM-yyyy");
                                ra.RASignOffDate = item.datSignOff == null ? "" : (Convert.ToDateTime(item.datSignOff)).ToString("dd-MMM-yyyy");
                                ra.RAResultID = Convert.ToInt32(item.intRiskAssessmentResultID);

                                RAResultRecord.Add(ra);
                            }
                            trainingCard.RAResult = RAResultRecord;
                        }

                        if (docResultList != null && docResultList.Count > 0)
                        {
                            foreach (var item in docResultList)
                            {
                                TraineeDocResult docProgress = new TraineeDocResult();
                                docProgress.DocName = item.docName;
                                docProgress.ViewedOn = item.viewedOn == null ? "" : (Convert.ToDateTime(item.viewedOn)).ToString("dd-MMM-yyyy");

                                DocResultRecord.Add(docProgress);
                            }
                            trainingCard.DocResult = DocResultRecord;
                        }
                    }
                }
                return trainingCard;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Learner assigned course report

        public AssignedCourseReport GetLearnerAssignedCourseReport(LearnerReportFilter searchCriteria)
        {
            try
            {
                AssignedCourseReport assignedCourseReport = new AssignedCourseReport();
                List<AssignedCourseItem> assignedCourseRecord = new List<AssignedCourseItem>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_get_LearnerAssignedCourses(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.SearchText, 
                        searchCriteria.Location, searchCriteria.Department, searchCriteria.Course, searchCriteria.Company, searchCriteria.Status, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        assignedCourseReport.TotalRecords = resultList.Count();
                        var data = resultList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            AssignedCourseItem record = new AssignedCourseItem();
                            record.UserID = Convert.ToInt64(item.intContactID);
                            record.FirstName = item.strFirstName;
                            record.LastName = item.strSurname;
                            record.EmployeeNumber = item.strEmployeeNumber;
                            record.EmailId = item.strEmail;
                            record.Location = item.strLocation;
                            record.Department = item.strDepartment;
                            record.CourseName = item.strCourse;
                            record.Course = Convert.ToInt64(item.intCourseId);
                            record.ProgressRecordID = Convert.ToInt64(item.intRecordId);
                            record.AssignedOn = item.dateAssignedOn == null ? "" : (Convert.ToDateTime(item.dateAssignedOn)).ToString("dd-MMM-yyyy");
                            record.CompletionDate = item.dateCompletedOn == null ? "" : (Convert.ToDateTime(item.dateCompletedOn)).ToString("dd-MMM-yyyy");
                            record.CourseStatus = item.strStatus;
                            assignedCourseRecord.Add(record);
                        }
                    }
                }
                assignedCourseReport.AssignedRecords = assignedCourseRecord;
                return assignedCourseReport;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region Training reset fixed calendar

        public DateTime? GetTrainingResetDate(Int64 org)
        {
            DateTime? resetDate = null;
            try
            {
                using (var context = new lmsdbEntities())
                {
                    resetDate = context.lms_admin_get_TrainingResetDate(org).FirstOrDefault();
                }
                return resetDate;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int UpdateTrainingResetDate(Int64 organisationID, string resetDate, string otp, Guid otp_txn)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_update_TrainingResetDate(organisationID, resetDate, otp, otp_txn, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Training Summary Report
        public List<CourseLearningStatistics> GetLearningStatistics(Int64 org)
        {
            try
            {
                List<CourseLearningStatistics> progressRecord = new List<CourseLearningStatistics>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_get_AllCourses_summaryReport(org).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        foreach (var item in resultList)
                        {
                            CourseLearningStatistics progress = new CourseLearningStatistics();
                            progress.CourseID = Convert.ToInt64(item.intCourseID);
                            progress.Course = item.strCourse;
                            progress.Assigned = Convert.ToInt32(item.TotalAssignments);
                            progress.Completed = Convert.ToInt32(item.TotalCompleted);
                            progress.Inprogress = Convert.ToInt32(item.TotalInProgress);
                            progress.Notstarted = Convert.ToInt32(item.TotalNotStarted);

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
        public List<CourseLearningStatistics_perLocation> GetLearningStatistics_perLocation(Int64 org, int course)
        {
            try
            {
                List<CourseLearningStatistics_perLocation> progressRecord = new List<CourseLearningStatistics_perLocation>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_get_completionPerLocation_summaryReport(org, course).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        foreach (var item in resultList)
                        {
                            CourseLearningStatistics_perLocation progress = new CourseLearningStatistics_perLocation();
                            progress.Location = item.LocationName;
                            progress.LocationId = Convert.ToInt32(item.intLocationID);
                            progress.Completed = Convert.ToInt32(item.TotalCompletions);

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
        public List<CourseLearningStatistics_perDepartment> GetLearningStatistics_perDepartment(int location, int course)
        {
            try
            {
                List<CourseLearningStatistics_perDepartment> progressRecord = new List<CourseLearningStatistics_perDepartment>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_get_completionPerDepartment_summaryReport(location, course).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        foreach (var item in resultList)
                        {
                            CourseLearningStatistics_perDepartment progress = new CourseLearningStatistics_perDepartment();
                            progress.Department = item.DepartmentName;
                            progress.Assigned = Convert.ToInt32(item.TotalAssignments);
                            progress.Completed = Convert.ToInt32(item.TotalCompleted);
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

        #region widget report

        public WidgeteProgressReport GetWidgetReport(LearnerReportFilter searchCriteria)
        {
            try
            {
                WidgeteProgressReport progressReport = new WidgeteProgressReport();
                List<WidgetProgressItem> progressRecord = new List<WidgetProgressItem>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_get_widgetReport(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.SearchText, searchCriteria.UserStatus, searchCriteria.Location, searchCriteria.Department, searchCriteria.Course, searchCriteria.Company, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        progressReport.TotalRecords = resultList.Count();
                        var data = resultList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            WidgetProgressItem progress = new WidgetProgressItem();
                            progress.UserID = Convert.ToInt64(item.intContactID);
                            progress.FirstName = item.strFirstName;
                            progress.LastName = item.strSurname;
                            progress.EmployeeNumber = item.strEmployeeNumber;
                            progress.EmailId = item.strEmail;
                            progress.Location = item.strLocation;
                            progress.Department = item.strDepartment;
                            progress.CourseName = item.strCourse;
                            progress.QuesType = Convert.ToInt32(item.qtype);
                            progress.Question = item.widget_que_text;
                            progress.AfterQuestion = item.widget_que_text_after;
                            progress.Response = item.resp_text;
                            progress.AfterResponse = item.resp_text_after;
                            progress.Response_1 = item.resp_mac_1;
                            progress.Response_2 = item.resp_mac_2;
                            progress.Response_3 = item.resp_mac_3;  
                            progress.FeedBackResponse = item.resp_mac_feedback;
                            progress.FeedBackResponseText = item.resp_mac_feedback_text;
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
        public List<WidgetProgressItem> DownloadWidgetReport(LearnerReportFilter searchCriteria)
        {
            try
            {
                List<WidgetProgressItem> progressRecord = new List<WidgetProgressItem>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_get_widgetReport(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.SearchText, searchCriteria.UserStatus, searchCriteria.Location, searchCriteria.Department, searchCriteria.Course, searchCriteria.Company, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        foreach (var item in resultList)
                        {
                            WidgetProgressItem progress = new WidgetProgressItem();
                            progress.UserID = Convert.ToInt64(item.intContactID);
                            progress.FirstName = item.strFirstName;
                            progress.LastName = item.strSurname;
                            progress.EmployeeNumber = item.strEmployeeNumber;
                            progress.EmailId = item.strEmail;
                            progress.Location = item.strLocation;
                            progress.Department = item.strDepartment;
                            progress.CourseName = item.strCourse;
                            progress.QuesType = Convert.ToInt32(item.qtype);
                            progress.Question = item.widget_que_text;
                            progress.AfterQuestion = item.widget_que_text_after;
                            progress.Response = item.resp_text;
                            progress.AfterResponse = item.resp_text_after;
                            progress.Response_1 = item.resp_mac_1;
                            progress.Response_2 = item.resp_mac_2;
                            progress.Response_3 = item.resp_mac_3;
                            progress.FeedBackResponse = item.resp_mac_feedback;
                            progress.FeedBackResponseText = item.resp_mac_feedback_text;
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

        #region sub-module progress report

        public CourseSubModuleProgressReport GetSubModuleProgressReport(LearnerSubModuleReportFilter searchCriteria)
        {
            try
            {
                CourseSubModuleProgressReport progressReport = new CourseSubModuleProgressReport();
                List<CourseSubModuleProgressItem> progressRecord = new List<CourseSubModuleProgressItem>();

                using (var context = new lmsdbEntities())
                {
                    var resultList = context.lms_admin_getLearnerProgressReport(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.SearchText, searchCriteria.UserStatus, searchCriteria.Location, searchCriteria.Department, searchCriteria.Status, searchCriteria.Course, searchCriteria.Company, searchCriteria.SortCol, searchCriteria.SortColDir, searchCriteria.FromDate, searchCriteria.ToDate, searchCriteria.AccessStatus).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        progressReport.TotalRecords = resultList.Count();
                        var data = resultList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            CourseSubModuleProgressItem progress = new CourseSubModuleProgressItem();
                            progress.UserID = Convert.ToInt64(item.intContactID);
                            progress.FirstName = item.strFirstName;
                            progress.LastName = item.strSurname;
                            progress.EmployeeNumber = item.strEmployeeNumber;
                            progress.EmailId = item.strEmail;
                            progress.Location = item.strLocation;
                            progress.Department = item.strDepartment;
                            progress.CourseName = item.strCourse;
                            progress.Course = Convert.ToInt64(item.intCourseId);
                            progress.RecordId = Convert.ToInt64(item.intRecordID);
                            progress.CourseStatus = item.strStatus;
                            progress.Score = Convert.ToInt32(item.intScore);
                            progress.AssignedOn = item.dateAssignedOn == null ? "" : (Convert.ToDateTime(item.dateAssignedOn)).ToString("dd-MMM-yyyy");
                            progress.CompletionDate = item.dateCompletedOn == null ? "" : (Convert.ToDateTime(item.dateCompletedOn)).ToString("dd-MMM-yyyy");
                            progress.LastAccessedDate = item.dateLastStarted == null ? "" : (Convert.ToDateTime(item.dateLastStarted)).ToString("dd-MMM-yyyy");
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
        #endregion
    }
}
