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
using ELG.Web.Helper;
using ELG.Model.Learner;
using ELG.DAL.LearnerDAL;
using iText.Kernel.Font;
using iText.Kernel.Pdf.Canvas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;

namespace ELG.Web.Areas.Learner.Controllers
{
    [Area("Learner")]
    [SessionCheck]
    public class CertificateController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public CertificateController(IWebHostEnvironment env)
        {
            _env = env;
        }

        public ActionResult GetCertificate(long id)
        {
            LearnerCertificateRecord progress = new LearnerCertificateRecord();
            try
            {
                var reportRep = new LearnerCourseRep();
                progress = reportRep.GetCertificateRecord(id);

                if (progress == null || progress.RecordId <= 0)
                {
                    return NotFound();
                }

                // Construct path: use WebRootPath only (it already contains wwwroot)
                string basePath = _env.WebRootPath ?? System.IO.Path.Combine(_env.ContentRootPath, "wwwroot");
                string strFilePath = System.IO.Path.Combine(basePath, "content", "certificate");

                string userName = $"{progress.FirstName ?? string.Empty} {progress.LastName ?? string.Empty}".Trim();
                string courseName = progress.CourseName ?? string.Empty;
                string completiondate = progress.CompletionDate ?? string.Empty;
                string certificateNumber = progress.CertificateNumber ?? string.Empty;
                string certificateText = "has successfully completed the online course";

                string strPDFFileName = $"Certificate_{progress.RecordId}.pdf";

                byte[] pdfBytes;
                using (var stream = new MemoryStream())
                using (var writer = new PdfWriter(stream))
                using (var pdf = new PdfDocument(writer))
                {
                    PdfPage page = pdf.AddNewPage();
                    PdfCanvas canvas = new PdfCanvas(page);

                    Image img;
                    if (SessionHelper.CompanyCertificate != null && SessionHelper.CompanyCertificate.Length > 0)
                    {
                        img = new Image(ImageDataFactory.Create(SessionHelper.CompanyCertificate))
                            .ScaleAbsolute(PageSize.A4.GetWidth(), PageSize.A4.GetHeight())
                            .SetFixedPosition(0, 0);
                    }
                    else
                    {
                        img = new Image(ImageDataFactory.Create(System.IO.Path.Combine(strFilePath, "certificate_new.jpg")))
                            .ScaleAbsolute(PageSize.A4.GetWidth(), PageSize.A4.GetHeight())
                            .SetFixedPosition(0, 0);
                    }

                    // Add image directly to document
                    iText.Layout.Document doc = new iText.Layout.Document(pdf);
                    doc.Add(img);

                    // Set Fonts
                    string ttfFilePath1 = System.IO.Path.Combine(strFilePath, "fonts", "NeueHaasDisplayBold.ttf");
                    string ttfFilePath2 = System.IO.Path.Combine(strFilePath, "fonts", "NeueHaasDisplayThin.ttf");
                    PdfFont customFontName = PdfFontFactory.CreateFont(ttfFilePath1, "Identity-H");
                    PdfFont customFontDetails = PdfFontFactory.CreateFont(ttfFilePath2, "Identity-H");

                    // Draw Text Over Image
                    float pageWidth = PageSize.A4.GetWidth();
                    float pageHeight = PageSize.A4.GetHeight();

                    // Function to center text horizontally
                    float CenterTextX(PdfFont font, float fontSize, string text)
                    {
                        float textWidth = font.GetWidth(text, fontSize); // Get text width
                        return (pageWidth - textWidth) / 2; // Centered X position
                    }

                    // Draw Text Over Image Properly Center-Aligned
                    canvas.BeginText()
                        .SetFontAndSize(customFontDetails, 10)
                        .SetTextMatrix(CenterTextX(customFontDetails, 10, "CERTIFICATE NUMBER: " + SanitizePdfText(certificateNumber)), 20)
                        .ShowText("CERTIFICATE NUMBER: " + SanitizePdfText(certificateNumber))
                        .EndText();

                    canvas.BeginText()
                        .SetFontAndSize(customFontName, 48)
                        .SetTextMatrix(CenterTextX(customFontName, 48, SanitizePdfText(userName)), 420)
                        .ShowText(SanitizePdfText(userName))
                        .EndText();

                    canvas.BeginText()
                        .SetFontAndSize(customFontDetails, 16)
                        .SetTextMatrix(CenterTextX(customFontDetails, 16, SanitizePdfText(certificateText)), 390)
                        .ShowText(SanitizePdfText(certificateText))
                        .EndText();

                    canvas.BeginText()
                        .SetFontAndSize(customFontDetails, 20)
                        .SetTextMatrix(CenterTextX(customFontDetails, 20, SanitizePdfText(courseName)), 330)
                        .ShowText(SanitizePdfText(courseName))
                        .EndText();

                    canvas.BeginText()
                        .SetFontAndSize(customFontDetails, 20)
                        .SetTextMatrix(CenterTextX(customFontDetails, 20, SanitizePdfText(completiondate)), 250)
                        .ShowText(SanitizePdfText(completiondate))
                        .EndText();

                    canvas.Stroke();

                    pdf.Close();
                    pdfBytes = stream.ToArray();
                }

                return new FileContentResult(pdfBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                // Log error if logger is available
                // Logger.Error(ex.Message, ex);
                return BadRequest($"Error generating certificate: {ex.Message}");
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
    }
}
