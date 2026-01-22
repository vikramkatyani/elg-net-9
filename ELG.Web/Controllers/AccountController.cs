using ELG.Model.OrgAdmin;
using ELG.Model.Learner;
using ELG.Web.Helper;
using ELG.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using ELG.DAL.OrgAdminDAL;
using ELG.DAL.LearnerDAL;
using ELG.DAL.Utilities;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;

namespace ELG.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly EmailUtility emailUti;
        private readonly IWebHostEnvironment _env;

        public AccountController(IWebHostEnvironment env)
        {
            _env = env;
            emailUti = new EmailUtility(_env);
        }

        #region Login
        // Unified login for admin and learner users
        public IActionResult Login(LoginViewModel login)
        {
            ELG.Model.OrgAdmin.ControllerResponse response = new ELG.Model.OrgAdmin.ControllerResponse();
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if master password is being used for login
                    Boolean isMasterPwd = login.Password == CommonHelper.GetAppSettingValue("LMS_MasterPassword");

                    // First, try to authenticate as admin
                    var adminAcc = new OrgAdminAccountRep();
                    List<OrgAdminInfo> admins = adminAcc.GetAdmin(login.Email, login.Password, 
                        CommonHelper.GetAppSettingValue("LMS_PasswordEncryptionKey"), isMasterPwd);
                    
                    // If no admin found, try learner authentication
                    List<ELG.Model.Learner.LearnerInfo> learners = null;
                    if (admins == null || admins.Count == 0)
                    {
                        var learnerAcc = new LearnerAccountRep();
                        learners = learnerAcc.GetLearnerInfoByUsernamePassword(login.Email, login.Password, 
                            CommonHelper.GetAppSettingValue("LMS_PasswordEncryptionKey"), isMasterPwd);
                    }

                    // Handle admin authentication (assuming no duplicate account)
                    if (admins != null && admins.Count >= 1)
                    {
                        OrgAdminInfo admin = admins[0];
                        
                        if (!admin.IsCompanyActive)
                        {
                            response.Err = 2;
                            response.Message = "Account Expired – Contact your Administrator";
                            return Json(response);
                        }

                        // Check if admin also has learner rights
                        var learnerAcc = new LearnerAccountRep();
                        var learnerInfo = learnerAcc.GetLearnerInfoByUserID((int)admin.UserID);

                        // Set learner session (login as learner first)
                        if (learnerInfo != null && learnerInfo.UserID > 0)
                        {
                            SetLearnerSessionDetails(learnerInfo);
                        }
                        else
                        {
                            // Create minimal learner object from admin data
                            var minimalLearner = new ELG.Model.Learner.LearnerInfo
                            {
                                UserID = admin.UserID,
                                EmailId = admin.EmailId,
                                FirstName = admin.FirstName,
                                LastName = admin.LastName,
                                CompanyId = admin.CompanyId,
                                CompanyName = admin.CompanyName,
                                CompanyNumber = admin.CompanyNumber,
                                AccidentIncidentFeature = admin.AccidentIncidentFeature,
                                MenuItems = admin.MenuItems,
                                CompanyLogo = admin.CompanyLogo,
                                ProfilePic = admin.ProfilePic,
                                CompanyCertificate = admin.CompanyCertificate
                            };
                            SetLearnerSessionDetails(minimalLearner);
                        }

                        // Mark as having admin rights
                        SessionHelper.HasAdminRights = true;

                        // Get landing page and redirect
                        string landingPage = GetLandingPageFromMenu(SessionHelper.OrgAdminAvailableMenu as string);
                        var redirectUrl = Url.Action(landingPage, "Home", new { area = "Learner" });

                        // Redirect to password reset if needed
                        if (!admin.IsPasswordReset)
                            redirectUrl = emailUti.CreatePasswordResetLink(admin.UserID);

                        response.Err = 0;
                        response.Url = redirectUrl;
                        response.Message = "success";
                        return Json(response);
                    }
                    // Handle learner authentication (assuming no duplicate account)
                    else if (learners != null && learners.Count >= 1)
                    {
                        ELG.Model.Learner.LearnerInfo learner = learners[0];
                        
                        if (learner.IsDeactivated)
                        {
                            response.Err = 2;
                            response.Message = "Account not active. Please contact your training department";
                            return Json(response);
                        }
                        
                        if (!learner.IsCompanyActive)
                        {
                            response.Err = 2;
                            response.Message = "Company account not active. Please contact your training department";
                            return Json(response);
                        }

                        // Check if learner has admin rights
                        var learnerAccForRights = new LearnerAccountRep();
                        int hasAdminRights = learnerAccForRights.CheckIfLearnerHasAdminRights(learner.UserID);

                        // Set learner session
                        SetLearnerSessionDetails(learner);
                        SessionHelper.HasAdminRights = hasAdminRights > 0;

                        // Get landing page and redirect
                        string landingPage = GetLandingPageFromMenu(SessionHelper.OrgAdminAvailableMenu as string);
                        var redirectUrl = Url.Action(landingPage, "Home", new { area = "Learner" });

                        // Redirect to password reset if needed
                        if (!learner.IsPasswordReset)
                            redirectUrl = emailUti.CreatePasswordResetLink((int)learner.UserID);

                        response.Err = 0;
                        response.Url = redirectUrl;
                        response.Message = "success";
                        return Json(response);
                    }
                    else
                    {
                        response.Err = 1;
                        response.Message = "Invalid credentials";
                        return Json(response);
                    }
                }
                // If not valid, show login view or redirect
                if (!this.User.Identity.IsAuthenticated)
                {
                    return View(login);
                }
                else
                {
                    // Redirect to configured landing page
                    string landingPage = GetLandingPageFromMenu(SessionHelper.OrgAdminAvailableMenu as string);
                    return this.RedirectToAction(landingPage, "Home");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                ModelState.AddModelError("", "Something went wrong. Please try after sometime.");
                response.Err = 2;
                response.Message = "Something went wrong!";
                return Json(response);
            }
        }


        // GET: Account, validate login credentials for organisation admin
        public IActionResult ValidateCompany()
        {
            var orgList = TempData["adminOrganisationList"] as List<OrgAdminInfo>;
            return View("ValidateCompany", orgList);
        }

        // GET: Account, validate login credentials for organisation admin
        public IActionResult ValidateAdminCompany(string companyNumber)
        {
            var orgList = TempData["adminOrganisationList"] as List<OrgAdminInfo>;
            ELG.Model.OrgAdmin.ControllerResponse response = new ELG.Model.OrgAdmin.ControllerResponse();
            try
            {
                if (String.IsNullOrEmpty(companyNumber))
                {
                    return View("ValidateCompany", orgList);
                }
                else
                {
                    var acc = new OrgAdminAccountRep();
                    OrgAdminInfo admin = acc.GetOrgAdmin(companyNumber, Convert.ToString(SessionHelper.UserName));
                    if (admin != null && admin.CompanyId > 0)
                    {
                        // set session value for UserName
                        // TODO ASP.NET membership should be replaced with ASP.NET Core identity. For more details see https://docs.microsoft.com/aspnet/core/migration/proper-to-2x/membership-to-core-identity.
                                                // TODO: Set authentication cookie using ASP.NET Core Identity if needed
                        SessionHelper.UserId = admin.UserID;
                        SessionHelper.UserName = admin.EmailId;
                        SessionHelper.UserDisplayName = admin.FirstName + " " + admin.LastName;
                        SessionHelper.CompanyId = admin.CompanyId;
                        SessionHelper.CompanyName = admin.CompanyName;
                        SessionHelper.CompanyNumber = admin.CompanyNumber;
                        SessionHelper.AccidentIncidentFeature = admin.AccidentIncidentFeature;
                        SessionHelper.TrainingRenewalMode = admin.TrainingRenewalMode;
                        SessionHelper.CompanyCourseAssignmentMode = admin.CourseAssignMode;
                        
                        // Check if admin has learner rights and use learner menu if available
                        var learnerAccRep = new LearnerAccountRep();
                        var learnerInfo = learnerAccRep.GetLearnerInfoByUserID((int)admin.UserID);
                        
                        if (learnerInfo != null && !string.IsNullOrEmpty(learnerInfo.MenuItems))
                        {
                            SessionHelper.OrgAdminAvailableMenu = learnerInfo.MenuItems;
                        }
                        else
                        {
                            SessionHelper.OrgAdminAvailableMenu = admin.MenuItems;
                        }
                        
                        // IMPORTANT: Login as learner, not admin
                        SessionHelper.IsLearnerUser = true;
                        SessionHelper.HasAdminRights = true;

                        if (admin.CompanyLogo != null)
                        {
                            string companyLogoBase64Data = Convert.ToBase64String(admin.CompanyLogo);
                            SessionHelper.CompanyLogo = string.Format("data:image/png;base64,{0}", companyLogoBase64Data);
                        }


                        if (admin.ProfilePic != null)
                        {
                            string learnerProfileBase64Data = Convert.ToBase64String(admin.ProfilePic);
                            SessionHelper.ProfilePic = string.Format("data:image/png;base64,{0}", learnerProfileBase64Data);
                        }


                        SessionHelper.CompanyCertificate = admin.CompanyCertificate;
                        SessionHelper.IsSSOLogin = false;

                        // Skip admin privilege setup since we're logging in as learner
                        // SetSessionCompanySettings(Convert.ToInt32(admin.CompanyId));
                        // SetSessionAdminPriveleges(Convert.ToInt32(admin.UserID));

                        // Read landing page from learner menu settings
                        var learnerMenuSettingsJson = SessionHelper.OrgAdminAvailableMenu as string;
                        dynamic learnerMenuSettings = !string.IsNullOrEmpty(learnerMenuSettingsJson)
                            ? Newtonsoft.Json.JsonConvert.DeserializeObject(learnerMenuSettingsJson)
                            : null;

                        // Safely extract landingPage with default fallback to "Dashboard"
                        string landingPage = learnerMenuSettings?.landingPage ?? "Dashboard";

                        // Redirect to learner view using the configured landing page
                        var redirectUrl = Url.Action(landingPage, "Home", new { area = "Learner" });
                        response.Err = 0;
                        response.Url = redirectUrl;
                        response.Message = "Success!";
                    }
                    else
                    {
                        response.Err = 1;
                        response.Url = "";
                        response.Message = "Invalid company number!";
                    }
                }

                return Json(response);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                response.Err = 1;
                response.Url = "";
                response.Message = "Something went wrong!";
                return Json(response);
            }
        }

        // GET: Validate learner company
        public IActionResult ValidateLearnerCompany()
        {
            var learnerList = TempData["learnerOrganisationList"] as List<ELG.Model.Learner.LearnerInfo>;
            return View("ValidateLearnerCompany", learnerList);
        }

        // POST: Validate learner company number
        public IActionResult ValidateLearnerCompanyNumber(string companyNumber)
        {
            var learnerList = TempData["learnerOrganisationList"] as List<ELG.Model.Learner.LearnerInfo>;
            ELG.Model.OrgAdmin.ControllerResponse response = new ELG.Model.OrgAdmin.ControllerResponse();
            try
            {
                if (String.IsNullOrEmpty(companyNumber))
                {
                    return View("ValidateLearnerCompany", learnerList);
                }
                else
                {
                    var acc = new LearnerAccountRep();
                    var learner = learnerList?.FirstOrDefault(l => l.CompanyNumber == companyNumber);
                    
                    if (learner != null && learner.CompanyId > 0)
                    {
                        // Set session variables for learner
                        SessionHelper.UserId = learner.UserID;
                        SessionHelper.UserName = learner.EmailId;
                        SessionHelper.UserDisplayName = learner.FirstName + " " + learner.LastName;
                        SessionHelper.CompanyId = learner.CompanyId;
                        SessionHelper.CompanyName = learner.CompanyName;
                        SessionHelper.CompanyNumber = learner.CompanyNumber;
                        SessionHelper.AccidentIncidentFeature = learner.AccidentIncidentFeature;
                        SessionHelper.IsLearnerUser = true;

                        if (learner.CompanyLogo != null)
                        {
                            string companyLogoBase64Data = Convert.ToBase64String(learner.CompanyLogo);
                            SessionHelper.CompanyLogo = string.Format("data:image/png;base64,{0}", companyLogoBase64Data);
                        }

                        if (learner.ProfilePic != null)
                        {
                            string learnerProfileBase64Data = Convert.ToBase64String(learner.ProfilePic);
                            SessionHelper.ProfilePic = string.Format("data:image/png;base64,{0}", learnerProfileBase64Data);
                        }

                        SessionHelper.CompanyCertificate = learner.CompanyCertificate;
                        SessionHelper.IsSSOLogin = false;

                        // Redirect to learner dashboard
                        var redirectUrl = Url.Action("Dashboard", "Home", new { area = "Learner" });
                        response.Err = 0;
                        response.Url = redirectUrl;
                        response.Message = "Success!";
                    }
                    else
                    {
                        response.Err = 1;
                        response.Url = "";
                        response.Message = "Invalid company number!";
                    }
                }

                return Json(response);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                response.Err = 1;
                response.Url = "";
                response.Message = "Something went wrong!";
                return Json(response);
            }
        }

        #region Toggle View Between Admin and Learner

        // GET: Switch to learner view (no password required)
        public IActionResult SwitchToLearnerView()
        {
            try
            {
                if (!SessionHelper.HasLearnerRights)
                {
                    return Json(new { err = 1, message = "You do not have learner access rights" });
                }

                // Load learner information from database
                var learnerAcc = new LearnerAccountRep();
                var learnerInfo = learnerAcc.GetLearnerInfoByUserID((int)SessionHelper.UserId);

                if (learnerInfo == null || learnerInfo.UserID == 0)
                {
                    return Json(new { err = 1, message = "Unable to load learner information" });
                }

                // Reset and set learner session variables
                SetLearnerSessionDetails(learnerInfo);

                // Get configured landing page
                string landingPage = GetLandingPageFromMenu(learnerInfo.MenuItems);
                var redirectUrl = Url.Action(landingPage, "Home", new { area = "Learner" });

                return Json(new { err = 0, url = redirectUrl, message = "Switched to learner view" });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Json(new { err = 1, message = "Error switching view" });
            }
        }

        // POST: Verify password before switching to admin view
        [HttpPost]
        public IActionResult SwitchToAdminView(string password)
        {
            ELG.Model.OrgAdmin.ControllerResponse response = new ELG.Model.OrgAdmin.ControllerResponse();
            try
            {
                if (!SessionHelper.HasAdminRights)
                {
                    response.Err = 1;
                    response.Message = "You do not have admin access rights";
                    return Json(response);
                }

                if (string.IsNullOrEmpty(password))
                {
                    response.Err = 1;
                    response.Message = "Password is required";
                    return Json(response);
                }

                // Verify password
                var acc = new OrgAdminAccountRep();
                Boolean isMasterPwd = false;
                if (password == CommonHelper.GetAppSettingValue("LMS_MasterPassword"))
                    isMasterPwd = true;

                List<OrgAdminInfo> admins = acc.GetAdmin(SessionHelper.UserName, password, 
                    CommonHelper.GetAppSettingValue("LMS_PasswordEncryptionKey"), isMasterPwd);

                if (admins != null && admins.Count > 0)
                {
                    // Password verified, reset and set admin session
                    OrgAdminInfo admin = admins[0];
                    SetAdminSessionDetails(admin);

                    // Get configured landing page from admin menu
                    string landingPage = GetLandingPageFromMenu(admin.MenuItems);
                    var redirectUrl = Url.Action(landingPage, "Home");

                    response.Err = 0;
                    response.Url = redirectUrl;
                    response.Message = "Switched to admin view";
                }
                else
                {
                    response.Err = 1;
                    response.Message = "Invalid password";
                }

                return Json(response);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                response.Err = 1;
                response.Message = "Error switching view";
                return Json(response);
            }
        }

        #endregion

        #region Session Management Helpers

        /// <summary>
        /// Sets all admin-specific session variables
        /// </summary>
        private void SetAdminSessionDetails(OrgAdminInfo admin)
        {
            SessionHelper.UserId = admin.UserID;
            SessionHelper.UserName = admin.EmailId;
            SessionHelper.UserDisplayName = admin.FirstName + " " + admin.LastName;
            SessionHelper.CompanyId = admin.CompanyId;
            SessionHelper.CompanyName = admin.CompanyName;
            SessionHelper.CompanyNumber = admin.CompanyNumber;
            SessionHelper.AccidentIncidentFeature = admin.AccidentIncidentFeature;
            SessionHelper.TrainingRenewalMode = admin.TrainingRenewalMode;
            SessionHelper.CompanyCourseAssignmentMode = admin.CourseAssignMode;
            SessionHelper.OrgAdminAvailableMenu = admin.MenuItems;
            SessionHelper.IsLearnerUser = false;
            SessionHelper.HasAdminRights = true;

            if (admin.CompanyLogo != null)
            {
                string companyLogoBase64Data = Convert.ToBase64String(admin.CompanyLogo);
                SessionHelper.CompanyLogo = string.Format("data:image/png;base64,{0}", companyLogoBase64Data);
            }
            if (admin.ProfilePic != null)
            {
                string learnerProfileBase64Data = Convert.ToBase64String(admin.ProfilePic);
                SessionHelper.ProfilePic = string.Format("data:image/png;base64,{0}", learnerProfileBase64Data);
            }
            if (admin.CompanyCertificate != null)
            {
                SessionHelper.CompanyCertificate = admin.CompanyCertificate;
            }

            SessionHelper.IsSSOLogin = false;

            // Set admin-specific session settings
            SetSessionCompanySettings(Convert.ToInt32(admin.CompanyId));
            SetSessionAdminPriveleges(Convert.ToInt32(admin.UserID));
        }

        /// <summary>
        /// Sets all learner-specific session variables
        /// </summary>
        private void SetLearnerSessionDetails(ELG.Model.Learner.LearnerInfo learner)
        {
            SessionHelper.UserId = learner.UserID;
            SessionHelper.UserName = learner.EmailId;
            SessionHelper.UserDisplayName = learner.FirstName + " " + learner.LastName;
            SessionHelper.CompanyId = learner.CompanyId;
            SessionHelper.CompanyName = learner.CompanyName;
            SessionHelper.CompanyNumber = learner.CompanyNumber;
            SessionHelper.AccidentIncidentFeature = learner.AccidentIncidentFeature;
            SessionHelper.OrgAdminAvailableMenu = learner.MenuItems;
            SessionHelper.IsLearnerUser = true;
            SessionHelper.HasLearnerRights = true;

            if (learner.CompanyLogo != null)
            {
                string companyLogoBase64Data = Convert.ToBase64String(learner.CompanyLogo);
                SessionHelper.CompanyLogo = string.Format("data:image/png;base64,{0}", companyLogoBase64Data);
            }
            if (learner.ProfilePic != null)
            {
                string learnerProfileBase64Data = Convert.ToBase64String(learner.ProfilePic);
                SessionHelper.ProfilePic = string.Format("data:image/png;base64,{0}", learnerProfileBase64Data);
            }
            if (learner.CompanyCertificate != null)
            {
                SessionHelper.CompanyCertificate = learner.CompanyCertificate;
            }

            SessionHelper.IsSSOLogin = false;
        }

        /// <summary>
        /// Gets the configured landing page from menu settings JSON
        /// </summary>
        private string GetLandingPageFromMenu(string menuJson)
        {
            if (string.IsNullOrEmpty(menuJson))
                return "Dashboard";

            try
            {
                dynamic menuSettings = Newtonsoft.Json.JsonConvert.DeserializeObject(menuJson);
                return menuSettings?.landingPage ?? "Dashboard";
            }
            catch
            {
                return "Dashboard";
            }
        }

        #endregion

        private void SetSessionCompanySettings(int CompanyId)
        {

            var adminRep = new AdminRep();
            OrganizationInfo regSettings = adminRep.GetAdminOrgRegSettings(CompanyId);
            SessionHelper.CompanySettings = regSettings;
        }


        //set admin roles and 
        private void SetSessionAdminPriveleges(Int64 adminUserId)
        {
            var adminRep = new AdminRep();
            List<AdminPrivilege> adminPriv = adminRep.GetAdminPrevelageSettings(adminUserId);
            SessionHelper.AdminPrivileges = adminPriv;
            if (adminPriv != null && adminPriv.Count > 0)
            {
                var admin = adminPriv.First();
                SessionHelper.UserRole = admin.AdminRole;
            }
            
        }

        //Logout 
        public ActionResult LogOut()
        {
            bool isssologin = (SessionHelper.IsSSOLogin);

            // TODO: Clear session using ASP.NET Core session management if needed
            // TODO ASP.NET membership should be replaced with ASP.NET Core identity. For more details see https://docs.microsoft.com/aspnet/core/migration/proper-to-2x/membership-to-core-identity.
            // TODO: Sign out using ASP.NET Core Identity if needed
            if (isssologin)
                return RedirectToAction("Index", "SSOAccount");
            else
                return RedirectToAction("Login");
        }
        #endregion
        
        #region Forget Pwd
        public ActionResult ForgetPassword(ForgetPasswordViewModel frgtPwd)
        {
            ELG.Model.OrgAdmin.ControllerResponse response = new ELG.Model.OrgAdmin.ControllerResponse();
            try
            {

                if (ModelState.IsValid)
                {
                    var acc = new OrgAdminAccountRep();

                    // check if master password is being used for login
                    Boolean isMasterPwd = true;

                    List<OrgAdminInfo> learner = acc.GetAdmin(frgtPwd.Email, "", CommonHelper.GetAppSettingValue("LMS_PasswordEncryptionKey"), isMasterPwd);
                    if (learner != null && learner.Count > 1)
                    {
                        // TODO ASP.NET membership should be replaced with ASP.NET Core identity. For more details see https://docs.microsoft.com/aspnet/core/migration/proper-to-2x/membership-to-core-identity.
                        // TODO: Set authentication cookie using ASP.NET Core Identity if needed
                        SessionHelper.UserName = learner[0].EmailId;
                        TempData["adminOrganisationList_forgot"] = learner;

                        // prompt to validate company number if more than 1 records are found with same credentials
                        var redirectUrl = Url.Action("ValidateCompanyForgetPassword");

                        response.Err = 0;
                        response.Url = redirectUrl;
                        response.Message = "Multiple records";

                    }
                    else if (learner != null && learner.Count == 1)
                    {


                        var companyRep = new ELG.DAL.OrgAdminDAL.CompanyRep();
                        var org = companyRep.GetCompanyInfo(learner.FirstOrDefault().CompanyId);

                        string learnerEmail = learner.FirstOrDefault().EmailId;
                        string link = emailUti.CreateLoginLinkToBeSendInMail(org.CompanyBaseURL, (int)learner.FirstOrDefault().UserID);
                        string emailTemplate = emailUti.GetEmailTemplate("ForgetPwdEmail.html");
                        emailTemplate = emailTemplate.Replace("{loginLink}", link);
                        emailTemplate = emailTemplate.Replace("{useremail}", learnerEmail);

                        acc.ResetPasswordFlag((int)learner.FirstOrDefault().UserID);
                        // emailUti.SendEMail(learnerEmail, "Password Reset", emailTemplate);
                       emailUti.SendEmailUsingSendGrid(learnerEmail, "Password Reset", emailTemplate);
                        response.Err = 0;
                        response.Url = "";
                        response.Message = "Your reset password link has been sent to your registered email address. Please check your email and follow given instructions.";
                    }
                    else
                    {
                        response.Err = 1;
                        response.Url = "";
                        response.Message = "Invalid Email/Username";
                    }

                    return Json(response);
                }
                else
                {
                    return View();
                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                ModelState.AddModelError("", "Something went wrong. Please try after sometime.");
                response.Err = 2;
                response.Url = "";
                response.Message = "Something went wrong!";
                return Json(response);

            }
        }

        public ActionResult ValidateCompanyForgetPassword()
        {
            var orgList = TempData["adminOrganisationList_forgot"] as List<OrgAdminInfo>;
            return View("ValidateCompanyForgetPassword", orgList);
        }

        // GET: Account, validate login credentials for Learner for Forget Password
        public ActionResult ValidateCompany_ForgetPassword(String companyNumber)
        {
            ELG.Model.OrgAdmin.ControllerResponse response = new ELG.Model.OrgAdmin.ControllerResponse();
            try
            {
                var acc = new OrgAdminAccountRep();
                OrgAdminInfo admin = acc.GetOrgAdmin(companyNumber, Convert.ToString(SessionHelper.UserName));
                if (String.IsNullOrEmpty(companyNumber))
                {
                    return View("ValidateCompanyForgetPassword");
                }
                else if (admin != null && admin.CompanyId > 0)
                {
                    string link = emailUti.CreateLoginLinkToBeSendInMail((int)admin.UserID);
                    string emailTemplate = emailUti.GetEmailTemplate("ForgetPwdEmail.html");
                    emailTemplate = emailTemplate.Replace("{loginLink}", link);

                    acc.ResetPasswordFlag((int)admin.UserID);
                    //emailUti.SendEMail(admin.EmailId, "Password Reset", emailTemplate);
                    emailUti.SendEmailUsingSendGrid(admin.EmailId, "Password Reset", emailTemplate);
                    response.Err = 0;
                    response.Url = "";
                    response.Message = "Your reset password link has been sent to your registered email address. Please check your email and follow given instructions.";
                }
                else
                {
                    response.Err = 1;
                    response.Url = "";
                    response.Message = "Invalid company number!";
                }

                return Json(response);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                response.Err = 1;
                response.Url = "";
                response.Message = "Something went wrong!";
                return Json(response);
            }
        }
        #endregion

        #region Generate Login Link
        [AllowAnonymous]
        [HttpGet]
        public ActionResult GenerateLoginLink()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult GenerateLoginLink([FromBody] ForgetPasswordViewModel frgtPwd)
        {
            ELG.Model.OrgAdmin.ControllerResponse response = new ELG.Model.OrgAdmin.ControllerResponse();
            try
            {
                if (ModelState.IsValid)
                {
                    var acc = new OrgAdminAccountRep();
                    // check if master password is being used for login
                    Boolean isMasterPwd = true;
                    List<OrgAdminInfo> admin = acc.GetAdmin(frgtPwd.Email, "", CommonHelper.GetAppSettingValue("LMS_PasswordEncryptionKey"), isMasterPwd);
                    if (admin != null && admin.Count > 1)
                    {
                        SessionHelper.UserName = admin[0].EmailId;
                        var redirectUrl = Url.Action("ValidateCompanyGenerateLoginLink");
                        response.Err = 0;
                        response.Url = redirectUrl;
                        response.Message = "Multiple records";
                    }
                    else if (admin != null && admin.Count == 1)
                    {
                        var companyRep = new ELG.DAL.OrgAdminDAL.CompanyRep();
                        var org = companyRep.GetCompanyInfo(admin.FirstOrDefault().CompanyId);
                        string subject = $"{org.CompanyBrandName}: Account Login Link Inside";
                        string adminEmail = admin.FirstOrDefault().EmailId;
                        //var baseUrl = CommonHelper.GetAppSettingValue("LMS_BaseURL");
                        var baseUrl = org.CompanyBaseURL; 

                        var pathBase = Environment.GetEnvironmentVariable("ASPNETCORE_PATHBASE");
                        if (!string.IsNullOrEmpty(baseUrl) && !string.IsNullOrEmpty(pathBase) && !baseUrl.Contains(pathBase, StringComparison.OrdinalIgnoreCase))
                        {
                            baseUrl = baseUrl.TrimEnd('/') + pathBase;
                        }
                        if (string.IsNullOrEmpty(baseUrl))
                        {
                            baseUrl = org.CompanyBaseURL;
                        }
                        string link = emailUti.CreateDirectLoginLinkToBeSendInMail(baseUrl, (int)admin.FirstOrDefault().UserID);
                        string emailTemplate = emailUti.GetEmailTemplate("DirectLoginLink.html");
                        emailTemplate = emailTemplate.Replace("{username}", $"{adminEmail}");
                        emailTemplate = emailTemplate.Replace("{tenantbrandname}", org.CompanyBrandName);
                        emailTemplate = emailTemplate.Replace("{tenantname}", org.CompanyName);
                        emailTemplate = emailTemplate.Replace("{tenantcontactemail}", org.CompanySupportEmail);
                        emailTemplate = emailTemplate.Replace("{loginLink}", link);
                        emailTemplate = emailTemplate.Replace("{useremail}", adminEmail);
                        emailUti.SendEmailUsingSendGrid(adminEmail, subject, emailTemplate);
                        response.Err = 0;
                        response.Url = "";
                        response.Message = "Login link has been sent to your registered email address. Please check your email and follow given instructions.";
                    }
                    else
                    {
                        response.Err = 1;
                        response.Url = "";
                        response.Message = "Invalid Email";
                    }
                }
                else
                {
                    response.Err = 1;
                    response.Url = "";
                    response.Message = "Invalid input. Please check your email and try again.";
                }
                return Json(response);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                response.Err = 2;
                response.Url = "";
                response.Message = ex.Message;
                return Json(response);
            }
        }

        [AllowAnonymous]
        public ActionResult ValidateCompanyGenerateLoginLink()
        {
            var orgList = TempData["adminOrganisationList_generateLink"] as List<OrgAdminInfo>;
            return View("ValidateCompanyGenerateLoginLink", orgList);
        }

        [AllowAnonymous]
        public ActionResult ActiveLogin(string id)
        {
            try
            {
                string sid = emailUti.DecodeString(id);

                if (!sid.Contains("~") || sid.Split('~').Count() != 2)
                {
                    ViewBag.Error = 1;
                    ViewBag.Message = "Invalid Request";
                    return View("GenerateLoginLink");
                }

                int userId = Convert.ToInt32(sid.Split('~')[0]);
                DateTime issuedAt = DateTimeOffset.Parse(sid.Split('~')[1], null, DateTimeStyles.RoundtripKind).UtcDateTime;

                var expirySetting = CommonHelper.GetAppSettingValue("LMS_LoginLinkExpiryPeriod");
                double expiryHours = 4;
                if (!string.IsNullOrEmpty(expirySetting) && double.TryParse(expirySetting, out var parsed))
                {
                    expiryHours = parsed;
                }

                if ((DateTime.UtcNow - issuedAt).TotalHours > expiryHours)
                {
                    ViewBag.Error = 2;
                    ViewBag.Message = "Login link expired";
                    return View("GenerateLoginLink");
                }

                var acc = new OrgAdminAccountRep();
                var adminInfo = acc.GetNewUserInfoOrResetPasswordInfo(userId);
                if (adminInfo == null || adminInfo.UserID <= 0)
                {
                    ViewBag.Error = 1;
                    ViewBag.Message = "Invalid Request";
                    return View("GenerateLoginLink");
                }

                // Prefer learner profile if it exists so menus/landing page are consistent
                var learnerAcc = new LearnerAccountRep();
                var learnerInfo = learnerAcc.GetLearnerInfoByUserID(userId);

                if (learnerInfo != null && learnerInfo.UserID > 0)
                {
                    SetLearnerSessionDetails(learnerInfo);
                }
                else
                {
                    // Minimal session from admin data
                    var companyRep = new ELG.DAL.OrgAdminDAL.CompanyRep();
                    var company = companyRep.GetCompanyInfo(adminInfo.CompanyId);

                    var minimalLearner = new ELG.Model.Learner.LearnerInfo
                    {
                        UserID = adminInfo.UserID,
                        EmailId = adminInfo.EmailId,
                        FirstName = adminInfo.FirstName,
                        LastName = adminInfo.LastName,
                        CompanyId = adminInfo.CompanyId,
                        CompanyName = company?.CompanyName,
                        CompanyNumber = company?.CompanyNumber,
                        AccidentIncidentFeature = 0
                    };

                    SetLearnerSessionDetails(minimalLearner);
                }

                SessionHelper.HasAdminRights = true;
                SessionHelper.IsSSOLogin = false;
                SetSessionCompanySettings(Convert.ToInt32(adminInfo.CompanyId));
                SetSessionAdminPriveleges(adminInfo.UserID);

                string landingPage = GetLandingPageFromMenu(SessionHelper.OrgAdminAvailableMenu as string);
                var redirectUrl = Url.Action(landingPage, "Home", new { area = "Learner" });
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                ViewBag.Error = 1;
                ViewBag.Message = "Invalid login link";
                return View("GenerateLoginLink");
            }
        }

        #endregion

        #region Activate User
        public ActionResult ActivateUser(string id)
        {
            try
            {
                var acc = new OrgAdminAccountRep();

                string sid = emailUti.DecodeString(id);

                if (!sid.Contains("~") || sid.Split('~').Count() != 2)
                {
                    ViewBag.Error = 1;
                    ViewBag.Message = "Invalid Request";
                    return View();
                }

                //string userID = sid.Split('~')[0];
                //string activationTime = sid.Split('~')[1];

                //CultureInfo provider = System.Globalization.CultureInfo.InvariantCulture;
                //DateTime acttime = DateTime.ParseExact(activationTime, "dd/MM/yyyy HH:mm:ss", new CultureInfo("fr-FR"));
                //int UserID = Convert.ToInt32(userID);

                int UserID = Convert.ToInt32(sid.Split('~')[0]);
                DateTime acttime = DateTimeOffset.Parse(sid.Split('~')[1]).DateTime;


                var result = (DateTime.UtcNow - acttime).TotalDays;

                if (result > Convert.ToInt32(ELG.Web.Helper.CommonHelper.GetAppSettingValue("LMS_PasswordLinkExpiryPeriod")))
                {
                    ViewBag.Error = 2;
                    return View();
                }

                OrgAdminInfo learner = acc.GetNewUserInfoOrResetPasswordInfo(UserID);
                if (learner != null && learner.UserID > 0)
                {
                    ViewBag.Error = 3;// Account Already activated
                    return View();
                }

                ViewBag.Error = 0;
                ViewBag.UserID = UserID;
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = 1;
                ViewBag.Message = ex.Message;
                return View();
            }
        }

        // GET: Account, validate login credentials for Learner for Forget Password
        public ActionResult ActivateUserPassword(ActivateUserViewModel actUser)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var acc = new OrgAdminAccountRep();
                    acc.UpdateAdminPassword(actUser.UserID, actUser.Password, CommonHelper.GetAppSettingValue("LMS_PasswordEncryptionKey"));
                    ViewBag.Error = 10;
                    ViewBag.Message = "Your password has been updated.";
                    return View("ActivateUser", actUser);
                }
                else
                {
                    return View("ActivateUser", actUser);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("ActivateUser", actUser);
            }
        }
        #endregion
    }
}