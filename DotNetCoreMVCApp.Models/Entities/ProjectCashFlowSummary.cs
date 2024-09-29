using Microsoft.EntityFrameworkCore;
using System;

namespace DotNetCoreMVCApp.Models.Entities
{
    [Keyless]
    public class ProjectCashFlowSummary
    {
        public string Project { get; set; }
        public DateTime Date { get; set; }
        public decimal CashIn { get; set; }
        public decimal CashOut { get; set; }
        public string Status { get; set; }
        public DateTime? DelayedDate { get; set; }
    }
}