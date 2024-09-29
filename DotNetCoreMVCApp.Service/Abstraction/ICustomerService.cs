using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetCoreMVCApp.Models.Repository;
using DotNetCoreMVCApp.Models.Web;

namespace DotNetCoreMVCApp.Service.Abstraction
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerViewModel>> GetAllAsync();
        Task<CustomerViewModel> GetByIdAsync(int id);
        Task<ErrorStateModel> ValidateAsync(CustomerViewModel customer);
        Task<bool> CreateAsync(CustomerViewModel customer, string userId);
        Task<bool> UpdateAsync(CustomerViewModel customer, string userId);
        Task<bool> DeleteAsync(int id, string userId);
    }
}
