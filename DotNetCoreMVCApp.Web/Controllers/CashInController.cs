using CsvHelper.Configuration;
using DotNetCoreMVCApp.Models;
using DotNetCoreMVCApp.Models.Entities;
using DotNetCoreMVCApp.Models.Repository;
using DotNetCoreMVCApp.Models.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using OfficeOpenXml.Style;
using System.Drawing;
using iTextSharp.text;
using iTextSharp.text.pdf;
using CsvHelper;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;


namespace DotNetCoreMVCApp.Web.Controllers
{
    [Authorize]
    public class CashInController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CashInController> _logger;

        public CashInController(ApplicationDbContext context, ILogger<CashInController> logger)
        {
            _context = context;
            _logger = logger;
        }
        public IActionResult Export()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ExportData(string format)
        {
            var cashins = await _context.CashInSet.Include(c => c.CashInInstallments).ToListAsync();

            byte[] fileContents;
            string fileName;
            string contentType;

            switch (format.ToLower())
            {
                case "excel":
                    fileContents = ExportToExcel(cashins);
                    fileName = "CashIns.xlsx";
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    break;
                case "pdf":
                    fileContents = ExportToPdf(cashins);
                    fileName = "CashIns.pdf";
                    contentType = "application/pdf";
                    break;
                case "csv":
                    fileContents = ExportToCsv(cashins);
                    fileName = "CashIns.csv";
                    contentType = "text/csv";
                    break;
                default:
                    return BadRequest("Invalid format specified");
            }

            return File(fileContents, contentType, fileName);
        }

        private byte[] ExportToExcel(List<CashIn> cashins)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("CashIns");

                // Add report title
                worksheet.Cells[1, 1].Value = "CashIn Data Report";
                worksheet.Cells[1, 1, 1, 10].Merge = true;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.Size = 16;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells[1, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow);

                // Add headers
                var headerRow = worksheet.Cells[3, 1, 3, 10];
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                headerRow.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                worksheet.Cells[3, 1].Value = "Customer Name";
                worksheet.Cells[3, 2].Value = "Amount Due";
                worksheet.Cells[3, 3].Value = "Date";
                worksheet.Cells[3, 4].Value = "Status";
                worksheet.Cells[3, 5].Value = "Delayed Date";
                worksheet.Cells[3, 6].Value = "Project";
                worksheet.Cells[3, 7].Value = "Cost Center";
                worksheet.Cells[3, 8].Value = "Remark";
                worksheet.Cells[3, 9].Value = "Installment Amounts";
                worksheet.Cells[3, 10].Value = "Installment Due Dates";

                // Add data
                for (int i = 0; i < cashins.Count; i++)
                {
                    var cashin = cashins[i];
                    int row = i + 4;

                    worksheet.Cells[row, 1].Value = cashin.CustomerName;
                    worksheet.Cells[row, 2].Value = cashin.AmountDue;
                    worksheet.Cells[row, 3].Value = cashin.Date;
                    worksheet.Cells[row, 4].Value = cashin.Status;
                    worksheet.Cells[row, 5].Value = cashin.DelayedDate;
                    worksheet.Cells[row, 6].Value = cashin.ProjectName;
                    worksheet.Cells[row, 7].Value = cashin.CostCenter;
                    worksheet.Cells[row, 8].Value = cashin.Remark;
                    worksheet.Cells[row, 9].Value = string.Join(", ", cashin.CashInInstallments.Select(c => c.Amount));
                    worksheet.Cells[row, 10].Value = string.Join(", ", cashin.CashInInstallments.Select(c => c.DueDate.ToShortDateString()));
                }

