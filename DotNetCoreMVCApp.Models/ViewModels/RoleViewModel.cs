using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCoreMVCApp.Entity.ViewModels
{
    public class RoleViewModel
    {
        [Display(Name = "Role Name")]
        [Required]
        public string Name { get; set; }
    }
}
