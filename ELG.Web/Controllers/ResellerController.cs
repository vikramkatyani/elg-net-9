using ELG.Model.OrgAdmin;
using System;
using ELG.Web.Helper;
using System.Linq;
using ELG.Model.OrgAdmin;
using ELG.DAL.OrgAdminDAL;
using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Mvc;

namespace ELG.Web.Controllers
{
    [SessionCheck]
    public class ResellerController : Controller
    {
        // GET: Reseller Companies
        public ActionResult Companies()
        {
            return View();
        }

        // get report on applied filter
        [HttpPost]
        public ActionResult LoadResellerCompanies(DataTableFilter searchCriteria)
        {
            try
            {
                var resellerRep = new CompanyRep();

                searchCriteria.Company = Convert.ToInt64(SessionHelper.CompanyId);
                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                }

                searchCriteria.Draw = Request.Form["draw"].FirstOrDefault();
                searchCriteria.Start = Request.Form["start"].FirstOrDefault();
                searchCriteria.Length = Request.Form["length"].FirstOrDefault();
                //Find Order Column
                searchCriteria.SortCol = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                searchCriteria.SortColDir = Request.Form["order[0][dir]"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                ResellerCompanyList companyList = resellerRep.GetResellerCompanies(searchCriteria);

                return Json(new { draw = searchCriteria.Draw, recordsFiltered = companyList.TotalRecords, recordsTotal = companyList.TotalRecords, data = companyList.CompanyList });

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("Learner");
            }
        }

        [HttpPost]
        public ActionResult LoadLicenceListForCompany(DataTableFilter searchCriteria)
        {
            try
            {
                var resellerRep = new CompanyRep();
                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                }

                searchCriteria.Draw = Request.Form["draw"].FirstOrDefault();
                searchCriteria.Start = Request.Form["start"].FirstOrDefault();
                searchCriteria.Length = Request.Form["length"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                ResellerCompanyLicenceList courseList = resellerRep.GetLicencesForCompany(searchCriteria.Company);

                return Json(new { draw = searchCriteria.Draw, recordsFiltered = courseList.TotalRecords, recordsTotal = courseList.TotalRecords, data = courseList.LicenceList });

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("Learner");
            }
        }

        #region Reseller licence transaction report
        // GET: Learning Progress Report
        public ActionResult LicenceTransactionReport()
        {
            return View();
        }

        [HttpPost]
        //fetch learning progress records
        public ActionResult LoadResellerLicenceTransactionReport(ResellerLicenceReportFilter searchCriteria)
        {
            ResellerLicenseTransactionReport transactionReport = new ResellerLicenseTransactionReport();
            transactionReport.TransactionSummary = new List<ResellerLicenseTransactionItem>();
            try
            {
                var reportRep = new ModuleRep();

                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;

                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                }

                if (!String.IsNullOrEmpty(Request.Form["FromDate"].FirstOrDefault()))
                    searchCriteria.FromDate = Convert.ToDateTime(Request.Form["FromDate"].FirstOrDefault());

                if (!String.IsNullOrEmpty(Request.Form["ToDate"].FirstOrDefault()))
                    searchCriteria.ToDate = Convert.ToDateTime(Request.Form["ToDate"].FirstOrDefault());

                searchCriteria.Draw = Request.Form["draw"].FirstOrDefault();
                searchCriteria.Start = Request.Form["start"].FirstOrDefault();
                searchCriteria.Length = Request.Form["length"].FirstOrDefault();

                //Find Order Column
                searchCriteria.SortCol = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                searchCriteria.SortColDir = Request.Form["order[0][dir]"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                transactionReport = reportRep.GetResellerTransactionReport(searchCriteria);
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
                return Json(new { draw = searchCriteria.Draw, recordsFiltered = transactionReport.TotalItems, recordsTotal = transactionReport.TotalItems, data = transactionReport.TransactionSummary });
        }

        //download learning progress records
        public ActionResult DownloadResellerTransactionReport(ResellerLicenceReportFilter searchCriteria)
        {
            List<ResellerLicenseTransactionDownloadItem> progressReport = new List<ResellerLicenseTransactionDownloadItem>();
            try
            {
                var reportRep = new ModuleRep();

                searchCriteria.Company = SessionHelper.CompanyId;
                searchCriteria.AdminRole = SessionHelper.UserRole;
                searchCriteria.AdminUserId = SessionHelper.UserId;

                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                }

                string fromDate = Request.Query["From"].ToString();
                string toDate = Request.Query["To"].ToString();
                if (!String.IsNullOrEmpty(fromDate))
                    searchCriteria.FromDate = Convert.ToDateTime(fromDate);

                if (!String.IsNullOrEmpty(toDate))
                    searchCriteria.ToDate = Convert.ToDateTime(toDate);

                progressReport = reportRep.DownloadResellerTransactionReport(searchCriteria);

                DataTable dtReport = CommonHelper.ListToDataTable(progressReport);
                string[] columns = { "Company", "Course", "Action", "Licence Count", "Transaction Date"};
                byte[] filecontent = CommonHelper.ExportExcel(dtReport, "Licence Transaction Report", true, columns);
                return File(filecontent, CommonHelper.ExcelContentType, "LicenceTransactionReport.xlsx");
            }

            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("LicenceTransactionReport");
            }
        }
        #endregion
    }
}