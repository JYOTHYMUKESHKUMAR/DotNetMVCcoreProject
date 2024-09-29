using DotNetCoreMVCApp.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetCoreMVCApp.Models.Repository
{
    [Table(nameof(CashOut))]
    public class CashOut
    {
        public CashOut()
        {
            CashOutInstallments = new List<CashOutInstallment>();
            ProjectDatabases = new List<ProjectDatabase>();
        }

        [Key]
        public int Id { get; set; }



        [Required]
        public int SupplierId { get; set; }

        [Required]
        [MaxLength(100)]
        public string SupplierName { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountDue { get; set; }


        [Required]
        public DateTime Date { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; }

        public DateTime? DelayedDate { get; set; }

        public int ProjectId { get; set; }
        public string ProjectName { get; set; }

        [Required]
        [MaxLength(50)]
        public string CostCenter { get; set; }

        public string Remark { get; set; }

        public bool PayAsInstallment { get; set; }

        [Required]
        public string CreatedBy { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; }

        public string UpdatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public bool IsDeleted { get; set; }

        public string DeletedBy { get; set; }

        public DateTime? DeletedOn { get; set; }

        public List<CashOutInstallment>? CashOutInstallments { get; set; }
        public List<ProjectDatabase>? ProjectDatabases { get; set; }
    }
}
