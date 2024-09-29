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
    public class StartingBalanceService : IStartingBalanceService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILog _logger;

        public StartingBalanceService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = LogManager.GetLogger(typeof(StartingBalanceService));
        }

        public async Task<IEnumerable<StartingBalanceViewModel>> GetAllAsync()
        {
            return _mapper.Map<List<StartingBalanceViewModel>>(await _unitOfWork.StartingBalanceRepository.GetAsync(filter: (c => c.IsDeleted == false), (c => c.OrderBy(a => a.StartingDate))));
        }

        public async Task<StartingBalanceViewModel> GetByIdAsync(int id)
        {
            return _mapper.Map<StartingBalanceViewModel>(await _unitOfWork.StartingBalanceRepository.GetByIdAsync(id));
        }

        public async Task<bool> CreateAsync(StartingBalanceViewModel startingbalanceModel, string userId)
        {
            _logger.Info($"startingbalance create request by user: {userId} : {JsonConvert.SerializeObject(startingbalanceModel)}");
            var startingbalance = _mapper.Map<Models.Entities.StartingBalance>(startingbalanceModel);
            startingbalance.CreatedBy = userId;
            startingbalance.CreatedOn = DateTime.Now;
            await _unitOfWork.StartingBalanceRepository.InsertAsync(startingbalance);
            await _unitOfWork.SaveAsync();
            _logger.Info($"Created StartingBalance");
            return true;
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            _logger.Info($"StartingBalance delete request by user: {userId} : startingbalance Id: {id}");
            var startingbalance = await _unitOfWork.StartingBalanceRepository.GetByIdAsync(id);
            startingbalance.IsDeleted = true;
            startingbalance.DeletedBy = userId;
            startingbalance.DeletedOn = DateTime.Now;

            _unitOfWork.StartingBalanceRepository.Update(startingbalance);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> UpdateAsync(StartingBalanceViewModel startingbalanceModel, string userId)
        {
            _logger.Info($"starting balance update request by user: {userId} : {JsonConvert.SerializeObject(startingbalanceModel)}");
            var startingbalance = await _unitOfWork.StartingBalanceRepository.GetByIdAsync(startingbalanceModel.Id);

            startingbalance.StartingDate = startingbalanceModel.StartingDate;
            startingbalance.UpdatedBy = userId;
            startingbalance.UpdatedOn = DateTime.Now;
            _unitOfWork.StartingBalanceRepository.Update(startingbalance);
            await _unitOfWork.SaveAsync();
            _logger.Info($"Created StartingBalance");
            return true;
        }

        public async Task<ErrorStateModel> ValidateAsync(StartingBalanceViewModel startingbalanceModel)
        {
            //Check if customer with same name or code exists
            ErrorStateModel errorStateModel = new();

            errorStateModel.IsValid = !(await _unitOfWork.StartingBalanceRepository.GetAsync(filter: (c => c.Id != startingbalanceModel.Id && c.IsDeleted == false && (c.StartingDate == startingbalanceModel.StartingDate)))).Any();
            if (!errorStateModel.IsValid)
            {
                errorStateModel.Errors.Add("startingbalance", "startingbalance with same code or name exists.");
            }
            return errorStateModel;
        }
    }
}
