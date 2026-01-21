using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.Model.Learner
{
    public class CourseProgressResponse
    {
        public int SendABSCertificate { get; set; }
        public string Success { get; set; }
    }
    public class InitCourse
    {
        public Int64 UserId { get; set; }
        public Int64 CourseId { get; set; }
        public Int64 MasterCourseId { get; set; }
    }

    public class CourseProgress: InitCourse
    {
        public string UserFistName { get; set; }
        public string UserLastName { get; set; }
        public string SuspendData { get; set; }
        public string Bookmark { get; set; }
        public string ProgressStatus { get; set; }
        public double Score { get; set; }
        public string SessionTime { get; set; }
        //public int PassingScore { get; set; }
    }

    public class CourseProgressData: InitCourse
    {
        public string Parameter { get; set; }
        public string Value { get; set; }
        public string Action { get; set; }
    }

    public class CourseProgressHistoryData : InitCourse
    {
        public Int64 RecordId { get; set; }
        public string Course { get; set; }
        public string ProgressStatus { get; set; }
        public int Score { get; set; }
        public string AssignedOn { get; set; }
        public string CompletedOn { get; set; }
    }

    public class CourseProgressHistoryList
    {
        public List<CourseProgressHistoryData> History { get; set; }
        public int TotalRecords { get; set; }
    }

    public class Classroom
    {
        public Int64 ClassroomId { get; set; }
        public Int64 Organisation { get; set; }
        public string ClassroomName { get; set; }
        public bool Active { get; set; }
        public string ClassDesc { get; set; }
        public string CreatedOn { get; set; }
    }

    public class ClassroomProgress: Classroom
    {
        public Int64 Learner { get; set; }
        public string Status { get; set; }
        public string RequestDate { get; set; }
        public string AttendedOn { get; set; }
        public Int64 MarkedBy { get; set; }
        public string Accepted { get; set; }
    }

    public class ClassroomList
    {
        public List<ClassroomProgress> Classrooms { get; set; }
        public int TotalClassrooms { get; set; }
    }
}
