using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetCoreMVCApp.Models.Repository;

using DotNetCoreMVCApp.Models.Web;

namespace DotNetCoreMVCApp.Service.Abstraction
{
    public interface IProjectDatabaseService
    {
        Task<IEnumerable<ProjectDatabaseViewModel>> GetAllAsync();
        Task<ProjectDatabaseViewModel> GetByIdAsync(int id);
        Task<ErrorStateModel> ValidateAsync(ProjectDatabaseViewModel projectdatabase);
        Task<bool> CreateAsync(ProjectDatabaseViewModel projectdatabase, string userId);
        Task<bool> UpdateAsync(ProjectDatabaseViewModel projectdatabase, string userId);
        Task<bool> DeleteAsync(int id, string userId);
    }
}
