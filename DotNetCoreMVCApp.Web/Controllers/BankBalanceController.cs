using DotNetCoreMVCApp.Models;
using DotNetCoreMVCApp.Models.Entities;
using DotNetCoreMVCApp.Models.Repository;
using DotNetCoreMVCApp.Models.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreMVCApp.Web.Controllers
{
    public class BankBalanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BankBalanceController> _logger;

        public BankBalanceController(ApplicationDbContext context, ILogger<BankBalanceController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: BankBalance/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BankBalance/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BankBalanceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var bankbalance = new BankBalance
                {
                    BankName = model.BankName,
                    CurrentBalance = model.CurrentBalance,
                    CreatedBy = User.Identity?.Name,
                    CreatedOn = DateTime.Now,
                    IsDeleted = false
                };
                _context.BankBalanceSet.Add(bankbalance);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"BankBalance created: {bankbalance.Id}");
                await transaction.CommitAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while saving bank balance");
                ModelState.AddModelError("", "An error occurred while saving the bank balance.");
                return View(model);
            }
        }

        // GET: BankBalance/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bankBalance = await _context.BankBalanceSet.FindAsync(id);
            if (bankBalance == null)
            {
                return NotFound();
            }

            var viewModel = new BankBalanceViewModel
            {
                Id = bankBalance.Id,
                BankName = bankBalance.BankName,
                CurrentBalance = bankBalance.CurrentBalance
            };

            return View(viewModel);
        }

        // POST: BankBalance/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BankBalanceViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var bankBalance = await _context.BankBalanceSet.FindAsync(id);
                    if (bankBalance == null)
                    {
                        return NotFound();
                    }

                    bankBalance.BankName = model.BankName;
                    bankBalance.CurrentBalance = model.CurrentBalance;
                    bankBalance.UpdatedBy = User.Identity?.Name;
                    bankBalance.UpdatedOn = DateTime.Now;

                    _context.Update(bankBalance);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Bank balance updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BankBalanceExists(model.Id))
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

        // GET: BankBalance/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bankBalance = await _context.BankBalanceSet
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bankBalance == null)
            {
                return NotFound();
            }

            var viewModel = new BankBalanceViewModel
            {
                Id = bankBalance.Id,
                BankName = bankBalance.BankName,
                CurrentBalance = bankBalance.CurrentBalance
            };

            return View(viewModel);
        }

        // POST: BankBalance/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bankBalance = await _context.BankBalanceSet.FindAsync(id);
            if (bankBalance != null)
            {
                bankBalance.IsDeleted = true;
                bankBalance.DeletedBy = User.Identity?.Name;
                bankBalance.DeletedOn = DateTime.Now;
                _context.Update(bankBalance);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Bank balance deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Bank balance not found.";
            }

            return RedirectToAction(nameof(Index));
        }
        // GET: BankBalance/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var bankbalances = await _context.BankBalanceSet.Where(b => !b.IsDeleted).ToListAsync();
                var viewModel = bankbalances.Select(b => new BankBalanceViewModel
                {
                    Id = b.Id,
                    BankName = b.BankName,
                    CurrentBalance = b.CurrentBalance,
                }).ToList();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching bank balance");
                return StatusCode(500, "An error occurred while fetching bank balance. Please try again later.");
            }
        }

        private bool BankBalanceExists(int id)
        {
            return _context.BankBalanceSet.Any(e => e.Id == id);
        }
    }
}