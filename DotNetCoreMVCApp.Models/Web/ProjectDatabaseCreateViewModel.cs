using System.ComponentModel.DataAnnotations;

namespace DotNetCoreMVCApp.Models.Web
{
    public class ProjectDatabaseCreateViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string ProjectName { get; set; }
    }
}