using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DotNetCoreMVCApp.Models.Web
{
    // Enum for Status options
    public enum CashInStatus
    {
        Received,
        Scheduled,
        Delayed
    }

    // Enum for CostCenter options
    public enum CashInCostCenter
    {
        GeneralChemicals,
        Catalyst,
        Overhead,
        OilAndGas
    }

    public class CashInCreateViewModel
    {
        public CashInCreateViewModel()
        {
            CashInInstallments = new List<CashInInstallmentViewModel>();
        }

        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        [MaxLength(100)]
        public string CustomerName { get; set; }

        [Required]
        public decimal AmountDue { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime Date { get; set; }
        [Required]
        public string DateString { get; set; }

        [Required]
        [MaxLength(50)]
        public CashInStatus Status { get; set; }  // Use enum here

        public Nullable<DateTime> DelayedDate { get; set; }

       
        public int ProjectId { get; set; }

        [Required]
        [MaxLength(50)]
        public CashInCostCenter CostCenter { get; set; }  // Use enum here

        public string Remark { get; set; }
        public bool PayAsInstallment { get; set; }
        public List<SelectListItem> Customerlist { get; set; }
        public List<SelectListItem> ProjectDatabaselist { get; set; }



        public List<CashInInstallmentViewModel>? CashInInstallments { get; set; }
        public List<SelectListItem> InstallmentNumbers { get; set; }
    }
}