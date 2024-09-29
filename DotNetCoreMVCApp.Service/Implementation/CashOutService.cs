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
    public class CashOutService : ICashOutService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILog _logger;

        public CashOutService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = LogManager.GetLogger(typeof(CashOutService));
        }

        public async Task<IEnumerable<CashOutViewModel>> GetAllAsync()
        {
            return _mapper.Map<List<CashOutViewModel>>(await _unitOfWork.CashOutRepository.GetAsync(filter: (c => c.IsDeleted == false), (c => c.OrderBy(a => a.Id))));
        }

        public async Task<CashOutViewModel> GetByIdAsync(int id)
        {
            return _mapper.Map<CashOutViewModel>(await _unitOfWork.CashOutRepository.GetByIdAsync(id));
        }

        public async Task<bool> CreateAsync(CashOutViewModel cashoutModel, string userId)
        {
            _logger.Info($"cashin create request by user: {userId} : {JsonConvert.SerializeObject(cashoutModel)}");
            var cashout = _mapper.Map<CashOut>(cashoutModel);
            cashout.CreatedBy = userId;
            cashout.CreatedOn = DateTime.Now;
            await _unitOfWork.CashOutRepository.InsertAsync(cashout);
            await _unitOfWork.SaveAsync();
            _logger.Info($"Created cashout");
            return true;
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            _logger.Info($"cashout delete request by user: {userId} : cashout Id: {id}");
            var cashout = await _unitOfWork.CashOutRepository.GetByIdAsync(id);
            cashout.IsDeleted = true;
            cashout.DeletedBy = userId;
            cashout.DeletedOn = DateTime.Now;

            _unitOfWork.CashOutRepository.Update(cashout);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> UpdateAsync(CashOutViewModel cashoutModel, string userId)
        {
            _logger.Info($"cashout update request by user: {userId} : {JsonConvert.SerializeObject(cashoutModel)}");
            var cashout = await _unitOfWork.CashOutRepository.GetByIdAsync(cashoutModel.Id);

            cashout.Id = cashoutModel.Id;
            cashout.UpdatedBy = userId;
            cashout.UpdatedOn = DateTime.Now;
            _unitOfWork.CashOutRepository.Update(cashout);
            await _unitOfWork.SaveAsync();
            _logger.Info($"Created cashout");
            return true;
        }

        public async Task<ErrorStateModel> ValidateAsync(CashOutViewModel cashoutModel)
        {
            //Check if customer with same name or code exists
            ErrorStateModel errorStateModel = new();

            errorStateModel.IsValid = !(await _unitOfWork.CashOutRepository.GetAsync(filter: (c => c.Id != cashoutModel.Id && c.IsDeleted == false && (c.Id == cashoutModel.Id)))).Any();
            if (!errorStateModel.IsValid)
            {
                errorStateModel.Errors.Add("cashout", "cashout with same code or name exists.");
            }
            return errorStateModel;
        }
    }
}
