using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.Model.SuperAdmin
{
    public class CHSEReseller
    {
        public Int64 ResellerId { get; set; }
        public String OrgUId { get; set; }
        public string ResellerName { get; set; }
        public string AdminFirstName { get; set; }
        public string AdminLastName { get; set; }
        public string AdminEmail { get; set; }
        public string Sector { get; set; }
        public string Rate { get; set; }
        public string ExpiryDate { get; set; }
        public string ContractorName { get; set; }
        public string ContractorEmail { get; set; }
        public string Status { get; set; }
        public int LinkedCompanies { get; set; }
    }

    public class CHSEResellerListing
    {
        public List<CHSEReseller> ResellerList { get; set; }
        public int TotalRecords { get; set; }
    }
}
