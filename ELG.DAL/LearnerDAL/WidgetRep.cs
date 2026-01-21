using ELG.DAL.DBEntity;
using ELG.DAL.DbEntityLearner;
using ELG.Model.Learner;
using ELG.Model.OrgAdmin;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.DAL.LearnerDAL
{
    public class WidgetRep
    {

        /// <summary>
        /// Get list of available classes for a learner
        /// </summary>
        /// <param name="classFilter"></param>
        /// <returns></returns>
        public TIWResponse GetWidgetResponse(string widgetId, Int64 learnerId)
        {
            try
            {
                TIWResponse response = new TIWResponse();
                using (learnerDBEntities context = new learnerDBEntities())
                {
                    var res = context.lms_learner_get_widget_response(widgetId, learnerId).FirstOrDefault();
                    if (res != null)
                    {
                        response.QueWidgetID = res.widget_que_guid;
                        response.AfterResponse = res.resp_text_after;
                        response.Response = res.resp_text;
                        response.AfterQuestion = res.widget_que_text_after;
                        response.Question = res.widget_que_text;
                        response.HeaderColor = res.widget_header_color;
                        response.QuesType = Convert.ToInt32(res.widget_que_type);
                        response.QueWidgetTitle = res.widget_que_title;
                        response.ShowQueTextBeforeFR = res.widget_show_que_text_bfr_fr.HasValue ? Convert.ToBoolean(res.widget_show_que_text_bfr_fr) : false;
                    }
                }
                return response;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public MACResponse GetMACWidgetResponse(string widgetId, Int64 learnerId)
        {
            try
            {
                MACResponse response = new MACResponse();
                using (learnerDBEntities context = new learnerDBEntities())
                {
                    var res = context.lms_learner_get_mac_widget_response(widgetId, learnerId).FirstOrDefault();
                    if (res != null)
                    {
                        response.HeaderColor = res.widget_header_color;
                        response.QueWidgetID = res.widget_que_guid;
                        response.QueWidgetTitle = res.widget_que_title;
                        response.Question = res.widget_que_text;
                        response.QuesType = Convert.ToInt32(res.widget_que_type);
                        response.ModelResponseHeading = res.widget_model_answer_heading;
                        response.ModelResponse_1 = res.widget_model_answer_resp1;
                        response.ModelResponse_2 = res.widget_model_answer_resp2;
                        response.ModelResponse_3 = res.widget_model_answer_resp3;
                        response.Response_1 = res.resp_mac_1;
                        response.Response_2 = res.resp_mac_2;
                        response.Response_3 = res.resp_mac_3;
                        response.FeedBackResponse = res.resp_mac_feedback;
                        response.FeedBackResponseText = res.resp_mac_feedback_text;
                        response.Responded = Convert.ToBoolean(res.resp_mac_responded); 
                    }
                }
                return response;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public BPCResponse GetBPCWidgetResponse(string widgetId, Int64 learnerId)
        {
            try
            {
                BPCResponse response = new BPCResponse();
                using (learnerDBEntities context = new learnerDBEntities())
                {
                    var res = context.lms_learner_get_bpc_widget_response(widgetId, learnerId).FirstOrDefault();
                    if (res != null)
                    {
                        response.HeaderColor = res.widget_header_color;
                        response.QueWidgetID = res.widget_que_guid;
                        response.QueWidgetTitle = res.widget_que_title;
                        response.Question = res.widget_que_text;
                        response.QuesType = Convert.ToInt32(res.widget_que_type);
                        response.AfterResponseHeading = res.widget_que_label_after;
                        response.AfterQuestion = res.widget_que_text_after;
                        response.Response_1 = res.resp_mac_1;
                        response.Response_2 = res.resp_mac_2;
                        response.Response_3 = res.resp_mac_3;
                        response.FeedBackResponseText = res.resp_mac_feedback_text;
                        response.Responded = Convert.ToBoolean(res.resp_mac_responded);
                    }
                }
                return response;
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Insert learner's response from a widget
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public int InsertWidgetResponse(LearnerResponse response)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("created", typeof(int));
                using (var context = new learnerDBEntities())
                {
                    var result = context.lms_learner_insert_widget_response(response.LearnerID, response.QueWidgetID, response.Response, response.ResponseFor,  retVal);
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
        /// Insert learner's response from a MAC widget
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public int InsertMACWidgetResponse(LearnerMACResponse response)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("created", typeof(int));
                using (var context = new learnerDBEntities())
                {
                    var result = context.lms_learner_insert_mac_widget_response(response.LearnerID, response.QueWidgetID, response.Response_1, response.Response_2, response.Response_3, retVal);
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
        /// Insert leaner's mac feedback
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public int InsertMACWidgetFeedback(LearnerMACResponse response)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("created", typeof(int));
                using (var context = new learnerDBEntities())
                {
                    var result = context.lms_learner_insert_mac_widget_feedback(response.LearnerID, response.QueWidgetID, response.FeedBackResponse, response.FeedBackResponseText, retVal);
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
