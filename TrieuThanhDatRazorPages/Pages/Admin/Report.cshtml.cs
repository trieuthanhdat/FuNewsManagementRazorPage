using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FUNewsManagement.App.Interfaces;
using FUNewsManagement.Domain.Entities;
using FUNewsManagement.Domain.Shared;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ClosedXML.Excel;

namespace TrieuThanhDatRazorPages.Pages.Admin
{

    [Authorize(Roles = "Admin")]
    public class ReportModel : PageModel
    {
        private readonly INewsService _newsArticleService;

        public ReportModel(INewsService newsArticleService)
        {
            _newsArticleService = newsArticleService;
        }

        public List<NewsArticle> ReportData { get; set; } = new List<NewsArticle>();

        public async Task OnGetAsync()
        {
            // Default: Get last 7 days report
            DateTime startDate = DateTime.UtcNow.AddDays(-7);
            DateTime endDate = DateTime.UtcNow;

            ReportData = (await _newsArticleService.GetNewsByDateRangeAsync(startDate, endDate)).ToList();
        }
        public async Task<IActionResult> OnGetGetReportAsync(DateTime startDate, DateTime endDate, string? search)
        {
            var articles = await _newsArticleService.GetNewsByDateRangeAsync(startDate, endDate);

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                articles = articles.Where(a =>
                    a.NewsTitle.ToLower().Contains(search) ||
                    a.Headline.ToLower().Contains(search) ||
                    (a.CreatedBy?.AccountName ?? "").ToLower().Contains(search)
                ).ToList();
            }

            if (!articles.Any())
            {
                return new JsonResult(new { success = false, message = "No data found" });
            }

            var reportData = articles.Select(a => new
            {
                newsArticleId = a.NewsArticleId,
                newsTitle = a.NewsTitle,
                headline = a.Headline,
                createdDate = a.CreatedDate?.ToString("yyyy-MM-dd"),
                createdBy = a.CreatedBy?.AccountName
            });

            return new JsonResult(new { success = true, data = reportData });
        }
        public async Task<IActionResult> OnGetExportExcelAsync(DateTime startDate, DateTime endDate)
        {
            var articles = await _newsArticleService.GetNewsByDateRangeAsync(startDate, endDate);

            if (!articles.Any())
            {
                return new JsonResult(new { success = false, message = "No data available for this period" });
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("News Report");
                var currentRow = 1;

                // Headers
                worksheet.Cell(currentRow, 1).Value = "ID";
                worksheet.Cell(currentRow, 2).Value = "Title";
                worksheet.Cell(currentRow, 3).Value = "Headline";
                worksheet.Cell(currentRow, 4).Value = "Created Date";
                worksheet.Cell(currentRow, 5).Value = "Author";

                // Styling Headers
                worksheet.Range("A1:E1").Style.Font.Bold = true;
                worksheet.Range("A1:E1").Style.Fill.BackgroundColor = XLColor.LightGray;

                // Data Rows
                foreach (var article in articles)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = article.NewsArticleId;
                    worksheet.Cell(currentRow, 2).Value = article.NewsTitle;
                    worksheet.Cell(currentRow, 3).Value = article.Headline;
                    worksheet.Cell(currentRow, 4).Value = article.CreatedDate?.ToString("yyyy-MM-dd");
                    worksheet.Cell(currentRow, 5).Value = article.CreatedBy?.AccountName ?? "Unknown";
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                // Save as Memory Stream
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "NewsReport.xlsx");
                }
            }
        }
        public async Task<IActionResult> OnGetExportPdfAsync(DateTime startDate, DateTime endDate)
        {
            var articles = await _newsArticleService.GetNewsByDateRangeAsync(startDate, endDate);

            if (!articles.Any())
            {
                return new JsonResult(new { success = false, message = "No data available for this period" });
            }

            using (var stream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                document.Open();

                // Title
                Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                Paragraph title = new Paragraph("News Report", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);
                document.Add(new Paragraph("\n"));

                // Table
                PdfPTable table = new PdfPTable(5) { WidthPercentage = 100 };
                table.SetWidths(new float[] { 10f, 30f, 30f, 20f, 20f });

                // Table Header
                string[] headers = { "ID", "Title", "Headline", "Created Date", "Author" };
                foreach (string header in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12)))
                    {
                        BackgroundColor = new BaseColor(220, 220, 220),
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };
                    table.AddCell(cell);
                }

                // Data Rows
                foreach (var article in articles)
                {
                    table.AddCell(article.NewsArticleId.ToString());
                    table.AddCell(article.NewsTitle);
                    table.AddCell(article.Headline);
                    table.AddCell(article.CreatedDate?.ToString("yyyy-MM-dd"));
                    table.AddCell(article.CreatedBy?.AccountName ?? "Unknown");
                }

                document.Add(table);
                document.Close();

                return File(stream.ToArray(), "application/pdf", "NewsReport.pdf");
            }
        }
    }

}
