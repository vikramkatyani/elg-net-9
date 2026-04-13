using ELG.Web.Helper;
using ELG.DAL.OrgAdminDAL;
using ELG.Model.OrgAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Action;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Pdf.Xobject;
using iText.Kernel.Geom;
using iText.Kernel.Font;
using ELG.DAL.LearnerDAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
//using System.Drawing;

namespace ELG.Web.Controllers
{
    [SessionCheck]
    public class CertificateController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public CertificateController(IWebHostEnvironment env)
        {
            _env = env;
        }
        public ActionResult External()
        {
            return View();
        }

        // GET: Certificate
        public ActionResult GetCertificate(Int64 id)
        {

            CourseProgressItem progress = new CourseProgressItem();
            try
            {
                var reportRep = new ReportRep();
                progress = reportRep.GetCertificateRecord(id);

                if (progress == null || progress.RecordId <= 0)
                {
                    return NotFound();
                }

                string strFilePath = System.IO.Path.Combine(_env.WebRootPath, "content", "certificate") + System.IO.Path.DirectorySeparatorChar;

                string userName = $"{progress.FirstName ?? string.Empty} {progress.LastName ?? string.Empty}".Trim();
                string courseName = progress.CourseName ?? string.Empty;
                string completiondate = progress.CompletionDate ?? string.Empty;
                string certificateNumber = progress.CertificateNumber ?? string.Empty;
                string certificateText = "has successfully completed the online course";
                //string certificateText_2 = "Donec aliquet lacus et nibh rutrum, non fringilla neque facilisis.";
                //string cpdScore = "";
                //file name to be created   
                string strPDFFileName = string.Format("Certificate_" + progress.RecordId + ".pdf");

                byte[] pdfBytes;
                using (var stream = new MemoryStream())
                using (var writer = new PdfWriter(stream))
                using (var pdf = new PdfDocument(writer))
                {
                    PdfPage page = pdf.AddNewPage(PageSize.A4);
                    var canvas = new iText.Kernel.Pdf.Canvas.PdfCanvas(page);
                    using (var document = new iText.Layout.Document(pdf, PageSize.A4))
                    {
                    Image img;
                    if (SessionHelper.CompanyCertificate != null && SessionHelper.CompanyCertificate.Length > 0)
                    {

                        img = new Image(ImageDataFactory
                           .Create(SessionHelper.CompanyCertificate))
                           .ScaleAbsolute(PageSize.A4.GetWidth(), PageSize.A4.GetHeight())
                           .SetFixedPosition(0, 0);
                    }
                    else
                    {
                        img = new Image(ImageDataFactory
                           .Create(strFilePath + "certificate_new.jpg"))
                           .ScaleAbsolute(PageSize.A4.GetWidth(), PageSize.A4.GetHeight())
                           .SetFixedPosition(0, 0);
                    }

                    document.Add(img);

                    string ttf_file_path_1 = System.IO.Path.Combine(_env.WebRootPath, "content", "certificate", "fonts", "NeueHaasDisplayBold.ttf");
                    string ttf_file_path_2 = System.IO.Path.Combine(_env.WebRootPath, "content", "certificate", "fonts", "NeueHaasDisplayThin.ttf");
                    PdfFont customFont_name = PdfFontFactory.CreateFont(ttf_file_path_1, "Identity-H");
                    PdfFont customFont_details = PdfFontFactory.CreateFont(ttf_file_path_2, "Identity-H");

                    float pageWidth = PageSize.A4.GetWidth();

                    float CenterTextX(PdfFont font, float fontSize, string text)
                    {
                        float textWidth = font.GetWidth(text, fontSize);
                        return (pageWidth - textWidth) / 2;
                    }

                    string safeCertificateNo = SanitizePdfText(certificateNumber);
                    string safeUserName = SanitizePdfText(userName);
                    string safeCertText = SanitizePdfText(certificateText);
                    string safeCourseName = SanitizePdfText(courseName);
                    string safeCompletionDate = SanitizePdfText(completiondate);

                    canvas.BeginText()
                        .SetFontAndSize(customFont_details, 10)
                        .SetTextMatrix(CenterTextX(customFont_details, 10, "CERTIFICATE NUMBER: " + safeCertificateNo), 20)
                        .ShowText("CERTIFICATE NUMBER: " + safeCertificateNo)
                        .EndText();

                    canvas.BeginText()
                        .SetFontAndSize(customFont_name, 48)
                        .SetTextMatrix(CenterTextX(customFont_name, 48, safeUserName), 420)
                        .ShowText(safeUserName)
                        .EndText();

                    canvas.BeginText()
                        .SetFontAndSize(customFont_details, 16)
                        .SetTextMatrix(CenterTextX(customFont_details, 16, safeCertText), 390)
                        .ShowText(safeCertText)
                        .EndText();

                    canvas.BeginText()
                        .SetFontAndSize(customFont_details, 20)
                        .SetTextMatrix(CenterTextX(customFont_details, 20, safeCourseName), 330)
                        .ShowText(safeCourseName)
                        .EndText();

                    canvas.BeginText()
                        .SetFontAndSize(customFont_details, 20)
                        .SetTextMatrix(CenterTextX(customFont_details, 20, safeCompletionDate), 250)
                        .ShowText(safeCompletionDate)
                        .EndText();

                    canvas.Stroke();
                    }

                    pdfBytes = stream.ToArray();
                }
                //return File(pdfBytes, "application/pdf", strPDFFileName);
                return new FileContentResult(pdfBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View();
            }
        }

        private static string SanitizePdfText(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return value.Replace("\r", " ").Replace("\n", " ").Trim();
        }

        // GET: Company Business Certificate
        public ActionResult GetBusinessCertificate()
        {

            CompanyBusinessCertificate details = new CompanyBusinessCertificate();
            try
            {
                var reportRep = new ReportRep();
                details = reportRep.GetCompanyBusinessCertificate(Convert.ToInt64(SessionHelper.CompanyId));

                string strFilePath = System.IO.Path.Combine(_env.WebRootPath, "content", "img") + System.IO.Path.DirectorySeparatorChar;

                string companyName = "";
                string assignedDate = "";
                string expiryDate = "";
                //file name to be created   
                string strPDFFileName = string.Format("BussinessCertificate_" + SessionHelper.CompanyNumber + ".pdf");


                byte[] pdfBytes;
                using (var stream = new MemoryStream())
                using (var writer = new PdfWriter(stream))
                using (var pdf = new PdfDocument(writer))
                using (var document = new iText.Layout.Document(pdf))
                {
                    if (!string.IsNullOrEmpty(details.CompanyName) || !string.IsNullOrEmpty(details.CompanyName))
                        companyName = details.CompanyName;
                    if (!string.IsNullOrEmpty(details.AssignedDate))
                        assignedDate = details.AssignedDate;
                    if (!string.IsNullOrEmpty(details.ExpiryDate))
                        expiryDate = details.ExpiryDate;


                    PdfFormXObject template = new PdfFormXObject(new Rectangle(0, 0, PageSize.A4.GetWidth(), PageSize.A4.GetHeight()));
                    Canvas templateCanvas = new Canvas(template, pdf);

                    // Add image
                    Image img = new Image(ImageDataFactory
                       .Create(strFilePath + "business_certificate.jpg"))
                       .SetTextAlignment(TextAlignment.CENTER);

                    string ttf_file_path = System.IO.Path.Combine(_env.WebRootPath, "content", "certificate", "fonts", "NeueHaasDisplayThin.ttf");
                    PdfFont customFont = PdfFontFactory.CreateFont(ttf_file_path, "Identity-H");

                    //Color hseColor = new DeviceRgb(243, 112, 33);

                    // Add paragraph
                    Paragraph paraCompanyName = new Paragraph(companyName).SetFont(customFont);
                    if(companyName.Length > 32)
                        paraCompanyName.SetFontSize(23);
                    else
                        paraCompanyName.SetFontSize(30);
                    Paragraph paraAssignedDate = new Paragraph(assignedDate).SetFont(customFont).SetFontSize(20);
                    Paragraph paraExpiryDate = new Paragraph(expiryDate).SetFont(customFont).SetFontSize(20);

                    templateCanvas.Add(img)
                    .ShowTextAligned(paraCompanyName, 300, 480, TextAlignment.CENTER)
                    .ShowTextAligned(paraAssignedDate, 310, 190, TextAlignment.CENTER)
                    .ShowTextAligned(paraExpiryDate, 310, 140, TextAlignment.CENTER);

                    // In order to add a formXObject to the document, one can wrap it with an image
                    document.Add(new Image(template));
                    templateCanvas.Close();

                    document.Close();
                    document.Flush();

                    pdfBytes = stream.ToArray();
                }
                return new FileContentResult(pdfBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return View();
            }
        }
    }
}