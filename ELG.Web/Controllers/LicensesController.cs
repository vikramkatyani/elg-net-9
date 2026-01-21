using ELG.Web.Helper;
using ELG.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using ELG.Model.OrgAdmin;
using ELG.DAL.OrgAdminDAL;
using ELG.DAL.Utilities;
using System.Data;
using Microsoft.AspNetCore.Mvc;

namespace ELG.Web.Controllers
{
    [SessionCheck]
    public class LicensesController : Controller
    {
        
        // GET: list license summary of all modules assigned to organisation
        public ActionResult ReviewLicenses()
        {
            return View("ReviewLicenses");
        }

        // get report on applied filter
        [HttpPost]
        public ActionResult LoadLicenseSummaryData(DataTableFilter searchCriteria)
        {
            LicenseSummaryReport licenseSummaryReport = new LicenseSummaryReport();
            licenseSummaryReport.ModuleList = new List<ModuleLicenceSummary>();
            try
            {
                var moduleRep = new ModuleRep();

                searchCriteria.Company = Convert.ToInt64(SessionHelper.CompanyId);
                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                }

                searchCriteria.Draw = Request.Form["draw"].FirstOrDefault();
                searchCriteria.Start = Request.Form["start"].FirstOrDefault();
                searchCriteria.Length = Request.Form["length"].FirstOrDefault();
                //Find Order Column
                searchCriteria.SortCol = Request.Form[$"columns[{Request.Form["order[0][column]"].FirstOrDefault()}][name]"].FirstOrDefault();
                searchCriteria.SortColDir = Request.Form["order[0][dir]"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                licenseSummaryReport = moduleRep.GetLicenseSummary(searchCriteria);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { draw = searchCriteria.Draw, recordsFiltered = licenseSummaryReport.TotalModules, recordsTotal = licenseSummaryReport.TotalModules, data = licenseSummaryReport.ModuleList }),
                    ContentType = "application/json"
                };

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { draw = searchCriteria.Draw, recordsFiltered = licenseSummaryReport.TotalModules, recordsTotal = licenseSummaryReport.TotalModules, data = licenseSummaryReport.ModuleList }),
                    ContentType = "application/json"
                };
            }
        }

        //download Licence transaction report
        public ActionResult DownloadLicenseSummary(DataTableFilter searchCriteria)
        {
            List<DownloadLicenceSummary> licenceSummaryReport = new List<DownloadLicenceSummary>();
            try
            {
                var moduleRep = new ModuleRep();

                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                }

                searchCriteria.Company = Convert.ToInt64(SessionHelper.CompanyId);

                licenceSummaryReport = moduleRep.DownloadLicenceSummaryReport(searchCriteria);

                DataTable dtReport = CommonHelper.ListToDataTable(licenceSummaryReport);
                string[] columns = { "Course", "TotalLicenses", "AllocatedLicenses", "AvailableLicenses", "UsedLicenses", "DeletedLicenses" };
                byte[] filecontent = CommonHelper.ExportExcel(dtReport, "Licence Summary Report", true, columns);
                return File(filecontent, CommonHelper.ExcelContentType, "LicenceSummaryReport.xlsx");

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("AccidentIncident");
            }
        }

        // GET: list licence transactions
        public ActionResult LicenseTransactions()
        {
            return View("LicenseTransactions");
        }

        // get report on applied filter
        [HttpPost]
        public ActionResult LoadLicenseTransactionsData(LicenseTransactionFilter searchCriteria)
        {
            LicenseTransactionReport transactionSummaryReport = new LicenseTransactionReport();
            transactionSummaryReport.TransactionSummary = new List<LicenseTransactionItem>();
            try
            {
                var moduleRep = new ModuleRep();

                searchCriteria.Company = Convert.ToInt64(SessionHelper.CompanyId);

                if (!String.IsNullOrEmpty(Request.Form["FromDate"].FirstOrDefault()))
                    searchCriteria.FromDate = Convert.ToDateTime(Request.Form["FromDate"].FirstOrDefault());

                if (!String.IsNullOrEmpty(Request.Form["ToDate"].FirstOrDefault()))
                    searchCriteria.ToDate = Convert.ToDateTime(Request.Form["ToDate"].FirstOrDefault());

                searchCriteria.Draw = Request.Form["draw"].FirstOrDefault();
                searchCriteria.Start = Request.Form["start"].FirstOrDefault();
                searchCriteria.Length = Request.Form["length"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                transactionSummaryReport = moduleRep.GetLicenseTransactionSummary(searchCriteria);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { draw = searchCriteria.Draw, recordsFiltered = transactionSummaryReport.TotalItems, recordsTotal = transactionSummaryReport.TotalItems, data = transactionSummaryReport.TransactionSummary }),
                    ContentType = "application/json"
                };

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { draw = searchCriteria.Draw, recordsFiltered = transactionSummaryReport.TotalItems, recordsTotal = transactionSummaryReport.TotalItems, data = transactionSummaryReport.TransactionSummary }),
                    ContentType = "application/json"
                };
            }
        }

        //download Licence transaction report
        public ActionResult DownloadLicenseTransactions(LicenseTransactionFilter searchCriteria)
        {
            List<DownloadLicenceTransactionReport> transactionSummaryReport = new List<DownloadLicenceTransactionReport>();
            try
            {
                var moduleRep = new ModuleRep();

                if (searchCriteria == null)
                {
                    searchCriteria.SearchText = String.Empty;
                }

                searchCriteria.Company = Convert.ToInt64(SessionHelper.CompanyId);
                string fromDate = Request.Query["FromDate"].ToString();
                string toDate = Request.Query["ToDate"].ToString();
                if (!String.IsNullOrEmpty(fromDate))
                    searchCriteria.FromDate = Convert.ToDateTime(fromDate);

                if (!String.IsNullOrEmpty(toDate))
                    searchCriteria.ToDate = Convert.ToDateTime(toDate);

                transactionSummaryReport = moduleRep.DownloadLicenseTransactions(searchCriteria);

                DataTable dtReport = CommonHelper.ListToDataTable(transactionSummaryReport);
                string[] columns = { "Date", "Course", "Action", "LicenceCount", "FirstName", "LastName", "Email" };
                byte[] filecontent = CommonHelper.ExportExcel(dtReport, "Transaction Report", true, columns);
                return File(filecontent, CommonHelper.ExcelContentType, "TransactionReport.xlsx");

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View("AccidentIncident");
            }
        }

        // GET: list licence transactions
        public ActionResult LicenseAutoAllocation()
        {
            return View("LicenseAutoAllocation");
        }

        // get report on applied filter
        [HttpPost]
        public ActionResult LoadDepartmentForLicenseAutoAllocation(DataTableFilter searchCriteria)
        {
            DepartmentListForModuleAutoAssignment departmentList = new DepartmentListForModuleAutoAssignment();
            departmentList.DepartmentList = new List<DepartmentForLicenseAutoAssignment>();
            try
            {
                var moduleRep = new ModuleRep();

                searchCriteria.Company = Convert.ToInt64(SessionHelper.CompanyId);

                searchCriteria.Draw = Request.Form["draw"].FirstOrDefault();
                searchCriteria.Start = Request.Form["start"].FirstOrDefault();
                searchCriteria.Length = Request.Form["length"].FirstOrDefault();

                //Find Order Column
                searchCriteria.SortCol = Request.Form["columns[" + Request.Form["order[0][column]"] + "][name]"].FirstOrDefault();
                searchCriteria.SortColDir = Request.Form["order[0][dir]"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                departmentList = moduleRep.GetDepartmentListForModuleAutoAssignment(searchCriteria);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { draw = searchCriteria.Draw, recordsFiltered = departmentList.TotalDepartments, recordsTotal = departmentList.TotalDepartments, data = departmentList.DepartmentList }),
                    ContentType = "application/json"
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { draw = searchCriteria.Draw, recordsFiltered = departmentList.TotalDepartments, recordsTotal = departmentList.TotalDepartments, data = departmentList.DepartmentList }),
                    ContentType = "application/json"
                };
            }
        }

        // get all departments of a company for licence auto allocation
        [HttpPost]
        public ActionResult LoadAllDepartmentForLicenseAutoAllocation(DataTableFilter searchCriteria)
        {
            DepartmentListForModuleAutoAssignment departmentList = new DepartmentListForModuleAutoAssignment();
            departmentList.DepartmentList = new List<DepartmentForLicenseAutoAssignment>();
            try
            {
                var moduleRep = new ModuleRep();

                searchCriteria.Company = Convert.ToInt64(SessionHelper.CompanyId);

                searchCriteria.Draw = Request.Form["draw"].FirstOrDefault();
                searchCriteria.Start = Request.Form["start"].FirstOrDefault();
                searchCriteria.Length = Request.Form["length"].FirstOrDefault();

                //Find Order Column
                searchCriteria.SortCol = Request.Form["columns[" + Request.Form["order[0][column]"] + "][name]"].FirstOrDefault();
                searchCriteria.SortColDir = Request.Form["order[0][dir]"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                departmentList = moduleRep.GetAllDepartmentListForModuleAutoAssignment(searchCriteria);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
            return new ContentResult {
                Content = System.Text.Json.JsonSerializer.Serialize(new { draw = searchCriteria.Draw, recordsFiltered = departmentList.TotalDepartments, recordsTotal = departmentList.TotalDepartments, data = departmentList.DepartmentList }),
                ContentType = "application/json"
            };
        }

        //get list of all Locations to set licence auto allocation
        [HttpPost]
        public ActionResult LoadAllLocationsForLicenseAutoAllocation(DataTableFilter searchCriteria)
        {
            LocationListForModuleAutoAssignment locationList = new LocationListForModuleAutoAssignment();
            locationList.LocationList = new List<LocationForLicenseAutoAssignment>();
            try
            {
                var moduleRep = new ModuleRep();

                searchCriteria.Company = Convert.ToInt64(SessionHelper.CompanyId);

                searchCriteria.Draw = Request.Form["draw"].FirstOrDefault();
                searchCriteria.Start = Request.Form["start"].FirstOrDefault();
                searchCriteria.Length = Request.Form["length"].FirstOrDefault();

                //Find Order Column
                searchCriteria.SortCol = Request.Form["columns[" + Request.Form["order[0][column]"] + "][name]"].FirstOrDefault();
                searchCriteria.SortColDir = Request.Form["order[0][dir]"].FirstOrDefault();

                searchCriteria.PageSize = searchCriteria.Length != null ? Convert.ToInt32(searchCriteria.Length) : 0;
                searchCriteria.Skip = searchCriteria.Start != null ? Convert.ToInt32(searchCriteria.Start) : 0;
                searchCriteria.RecordTotal = 0;

                locationList = moduleRep.GetAllLocationListForModuleAutoAssignment(searchCriteria);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
            return new ContentResult {
                Content = System.Text.Json.JsonSerializer.Serialize(new { draw = searchCriteria.Draw, recordsFiltered = locationList.TotalLocations, recordsTotal = locationList.TotalLocations, data = locationList.LocationList }),
                ContentType = "application/json"
            };
        }


        // set department to license auto allocation for a module - selected location
        [HttpPost]
        public ActionResult SetLicenseAutoAllocationForDepartment(DepartmentFilterForLicenseAutoAssignment searchCriteria)
        {
            try
            {
                var moduleRep = new ModuleRep();
                int result = moduleRep.SetDepartmentForAutoAllocationOfModule(searchCriteria);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { success = result }),
                    ContentType = "application/json"
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { success = -1 }),
                    ContentType = "application/json"
                };
            }
        }

        // set department to license auto allocation for a module - selected department in all locations
        [HttpPost]
        public ActionResult SetLicenseAutoAllocationForAllDepartments(DepartmentFilterForLicenseAutoAssignment searchCriteria, bool allSelected, Int64[] selectedDepartmentList, Int64[] unselectedDepartmentList)
        {
            try
            {
                // get list of all departments in location and convert to comma seperated string
                var utilities = new CommonMethods();
                var departmentList = utilities.GetDepartmentsForLocation(SessionHelper.UserRole, SessionHelper.UserId, searchCriteria.Location);
                string departments = "";

                if (allSelected)
                {
                    //remove if there are unselected departments
                    if (unselectedDepartmentList != null && unselectedDepartmentList.Length > 0)
                    {
                        foreach(var dep in unselectedDepartmentList)
                        {
                            var depToRemove = departmentList.Single(x => x.DepartmentId == dep);
                            departmentList.Remove(depToRemove);
                        }
                    }

                    // convert to string
                    departments = string.Join(",", departmentList.Select(x => x.DepartmentId));
                }
                else
                {
                    // if few are selected
                    if (selectedDepartmentList != null && selectedDepartmentList.Length > 0)
                    {
                        List<OrganisationDepartment> selectedDeps = new List<OrganisationDepartment>();
                        foreach (var dep in selectedDepartmentList)
                        {
                            OrganisationDepartment newDep = new OrganisationDepartment();
                            var depToAdd = departmentList.Single(x => x.DepartmentId == dep);
                            selectedDeps.Add(depToAdd);
                        }

                        // convert to string
                        departments = string.Join(",", selectedDeps.Select(x => x.DepartmentId));
                    }

                }

                searchCriteria.Departments = departments;
                var moduleRep = new ModuleRep();
                int result = moduleRep.SetDepartmentForAutoAllocationOfModule(searchCriteria);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { success = result }),
                    ContentType = "application/json"
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { success = -1 }),
                    ContentType = "application/json"
                };
            }
        }

        //set department to license auto allocation for a module - selected multiple departments in all locations
        [HttpPost]
        public ActionResult SetLicenseAutoAllocationForAllDepartmentsInOrganisation(DepartmentFilterForLicenseAutoAssignment searchCriteria, bool allSelected, Int64[] selectedDepartmentList, Int64[] unselectedDepartmentList)
        {
            try
            {
                // get list of all departments in location and convert to comma seperated string
                var utilities = new CommonMethods();
                var departmentList = utilities.GetDepartmentsForOrganisation(SessionHelper.CompanyId);
                string departments = "";

                if (allSelected)
                {
                    //remove if there are unselected departments
                    if (unselectedDepartmentList != null && unselectedDepartmentList.Length > 0)
                    {
                        foreach (var dep in unselectedDepartmentList)
                        {
                            var depToRemove = departmentList.Single(x => x.DepartmentId == dep);
                            departmentList.Remove(depToRemove);
                        }
                    }

                    // convert to string
                    departments = string.Join(",", departmentList.Select(x => x.DepartmentId));
                }
                else
                {
                    // if few are selected
                    if (selectedDepartmentList != null && selectedDepartmentList.Length > 0)
                    {
                        List<OrganisationDepartment> selectedDeps = new List<OrganisationDepartment>();
                        foreach (var dep in selectedDepartmentList)
                        {
                            OrganisationDepartment newDep = new OrganisationDepartment();
                            var depToAdd = departmentList.Single(x => x.DepartmentId == dep);
                            selectedDeps.Add(depToAdd);
                        }

                        // convert to string
                        departments = string.Join(",", selectedDeps.Select(x => x.DepartmentId));
                    }

                }

                searchCriteria.Departments = departments;
                var moduleRep = new ModuleRep();
                int result = moduleRep.SetAllOrganisationDepartmentsForAutoAllocationOfModule(searchCriteria);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { success = result }),
                    ContentType = "application/json"
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { success = -1 }),
                    ContentType = "application/json"
                };
            }
        }

        //set location to license auto allocation for a module - selected multiple locations in an organisation
        [HttpPost]
        public ActionResult SetLicenseAutoAllocationForLocationInOrganisation(LocationFilterForLicenseAutoAssignment searchCriteria, bool allSelected, Int64[] selectedLocationList, Int64[] unselectedLocationList)
        {
            try
            {
                // get list of all departments in location and convert to comma seperated string
                var utilities = new CommonMethods();
                var locationList = utilities.GetLocationsForCompany(SessionHelper.CompanyId);
                string locations = "";

                if (allSelected)
                {
                    //remove if there are unselected departments
                    if (unselectedLocationList != null && unselectedLocationList.Length > 0)
                    {
                        foreach (var loc in unselectedLocationList)
                        {
                            var locToRemove = locationList.Single(x => x.LocationId == loc);
                            locationList.Remove(locToRemove);
                        }
                    }

                    // convert to string
                    locations = string.Join(",", locationList.Select(x => x.LocationId));
                }
                else
                {
                    // if few are selected
                    if (selectedLocationList != null && selectedLocationList.Length > 0)
                    {
                        List<OrganisationLocation> selectedLocs = new List<OrganisationLocation>();
                        foreach (var loc in selectedLocationList)
                        {
                            OrganisationLocation newLoc = new OrganisationLocation();
                            var locToAdd = locationList.Single(x => x.LocationId == loc);
                            selectedLocs.Add(locToAdd);
                        }

                        // convert to string
                        locations = string.Join(",", selectedLocs.Select(x => x.LocationId));
                    }

                }

                searchCriteria.Locations = locations;
                var moduleRep = new ModuleRep();
                int result = moduleRep.SetAutoAllocationOfModuleForEntireLocationInOrg(searchCriteria);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { success = result }),
                    ContentType = "application/json"
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { success = -1 }),
                    ContentType = "application/json"
                };
            }
        }


        [HttpPost]
        public ActionResult SetLicenseAutoAllocationForEntireOrg(DataTableFilter searchCriteria)
        {
            try
            {
                searchCriteria.Company = SessionHelper.CompanyId;
                var moduleRep = new ModuleRep();
                int result = moduleRep.SetAutoAllocationOfModuleForEntireOrg(searchCriteria);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { success = result }),
                    ContentType = "application/json"
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { success = -1 }),
                    ContentType = "application/json"
                };
            }
        }

        //auto allocation for selected department in company, all locations
        [HttpPost]
        public ActionResult SetLicenseAutoAllocationForEntireOrgDepartment(DataTableFilter searchCriteria)
        {
            try
            {
                searchCriteria.Company = SessionHelper.CompanyId;
                var moduleRep = new ModuleRep();
                int result = moduleRep.SetLicenseAutoAllocationForEntireOrgDepartment(searchCriteria);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { success = result }),
                    ContentType = "application/json"
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { success = -1 }),
                    ContentType = "application/json"
                };
            }
        }

        // remove department to license auto allocation for a module
        [HttpPost]
        public ActionResult RemoveLicenseAutoAllocationForDepartment(DepartmentFilterForLicenseAutoAssignment searchCriteria)
        {
            try
            {
                var moduleRep = new ModuleRep();
                int result = moduleRep.RemoveDepartmentForAutoAllocationOfModule(searchCriteria);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { success = result }),
                    ContentType = "application/json"
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { success = -1 }),
                    ContentType = "application/json"
                };
            }
        }

        // remove department to license auto allocation for a module
        [HttpPost]
        public ActionResult RemoveLicenseAutoAllocationForDepartmentInOrg(DepartmentFilterForLicenseAutoAssignment searchCriteria)
        {
            try
            {
                var moduleRep = new ModuleRep();
                int result = moduleRep.RemoveDepartmentFromAllLocationsForAutoAllocationOfModule(searchCriteria);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { success = result }),
                    ContentType = "application/json"
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { success = -1 }),
                    ContentType = "application/json"
                };
            }
        }

        // remove license auto allocation for a module from a location
        [HttpPost]
        public ActionResult RemoveLicenseAutoAllocationFromLocationInOrg(LocationFilterForLicenseAutoAssignment searchCriteria)
        {
            try
            {
                var moduleRep = new ModuleRep();
                int result = moduleRep.RemoveAllLocationsForAutoAllocationOfModule(searchCriteria);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { success = result }),
                    ContentType = "application/json"
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { success = -1 }),
                    ContentType = "application/json"
                };
            }
        }

        [HttpPost]
        public ActionResult RemoveLicenseAutoAllocationForAllDepartments(DepartmentFilterForLicenseAutoAssignment searchCriteria)
        {
            try
            {
                // get list of all departments in location and convert to comma seperated string
                var utilities = new CommonMethods();
                var departmentList = utilities.GetDepartmentsForLocation(SessionHelper.UserRole, SessionHelper.UserId, searchCriteria.Location);
                var departments = string.Join(",", departmentList.Select(x => x.DepartmentId));
                searchCriteria.Departments = departments;

                var moduleRep = new ModuleRep();
                int result = moduleRep.RemoveDepartmentForAutoAllocationOfModule(searchCriteria);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { success = result }),
                    ContentType = "application/json"
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { success = -1 }),
                    ContentType = "application/json"
                };
            }
        }

        //function to remove from all departments in entire Organisation
        [HttpPost]
        public ActionResult RemoveLicenseAutoAllocationForAllDepartmentsInOrganisation(DepartmentFilterForLicenseAutoAssignment searchCriteria)
        {
            try
            {
                var moduleRep = new ModuleRep();
                int result = moduleRep.RemoveDepartmentForAutoAllocationOfModule(searchCriteria);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { success = result }),
                    ContentType = "application/json"
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { success = -1 }),
                    ContentType = "application/json"
                };
            }
        }

        //Function to free up unused licences from list of users
        [HttpPost]
        public ActionResult FreeUpUnusedLicenses(Int64 Course, String userList, int count)
        {
            try
            {
                var moduleRep = new ModuleRep();
                int result = moduleRep.FreeUpUnusedLicenses(Course, userList, count);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { success = result }),
                    ContentType = "application/json"
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { success = -1 }),
                    ContentType = "application/json"
                };
            }
        }

        //Function to free up unused licences from list of users
        [HttpPost]
        public ActionResult FreeUpUnusedLicensesFromMultiple(LearnerModuleFilter searchCriteria, bool allSelected, Int64[] selectedUserList, Int64[] unselectedUserList)
        {
            try
            {
                // convert to string

                string selUsers = "";
                string unselUsers = "";

                if (selectedUserList != null && selectedUserList.Length > 0)
                    selUsers = String.Join(",", selectedUserList);
                if (unselectedUserList != null && unselectedUserList.Length > 0)
                    unselUsers = String.Join(",", unselectedUserList);

                searchCriteria.Company = SessionHelper.CompanyId;

                var moduleRep = new ModuleRep();
                int result = moduleRep.FreeUpUnusedLicenses_Multiple(searchCriteria, selUsers, unselUsers, allSelected, selectedUserList.Length);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { success = result }),
                    ContentType = "application/json"
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new ContentResult {
                    Content = System.Text.Json.JsonSerializer.Serialize(new { success = -1 }),
                    ContentType = "application/json"
                };
            }
        }


        // Request licence page
        public ActionResult RequestLicence()
        {
            return View("RequestLicence");
        }
    }
}