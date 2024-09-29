using System;
using System.ComponentModel.DataAnnotations;

namespace DotNetCoreMVCApp.Models.Web
{
    public class CashOutInstallmentViewModel
    {
        public int InstallmentId { get; set; }

        public int CashOutId { get; set; }

        [Required]
        public int NumberOfInstallment { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0: dd-MM-yyyy}")]
        public DateTime DueDate { get; set; }
    }
}