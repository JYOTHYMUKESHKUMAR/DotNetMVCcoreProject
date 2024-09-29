using DotNetCoreMVCApp.Models.Entities; // Ensure you have the correct namespace for ApplicationUser
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetCoreMVCApp.Models.Repository
{
    [Table("BankBalance")]
    public class BankBalance
    {
        [Key]
        
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        
        public string BankName { get; set; }

       

        [Required]
        public decimal CurrentBalance { get; set; }

        
        [Required]
        public string CreatedBy { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; }

        public string UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }

        public bool IsDeleted { get; set; }

        public string DeletedBy { get; set; }
        public DateTime? DeletedOn { get; set; }


    }
}