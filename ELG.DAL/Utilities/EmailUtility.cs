using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace ELG.DAL.Utilities
{
    public class EmailUtility
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;
        readonly static string CcEmailIDs = ConfigurationManager.AppSettings["CcEmailIDs"];

        public EmailUtility(IWebHostEnvironment env)
        {
            _env = env;
            _configuration = new ConfigurationBuilder()
                .SetBasePath(_env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();
        }

        //public void SendUserCourseRegistrationEmail(string orgSqnNumber, string courseName, string strEmail, int userId, bool isNewUser)
        //{
        //    try
        //    {
        //        if (isNewUser)
        //        {
        //            SendAccountActivationMail(userId, strEmail, orgSqnNumber);
        //            LogHelper.Info("user creation email sent to user" + strEmail);
        //        }
        //        SendCourseAssignedMail(courseName, strEmail, orgSqnNumber);

        //        LogHelper.Info("course assigned email sent to user" + strEmail);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.Error(ex);
        //    }
        //}

        ///// <summary>
        ///// function to send activation link in email after creation of new user
        ///// </summary>
        ///// <param name="userId"></param>
        ///// <param name="strEmail"></param>
        ///// <param name="companyNumber"></param>
        ///// <returns></returns>
        //public void SendAccountActivationMail(int userId, string strEmail, string companyNumber)
        //{
        //    //mail format for web version						
        //    string strLoginLink = CreateLoginLinkToBeSendInMail(userId);
        //    string strMailContent = ComposeNewuserMail(strLoginLink, companyNumber, strEmail);
        //    SendEMail(strEmail, "Account Activation", strMailContent);
        //}
        
        // function to create random string to be appended in the login link
        private string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

        //function to encode password into base64 string
        private string EncodeString(string stringToEncode)
        {
            var encodedString = Encoding.UTF8.GetBytes(stringToEncode);
            return Convert.ToBase64String(encodedString);
        }
        
        //function to decode password into base64 string
        public string DecodeString(string stringToDecode)
        {
            byte[] base64 = Convert.FromBase64String(stringToDecode);
            return Encoding.UTF8.GetString(base64);
        }

        public string ComposeNewuserMail(string loginLink, string companyNumber, string usrnameEmail)
        {
            string mailContent = "<div style='font-family:arial;font-size: 16px;'><p>Dear Trainee,</p><p>Welcome to the Webportal!</p><p>Please click on the link below to set a desired password:<br /><a href=" + loginLink + ">" + loginLink + "</a></p>";
            mailContent += "<p>If clicking the link does not seem to work, you can copy and paste the link into your browser's address window.</p>";
            mailContent += "<p><b>Please note – Above is <span style='text-decoration:underline;'>ONE TIME</span> link only, to be used only for the first time login. Please see below if you are a returning user.</b></p><br />";
            mailContent += "<table border='1' cellspacing='0' cellpadding='0' style='background:rgb(230,230,230);border-collapse:collapse;'><tbody><tr>";
            mailContent += "<td style='padding:0cm 5.4pt'><p><b><span style='font-family:Verdana;background:white'>RETURNING USERS:</span></b><span lang='EN-US'></span></p><p style='background:white'>Please follow the steps below to login:</p>";
            mailContent += "<p style='background:white'>STEP-1: Visit <a href='https://www.atfwebportal.co.uk/' target='_blank'>https://www.atfwebportal.co.uk</a></p>";
            mailContent += "<p style='background:white'>STEP-2: Enter your company number - <b>" + companyNumber + "</b></p>";
            mailContent += "<p style='background:white'>STEP-3: Enter your user name - <b>" + usrnameEmail + "</b></p>";
            mailContent += "<p style='background:white'>STEP-4: Enter your Password</p>";
            mailContent += "</td></tr></tbody></table><br /><p style='font-size: 12px;'>Don't remember your password?<br />Please click on “forget password” link to request password reset email on your registered email id. Or contact your manager for a one-time-password.</p></div>";

            return mailContent;
        }

        // send  email
        public void SendEmailUsingSendGrid(string strTo, string subject, string body)
        {
            try
            {
                var apiKey = GetSetting("sendGridAPIKey") ?? GetSetting("SendGridAPIKey");
                var client = new SendGridClient(apiKey);
                SendGridMessage sendGridMessage = new SendGridMessage
                {
                    From = new EmailAddress(GetSetting("strEmailUserName"), GetSetting("strEmailSenderName")),
                };
                sendGridMessage.AddTo(new EmailAddress(Convert.ToString(strTo)));

                sendGridMessage.SetSubject(subject);
                sendGridMessage.AddContent("text/html", body);
                client.SendEmailAsync(sendGridMessage);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // send licence consumption email
        public void SendNoLicenceEmailUsingSendGrid(string[] strTo, string subject, string body)
        {
            try
            {
                var apiKey = GetSetting("sendGridAPIKey") ?? GetSetting("SendGridAPIKey");
                var client = new SendGridClient(apiKey);
                SendGridMessage sendGridMessage = new SendGridMessage
                {
                    From = new EmailAddress(GetSetting("strEmailUserName"), GetSetting("strEmailSenderName")),
                };

                foreach(string str in strTo)
                {
                    if(!string.IsNullOrEmpty(str))
                        sendGridMessage.AddTo(new EmailAddress(Convert.ToString(str)));
                }
                
                var ccEmail = GetSetting("LMS_SenderEmail");
                if (!string.IsNullOrEmpty(ccEmail))
                {
                    sendGridMessage.AddCc(new EmailAddress(ccEmail));
                }

                sendGridMessage.SetSubject(subject);
                sendGridMessage.AddContent("text/html", body);
                client.SendEmailAsync(sendGridMessage);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //send email using sendgrid

        public Task SendEmailUsingSendGrid_Async(string strTo, string subject, string body)
        {
            return Execute(GetSetting("sendGridAPIKey") ?? GetSetting("SendGridAPIKey"), subject, body, strTo);
        }

        public Task Execute(string apiKey, string subject, string message, string email)
        {
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(GetSetting("strEmailUserName"), GetSetting("strEmailSenderName")),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(email));

            return client.SendEmailAsync(msg);
        }

        public void SendEMail(string strTo, string subject, string body)
        {
            MailAddress addressFrom = new MailAddress(GetSetting("strEmailSenderName"));
            MailAddress addressTo = new MailAddress(strTo);

            MailMessage message = new MailMessage(addressFrom, addressTo)
            {
                Subject = subject,
                Body = body
            };

            // Construct the alternate body as HTML.
            string htmlBody = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">";
            htmlBody += "<HTML><HEAD><META http-equiv=Content-Type content=\"text/html; charset=iso-8859-1\"></HEAD><BODY>";
            htmlBody += body;
            htmlBody += "</BODY></HTML>";

            ContentType mimeType = new System.Net.Mime.ContentType("text/html");
            // Add the alternate body to the message.

            AlternateView alternate = AlternateView.CreateAlternateViewFromString(htmlBody, mimeType);
            message.AlternateViews.Add(alternate);

            if (!string.IsNullOrEmpty(CcEmailIDs))
            {
                foreach (string CCEmail in CcEmailIDs.Split(','))
                {
                    message.CC.Add(new MailAddress(CCEmail)); //Adding Multiple CC email Id
                }
            }
            message.IsBodyHtml = true;

            SmtpClient smtp = new SmtpClient
            {
                Host = ConfigurationManager.AppSettings["strEmailServer"],
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(ConfigurationManager.AppSettings["strEmailUserName"].ToString(), ConfigurationManager.AppSettings["strEmailPassword"].ToString())
            };
            smtp.Send(message);
        }

        public string GetEmailTemplate(string templateName)
        {
            string templatesFolder = Path.Combine(_env.ContentRootPath, "wwwroot", "LearnerEmailTemplate");
            string filePath = Path.Combine(templatesFolder, templateName);
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Email template not found: {filePath}");
            return File.ReadAllText(filePath);
        }

        private string GetSetting(string key)
        {
            return ConfigurationManager.AppSettings[key] ?? _configuration?[key] ?? _configuration?[$"Email:{key}"];
        }

        // function to create login link to send to user in case of forgot link and for first time log in.
        public string CreateDirectLoginLinkToBeSendInMail(Int64 userid, bool createUserLink = false)
        {
            throw new NotSupportedException("Use the overload with baseURL parameter in .NET Core");
        }
        public string CreateDirectLoginLinkToBeSendInMail(string baseURL, Int64 userid)
        {
            StringBuilder sbLoginLink = new StringBuilder();
            string strInnerContent = Convert.ToString(userid) + "~" + DateTime.UtcNow.ToString("o");
            sbLoginLink.Append(baseURL.TrimEnd('/') + "/Account/ActiveLogin/");
            sbLoginLink.Append(System.Net.WebUtility.UrlEncode(EncodeString(strInnerContent)));
            return sbLoginLink.ToString();
        }

        // function to create login link to send to user in case of forgot link and for first time log in.
        public string CreateLoginLinkToBeSendInMail(Int64 userid, bool createUserLink = false)
        {
            throw new NotSupportedException("Use the overload with baseURL parameter in .NET Core");
        }
        public string CreatePasswordResetLink(Int64 userid, bool createUserLink = false)
        {
            StringBuilder sbLoginLink = new StringBuilder();
            string strInnerContent = Convert.ToString(userid) + "~" + DateTime.UtcNow.ToString("o");
            sbLoginLink.Append("/Account/ActivateUser/");
            sbLoginLink.Append(System.Net.WebUtility.UrlEncode(EncodeString(strInnerContent)));
            return sbLoginLink.ToString();
        }
        public string CreateLoginLinkToBeSendInMail(string baseURL, Int64 userid, bool createUserLink = false)
        {
            StringBuilder sbLoginLink = new StringBuilder();
            string strInnerContent = Convert.ToString(userid) + "~" + DateTime.UtcNow.ToString("o");
            sbLoginLink.Append(baseURL.TrimEnd('/') + "/Account/ActivateUser/");
            sbLoginLink.Append(System.Net.WebUtility.UrlEncode(EncodeString(strInnerContent)));
            return sbLoginLink.ToString();
        }

        // function to create login link to send to VHARMF user in case of forgot link and for first time log in.
        public string CreateLoginLinkToBeSendInMail_VHARMF(Int64 userid, bool createUserLink = false)
        {
            throw new NotSupportedException("Use the overload with baseURL parameter in .NET Core");
        }

    }
}
