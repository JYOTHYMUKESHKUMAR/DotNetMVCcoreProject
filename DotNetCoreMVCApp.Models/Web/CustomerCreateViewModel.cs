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
   
    public class CustomerCreateViewModel
    {
        public CustomerCreateViewModel()
        {
            CustomerContacts = new List<CustomerContactViewModel>();
        }
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        public List<CustomerContactViewModel>CustomerContacts { get; set; }
    }
}


