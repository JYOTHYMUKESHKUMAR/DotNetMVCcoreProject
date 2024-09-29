using DotNetCoreMVCApp.Models.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetCoreMVCApp.Models.Entities
{
    [Table(nameof(CashInInstallment))]
    public class CashInInstallment
    {
        [Key]
        public int InstallmentId { get; set; }

        [Required]
        public int CashInId { get; set; }

        [Required]
        public int NumberOfInstallment { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [ForeignKey(nameof(CashInId))]
        public CashIn CashIn { get; set; }
    }
}