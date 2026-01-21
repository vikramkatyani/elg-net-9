using ELG.DAL.DBEntitySA;
using ELG.Model.SuperAdmin;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;

namespace ELG.DAL.SuperAdminDAL
{
    public class CHSERARep
    {
        #region Risk Assessment List
        /// <summary>
        /// Get CHSE Risk assessment listing
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public CHSERiskAssessmentListing GetCHSERAListing(ModuleListingSearch searchCriteria)
        {
            try
            {
                CHSERiskAssessmentListing moduleList = new CHSERiskAssessmentListing();
                List<CHSERiskAssessment> moduleInfoList = new List<CHSERiskAssessment>();

                using (var context = new superadmindbEntities())
                {
                    var moduleListData = context.lms_superadmin_get_RAListing(searchCriteria.ModuleName, searchCriteria.Status, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (moduleListData != null && moduleListData.Count > 0)
                    {
                        moduleList.TotalRecords = moduleListData.Count();
                        var data = moduleListData.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            CHSERiskAssessment mod = new CHSERiskAssessment();
                            mod.RAID = Convert.ToInt64(item.intCourseID);
                            mod.RAName = item.strCourse;
                            mod.Status = Convert.ToBoolean(item.isArchived) ? "Archived" : "Active";
                            moduleInfoList.Add(mod);
                        }
                    }
                }
                moduleList.RAList = moduleInfoList;
                return moduleList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region Create Risk Assessment
        /// <summary>
        /// Funtion to create new risk assessment
        /// </summary>
        /// <param name="ra"></param>
        /// <returns></returns>
        public int CreateNewRiskAssessment(CHSERiskAssessment ra)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("id", typeof(long));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_create_NewRiskAssessment(ra.RAName, ra.RASummary, ra.RADescription, retVal);
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

        #region Update Risk Assessment
        /// <summary>
        /// get risk assessment info
        /// </summary>
        /// <param name="raid"></param>
        /// <returns></returns>
        public CHSERiskAssessment GetRiskAssessmentInfo(Int64 raid)
        {
            try
            {
                CHSERiskAssessment ra = new CHSERiskAssessment();
                using (var context = new superadmindbEntities())
                {
                    var raInfo = context.lms_superadmin_get_CourseInfo(raid).FirstOrDefault();
                    if (raInfo != null)
                    {
                        ra.RAName = raInfo.strCourse;
                        ra.RASummary = raInfo.strCourseSummary;
                        ra.RADescription = raInfo.strCourseDescription;
                    }
                }
                return ra;
            }
            catch (Exception)
            {

                throw;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="raid"></param>
        /// <returns></returns>
        public int UpdateRiskAssessmentInfo(CHSERiskAssessment ra)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_update_CourseInfo(ra.RAID, ra.RAName, ra.RASummary, ra.RADescription, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region risk assessment group

        /// <summary>
        /// Create risk assessment group
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public int CreateRiskAssessmentGroup(CHSERiskAssessmentGroup group)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("id", typeof(long));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_create_NewRiskAssessmentGroup(group.GroupName, group.RAId, retVal);
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
        /// Get CHSE Risk assessment listing
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public CHSERiskAssessmentGroupListing GetCHSERAGroupListing(RAGroupListingSearch searchCriteria)
        {
            try
            {
                CHSERiskAssessmentGroupListing groupList = new CHSERiskAssessmentGroupListing();
                List<CHSERiskAssessmentGroup> groupInfoList = new List<CHSERiskAssessmentGroup>();

                using (var context = new superadmindbEntities())
                {
                    var groupListData = context.lms_superadmin_get_GroupsOfRA(searchCriteria.RAId, searchCriteria.GroupName).ToList();
                    if (groupListData != null && groupListData.Count > 0)
                    {
                        groupList.TotalRecords = groupListData.Count();
                        var data = groupListData.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            CHSERiskAssessmentGroup mod = new CHSERiskAssessmentGroup();
                            mod.GroupId = Convert.ToInt64(item.intRAGroupID);
                            mod.RAId = Convert.ToInt64(item.intRACourseID);
                            mod.GroupName = item.strRAGroup;
                            groupInfoList.Add(mod);
                        }
                    }
                }
                groupList.RAGroupList = groupInfoList;
                return groupList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="raid"></param>
        /// <returns></returns>
        public int UpdateRiskAssessmentGroup(CHSERiskAssessmentGroup group)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_update_RiskAssessmentGroup(group.RAId, group.GroupName, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region risk assessment questions

        public List<CHSERiskAssessmentQuestion> GetModuleRiskAssessmentQuestion(Int64 raid)
        {
            List<CHSERiskAssessmentQuestion> questionList = new List<CHSERiskAssessmentQuestion>();
            try
            {
                using (var context = new superadmindbEntities())
                {
                    var resultList = context.lms_superadmin_get_AllRACourseQuestions(raid).ToList();
                    if (resultList != null && resultList.Count > 0)
                    {
                        foreach (var item in resultList)
                        {
                            CHSERiskAssessmentQuestion ques = new CHSERiskAssessmentQuestion();
                            ques.QuestionId = item.intQuestionID;
                            ques.QuestionText = item.strQuestion;
                            ques.QuestionType = item.intQuestionTypeID;
                            ques.Order = Convert.ToInt32(item.intOrder);
                            ques.GotoNO = Convert.ToInt32(item.intGotoNo);
                            ques.GotoYes = Convert.ToInt32(item.intGotoYes);
                            ques.NumberFrom = Convert.ToInt32(item.intNumberFrom);
                            ques.NumberTo = Convert.ToInt32(item.intNumberTo);
                            ques.FreeText = item.blnFreeText;
                            ques.MultiLine = item.blnMultiLine;
                            ques.End = item.blnEnd;
                            ques.BarChart = item.blnBarChart;
                            ques.BaseQuestion = item.blnBaseQuestion;
                            ques.Group = item.strRAGroup;
                            ques.CourseName = item.strCourse;
                            ques.OptionCount = Convert.ToInt32(item.OptionCount);

                            questionList.Add(ques);
                        }
                    }
                }
                return questionList;
            }
            catch (Exception)
            {

                throw;
            }
        }
        
        /// <summary>
        /// To get list of all group for a RA course
        /// </summary>
        /// <param name="courseID"></param>
        /// <returns></returns>
        public List<CHSERiskAssessmentQuestionOption> GetRAQuestionOptionList(Int64 queId)
        {
            try
            {
                List<CHSERiskAssessmentQuestionOption> optionList = new List<CHSERiskAssessmentQuestionOption>();

                using (var context = new superadmindbEntities())
                {
                    var oList = context.lms_superadmin_get_AllOptionsForQuestion(queId).ToList();
                    if (oList != null && oList.Count > 0)
                    {
                        foreach (var item in oList)
                        {
                            CHSERiskAssessmentQuestionOption option = new CHSERiskAssessmentQuestionOption();
                            option.Issue = item.blnIssue;
                            option.OptionText = item.strQuestionOption;
                            option.Order = item.intOrder;
                            option.QuestionId = queId;
                            option.QuestionOptionId = item.intQuestionOptionID;
                            optionList.Add(option);
                        }
                    }
                }
                return optionList;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// Create new question for Risk assessment course
        /// </summary>
        /// <param name="question"></param>
        /// <returns></returns>
        public int CreateNewRAQuestion(CHSERiskAssessmentQuestion question)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("id", typeof(long));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_create_RAQue(question.QuestionText, question.QuestionAdditionalText, question.GroupId, question.Group, question.CourseId, retVal);
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
        /// Create new Risk assessment question option
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public int CreateNewRAQuestionOption(CHSERiskAssessmentQuestionOption option)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("id", typeof(long));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_create_RAQueOption(option.OptionText, option.QuestionId, option.Order, option.Issue, retVal);
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
        /// remove risk assessment question
        /// </summary>
        /// <param name="question"></param>
        /// <returns></returns>
        public int RemoveRiskAssessmentQuestion(Int64 question)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("id", typeof(long));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_delete_RAQuestion(question, retVal);
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
        /// get risk assessment question details
        /// </summary>
        /// <param name="question"></param>
        /// <returns></returns>
        public CHSERiskAssessmentQuestion GetRiskAssessmentQuestionInfo(Int64 question)
        {
            CHSERiskAssessmentQuestion que = new CHSERiskAssessmentQuestion();
            try
            {
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_get_RAQuestionInfo(question).FirstOrDefault();
                    if (result != null)
                    {
                        que.QuestionId = result.intQuestionID;
                        que.QuestionText = result.strQuestion;
                        que.Order = Convert.ToInt32(result.intOrder);
                        que.GroupId = Convert.ToInt32(result.intGroupID);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return que;
        }

        /// <summary>
        /// delete option from ra question
        /// </summary>
        /// <param name="optionId"></param>
        /// <returns></returns>
        public int RemoveRiskAssessmentQuestionOption(Int64 optionId)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("id", typeof(long));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_delete_RAQuestionOption(optionId, retVal);
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
        /// Update risk assessment question option
        /// </summary>
        /// <param name="ques"></param>
        /// <returns></returns>
        public int UpdateRiskAssessmentQuestionOption(CHSERiskAssessmentQuestionOption option)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(long));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_update_RAQuestionOption(option.QuestionOptionId, option.OptionText, option.Order, option.Issue, retVal);
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
        /// Update risk assessment question option
        /// </summary>
        /// <param name="ques"></param>
        /// <returns></returns>
        public int UpdateRiskAssessmentQuestion(CHSERiskAssessmentQuestion question)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(long));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_update_RAQuestion(question.QuestionText, question.QuestionId, question.GroupId, question.Group, question.CourseId, question.Order, retVal);
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
    }
}
