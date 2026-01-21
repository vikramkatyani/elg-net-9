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
    public class AccidentIncidentRep
    {
        /// <summary>
        /// Get list of all accidents/incidents reported by the logged in user
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public ReportedIncidentsList GetReportedIncidents(DataTableFilter searchCriteria)
        {
            try
            {
                ReportedIncidentsList incidentsList = new ReportedIncidentsList();
                List<AccidentIncidentResponse> incidents = new List<AccidentIncidentResponse>();

                using (var context = new lmsdbEntities())
                {
                    var incidentList = context.lms_admin_get_reportedIncidents(searchCriteria.SearchText, searchCriteria.AdminUserId, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
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
                            incident.CreatorName = item.strFirstname + " " + item.strSurname;
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
        /// Get list of all accidents/incidents reported in an organisation
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public ReportedIncidentsList GetReportedIncidentsInOrg(AccidentIncidentReportFilter searchCriteria)
        {
            try
            {
                ReportedIncidentsList incidentsList = new ReportedIncidentsList();
                List<AccidentIncidentResponse> incidents = new List<AccidentIncidentResponse>();

                using (var context = new lmsdbEntities())
                {
                    var incidentList = context.lms_admin_get_reportedIncidents_for_org(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.SearchText, searchCriteria.Company, searchCriteria.SignedOff, searchCriteria.IsEmployee, searchCriteria.IsPermitted, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (incidentList != null && incidentList.Count > 0)
                    {
                        incidentsList.TotalIncidents = incidentList.Count();
                        var data = incidentList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in incidentList)
                        {
                            AccidentIncidentResponse incident = new AccidentIncidentResponse();
                            incident.ResponseId = item.intResponseId;
                            incident.AccidentIncidentId = item.intAccidentIncidentId;
                            incident.Title = item.strTitle;
                            incident.ReportedBy = item.reportedBy;
                            incident.ReportedOn = item.incident_reportedon == null ? "" : (Convert.ToDateTime(item.incident_reportedon)).ToString("dd-MMM-yyyy HH:mm");
                            incident.ReportedFor = item.reportedFor;
                            incident.IncidentOn = item.incident_happendon == null ? "" : (Convert.ToDateTime(item.incident_happendon)).ToString("dd-MMM-yyyy HH:mm");
                            incident.CreatorName = item.strFirstname + " " + item.strSurname;
                            incident.Response = item.strResponse;
                            incident.IsSignedOff = item.signed_off_on == null ? false : true;
                            incident.IsEmployee = Convert.ToBoolean(item.is_a_employee) ? "Yes" : "No";
                            incident.IsPermitted = Convert.ToBoolean(item.is_permited_activity) ? "Yes" : "No";
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
        /// Get list of all accidents/incidents reported in an organisation
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public List<DownloadAccidentIncidentResponse> DownloadReportedIncidentsInOrg(AccidentIncidentReportFilter searchCriteria)
        {
            try
            {
                List<DownloadAccidentIncidentResponse> incidents = new List<DownloadAccidentIncidentResponse>();

                using (var context = new lmsdbEntities())
                {
                    var incidentList = context.lms_admin_get_reportedIncidents_for_org(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.SearchText, searchCriteria.Company, searchCriteria.SignedOff, searchCriteria.IsEmployee, searchCriteria.IsPermitted, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (incidentList != null && incidentList.Count > 0)
                    {
                        foreach (var item in incidentList)
                        {
                            DownloadAccidentIncidentResponse incident = new DownloadAccidentIncidentResponse();
                            incident.Title = item.strTitle;
                            incident.ReportedBy = item.reportedBy;
                            incident.ReportedOn = item.incident_reportedon == null ? "" : (Convert.ToDateTime(item.incident_reportedon)).ToString("dd-MMM-yyyy HH:mm");
                            incident.Injured = item.reportedFor;
                            incident.IncidentOn = item.incident_happendon == null ? "" : (Convert.ToDateTime(item.incident_happendon)).ToString("dd-MMM-yyyy HH:mm");
                            incident.CreatedBy = item.strFirstname + " " + item.strSurname;
                            incident.Employee = Convert.ToBoolean(item.is_a_employee) ? "Yes" : "No";
                            incident.Reportable = Convert.ToBoolean(item.is_permited_activity) ? "Yes" : "No";
                            incident.SignedOff = item.signed_off_on == null ? "No" : "Yes";
                            incident.SignedOffDate = item.signed_off_on == null ? "" : (Convert.ToDateTime(item.signed_off_on)).ToString("dd-MMM-yyyy HH:mm");
                            incident.Comment = item.sign_off_comment;

                            incidents.Add(incident);
                        }
                    }
                }
                return incidents;
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

                using (var context = new lmsdbEntities())
                {
                    var questionList = context.lms_admin_get_accidentIncidentQues(accidentId).ToList();
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
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_create_accidentincident_response(response.CreatorId, response.AccidentIncidentId, response.Response, response.CreatorId, response.ReportedBy, response.ReportedFor, response.IncidentOn, retVal);
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
        /// to signoff reported accident/incident
        /// </summary>
        /// <param name="aiid"></param>
        /// <param name="adminComments"></param>
        /// <param name="adminId"></param>
        /// <returns></returns>
        public int SignOffAccidentIncident(Int64 aiid, string adminComments, Int64 adminId)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_update_reportedAccidentIncident(aiid, adminId, adminComments, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
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
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_create_Incident_ResponseEvidence(evidence.ResponseId, evidence.ImagePath, retVal);
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
