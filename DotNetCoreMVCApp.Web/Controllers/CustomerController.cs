using DotNetCoreMVCApp.Models.Repository;
using DotNetCoreMVCApp.Models.Web;
using DotNetCoreMVCApp.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetCoreMVCApp.Models;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace DotNetCoreMVCApp.Web.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(ApplicationDbContext context, ILogger<CustomerController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CustomerCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var customer = new Customer
                {
                    Name = model.Name,
                    CreatedBy = User.Identity?.Name,
                    CreatedOn = DateTime.Now,
                    IsDeleted = false
                };

                _context.CustomerSet.Add(customer);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Customer created: {customer.Id}");

                if (model.CustomerContacts != null && model.CustomerContacts.Any())
                {
                    var contactViewModels = model.CustomerContacts.Select(contact => new CustomerContact
                    {
                        ContactName = contact.ContactName,
                        CustomerId = customer.Id,
                        Email = contact.Email,
                        Designation = contact.Designation,
                        Mobile = contact.Mobile,
                    }).ToList();

                    _context.CustomerContactSet.AddRange(contactViewModels);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Customer contacts added for customer: {customer.Id}");
                }

                await transaction.CommitAsync();

                return Ok(new { message = "Customer created successfully" });
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Database update error occurred while saving customer");
                return StatusCode(500, new { message = "A database error occurred while saving the customer.", error = ex.InnerException?.Message ?? ex.Message });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while saving customer");
                return StatusCode(500, new { message = "An error occurred while saving the customer.", error = ex.Message });
            }
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var customers = await _context.CustomerSet.Include(c => c.CustomerContacts).ToListAsync();
                return View(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching customers");
                return StatusCode(500, "An error occurred while fetching customers. Please try again later.");
            }
        }
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _context.CustomerSet
                .Include(c => c.CustomerContacts)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
            {
                return NotFound();
            }

            var viewModel = new CustomerViewModel
            {
                Id = customer.Id,
                Name = customer.Name,
                CustomerContacts = customer.CustomerContacts.Select(cc => new CustomerContactViewModel
                {
                    ContactId = cc.ContactId,
                    ContactName = cc.ContactName,
                    Designation = cc.Designation,
                    Mobile = cc.Mobile,
                    Email = cc.Email
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CustomerViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var customer = await _context.CustomerSet
                        .Include(c => c.CustomerContacts)
                        .FirstOrDefaultAsync(c => c.Id == id);

                    if (customer == null)
                    {
                        return NotFound();
                    }

                    // Update customer details
                    customer.Name = model.Name;
                    customer.UpdatedBy = User.Identity?.Name;
                    customer.UpdatedOn = DateTime.Now;

                    // Update contacts if they are provided
                    if (model.CustomerContacts != null)
                    {
                        foreach (var contactViewModel in model.CustomerContacts)
                        {
                            if (contactViewModel.IsDeleted)
                            {
                                var contactToRemove = customer.CustomerContacts
                                    .FirstOrDefault(cc => cc.ContactId == contactViewModel.ContactId);
                                if (contactToRemove != null)
                                {
                                    _context.CustomerContactSet.Remove(contactToRemove);
                                }
                            }
                            else if (contactViewModel.ContactId != 0)
                            {
                                var existingContact = customer.CustomerContacts
                                    .FirstOrDefault(cc => cc.ContactId == contactViewModel.ContactId);

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
                                customer.CustomerContacts.Add(new CustomerContact
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
                    TempData["SuccessMessage"] = "Customer updated successfully";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(model.Id))
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

        private bool CustomerExists(int id)
        {
            return _context.CustomerSet.Any(e => e.Id == id);
        }
    }
}
