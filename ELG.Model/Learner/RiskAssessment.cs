using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.Model.Learner
{
    public class RiskAssessment
    {
        public Int64 RiskAssessmentResultID { get; set; } = 0;
        public Int64 RiskAssessmentID { get; set; }
        public Int64 LearnerID { get; set; }
        public Int64 CourseId { get; set; }
        public int IssueCount { get; set; }
        public string StrLocationName { get; set; }
    }
    public class RiskAssessmentRecord
    {
        public Int64 RiskAssessmentResultID { get; set; }
        public Int64 RiskAssessmentID { get; set; }
        public Int64 SubModuleId { get; set; }
        public Int64 LearnerID { get; set; }
        public Int64 CourseId { get; set; }
        public int IssueCount { get; set; }
        public string StrLocationName { get; set; }
    }

    public class RiskAssessmentQuestion: RiskAssessment
    {
        public Int64 QuestionId { get; set; }
        public Int64 AnswerID { get; set; }
        public Int64 GroupId { get; set; }
        public string QuestionText { get; set; }
        public string Instructions { get; set; }
        public string StrAdditionalText { get; set; }
        public int Order { get; set; }
        public string GroupText { get; set; }
        public List<RiskAssessmentQuestionOption> Options { get; set; }
    }

    public class RiskAssessmentQuestionOption
    {
        public Int64 QuestionOptionId { get; set; }
        public string OptionText { get; set; }
        public bool Issue { get; set; }
        public int Order { get; set; }
    }

    public class RiskAssessmentResponse
    {
        public Int64 AnswerId { get; set; }
        public Int64 OptionId { get; set; }
        public string IssueText { get; set; }
    }

    public class RiskAssessmentEvidence
    {
        public Int64 AnswerId { get; set; }
        public string EvidencePath { get; set; }
    }

    public class RiskAssessmentResponseReview
    {
        public string Question { get; set; }
        public string Answer { get; set; }
        public string Notes { get; set; }
        public bool Issue { get; set; }
    }
}
