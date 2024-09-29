using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCoreMVCApp.Models.Web
{
    public class CashFlowSummaryViewModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal CashIn { get; set; }
        public decimal CashOut { get; set; }
        public decimal Balance { get; set; }
    }

}
