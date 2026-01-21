using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.Model.SuperAdmin
{
    public class SuperAdminInfo
    {
        public Int64 UserID { get; set; }
        public int UserRole { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
        public string Password { get; set; }
        public bool IsPasswordReset { get; set; }
    }

}
