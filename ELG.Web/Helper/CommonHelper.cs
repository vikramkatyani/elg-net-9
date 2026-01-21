using ClosedXML.Excel;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;

namespace ELG.Web.Helper
{
    public class CommonHelper
    {

        /// <summary>
        /// Holds the application configuration (set in Program.cs)
        /// </summary>
        public static IConfiguration Configuration { get; set; }

        /// <summary>
        /// Read value from appsettings.json using key name
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetAppSettingValue(string key)
        {
            return Configuration[key];
        }

        /// <summary>
        /// Read connection string by name
        /// </summary>
        public static string GetConnectionString(string name)
        {
            return Configuration.GetConnectionString(name);
        }


        public static string ExcelContentType
        {
            get
            { return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"; }
        }

        /// <summary>
        /// convert list to datatable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static DataTable ListToDataTable<T>(List<T> data)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            DataTable dataTable = new DataTable();

            for (int i = 0; i < properties.Count; i++)
            {
                PropertyDescriptor property = properties[i];
                dataTable.Columns.Add(property.Name, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
            }

            object[] values = new object[properties.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = properties[i].GetValue(item);
                }

                dataTable.Rows.Add(values);
            }
            return dataTable;
        }

        //public static byte[] ExportExcelWithHeader(DataTable dataTable, string heading = "", bool showSrNo = false, string[] headerArray = null, params string[] columnsToTake)
        //{
        //    byte[] result = null;
        //    using (ExcelPackage package = new ExcelPackage())
        //    {
        //        ExcelWorksheet workSheet = package.Workbook.Worksheets.Add($"{heading} Data");
        //        int startRowFrom = 1;

        //        if (showSrNo)
        //        {
        //            DataColumn dataColumn = dataTable.Columns.Add("#", typeof(int));
        //            dataColumn.SetOrdinal(0);
        //            int index = 1;
        //            foreach (DataRow item in dataTable.Rows)
        //            {
        //                item[0] = index++;
        //            }
        //        }

        //        // Removed default header creation from LoadFromDataTable
        //        workSheet.Cells["A" + startRowFrom].LoadFromDataTable(dataTable, false);

        //        int columnIndex = 1;
        //        foreach (DataColumn column in dataTable.Columns)
        //        {
        //            workSheet.Column(columnIndex).AutoFit();
        //            columnIndex++;
        //        }

        //        // Set custom headers if provided
        //        if (headerArray != null && headerArray.Length > 0)
        //        {
        //            for (int i = 0; i < headerArray.Length; i++)
        //            {
        //                workSheet.Cells[startRowFrom, i + 1].Value = headerArray[i];
        //            }
        //        }

        //        // Format header - bold, background color
        //        using (ExcelRange r = workSheet.Cells[startRowFrom, 1, startRowFrom, headerArray?.Length ?? dataTable.Columns.Count])
        //        {
        //            r.Style.Font.Color.SetColor(System.Drawing.Color.White);
        //            r.Style.Font.Bold = true;
        //            r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        //            r.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#1fb5ad"));
        //        }

        //        // Remove ignored columns from `columnsToTake`
        //        for (int i = dataTable.Columns.Count - 1; i >= 0; i--)
        //        {
        //            if (i == 0 && showSrNo)
        //            {
        //                continue;
        //            }
        //            if (!columnsToTake.Contains(dataTable.Columns[i].ColumnName))
        //            {
        //                workSheet.DeleteColumn(i + 1);
        //            }
        //        }

        //        result = package.GetAsByteArray();
        //    }
        //    return result;
        //}
        public static byte[] ExportExcelWithHeader(DataTable dataTable, string heading = "", bool showSrNo = false, string[] headerArray = null, params string[] columnsToTake)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add($"{heading} Data");
                int startRowFrom = 1;

                if (showSrNo)
                {
                    dataTable.Columns.Add("#", typeof(int)).SetOrdinal(0);
                    for (int i = 0; i < dataTable.Rows.Count; i++)
                    {
                        dataTable.Rows[i][0] = i + 1;
                    }
                }

                // Set headers from headerArray
                if (headerArray != null && headerArray.Length > 0)
                {
                    for (int i = 0; i < headerArray.Length; i++)
                    {
                        worksheet.Cell(startRowFrom, i + 1).Value = headerArray[i];
                        worksheet.Cell(startRowFrom, i + 1).Style.Font.Bold = true;
                        worksheet.Cell(startRowFrom, i + 1).Style.Fill.BackgroundColor = XLColor.Teal;
                        worksheet.Cell(startRowFrom, i + 1).Style.Font.FontColor = XLColor.White;
                    }
                }

                // Load data below the headers
                int dataStartRow = startRowFrom + 1;
                foreach (DataRow row in dataTable.Rows)
                {
                    int colIndex = 1;
                    if (showSrNo)
                    {
                        colIndex++;
                    }
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        if (columnsToTake.Contains(column.ColumnName))
                        {
                            worksheet.Cell(dataStartRow, colIndex).Value = Convert.ToString(row[column.ColumnName]); 
                            worksheet.Column(colIndex).AdjustToContents();
                            colIndex++;
                        }
                    }
                    dataStartRow++; // Move to the next row
                }

