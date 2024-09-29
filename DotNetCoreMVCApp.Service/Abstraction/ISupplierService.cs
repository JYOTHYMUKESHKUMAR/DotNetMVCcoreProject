using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetCoreMVCApp.Models.Repository;
using DotNetCoreMVCApp.Models.Web;

namespace DotNetCoreMVCApp.Service.Abstraction
{
    public interface ISupplierService
    {
        Task<IEnumerable<SupplierViewModel>> GetAllAsync();
        Task<SupplierViewModel> GetByIdAsync(int id);
        Task<ErrorStateModel> ValidateAsync(SupplierViewModel supplier);
        Task<bool> CreateAsync(SupplierViewModel supplier, string userId);
        Task<bool> UpdateAsync(SupplierViewModel supplier, string userId);
        Task<bool> DeleteAsync(int id, string userId);
    }
}
