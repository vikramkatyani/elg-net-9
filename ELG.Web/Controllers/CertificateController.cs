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

                string strFilePath = System.IO.Path.Combine(_env.WebRootPath, "content", "certificate") + System.IO.Path.DirectorySeparatorChar;

                string userName = "";
                string courseName = "";
                string score = "";
                string completiondate = "";
                string certificateNumber = "";
                string certificateText = "has successfully completed the online course";
                //string certificateText_2 = "Donec aliquet lacus et nibh rutrum, non fringilla neque facilisis.";
                //string cpdScore = "";
                //file name to be created   
                string strPDFFileName = string.Format("Certificate_" + progress.RecordId + ".pdf");

                byte[] pdfBytes;
                using (var stream = new MemoryStream())
                using (var writer = new PdfWriter(stream))
                using (var pdf = new PdfDocument(writer))
                using (var document = new iText.Layout.Document(pdf, PageSize.A4))
                {
                    if (!string.IsNullOrEmpty(progress.CertificateNumber))
                        certificateNumber = progress.CertificateNumber;
                    if (!string.IsNullOrEmpty(progress.FirstName) || !string.IsNullOrEmpty(progress.FirstName))
                        userName = progress.FirstName + " " + progress.LastName;
                    if (!string.IsNullOrEmpty(progress.CourseName))
                        courseName = progress.CourseName;
                    if (!string.IsNullOrEmpty(progress.CompletionDate))
                        completiondate = progress.CompletionDate;
                    if (!string.IsNullOrEmpty(Convert.ToString(progress.Score)))
                        score = Convert.ToString(progress.Score);
                    //if (progress.CPDScore > 0)
                    //    cpdScore = Convert.ToString(progress.CPDScore);


                    PdfFormXObject template = new PdfFormXObject(new Rectangle(0, 0, PageSize.A4.GetWidth(), PageSize.A4.GetHeight()));
                    Canvas templateCanvas = new Canvas(template, pdf);

                    //// Add image
                    //Image img = new Image(ImageDataFactory
                    //   .Create(strFilePath + certificateImage))
                    //   .SetTextAlignment(TextAlignment.CENTER);
                    Image img;
                    if (SessionHelper.CompanyCertificate != null && SessionHelper.CompanyCertificate.Length > 0)
                    {

                        img = new Image(ImageDataFactory
                           .Create(SessionHelper.CompanyCertificate))
                           .SetTextAlignment(TextAlignment.CENTER);
                    }
                    else
                    {
                        img = new Image(ImageDataFactory
                           .Create(strFilePath + "certificate_new.jpg"))
                           .SetTextAlignment(TextAlignment.CENTER);
                    }

                    string ttf_file_path_1 = System.IO.Path.Combine(_env.WebRootPath, "content", "certificate", "fonts", "NeueHaasDisplayBold.ttf");
                    string ttf_file_path_2 = System.IO.Path.Combine(_env.WebRootPath, "content", "certificate", "fonts", "NeueHaasDisplayThin.ttf");
                    PdfFont customFont_name = PdfFontFactory.CreateFont(ttf_file_path_1, "Identity-H");
                    PdfFont customFont_details = PdfFontFactory.CreateFont(ttf_file_path_2, "Identity-H");

                    Color certNoColor = new DeviceRgb(0, 0, 0);

                    // Add paragraph

                    Paragraph paraCertNo = new Paragraph("CERTIFICATE NUMBER: " + certificateNumber).SetFont(customFont_details).SetFontSize(10).SetFontColor(certNoColor).SetTextAlignment(TextAlignment.CENTER);
                    Paragraph parauserName = new Paragraph(userName).SetFont(customFont_name).SetFontSize(48).SetTextAlignment(TextAlignment.CENTER);
                    Paragraph paracourseName = new Paragraph(courseName).SetFont(customFont_details).SetFontSize(20).SetTextAlignment(TextAlignment.CENTER);
                    Paragraph paracompletiondate = new Paragraph(completiondate).SetFont(customFont_details).SetFontSize(20).SetTextAlignment(TextAlignment.CENTER);
                    Paragraph paraCertText = new Paragraph(certificateText).SetFont(customFont_details).SetFontSize(16).SetTextAlignment(TextAlignment.CENTER);
                    //Paragraph paraCertText_2 = new Paragraph(certificateText_2).SetFont(customFont_details).SetFontSize(10).SetTextAlignment(TextAlignment.CENTER);

                    //Paragraph paraCPDTitle = new Paragraph().SetFont(customFont).SetFontSize(10).SetBold();
                    //Paragraph paraCPDScore = new Paragraph("CPD Hour Credits: " + cpdScore).SetFont(customFont).SetFontSize(12);

                    templateCanvas.Add(img)
                    .ShowTextAligned(paraCertNo, 255, 790, TextAlignment.LEFT)
                    .ShowTextAligned(parauserName, 300, 420, TextAlignment.CENTER)
                    .ShowTextAligned(paraCertText, 300, 390, TextAlignment.CENTER)
                    //.ShowTextAligned(paraCertText_2, 300, 380, TextAlignment.CENTER)
                    .ShowTextAligned(paracourseName, 300, 315, TextAlignment.CENTER)
                    .ShowTextAligned(paracompletiondate, 300, 250, TextAlignment.CENTER);

                    // In order to add a formXObject to the document, one can wrap it with an image
                    document.Add(new Image(template));
                    templateCanvas.Close();

                    // Close document
                    document.Close();
                    document.Flush();

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