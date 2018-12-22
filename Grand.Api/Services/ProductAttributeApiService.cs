using Grand.Api.DTOs.Catalog;
using Grand.Api.Extensions;
using Grand.Data;
using Grand.Services.Catalog;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq;

namespace Grand.Api.Services
{
    public partial class ProductAttributeApiService : IProductAttributeApiService
    {
        private readonly IMongoDBContext _mongoDBContext;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IMongoCollection<ProductAttributeDto> _productAttribute;

        public ProductAttributeApiService(IMongoDBContext mongoDBContext, IProductAttributeService productAttributeService)
        {
            _mongoDBContext = mongoDBContext;
            _productAttributeService = productAttributeService;
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

        public virtual ProductAttributeDto InsertProductAttribute(ProductAttributeDto model)
        {
            var productAttribute = model.ToEntity();
            _productAttributeService.InsertProductAttribute(productAttribute);
            return productAttribute.ToModel();
        }

        public virtual ProductAttributeDto UpdateProductAttribute(ProductAttributeDto model)
        {
            var productAttribute = _productAttributeService.GetProductAttributeById(model.Id);
            productAttribute = model.ToEntity(productAttribute);
            _productAttributeService.UpdateProductAttribute(productAttribute);
            return productAttribute.ToModel();
        }
        public virtual void DeleteProductAttribute(ProductAttributeDto model)
        {
            var productAttribute = _productAttributeService.GetProductAttributeById(model.Id);
            if (productAttribute != null)
                _productAttributeService.DeleteProductAttribute(productAttribute);

        }


    }
}
