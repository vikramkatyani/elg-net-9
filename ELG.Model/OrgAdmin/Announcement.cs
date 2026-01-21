using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.Model.OrgAdmin
{
    public class Announcement
    {
        public Int64 AnnouncementId { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public Int64 AnnouncementOrganisation { get; set; } 
        public string PublishDate { get; set; }
        public string ExpiryDate { get; set; }
        public string DateCreated { get; set; }
        public int AnnouncementCancelled { get; set; }
    }

    public class AnnouncementList
    {
        public List<Announcement> AnnouncementRecords { get; set; }
        public int TotalAnnouncements { get; set; }
    }

    public class AnnouncementFilter : DataTableFilter
    {
        public int Status { get; set; }
    }
}
