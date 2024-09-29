using DotNetCoreMVCApp.Models.Entities;
using DotNetCoreMVCApp.Models.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCoreMVCApp.Models.Web
{
   
    public class SupplierViewModel
    {
       
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        public string BankName { get; set; }

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

        public List<SupplierContactViewModel> SupplierContacts { get; set; }
    }
}


