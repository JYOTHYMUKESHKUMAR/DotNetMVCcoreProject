
using DotNetCoreMVCApp.Models.Entities;
using DotNetCoreMVCApp.Models.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCoreMVCApp.Models.Web
{

    public class DashboardViewModel
    {
        public decimal TotalCashIn { get; set; }
        public decimal TotalCashOut { get; set; }
        public decimal TodayBalance { get; set; }
        public List<MonthlyCashFlow> MonthlyCashFlow { get; set; }
        public List<CashFlowSummary> CashFlowSummaries { get; set; }
        public List<ProjectCashFlowChartData> ProjectCashFlowData { get; set; }
        public List<string> Projects { get; set; }
        public string SelectedProject { get; set; }
        public List<ProjectCashFlowSummary> ProjectCashFlowSummaries { get; set; }
        public List<PieChartData> CashInPieChartData { get; set; }
        public List<PieChartData> CashOutPieChartData { get; set; }
    }
    public class PieChartData
    {
        public string Label { get; set; }
        public decimal Value { get; set; }
    }

    public class MonthlyCashFlow
    {
        public DateTime Date { get; set; }
        public decimal CashIn { get; set; }
        public decimal CashOut { get; set; }
    }
    public class ProjectCashFlowChartData
    {
        public string Project { get; set; }
        public decimal CashIn { get; set; }
        public int DelayedDays { get; set; }
    }
}
