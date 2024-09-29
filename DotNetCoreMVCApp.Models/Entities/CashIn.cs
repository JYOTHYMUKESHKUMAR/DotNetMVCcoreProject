using DotNetCoreMVCApp.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetCoreMVCApp.Models.Repository
{
    [Table(nameof(CashIn))]
    public class CashIn
    {
        public CashIn()
        {
            CashInInstallments = new List<CashInInstallment>();
            ProjectDatabases = new List<ProjectDatabase>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        [MaxLength(100)]
        public string CustomerName { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountDue { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; }

        public DateTime ? DelayedDate { get; set; }

        public int ProjectId { get; set; }
        public string? ProjectName { get; set; }

        [Required]
        [MaxLength(50)]
        public string CostCenter { get; set; }

        public string Remark { get; set; }

        public bool PayAsInstallment { get; set; }

        [Required]
        public string CreatedBy { get; set; }

        [Required]
        public DateTime? CreatedOn { get; set; }

        public string UpdatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public bool IsDeleted { get; set; }

        public string DeletedBy { get; set; }

        public DateTime? DeletedOn { get; set; }
        /*public string ModifiedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }*/

        public List<CashInInstallment>? CashInInstallments { get; set; }
        public List<ProjectDatabase>? ProjectDatabases { get; set; }
    }
}
