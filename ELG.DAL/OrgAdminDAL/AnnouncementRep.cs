using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ELG.Model.OrgAdmin;
using ELG.DAL.DBEntity;
using ELG.DAL.Utilities;
using System.Data.Entity.Core.Objects;

namespace ELG.DAL.OrgAdminDAL
{
    public class AnnouncementRep
    {

        /// <summary>
        /// To create new announcement
        /// </summary>
        /// <param name="announcement"></param>
        /// <returns></returns>
        public int CreateNewAnnouncement(Announcement announcement)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(long));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_createAnnouncement(announcement.AnnouncementOrganisation, announcement.Title, announcement.Summary, Convert.ToDateTime(announcement.PublishDate), Convert.ToDateTime(announcement.ExpiryDate), retVal);
                    success = Convert.ToInt32(retVal.Value);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return success;
        }

        /// <summary>
        /// To get list of all announcements
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public AnnouncementList GetAnnouncementList(AnnouncementFilter searchCriteria)
        {
            try
            {
                AnnouncementList announcementList = new AnnouncementList();
                List<Announcement> announcementInfoList = new List<Announcement>();

                using (var context = new lmsdbEntities())
                {
                    var annList = context.lms_admin_getAllAnnouncments(searchCriteria.SearchText, searchCriteria.Status, searchCriteria.Company).ToList();
                    if (annList != null && annList.Count > 0)
                    {
                        announcementList.TotalAnnouncements = annList.Count();
                        var data = annList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            Announcement announcementInfo = new Announcement();
                            announcementInfo.AnnouncementId = item.intAnnouncementId;
                            announcementInfo.AnnouncementOrganisation = Convert.ToInt64(item.intOrganisationid);
                            announcementInfo.DateCreated = item.datCreated == null ? "" : (Convert.ToDateTime(item.datCreated)).ToString("dd-MMM-yyyy");
                            announcementInfo.ExpiryDate = item.datExpire == null ? "" : (Convert.ToDateTime(item.datExpire)).ToString("dd-MMM-yyyy");
                            announcementInfo.PublishDate = item.datPublish == null ? "" : (Convert.ToDateTime(item.datPublish)).ToString("dd-MMM-yyyy");
                            announcementInfo.Summary = item.strSummary;
                            announcementInfo.Title = item.strTitle;
                            announcementInfo.AnnouncementCancelled = Convert.ToInt32(item.blnCancelled);
                            announcementInfoList.Add(announcementInfo);
                        }
                    }
                }
                announcementList.AnnouncementRecords = announcementInfoList;
                return announcementList;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// to change active status of announcement
        /// </summary>
        /// <param name="classroom"></param>
        /// <returns></returns>
        public int ArchiveAnnouncement(Announcement ann)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_updateAnnouncementStatus(ann.AnnouncementId, ann.AnnouncementCancelled, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// update announcement info
        /// </summary>
        /// <param name="announcement"></param>
        /// <returns></returns>
        public int UpdateAnnouncement(Announcement announcement)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_updateAnnouncement(announcement.AnnouncementId, announcement.Title, announcement.Summary, Convert.ToDateTime(announcement.PublishDate), Convert.ToDateTime(announcement.ExpiryDate), retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
