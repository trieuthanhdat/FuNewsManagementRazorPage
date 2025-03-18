using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FUNewsManagement.App.Interfaces;
using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using FUNewsManagement.App.Services;

namespace TrieuThanhDatRazorPages.Pages.Admin
{
    public class ReportsModel : PageModel
    {
        private readonly IAccountService _accountService;
        private readonly INewsService _newsService;

        public ReportsModel(IAccountService accountService, INewsService newsService)
        {
            _accountService = accountService;
            _newsService = newsService;
        }

        public int TotalUsers { get; set; }
        public int ActiveNewsArticles { get; set; }
        public int PendingApprovals { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            //TotalUsers = await _accountService.GetTotalUsersAsync();
            //ActiveNewsArticles = await _newsService.GetActiveNewsCountAsync();
            //PendingApprovals = await _newsService.GetPendingApprovalCountAsync();
            return Page();
        }

        // Generate Excel Report
        public async Task<IActionResult> OnPostGenerateExcelReportAsync()
        {
            var users = await _accountService.GetAllAccountsAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Users Report");
            worksheet.Cell(1, 1).Value = "User ID";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "Email";
            worksheet.Cell(1, 4).Value = "Role";

            int row = 2;
            foreach (var user in users)
            {
                worksheet.Cell(row, 1).Value = user.AccountID;
                worksheet.Cell(row, 2).Value = user.AccountName;
                worksheet.Cell(row, 3).Value = user.AccountEmail;
                worksheet.Cell(row, 4).Value = user.AccountRole.ToString();
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "UserReport.xlsx");
        }

        // Generate PDF Report
        public async Task<IActionResult> OnPostGeneratePdfReportAsync()
        {
            var users = await _accountService.GetAllAccountsAsync();

            using var stream = new MemoryStream();
            var document = new Document();
            PdfWriter.GetInstance(document, stream);
            document.Open();

            document.Add(new Paragraph("User Report"));
            document.Add(new Paragraph("Generated on: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            document.Add(new Paragraph("\n"));

            PdfPTable table = new PdfPTable(4);
            table.AddCell("User ID");
            table.AddCell("Name");
            table.AddCell("Email");
            table.AddCell("Role");

            foreach (var user in users)
            {
                table.AddCell(user.AccountID.ToString());
                table.AddCell(user.AccountName);
                table.AddCell(user.AccountEmail);
                table.AddCell(user.AccountRole.ToString());
            }

            document.Add(table);
            document.Close();

            return File(stream.ToArray(), "application/pdf", "UserReport.pdf");
        }
    }
}
