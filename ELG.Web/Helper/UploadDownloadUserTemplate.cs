using ELG.DAL.OrgAdminDAL;
using ELG.DAL.Utilities;
using ELG.Model.OrgAdmin;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ELG.Web.Helper
{
    public class UploadDownloadUserTemplate
    {
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;

        public UploadDownloadUserTemplate(Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            _env = env;
        }
        public int intExistingContactCount = 0; // Number of contacts already in database
        public int intUploadedContactCount = 0; // Number of contacts uploaded to database
        public int intUploadedEmptyContactCount = 0; // Number of contacts uploaded to database
        public int intFailedContactCount = 0; // Number of failed attempts

        /// <summary>
        /// Function to return list of fields as per organisation setting to be used as header of excel template for upload contacts
        /// </summary>
        /// <returns>list of header string</returns>
        public List<String> CreateHeaders(int orgID)
        {
            List<String> headerList = new List<string>();

            var adminRep = new AdminRep();
            OrganizationInfo regSettings = adminRep.GetAdminOrgRegSettings(orgID);
            
            Dictionary<string, bool> _dictionary = new Dictionary<string, bool>();
            _dictionary = getDictionary(regSettings.RegSettings);

            headerList.Add(regSettings.emailIdDescription.Trim().Replace(" ", "_"));
            headerList.Add(regSettings.strTitleDescription.Trim().Replace(" ", "_"));
            headerList.Add(regSettings.strFirstNameDescription.Trim().Replace(" ", "_"));
            headerList.Add(regSettings.strSurnameDescription.Trim().Replace(" ", "_"));
            headerList.Add(regSettings.strLocationDescription.Trim().Replace(" ", "_"));
            headerList.Add(regSettings.strDepartmentDescription.Trim().Replace(" ", "_"));
            headerList.Add(regSettings.strEmployeeNumberDescription.Trim().Replace(" ", "_"));

            //if (_dictionary["makemailusernm"] == false)
            //    headerList.Add(regSettings.strEmployeeNumberDescription.Trim().Replace(" ", "_"));

            //if (regSettings.PayrollMandatory == true)
            //    headerList.Add(regSettings.PayrollDescription.Trim().Replace(" ", "_"));

            //if (regSettings.JobtitleMandatory == true)
            //    headerList.Add(regSettings.JobtitleDescription.Trim().Replace(" ", "_"));

            //headerList.Add("Notes");

            return headerList;
        }
        
        public Dictionary<string, bool> getDictionary(string s)
        {
            Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
            try
            {
                if (s == "" || s == null)
                {
                    throw new Exception();
                }
                string[] tokens = s.Split(new char[] { ':', ',' }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < tokens.Length; i += 2)
                {
                    string name = tokens[i];
                    string freq = tokens[i + 1];
                    bool count = bool.Parse(freq);
                    dictionary.Add(name, count);
                }
                return dictionary;
            }
            catch
            {
                dictionary.Add("regdeny", false);
                dictionary.Add("makemailusernm", false);
                dictionary.Add("titlediplay", true);
                dictionary.Add("titlemandatory", true);
                dictionary.Add("fnamedisplay", true);
                dictionary.Add("fnamemandatory", true);
                dictionary.Add("snamedisplay", true);
                dictionary.Add("snamemandatory", true);
                //dictionary.Add("blnPayrollMandatory", true);
                //dictionary.Add("blnJobtitleMandatory", true);
                return dictionary;
            }
        }

        //public async Task<UploadContactResult> AsynchImportContact(Int64 intOrganisationID, int role, Int64 userID, IFormFile inputFile, string fileType, string csvfilepath)
        public async Task<UploadContactResult> AsynchImportContact(Int64 intOrganisationID, bool isSSoOrg, Int64 userID, Microsoft.AspNetCore.Http.IFormFile inputFile, string fileType, string csvfilepath)
        {
            UploadContactResult result = new UploadContactResult();
            await Task.Run(() =>
            {
                //result = ImportContact(intOrganisationID, role, userID, inputFile, fileType, csvfilepath);
                // string host = HttpContext.Current.Request.Url.Host; // Not available in ASP.NET Core, should be passed in or accessed via DI
                string host = ""; // TODO: Pass host from controller or context
                result = ImportContact(intOrganisationID, isSSoOrg, userID, inputFile, fileType, csvfilepath, host);
            });
            return result;
        }

        /// <summary>
        /// Import contacts
        /// </summary>
        /// <param name="intOrganisationID"></param>
        /// <param name="strFileName"></param>
        /// <returns>
        /// Error 0 => No error
        /// Error 1 => Empty excel, No data to upload
        /// Error 2 => In correct column name in header
        /// </returns>
        //public UploadContactResult ImportContact(Int64 intOrganisationID, int role, Int64 userID, IFormFile inputFile, string fileType, string csvfilepath)
        public UploadContactResult ImportContact(Int64 intOrganisationID, bool isSSoOrg, Int64 userID, Microsoft.AspNetCore.Http.IFormFile inputFile, string fileType, string csvfilepath, string domain)
        {

            UploadContactResult result = new UploadContactResult();

            var adminRep = new AdminRep();
            EmailUtility emailUti = new EmailUtility(_env);
            var learnerRep = new LearnerRep();
            LearnerInfo learner = new LearnerInfo();


            var companyRep = new CompanyRep();
            var org = companyRep.GetCompanyInfo(SessionHelper.CompanyId);

            int intDepartmentID=0;
            int intLocationID=0;
            var locFilter = new CommonMethods();

            List<OrganisationLocation> locationList = new List<OrganisationLocation>();
            //locationList = locFilter.GetLocationsForCompany(role, userID, intOrganisationID);
            locationList = locFilter.GetLocationsForCompany(intOrganisationID);

            string filterstring = "";
            DataTable dtClient = new DataTable();

            if (fileType == ".xls" || fileType == ".xlsx")
                dtClient = Fill_dataTable(inputFile.OpenReadStream());
            else
                dtClient = ReadCSVFile(csvfilepath);

            // check empty data table (spread sheet)
            if (dtClient == null || dtClient.Rows.Count <= 0)
            {
                result.Error = 1;
                return result;
            }

            dtClient = dtClient.Rows.Cast<DataRow>().Where(row => !row.ItemArray.All(field => field is DBNull || string.IsNullOrWhiteSpace(field as string))).CopyToDataTable();

            OrganizationInfo regSettings = adminRep.GetAdminOrgRegSettings(intOrganisationID);

            Dictionary<string, bool> _dictionary = getDictionary(regSettings.RegSettings);

            //filterstring += regSettings.strTitleDescription.Trim().Replace(" ", "_") + " <> '' AND " + regSettings.strTitleDescription.Trim().Replace(" ", "_") + " is not null ";
            filterstring += regSettings.strFirstNameDescription.Trim().Replace(" ", "_") + " <> '' AND " + regSettings.strFirstNameDescription.Trim().Replace(" ", "_") + " is not null ";
            filterstring += " AND " + regSettings.strSurnameDescription.Trim().Replace(" ", "_") + " <> '' AND " + regSettings.strSurnameDescription.Trim().Replace(" ", "_") + " is not null ";
            filterstring += " AND " + regSettings.emailIdDescription.Trim().Replace(" ", "_") + " <> '' AND " + regSettings.emailIdDescription.Trim().Replace(" ", "_") + " is not null ";

            //checking format of excel columns
            if (!dtClient.Columns.Contains(regSettings.strTitleDescription.Trim().Replace(" ", "_")))
            {
                result.Error = 2;
                return result;
            }

            if (!dtClient.Columns.Contains(regSettings.strFirstNameDescription.Trim().Replace(" ", "_")))
            {
                result.Error = 2;
                return result;
            }

            if (!dtClient.Columns.Contains(regSettings.strSurnameDescription.Trim().Replace(" ", "_")))
            {
                result.Error = 2;
                return result;
            }

            if (!dtClient.Columns.Contains(regSettings.emailIdDescription.Trim().Replace(" ", "_")))
            {
                result.Error = 2;
                return result;
            }

            if (!dtClient.Columns.Contains(regSettings.strLocationDescription.Trim().Replace(" ", "_")))
            {
                result.Error = 2;
                return result;
            }

            if (!dtClient.Columns.Contains(regSettings.strDepartmentDescription.Trim().Replace(" ", "_")))
            {
                result.Error = 2;
                return result;
            }

            DataRow[] drs = dtClient.Select(filterstring);

            // Check empty record
            if (drs == null || drs.Length == 0)
            {
                result.Error = 1;
                return result;
            }

            // Step through rows and insert/update entries
            this.intExistingContactCount = 0; // Number of contacts already in database
            this.intUploadedContactCount = 0; // Number of contacts uploaded to database
            this.intFailedContactCount = 0; // Number of contacts uploaded to database
            this.intUploadedEmptyContactCount = (dtClient.Rows.Count - drs.Length); // Number of contacts empty in uploaded file

            //looping over each row
            foreach (DataRow drwRow in drs)
            {
                //setting learner details
                learner.CompanyID = intOrganisationID;
                learner.Title = drwRow[regSettings.strTitleDescription.Trim().Replace(" ", "_")].ToString().Trim();
                learner.FirstName = drwRow[regSettings.strFirstNameDescription.Trim().Replace(" ", "_")].ToString().Trim();
                learner.LastName = drwRow[regSettings.strSurnameDescription.Trim().Replace(" ", "_")].ToString().Trim();
                learner.EmailId = drwRow[regSettings.emailIdDescription.Trim().Replace(" ", "_")].ToString().Trim();
                learner.Department = drwRow[regSettings.strDepartmentDescription.Trim().Replace(" ", "_")].ToString().Trim();
                learner.Location = drwRow[regSettings.strLocationDescription.Trim().Replace(" ", "_")].ToString().Trim();
                learner.EmployeeNumber = drwRow[regSettings.strEmployeeNumberDescription.Trim().Replace(" ", "_")].ToString().Trim();

                if (!String.IsNullOrWhiteSpace(learner.Location))
                {
                    // getting location id from loaction string
                    OrganisationLocation loc= locationList.Find(x => x.LocationName.Trim().ToLower() == learner.Location.ToLower());
                    if (loc != null) { intLocationID = Convert.ToInt32(loc.LocationId); }

                    //commented to avoid creating new location
                    //else
                    //{
                    //    //create new location if not found
                    //    Int64 newLocId = locFilter.CreateNewLocation(intOrganisationID, learner.Location);
                    //    intLocationID = Convert.ToInt32(newLocId);

                    //    //create department and map with location
                    //    Int64 newDepId = locFilter.CreateNewLocationDepartment(intOrganisationID, intLocationID, learner.Department);

                    //    //update location list
                    //    locationList = new List<OrganisationLocation>();
                    //    locationList = locFilter.GetLocationsForCompany(SessionHelper.UserRole, SessionHelper.UserId, intOrganisationID);
                    //}
                    else
                    {
                        ++intFailedContactCount;
                        break;
                    }
                }

                if ((!String.IsNullOrWhiteSpace(learner.Location)) && (!String.IsNullOrWhiteSpace(learner.Department)))
                {
                    List<OrganisationDepartment> departmentList = new List<OrganisationDepartment>();
                    //departmentList = locFilter.GetDepartmentsForLocation(role, userID, intLocationID);
                    departmentList = locFilter.GetDepartmentsForLocation_Admin(intLocationID);

                    OrganisationDepartment dep = departmentList.Find(x => x.DepartmentName.Trim().ToLower() == learner.Department.ToLower());
                    if (dep != null) { intDepartmentID = Convert.ToInt32(dep.DepartmentId); }

                    //commented to avoid creation of new department while uploading users
                    //else
                    //{
                    //    //create department if not exists
                    //    intDepartmentID = Convert.ToInt32(locFilter.CreateNewLocationDepartment(intOrganisationID, intLocationID, learner.Department));
                    //}
                    else
                    {
                        ++intFailedContactCount;
                        break;
                    }
                }

                learner.LocationID = intLocationID;
                learner.DepartmentID = intDepartmentID;

                NewLearner newLearner = learnerRep.CreateNewLearner(learner, SessionHelper.CompanyCourseAssignmentMode, SessionHelper.UserRole, SessionHelper.UserId);
                int learnerId = Convert.ToInt32(newLearner.UserID);

                if (learnerId >= 1)
                {
                    ++intUploadedContactCount;

                    //send email
                    //mail format for web version			
                    string learnerEmail = learner.EmailId;

                    string link = "";
                    string emailTemplate = "";
                    string subject = $"{org.CompanyBrandName}: Account Activation Link Inside";

                    if (isSSoOrg)
                    {
                        emailTemplate = emailUti.GetEmailTemplate("NewSSOUserEmail.html");
                        //link = ConfigurationManager.AppSettings["LMS_LearnerBaseURL"] + "ssoaccount";
                        link = domain + "/ssoaccount";
                    }

                    else
                    {
                        emailTemplate = emailUti.GetEmailTemplate("NewUserEmail.html");
                        link = emailUti.CreateLoginLinkToBeSendInMail(org.CompanyBaseURL, learnerId, true);
                    }

                    emailTemplate = emailTemplate.Replace("{username}", $"{learner.FirstName} {learner.LastName}");
                    emailTemplate = emailTemplate.Replace("{tenantbrandname}", org.CompanyBrandName);
                    emailTemplate = emailTemplate.Replace("{tenantname}", org.CompanyName);
                    emailTemplate = emailTemplate.Replace("{tenantcontactemail}", org.CompanySupportEmail);
                    emailTemplate = emailTemplate.Replace("{loginLink}", link);
                    emailTemplate = emailTemplate.Replace("{useremail}", learner.EmailId);

                    try
                    {
                        emailUti.SendEmailUsingSendGrid(drwRow[regSettings.emailIdDescription.Trim().Replace(" ", "_")].ToString().Trim(), subject, emailTemplate);
                    }
                    catch (Exception)
                    {
                    }
                }
                else if (learnerId == 0)
                {
                    ++intExistingContactCount;
                }
                else if (learnerId == -99)
                {
                    result.Error = -99;
                    ++intFailedContactCount;
                }
                // Error
                else
                {
                    ++intFailedContactCount;
                }
            }
            result.ExistingCount = intExistingContactCount;
            result.UploadedCount = intUploadedContactCount;
            result.EmptyCount = intUploadedEmptyContactCount;
            result.FailedCount = intFailedContactCount;
            result.Error = 0;
            return result;
        }

        // creating data table from spread sheet
        public static DataTable Fill_dataTable(Stream fileName)
        {
            DataTable dt = new DataTable();

            using (SpreadsheetDocument spreadSheetDocument = SpreadsheetDocument.Open(fileName, false))
            {
                WorkbookPart workbookPart = spreadSheetDocument.WorkbookPart;
                IEnumerable<Sheet> sheets = spreadSheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();
                string relationshipId = sheets.First().Id.Value;
                WorksheetPart worksheetPart = (WorksheetPart)spreadSheetDocument.WorkbookPart.GetPartById(relationshipId);
                Worksheet workSheet = worksheetPart.Worksheet;
                SheetData sheetData = workSheet.GetFirstChild<SheetData>();
                IEnumerable<Row> rows = sheetData.Descendants<Row>();

                foreach (Cell cell in rows.ElementAt(0))
                {
                    dt.Columns.Add(GetCellValue(spreadSheetDocument, cell));
                }

                foreach (Row row in rows) 
                {
                    DataRow tempRow = dt.NewRow();

                    for (int i = 0; i < row.Descendants<Cell>().Count(); i++)
                    {
                        Cell cell = row.Descendants<Cell>().ElementAt(i);
                        int actualCellIndex = CellReferenceToIndex(cell);
                        tempRow[actualCellIndex] = GetCellValue(spreadSheetDocument, cell);
                    }

                    dt.Rows.Add(tempRow);
                }
            }

            dt.Rows.RemoveAt(0); //removing header row
            return dt;
        }

        private static int CellReferenceToIndex(Cell cell)
        {
            int index = 0;
            string reference = cell.CellReference.ToString().ToUpper();
            foreach (char ch in reference)
            {
                if (Char.IsLetter(ch))
                {
                    int value = (int)ch - (int)'A';
                    index = (index == 0) ? value : ((index + 1) * 26) + value;
                }
                else
                    return index;
            }
            return index;
        }

        public static string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
            string value = cell.InnerText;

            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                return stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
            }
            else
            {
                return value;
            }
        }

        private DataTable ReadCSVFile(string csvPath)
        {
            //Read the contents of CSV file.  
            string csvData = File.ReadAllText(csvPath);
            csvData = csvData.Replace("\r", "\n");
            DataTable dtClient = new DataTable();

            bool firstRow = true;
            //Execute a loop over the rows.  
            foreach (string row in csvData.Split('\n'))
            {
                if (!string.IsNullOrEmpty(row))
                {
                    int i = 0;

                    if (firstRow)
                    {
                        foreach (string cell in row.Split(','))
                        {
                            dtClient.Columns.Add(cell);
                            i++;
                        }
                        firstRow = false;
                    }
                    else
                    {
                        dtClient.Rows.Add();

                        //Execute a loop over the columns.  
                        foreach (string cell in row.Split(','))
                        {
                            dtClient.Rows[dtClient.Rows.Count - 1][i] = cell;
                            i++;
                        }

                    }
                }
            }
            return dtClient;
        }
    }
}