using ELG.DAL.DBEntitySA;
using ELG.DAL.Utilities;
using ELG.Model.SuperAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.DAL.SuperAdminDAL
{
    public class SuperAdminRep
    {
        /// <summary>
        /// Get user info by username
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public List<SuperAdminInfo> GetAdmin(string username, string password, string key, Boolean masterPwd)
        {
            try
            {
                var enc_password = CommonMethods.EncodePassword(password, key);
                List<SuperAdminInfo> admins = new List<SuperAdminInfo>();
                using (var context = new superadmindbEntities())
                {
                    var adminList = context.lms_superadmin_getAdminLoginDetails(username, enc_password, masterPwd).ToList();
                    if (adminList != null && adminList.Count > 0)
                    {
                        foreach (var item in adminList)
                        {
                            SuperAdminInfo admin = new SuperAdminInfo();
                            admin.UserID = item.intcontactid;
                            admin.UserRole = Convert.ToInt32(item.intAdminLevelID);
                            admin.FirstName = item.strFirstName;
                            admin.LastName = item.strSurname;
                            admin.EmailId = item.strEmail;
                            admin.IsPasswordReset = Convert.ToBoolean(item.IsRestPassword);

                            admins.Add(admin);
                        }
                    }
                }
                return admins;
            }
            catch (Exception)
            {
                throw;
            }
        }
        //#region Other

        ///// <summary>
        ///// Save strUnlockKey into New CourseLicenseDetail table
        ///// </summary>
        ///// <returns></returns>
        //public async Task SaveUnlockKeyIntoCourseLicenseDetail()
        //{
        //    try
        //    {
        //        await Task.Run(() =>
        //        {
        //            using (var context = new LearnerATF())
        //            {
        //                var resultList = context.tbCourses.ToList();
        //                if (resultList != null && resultList.Count > 0)
        //                {
        //                    Security sec = new Security();
        //                    foreach (var item in resultList)
        //                    {
        //                        if (!string.IsNullOrWhiteSpace(item.strUnlockKey))
        //                        {
        //                            var keys = sec.DecryptUnlockKey(item.strUnlockKey);
        //                            if (keys.intCourseID > 0)
        //                            {
        //                                context.lms_superadmin_UpdatetbCourseLicenseDetails(keys.intCourseID, keys.datExpiry, keys.intNumUsers);

        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        });
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}
        //#endregion
    }
}
