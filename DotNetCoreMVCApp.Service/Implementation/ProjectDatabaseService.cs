using AutoMapper;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCoreMVCApp.Models.Repository;
using DotNetCoreMVCApp.Models.Web;
using DotNetCoreMVCApp.Repository.Implementation;
using DotNetCoreMVCApp.Service.Abstraction;


namespace DotNetCoreMVCApp.Service.Implementation
{
    public class ProjectDatabaseService : IProjectDatabaseService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILog _logger;

        public ProjectDatabaseService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = LogManager.GetLogger(typeof(ProjectDatabaseService));
        }

        public async Task<IEnumerable<ProjectDatabaseViewModel>> GetAllAsync()
        {
            return _mapper.Map<List<ProjectDatabaseViewModel>>(await _unitOfWork.ProjectDatabaseRepository.GetAsync(filter: (c => c.IsDeleted == false), orderBy: (c => c.OrderBy(a => a.ProjectName))));
        }

        public async Task<ProjectDatabaseViewModel> GetByIdAsync(int id)
        {
            return _mapper.Map<ProjectDatabaseViewModel>(await _unitOfWork.ProjectDatabaseRepository.GetByIdAsync(id));
        }

        public async Task<bool> CreateAsync(ProjectDatabaseViewModel projectdatabaseModel, string userId)
        {
            _logger.Info($"Project database create request by user: {userId} : {JsonConvert.SerializeObject(projectdatabaseModel)}");
            var projectdatabase = _mapper.Map<ProjectDatabase>(projectdatabaseModel);
            projectdatabase.CreatedBy = userId;
            projectdatabase.CreatedOn = DateTime.Now;
            await _unitOfWork.ProjectDatabaseRepository.InsertAsync(projectdatabase);
            await _unitOfWork.SaveAsync();
            _logger.Info($"Created project database");
            return true;
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            _logger.Info($"Project database delete request by user: {userId} : Project database Id: {id}");
            var projectdatabase = await _unitOfWork.ProjectDatabaseRepository.GetByIdAsync(id);
            projectdatabase.IsDeleted = true;
            projectdatabase.DeletedBy = userId;
            projectdatabase.DeletedOn = DateTime.Now;

            _unitOfWork.ProjectDatabaseRepository.Update(projectdatabase);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> UpdateAsync(ProjectDatabaseViewModel projectdatabaseModel, string userId)
        {
            _logger.Info($"Project database update request by user: {userId} : {JsonConvert.SerializeObject(projectdatabaseModel)}");
            var projectdatabase = await _unitOfWork.ProjectDatabaseRepository.GetByIdAsync(projectdatabaseModel.Id);
            projectdatabase.ProjectName = projectdatabaseModel.ProjectName;
            projectdatabase.UpdatedBy = userId;
            projectdatabase.UpdatedOn = DateTime.Now;
            _unitOfWork.ProjectDatabaseRepository.Update(projectdatabase);
            await _unitOfWork.SaveAsync();
            _logger.Info($"Updated project database");
            return true;
        }

        public async Task<ErrorStateModel> ValidateAsync(ProjectDatabaseViewModel projectdatabaseModel)
        {
            // Check if project database with same name exists
            ErrorStateModel errorStateModel = new();

            errorStateModel.IsValid = !(await _unitOfWork.ProjectDatabaseRepository.GetAsync(filter: (c => c.Id != projectdatabaseModel.Id && c.IsDeleted == false && c.ProjectName == projectdatabaseModel.ProjectName))).Any();
            if (!errorStateModel.IsValid)
            {
                errorStateModel.Errors.Add("projectDatabase", "Project database with same name exists.");
            }
            return errorStateModel;
        }
    }
}
