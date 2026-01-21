using System;
using System.Collections.Generic;
using System.Linq;
using ELG.Model.OrgAdmin;
using ELG.DAL.DBEntity;
using System.Data.Entity.Core.Objects;

namespace ELG.DAL.OrgAdminDAL
{
    public class WidgetRep
    {

        /// <summary>
        /// Return list of all courses created for widget within a company
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public WidgetCourseList GetWidgetCourses(DataTableFilter searchCriteria)
        {
            try
            {
                WidgetCourseList courseList = new WidgetCourseList();
                List<WidgetCourse> courseInfoList = new List<WidgetCourse>();

                using (var context = new lmsdbEntities())
                {
                    var cl = context.lms_admin_get_AllWidgetCourses(searchCriteria.Company, searchCriteria.SearchText).ToList();
                    if (cl != null && cl.Count > 0)
                    {
                        courseList.TotalCourses = cl.Count();
                        var data = cl.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            WidgetCourse wc = new WidgetCourse();
                            wc.CourseId = item.id;
                            wc.CourseName = item.name;
                            wc.CourseGUID = item.course_uid;
                            wc.CourseDesc = item.descrip;
                            wc.CourseWidgetCount = Convert.ToInt32(item.widget_count);
                            courseInfoList.Add(wc);
                        }
                    }
                    courseList.CourseList = courseInfoList;
                }
                
                return courseList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        
        /// <summary>
        /// Create new course
        /// </summary>
        /// <param name="course"></param>
        /// <returns></returns>
        public int CreateNewWidgetCourse(WidgetCourse course)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("created", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_add_WidgetCourse(course.CourseOrg, course.CourseName, course.CourseDesc, retVal);
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
        /// Return list of all widgets in a course
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public CourseWidgetList GetCourseWidgets(DataTableFilterWidgets searchCriteria)
        {
            try
            {
                CourseWidgetList widgetList = new CourseWidgetList();
                List<CourseWidget> courseInfoList = new List<CourseWidget>();

                using (var context = new lmsdbEntities())
                {
                    var wl = context.lms_admin_get_CourseWidgets(searchCriteria.AdminRole, searchCriteria.AdminUserId, searchCriteria.SearchText, searchCriteria.Company, searchCriteria.Status, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (wl != null && wl.Count > 0)
                    {
                        widgetList.TotalWidgets = wl.Count();
                        var data = wl.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            CourseWidget wc = new CourseWidget();
                            wc.CourseId = item.widget_course_id;
                            wc.CourseName = item.widget_course_name;
                            wc.QueId = item.widget_que_id;
                            wc.QueGUID = item.widget_que_guid;
                            wc.QuesText = item.widget_que_text;
                            wc.QueType = item.widget_que_type;
                            wc.QuesTitle = item.widget_que_title;
                            wc.QuesRef = item.widget_que_ref;
                            wc.QueModelAnswerHeading = item.widget_model_answer_heading;
                            wc.QueModelAnswerResp_1 = item.widget_model_answer_resp1;
                            wc.QueModelAnswerResp_2 = item.widget_model_answer_resp2;
                            wc.QueModelAnswerResp_3 = item.widget_model_answer_resp3;
                            courseInfoList.Add(wc);
                        }
                    }
                    widgetList.WidgetList = courseInfoList;
                }

                return widgetList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Create new widget
        /// </summary>
        /// <param name="course"></param>
        /// <returns></returns>
        public int CreateNewCourseWidget(CourseWidget widget)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("created", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_add_widgetQuestion(widget.QueType, widget.QuesText, widget.QuesTitle, widget.QuesRef, widget.AfterQuesText, widget.CourseGUID, widget.HeaderColor, widget.AfterQuesLabelText, widget.QueModelAnswerHeading, widget.QueModelAnswerResp_1, widget.QueModelAnswerResp_2, widget.QueModelAnswerResp_3, widget.ShowQueTextBeforeFR, retVal);
                    success = Convert.ToInt32(retVal.Value);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return success;
        }
    }
}
