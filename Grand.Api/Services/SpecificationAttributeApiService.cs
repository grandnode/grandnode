using Grand.Api.DTOs.Catalog;
using Grand.Api.Extensions;
using Grand.Api.Interfaces;
using Grand.Core.Data;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Services.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Api.Services
{
    public partial class SpecificationAttributeApiService : ISpecificationAttributeApiService
    {
        private readonly IMongoDBContext _mongoDBContext;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerActivityService _customerActivityService;

        private readonly IMongoCollection<SpecificationAttributeDto> _specificationAttribute;

        public SpecificationAttributeApiService(IMongoDBContext mongoDBContext, ISpecificationAttributeService specificationAttributeService,
            ILocalizationService localizationService, ICustomerActivityService customerActivityService)
        {
            _mongoDBContext = mongoDBContext;
            _specificationAttributeService = specificationAttributeService;
            _localizationService = localizationService;
            _customerActivityService = customerActivityService;

            _specificationAttribute = _mongoDBContext.Database().GetCollection<SpecificationAttributeDto>(typeof(Core.Domain.Catalog.SpecificationAttribute).Name);
        }
        public virtual Task<SpecificationAttributeDto> GetById(string id)
        {
            return _specificationAttribute.AsQueryable().FirstOrDefaultAsync(x => x.Id == id);
        }
        public virtual IMongoQueryable<SpecificationAttributeDto> GetSpecificationAttributes()
        {
            return _specificationAttribute.AsQueryable();
        }
        public virtual async Task<SpecificationAttributeDto> InsertOrUpdateSpecificationAttribute(SpecificationAttributeDto model)
        {
            if (string.IsNullOrEmpty(model.Id))
                model = await InsertSpecificationAttribute(model);
            else
                model = await UpdateSpecificationAttribute(model);

            return model;
        }
        public virtual async Task<SpecificationAttributeDto> InsertSpecificationAttribute(SpecificationAttributeDto model)
        {
            var specificationAttribute = model.ToEntity();
            await _specificationAttributeService.InsertSpecificationAttribute(specificationAttribute);

            //activity log
            await _customerActivityService.InsertActivity("AddNewSpecAttribute", specificationAttribute.Id, _localizationService.GetResource("ActivityLog.AddNewSpecAttribute"), specificationAttribute.Name);

            return specificationAttribute.ToModel();
        }

        public virtual async Task<SpecificationAttributeDto> UpdateSpecificationAttribute(SpecificationAttributeDto model)
        {
            var specificationAttribute = await _specificationAttributeService.GetSpecificationAttributeById(model.Id);
            foreach (var option in specificationAttribute.SpecificationAttributeOptions)
            {
                if (model.SpecificationAttributeOptions.FirstOrDefault(x => x.Id == option.Id) == null)
                {
                    await _specificationAttributeService.DeleteSpecificationAttributeOption(option);
                }
            }
            specificationAttribute = model.ToEntity(specificationAttribute);
            await _specificationAttributeService.UpdateSpecificationAttribute(specificationAttribute);

            //activity log
            await _customerActivityService.InsertActivity("EditSpecAttribute", specificationAttribute.Id, _localizationService.GetResource("ActivityLog.EditSpecAttribute"), specificationAttribute.Name);

            return specificationAttribute.ToModel();
        }
        public virtual async Task DeleteSpecificationAttribute(SpecificationAttributeDto model)
        {
            var specificationAttribute = await _specificationAttributeService.GetSpecificationAttributeById(model.Id);
            if (specificationAttribute != null)
            {
                await _specificationAttributeService.DeleteSpecificationAttribute(specificationAttribute);
                //activity log
                await _customerActivityService.InsertActivity("DeleteSpecAttribute", specificationAttribute.Id, _localizationService.GetResource("ActivityLog.DeleteSpecAttribute"), specificationAttribute.Name);
            }
        }
    }
}
