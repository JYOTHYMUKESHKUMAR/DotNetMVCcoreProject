using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCoreMVCApp.Entity.ViewModels
{
    public class ChangePasswordViewModel
    {
        [DataType(DataType.Password)]
        [Required]
        public string CurrentPassword { get; set;}
        [DataType(DataType.Password)]
        [Required]
        public string NewPassword { get; set;}
        [DataType(DataType.Password)]
        [Required]
        [Compare("NewPassword",ErrorMessage ="NewPassword and Confirm Password does not match ")]
        public string ConfirmPassword { get; set;}
    }
}
