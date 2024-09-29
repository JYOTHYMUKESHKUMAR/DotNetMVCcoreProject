using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetCoreMVCApp.Models.Repository;
using DotNetCoreMVCApp.Models.Web;

namespace DotNetCoreMVCApp.Service.Abstraction
{
    public interface IBankBalanceService
    {
        Task<IEnumerable<BankBalanceViewModel>> GetAllAsync();
        Task<BankBalanceViewModel> GetByIdAsync(int id);
        Task<ErrorStateModel> ValidateAsync(BankBalanceViewModel bankBalance);
        Task<bool> CreateAsync(BankBalanceViewModel bankbalance, string userId);
        Task<bool> UpdateAsync(BankBalanceViewModel bankbalance, string userId);
        Task<bool> DeleteAsync(int id, string userId);
    }
}
