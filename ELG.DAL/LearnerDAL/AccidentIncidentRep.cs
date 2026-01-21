using System;
using System.Collections.Generic;
using System.Linq;
using ELG.Model.Learner;
using ELG.DAL.DbEntityLearner;
using System.Data.Entity.Core.Objects;

namespace ELG.DAL.LearnerDAL
{
    public class AccidentIncidentRep
    {
        /// <summary>
        /// Get list of all modules assigned to a learner
        /// </summary>
        /// <param name="course"></param>
        /// <returns></returns>
        public ReportedIncidentsList GetReportedIncidents(DataTableFilter searchCriteria)
        {
            try
            {
                ReportedIncidentsList incidentsList = new ReportedIncidentsList();
                List<AccidentIncidentResponse> incidents = new List<AccidentIncidentResponse>();

                using (learnerDBEntities context = new learnerDBEntities())
                {
                    var incidentList = context.lms_learner_get_reportedIncidents(searchCriteria.SearchText, searchCriteria.Learner, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (incidentList != null && incidentList.Count > 0)
                    {
                        incidentsList.TotalIncidents = incidentList.Count();
                        var data = incidentList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in incidentList)
                        {
                            AccidentIncidentResponse incident = new AccidentIncidentResponse();
                            incident.AccidentIncidentId = item.intAccidentIncidentId;
                            incident.Title = item.strTitle;
                            incident.ReportedBy = item.reportedBy;
                            incident.ReportedOn = item.incident_reportedon == null ? "" : (Convert.ToDateTime(item.incident_reportedon)).ToString("dd-MMM-yyyy HH:mm");
                            incident.ReportedFor = item.reportedFor;
                            incident.IncidentOn = item.incident_happendon == null ? "" : (Convert.ToDateTime(item.incident_happendon)).ToString("dd-MMM-yyyy HH:mm");
                            incident.CreatorName = item.strFirstname+" "+item.strSurname;
                            incident.Response = item.strResponse;
                            incident.IsSignedOff = item.signed_off_on == null ? false : true;
                            incident.SignedOffOn = item.signed_off_on == null ? "" : (Convert.ToDateTime(item.signed_off_on)).ToString("dd-MMM-yyyy HH:mm");
                            incident.SignedOffBy = item.signedoffBy_fn + " " + item.signedoffBy_ln;
                            incident.Comment = item.sign_off_comment;
                            incident.ResponseImageURL = item.response_imageURL;

                            incidents.Add(incident);
                        }
                    }
                }
                incidentsList.IncidentList = incidents;
                return incidentsList;
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
        public List<AccidentIncidentQuestion> GetAccidentIncidentQuestions(Int64 accidentId)
        {
            try
            {
                List<AccidentIncidentQuestion> questions = new List<AccidentIncidentQuestion>();

                using (learnerDBEntities context = new learnerDBEntities())
                {
                    var questionList = context.lms_learner_get_accidentIncidentQues(accidentId).ToList();
                    if (questionList != null && questionList.Count > 0)
                    {
                        foreach (var item in questionList)
                        {
                            AccidentIncidentQuestion que = new AccidentIncidentQuestion();
                            que.QuestionID = item.intQuestionId;
                            que.Question = item.strQuestion;
                            que.QuestionType = item.strQuestionType.ToLower();
                            que.Options = item.options;
                            que.IsMandatory = item.isMandatory;
                            que.Title = item.strTitle;
                            que.DisplaySequence = item.strQuestionDisplaySequense;

                            questions.Add(que);
                        }
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
        /// to save ra responses
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public int SaveAccidentIncident(AccidentIncidentResponse response)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("id", typeof(long));
                using (var context = new learnerDBEntities())
                {
                    var result = context.lms_learner_create_accidentincident_response(response.CreatorId, response.AccidentIncidentId, response.Response, response.CreatorId, response.ReportedBy, response.ReportedFor, response.IncidentOn, response.IsEmployee, response.IsPermitted, retVal);
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
       /// To save incident image
       /// </summary>
       /// <param name="evidence"></param>
       /// <returns></returns>
        public int SaveIncidentImage(IncidentImage evidence)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("retval", typeof(long));
                using (var context = new learnerDBEntities())
                {
                    var result = context.lms_learner_create_Incident_ResponseEvidence(evidence.ResponseId, evidence.ImagePath, retVal);
                    success = Convert.ToInt32(retVal.Value);
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
