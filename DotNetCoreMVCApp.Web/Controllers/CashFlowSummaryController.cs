using CsvHelper.Configuration;
using DotNetCoreMVCApp.Models;
using DotNetCoreMVCApp.Models.Entities;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;


namespace DotNetCoreMVCApp.Controllers
{
    public class CashFlowSummaryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CashFlowSummaryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            IQueryable<CashFlowSummary> query = _context.CashFlowSummaries;
            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(cfs => cfs.Date >= startDate.Value && cfs.Date <= endDate.Value);
                ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
                ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");
            }
            else
            {
                ViewBag.StartDate = null;
                ViewBag.EndDate = null;
            }
            var summary = await query.OrderBy(cfs => cfs.Date).ToListAsync();
            return View(summary);
        }

        public async Task<IActionResult> Details(string date)
        {
            if (!DateTime.TryParse(date, out DateTime parsedDate))
            {
                return BadRequest("Invalid date format");
            }

            var cashInDetails = await _context.CashInSet
                .Include(ci => ci.CashInInstallments)
                .Where(ci => ci.Date.Date == parsedDate.Date ||
                             (ci.Status == "Delayed" && ci.DelayedDate.HasValue && ci.DelayedDate.Value.Date == parsedDate.Date) ||
                             ci.CashInInstallments.Any(cii => cii.DueDate.Date == parsedDate.Date))
                .ToListAsync();

            var cashOutDetails = await _context.CashOutSet
                .Include(co => co.CashOutInstallments)
                .Where(co => co.Date.Date == parsedDate.Date ||
                             (co.Status == "Delayed" && co.DelayedDate.HasValue && co.DelayedDate.Value.Date == parsedDate.Date) ||
                             co.CashOutInstallments.Any(coi => coi.DueDate.Date == parsedDate.Date))
                .ToListAsync();

            var viewModel = new CashFlowDetailsViewModel
            {
                Date = parsedDate,
                CashInTransactions = cashInDetails,
                CashOutTransactions = cashOutDetails
            };

            return View(viewModel);
        }
        public IActionResult Export()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ExportData(string format, DateTime? startDate, DateTime? endDate)
        {
            IQueryable<CashFlowSummary> query = _context.CashFlowSummaries;
            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(cfs => cfs.Date >= startDate.Value && cfs.Date <= endDate.Value);
            }
            var cashFlowSummaries = await query.OrderBy(cfs => cfs.Date).ToListAsync();

            byte[] fileContents;
            string fileName;
            string contentType;

            switch (format.ToLower())
            {
                case "excel":
                    fileContents = ExportToExcel(cashFlowSummaries);
                    fileName = "CashFlowSummary.xlsx";
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    break;
                case "pdf":
                    fileContents = ExportToPdf(cashFlowSummaries);
                    fileName = "CashFlowSummary.pdf";
                    contentType = "application/pdf";
                    break;
                case "csv":
                    fileContents = ExportToCsv(cashFlowSummaries);
                    fileName = "CashFlowSummary.csv";
                    contentType = "text/csv";
                    break;
                default:
                    return BadRequest("Invalid format specified");
            }

            return File(fileContents, contentType, fileName);
        }

        private byte[] ExportToExcel(List<CashFlowSummary> cashFlowSummaries)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("CashFlowSummary");

                // Add report title
                worksheet.Cells[1, 1].Value = "Cash Flow Summary Report";
                worksheet.Cells[1, 1, 1, 4].Merge = true;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.Size = 16;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells[1, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow);

                // Add headers
                var headerRow = worksheet.Cells[3, 1, 3, 4];
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                headerRow.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                worksheet.Cells[3, 1].Value = "Date";
                worksheet.Cells[3, 2].Value = "Cash In";
                worksheet.Cells[3, 3].Value = "Cash Out";
                worksheet.Cells[3, 4].Value = "Balance";

                // Add data
                for (int i = 0; i < cashFlowSummaries.Count; i++)
                {
                    var summary = cashFlowSummaries[i];
                    int row = i + 4;

                    worksheet.Cells[row, 1].Value = summary.Date;
                    worksheet.Cells[row, 2].Value = summary.CashIn;
                    worksheet.Cells[row, 3].Value = summary.CashOut;
                    worksheet.Cells[row, 4].Value = summary.Balance;
                }

                worksheet.Cells.AutoFitColumns();
                return package.GetAsByteArray();
            }
        }

        private byte[] ExportToPdf(List<CashFlowSummary> cashFlowSummaries)
        {
            using (var ms = new MemoryStream())
            {
                var document = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
                var writer = PdfWriter.GetInstance(document, ms);

                document.Open();

                // Add report title
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var title = new Paragraph("Cash Flow Summary Report", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 20f;
                document.Add(title);

                var table = new PdfPTable(4);
                table.WidthPercentage = 100;

                // Add headers
                string[] headers = { "Date", "Cash In", "Cash Out", "Balance" };
                foreach (var header in headers)
                {
                    var cell = new PdfPCell(new Phrase(header, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10)));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.BackgroundColor = new BaseColor(200, 200, 200); // Light gray
                    table.AddCell(cell);
                }

                // Add data
                foreach (var summary in cashFlowSummaries)
                {
                    table.AddCell(summary.Date.ToShortDateString());
                    table.AddCell(summary.CashIn.ToString("C"));
                    table.AddCell(summary.CashOut.ToString("C"));
                    table.AddCell(summary.Balance.ToString("C"));
                }

                document.Add(table);
                document.Close();

                return ms.ToArray();
            }
        }

        private byte[] ExportToCsv(List<CashFlowSummary> cashFlowSummaries)
        {
            using (var ms = new MemoryStream())
            using (var writer = new StreamWriter(ms))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                // Write report title
                csv.WriteField("Cash Flow Summary Report");
                csv.NextRecord();
                csv.WriteField(string.Empty);  // Empty row after title
                csv.NextRecord();

                // Write headers and data
                csv.WriteRecords(cashFlowSummaries.Select(s => new
                {
                    Date = s.Date.ToShortDateString(),
                    CashIn = s.CashIn,
                    CashOut = s.CashOut,
                    Balance = s.Balance
                }));

                writer.Flush();
                return ms.ToArray();
            }
        }
    }
}
    