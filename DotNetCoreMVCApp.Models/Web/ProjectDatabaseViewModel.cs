using System.ComponentModel.DataAnnotations;


namespace DotNetCoreMVCApp.Models.Web
{
    public class ProjectDatabaseViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Project Name is required")]
        [Display(Name = "Project Name")]
        public string ProjectName { get; set; }
    }
}