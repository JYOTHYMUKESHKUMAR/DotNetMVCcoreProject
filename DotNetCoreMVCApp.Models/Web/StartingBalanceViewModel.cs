using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCoreMVCApp.Models.Web
{

    public class StartingBalanceViewModel
    {
        public int Id { get; set; }
        public DateTime StartingDate { get; set; }
        
        public decimal AvailableBalance { get; set; }
       
    }
}