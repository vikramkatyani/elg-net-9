using ELG.DAL.DbEntityLearner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.DAL.LearnerDAL
{
    public class PurchaseRep
    {
        public string Purchase(string from)
        {
            string success = "failed";
            try
            {
                using (learnerDBEntities context = new learnerDBEntities())
                {
                    //atf_sales_purchase_records rec = new atf_sales_purchase_records();
                    //rec.pageURL = from;
                    //rec.recordedOn = DateTime.UtcNow;
                    //context.atf_sales_purchase_records.Add(rec);
                    //context.SaveChanges();
                    success = "success";
                }
                return success;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
