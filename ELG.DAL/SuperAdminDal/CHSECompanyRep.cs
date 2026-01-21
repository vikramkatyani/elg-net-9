using ELG.DAL.DBEntitySA;
using ELG.Model.SuperAdmin;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;

namespace ELG.DAL.SuperAdminDAL
{
    public class CHSECompanyRep
    {
        /// <summary>
        /// Create new organisation
        /// </summary>
        /// <param name="organisation"></param>
        /// <returns></returns>
        public string CreateNewOrganisation(OrganisationInfo organisation)
        {
            string success = null;
            try
            {
                ObjectParameter retVal = new ObjectParameter("id", typeof(Guid));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_create_organisation(organisation.ResellerId, organisation.OrganisationName, organisation.OrganisationBrandName, organisation.ContractorName, organisation.ContractorEmail, organisation.SupportEmail, organisation.SupportPhone, organisation.OrganisationBaseURL, organisation.ExpiryDate, organisation.Sector,organisation.Rate, organisation.OrganisationOwner, organisation.TrainingManagerFirstName, organisation.TrainingManagerLastName, organisation.TrainingManagerEmail, organisation.TrainingManagerPhone, organisation.TrainingManagerExtension, organisation.OrganisationNumber, organisation.IsReseller, organisation.MaxAllowedUserCount, retVal);
                    success = Convert.ToString(retVal.Value);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return success;
        }
        
        /// <summary>
        /// Return list of registered learner based on search criteria
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public bool SequentialIdExists(string sequentialId)
        {
            bool exist = false;
            try
            {
                using (var context = new superadmindbEntities())
                {
                    var organisationList = context.lms_superadmin_get_company_by_sequential_id(sequentialId).ToList();
                    if (organisationList == null || organisationList.Count > 0)
                    {
                        exist = true;
                    }
                }
                return exist;
            }
            catch (Exception)
            {
                throw;
            }
        }
        
        /// <summary>
        /// Return list of companies based on search criteria
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public OrganisationListing GetCompanyListing(OrganisationListingSearch searchCriteria)
        {
            try
            {
                OrganisationListing orgList = new OrganisationListing();
                List<OrganisationInfo> orgInfoList = new List<OrganisationInfo>();

                using (var context = new superadmindbEntities())
                {
                    var organisationList = context.lms_superadmin_get_companyListing(searchCriteria.Reseller, searchCriteria.OrganisationName, searchCriteria.OrganisationSector, searchCriteria.OrganisationOwner, searchCriteria.Status, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (organisationList != null && organisationList.Count > 0)
                    {
                        orgList.TotalRecords = organisationList.Count();
                        var data = organisationList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            OrganisationInfo org = new OrganisationInfo();
                            org.OrgId = Convert.ToInt64(item.intOrganisationId);
                            org.OrgUId = Convert.ToString(item.org_uid);
                            org.OrganisationName = item.strOrganisation;
                            org.OrganisationBrandName = item.orgBrandName;
                            org.OrganisationNumber = item.ul_sequentialid;
                            org.ContractorName = item.strContractorName;
                            org.ContractorEmail = item.strContractorEmail;
                            org.SupportEmail = item.supportEmail;
                            org.SupportPhone = item.supportPhone;
                            org.TrainingManagerFirstName = item.TrainerFirstName;
                            org.TrainingManagerLastName = item.TrainerLastName;
                            org.TrainingManagerEmail = item.TrainerEmail;
                            org.TrainingManagerPhone = item.TrainerPhone;
                            org.TrainingManagerExtension = item.TrainerExt;
                            org.OrganisationOwner = item.stringOwner;
                            org.Sector = item.strSector;
                            org.Rate = item.strRate;
                            org.Status = Convert.ToBoolean(item.ul_blnLive) ? "Active" : "In-active";
                            org.ExpiryDate = item.ul_datExpire == null ? "" : (Convert.ToDateTime(item.ul_datExpire)).ToString("dd-MMM-yyyy");
                            orgInfoList.Add(org);
                        }
                    }
                }
                orgList.OrganisationList = orgInfoList;
                return orgList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        
        /// <summary>
        /// Return list of companies for drop off report
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public OrganisationListing_dropOff GetCompanyListing_dropOff(OrganisationListingSearch searchCriteria)
        {
            try
            {
                OrganisationListing_dropOff orgList = new OrganisationListing_dropOff();
                List<OrganisationInfo_dropOff> orgInfoList = new List<OrganisationInfo_dropOff>();

                using (var context = new superadmindbEntities())
                {
                    var organisationList = context.lms_superadmin_get_companyListing_dropoff(searchCriteria.Reseller, searchCriteria.OrganisationName, searchCriteria.Status, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (organisationList != null && organisationList.Count > 0)
                    {
                        orgList.TotalRecords = organisationList.Count();
                        var data = organisationList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            OrganisationInfo_dropOff org = new OrganisationInfo_dropOff();
                            org.OrgId = Convert.ToInt64(item.intOrganisationID);
                            org.OrganisationName = item.strOrganisation;
                            org.OrganisationNumber = item.ul_SequentialId;
                            org.ContractorName = item.strContractorName;
                            org.ContractorEmail = item.strContractorEmail;
                            org.Status = Convert.ToBoolean(item.ul_blnLive) ? "Active" : "In-active";
                            org.CreationDate = item.datCreated == null ? "" : (Convert.ToDateTime(item.datCreated)).ToString("dd-MMM-yyyy");
                            org.ExpiryDate = item.UL_datExpire == null ? "" : (Convert.ToDateTime(item.UL_datExpire)).ToString("dd-MMM-yyyy");
                            org.Learners = Convert.ToInt32(item.learners);
                            org.LastLoginDate = item.LastLogIn == null ? "" : (Convert.ToDateTime(item.LastLogIn)).ToString("dd-MMM-yyyy");
                            org.LastCourseLaunchDate = item.LastCourseLaunch == null ? "" : (Convert.ToDateTime(item.LastCourseLaunch)).ToString("dd-MMM-yyyy");
                            orgInfoList.Add(org);
                        }
                    }
                }
                orgList.OrganisationList = orgInfoList;
                return orgList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //update company active status
        public int UpdateCompanyActiveStatus(string OrgUID, bool Status)
        {
            int updated = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(long));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_update_CompanyActiveStatus(OrgUID, Status, retVal);
                    updated = Convert.ToInt32(retVal.Value);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return updated;
        }

        /// <summary>
        /// Return list of company expiring this week
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public OrganisationListing GetCompanyExpiringThisWeek(OrganisationListingSearch searchCriteria)
        {
            try
            {
                OrganisationListing orgList = new OrganisationListing();
                List<OrganisationInfo> orgInfoList = new List<OrganisationInfo>();

                using (var context = new superadmindbEntities())
                {
                    var organisationList = context.lms_superadmin_get_AboutToExpiryCompanies_ThisWeek().ToList();
                    if (organisationList != null && organisationList.Count > 0)
                    {
                        orgList.TotalRecords = organisationList.Count();
                        var data = organisationList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            OrganisationInfo org = new OrganisationInfo();
                            org.OrgId = Convert.ToInt64(item.intorganisationid);
                            org.OrganisationName = item.strOrganisation;
                            org.OrganisationNumber = item.UL_sequentialId;
                            org.ExpiryDate = item.UL_datExpire == null ? "" : (Convert.ToDateTime(item.UL_datExpire)).ToString("dd-MMM-yyyy");
                            orgInfoList.Add(org);
                        }
                    }
                }
                orgList.OrganisationList = orgInfoList;
                return orgList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Return list of company expiring next week
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public OrganisationListing GetCompanyExpiringNextWeek(OrganisationListingSearch searchCriteria)
        {
            try
            {
                OrganisationListing orgList = new OrganisationListing();
                List<OrganisationInfo> orgInfoList = new List<OrganisationInfo>();

                using (var context = new superadmindbEntities())
                {
                    var organisationList = context.lms_superadmin_get_AboutToExpiryCompanies_NextWeek().ToList();
                    if (organisationList != null && organisationList.Count > 0)
                    {
                        orgList.TotalRecords = organisationList.Count();
                        var data = organisationList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            OrganisationInfo org = new OrganisationInfo();
                            org.OrgId = Convert.ToInt64(item.intorganisationid);
                            org.OrganisationName = item.strOrganisation;
                            org.OrganisationNumber = item.UL_sequentialId;
                            org.ExpiryDate = item.UL_datExpire == null ? "" : (Convert.ToDateTime(item.UL_datExpire)).ToString("dd-MMM-yyyy");
                            orgInfoList.Add(org);
                        }
                    }
                }
                orgList.OrganisationList = orgInfoList;
                return orgList;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// Return organisation info based on organisation's uid
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public OrganisationDetailedInfo GetCompanyInfo(string orgGUID)
        {
            try
            {
                OrganisationDetailedInfo org = new OrganisationDetailedInfo();

                using (var context = new superadmindbEntities())
                {
                    var organisationList = context.lms_superadmin_get_companyInfo(orgGUID.Trim()).FirstOrDefault();
                    if (organisationList != null)
                    {
                        //org.OrgId = Convert.ToInt64(organisationList.intOrganisationId);
                        org.ResellerId = Convert.ToInt64(organisationList.resellerId);
                        org.OrgUId = Convert.ToString(organisationList.org_uid);
                        org.OrganisationBaseURL = organisationList.orgBaseURL;
                        org.OrganisationName = organisationList.strOrganisation;
                        org.OrganisationBrandName = organisationList.orgBrandName;
                        org.MaxAllowedUserCount = Convert.ToInt32(organisationList.maxAllowedUserCount);
                        org.OrganisationNumber = organisationList.ul_sequentialid;
                        org.ContractorName = organisationList.strContractorName;
                        org.ContractorEmail = organisationList.strContractorEmail;
                        org.SupportEmail = organisationList.supportEmail;
                        org.SupportPhone = organisationList.supportPhone;
                        org.Sector = organisationList.strSector;
                        org.Rate = organisationList.strRate;
                        org.Notes = organisationList.org_notes;
                        org.Status = Convert.ToBoolean(organisationList.ul_blnLive) ? "Active" : "In-active";
                        org.ExpiryDate = organisationList.ul_datExpire == null ? "" : (Convert.ToDateTime(organisationList.ul_datExpire)).ToString("yyyy-MM-dd");
                        org.TrainingManagerEmail = organisationList.trainerEmail;
                        org.TrainingManagerFirstName = organisationList.trainerFirstName;
                        org.TrainingManagerLastName = organisationList.trainerLastName;
                        org.TrainingManagerPhone = organisationList.trainerPhone;
                        org.TrainingManagerExtension = organisationList.trainerPhoneExt;
                        org.DocUploadEnabled = Convert.ToBoolean(organisationList.docUploadEnabled);
                        org.SelfRegistrationEnabled = Convert.ToBoolean(organisationList.SelfRegistrationEnabled);
                        org.AllowedUploadCount = Convert.ToInt32(organisationList.allowedDocCount);
                        org.TitleDescription = organisationList.strTitleDescription;
                        org.FirstNameDescription = organisationList.strFirstNameDescription;
                        org.LastNameDescription = organisationList.strSurnameDescription;
                        org.EmailDescription = organisationList.UL_emailIdDescription;
                        org.LocationDescription = organisationList.strLocationDescription;
                        org.DepartmentDescription = organisationList.strDepartmentDescription;
                        org.AllowedAccidentIncidentFeature = Convert.ToInt16(organisationList.incidentAccidentEnabled);
                        org.TrainingRenewalMode = Convert.ToInt32(organisationList.trainingResetType);
                        org.OrganisationOwner = organisationList.stringOwner;
                        org.CourseAssignMode = Convert.ToInt32(organisationList.courseAssignMode);
                    }
                }
                return org;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get company banner image
        /// </summary>
        /// <param name="orgGUID"></param>
        /// <returns></returns>
        public OrganisationImage GetCompanyBanner(string orgGUID)
        {
            try
            {
                OrganisationImage img = new OrganisationImage();

                using (var context = new superadmindbEntities())
                {
                    var org = context.lms_superadmin_get_companyBanner(orgGUID.Trim()).FirstOrDefault();
                    if (org != null)
                    {
                        img.Image = org;
                    }
                }
                return img;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get company certificate image
        /// </summary>
        /// <param name="orgGUID"></param>
        /// <returns></returns>
        public OrganisationImage GetCompanyCertificate(string orgGUID)
        {
            try
            {
                OrganisationImage img = new OrganisationImage();

                using (var context = new superadmindbEntities())
                {
                    var org = context.lms_superadmin_get_companyCertificate(orgGUID.Trim()).FirstOrDefault();
                    if (org != null)
                    {
                        img.Image = org;
                    }
                }
                return img;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Update company banner image
        /// </summary>
        /// <param name="org"></param>
        /// <returns></returns>
        public int UpdateCompanyBanner(OrganisationImage org)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("update", typeof(int));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_insert_companyBanner(org.OrgUid, org.Image, retVal);
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
        /// Remove company banner image
        /// </summary>
        /// <param name="org"></param>
        /// <returns></returns>
        public int RemoveCompanyBanner(OrganisationImage org)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_remove_customlogo(org.OrgUid, retVal);
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
        /// Update company certificate image
        /// </summary>
        /// <param name="org"></param>
        /// <returns></returns>
        public int UpdateCompanyCertificate(OrganisationImage org)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("update", typeof(int));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_insert_companyCertificate(org.OrgUid, org.Image, retVal);
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
        /// Remove company custom certificate
        /// </summary>
        /// <param name="org"></param>
        /// <returns></returns>
        public int RemoveCompanyCertificate(OrganisationImage org)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_remove_customcertificate(org.OrgUid, retVal);
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
        /// update organisation info
        /// </summary>
        /// <param name="organisation"></param>
        /// <returns></returns>
        public int UpdateOrganisationInfo(OrganisationInfo organisation)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_update_CompanyInformation(organisation.ResellerId, organisation.OrganisationName, organisation.OrganisationBrandName, organisation.ContractorName, organisation.ContractorEmail, organisation.SupportEmail, organisation.SupportPhone, organisation.OrganisationBaseURL, organisation.ExpiryDate, organisation.Sector, organisation.Rate, organisation.TrainingManagerFirstName, organisation.TrainingManagerLastName, organisation.TrainingManagerEmail, organisation.TrainingManagerPhone, organisation.TrainingManagerExtension,organisation.OrganisationOwner, organisation.OrgUId.Trim(), organisation.MaxAllowedUserCount, retVal);
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
        /// update organsiation notes
        /// </summary>
        /// <param name="organisation"></param>
        /// <returns></returns>
        public int UpdateOrganisationNotes(OrganisationInfo organisation)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_update_CompanyNotes(organisation.Notes, organisation.OrgUId.Trim(), retVal);
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
        /// update organisation info
        /// </summary>
        /// <param name="organisation"></param>
        /// <returns></returns>
        public int UpdateOrganisationGeneralSettings(OrganisationDetailedInfo organisation)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_update_CompanyGeneralSettings(organisation.DocUploadEnabled, organisation.AllowedUploadCount, organisation.AllowedAccidentIncidentFeature, organisation.TrainingRenewalMode, organisation.CourseAssignMode, organisation.SelfRegistrationEnabled, organisation.OrgUId.Trim(), retVal);
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
        /// update organisation info
        /// </summary>
        /// <param name="organisation"></param>
        /// <returns></returns>
        public int UpdateOrganisationFieldText(OrganisationDetailedInfo organisation)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_update_CompanyFieldTextSettings(organisation.TitleDescription, organisation.FirstNameDescription, organisation.LastNameDescription, organisation.EmailDescription, organisation.LocationDescription,organisation.DepartmentDescription, organisation.OrgUId.Trim(), retVal);
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
        /// Download company listing
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public List<DownloadOrganisationList> DownloadCompanyListing(OrganisationListingSearch searchCriteria)
        {
            try
            {
                List<DownloadOrganisationList> orgInfoList = new List<DownloadOrganisationList>();

                using (var context = new superadmindbEntities())
                {
                    var organisationList = context.lms_superadmin_get_companyListing(searchCriteria.Reseller, searchCriteria.OrganisationName, searchCriteria.OrganisationSector, searchCriteria.OrganisationOwner, searchCriteria.Status, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (organisationList != null && organisationList.Count > 0)
                    {
                        foreach (var item in organisationList)
                        {
                            DownloadOrganisationList org = new DownloadOrganisationList();
                            org.OrganisationName = item.strOrganisation;
                            org.OrganisationNumber = item.ul_sequentialid;
                            org.ContractManagerName = item.strContractorName;
                            org.ContractManagerEmail = item.strContractorEmail;
                            org.Sector = item.strSector;
                            org.Rate = item.strRate;
                            org.Status = Convert.ToBoolean(item.ul_blnLive) ? "Active" : "In-active";
                            //org.ExpiryDate = item.ul_datExpire == null ? "" : (Convert.ToDateTime(item.ul_datExpire)).ToString("dd-MMM-yyyy");
                            org.ExpiryDate = Convert.ToDateTime(item.ul_datExpire) ;
                            org.Owner = item.stringOwner;
                            orgInfoList.Add(org);
                        }
                    }
                }
                return orgInfoList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Download company listing
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public List<DownloadOrganisationList_dropOff> DownloadCompanyListing_dropOff(OrganisationListingSearch searchCriteria)
        {
            try
            {
                List<DownloadOrganisationList_dropOff> orgInfoList = new List<DownloadOrganisationList_dropOff>();

                using (var context = new superadmindbEntities())
                {
                    var organisationList = context.lms_superadmin_get_companyListing_dropoff(searchCriteria.Reseller, searchCriteria.OrganisationName, searchCriteria.Status, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (organisationList != null && organisationList.Count > 0)
                    {
                        foreach (var item in organisationList)
                        {
                            DownloadOrganisationList_dropOff org = new DownloadOrganisationList_dropOff();
                            org.OrganisationName = item.strOrganisation;
                            org.OrganisationNumber = item.ul_SequentialId;
                            org.ContractManagerName = item.strContractorName;
                            org.ContractManagerEmail = item.strContractorEmail;
                            org.Learners = Convert.ToInt32(item.learners);
                            org.Status = Convert.ToBoolean(item.ul_blnLive) ? "Active" : "In-active";
                            org.ExpiryDate = Convert.ToDateTime(item.UL_datExpire);
                            org.LastLogin = Convert.ToDateTime(item.LastLogIn);
                            org.LastCourseLaunch = Convert.ToDateTime(item.LastCourseLaunch);
                            orgInfoList.Add(org);
                        }
                    }
                }
                return orgInfoList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int CreateNewOTP(Int64 adminid, string otp)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_create_OTP_record(adminid, otp, retVal);
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
        /// Delete Organisation
        /// </summary>
        /// <param name="OrganisationId"></param>
        /// <returns></returns>
        public int DeleteOrganisation(Int64 OrganisationId, Int64 adminId, string otp)
        {
            int deleted = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(long));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_delete_company(OrganisationId, adminId, otp, retVal);
                    deleted = Convert.ToInt32(retVal.Value);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return deleted;
        }
        
        /// <summary>
        /// Delete Reseller
        /// </summary>
        /// <param name="OrganisationId"></param>
        /// <returns></returns>
        public int DeleteReseller(Int64 OrganisationId, Int64 adminId, string otp)
        {
            int deleted = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(long));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_delete_reseller(OrganisationId, adminId, otp, retVal);
                    deleted = Convert.ToInt32(retVal.Value);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return deleted;
        }

        /// <summary>
        /// Delete Organisation
        /// </summary>
        /// <param name="OrganisationId"></param>
        /// <returns></returns>
        public int CreateDeletedOrganisationRecord(Int64 adminId, Int64 organisationId, string organisationName, string organisationNumber)
        {
            int deleted = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_create_deleted_company_record(adminId, organisationId, organisationName, organisationNumber, retVal);
                    deleted = Convert.ToInt32(retVal.Value);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return deleted;
        }

        /// <summary>
        /// Return list of all Locations
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public OrganisationLocationListing GetCompanyLocationListing(SearchListing searchCriteria)
        {
            try
            {
                OrganisationLocationListing locList = new OrganisationLocationListing();
                List<OrganisationLocation> locInfoList = new List<OrganisationLocation>();

                using (var context = new superadmindbEntities())
                {
                    var locationList = context.lms_superadmin_getAllCompnayLocations(searchCriteria.UID, searchCriteria.SearchText).ToList();
                    if (locationList != null && locationList.Count > 0)
                    {
                        locList.TotalRecords = locationList.Count();
                        var data = locationList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            OrganisationLocation loc = new OrganisationLocation();
                            loc.LocationId = item.intLocationID;
                            loc.LocationName = item.strLocation;
                            locInfoList.Add(loc);
                        }
                    }
                }
                locList.LocationList = locInfoList;
                return locList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Return list of all Locations
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public OrganisationDepartmentListing GetCompanyDepartmentListing(SearchListing searchCriteria)
        {
            try
            {
                OrganisationDepartmentListing depList = new OrganisationDepartmentListing();
                List<OrganisationDepartment> depInfoList = new List<OrganisationDepartment>();

                using (var context = new superadmindbEntities())
                {
                    var departmentList = context.lms_superadmin_getAllCompnayDepartments(searchCriteria.UID, searchCriteria.SearchText).ToList();
                    if (departmentList != null && departmentList.Count > 0)
                    {
                        depList.TotalRecords = departmentList.Count();
                        var data = departmentList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            OrganisationDepartment dep = new OrganisationDepartment();
                            dep.DepartmentId = item.intDepartmentID;
                            dep.DepartmentName = item.strDepartment;
                            depInfoList.Add(dep);
                        }
                    }
                }
                depList.DepartmentList = depInfoList;
                return depList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Update company banner image
        /// </summary>
        /// <param name="org"></param>
        /// <returns></returns>
        public int MapLocationDepartment(OrganisationDepartment dep)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_insert_LocDepMap(dep.LocationId, dep.DepartmentId, retVal);
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
        /// Return list of all Locations
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public OrganisationDepartmentListing GetLocationMappedDepartmentListing(DepartmentMapSearchListing searchCriteria)
        {
            try
            {
                OrganisationDepartmentListing depList = new OrganisationDepartmentListing();
                List<OrganisationDepartment> depInfoList = new List<OrganisationDepartment>();

                using (var context = new superadmindbEntities())
                {
                    var departmentList = context.lms_superadmin_get_CompnayLocationDepartmentMap(searchCriteria.LocationId, searchCriteria.SearchText).ToList();
                    if (departmentList != null && departmentList.Count > 0)
                    {
                        depList.TotalRecords = departmentList.Count();
                        var data = departmentList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            OrganisationDepartment dep = new OrganisationDepartment();
                            dep.DepartmentId = item.intDepartmentID;
                            dep.DepartmentName = item.strDepartment;
                            dep.Mapped = Convert.ToInt32(item.selected);
                            depInfoList.Add(dep);
                        }
                    }
                }
                depList.DepartmentList = depInfoList;
                return depList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Create new location for a company
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public Int64 CreateNewLocation(String Organisation, string LocationName)
        {
            Int64 locationId = 0;
            try
            {
                ObjectParameter newLocId = new ObjectParameter("newLocId", typeof(long));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_create_location(Organisation, LocationName, newLocId);
                    locationId = Convert.ToInt32(newLocId.Value);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return locationId;
        }

        //update location
        public int UpdateLocation(Int64 LocationId, string LocationName)
        {
            int updated = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(long));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_update_location(LocationId, LocationName, retVal);
                    updated = Convert.ToInt32(retVal.Value);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return updated;
        }

        //delete location
        public int DeleteLocation(Int64 LocationId)
        {
            int deleted = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(long));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_delete_location(LocationId, retVal);
                    deleted = Convert.ToInt32(retVal.Value);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return deleted;
        }

        /// <summary>
        /// Create new location for a company
        /// </summary>
        /// <param name="DepartmentName"></param>
        /// <returns></returns>
        public Int64 CreateNewDepartment(String Organisation, string DepartmentName)
        {
            Int64 departmentID = 0;
            try
            {
                ObjectParameter newDepId = new ObjectParameter("newDepId", typeof(long));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_create_department(Organisation, DepartmentName, newDepId);
                    departmentID = Convert.ToInt32(newDepId.Value);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return departmentID;
        }

        //update department
        public int UpdateDepartment(Int64 DepartmentId, string DepartmentName)
        {
            int updated = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(long));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_update_department(DepartmentId, DepartmentName, retVal);
                    updated = Convert.ToInt32(retVal.Value);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return updated;
        }

        //delete department
        public int DeleteDepartment(Int64 DepartmentId)
        {
            int deleted = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(long));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_delete_department(DepartmentId, retVal);
                    deleted = Convert.ToInt32(retVal.Value);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return deleted;
        }

        /// <summary>
        /// Get company admin id
        /// </summary>
        /// <param name="orgUID"></param>
        /// <returns></returns>
        public LearnerInfo GetCompanyAdminInfo(string orgUID)
        {
            try
            {
                LearnerInfo learner = new LearnerInfo();
                using (var context = new superadmindbEntities())
                {
                    var learnerInfo = context.lms_superadmin_get_CompanyAdminInfo(orgUID).FirstOrDefault();
                    if (learnerInfo != null)
                    {
                        learner.UserID = learnerInfo.intContactID;
                        learner.FirstName = learnerInfo.strFirstName;
                        learner.LastName = learnerInfo.strSurname;
                        learner.EmailId = learnerInfo.strEmail;
                        learner.Location = learnerInfo.strLocation;
                        learner.Department = learnerInfo.strDepartment;
                        learner.CompanyID = Convert.ToInt64(learnerInfo.intOrganisationID);
                        learner.LocationID = Convert.ToInt64(learnerInfo.intLocationID);
                        learner.DepartmentID = Convert.ToInt64(learnerInfo.intDepartmentID);
                    }
                }
                return learner;
            }
            catch (Exception)
            {

                throw;
            }

        }

        /// <summary>
        /// Return list of admins of the organisation
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public OrganisationLearnerList GetCompanyAdminList(LearnerReportSearch searchCriteria)
        {
            try
            {
                OrganisationLearnerList orglearnerList = new OrganisationLearnerList();
                List<LearnerInfo> adminInfoList = new List<LearnerInfo>();

                using (var context = new superadmindbEntities())
                {
                    var learnerList = context.lms_superadmin_get_CompanyAdminList(searchCriteria.SearchLearnerOrganisation, searchCriteria.SearchLearnerName, searchCriteria.SortCol, searchCriteria.SortColDir).ToList();
                    if (learnerList != null && learnerList.Count > 0)
                    {
                        orglearnerList.TotalLearners = learnerList.Count();
                        var data = learnerList.Skip(searchCriteria.Skip).Take(searchCriteria.PageSize).ToList();

                        foreach (var item in data)
                        {
                            LearnerInfo admin = new LearnerInfo();
                            admin.UserID = item.intContactID;
                            admin.FirstName = item.strFirstName;
                            admin.LastName = item.strSurname;
                            admin.EmailId = item.strEmail;
                            admin.Location = item.strLocation;
                            admin.Department = item.strDepartment;
                            adminInfoList.Add(admin);
                        }
                    }
                }
                orglearnerList.LearnerList = adminInfoList
;
                return orglearnerList;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// Remove super admin rights from the user
        /// </summary>
        /// <param name="adminID"></param>
        /// <returns></returns>
        public int RemoveAdminRights(Int64 adminID)
        {
            try
            {
                ObjectParameter retVal = new ObjectParameter("retVal", typeof(int));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_remove_superadmin_rights(adminID, retVal);
                }
                return Convert.ToInt32(retVal.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Function to create new learner in an organisation
        /// </summary>
        /// <param name="learner"></param>
        /// <returns></returns>
        public int CreateNewAdminLearner(String orgUID, LearnerInfo learner)
        {
            int success = 0;
            try
            {
                ObjectParameter retVal = new ObjectParameter("id", typeof(long));
                ObjectParameter assign = new ObjectParameter("retval", typeof(int));
                using (var context = new superadmindbEntities())
                {
                    var result = context.lms_superadmin_create_CompanyAdminLearner(learner.Title, learner.FirstName, learner.LastName, learner.EmailId, orgUID, learner.LocationID, learner.DepartmentID, retVal);
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