                worksheet.Cells.AutoFitColumns();
                return package.GetAsByteArray();
            }
        }

        private byte[] ExportToPdf(List<CashIn> cashins)
        {
            using (var ms = new MemoryStream())
            {
                var document = new Document(PageSize.A4.Rotate(), 10f, 10f, 10f, 0f);
                var writer = PdfWriter.GetInstance(document, ms);

                document.Open();

                // Add report title
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var title = new Paragraph("CashIn Data Report", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 20f;
                document.Add(title);

                var table = new PdfPTable(10);
                table.WidthPercentage = 100;

                // Add headers
                string[] headers = { "Customer Name", "Amount Due", "Date", "Status", "Delayed Date", "Project", "Cost Center", "Remark", "Installment Amounts", "Installment Due Dates" };
                foreach (var header in headers)
                {
                    var cell = new PdfPCell(new Phrase(header, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10)));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.BackgroundColor = new BaseColor(200, 200, 200); // Light gray
                    table.AddCell(cell);
                }

                // Add data
                foreach (var cashin in cashins)
                {
                    table.AddCell(cashin.CustomerName);
                    table.AddCell(cashin.AmountDue.ToString());
                    table.AddCell(cashin.Date.ToShortDateString());
                    table.AddCell(cashin.Status);
                    table.AddCell(cashin.DelayedDate?.ToShortDateString() ?? "N/A");
                    table.AddCell(cashin.ProjectName);
                    table.AddCell(cashin.CostCenter ?? "N/A");
                    table.AddCell(cashin.Remark ?? "N/A");
                    table.AddCell(string.Join(", ", cashin.CashInInstallments.Select(c => c.Amount)));
                    table.AddCell(string.Join(", ", cashin.CashInInstallments.Select(c => c.DueDate.ToShortDateString())));
                }

                document.Add(table);
                document.Close();

                return ms.ToArray();
            }
        }

        private byte[] ExportToCsv(List<CashIn> cashins)
        {
            using (var ms = new MemoryStream())
            using (var writer = new StreamWriter(ms))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                // Write report title
                csv.WriteField("CashIn Data Report");
                csv.NextRecord();
                csv.WriteField(string.Empty);  // Empty row after title
                csv.NextRecord();

                // Write headers and data
                csv.WriteRecords(cashins.Select(c => new
                {
                    CustomerName = c.CustomerName,
                    AmountDue = c.AmountDue,
                    Date = c.Date,
                    Status = c.Status,
                    DelayedDate = c.DelayedDate?.ToShortDateString() ?? "N/A",
                    ProjectName = c.ProjectName,
                    CostCenter = c.CostCenter ?? "N/A",
                    Remark = c.Remark ?? "N/A",
                    InstallmentAmounts = string.Join(", ", c.CashInInstallments.Select(i => i.Amount)),
                    InstallmentDueDates = string.Join(", ", c.CashInInstallments.Select(i => i.DueDate.ToShortDateString()))
                }));

                writer.Flush();
                return ms.ToArray();
            }
        }
        public IActionResult Import()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Please select a file to import.");
                return View();
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (fileExtension != ".csv" && fileExtension != ".xlsx")
            {
                ModelState.AddModelError("", "Please upload a CSV or Excel file.");
                return View();
            }

            List<CashIn> cashIns = new List<CashIn>();
            string createdBy = User.Identity?.Name ?? "System";

            try
            {
                if (fileExtension == ".csv")
                {
                    using (var reader = new StreamReader(file.OpenReadStream()))
                    {
                        // Skip the header
                        reader.ReadLine();

                        int lineNumber = 1;
                        while (!reader.EndOfStream)
                        {
                            lineNumber++;
                            var line = reader.ReadLine();
                            var values = line.Split(',');

                            if (values.Length >= 7)
                            {
                                try
                                {
                                    cashIns.Add(new CashIn
                                    {
                                        CustomerName = values[0].Trim(),
                                        AmountDue = decimal.Parse(values[1].Trim()),
                                        Date = DateTime.Parse(values[2].Trim()),
                                        Status = values[3].Trim(),
                                        ProjectName = values[4].Trim(),
                                        CostCenter = values[5].Trim(),
                                        Remark = values[6].Trim(),
                                        CreatedBy = createdBy,
                                        CreatedOn = DateTime.Now,
                                        IsDeleted = false
                                    });
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, $"Error parsing CSV line {lineNumber}: {line}");
                                    ModelState.AddModelError("", $"Error on CSV line {lineNumber}: {ex.Message}");
                                }
                            }
                            else
                            {
                                _logger.LogWarning($"Skipping CSV line {lineNumber} due to insufficient columns: {line}");
                                ModelState.AddModelError("", $"CSV line {lineNumber} has insufficient columns and was skipped.");
                            }
                        }
                    }
                }
                else // Excel file
                {
                    using (var stream = new MemoryStream())
                    {
                        await file.CopyToAsync(stream);
                        using (var package = new ExcelPackage(stream))
                        {
                            ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                            var rowCount = worksheet.Dimension.Rows;

                            for (int row = 2; row <= rowCount; row++)
                            {
                                try
                                {
                                    cashIns.Add(new CashIn
                                    {
                                        CustomerName = worksheet.Cells[row, 1].Value?.ToString().Trim(),
                                        AmountDue = decimal.Parse(worksheet.Cells[row, 2].Value?.ToString() ?? "0"),
                                        Date = DateTime.Parse(worksheet.Cells[row, 3].Value?.ToString() ?? DateTime.Now.ToString()),
                                        Status = worksheet.Cells[row, 4].Value?.ToString().Trim(),
                                        ProjectName = worksheet.Cells[row, 5].Value?.ToString().Trim(),
                                        CostCenter = worksheet.Cells[row, 6].Value?.ToString().Trim(),
                                        Remark = worksheet.Cells[row, 7].Value?.ToString().Trim(),
                                        CreatedBy = createdBy,
                                        CreatedOn = DateTime.Now,
                                        IsDeleted = false
                                    });
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, $"Error parsing Excel row {row}");
                                    ModelState.AddModelError("", $"Error on Excel row {row}: {ex.Message}");
                                }
                            }
                        }
                    }
                }

                if (cashIns.Count == 0)
                {
                    ModelState.AddModelError("", "No valid data was found in the file.");
                    return View();
                }

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var projectNames = cashIns.Select(c => c.ProjectName).Distinct().ToList();
                        var existingProjects = await _context.ProjectDatabaseSet
                            .Where(p => projectNames.Contains(p.ProjectName))
                            .ToDictionaryAsync(p => p.ProjectName, p => p.Id);

                        foreach (var cashIn in cashIns)
                        {
                            // Handle Customer
                            var customer = await _context.CustomerSet.FirstOrDefaultAsync(c => c.Name == cashIn.CustomerName);
                            if (customer == null)
                            {
                                customer = new Customer
                                {
                                    Name = cashIn.CustomerName,
                                    CreatedBy = createdBy,
                                    CreatedOn = DateTime.Now
                                };
                                _context.CustomerSet.Add(customer);
                                await _context.SaveChangesAsync();
                            }
                            cashIn.CustomerId = customer.Id;

                            // Handle Project
                            if (!existingProjects.TryGetValue(cashIn.ProjectName, out int projectId))
                            {
                                var newProject = new ProjectDatabase
                                {
                                    ProjectName = cashIn.ProjectName,
                                    CreatedBy = createdBy,
                                    CreatedOn = DateTime.Now
                                    // Add other required fields for ProjectDatabase
                                };
                                _context.ProjectDatabaseSet.Add(newProject);
                                await _context.SaveChangesAsync();
                                projectId = newProject.Id;
                                existingProjects[cashIn.ProjectName] = projectId;
                            }
                            cashIn.ProjectId = projectId;

                            _context.CashInSet.Add(cashIn);
                        }
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        TempData["SuccessMessage"] = $"{cashIns.Count} records imported successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateException dbEx)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(dbEx, "Database update error occurred while saving imported CashIn data");
                        ModelState.AddModelError("", $"A database error occurred while saving the data: {dbEx.InnerException?.Message ?? dbEx.Message}");
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Error occurred while saving imported CashIn data");
                        ModelState.AddModelError("", $"An error occurred while saving the data: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the imported file");
                ModelState.AddModelError("", $"An error occurred while processing the file: {ex.Message}");
            }

            return View();
        }

        public IActionResult DownloadTemplate()
        {
            var stream = new MemoryStream();
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets.Add("CashIn Template");

                // Add headers
                worksheet.Cells[1, 1].Value = "Customer Name";
                worksheet.Cells[1, 2].Value = "Amount Due";
                worksheet.Cells[1, 3].Value = "Date";
                worksheet.Cells[1, 4].Value = "Status";
                worksheet.Cells[1, 5].Value = "Project Name";
                worksheet.Cells[1, 6].Value = "Cost Center";
                worksheet.Cells[1, 7].Value = "Remark";

                // Style the header row
                using (var range = worksheet.Cells[1, 1, 1, 7])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                }

                // Add a sample row (optional)
                worksheet.Cells[2, 1].Value = "Sample Customer";
                worksheet.Cells[2, 2].Value = 1000.00;
                worksheet.Cells[2, 3].Value = DateTime.Now.ToString("yyyy-MM-dd");
                worksheet.Cells[2, 4].Value = "Scheduled";
                worksheet.Cells[2, 5].Value = "Sample Project";
                worksheet.Cells[2, 6].Value = "General Chemicals";
                worksheet.Cells[2, 7].Value = "Sample remark";

                worksheet.Cells.AutoFitColumns();
                package.Save();
            }
            stream.Position = 0;
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CashInImportTemplate.xlsx");
        }
        public async Task<IActionResult> Index()
        {
            try
            {
                var cashins = await _context.CashInSet.Include(c => c.CashInInstallments).ToListAsync();

                return View(cashins);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching CashIn records");
                return StatusCode(500, "An error occurred while fetching CashIn records. Please try again later.");
            }
        }

        public async Task<IActionResult> Create()
        {
            CashInCreateViewModel model = new();
            model.Date = DateTime.Now;
            model.Customerlist = await GetCustomerList();
            model.ProjectDatabaselist = await GetProjectDatabaseList();

            return View(model);


        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CashInViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customerName = await _context.CustomerSet
                .Where(x => x.Id == model.CustomerId).Select(c => c.Name).FirstOrDefaultAsync();
            var projectName = await _context.ProjectDatabaseSet.Where(p => p.Id == model.ProjectId).
                Select(x => x.ProjectName).FirstOrDefaultAsync();
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var cashin = new CashIn
                {
                    CustomerId = model.CustomerId,
                    CustomerName = customerName,
                    AmountDue = model.AmountDue,
                    Date = model.Date,
                    Status = model.Status,
                    DelayedDate = model.DelayedDate,
                    ProjectId = model.ProjectId,
                    ProjectName = projectName,
                    CostCenter = model.CostCenter,
                    Remark = model.Remark,
                    PayAsInstallment = model.PayAsInstallment,
                    CreatedBy = User.Identity?.Name,
                    CreatedOn = DateTime.Now,
                    IsDeleted = false
                };

                _context.CashInSet.Add(cashin);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"CashIn created: {cashin.Id}");

                if (model.CashInInstallments != null && model.CashInInstallments.Any())
                {
                    var installmentEntities = model.CashInInstallments.Select(installment => new CashInInstallment
                    {
                        CashInId = cashin.Id,
                        NumberOfInstallment = installment.NumberOfInstallment,
                        Amount = installment.Amount,
                        DueDate = installment.DueDate
                    }).ToList();

                    _context.CashInInstallmentSet.AddRange(installmentEntities);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Installment details added: {cashin.Id}");
                }

                await transaction.CommitAsync();

                return Ok(new { message = "CashIn created successfully" });
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Database update error occurred while saving CashIn");
                return StatusCode(500, new { message = "A database error occurred while saving the CashIn.", error = ex.InnerException?.Message ?? ex.Message });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while saving CashIn");
                return StatusCode(500, new { message = "An error occurred while saving the CashIn.", error = ex.Message });
            }
        }
        // GET: CashIn/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cashin = await _context.CashInSet
                .Include(c => c.CashInInstallments)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (cashin == null)
            {
                return NotFound();
            }

            var viewModel = new CashInViewModel
            {
                Id = cashin.Id,
                CustomerId = cashin.CustomerId,
                CustomerName = cashin.CustomerName,
                AmountDue = cashin.AmountDue,
                Date = cashin.Date,
                Status = cashin.Status,
                DelayedDate = cashin.DelayedDate,
                ProjectId = cashin.ProjectId,
                CostCenter = cashin.CostCenter,
                Remark = cashin.Remark,

                CashInInstallments = cashin.CashInInstallments.Select(i => new CashInInstallmentViewModel
                {
                    NumberOfInstallment = i.NumberOfInstallment,
                    Amount = i.Amount,
                    DueDate = i.DueDate
                }).ToList()
            };

            await PopulateViewModelLists(viewModel);

            return View(viewModel);
        }

        // POST: CashIn/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CashInViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var cashin = await _context.CashInSet
                        .Include(c => c.CashInInstallments)
                        .FirstOrDefaultAsync(m => m.Id == id);

                    if (cashin == null)
                    {
                        return NotFound();
                    }

                    await UpdateCashInFromViewModel(cashin, viewModel);

                    _context.Update(cashin);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "CashIn updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CashInExists(viewModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ModelState.AddModelError("", "An error occurred while saving. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                }
            }

            await PopulateViewModelLists(viewModel);
            return View(viewModel);
        }

        private async Task PopulateViewModelLists(CashInViewModel viewModel)
        {
            viewModel.CustomerList = await GetCustomerList();
            viewModel.ProjectDatabaseList = await GetProjectDatabaseList();
            viewModel.StatusList = GetStatusList();
            viewModel.CostCenterList = GetCostCenterList();
        }

        

        private List<SelectListItem> GetStatusList()
        {
            return new List<SelectListItem>
        {
            new SelectListItem { Value = "Received", Text = "Received" },
            new SelectListItem { Value = "Scheduled", Text = "Scheduled" },
            new SelectListItem { Value = "Delayed", Text = "Delayed" }
        };
        }

        private List<SelectListItem> GetCostCenterList()
        {
            return new List<SelectListItem>
        {
            new SelectListItem { Value = "General Chemicals", Text = "General Chemicals" },
            new SelectListItem { Value = "Catalyst", Text = "Catalyst" },
            new SelectListItem { Value = "Overhead", Text = "Overhead" },
            new SelectListItem { Value = "Oil and Gas", Text = "Oil and Gas" }
        };
        }

        private async Task UpdateCashInFromViewModel(CashIn cashin, CashInViewModel viewModel)
        {
            cashin.CustomerId = viewModel.CustomerId;
            cashin.CustomerName = await _context.CustomerSet
                .Where(c => c.Id == viewModel.CustomerId)
                .Select(c => c.Name)
                .FirstOrDefaultAsync();
            cashin.AmountDue = viewModel.AmountDue;
            cashin.Date = viewModel.Date;
            cashin.Status = viewModel.Status;
            cashin.DelayedDate = viewModel.DelayedDate;
            cashin.ProjectId = viewModel.ProjectId;
            cashin.ProjectName = await _context.ProjectDatabaseSet
                .Where(p => p.Id == viewModel.ProjectId)
                .Select(p => p.ProjectName)
                .FirstOrDefaultAsync();
            cashin.CostCenter = viewModel.CostCenter;
            cashin.Remark = viewModel.Remark;

            // Remove existing installments
            _context.CashInInstallmentSet.RemoveRange(cashin.CashInInstallments);

            // Add updated installments
            if (viewModel.CashInInstallments != null && viewModel.CashInInstallments.Any())
            {
                cashin.CashInInstallments = viewModel.CashInInstallments.Select(i => new CashInInstallment
                {
                    CashInId = cashin.Id,
                    NumberOfInstallment = i.NumberOfInstallment,
                    Amount = i.Amount,
                    DueDate = i.DueDate
                }).ToList();
            }
        }

        private bool CashInExists(int id)
        {
            return _context.CashInSet.Any(e => e.Id == id);
        }


        // GET: CashIn/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cashin = await _context.CashInSet
                .Include(c => c.CashInInstallments)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (cashin == null)
            {
                return NotFound();
            }

            return View(cashin);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cashin = await _context.CashInSet
                .Include(c => c.CashInInstallments)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (cashin == null)
            {
                return NotFound();
            }

            try
            {
                // Remove associated installments
                _context.CashInInstallmentSet.RemoveRange(cashin.CashInInstallments);

                // Remove the CashIn entity
                _context.CashInSet.Remove(cashin);
                await _context.SaveChangesAsync();

                // Return a JSON result with success message
                return Json(new { success = true, message = "CashIn deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting CashIn with ID {id}");
                // Return a JSON result with error message
                return Json(new { success = false, message = "An error occurred while deleting the CashIn. Please try again." });
            }
        }



        [NonAction]
        private async Task<List<SelectListItem>> GetCustomerList()
        {
            var customers = await _context.CustomerSet.ToListAsync();
            return customers.Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString()
            }).ToList();
        }

        [NonAction]
        private async Task<List<SelectListItem>> GetProjectDatabaseList()
        {
            var projects = await _context.ProjectDatabaseSet.ToListAsync();
            return projects.Select(p => new SelectListItem
            {
                Text = p.ProjectName,
                Value = p.Id.ToString()
            }).ToList();
        }
        
    }
}


