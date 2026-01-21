using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.Model.SuperAdmin
{
    public class CHSERiskAssessment
    {
        public Int64 RAID { get; set; }
        public string RAName { get; set; }
        public string RASummary { get; set; }
        public string RADescription { get; set; }
        public string Status { get; set; }
    }

    public class CHSERiskAssessmentListing
    {
        public List<CHSERiskAssessment> RAList { get; set; }
        public int TotalRecords { get; set; }
    }
    public class CHSERiskAssessmentGroup
    {
        public Int64 GroupId { get; set; }
        public Int64 RAId { get; set; }
        public string GroupName { get; set; }
    }

    public class CHSERiskAssessmentGroupListing
    {
        public List<CHSERiskAssessmentGroup> RAGroupList { get; set; }
        public int TotalRecords { get; set; }
    }

    public class CHSERiskAssessmentQuestion
    {
        public Int64 QuestionId { get; set; }
        public Int64 CourseId { get; set; }
        public Int64 GroupId { get; set; }
        public string QuestionText { get; set; }
        public string QuestionAdditionalText { get; set; }
        public int QuestionType { get; set; }
        public int Order { get; set; }
        public int GotoNO { get; set; }
        public int GotoYes { get; set; }
        public int NumberFrom { get; set; }
        public int NumberTo { get; set; }
        public int OptionCount { get; set; }
        public bool FreeText { get; set; }
        public bool MultiLine { get; set; }
        public bool End { get; set; }
        public bool BarChart { get; set; }
        public bool BaseQuestion { get; set; }
        public string Group { get; set; }
        public string CourseName { get; set; }
    }

    public class CHSERiskAssessmentQuestionOption
    {
        public Int64 QuestionOptionId { get; set; }
        public Int64 QuestionId { get; set; }
        public string OptionText { get; set; }
        public bool Issue { get; set; }
        public int Order { get; set; }
    }
}
