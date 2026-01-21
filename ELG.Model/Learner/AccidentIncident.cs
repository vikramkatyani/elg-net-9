using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.Model.Learner
{
    public class AccidentIncident
    {
        public Int64 AccidentIncidentId { get; set; }
        public string Title { get; set; }
    }

    public class AccidentIncidentQuestion : AccidentIncident
    {
        public Int64 QuestionID { get; set; }
        public string Question { get; set; }
        public string QuestionType { get; set; }
        public bool IsMandatory { get; set; }
        public string Options { get; set; }
        public string DisplaySequence { get; set; }
    }

    public class AccidentIncidentResponse : AccidentIncident
    {
        public string ReportedBy { get; set; }
        public string ReportedOn { get; set; }
        public string ReportedFor { get; set; }
        public string IncidentOn { get; set; }
        public Int64 CreatorId { get; set; }
        public string CreatorName { get; set; }
        public string Response { get; set; }
        public bool IsSignedOff { get; set; }
        public int IsEmployee { get; set; }
        public int IsPermitted { get; set; }
        public string SignedOffOn { get; set; }
        public string SignedOffBy { get; set; }
        public string Comment { get; set; }
        public string ResponseImageURL { get; set; }
    }

    public class ReportedIncidentsList
    {
        public List<AccidentIncidentResponse> IncidentList { get; set; }
        public int TotalIncidents { get; set; }
    }

    public class IncidentImage
    {
        public Int64 ResponseId { get; set; }
        public string ImagePath { get; set; }
    }

}
