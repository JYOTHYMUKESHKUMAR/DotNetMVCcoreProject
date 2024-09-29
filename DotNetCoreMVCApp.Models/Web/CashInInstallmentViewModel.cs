using System;
using System.ComponentModel.DataAnnotations;

namespace DotNetCoreMVCApp.Models.Web
{
    public class CashInInstallmentViewModel
    {
        public int InstallmentId { get; set; }

        public int CashInId { get; set; }

        [Required]
        public int NumberOfInstallment { get; set; }

        [Required]
        public decimal Amount { get; set; }
        [Required]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime DueDate { get; set; }

        //[Required]
        //public string DueDateString { get; set; }
    }
}