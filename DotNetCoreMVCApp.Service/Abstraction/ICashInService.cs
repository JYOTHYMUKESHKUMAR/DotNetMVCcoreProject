using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetCoreMVCApp.Models.Repository;
using DotNetCoreMVCApp.Models.Web;

namespace DotNetCoreMVCApp.Service.Abstraction
{
    public interface ICashInService
    {
        Task<IEnumerable<CashInViewModel>> GetAllAsync();
        Task<CashInViewModel> GetByIdAsync(int id);
        Task<ErrorStateModel> ValidateAsync(CashInViewModel cashin);
        Task<bool> CreateAsync(CashInViewModel cashin, string userId);
        Task<bool> UpdateAsync(CashInViewModel cashin, string userId);
        Task<bool> DeleteAsync(int id, string userId);
    }
}
