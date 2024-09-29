using AutoMapper;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetCoreMVCApp.Models.Repository;
using DotNetCoreMVCApp.Models.Web;
using DotNetCoreMVCApp.Repository.Implementation;
using DotNetCoreMVCApp.Service.Abstraction;

namespace DotNetCoreMVCApp.Service.Implementation
{
    public class BankBalanceService : IBankBalanceService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILog _logger;

        public BankBalanceService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = LogManager.GetLogger(typeof(BankBalanceService));
        }

        public async Task<IEnumerable<BankBalanceViewModel>> GetAllAsync()
        {
            return _mapper.Map<List<BankBalanceViewModel>>(await _unitOfWork.BankBalanceRepository.GetAsync(filter: (c => c.IsDeleted == false), (c => c.OrderBy(a => a.BankName))));
        }

        public async Task<BankBalanceViewModel> GetByIdAsync(int id)
        {
            return _mapper.Map<BankBalanceViewModel>(await _unitOfWork.BankBalanceRepository.GetByIdAsync(id));
        }

        public async Task<bool> CreateAsync(BankBalanceViewModel bankbalanceModel, string userId)
        {
            _logger.Info($"bankbalance create request by user: {userId} : {JsonConvert.SerializeObject(bankbalanceModel)}");
            var bankbalance = _mapper.Map<BankBalance>(bankbalanceModel);
            bankbalance.CreatedBy = userId;
            bankbalance.CreatedOn = DateTime.Now;
            await _unitOfWork.BankBalanceRepository.InsertAsync(bankbalance);
            await _unitOfWork.SaveAsync();
            _logger.Info($"Created BankBalance");
            return true;
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            _logger.Info($"BankBalance delete request by user: {userId} : bankbalance Id: {id}");
            var bankbalance = await _unitOfWork.BankBalanceRepository.GetByIdAsync(id);
            bankbalance.IsDeleted = true;
            bankbalance.DeletedBy = userId;
            bankbalance.DeletedOn = DateTime.Now;

            _unitOfWork.BankBalanceRepository.Update(bankbalance);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> UpdateAsync(BankBalanceViewModel bankbalanceModel, string userId)
        {
            _logger.Info($"bank balance update request by user: {userId} : {JsonConvert.SerializeObject(bankbalanceModel)}");
            var bankbalance = await _unitOfWork.SupplierRepository.GetByIdAsync(bankbalanceModel.Id);

            bankbalance.BankName = bankbalanceModel.BankName;
            bankbalance.UpdatedBy = userId;
            bankbalance.UpdatedOn = DateTime.Now;
            _unitOfWork.SupplierRepository.Update(bankbalance);
            await _unitOfWork.SaveAsync();
            _logger.Info($"Created BankBalance");
            return true;
        }

        public async Task<ErrorStateModel> ValidateAsync(BankBalanceViewModel bankbalanceModel)
        {
            //Check if customer with same name or code exists
            ErrorStateModel errorStateModel = new();

            errorStateModel.IsValid = !(await _unitOfWork.BankBalanceRepository.GetAsync(filter: (c => c.Id != bankbalanceModel.Id && c.IsDeleted == false && (c.BankName == bankbalanceModel.BankName)))).Any();
            if (!errorStateModel.IsValid)
            {
                errorStateModel.Errors.Add("bankbalance", "bankbalance with same code or name exists.");
            }
            return errorStateModel;
        }
    }
}
