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
    public class ProjectCashFlowSummaryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProjectCashFlowSummaryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string projectName, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.ProjectCashFlowSummaries.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(projectName))
            {
                query = query.Where(s => s.Project == projectName);
            }

            if (startDate.HasValue)
            {
                query = query.Where(s => s.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                query = query.Where(s => s.Date <= endDate.Value.Date);
            }

            var summary = await query
                .OrderBy(s => s.Project)
                .ThenBy(s => s.Date)
                .ToListAsync();

            ViewBag.ProjectName = projectName;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            ViewBag.Projects = await _context.ProjectCashFlowSummaries
                .AsNoTracking()
                .Select(s => s.Project)
                .Distinct()
                .OrderBy(p => p)
                .ToListAsync();

            return View(summary);
        }
        public IActionResult Export()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ExportData(string format, string projectName, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.ProjectCashFlowSummaries.AsNoTracking().AsQueryable();
            if (!string.IsNullOrEmpty(projectName))
            {
                query = query.Where(s => s.Project == projectName);
            }
            if (startDate.HasValue)
            {
                query = query.Where(s => s.Date >= startDate.Value.Date);
            }
            if (endDate.HasValue)
            {
                query = query.Where(s => s.Date <= endDate.Value.Date);
            }
            var projectCashFlowSummaries = await query
                .OrderBy(s => s.Project)
                .ThenBy(s => s.Date)
                .ToListAsync();

            byte[] fileContents;
            string fileName;
            string contentType;

            switch (format.ToLower())
            {
                case "excel":
                    fileContents = ExportToExcel(projectCashFlowSummaries);
                    fileName = "ProjectCashFlowSummary.xlsx";
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    break;
                case "pdf":
                    fileContents = ExportToPdf(projectCashFlowSummaries);
                    fileName = "ProjectCashFlowSummary.pdf";
                    contentType = "application/pdf";
                    break;
                case "csv":
                    fileContents = ExportToCsv(projectCashFlowSummaries);
                    fileName = "ProjectCashFlowSummary.csv";
                    contentType = "text/csv";
                    break;
                default:
                    return BadRequest("Invalid format specified");
            }

            return File(fileContents, contentType, fileName);
        }

        private byte[] ExportToExcel(List<ProjectCashFlowSummary> projectCashFlowSummaries)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("ProjectCashFlowSummary");

                // Add report title
                worksheet.Cells[1, 1].Value = "Project Cash Flow Summary Report";
                worksheet.Cells[1, 1, 1, 6].Merge = true;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.Size = 16;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells[1, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow);

                // Add headers
                var headerRow = worksheet.Cells[3, 1, 3, 6];
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                headerRow.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                worksheet.Cells[3, 1].Value = "Project";
                worksheet.Cells[3, 2].Value = "Date";
                worksheet.Cells[3, 3].Value = "Cash In";
                worksheet.Cells[3, 4].Value = "Cash Out";
                worksheet.Cells[3, 5].Value = "Status";
                worksheet.Cells[3, 6].Value = "Delayed Date";

                // Add data
                for (int i = 0; i < projectCashFlowSummaries.Count; i++)
                {
                    var summary = projectCashFlowSummaries[i];
                    int row = i + 4;

                    worksheet.Cells[row, 1].Value = summary.Project;
                    worksheet.Cells[row, 2].Value = summary.Date;
                    worksheet.Cells[row, 3].Value = summary.CashIn;
                    worksheet.Cells[row, 4].Value = summary.CashOut;
                    worksheet.Cells[row, 5].Value = summary.Status;
                    worksheet.Cells[row, 6].Value = summary.DelayedDate;
                }

                worksheet.Cells.AutoFitColumns();
                return package.GetAsByteArray();
            }
        }

        private byte[] ExportToPdf(List<ProjectCashFlowSummary> projectCashFlowSummaries)
        {
            using (var ms = new MemoryStream())
            {
                var document = new Document(PageSize.A4.Rotate(), 10f, 10f, 10f, 0f);
                var writer = PdfWriter.GetInstance(document, ms);

                document.Open();

                // Add report title
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var title = new Paragraph("Project Cash Flow Summary Report", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 20f;
                document.Add(title);

                var table = new PdfPTable(6);
                table.WidthPercentage = 100;

                // Add headers
                string[] headers = { "Project", "Date", "Cash In", "Cash Out", "Status", "Delayed Date" };
                foreach (var header in headers)
                {
                    var cell = new PdfPCell(new Phrase(header, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10)));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.BackgroundColor = new BaseColor(200, 200, 200); // Light gray
                    table.AddCell(cell);
                }

                // Add data
                foreach (var summary in projectCashFlowSummaries)
                {
                    table.AddCell(summary.Project);
                    table.AddCell(summary.Date.ToShortDateString());
                    table.AddCell(summary.CashIn.ToString("C"));
                    table.AddCell(summary.CashOut.ToString("C"));
                    table.AddCell(summary.Status);
                    table.AddCell(summary.DelayedDate?.ToShortDateString() ?? "N/A");
                }

                document.Add(table);
                document.Close();

                return ms.ToArray();
            }
        }

        private byte[] ExportToCsv(List<ProjectCashFlowSummary> projectCashFlowSummaries)
        {
            using (var ms = new MemoryStream())
            using (var writer = new StreamWriter(ms))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                // Write report title
                csv.WriteField("Project Cash Flow Summary Report");
                csv.NextRecord();
                csv.WriteField(string.Empty);  // Empty row after title
                csv.NextRecord();

                // Write headers and data
                csv.WriteRecords(projectCashFlowSummaries.Select(s => new
                {
                    s.Project,
                    Date = s.Date.ToShortDateString(),
                    s.CashIn,
                    s.CashOut,
                    s.Status,
                    DelayedDate = s.DelayedDate?.ToShortDateString() ?? "N/A"
                }));

                writer.Flush();
                return ms.ToArray();
            }
        }
    }
}