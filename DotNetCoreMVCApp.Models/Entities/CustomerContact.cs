using DotNetCoreMVCApp.Models.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCoreMVCApp.Models.Entities
{
    [Table(nameof(CustomerContact))]
    public class CustomerContact
    {
        [Key]
        public int ContactId { get; set; }
        public int CustomerId { get; set; }
        public string ContactName { get; set; }
        public string Designation { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        [ForeignKey(nameof(CustomerId))]
        public Customer Customer { get; set; }
    }
}
