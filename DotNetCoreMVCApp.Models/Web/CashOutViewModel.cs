
using DotNetCoreMVCApp.Models.Entities;
using DotNetCoreMVCApp.Models.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCoreMVCApp.Models.Web
{

    public class CashOutViewModel
    {

        public int Id { get; set; }



        [Required]
        public int SupplierId { get; set; }


        public string? SupplierName { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountDue { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; }

        public Nullable<DateTime> DelayedDate { get; set; }
        public int ProjectId { get; set; }

        [MaxLength(50)]
        public string CostCenter { get; set; }

        public string Remark { get; set; }
        public bool PayAsInstallment { get; set; }

        //public List<SelectListItem>
        public List<CashOutInstallmentViewModel> CashOutInstallments { get; set; }
        public List<SelectListItem> SupplierList { get; set; }
        public List<SelectListItem> ProjectDatabaseList { get; set; }
        public List<SelectListItem> StatusList { get; set; }
        public List<SelectListItem> CostCenterList { get; set; }


    }
}


