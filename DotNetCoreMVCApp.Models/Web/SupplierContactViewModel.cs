using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCoreMVCApp.Models.Web
{
    public class SupplierContactViewModel
    {
        public int ContactId { get; set; }
        public int SupplierId { get; set; }
        public string ContactName { get; set; }
        public string Designation { get; set; }
        [RegularExpression(@"^\d{8}$", ErrorMessage = "Mobile number must be exactly 8 digits")]
        public string Mobile { get; set; }


        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
        public bool IsDeleted { get; set; }
    }
}
