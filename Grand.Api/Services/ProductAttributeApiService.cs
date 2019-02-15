using Grand.Api.DTOs.Catalog;
using Grand.Api.Extensions;
using Grand.Api.Interfaces;
using Grand.Data;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Services.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq;

namespace Grand.Api.Services
{
    public partial class ProductAttributeApiService : IProductAttributeApiService
    {
        private readonly IMongoDBContext _mongoDBContext;
        private readonly IProductAttributeService _productAttributeService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;

        private readonly IMongoCollection<ProductAttributeDto> _productAttribute;

        public ProductAttributeApiService(IMongoDBContext mongoDBContext, IProductAttributeService productAttributeService,
            ICustomerActivityService customerActivityService, ILocalizationService localizationService)
        {
            _mongoDBContext = mongoDBContext;
            _productAttributeService = productAttributeService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;

            _productAttribute = _mongoDBContext.Database().GetCollection<ProductAttributeDto>(typeof(Core.Domain.Catalog.ProductAttribute).Name);
        }
        public virtual ProductAttributeDto GetById(string id)
        {
            return _productAttribute.AsQueryable().FirstOrDefault(x => x.Id == id);
        }
        public virtual IMongoQueryable<ProductAttributeDto> GetProductAttributes()
        {
            return _productAttribute.AsQueryable();
        }
        public virtual ProductAttributeDto InsertOrUpdateProductAttribute(ProductAttributeDto model)
        {
            if (string.IsNullOrEmpty(model.Id))
                model = InsertProductAttribute(model);
            else
                model = UpdateProductAttribute(model);

            return model;
        }
        public virtual ProductAttributeDto InsertProductAttribute(ProductAttributeDto model)
        {
            var productAttribute = model.ToEntity();
            _productAttributeService.InsertProductAttribute(productAttribute);

            //activity log
            _customerActivityService.InsertActivity("AddNewProductAttribute", productAttribute.Id, _localizationService.GetResource("ActivityLog.AddNewProductAttribute"), productAttribute.Name);

            return productAttribute.ToModel();
        }

        public virtual ProductAttributeDto UpdateProductAttribute(ProductAttributeDto model)
        {
            var productAttribute = _productAttributeService.GetProductAttributeById(model.Id);
            productAttribute = model.ToEntity(productAttribute);
            _productAttributeService.UpdateProductAttribute(productAttribute);

            //activity log
            _customerActivityService.InsertActivity("EditProductAttribute", productAttribute.Id, _localizationService.GetResource("ActivityLog.EditProductAttribute"), productAttribute.Name);

            return productAttribute.ToModel();
        }
        public virtual void DeleteProductAttribute(ProductAttributeDto model)
        {
            var productAttribute = _productAttributeService.GetProductAttributeById(model.Id);
            if (productAttribute != null)
            {
                _productAttributeService.DeleteProductAttribute(productAttribute);

                //activity log
                _customerActivityService.InsertActivity("DeleteProductAttribute", productAttribute.Id, _localizationService.GetResource("ActivityLog.DeleteProductAttribute"), productAttribute.Name);
            }
        }


    }
}
