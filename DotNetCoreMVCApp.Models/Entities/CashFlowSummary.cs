using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetCoreMVCApp.Models.Entities
{
    public class CashFlowSummary
    {
        public DateTime Date { get; set; }
        [Column("CashIn")]
        public decimal CashIn { get; set; }
        [Column("CashOut")]
        public decimal CashOut { get; set; }
        public decimal Balance { get; set; }
    }
}