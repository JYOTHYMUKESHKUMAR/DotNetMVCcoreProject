using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetCoreMVCApp.Models.Repository;
using DotNetCoreMVCApp.Models.Web;

namespace DotNetCoreMVCApp.Service.Abstraction
{
    public interface ICashOutService
    {
        Task<IEnumerable<CashOutViewModel>> GetAllAsync();
        Task<CashOutViewModel> GetByIdAsync(int id);
        Task<ErrorStateModel> ValidateAsync(CashOutViewModel cashout);
        Task<bool> CreateAsync(CashOutViewModel cashout, string userId);
        Task<bool> UpdateAsync(CashOutViewModel cashout, string userId);
        Task<bool> DeleteAsync(int id, string userId);
    }
}
