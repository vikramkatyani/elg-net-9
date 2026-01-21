using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.Model.Learner
{
    public class AnnouncementFilter : DataTableFilter
    {
        public Int64 AnnouncementId { get; set; }
        public string Title { get; set; }
    }

    public class Announcement
    {
        public Int64 AnnouncementId { get; set; }
        public string Title { get; set; }
        public string AnnouncementText { get; set; }
        public string PublishDate { get; set; }
        public string ExpiryDate { get; set; }
        public bool IsViewed { get; set; }
        public string ViewedDate { get; set; }
    }

    public class AnnouncmentRecords
    {
        public List<Announcement> AnnouncementList { get; set; }
        public int TotalAnnouncments { get; set; }
    }
}
