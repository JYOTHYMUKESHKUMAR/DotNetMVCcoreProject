using CsvHelper.Configuration;
using DotNetCoreMVCApp.Models;
using DotNetCoreMVCApp.Models.Entities;
using DotNetCoreMVCApp.Models.Repository;
using DotNetCoreMVCApp.Models.Web;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml.Style;
using System.Drawing;

namespace DotNetCoreMVCApp.Web.Controllers
{
    public class CashOutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CashOutController> _logger;

        public CashOutController(ApplicationDbContext context, ILogger<CashOutController> logger)
        {
            _context = context;
            _logger = logger;
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

            List<CashOut> cashOuts = new List<CashOut>();
            string createdBy = User.Identity?.Name ?? "System";

            try
            {
                if (fileExtension == ".csv")
                {
                    using (var reader = new StreamReader(file.OpenReadStream()))
                    using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                    {
                        cashOuts = csv.GetRecords<CashOut>().ToList();
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
                                cashOuts.Add(new CashOut
                                {
                                    SupplierName = worksheet.Cells[row, 1].Value?.ToString().Trim(),
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
                        }
                    }
                }

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var projectNames = cashOuts.Where(c => !string.IsNullOrEmpty(c.ProjectName))
                                                   .Select(c => c.ProjectName)
                                                   .Distinct()
                                                   .ToList();

                        var existingProjects = await _context.ProjectDatabaseSet
                            .Where(p => projectNames.Contains(p.ProjectName))
                            .ToDictionaryAsync(p => p.ProjectName, p => p.Id);

                        foreach (var cashOut in cashOuts)
                        {
                            if (string.IsNullOrEmpty(cashOut.SupplierName))
                            {
                                ModelState.AddModelError("", $"Supplier name is required for CashOut with Amount Due: {cashOut.AmountDue}");
                                continue;
                            }

                            var supplier = await _context.SupplierSet.FirstOrDefaultAsync(s => s.Name == cashOut.SupplierName);
                            if (supplier == null)
                            {
                                supplier = new Supplier
                                {
                                    Name = cashOut.SupplierName,
                                    CreatedBy = createdBy,
                                    CreatedOn = DateTime.Now
                                };
                                _context.SupplierSet.Add(supplier);
                                await _context.SaveChangesAsync();
                            }
                            cashOut.SupplierId = supplier.Id;

                            if (string.IsNullOrEmpty(cashOut.ProjectName))
                            {
                                ModelState.AddModelError("", $"Project name is required for CashOut with Supplier: {cashOut.SupplierName}");
                                continue;
                            }

                            if (!existingProjects.TryGetValue(cashOut.ProjectName, out int projectId))
                            {
                                var newProject = new ProjectDatabase
                                {
                                    ProjectName = cashOut.ProjectName,
                                    CreatedBy = createdBy,
                                    CreatedOn = DateTime.Now
                                };
                                _context.ProjectDatabaseSet.Add(newProject);
                                await _context.SaveChangesAsync();
                                projectId = newProject.Id;
                                existingProjects[cashOut.ProjectName] = projectId;
                            }
                            cashOut.ProjectId = projectId;

                            _context.CashOutSet.Add(cashOut);
                        }

                        if (ModelState.IsValid)
                        {
                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();
                            TempData["SuccessMessage"] = $"{cashOuts.Count} records imported successfully!";
                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            await transaction.RollbackAsync();
                            return View();
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Error occurred while saving imported CashOut data");
                        ModelState.AddModelError("", $"An error occurred while saving the data: {ex.Message}");
                        return View();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the imported file");
                ModelState.AddModelError("", $"An error occurred while processing the file: {ex.Message}");
                return View();
            }
        }
        public IActionResult DownloadTemplate()
        {
            var stream = new MemoryStream();
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets.Add("CashOut Template");

                // Add headers
                worksheet.Cells[1, 1].Value = "Supplier Name";
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
                worksheet.Cells[2, 1].Value = "Sample Supplier";
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
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CashOutImportTemplate.xlsx");
        }
        public IActionResult Export()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ExportData(string format)
        {
            var cashouts = await _context.CashOutSet.Include(c => c.CashOutInstallments).ToListAsync();

            byte[] fileContents;
            string fileName;
            string contentType;

            switch (format.ToLower())
            {
                case "excel":
                    fileContents = ExportToExcel(cashouts);
                    fileName = "CashOuts.xlsx";
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    break;
                case "pdf":
                    fileContents = ExportToPdf(cashouts);
                    fileName = "CashOuts.pdf";
                    contentType = "application/pdf";
                    break;
                case "csv":
                    fileContents = ExportToCsv(cashouts);
                    fileName = "CashOuts.csv";
                    contentType = "text/csv";
                    break;
                default:
                    return BadRequest("Invalid format specified");
            }

            return File(fileContents, contentType, fileName);
        }

        private byte[] ExportToExcel(List<CashOut> cashouts)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("CashOuts");

                worksheet.Cells[1, 1].Value = "CashOut Data Report";
                worksheet.Cells[1, 1, 1, 10].Merge = true;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.Size = 16;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                var headerRow = worksheet.Cells[3, 1, 3, 10];
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                headerRow.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                worksheet.Cells[3, 1].Value = "Supplier Name";
                worksheet.Cells[3, 2].Value = "Amount Due";
                worksheet.Cells[3, 3].Value = "Date";
                worksheet.Cells[3, 4].Value = "Status";
                worksheet.Cells[3, 5].Value = "Delayed Date";
                worksheet.Cells[3, 6].Value = "Project";
                worksheet.Cells[3, 7].Value = "Cost Center";
                worksheet.Cells[3, 8].Value = "Remark";
                worksheet.Cells[3, 9].Value = "Installment Amounts";
                worksheet.Cells[3, 10].Value = "Installment Due Dates";

                for (int i = 0; i < cashouts.Count; i++)
                {
                    var cashout = cashouts[i];
                    int row = i + 4;

                    worksheet.Cells[row, 1].Value = cashout.SupplierName;
                    worksheet.Cells[row, 2].Value = cashout.AmountDue;
                    worksheet.Cells[row, 3].Value = cashout.Date;
                    worksheet.Cells[row, 4].Value = cashout.Status;
                    worksheet.Cells[row, 5].Value = cashout.DelayedDate;
                    worksheet.Cells[row, 6].Value = cashout.ProjectName;
                    worksheet.Cells[row, 7].Value = cashout.CostCenter;
                    worksheet.Cells[row, 8].Value = cashout.Remark;
                    worksheet.Cells[row, 9].Value = string.Join(", ", cashout.CashOutInstallments.Select(c => c.Amount));
                    worksheet.Cells[row, 10].Value = string.Join(", ", cashout.CashOutInstallments.Select(c => c.DueDate.ToShortDateString()));
                }

                worksheet.Cells.AutoFitColumns();
                return package.GetAsByteArray();
            }
        }

