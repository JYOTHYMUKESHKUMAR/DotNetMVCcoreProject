using DotNetCoreMVCApp.Models.Repository;
using DotNetCoreMVCApp.Models.Web;
using DotNetCoreMVCApp.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;
using DotNetCoreMVCApp.Models;

namespace DotNetCoreMVCApp.Web.Controllers
{
    public class SupplierController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SupplierController> _logger;

        public SupplierController(ApplicationDbContext context, ILogger<SupplierController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Supplier/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Supplier/Create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SupplierCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var supplier = new Supplier
                {
                    Name = model.Name,
                    BankName = model.BankName,
                    Branch = model.Branch,
                    AccountHolderName = model.AccountHolderName,
                    AccountNumber = model.AccountNumber,
                    IBAN = model.IBAN,
                    SwiftCode = model.SwiftCode,
                    CreatedBy = User.Identity?.Name,
                    CreatedOn = DateTime.Now,
                    IsDeleted = false
                };

                _context.SupplierSet.Add(supplier);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Supplier created: {supplier.Id}");

                if (model.SupplierContacts != null && model.SupplierContacts.Any())
                {
                    var contactViewModels = model.SupplierContacts.Select(contact => new SupplierContact
                    {
                        ContactName = contact.ContactName,
                        SupplierId = supplier.Id,
                        Email = contact.Email,
                        Designation = contact.Designation,
                        Mobile = contact.Mobile,
                    }).ToList();

                    _context.SupplierContactSet.AddRange(contactViewModels);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Supplier contacts added for supplier: {supplier.Id}");
                }

                await transaction.CommitAsync();

                return Ok(new { message = "Supplier created successfully" });
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Database update error occurred while saving supplier");
                return StatusCode(500, new { message = "A database error occurred while saving the supplier.", error = ex.InnerException?.Message ?? ex.Message });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while saving supplier");
                return StatusCode(500, new { message = "An error occurred while saving the supplier.", error = ex.Message });
            }
        }

        // GET: Supplier/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var suppliers = await _context.SupplierSet.Include(s => s.SupplierContacts).ToListAsync();
                return View(suppliers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching suppliers");
                return StatusCode(500, "An error occurred while fetching suppliers. Please try again later.");
            }
        }
        public async Task<IActionResult> Edit(int id)
        {
            var supplier = await _context.SupplierSet
                .Include(s => s.SupplierContacts)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (supplier == null)
            {
                return NotFound();
            }

            var viewModel = new SupplierViewModel
            {
                Id = supplier.Id,
                Name = supplier.Name,
                BankName = supplier.BankName,
                Branch = supplier.Branch,
                AccountHolderName = supplier.AccountHolderName,
                AccountNumber = supplier.AccountNumber,
                IBAN = supplier.IBAN,
                SwiftCode = supplier.SwiftCode,
                SupplierContacts = supplier.SupplierContacts.Select(sc => new SupplierContactViewModel
                {
                    ContactId = sc.ContactId,
                    ContactName = sc.ContactName,
                    Designation = sc.Designation,
                    Mobile = sc.Mobile,
                    Email = sc.Email
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SupplierViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var supplier = await _context.SupplierSet
                        .Include(s => s.SupplierContacts)
                        .FirstOrDefaultAsync(s => s.Id == id);

                    if (supplier == null)
                    {
                        return NotFound();
                    }

                    // Update supplier details
                    supplier.Name = model.Name;
                    supplier.BankName = model.BankName;
                    supplier.Branch = model.Branch;
                    supplier.AccountHolderName = model.AccountHolderName;
                    supplier.AccountNumber = model.AccountNumber;
                    supplier.IBAN = model.IBAN;
                    supplier.SwiftCode = model.SwiftCode;
                    supplier.UpdatedBy = User.Identity?.Name;
                    supplier.UpdatedOn = DateTime.Now;

                    // Update contacts if they are provided
                    if (model.SupplierContacts != null && model.SupplierContacts.Any())
                    {
                        foreach (var contactViewModel in model.SupplierContacts)
                        {
                            if (contactViewModel.IsDeleted)
                            {
                                var contactToRemove = supplier.SupplierContacts
                                    .FirstOrDefault(sc => sc.ContactId == contactViewModel.ContactId);
                                if (contactToRemove != null)
                                {
                                    _context.SupplierContactSet.Remove(contactToRemove);
                                }
                            }
                            else if (contactViewModel.ContactId != 0)
                            {
                                var existingContact = supplier.SupplierContacts
                                    .FirstOrDefault(sc => sc.ContactId == contactViewModel.ContactId);

                                if (existingContact != null)
                                {
                                    existingContact.ContactName = contactViewModel.ContactName;
                                    existingContact.Designation = contactViewModel.Designation;
                                    existingContact.Mobile = contactViewModel.Mobile;
                                    existingContact.Email = contactViewModel.Email;
                                }
                            }
                            else
                            {
                                supplier.SupplierContacts.Add(new SupplierContact
                                {
                                    ContactName = contactViewModel.ContactName,
                                    Designation = contactViewModel.Designation,
                                    Mobile = contactViewModel.Mobile,
                                    Email = contactViewModel.Email
                                });
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Supplier updated successfully";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SupplierExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(model);
        }
        private bool SupplierExists(int id)
        {
            return _context.SupplierSet.Any(e => e.Id == id);
        }
    }
}
