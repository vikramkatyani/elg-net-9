using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.Model.Learner
{
    public class Company
    {
        public Int64 CompanyId { get; set; }
        public string CompanyNumber { get; set; }
        public string CompanyName { get; set; }
        public string CompanyBrandName { get; set; }
        public string CompanyBaseURL { get; set; }
        public string CompanySupportMail { get; set; }
        public bool Cancelled { get; set; }
        public bool Live { get; set; }
        public string Status { get; set; }
        public string Settings { get; set; }
        public bool AllowedSelfRegistration { get; set; }
    }
    public class CompanyDomainDetails
    {
        public Int64 CompanyId { get; set; }
        public string Domain { get; set; }
        public string Favicon { get; set; }
        public string CSS { get; set; }
        public string TitleText { get; set; }
        public string LogoPath { get; set; }
    }
    public class LearnerAvailableMenu
    {
        public string Settings { get; set; }
        public string LandingPage { get; set; }
    }
}
