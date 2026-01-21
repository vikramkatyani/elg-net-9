using ELG.DAL.DBEntitySA;
using ELG.Model.SuperAdmin;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;

namespace ELG.DAL.SuperAdminDAL
{
    public class LMSCourseRep
    {
        /// <summary>
        /// Get listing of all modules in the system
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public LMSCourseListing GetLMSCourseListing(CourseListingSearch searchCriteria)
        {
            try
            {
                LMSCourseListing courseListing = new LMSCourseListing();
                List<LMS_COURSE> courseInfoList = new List<LMS_COURSE>();

                using (var context = new superadmindbEntities())
                {
                    var courseListData = context.lms_superadmin_get_courseListing(searchCriteria.CourseName, searchCriteria.Status, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (courseListData != null && courseListData.Count > 0)
                    {
                        courseListing.TotalRecords = courseListData.Count();
                        var data = courseListData.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            LMS_COURSE c = new LMS_COURSE();
                            c.CourseId = Convert.ToInt64(item.block_id);
                            c.CourseName = item.block_name;
                            c.CourseDesc = item.block_desc;
                            c.Status = Convert.ToBoolean(item.block_active) ? "Archived" : "Active";
                            courseInfoList.Add(c);
                        }
                    }
                }
                courseListing.CourseList = courseInfoList;
                return courseListing;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
