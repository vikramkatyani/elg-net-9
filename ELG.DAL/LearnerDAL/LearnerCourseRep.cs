using System;
using System.Collections.Generic;
using System.Linq;
using ELG.Model.Learner;
using ELG.DAL.DbEntityLearner;
using System.Data.Entity.Core.Objects;

namespace ELG.DAL.LearnerDAL
{
    public class LearnerCourseRep
    {
        /// <summary>
        /// Get list of all modules assigned to a learner
        /// </summary>
        /// <param name="course"></param>
        /// <returns></returns>
        public List<LearnerAssignedCourse> GetLearnerCourses(string coursename, Int64 learner, string sort)
        {
            try
            {
                List<LearnerAssignedCourse> courses = new List<LearnerAssignedCourse>();

                using (learnerDBEntities context = new learnerDBEntities())
                {
                    // Pass NULL to SP if parameters are empty (SP may filter differently on empty string vs NULL)
                    var learnerCourseList = context.lms_learner_getAllCourses(learner, string.IsNullOrWhiteSpace(coursename) ? null : coursename, string.IsNullOrWhiteSpace(sort) ? null : sort).ToList();
                    if (learnerCourseList != null && learnerCourseList.Count > 0)
                    {
                        foreach (var item in learnerCourseList)
                        {
                            LearnerAssignedCourse course = new LearnerAssignedCourse();
                            course.CourseId = item.intcourseid;
                            course.ProgressRecordId = Convert.ToInt64(item.intrecordid);
                            course.CourseName = item.strcourse;
                            course.CourseDesc = item.strcoursedescription;
                            course.CourseSummary = item.strcoursesummary;
                            course.CourseLogo = item.strcourselogo;
                            course.CoursePath = item.strcoursetutorialpath;
                            course.IsCourseRA = item.blncoursera;
                            course.TestFrequency = item.inttestfrequency;
                            course.IsExpired = item.iscourseexpired;
                            course.SelfCourseResetEnabled = Convert.ToBoolean(item.SelfCourseResetEnabled);
                            course.SubModuleCount = Convert.ToInt32(item.subModuleCount);

                            if (item.subModuleCount > 0)
                            {
                                var subModuleList = context.lms_learner_get_course_submodules(item.intcourseid, learner).ToList();
                                course.SubModuleList = new List<SubModule>();
                                if (subModuleList != null && subModuleList.Count > 0)
                                {
                                    foreach (var sub in subModuleList)
                                    {
                                        SubModule subModule = new SubModule();
                                        subModule.SubModuleID = Convert.ToInt64(sub.smid);
                                        subModule.CourseId = Convert.ToInt64(sub.courseId);
                                        subModule.RAID = Convert.ToInt32(sub.RAID);
                                        subModule.SubModuleName = sub.name;
                                        subModule.SubModuleDesc = sub.descrip;
                                        subModule.SubModulePath = sub.path;
                                        subModule.Sequence = Convert.ToInt32(sub.seq);
                                        subModule.SubModuleStatus = sub.strStatus;
                                        subModule.SubModuleAccessDate = sub.lastAccessedOn == null ? "" : (Convert.ToDateTime(sub.lastAccessedOn)).ToString("dd-MMM-yyyy");
                                        course.SubModuleList.Add(subModule);
                                    }
                                }
                            }

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

                            courses.Add(course);
                        }
                    }
                }
                return courses;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// Check if there are any courses with auto assignment with no available licences
        /// </summary>
        /// <param name="learner"></param>
        /// <param name="location"></param>
        /// <param name="department"></param>
        /// <returns></returns>
        public bool GetAutoCoursesWithNoLicence(Int64 learner)
        {
            try
            {
                bool courseWithNoLicences = false;

                using (learnerDBEntities context = new learnerDBEntities())
                {
                    var learnerCourseList = context.lms_learner_check_unassignedCoursesWithNoLicences(learner).ToList();
                    if (learnerCourseList != null && learnerCourseList.Count > 0)
                    {
                        courseWithNoLicences = true;
                    }
                }
                return courseWithNoLicences;
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Get list of all modules assigned to a learner
        /// </summary>
        /// <param name="course"></param>
        /// <returns></returns>
        public CourseProgressHistoryList GetLearnerCourseHistory(DataTableFilter searchCriteria)
        {
            try
            {
                CourseProgressHistoryList historyList = new CourseProgressHistoryList();
                List<CourseProgressHistoryData> progressItems = new List<CourseProgressHistoryData>();

                using (learnerDBEntities context = new learnerDBEntities())
                {
                    var learnerCourseList = context.lms_learner_get_progress_history(searchCriteria.Learner, searchCriteria.Course).ToList();
                    if (learnerCourseList != null && learnerCourseList.Count > 0)
                    {
                        historyList.TotalRecords = learnerCourseList.Count();
                        //var data = learnerCourseList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in learnerCourseList)
                        {
                            CourseProgressHistoryData progressItem = new CourseProgressHistoryData();
                            progressItem.UserId = searchCriteria.Learner;
                            progressItem.CourseId = Convert.ToInt64(item.intcourseid);
                            progressItem.RecordId = Convert.ToInt64(item.intrecordid);
                            progressItem.Course = item.strcourse;

                            if (item.strstatus == null)
                                progressItem.ProgressStatus = "NOT STARTED";
                            else if (item.strstatus.ToLower() == "started")
                                progressItem.ProgressStatus = "IN-PROGRESS";
                            else if (item.strstatus.ToLower() == "incomplete")
                                progressItem.ProgressStatus = "IN-PROGRESS";
                            else
                                progressItem.ProgressStatus = item.strstatus.ToUpper();

                            progressItem.Score = Convert.ToInt32(item.intscore);
                            progressItem.AssignedOn = item.dateassignedon == null ? "" : (Convert.ToDateTime(item.dateassignedon)).ToString("dd-MMM-yyyy");
                            progressItem.CompletedOn = item.datecompletedon == null ? "" : (Convert.ToDateTime(item.datecompletedon)).ToString("dd-MMM-yyyy");

                            progressItems.Add(progressItem);
                        }
                    }
                }
                historyList.History = progressItems;
                return historyList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region Certificate
        public LearnerCertificateRecord GetCertificateRecord(Int64 recordid)
        {
            try
            {
                LearnerCertificateRecord progress = new LearnerCertificateRecord();

                using (var context = new learnerDBEntities())
                {
                    var item = context.lms_learner_get_certificate_record(recordid).FirstOrDefault();
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
        #endregion

        /// <summary>
        /// Get list of all risk assessment modules assigned to a learner
        /// </summary>
        /// <param name="course"></param>
        /// <returns></returns>
        public LearnerRACourseList GetLearnerRA(DataTableFilter searchCriteria)
        {
            try
            {
                LearnerRACourseList raList = new LearnerRACourseList();
                List<LearnerRACourse> raInfoList = new List<LearnerRACourse>();

                using (var context = new learnerDBEntities())
                {
                    var raListdb = context.lms_learner_getAllRACourses(searchCriteria.Learner, searchCriteria.Organisation, searchCriteria.SearchText).ToList();
                    if (raListdb != null && raListdb.Count > 0)
                    {
                        raList.TotalCount = raListdb.Count();
                        var data = raListdb.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            LearnerRACourse ra = new LearnerRACourse();
                            ra.CourseId = Convert.ToInt64(item.intCourseID);
                            ra.CourseName = item.strCourse;
                            ra.IssueCount = Convert.ToInt32(item.intIssueCount);
                            ra.RACompletedOn = item.datCompleted == null ? "" : (Convert.ToDateTime(item.datCompleted)).ToString("dd-MMM-yyyy");
                            ra.RAStartedOn = item.datBegun == null ? "" : (Convert.ToDateTime(item.datBegun)).ToString("dd-MMM-yyyy");
                            ra.RASignOffDate = item.datSignOff == null ? "" : (Convert.ToDateTime(item.datSignOff)).ToString("dd-MMM-yyyy");
                            ra.RAResultID = Convert.ToInt32(item.intRiskAssessmentResultID);
                            raInfoList.Add(ra);
                        }
                    }
                }
                raList.RAList = raInfoList;
                return raList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public LearnerRACourseList GetLearnerRAAttempts(RAAttemptFilter searchCriteria)
        {
            try
            {
                LearnerRACourseList raList = new LearnerRACourseList();
                List<LearnerRACourse> raInfoList = new List<LearnerRACourse>();

                using (var context = new learnerDBEntities())
                {
                    var raListdb = context.lms_learner_get_RA_attempts(searchCriteria.Learner, searchCriteria.RACourseID, searchCriteria.RASubModuleID).ToList();
                    if (raListdb != null && raListdb.Count > 0)
                    {
                        raList.IncompleteRACount = 0;
                        raList.TotalCount = raListdb.Count();
                        var data = raListdb.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();
                        foreach (var item in data)
                        {
                            LearnerRACourse ra = new LearnerRACourse();
                            ra.CourseName = item.strCourse;
                            ra.LocationName = item.strLocationName;
                            ra.IssueCount = Convert.ToInt32(item.intIssueCount);
                            ra.RACompletedOn = item.datCompleted == null ? "" : (Convert.ToDateTime(item.datCompleted)).ToString("dd-MMM-yyyy");
                            if(item.datCompleted == null)
                            {
                                raList.IncompleteRACount++;
                            }
                            ra.RAStartedOn = item.datBegun == null ? "" : (Convert.ToDateTime(item.datBegun)).ToString("dd-MMM-yyyy");
                            ra.RASignOffDate = item.datSignOff == null ? "" : (Convert.ToDateTime(item.datSignOff)).ToString("dd-MMM-yyyy");
                            ra.RAResultID = Convert.ToInt32(item.intRiskAssessmentResultID);
                            raInfoList.Add(ra);
                        }
                    }
                }
                raList.RAList = raInfoList;
                return raList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get all risk assessment questions
        /// </summary>
        /// <param name="learnerRA"></param>
        /// <returns></returns>
        public List<RiskAssessmentQuestion> GetLearnerRAQuestions(RiskAssessment learnerRA)
        {
            try
            {
                List<RiskAssessmentQuestion> questions = new List<RiskAssessmentQuestion>();

                using (learnerDBEntities context = new learnerDBEntities())
                {
                    var learnerQueList = context.lms_learner_getRAQuestion(learnerRA.LearnerID, learnerRA.CourseId).ToList();
                    if (learnerQueList != null && learnerQueList.Count > 0)
                    {
                        int queId = 0;
                        RiskAssessmentQuestion que = new RiskAssessmentQuestion();
                        List<RiskAssessmentQuestionOption> optionList = new List<RiskAssessmentQuestionOption>();
                        foreach (var item in learnerQueList)
                        {
                            if (queId > 0 && queId != item.intQuestionID)
                                questions.Add(que);

                            if(queId != item.intQuestionID)
                            {
                                queId = Convert.ToInt32(item.intQuestionID);
                                que = new RiskAssessmentQuestion();
                                optionList = new List<RiskAssessmentQuestionOption>();
                                que.CourseId = learnerRA.CourseId;
                                que.GroupText = item.strGroupText;
                                que.LearnerID = learnerRA.LearnerID;
                                que.Order = Convert.ToInt32(item.intOrder);
                                que.QuestionId = Convert.ToInt64(item.intQuestionID);
                                que.QuestionText = item.strQuestion;    
                                que.AnswerID = Convert.ToInt64(item.intanswerid);                            
                                
                            }
                            else
                            {
                                que.CourseId = learnerRA.CourseId;
                                que.GroupText = item.strGroupText;
                                que.LearnerID = learnerRA.LearnerID;
                                que.Order = Convert.ToInt32(item.intOrder);
                                que.QuestionId = Convert.ToInt64(item.intQuestionID);
                                que.QuestionText = item.strQuestion;
                                que.AnswerID = Convert.ToInt64(item.intanswerid);
                            }

                            RiskAssessmentQuestionOption option = new RiskAssessmentQuestionOption();
                            option.Issue = Convert.ToBoolean(item.optIssue);
                            option.OptionText = item.strValue;
                            option.Order = Convert.ToInt32(item.optOrder);
                            option.QuestionOptionId = Convert.ToInt32(item.intQuestionOptionID);
                            optionList.Add(option);

                            que.Options = optionList;
                        }
                        // adding last question
                        questions.Add(que);
                    }
                }
                return questions;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get all risk assessment questions
        /// </summary>
        /// <param name="learnerRA"></param>
        /// <returns></returns>
        public List<RiskAssessmentQuestion> GetLearnerSubModuleRAQuestions(RiskAssessmentRecord learnerRA)
        {
            try
            {
                List<RiskAssessmentQuestion> questions = new List<RiskAssessmentQuestion>();

                using (learnerDBEntities context = new learnerDBEntities())
                {
                    var learnerQueList = context.lms_learner_get_subModule_RAQuestion(learnerRA.LearnerID, learnerRA.RiskAssessmentResultID, learnerRA.SubModuleId).ToList();
                    if (learnerQueList != null && learnerQueList.Count > 0)
                    {
                        int queId = 0;
                        RiskAssessmentQuestion que = new RiskAssessmentQuestion();
                        List<RiskAssessmentQuestionOption> optionList = new List<RiskAssessmentQuestionOption>();
                        foreach (var item in learnerQueList)
                        {
                            if (queId > 0 && queId != item.intQuestionID)
                                questions.Add(que);

                            if (queId != item.intQuestionID)
                            {
                                queId = Convert.ToInt32(item.intQuestionID);
                                que = new RiskAssessmentQuestion();
                                optionList = new List<RiskAssessmentQuestionOption>();
                                que.CourseId = learnerRA.CourseId;
                                que.GroupText = item.strGroupText;
                                que.LearnerID = learnerRA.LearnerID;
                                que.Order = Convert.ToInt32(item.intOrder);
                                que.QuestionId = Convert.ToInt64(item.intQuestionID);
                                que.QuestionText = item.strQuestion;
                                que.AnswerID = Convert.ToInt64(item.intanswerid);

                            }
                            else
                            {
                                que.CourseId = learnerRA.CourseId;
                                que.GroupText = item.strGroupText;
                                que.LearnerID = learnerRA.LearnerID;
                                que.Order = Convert.ToInt32(item.intOrder);
                                que.QuestionId = Convert.ToInt64(item.intQuestionID);
                                que.QuestionText = item.strQuestion;
                                que.AnswerID = Convert.ToInt64(item.intanswerid);
                            }

                            RiskAssessmentQuestionOption option = new RiskAssessmentQuestionOption();
                            option.Issue = Convert.ToBoolean(item.optIssue);
                            option.OptionText = item.strValue;
                            option.Order = Convert.ToInt32(item.optOrder);
                            option.QuestionOptionId = Convert.ToInt32(item.intQuestionOptionID);
                            optionList.Add(option);

                            que.Options = optionList;
                            que.RiskAssessmentResultID = item.RARID.HasValue ? Convert.ToInt64(item.RARID.Value) : 0;
                        }
                        // adding last question
                        questions.Add(que);
                    }
                }
                return questions;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// Get all risk assessment questions
        /// </summary>
        /// <param name="learnerRA"></param>
        /// <returns></returns>
        public List<RiskAssessmentResponseReview> GetLearnerRAResponses(RAResponseFilter learnerRA)
        {
            try
            {
                List<RiskAssessmentResponseReview> responses = new List<RiskAssessmentResponseReview>();

                using (learnerDBEntities context = new learnerDBEntities())
                {
                    var learnerResponseList = context.lms_learner_getRAResponses(learnerRA.Learner, learnerRA.RACourseID).ToList();
                    if (learnerResponseList != null && learnerResponseList.Count > 0)
                    {
                        foreach (var item in learnerResponseList)
                        {
                            RiskAssessmentResponseReview rev = new RiskAssessmentResponseReview();
                            rev.Answer = item.strQuestionOption;
                            rev.Issue = item.blnIssue;
                            rev.Notes = item.strText;
                            rev.Question = item.strQuestion;

                            responses.Add(rev);
                        }
                    }
                }
                return responses;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<RiskAssessmentResponseReview> GetLearnerSubModuleRAResponses(RAResponseFilter learnerRA)
        {
            try
            {
                List<RiskAssessmentResponseReview> responses = new List<RiskAssessmentResponseReview>();

                using (learnerDBEntities context = new learnerDBEntities())
                {
                    var learnerResponseList = context.lms_learner_get_SubModuleRAResponses(learnerRA.Learner, learnerRA.RAResultID).ToList();
                    if (learnerResponseList != null && learnerResponseList.Count > 0)
                    {
                        foreach (var item in learnerResponseList)
                        {
                            RiskAssessmentResponseReview rev = new RiskAssessmentResponseReview();
                            rev.Answer = item.strQuestionOption;
                            rev.Issue = item.blnIssue;
                            rev.Notes = item.strText;
                            rev.Question = item.strQuestion;

                            responses.Add(rev);
                        }
                    }
                }
                return responses;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// to set RA as started
        /// </summary>
        /// <param name="learnerRA"></param>
        /// <returns></returns>
        public int StartRiskAssessment(RiskAssessment learnerRA)
        {
            int success = 0;
            try
            {
                using (var context = new learnerDBEntities())
                {
                    var result = context.lms_learner_startRiskAssessment(learnerRA.LearnerID, learnerRA.CourseId);
                    success = 1;
                }
            }
            catch (Exception)
            {

                throw;
            }
            return success;
        }
        public int StartSubModuleRiskAssessment(RiskAssessment learnerRA)
        {
            int success = 0;
            try
            {
                using (var context = new learnerDBEntities())
                {
                    var result = context.lms_learner_start_subModuleRiskAssessment(learnerRA.LearnerID, learnerRA.RiskAssessmentResultID);
                    success = 1;
                }
            }
            catch (Exception)
            {

                throw;
            }
            return success;
        }

        /// <summary>
        /// set ra as completed
        /// </summary>
        /// <param name="learnerRA"></param>
        /// <returns></returns>
        public int FinishtRiskAssessment(RiskAssessment learnerRA)
        {
            int success = 0;
            try
            {
                using (var context = new learnerDBEntities())
                {
                    var result = context.lms_learner_FinishRiskAssessment(learnerRA.LearnerID, learnerRA.CourseId);
                    success = 1;
                }
            }
            catch (Exception)
            {

                throw;
            }
            return success;

        }
        public int FinishSubModuleRiskAssessment(RiskAssessment learnerRA)
        {
            int success = 0;
            try
            {
                using (var context = new learnerDBEntities())
                {
                    var result = context.lms_learner_finish_SubModuleRiskAssessment(
                        learnerRA.LearnerID, 
                        learnerRA.RiskAssessmentResultID,
                        learnerRA.StrLocationName);
                    success = 1;
                }
            }
            catch (Exception)
            {

                throw;
            }
            return success;

        }

        /// <summary>
        /// to save ra responses
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public int SaveRAResponse(RiskAssessmentResponse response)
        {
            int success = 0;
            try
            {
                using (var context = new learnerDBEntities())
                {
                    var result = context.lms_learner_createRAResponse(response.AnswerId, response.OptionId, response.IssueText);
                    success = 1;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return success;

        }

        /// <summary>
        /// Function to create new document category in an organisation
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public int SaveRAEvidence(RiskAssessmentEvidence evidence)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("id", typeof(long));
                using (var context = new learnerDBEntities())
                {
                    var result = context.lms_learner_create_RA_AnswerEvidence(evidence.AnswerId, evidence.EvidencePath, retVal);
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
        /// Reset learning record
        /// </summary>
        /// <param name="Learner"></param>
        /// <param name="Course"></param>
        /// <param name="RecordId"></param>
        /// <returns></returns>
        public int ResetLearningProgress(Int64 Learner, Int64 Course, Int64 RecordId)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("res", typeof(int));
                using (var context = new learnerDBEntities())
                {
                    var result = context.lms_learner_resetProgress(Learner, Course, RecordId, retVal);
                    success = Convert.ToInt32(retVal.Value);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return success;
        }
        public int UpdateSubModuleProgress(Int64 Learner, Int64 Course)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("res", typeof(int));
                using (var context = new learnerDBEntities())
                {
                    var result = context.lms_learner_updateSubModuleProgressRecord(Learner, Course);
                    success = 1;
                }

            }
            catch (Exception)
            {
                throw;
            }
            return success;
        }
    }
}
