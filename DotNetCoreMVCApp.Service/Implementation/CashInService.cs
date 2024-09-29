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
    public class CashInService : ICashInService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILog _logger;

        public CashInService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = LogManager.GetLogger(typeof(CashInService));
        }

        public async Task<IEnumerable<CashInViewModel>> GetAllAsync()
        {
            return _mapper.Map<List<CashInViewModel>>(await _unitOfWork.CashInRepository.GetAsync(filter: (c => c.IsDeleted == false), (c => c.OrderBy(a => a.Id))));
        }

        public async Task<CashInViewModel> GetByIdAsync(int id)
        {
            return _mapper.Map<CashInViewModel>(await _unitOfWork.CashInRepository.GetByIdAsync(id));
        }

        public async Task<bool> CreateAsync(CashInViewModel cashinModel, string userId)
        {
            _logger.Info($"customer create request by user: {userId} : {JsonConvert.SerializeObject(cashinModel)}");
            var cashin = _mapper.Map<CashIn>(cashinModel);
            cashin.CreatedBy = userId;
            cashin.CreatedOn = DateTime.Now;
            await _unitOfWork.CashInRepository.InsertAsync(cashin);
            await _unitOfWork.SaveAsync();
            _logger.Info($"Created cashin");
            return true;
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            _logger.Info($"cashin delete request by user: {userId} : cashin Id: {id}");
            var cashin = await _unitOfWork.CashInRepository.GetByIdAsync(id);
            cashin.IsDeleted = true;
            cashin.DeletedBy = userId;
            cashin.DeletedOn = DateTime.Now;

            _unitOfWork.CashInRepository.Update(cashin);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> UpdateAsync(CashInViewModel cashinModel, string userId)
        {
            _logger.Info($"cashin update request by user: {userId} : {JsonConvert.SerializeObject(cashinModel)}");
            var cashin = await _unitOfWork.CashInRepository.GetByIdAsync(cashinModel.Id);

            cashin.Id = cashinModel.Id;
            cashin.UpdatedBy = userId;
            cashin.UpdatedOn = DateTime.Now;
            _unitOfWork.CashInRepository.Update(cashin);
            await _unitOfWork.SaveAsync();
            _logger.Info($"Created cashin");
            return true;
        }

        public async Task<ErrorStateModel> ValidateAsync(CashInViewModel cashinModel)
        {
            //Check if customer with same name or code exists
            ErrorStateModel errorStateModel = new();

            errorStateModel.IsValid = !(await _unitOfWork.CashInRepository.GetAsync(filter: (c => c.Id != cashinModel.Id && c.IsDeleted == false && (c.Id == cashinModel.Id)))).Any();
            if (!errorStateModel.IsValid)
            {
                errorStateModel.Errors.Add("cashin", "cashin with same code or name exists.");
            }
            return errorStateModel;
        }
    }
}
