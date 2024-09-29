using DotNetCoreMVCApp.Models;
using DotNetCoreMVCApp.Models.Entities;
using DotNetCoreMVCApp.Models.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreMVCApp.Web.Controllers
{
    public class StartingBalanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StartingBalanceController> _logger;

        public StartingBalanceController(ApplicationDbContext context, ILogger<StartingBalanceController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: StartingBalance
        public async Task<IActionResult> Index()
        {
            try
            {
                var startingBalance = await _context.StartingBalanceSet.FirstOrDefaultAsync(sb => !sb.IsDeleted);
                var viewModel = startingBalance != null ? new StartingBalanceViewModel
                {
                    Id = startingBalance.Id,
                    StartingDate = startingBalance.StartingDate,
                    AvailableBalance = startingBalance.AvailableBalance,
                } : null;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching starting balance");
                return StatusCode(500, "An error occurred while fetching starting balance. Please try again later.");
            }
        }

        // GET: StartingBalance/Create
        public async Task<IActionResult> Create()
        {
            if (await _context.StartingBalanceSet.AnyAsync(sb => !sb.IsDeleted))
            {
                TempData["ErrorMessage"] = "A starting balance entry already exists.";
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        // POST: StartingBalance/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StartingBalanceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (await _context.StartingBalanceSet.AnyAsync(sb => !sb.IsDeleted))
            {
                ModelState.AddModelError("", "A starting balance entry already exists.");
                return View(model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var startingBalance = new StartingBalance
                {
                    StartingDate = model.StartingDate,
                    AvailableBalance = model.AvailableBalance,
                    CreatedBy = User.Identity?.Name,
                    CreatedOn = DateTime.Now,
                    IsDeleted = false
                };
                _context.StartingBalanceSet.Add(startingBalance);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"StartingBalance created: {startingBalance.Id}");
                await transaction.CommitAsync();
                TempData["SuccessMessage"] = "Starting balance created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while saving starting balance");
                ModelState.AddModelError("", "An error occurred while saving the starting balance.");
                return View(model);
            }
        }

        // GET: StartingBalance/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var startingBalance = await _context.StartingBalanceSet.FindAsync(id);
            if (startingBalance == null)
            {
                return NotFound();
            }

            var viewModel = new StartingBalanceViewModel
            {
                Id = startingBalance.Id,
                StartingDate = startingBalance.StartingDate,
                AvailableBalance = startingBalance.AvailableBalance
            };

            return View(viewModel);
        }

        // POST: StartingBalance/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StartingBalanceViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var startingBalance = await _context.StartingBalanceSet.FindAsync(id);
                    if (startingBalance == null)
                    {
                        return NotFound();
                    }

                    startingBalance.StartingDate = model.StartingDate;
                    startingBalance.AvailableBalance = model.AvailableBalance;
                    startingBalance.UpdatedBy = User.Identity?.Name;
                    startingBalance.UpdatedOn = DateTime.Now;

                    _context.Update(startingBalance);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Starting balance updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StartingBalanceExists(model.Id))
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

        private bool StartingBalanceExists(int id)
        {
            return _context.StartingBalanceSet.Any(e => e.Id == id);
        }
    }
}