                // Convert to byte array
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
        public static byte[] ExportExcel(DataTable dataTable, string heading = "", bool showSrNo = false, params string[] columnsToTake)
        {

            byte[] result = null;

            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet workSheet = package.Workbook.Worksheets.Add(String.Format("{0} Data", heading));
                //int startRowFrom = String.IsNullOrEmpty(heading) ? 1 : 3;
                int startRowFrom = 1;

                if (showSrNo)
                {
                    DataColumn dataColumn = dataTable.Columns.Add("#", typeof(int));
                    dataColumn.SetOrdinal(0);
                    int index = 1;
                    foreach (DataRow item in dataTable.Rows)
                    {
                        item[0] = index;
                        index++;
                    }
                }


                // add the content into the Excel file  
                workSheet.Cells["A" + startRowFrom].LoadFromDataTable(dataTable, true);

                // autofit width of cells with small content  
                int columnIndex = 1;
                foreach (DataColumn column in dataTable.Columns)
                {
                    ExcelRange columnCells = workSheet.Cells[workSheet.Dimension.Start.Row, columnIndex, workSheet.Dimension.End.Row, columnIndex];
                    workSheet.Column(columnIndex).AutoFit();
                    columnIndex++;
                }

                // format header - bold, yellow on black  
                using (ExcelRange r = workSheet.Cells[startRowFrom, 1, startRowFrom, dataTable.Columns.Count])
                {
                    r.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    r.Style.Font.Bold = true;
                    r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    r.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#1fb5ad"));
                }

                // format cells - add borders  
                //using (ExcelRange r = workSheet.Cells[startRowFrom + 1, 1, startRowFrom + dataTable.Rows.Count, dataTable.Columns.Count])
                //{
                //    r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                //    r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                //    r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                //    r.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                //    r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                //    r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                //    r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                //    r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                //}

                // removed ignored columns  
                for (int i = dataTable.Columns.Count - 1; i >= 0; i--)
                {
                    if (i == 0 && showSrNo)
                    {
                        continue;
                    }
                    if (!columnsToTake.Contains(dataTable.Columns[i].ColumnName))
                    {
                        workSheet.DeleteColumn(i + 1);
                    }
                }

                //if (!String.IsNullOrEmpty(heading))
                //{
                //    workSheet.Cells["A1"].Value = heading;
                //    workSheet.Cells["A1"].Style.Font.Size = 20;

                //    workSheet.InsertColumn(1, 1);
                //    workSheet.InsertRow(1, 1);
                //    workSheet.Column(1).Width = 5;
                //}

                result = package.GetAsByteArray();
            }

            return result;
        }

        public static byte[] ExportExcel<T>(List<T> data, string Heading = "", bool showSlno = false, params string[] ColumnsToTake)
        {
            return ExportExcel(ListToDataTable<T>(data), Heading, showSlno, ColumnsToTake);
        }

        public static string GetAdminLayoutPage(int adminRole)
        {
            string Layout = "~/Views/Shared/_Layout.cshtml";
            switch (adminRole)
            {
                case 1:
                    Layout = "~/Views/Shared/_Layout.cshtml";
                    break;
                case 2:
                    Layout = "~/Views/Shared/_DepartmentAdminLayout.cshtml";
                    break;
                case 3:
                    Layout = "~/Views/Shared/_LocationDepartmentAdminLayout.cshtml";
                    break;
                case 4:
                    Layout = "~/Views/Shared/_ReportAdminLayout.cshtml";
                    break;
                case 5:
                    Layout = "~/Views/Shared/_UserAdminLayout.cshtml";
                    break;
                case 6:
                    Layout = "~/Views/Shared/_ResellerAdminLayout.cshtml";
                    break;
                case 8:
                    Layout = "~/Views/Shared/_LocationSupervisorLayout.cshtml";
                    break;
                case 9:
                    Layout = "~/Views/Shared/_DepartmentAdminLayout.cshtml";
                    break;
                default:
                    Layout = "~/Views/Shared/_Layout.cshtml";
                    break;
            }
            return Layout;
        }

        public static bool EnsureAdminRole(int role)
        {
            bool valid = false;
            var sessionPriv = SessionHelper.AdminPrivileges;
            if(sessionPriv != null && sessionPriv.Count > 1)
            {
                var validRole = sessionPriv.Where(p => p.AdminRole == role);
                if (validRole != null)
                    valid = true;
            }
            return valid;
        }

        public static byte[] ConvertToBytes(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null) return null;
            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public static Image ConvertByteToImage(byte[] byteArrayIn)
        {
            try
            {
                // Image.FromStream is only supported on Windows
                #if Windows
                MemoryStream ms = new MemoryStream(byteArrayIn);
                Image returnImage = Image.FromStream(ms);
                return returnImage;
                #else
                // On non-Windows platforms, return null as System.Drawing.Common is Windows-only
                return null;
                #endif
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}