using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DotNetCoreMVCApp.Models.Web
{
    // Enum for Status options
    public enum CashOutStatus
    {
        Paid,
        Scheduled,
        Delayed
    }

    // Enum for CostCenter options
    public enum CashOutCostCenter
    {
        GeneralChemicals,
        Catalyst,
        Overhead,
        OilAndGas
    }

    public class CashOutCreateViewModel
    {
        public CashOutCreateViewModel()
        {
            CashOutInstallments = new List<CashOutInstallmentViewModel>();
        }

        public int Id { get; set; }

        [Required]
        public int SupplierId { get; set; }

        [Required]
        [MaxLength(100)]
        public string SupplierName { get; set; }

        [Required]
        public decimal AmountDue { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0: MM-dd-yyyy}")]

        public DateTime Date { get; set; }

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
        public List<SelectListItem> Supplierlist { get; set; }
        public List<SelectListItem> ProjectDatabaselist { get; set; }



        public List<CashOutInstallmentViewModel>? CashOutInstallments { get; set; }
        public List<SelectListItem> InstallmentNumbers { get; set; }
    }
}