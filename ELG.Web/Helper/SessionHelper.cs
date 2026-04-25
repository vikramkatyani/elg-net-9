using ELG.Model.OrgAdmin;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ELG.Web.Helper
{
    public static class SessionHelper
    {
        // This property should be set at startup (e.g., in Startup.cs)
        public static Microsoft.AspNetCore.Http.IHttpContextAccessor HttpContextAccessor { get; set; }

        private static Microsoft.AspNetCore.Http.ISession Session => HttpContextAccessor?.HttpContext?.Session;
        #region Public Static Methods
        /// <summary>
        /// Clears Session
        /// </summary>
        public static void ClearSession()
        {
            Session?.Clear();
        }
        /// <summary>
        /// Abandons Session
        /// </summary>
        public static void Abandon()
        {
            ClearSession();
            // No direct equivalent for Abandon in ASP.NET Core
        }

        /// <summary>
        /// Gets OrgDomainDetails from session, or refetches it from database if session expired
        /// This method handles the case where session expires (e.g., on login page left idle)
        /// </summary>
        /// <returns>CompanyDomainDetails object with organization information</returns>
        public static CompanyDomainDetails GetOrgDomainDetailsWithSessionRefetch()
        {
            try
            {
                // First try to get from session
                var orgDetails = OrgDomainDetails;
                
                // If null (session expired), refetch based on host
                if (orgDetails == null)
                {
                    var host = HttpContextAccessor?.HttpContext?.Request.Host.Host;
                    if (!string.IsNullOrEmpty(host))
                    {
                        var rep = new ELG.DAL.OrgAdminDAL.CompanyRep();
                        orgDetails = rep.GetOrganizationFromHost(host);
                        
                        // Store back in session
                        if (orgDetails != null && orgDetails.CompanyId > 0)
                        {
                            OrgDomainDetails = orgDetails;
                        }
                    }
                }
                
                return orgDetails;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SessionHelper.GetOrgDomainDetailsWithSessionRefetch] Error: {ex.Message}");
                return null;
            }
        }
        #endregion

        #region Public Static Properties
        /// <summary>
        /// Gets/Sets Session for Language
        /// </summary>
        public static string Language
        {
            get
            {
                var value = Session?.GetString("Language");
                return string.IsNullOrEmpty(value) ? "en" : value;
            }
            set
            {
                Session?.SetString("Language", value);
            }
        }

        /// <summary>
        /// Gets/Sets Session for UserId
        /// </summary>
        public static Int64 UserId
        {
            get
            {
                var value = Session?.GetString("UserId");
                return string.IsNullOrEmpty(value) ? 0 : Convert.ToInt64(value);
            }
            set
            {
                Session?.SetString("UserId", value.ToString());
            }
        }

        /// <summary>
        /// Gets/Sets Session for Company
        /// </summary>
        public static Int64 CompanyId
        {
            get
            {
                var value = Session?.GetString("CompanyId");
                return string.IsNullOrEmpty(value) ? 0 : Convert.ToInt64(value);
            }
            set
            {
                Session?.SetString("CompanyId", value.ToString());
            }
        }

        /// <summary>
        /// Gets/Sets Company Name
        /// </summary>
        public static String CompanyName
        {
            get
            {
                return Session?.GetString("CompanyName") ?? string.Empty;
            }
            set
            {
                Session?.SetString("CompanyName", value);
            }
        }

        /// <summary>
        /// Gets/Sets Company Number
        /// </summary>
        public static String CompanyNumber
        {
            get
            {
                return Session?.GetString("CompanyNumber") ?? string.Empty;
            }
            set
            {
                Session?.SetString("CompanyNumber", value);
            }
        }

        /// <summary>
        /// Gets/Sets Company Logo
        /// </summary>
        public static String CompanyLogo
        {
            get
            {
                var value = Session?.GetString("CompanyLogo");
                return string.IsNullOrEmpty(value) ? "" : value;
            }
            set
            {
                Session?.SetString("CompanyLogo", value);
            }
        }

        /// <summary>
        /// Gets/Sets Company Certificate
        /// </summary>
        public static byte[] CompanyCertificate
        {
            get
            {
                var value = Session?.Get("CompanyCertificate");
                return value;
            }
            set
            {
                Session?.Set("CompanyCertificate", value);
            }
        }

        /// <summary>
        /// Gets/Sets Session for Username
        /// </summary>
        public static string UserDisplayName
        {
            get
            {
                var value = Session?.GetString("UserDisplayName");
                return string.IsNullOrEmpty(value) ? "" : value;
            }
            set
            {
                Session?.SetString("UserDisplayName", value);
            }
        }

        /// <summary>
        /// Gets/Sets Session for Username
        /// </summary>
        public static string UserName
        {
            get
            {
                var value = Session?.GetString("UserName");
                return string.IsNullOrEmpty(value) ? "" : value;
            }
            set
            {
                Session?.SetString("UserName", value);
            }
        }

        /// <summary>
        /// Gets/Sets Session for UserRole
        /// </summary>
        public static int UserRole
        {
            get
            {
                var value = Session?.GetString("UserRole");
                return string.IsNullOrEmpty(value) ? 0 : Convert.ToInt32(value);
            }
            set
            {
                Session?.SetString("UserRole", value.ToString());
            }
        }

        /// <summary>
        /// Gets/Sets Session for CourseId
        /// </summary>
        public static Int64 CourseId
        {
            get
            {
                var value = Session?.GetString("CourseId");
                return string.IsNullOrEmpty(value) ? 0 : Convert.ToInt64(value);
            }
            set
            {
                Session?.SetString("CourseId", value.ToString());
            }
        }

        /// <summary>
        /// Gets/Sets Session for Current user to modify/assign admin rights or other
        /// </summary>
        public static Int64 CurrentUserId
        {
            get
            {
                var value = Session?.GetString("CurrentUserId");
                return string.IsNullOrEmpty(value) ? 0 : Convert.ToInt64(value);
            }
            set
            {
                Session?.SetString("CurrentUserId", value.ToString());
            }
        }

        public static OrganizationInfo CompanySettings
        {
            get
            {
                var value = Session?.Get("CompanySettings");
                if (value == null) return new OrganizationInfo();
                return System.Text.Json.JsonSerializer.Deserialize<OrganizationInfo>(System.Text.Encoding.UTF8.GetString(value));
            }
            set
            {
                var json = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(value);
                Session?.Set("CompanySettings", json);
            }
        }

        public static List<AdminPrivilege> AdminPrivileges
        {
            get
            {
                var value = Session?.Get("AdminPrivileges");
                if (value == null) return new List<AdminPrivilege>();
                return System.Text.Json.JsonSerializer.Deserialize<List<AdminPrivilege>>(System.Text.Encoding.UTF8.GetString(value));
            }
            set
            {
                var json = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(value);
                Session?.Set("AdminPrivileges", json);
            }
        }

        /// <summary>
        /// Gets/Sets Company Logo
        /// </summary>
        public static String ProfilePic
        {
            get
            {
                var value = Session?.GetString("ProfilePic");
                return string.IsNullOrEmpty(value) ? "" : value;
            }
            set
            {
                Session?.SetString("ProfilePic", value);
            }
        }
        /// <summary>
        /// Gets/Sets Session for UserId
        /// </summary>
        public static string SAMLCert
        {
            get
            {
                var value = Session?.GetString("SAMLCert");
                return string.IsNullOrEmpty(value) ? "" : value;
            }
            set
            {
                Session?.SetString("SAMLCert", value);
            }
        }

        /// <summary>
        /// if logged in using SSO
        /// </summary>
        public static bool IsSSOLogin
        {
            get
            {
                var value = Session?.GetString("IsSSOLogin");
                return string.IsNullOrEmpty(value) ? false : Convert.ToBoolean(value);
            }
            set
            {
                Session?.SetString("IsSSOLogin", value.ToString());
            }
        }

        /// <summary>
        /// if logged in as learner user
        /// </summary>
        public static bool IsLearnerUser
        {
            get
            {
                var value = Session?.GetString("IsLearnerUser");
                return string.IsNullOrEmpty(value) ? false : Convert.ToBoolean(value);
            }
            set
            {
                Session?.SetString("IsLearnerUser", value.ToString());
            }
        }

        /// <summary>
        /// if user has admin rights
        /// </summary>
        public static bool HasAdminRights
        {
            get
            {
                var value = Session?.GetString("HasAdminRights");
                return string.IsNullOrEmpty(value) ? false : Convert.ToBoolean(value);
            }
            set
            {
                Session?.SetString("HasAdminRights", value.ToString());
            }
        }

        /// <summary>
        /// if user has learner rights
        /// </summary>
        public static bool HasLearnerRights
        {
            get
            {
                var value = Session?.GetString("HasLearnerRights");
                return string.IsNullOrEmpty(value) ? false : Convert.ToBoolean(value);
            }
            set
            {
                Session?.SetString("HasLearnerRights", value.ToString());
            }
        }

        /// <summary>
        /// Gets/Sets Session for Accident Incident feature
        /// </summary>
        public static int AccidentIncidentFeature
        {
            get
            {
                var value = Session?.GetString("AccidentIncidentFeature");
                return string.IsNullOrEmpty(value) ? 0 : Convert.ToInt16(value);
            }
            set
            {
                Session?.SetString("AccidentIncidentFeature", value.ToString());
            }
        }

        /// <summary>
        /// Gets/Sets Session for Training Renewal Mode
        /// </summary>
        public static int TrainingRenewalMode
        {
            get
            {
                var value = Session?.GetString("TrainingRenewalMode");
                return string.IsNullOrEmpty(value) ? 0 : Convert.ToInt16(value);
            }
            set
            {
                Session?.SetString("TrainingRenewalMode", value.ToString());
            }
        }

        /// <summary>
        /// Gets/Sets Session for Course assignment mode
        /// </summary>
        public static int CompanyCourseAssignmentMode
        {
            get
            {
                var value = Session?.GetString("CompanyCourseAssignmentMode");
                return string.IsNullOrEmpty(value) ? 0 : Convert.ToInt16(value);
            }
            set
            {
                Session?.SetString("CompanyCourseAssignmentMode", value.ToString());
            }
        }

        /// <summary>
        /// Gets/Sets Session for organisation details based on domain
        /// </summary>
        public static CompanyDomainDetails OrgDomainDetails
        {
            get
            {
                var value = Session?.Get("OrgDomainDetails");
                if (value == null) return null;
                return System.Text.Json.JsonSerializer.Deserialize<ELG.Model.OrgAdmin.CompanyDomainDetails>(System.Text.Encoding.UTF8.GetString(value));
            }
            set
            {
                if (value == null)
                {
                    Session?.Remove("OrgDomainDetails");
                }
                else
                {
                    var json = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(value);
                    Session?.Set("OrgDomainDetails", json);
                }
            }
        }

        /// <summary>
        /// Gets/Sets Session for organisation details based on domain
        /// </summary>
        public static String OrgAdminAvailableMenu
        {
            get
            {
                var value = Session?.GetString("OrgAdminAvailableMenu");
                return value;
            }
            set
            {
                Session?.SetString("OrgAdminAvailableMenu", value);
            }
        }

        /// <summary>
        /// Gets/Sets learner menu settings for dual-role users.
        /// </summary>
        public static String OrgLearnerAvailableMenu
        {
            get
            {
                var value = Session?.GetString("OrgLearnerAvailableMenu");
                return value;
            }
            set
            {
                Session?.SetString("OrgLearnerAvailableMenu", value ?? string.Empty);
            }
        }

        /// <summary>
        /// Gets/Sets admin UI view mode (classic | modern).
        /// </summary>
        public static string AdminViewMode
        {
            get
            {
                var value = Session?.GetString("AdminViewMode");
                return string.IsNullOrWhiteSpace(value) ? "classic" : value;
            }
            set
            {
                Session?.SetString("AdminViewMode", value);
            }
        }

        /// <summary>
        /// Gets/Sets learner UI view mode (classic | modern).
        /// </summary>
        public static string LearnerViewMode
        {
            get
            {
                var value = Session?.GetString("LearnerViewMode");
                return string.IsNullOrWhiteSpace(value) ? "classic" : value;
            }
            set
            {
                Session?.SetString("LearnerViewMode", value);
            }
        }
        #endregion
    }
}