using ELG.DAL.DBEntitySA;
using ELG.Model.SuperAdmin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ELG.DAL.SuperAdminDAL
{
    public class CHSEResellerRep
    {
        /// <summary>
        /// Get list of all resellers in the system
        /// </summary>
        /// <returns></returns>
        public List<Organisation> GetAllResellersOfCHSE()
        {
            try
            {
                List<Organisation> resellerList = new List<Organisation>();
                using (var context = new superadmindbEntities())
                {
                    var resellers = context.lms_superadmin_get_AllResellers().ToList();
                    if (resellers != null && resellers.Count > 0)
                    {
                        foreach (var reseller in resellers)
                        {
                            Organisation r = new Organisation();
                            r.OrganisationId = Convert.ToInt64(reseller.resellerId);
                            r.OrganisationName = reseller.resellerName;
                            resellerList.Add(r);
                        }
                    }
                }
                return resellerList;
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// Return list of registered learner based on search criteria
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public CHSEResellerListing GetCHSEResellerListing(OrganisationListingSearch searchCriteria)
        {
            try
            {
                CHSEResellerListing orgList = new CHSEResellerListing();
                List<CHSEReseller> orgInfoList = new List<CHSEReseller>();

                using (var context = new superadmindbEntities())
                {
                    var organisationList = context.lms_superadmin_get_resellerListing(searchCriteria.OrganisationName, searchCriteria.Status, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (organisationList != null && organisationList.Count > 0)
                    {
                        orgList.TotalRecords = organisationList.Count();
                        var data = organisationList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            CHSEReseller org = new CHSEReseller();
                            org.ResellerId = Convert.ToInt64(item.intOrganisationId);
                            org.OrgUId = Convert.ToString(item.org_uid);
                            org.ResellerName = item.strOrganisation;
                            org.AdminFirstName = item.AdminFirstName;
                            org.AdminLastName = item.AdminLastName;
                            org.AdminEmail = item.AdminEmail;
                            org.ContractorName = item.strContractorName == null ? "": item.strContractorName;
                            org.ContractorEmail = item.strContractorEmail == null ? "" : item.strContractorEmail;
                            org.Sector = item.strSector;
                            org.Rate = item.strRate;
                            org.LinkedCompanies = Convert.ToInt32(item.CompanyCount);
                            org.Status = Convert.ToBoolean(item.ul_blnLive) ? "Active" : "In-active";
                            org.ExpiryDate = item.ul_datExpire == null ? "" : (Convert.ToDateTime(item.ul_datExpire)).ToString("dd-MMM-yyyy");
                            orgInfoList.Add(org);
                        }
                    }
                }
                orgList.ResellerList = orgInfoList;
                return orgList;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