        private byte[] ExportToPdf(List<CashOut> cashouts)
        {
            using (var ms = new MemoryStream())
            {
                var document = new Document(PageSize.A4.Rotate(), 10f, 10f, 10f, 0f);
                var writer = PdfWriter.GetInstance(document, ms);

                document.Open();

                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var title = new Paragraph("CashOut Data Report", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 20f;
                document.Add(title);

                var table = new PdfPTable(10);
                table.WidthPercentage = 100;

                string[] headers = { "Supplier Name", "Amount Due", "Date", "Status", "Delayed Date", "Project", "Cost Center", "Remark", "Installment Amounts", "Installment Due Dates" };
                foreach (var header in headers)
                {
                    var cell = new PdfPCell(new Phrase(header, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10)));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.BackgroundColor = new BaseColor(200, 200, 200);
                    table.AddCell(cell);
                }

                foreach (var cashout in cashouts)
                {
                    table.AddCell(cashout.SupplierName);
                    table.AddCell(cashout.AmountDue.ToString());
                    table.AddCell(cashout.Date.ToShortDateString());
                    table.AddCell(cashout.Status);
                    table.AddCell(cashout.DelayedDate?.ToShortDateString() ?? "N/A");
                    table.AddCell(cashout.ProjectName);
                    table.AddCell(cashout.CostCenter ?? "N/A");
                    table.AddCell(cashout.Remark ?? "N/A");
                    table.AddCell(string.Join(", ", cashout.CashOutInstallments.Select(c => c.Amount)));
                    table.AddCell(string.Join(", ", cashout.CashOutInstallments.Select(c => c.DueDate.ToShortDateString())));
                }

                document.Add(table);
                document.Close();

                return ms.ToArray();
            }
        }

