using DotNetCoreMVCApp.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetCoreMVCApp.Models.Repository
{
    [Table(nameof(Supplier))]
    public class Supplier
    {
        public Supplier()
        {
            SupplierContacts = new List<SupplierContact>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        
        public string? BankName { get; set; }

        [MaxLength(100)]
        public string Branch { get; set; }

        [MaxLength(100)]
        public string AccountHolderName { get; set; }

        [MaxLength(50)]
        public string AccountNumber { get; set; }

        [MaxLength(34)]
        public string IBAN { get; set; }

        [MaxLength(11)]
        public string SwiftCode { get; set; }

        public string CreatedBy { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; }

        public string UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }

        public bool IsDeleted { get; set; }

        public string DeletedBy { get; set; }
        public DateTime? DeletedOn { get; set; }

        // Navigation property
        public List<SupplierContact> SupplierContacts { get; set; }
    }
}
