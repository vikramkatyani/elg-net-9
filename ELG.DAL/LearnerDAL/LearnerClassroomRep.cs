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
    public class LearnerClassroomRep
    {

        /// <summary>
        /// Get list of available classes for a learner
        /// </summary>
        /// <param name="classFilter"></param>
        /// <returns></returns>
        public ClassroomList GetClassroomList(DataTableFilter classFilter)
        {
            try
            {
                ClassroomList classroomList = new ClassroomList();
                List<ClassroomProgress> classroomInfoList = new List<ClassroomProgress>();

                using (learnerDBEntities context = new learnerDBEntities())
                {
                    var learnerClassList = context.lms_learner_getAllClassrooms(classFilter.Organisation, classFilter.Learner, classFilter.SearchText).ToList();
                    if (learnerClassList != null && learnerClassList.Count > 0)
                    {
                        classroomList.TotalClassrooms = learnerClassList.Count();
                        var data = learnerClassList.Skip(classFilter.Skip).Take(classFilter.PageSize).ToList();

                        foreach (var item in learnerClassList)
                        {
                            ClassroomProgress classroom = new ClassroomProgress();
                            classroom.ClassroomId = item.intClassroomId;
                            classroom.ClassroomName = item.strClassroomName;
                            classroom.ClassDesc = item.strClassroomDesc;
                            classroom.CreatedOn = item.dateCreatedOn == null ? "" : (Convert.ToDateTime(item.dateCreatedOn)).ToString("dd-MMM-yyyy");

                            if (item.accepted == null)
                                classroom.Accepted = "not requested";
                            else if (item.accepted == 0)
                                classroom.Accepted = "requested";

                            classroomInfoList.Add(classroom);
                        }
                    }
                }
                classroomList.Classrooms = classroomInfoList;
                return classroomList;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="classroom"></param>
        /// <returns></returns>
        public int SendClassRequest(ClassroomProgress classroom)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("id", typeof(int));
                using (learnerDBEntities context = new learnerDBEntities())
                {
                    var result = context.lms_learner_createClassroomRequest(classroom.ClassroomId, classroom.Learner, classroom.Organisation, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get list of available classes for a learner
        /// </summary>
        /// <param name="classFilter"></param>
        /// <returns></returns>
        public ClassroomList GetClassroomProgress(DataTableFilter classFilter)
        {
            try
            {
                ClassroomList classroomList = new ClassroomList();
                List<ClassroomProgress> classroomInfoList = new List<ClassroomProgress>();

                using (learnerDBEntities context = new learnerDBEntities())
                {
                    var learnerClassList = context.lms_learner_getAllAcceptedClassrooms(classFilter.Organisation, classFilter.Learner, classFilter.SearchText).ToList();
                    if (learnerClassList != null && learnerClassList.Count > 0)
                    {
                        classroomList.TotalClassrooms = learnerClassList.Count();
                        var data = learnerClassList.Skip(classFilter.Skip).Take(classFilter.PageSize).ToList();

                        foreach (var item in learnerClassList)
                        {
                            ClassroomProgress classroom = new ClassroomProgress();
                            classroom.ClassroomId = item.intClassroomId;
                            classroom.ClassroomName = item.strClassroomName;
                            classroom.ClassDesc = item.strClassroomDesc;
                            classroom.CreatedOn = item.dateCreatedOn == null ? "" : (Convert.ToDateTime(item.dateCreatedOn)).ToString("dd-MMM-yyyy");
                            classroom.AttendedOn = item.dateAttendedOn == null ? "" : (Convert.ToDateTime(item.dateAttendedOn)).ToString("dd-MMM-yyyy");

                            if (item.intStatus == 0)
                                classroom.Status = "Invite accepted";
                            else if (item.intStatus == 1)
                                classroom.Status = "Passed";
                            else if (item.intStatus == 2)
                                classroom.Status = "Failed";
                            else if (item.intStatus == 3)
                                classroom.Status = "Not Complete";

                            classroomInfoList.Add(classroom);
                        }
                    }
                }
                classroomList.Classrooms = classroomInfoList;
                return classroomList;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
