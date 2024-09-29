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
    public class SupplierService : ISupplierService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILog _logger;

        public SupplierService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = LogManager.GetLogger(typeof(SupplierService));
        }

        public async Task<IEnumerable<SupplierViewModel>> GetAllAsync()
        {
            return _mapper.Map<List<SupplierViewModel>>(await _unitOfWork.SupplierRepository.GetAsync(filter: (c => c.IsDeleted == false), (c => c.OrderBy(a => a.Name))));
        }

        public async Task<SupplierViewModel> GetByIdAsync(int id)
        {
            return _mapper.Map<SupplierViewModel>(await _unitOfWork.SupplierRepository.GetByIdAsync(id));
        }

        public async Task<bool> CreateAsync(SupplierViewModel supplierModel, string userId)
        {
            _logger.Info($"supplier create request by user: {userId} : {JsonConvert.SerializeObject(supplierModel)}");
            var supplier = _mapper.Map<Supplier>(supplierModel);
            supplier.CreatedBy = userId;
            supplier.CreatedOn = DateTime.Now;
            await _unitOfWork.SupplierRepository.InsertAsync(supplier);
            await _unitOfWork.SaveAsync();
            _logger.Info($"Created Supplier");
            return true;
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            _logger.Info($"Supplier delete request by user: {userId} : supplier Id: {id}");
            var supplier= await _unitOfWork.SupplierRepository.GetByIdAsync(id);
            supplier.IsDeleted = true;
            supplier.DeletedBy = userId;
            supplier.DeletedOn = DateTime.Now;

            _unitOfWork.SupplierRepository.Update(supplier);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> UpdateAsync(SupplierViewModel supplierModel, string userId)
        {
            _logger.Info($"customer update request by user: {userId} : {JsonConvert.SerializeObject(supplierModel)}");
            var supplier = await _unitOfWork.SupplierRepository.GetByIdAsync(supplierModel.Id);
         
            supplier.Name = supplierModel.Name;
            supplier.UpdatedBy = userId;
            supplier.UpdatedOn = DateTime.Now;
            _unitOfWork.SupplierRepository.Update(supplier);
            await _unitOfWork.SaveAsync();
            _logger.Info($"Created supplier");
            return true;
        }

        public async Task<ErrorStateModel> ValidateAsync(SupplierViewModel supplierModel)
        {
            //Check if customer with same name or code exists
            ErrorStateModel errorStateModel = new();

            errorStateModel.IsValid = !(await _unitOfWork.SupplierRepository.GetAsync(filter: (c => c.Id != supplierModel.Id && c.IsDeleted == false && (c.Name == supplierModel.Name )))).Any();
            if (!errorStateModel.IsValid)
            {
                errorStateModel.Errors.Add("supplier", "supplier with same code or name exists.");
            }
            return errorStateModel;
        }
    }
}
