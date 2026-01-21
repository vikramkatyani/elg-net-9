using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.Model.Learner
{
    public class Widget
    {
        public Int64 WidgetId { get; set; }
        public string Question {  get; set; }
        public string Response { get; set; }   
        public DateTime LastUpdatedOn { get; set; }
    }
    public class WidgetResponse
    {
        public String HeaderColor { get; set; }
        public string QueWidgetID { get; set; }
        public string QueWidgetTitle { get; set; }
        public string Question { get; set; }
        public int QuesType { get; set; }
        public string QuesMode { get; set; }
    }
    public class TIWResponse: WidgetResponse
    {
        public string AfterQuestion { get; set; }
        public string Response { get; set; }
        public string AfterResponse { get; set; }
        public bool ShowQueTextBeforeFR { get; set; }
    }
    public class MACResponse : WidgetResponse
    {
        public string ModelResponseHeading { get; set; }
        public string ModelResponse_1 { get; set; }
        public string ModelResponse_2 { get; set; }
        public string ModelResponse_3 { get; set; }
        public string Response_1 { get; set; }
        public string Response_2 { get; set; }
        public string Response_3 { get; set; }
        public string FeedBackResponse { get; set; }
        public string FeedBackResponseText { get; set; }
        public bool Responded { get; set; }
    }
    public class BPCResponse : WidgetResponse
    {
        public string AfterQuestion { get; set; }
        public string AfterResponseHeading { get; set; }
        public string Response_1 { get; set; }
        public string Response_2 { get; set; }
        public string Response_3 { get; set; }
        public string FeedBackResponseText { get; set; }
        public bool Responded { get; set; }
    }

    public class LearnerResponse
    {
        public Int64 LearnerID { get; set; }
        public string QueWidgetID { get; set; }
        public string Response { get; set; }
        public string ResponseFor { get; set; }
    }

    public class LearnerMACResponse
    {
        public Int64 LearnerID { get; set; }
        public string QueWidgetID { get; set; }
        public string Response_1 { get; set; }
        public string Response_2 { get; set; }
        public string Response_3 { get; set; }
        public string FeedBackResponse { get; set; }
        public string FeedBackResponseText { get; set; }
    }
}
