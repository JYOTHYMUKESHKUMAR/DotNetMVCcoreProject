using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DotNetCoreMVCApp.Models;
using DotNetCoreMVCApp.Models.Web;
using DotNetCoreMVCApp.Models.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using DotNetCoreMVCApp.Web.Models;

namespace DotNetCoreMVCApp.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Dashboard(string projectName)
        {
            var today = DateTime.Today;
            var cashFlowSummaries = await _context.CashFlowSummaries
                .OrderBy(cfs => cfs.Date)
                .ToListAsync();

            var totalCashIn = cashFlowSummaries.Sum(cfs => cfs.CashIn);
            var totalCashOut = cashFlowSummaries.Sum(cfs => cfs.CashOut);
            var todayBalance = cashFlowSummaries
                .FirstOrDefault(cfs => cfs.Date.Date == today)?.Balance
                ?? cashFlowSummaries
                    .Where(cfs => cfs.Date.Date < today)
                    .OrderByDescending(cfs => cfs.Date)
                    .FirstOrDefault()?.Balance
                ?? 0;

            var monthlyCashFlow = cashFlowSummaries
                .GroupBy(cfs => new { cfs.Date.Year, cfs.Date.Month })
                .Select(g => new MonthlyCashFlow
                {
                    Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                    CashIn = g.Sum(cfs => cfs.CashIn),
                    CashOut = g.Sum(cfs => cfs.CashOut)
                })
                .OrderBy(mcf => mcf.Date)
                .ToList();

            var projectCashFlowSummaries = await GetProjectCashFlowSummaries(projectName);

            var cashInPieChartData = await GetCashInPieChartData();
            var cashOutPieChartData = await GetCashOutPieChartData();

            var dashboardViewModel = new DashboardViewModel
            {
                TotalCashIn = totalCashIn,
                TotalCashOut = totalCashOut,
                TodayBalance = todayBalance,
                MonthlyCashFlow = monthlyCashFlow,
                CashFlowSummaries = cashFlowSummaries,
                ProjectCashFlowSummaries = projectCashFlowSummaries,
                Projects = await _context.ProjectCashFlowSummaries
                    .Select(p => p.Project)
                    .Distinct()
                    .OrderBy(p => p)
                    .ToListAsync(),
                SelectedProject = projectName,
                CashInPieChartData = cashInPieChartData,
                CashOutPieChartData = cashOutPieChartData
            };

            return View(dashboardViewModel);
        }

        private async Task<List<ProjectCashFlowSummary>> GetProjectCashFlowSummaries(string projectName)
        {
            var query = _context.ProjectCashFlowSummaries.AsQueryable();
            if (!string.IsNullOrEmpty(projectName))
            {
                query = query.Where(p => p.Project == projectName);
            }
            return await query
                .OrderBy(p => p.Project)
                .ThenBy(p => p.Date)
                .ToListAsync();
        }

        private async Task<List<PieChartData>> GetCashInPieChartData()
        {
            return await _context.CashInSet
                .GroupBy(c => c.Status)
                .Select(g => new PieChartData
                {
                    Label = g.Key,
                    Value = g.Sum(c => c.AmountDue)
                })
                .ToListAsync();
        }

        private async Task<List<PieChartData>> GetCashOutPieChartData()
        {
            return await _context.CashOutSet
                .GroupBy(c => c.Status)
                .Select(g => new PieChartData
                {
                    Label = g.Key,
                    Value = g.Sum(c => c.AmountDue)
                })
                .ToListAsync();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}