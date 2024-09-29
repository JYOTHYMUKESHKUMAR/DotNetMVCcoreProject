using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetCoreMVCApp.Models.Repository;
using DotNetCoreMVCApp.Models.Web;

namespace DotNetCoreMVCApp.Service.Abstraction
{
    public interface IStartingBalanceService
    {
        Task<IEnumerable<StartingBalanceViewModel>> GetAllAsync();
        Task<StartingBalanceViewModel> GetByIdAsync(int id);
        Task<ErrorStateModel> ValidateAsync(StartingBalanceViewModel startingBalance);
        Task<bool> CreateAsync(StartingBalanceViewModel startingbalance, string userId);
        Task<bool> UpdateAsync(StartingBalanceViewModel startingbalance, string userId);
        Task<bool> DeleteAsync(int id, string userId);
    }
}
