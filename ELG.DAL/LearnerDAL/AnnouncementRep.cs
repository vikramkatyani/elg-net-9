using ELG.DAL.DbEntityLearner;
using ELG.Model.Learner;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.DAL.LearnerDAL
{
    public class AnnouncementRep
    {
        /// <summary>
        /// Return list of all announcments within an organisation
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public AnnouncmentRecords GetAnnouncments(AnnouncementFilter searchCriteria)
        {
            try
            {
                AnnouncmentRecords announcementList = new AnnouncmentRecords();
                List<Announcement> announcementInfoList = new List<Announcement>();

                using (var context = new learnerDBEntities())
                {
                    var anList = context.lms_learner_getAllAnouncments(searchCriteria.Learner, searchCriteria.Organisation, searchCriteria.AnnouncementId, searchCriteria.Title).ToList();
                    if (anList != null && anList.Count > 0)
                    {
                        announcementList.TotalAnnouncments = anList.Count();
                        var data = anList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            Announcement ann = new Announcement();
                            ann.AnnouncementId = item.intAnnouncementId;
                            ann.Title = item.strTitle;
                            ann.AnnouncementText = item.strSummary;
                            ann.PublishDate = item.datPublish == null ? "" : (Convert.ToDateTime(item.datPublish)).ToString("dd-MMM-yyyy");
                            ann.ExpiryDate = item.datExpire == null ? "" : (Convert.ToDateTime(item.datExpire)).ToString("dd-MMM-yyyy");
                            ann.ViewedDate = item.datViewed == null ? "" : (Convert.ToDateTime(item.datViewed)).ToString("dd-MMM-yyyy");
                            ann.IsViewed = Convert.ToBoolean(item.announcmentRead);
                            announcementInfoList.Add(ann);
                        }
                    }
                }
                announcementList.AnnouncementList = announcementInfoList;
                return announcementList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Return list of 5 unread announcments for a learner
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public AnnouncmentRecords GetUnreadAnnouncments(AnnouncementFilter searchCriteria)
        {
            try
            {
                AnnouncmentRecords announcementList = new AnnouncmentRecords();
                List<Announcement> announcementInfoList = new List<Announcement>();

                using (var context = new learnerDBEntities())
                {
                    var anList = context.lms_learner_getAllUnreadNotifications(searchCriteria.Learner, searchCriteria.Organisation).ToList();
                    if (anList != null && anList.Count > 0)
                    {
                        announcementList.TotalAnnouncments = anList.Count();
                        var data = anList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            Announcement ann = new Announcement();
                            ann.AnnouncementId = item.intAnnouncementId;
                            if (!String.IsNullOrEmpty(item.strTitle) && item.strTitle.Length > 33)
                                ann.Title = item.strTitle.Substring(0, Math.Min(item.strTitle.Length, 30)) + "...";
                            else
                                ann.Title = item.strTitle;
                            ann.AnnouncementText = item.strSummary;
                            ann.PublishDate = item.datPublish == null ? "" : (Convert.ToDateTime(item.datPublish)).ToString("dd-MMM-yyyy");
                            ann.ExpiryDate = item.datExpire == null ? "" : (Convert.ToDateTime(item.datExpire)).ToString("dd-MMM-yyyy");
                            ann.ViewedDate = item.datViewed == null ? "" : (Convert.ToDateTime(item.datViewed)).ToString("dd-MMM-yyyy");
                            if(ann.ViewedDate == null)
                                ann.IsViewed = false;
                            else
                                ann.IsViewed = true;
                            announcementInfoList.Add(ann);
                        }
                    }
                }
                announcementList.AnnouncementList = announcementInfoList;
                return announcementList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Set announcement red
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public int SetAnnouncementRead(AnnouncementFilter searchCriteria)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new learnerDBEntities())
                {
                    var result = context.lms_learner_setNotificationRead(searchCriteria.Learner,searchCriteria.AnnouncementId, retVal);
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
