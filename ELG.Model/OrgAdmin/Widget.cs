using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.Model.OrgAdmin
{
    public class WidgetCourse
    {
        public Int64 CourseId { get; set; }
        public string CourseGUID { get; set; }
        public string CourseName { get; set; }
        public string CourseDesc { get; set; }
        public Int64 CourseOrg { get; set; }
        public Int32 CourseWidgetCount { get; set; }
    }
    public class WidgetCourseList
    {
        public List<WidgetCourse> CourseList { get; set; }
        public int TotalCourses { get; set; }
    }
    public class CourseWidget
    {
        public Int64 CourseId { get; set; }
        public string CourseGUID { get; set; }
        public string CourseName { get; set; }
        public Int64 QueId { get; set; }
        public string QueGUID { get; set; }
        public string QuesText { get; set; }
        public string QuesTitle { get; set; }
        public string QuesRef { get; set; }
        public bool ShowQueTextBeforeFR { get; set; }
        public string AfterQuesLabelText { get; set; }
        public string AfterQuesText { get; set; }
        public string QueType { get; set; }
        public string QueModelAnswerHeading { get; set; }
        public string QueModelAnswerResp_1 { get; set; }
        public string QueModelAnswerResp_2 { get; set; }
        public string QueModelAnswerResp_3 { get; set; }
        public string HeaderColor { get; set; }
    }
    public class CourseWidgetList
    {
        public List<CourseWidget> WidgetList { get; set; }
        public int TotalWidgets { get; set; }
    }

    public class DataTableFilterWidgets : DataTableFilter
    {
        public int Status { get; set; }
    }
}
