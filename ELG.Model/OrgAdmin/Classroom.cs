using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.Model.OrgAdmin
{
   public class Classroom
    {
        public int ClassroomId { get; set; }
        public string ClassroomName { get; set; }
        public bool Active { get; set; }
        public string ClassDesc { get; set; }
        public string CreatedOn { get; set; }
        public Int64 OrganisationId { get; set; }
        public Int64 Creator { get; set; }
    }

    public class ActiveClassroomList
    {
        public List<Classroom> ClassroomList { get; set; }
        public int TotalClassroom { get; set; }
    }

    public class ClassroomFilter : DataTableFilter
    {
        public int Status { get; set; }
    }

    public class ClassroomProgressFilter : DataTableFilter
    {
        public string ClassroomName { get; set; }
    }

    public class ClassroomProgressItem : LearnerInfo
    {
        public Int64 Course { get; set; }
        public String CourseName { get; set; }
        public String CourseDesc { get; set; }
        public String TeacherName { get; set; }
        public String Venue { get; set; }
        public String Comment { get; set; }
        public string AttendedOn { get; set; }
        public string ClassStatus { get; set; }
        public Int64 MarkedBy { get; set; }
    }

    public class ClassroomReportFilter : DataTableFilter
    {
        public int Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class ClassroomProgressReport
    {
        public List<ClassroomProgressItem> ClassroomRecords { get; set; }
        public int TotalRecords { get; set; }
    }

    public class DownloadClassroomReport : DownloadReport
    {
        public String ClassName { get; set; }
        public string Venue { get; set; }
        public string AttendedDate { get; set; }
    }
}
