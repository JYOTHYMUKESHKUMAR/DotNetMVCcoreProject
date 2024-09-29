using System;
using System.ComponentModel.DataAnnotations;

namespace DotNetCoreMVCApp.Models.Web
{
    public class BankBalanceCreateViewModel
    {
        
        public string BankName { get; set; }

        
        
        [DataType(DataType.Currency)]
        public decimal CurrentBalance { get; set; }

        
    }
}
