using System;
using System.Collections.Generic;
using DotNetCoreMVCApp.Models.Entities;
using DotNetCoreMVCApp.Models.Repository;

namespace DotNetCoreMVCApp.Models
{
    public class CashFlowDetailsViewModel
    {
        public DateTime Date { get; set; }
        public List<CashIn> CashInTransactions { get; set; }
        public List<CashOut> CashOutTransactions { get; set; }
    }
}
