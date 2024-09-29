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
    public class CustomerService : ICustomerService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILog _logger;

        public CustomerService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = LogManager.GetLogger(typeof(CustomerService));
        }

        public async Task<IEnumerable<CustomerViewModel>> GetAllAsync()
        {
            return _mapper.Map<List<CustomerViewModel>>(await _unitOfWork.CustomerRepository.GetAsync(filter: (c => c.IsDeleted == false), (c => c.OrderBy(a => a.Name))));
        }

        public async Task<CustomerViewModel> GetByIdAsync(int id)
        {
            return _mapper.Map<CustomerViewModel>(await _unitOfWork.CustomerRepository.GetByIdAsync(id));
        }

        public async Task<bool> CreateAsync(CustomerViewModel customerModel, string userId)
        {
            _logger.Info($"customer create request by user: {userId} : {JsonConvert.SerializeObject(customerModel)}");
            var customer = _mapper.Map<Customer>(customerModel);
            customer.CreatedBy = userId;
            customer.CreatedOn = DateTime.Now;
            await _unitOfWork.CustomerRepository.InsertAsync(customer);
            await _unitOfWork.SaveAsync();
            _logger.Info($"Created customer");
            return true;
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            _logger.Info($"customer delete request by user: {userId} : customer Id: {id}");
            var customer = await _unitOfWork.CustomerRepository.GetByIdAsync(id);
            customer.IsDeleted = true;
            customer.DeletedBy = userId;
            customer.DeletedOn = DateTime.Now;

            _unitOfWork.CustomerRepository.Update(customer);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> UpdateAsync(CustomerViewModel customerModel, string userId)
        {
            _logger.Info($"customer update request by user: {userId} : {JsonConvert.SerializeObject(customerModel)}");
            var customer = await _unitOfWork.CustomerRepository.GetByIdAsync(customerModel.Id);
         
            customer.Name = customerModel.Name;
            customer.UpdatedBy = userId;
            customer.UpdatedOn = DateTime.Now;
            _unitOfWork.CustomerRepository.Update(customer);
            await _unitOfWork.SaveAsync();
            _logger.Info($"Created customer");
            return true;
        }

        public async Task<ErrorStateModel> ValidateAsync(CustomerViewModel customerModel)
        {
            //Check if customer with same name or code exists
            ErrorStateModel errorStateModel = new();

            errorStateModel.IsValid = !(await _unitOfWork.CustomerRepository.GetAsync(filter: (c => c.Id != customerModel.Id && c.IsDeleted == false && (c.Name == customerModel.Name )))).Any();
            if (!errorStateModel.IsValid)
            {
                errorStateModel.Errors.Add("customer", "customer with same code or name exists.");
            }
            return errorStateModel;
        }
    }
}
