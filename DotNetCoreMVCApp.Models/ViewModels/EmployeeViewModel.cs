using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCoreMVCApp.Entity.ViewModels
{
    public class EmployeeViewModel
    {
        public int Id { get; set; }
        [Display(Name = "Employee Name")]
        [Required(ErrorMessage = "Please enter valid name")] // validation built in attributes
        public string EmpName { get; set; }
        [Range(18, 60, ErrorMessage = "Please enter range between 18,60")]
        public int Age { get; set; }
        public string Gender { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime? DOB { get; set; }
        public bool IsActive { get; set; }
        public int Salary { get; set; }
        public string Location { get; set; }
        public int DesgnId { get; set; }

        public string DesgnName { get; set; }
        public string Remarks { get; set; }
        public string Image { get; set; }
    }
}
