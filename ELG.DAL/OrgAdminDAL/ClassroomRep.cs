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
    public class ClassroomRep
    {
        /// <summary>
        /// To create a new classroom
        /// </summary>
        /// <param name="classroom"></param>
        /// <returns></returns>
        public int CreateNewClassroom(Classroom classroom)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("id", typeof(long));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_createClassroom(classroom.ClassroomName, classroom.ClassDesc, classroom.Creator, classroom.OrganisationId, retVal);
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
        /// get list of all active classes
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public ActiveClassroomList GetClassroomList(ClassroomFilter searchCriteria)
        {
            try
            {
                ActiveClassroomList classroomList = new ActiveClassroomList();
                List<Classroom> classroomInfoList = new List<Classroom>();

                using (var context = new lmsdbEntities())
                {
                    var classList = context.lms_admin_getAllClassrooms(searchCriteria.Company, searchCriteria.SearchText, searchCriteria.Status).ToList();
                    if (classList != null && classList.Count > 0)
                    {
                        classroomList.TotalClassroom = classList.Count();
                        var data = classList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            Classroom classinfo = new Classroom();
                            classinfo.ClassroomId = Convert.ToInt32(item.intClassroomId);
                            classinfo.ClassroomName = item.strClassroomName;
                            classinfo.ClassDesc = item.strClassroomDesc;
                            classinfo.Active = item.blnActive;
                            classinfo.CreatedOn = item.dateCreatedOn == null ? "" : (Convert.ToDateTime(item.dateCreatedOn)).ToString("dd-MMM-yyyy");
                            classroomInfoList.Add(classinfo);
                        }
                    }
                }
                classroomList.ClassroomList = classroomInfoList;
                return classroomList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        

        /// <summary>
        /// Get list of all pending requests for class
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public ClassroomProgressReport GetPendingRequests(ClassroomProgressFilter searchCriteria)
        {
            try
            {
                ClassroomProgressReport pendingRequestList = new ClassroomProgressReport();
                List<ClassroomProgressItem> classroomInfoList = new List<ClassroomProgressItem>();

                using (var context = new lmsdbEntities())
                {
                    var classList = context.lms_admin_getClassroomPendingRequest(searchCriteria.Company, searchCriteria.ClassroomName, searchCriteria.SearchText, searchCriteria.Location, searchCriteria.Department).ToList();
                    if (classList != null && classList.Count > 0)
                    {
                        pendingRequestList.TotalRecords = classList.Count();
                        var data = classList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            ClassroomProgressItem classinfo = new ClassroomProgressItem();
                            classinfo.Course = item.intClassroomId;
                            classinfo.CourseName = item.strClassroomName;
                            classinfo.CourseDesc = item.strClassroomDesc;
                            classinfo.UserID = Convert.ToInt64(item.intContactID);
                            classinfo.FirstName = item.strFirstName;
                            classinfo.LastName = item.strSurname;
                            classinfo.EmailId = item.strEmail;
                            classinfo.EmployeeNumber = item.strEmployeeNumber;
                            classinfo.Location = item.strLocation;
                            classinfo.Department = item.strDepartment;
                            classroomInfoList.Add(classinfo);
                        }
                    }
                }
                pendingRequestList.ClassroomRecords = classroomInfoList;
                return pendingRequestList;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// list of all accepted request to mark as attended
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public ClassroomProgressReport GetAcceptedRequests(ClassroomProgressFilter searchCriteria)
        {
            try
            {
                ClassroomProgressReport acceptedRequestList = new ClassroomProgressReport();
                List<ClassroomProgressItem> classroomInfoList = new List<ClassroomProgressItem>();

                using (var context = new lmsdbEntities())
                {
                    var classList = context.lms_admin_getClassroomProgress(searchCriteria.Company, searchCriteria.ClassroomName, searchCriteria.SearchText, searchCriteria.Location, searchCriteria.Department).ToList();
                    if (classList != null && classList.Count > 0)
                    {
                        acceptedRequestList.TotalRecords = classList.Count();
                        var data = classList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            ClassroomProgressItem classinfo = new ClassroomProgressItem();
                            classinfo.Course = item.intClassroomId;
                            classinfo.CourseName = item.strClassroomName;
                            classinfo.CourseDesc = item.strClassroomDesc;
                            classinfo.UserID = Convert.ToInt64(item.intContactID);
                            classinfo.FirstName = item.strFirstName;
                            classinfo.LastName = item.strSurname;
                            classinfo.EmailId = item.strEmail;
                            classinfo.EmployeeNumber = item.strEmployeeNumber;
                            classinfo.Location = item.strLocation;
                            classinfo.Department = item.strDepartment;

                            if (item.intStatus == 0)
                                classinfo.ClassStatus = "Invite accepted";
                            else if (item.intStatus == 1)
                                classinfo.ClassStatus = "Passed";
                            else if (item.intStatus == 2)
                                classinfo.ClassStatus = "Failed";
                            else if (item.intStatus == 3)
                                classinfo.ClassStatus = "Not Complete";

                            classinfo.AttendedOn = item.dateAttendedOn == null ? "" : (Convert.ToDateTime(item.dateAttendedOn)).ToString("dd-MMM-yyyy");
                            classroomInfoList.Add(classinfo);
                        }
                    }
                }
                acceptedRequestList.ClassroomRecords = classroomInfoList;
                return acceptedRequestList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// To get classroom Info
        /// </summary>
        /// <param name="classroomId"></param>
        /// <returns></returns>
        public Classroom GetClassroomInfo(Int64 classroomId)
        {
            try
            {
                Classroom classroomInfo = new Classroom();

                using (var context = new lmsdbEntities())
                {
                    var classInfo = context.lms_admin_getClassroomInfo(classroomId).FirstOrDefault();
                    if (classInfo != null)
                    {
                        classroomInfo.ClassroomId = Convert.ToInt32(classInfo.intClassroomId);
                        classroomInfo.ClassroomName = classInfo.strClassroomName;
                        classroomInfo.ClassDesc = classInfo.strClassroomDesc;
                        classroomInfo.CreatedOn = classInfo.dateCreatedOn == null ? "" : (Convert.ToDateTime(classInfo.dateCreatedOn)).ToString("dd-MMM-yyyy");
                    }
                }
                return classroomInfo;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// update classroom info
        /// </summary>
        /// <param name="classroom"></param>
        /// <returns></returns>
        public int UpdateClassroom(Classroom classroom)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_updateClassroom(classroom.ClassroomId, classroom.ClassroomName, classroom.ClassDesc, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// archive classroom 
        /// </summary>
        /// <param name="classroom"></param>
        /// <returns></returns>
        public int ArchiveClassroom(Classroom classroom)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_archiveClassroom(classroom.ClassroomId, classroom.Creator, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// To accept classroom request
        /// </summary>
        /// <param name="classroom"></param>
        /// <returns></returns>
        public int AcceptClassroomRequest(ClassroomProgressItem classroom)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("id", typeof(long));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_acceptClassroomPendingRequest(classroom.Course, classroom.UserID, classroom.MarkedBy, retVal);
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
        /// To reject classroom request
        /// </summary>
        /// <param name="classroom"></param>
        /// <returns></returns>
        public int RejectClassroomRequest(ClassroomProgressItem classroom)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("id", typeof(long));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_rejectClassroomPendingRequest(classroom.Course, classroom.UserID, retVal);
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
        /// mark class attended
        /// </summary>
        /// <param name="classroom"></param>
        /// <returns></returns>
        public int MarkClassAttended(ClassroomProgressItem classroom)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("id", typeof(long));
                using (var context = new lmsdbEntities())
                {
                    var result = context.lms_admin_markClassAttended(classroom.Course, classroom.UserID, Convert.ToInt32(classroom.ClassStatus), classroom.MarkedBy, classroom.AttendedOn, retVal);
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
