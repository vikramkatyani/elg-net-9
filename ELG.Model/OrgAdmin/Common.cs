using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.Model.OrgAdmin
{
    class Common
    {
    }

    public class ControllerResponse
    {
        public string Url { get; set; }
        public string Message { get; set; }
        public int Err { get; set; }
    }

    public class CompanyTrainerDetails
    {
        public string Company { get; set; }
        public string Trainer { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Extension { get; set; }
    }

    public class UploadContactResult
    {
        public int UploadedCount { get; set; }
        public int EmptyCount { get; set; }
        public int ExistingCount { get; set; }
        public int FailedCount { get; set; }
        public int Error { get; set; }
    }

    public class CourseRecordRequest
    {
        public int Course { get; set; }
    }
}