        private byte[] ExportToCsv(List<CashOut> cashouts)
        {
            using (var ms = new MemoryStream())
            using (var writer = new StreamWriter(ms))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                csv.WriteField("CashOut Data Report");
                csv.NextRecord();
                csv.WriteField(string.Empty);
                csv.NextRecord();

                csv.WriteRecords(cashouts.Select(c => new
                {
                    SupplierName = c.SupplierName,
                    AmountDue = c.AmountDue,
                    Date = c.Date,
                    Status = c.Status,
                    DelayedDate = c.DelayedDate?.ToShortDateString() ?? "N/A",
                    ProjectName = c.ProjectName,
                    CostCenter = c.CostCenter ?? "N/A",
                    Remark = c.Remark ?? "N/A",
                    InstallmentAmounts = string.Join(", ", c.CashOutInstallments.Select(i => i.Amount)),
                    InstallmentDueDates = string.Join(", ", c.CashOutInstallments.Select(i => i.DueDate.ToShortDateString()))
                }));

                writer.Flush();
                return ms.ToArray();
            }
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var cashouts = await _context.CashOutSet.Include(c => c.CashOutInstallments).ToListAsync();

                return View(cashouts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching CashOut records");
                return StatusCode(500, "An error occurred while fetching CashOut records. Please try again later.");
            }
        }

        public async Task<IActionResult> Create()
        {
            CashOutCreateViewModel model = new();
            model.Date = DateTime.Now;
            model.Supplierlist = await GetSupplierList();
            model.ProjectDatabaselist = await GetProjectDatabaseList();

            return View(model);


        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CashOutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var supplierName = await _context.SupplierSet
                .Where(x => x.Id == model.SupplierId).Select(c => c.Name).FirstOrDefaultAsync();
            var projectName = await _context.ProjectDatabaseSet.Where(p => p.Id == model.ProjectId).
                Select(x => x.ProjectName).FirstOrDefaultAsync();
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var cashout = new CashOut
                {
                    SupplierId = model.SupplierId,
                    SupplierName = supplierName,
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

                _context.CashOutSet.Add(cashout);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"CashOut created: {cashout.Id}");

                if (model.CashOutInstallments != null && model.CashOutInstallments.Any())
                {
                    var installmentEntities = model.CashOutInstallments.Select(installment => new CashOutInstallment
                    {
                        CashOutId = cashout.Id,
                        NumberOfInstallment = installment.NumberOfInstallment,
                        Amount = installment.Amount,
                        DueDate = installment.DueDate
                    }).ToList();

                    _context.CashOutInstallmentSet.AddRange(installmentEntities);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Installment details added: {cashout.Id}");
                }

                await transaction.CommitAsync();

                return Ok(new { message = "CashOut created successfully" });
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Database update error occurred while saving CashOut");
                return StatusCode(500, new { message = "A database error occurred while saving the CashIn.", error = ex.InnerException?.Message ?? ex.Message });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while saving Cashout");
                return StatusCode(500, new { message = "An error occurred while saving the Cashout.", error = ex.Message });
            }
        }

        // GET: CashOut/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cashout = await _context.CashOutSet
                .Include(c => c.CashOutInstallments)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (cashout == null)
            {
                return NotFound();
            }

            var viewModel = new CashOutViewModel
            {
                Id = cashout.Id,
                SupplierId = cashout.SupplierId,
                SupplierName = cashout.SupplierName,
                AmountDue = cashout.AmountDue,
                Date = cashout.Date,
                Status = cashout.Status,
                DelayedDate = cashout.DelayedDate,
                ProjectId = cashout.ProjectId,
                CostCenter = cashout.CostCenter,
                Remark = cashout.Remark,
                PayAsInstallment = cashout.PayAsInstallment,

                CashOutInstallments = cashout.CashOutInstallments.Select(i => new CashOutInstallmentViewModel
                {
                    NumberOfInstallment = i.NumberOfInstallment,
                    Amount = i.Amount,
                    DueDate = i.DueDate
                }).ToList()
            };

            await PopulateViewModelLists(viewModel);

            return View(viewModel);
        }

        // POST: CashOut/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CashOutViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var cashout = await _context.CashOutSet
                        .Include(c => c.CashOutInstallments)
                        .FirstOrDefaultAsync(m => m.Id == id);

                    if (cashout == null)
                    {
                        return NotFound();
                    }

                    await UpdateCashOutFromViewModel(cashout, viewModel);

                    _context.Update(cashout);
                    await _context.SaveChangesAsync();

                    TempData["CashOutSuccessMessage"] = "CashOut updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CashOutExists(viewModel.Id))
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
        private async Task PopulateViewModelLists(CashOutViewModel viewModel)
        {
            viewModel.SupplierList = await GetSupplierList();
            viewModel.ProjectDatabaseList = await GetProjectDatabaseList();
            viewModel.StatusList = GetStatusList();
            viewModel.CostCenterList = GetCostCenterList();
        }

        private List<SelectListItem> GetStatusList()
        {
            return new List<SelectListItem>
        {
            new SelectListItem { Value = "Paid", Text = "Paid" },
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

        private async Task UpdateCashOutFromViewModel(CashOut cashout, CashOutViewModel viewModel)
        {
            cashout.SupplierId = viewModel.SupplierId;
            cashout.SupplierName = await _context.SupplierSet
                .Where(s => s.Id == viewModel.SupplierId)
                .Select(s => s.Name)
                .FirstOrDefaultAsync();
            cashout.AmountDue = viewModel.AmountDue;
            cashout.Date = viewModel.Date;
            cashout.Status = viewModel.Status;
            cashout.DelayedDate = viewModel.DelayedDate;
            cashout.ProjectId = viewModel.ProjectId;
            cashout.ProjectName = await _context.ProjectDatabaseSet
                .Where(p => p.Id == viewModel.ProjectId)
                .Select(p => p.ProjectName)
                .FirstOrDefaultAsync();
            cashout.CostCenter = viewModel.CostCenter;
            cashout.Remark = viewModel.Remark;
            cashout.PayAsInstallment = viewModel.PayAsInstallment;

            // Remove existing installments
            _context.CashOutInstallmentSet.RemoveRange(cashout.CashOutInstallments);

            // Add updated installments
            if (viewModel.CashOutInstallments != null && viewModel.CashOutInstallments.Any())
            {
                cashout.CashOutInstallments = viewModel.CashOutInstallments.Select(i => new CashOutInstallment
                {
                    CashOutId = cashout.Id,
                    NumberOfInstallment = i.NumberOfInstallment,
                    Amount = i.Amount,
                    DueDate = i.DueDate
                }).ToList();
            }
        }

        private bool CashOutExists(int id)
        {
            return _context.CashOutSet.Any(e => e.Id == id);
        }

        // GET: CashIn/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cashout = await _context.CashOutSet
                .Include(c => c.CashOutInstallments)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (cashout == null)
            {
                return NotFound();
            }

            return View(cashout);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cashout = await _context.CashOutSet
                .Include(c => c.CashOutInstallments)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (cashout == null)
            {
                return NotFound();
            }

            try
            {
                // Remove associated installments
                _context.CashOutInstallmentSet.RemoveRange(cashout.CashOutInstallments);

                // Remove the CashIn entity
                _context.CashOutSet.Remove(cashout);
                await _context.SaveChangesAsync();

                // Return a JSON result with success message
                return Json(new { success = true, message = "CashOut deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting CashOut with ID {id}");
                // Return a JSON result with error message
                return Json(new { success = false, message = "An error occurred while deleting the CashIn. Please try again." });
            }
        }
        [NonAction]
        private async Task<List<SelectListItem>> GetSupplierList()
        {
            var suppliers = await _context.SupplierSet.ToListAsync();
            return suppliers.Select(c => new SelectListItem
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
