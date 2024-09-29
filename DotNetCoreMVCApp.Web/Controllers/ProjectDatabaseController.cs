using DotNetCoreMVCApp.Models;
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
    public class ProjectDatabaseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProjectDatabaseController> _logger;

        public ProjectDatabaseController(ApplicationDbContext context, ILogger<ProjectDatabaseController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: ProjectDatabase/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ProjectDatabase/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectDatabaseCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (await _context.ProjectDatabaseSet.AnyAsync(p => p.ProjectName == model.ProjectName))
            {
                ModelState.AddModelError("ProjectName", "A project with this name already exists.");
                return View(model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var projectDatabase = new ProjectDatabase
                {
                    ProjectName = model.ProjectName,
                    CreatedBy = User.Identity?.Name,
                    CreatedOn = DateTime.UtcNow,
                    IsDeleted = false
                };

                _context.ProjectDatabaseSet.Add(projectDatabase);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"ProjectDatabase created: {projectDatabase.Id}");
                await transaction.CommitAsync();
                TempData["SuccessMessage"] = "Project created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Database update error occurred while saving project");
                ModelState.AddModelError("", "A database error occurred while saving the project.");
                return View(model);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while saving project");
                ModelState.AddModelError("", "An error occurred while saving the project.");
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var projectDatabase = await _context.ProjectDatabaseSet.FindAsync(id);
            if (projectDatabase == null)
            {
                return NotFound();
            }

            var viewModel = new ProjectDatabaseViewModel
            {
                Id = projectDatabase.Id,
                ProjectName = projectDatabase.ProjectName
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProjectDatabaseViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (await _context.ProjectDatabaseSet.AnyAsync(p => p.ProjectName == model.ProjectName && p.Id != id))
                {
                    ModelState.AddModelError("ProjectName", "A project with this name already exists.");
                    return View(model);
                }

                try
                {
                    var projectDatabase = await _context.ProjectDatabaseSet.FindAsync(id);
                    if (projectDatabase == null)
                    {
                        return NotFound();
                    }

                    projectDatabase.ProjectName = model.ProjectName;
                    projectDatabase.UpdatedBy = User.Identity?.Name;
                    projectDatabase.UpdatedOn = DateTime.UtcNow;

                    _context.Update(projectDatabase);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"ProjectDatabase updated: {projectDatabase.Id}");
                    TempData["SuccessMessage"] = "Project updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, "Concurrency error occurred while updating project");
                    ModelState.AddModelError("", "The project was modified by another user. Please try again.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while updating project");
                    ModelState.AddModelError("", "An error occurred while updating the project.");
                }
            }

            return View(model);
        }


        // GET: ProjectDatabase/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var projects = await _context.ProjectDatabaseSet
                    .Where(b => !b.IsDeleted)
                    .Select(b => new ProjectDatabaseViewModel
                    {
                        Id = b.Id,
                        ProjectName = b.ProjectName
                    })
                    .ToListAsync();

                return View(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching projects");
                return StatusCode(500, "An error occurred while fetching projects. Please try again later.");
            }
        }
    }
}
        