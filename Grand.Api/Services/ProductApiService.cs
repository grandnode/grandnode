using Grand.Api.DTOs.Catalog;
using Grand.Api.Extensions;
using Grand.Api.Interfaces;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Seo;
using Grand.Core.Data;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Media;
using Grand.Services.Seo;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Api.Services
{
    public partial class ProductApiService : IProductApiService
    {
        private readonly IMongoDBContext _mongoDBContext;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IPictureService _pictureService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IBackInStockSubscriptionService _backInStockSubscriptionService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        private readonly IMongoCollection<ProductDto> _product;
        private readonly SeoSettings _seoSettings;

        public ProductApiService(IMongoDBContext mongoDBContext, IProductService productService, ICategoryService categoryService, IManufacturerService manufacturerService,
            IPictureService pictureService, ISpecificationAttributeService specificationAttributeService, IUrlRecordService urlRecordService,
            IBackInStockSubscriptionService backInStockSubscriptionService, ICustomerActivityService customerActivityService, ILocalizationService localizationService,
            ILanguageService languageService, SeoSettings seoSettings)
        {
            _mongoDBContext = mongoDBContext;
            _productService = productService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _pictureService = pictureService;
            _specificationAttributeService = specificationAttributeService;
            _urlRecordService = urlRecordService;
            _backInStockSubscriptionService = backInStockSubscriptionService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _languageService = languageService;
            _product = _mongoDBContext.Database().GetCollection<ProductDto>(typeof(Core.Domain.Catalog.Product).Name);
            _seoSettings = seoSettings;
        }

        protected async Task BackInStockNotifications(Product product, int prevStockQuantity, List<ProductWarehouseInventory> prevMultiWarehouseStock)
        {
            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                product.BackorderMode == BackorderMode.NoBackorders &&
                product.AllowBackInStockSubscriptions &&
                product.GetTotalStockQuantity() > 0 &&
                prevStockQuantity <= 0 && !product.UseMultipleWarehouses &&
                product.Published)
            {
                await _backInStockSubscriptionService.SendNotificationsToSubscribers(product, "");
            }
            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                product.BackorderMode == BackorderMode.NoBackorders &&
                product.AllowBackInStockSubscriptions &&
                product.UseMultipleWarehouses &&
                product.Published)
            {
                foreach (var prevstock in prevMultiWarehouseStock)
                {
                    if (prevstock.StockQuantity - prevstock.ReservedQuantity <= 0)
                    {
                        var actualStock = product.ProductWarehouseInventory.FirstOrDefault(x => x.WarehouseId == prevstock.WarehouseId);
                        if (actualStock != null)
                        {
                            if (actualStock.StockQuantity - actualStock.ReservedQuantity > 0)
                                await _backInStockSubscriptionService.SendNotificationsToSubscribers(product, prevstock.WarehouseId);
                        }
                    }
                }
                if (product.ProductWarehouseInventory.Sum(x => x.StockQuantity - x.ReservedQuantity) > 0)
                {
                    if (prevMultiWarehouseStock.Sum(x => x.StockQuantity - x.ReservedQuantity) <= 0)
                    {
                        await _backInStockSubscriptionService.SendNotificationsToSubscribers(product, "");
                    }
                }
            }
        }

        public virtual Task<ProductDto> GetById(string id)
        {
            return _product.AsQueryable().FirstOrDefaultAsync(x => x.Id == id);
        }
        public virtual IMongoQueryable<ProductDto> GetProducts()
        {
            return _product.AsQueryable();
        }

        public virtual async Task<ProductDto> InsertOrUpdateProduct(ProductDto model)
        {
            if (string.IsNullOrEmpty(model.Id))
                model = await InsertProduct(model);
            else
                model = await UpdateProduct(model);

            return model;
        }

        public virtual async Task<ProductDto> InsertProduct(ProductDto model)
        {
            var product = model.ToEntity();
            product.CreatedOnUtc = DateTime.UtcNow;
            product.UpdatedOnUtc = DateTime.UtcNow;
            await _productService.InsertProduct(product);

            model.SeName = await product.ValidateSeName(model.SeName, product.Name, true, _seoSettings, _urlRecordService, _languageService);
            product.SeName = model.SeName;
            //search engine name
            await _urlRecordService.SaveSlug(product, model.SeName, "");
            await _productService.UpdateProduct(product);

            //activity log
            await _customerActivityService.InsertActivity("AddNewProduct", product.Id, _localizationService.GetResource("ActivityLog.AddNewProduct"), product.Name);

            return product.ToModel();
        }

        public virtual async Task<ProductDto> UpdateProduct(ProductDto model)
        {
            //product
            var product = await _productService.GetProductById(model.Id);
            product = model.ToEntity(product);
            product.UpdatedOnUtc = DateTime.UtcNow;
            model.SeName = await product.ValidateSeName(model.SeName, product.Name, true, _seoSettings, _urlRecordService, _languageService);
            product.SeName = model.SeName;
            //search engine name
            await _urlRecordService.SaveSlug(product, model.SeName, "");
            //update product
            await _productService.UpdateProduct(product);

            //activity log
            await _customerActivityService.InsertActivity("EditProduct", product.Id, _localizationService.GetResource("ActivityLog.EditProduct"), product.Name);

            return product.ToModel();
        }
        public virtual async Task DeleteProduct(ProductDto model)
        {
            var product = await _productService.GetProductById(model.Id);
            if (product != null)
            {
                await _productService.DeleteProduct(product);

                //activity log
                await _customerActivityService.InsertActivity("DeleteProduct", product.Id, _localizationService.GetResource("ActivityLog.DeleteProduct"), product.Name);
            }
        }

        public virtual async Task UpdateStock(ProductDto model, string warehouseId, int stock)
        {
            var product = await _productService.GetProductById(model.Id);
            if (product != null)
            {
                var prevStockQuantity = product.GetTotalStockQuantity();
                var prevMultiWarehouseStock = product.ProductWarehouseInventory.Select(i => new ProductWarehouseInventory() { WarehouseId = i.WarehouseId, StockQuantity = i.StockQuantity, ReservedQuantity = i.ReservedQuantity }).ToList();

                if (string.IsNullOrEmpty(warehouseId))
                {
                    product.StockQuantity = stock;
                    await _productService.UpdateProduct(product);
                }
                else
                {
                    if (product.UseMultipleWarehouses)
                    {
                        var existingPwI = product.ProductWarehouseInventory.FirstOrDefault(x => x.WarehouseId == warehouseId);
                        if (existingPwI != null)
                        {
                            existingPwI.StockQuantity = stock;
                            existingPwI.ProductId = product.Id;
                            await _productService.UpdateProductWarehouseInventory(existingPwI);
                        }
                        else
                        {
                            var newPwI = new ProductWarehouseInventory
                            {
                                WarehouseId = warehouseId,
                                ProductId = product.Id,
                                StockQuantity = stock,
                                ReservedQuantity = 0
                            };
                            await _productService.InsertProductWarehouseInventory(newPwI);
                        }
                    }
                    else
                    {
                        throw new ArgumentException("Product don't support multiple warehouses (warehouseId should be null or empty)");
                    }
                }
                await BackInStockNotifications(product, prevStockQuantity, prevMultiWarehouseStock);

                //activity log
                await _customerActivityService.InsertActivity("EditProduct", product.Id, _localizationService.GetResource("ActivityLog.EditProduct"), product.Name);

            }
        }

        public virtual async Task InsertProductCategory(ProductDto product, ProductCategoryDto model)
        {
            var productCategory = new ProductCategory
            {
                ProductId = product.Id,
                CategoryId = model.CategoryId,
                IsFeaturedProduct = model.IsFeaturedProduct,
            };
            await _categoryService.InsertProductCategory(productCategory);
        }
        public virtual async Task UpdateProductCategory(ProductDto product, ProductCategoryDto model)
        {
            var productdb = await _productService.GetProductById(product.Id);
            var productCategory = productdb.ProductCategories.Where(x => x.CategoryId == model.CategoryId).FirstOrDefault();
            if (productCategory == null)
                throw new ArgumentException("No product category mapping found with the specified id");

            productCategory.CategoryId = model.CategoryId;
            productCategory.ProductId = product.Id;
            productCategory.IsFeaturedProduct = model.IsFeaturedProduct;
            await _categoryService.UpdateProductCategory(productCategory);
        }
        public virtual async Task DeleteProductCategory(ProductDto product, string categoryId)
        {
            var productdb = await _productService.GetProductById(product.Id);
            var productCategory = productdb.ProductCategories.Where(x => x.CategoryId == categoryId).FirstOrDefault();
            if (productCategory == null)
                throw new ArgumentException("No product category mapping found with the specified id");

            productCategory.ProductId = product.Id;
            await _categoryService.DeleteProductCategory(productCategory);
        }
        public virtual async Task InsertProductManufacturer(ProductDto product, ProductManufacturerDto model)
        {
            var productManufacturer = new ProductManufacturer
            {
                ProductId = product.Id,
                ManufacturerId = model.ManufacturerId,
                IsFeaturedProduct = model.IsFeaturedProduct,
            };
            await _manufacturerService.InsertProductManufacturer(productManufacturer);
        }
        public virtual async Task UpdateProductManufacturer(ProductDto product, ProductManufacturerDto model)
        {
            var productdb = await _productService.GetProductById(product.Id);
            var productManufacturer = productdb.ProductManufacturers.Where(x => x.ManufacturerId == model.ManufacturerId).FirstOrDefault();
            if (productManufacturer == null)
                throw new ArgumentException("No product manufacturer mapping found with the specified id");

            productManufacturer.ManufacturerId = model.ManufacturerId;
            productManufacturer.ProductId = product.Id;
            productManufacturer.IsFeaturedProduct = model.IsFeaturedProduct;
            await _manufacturerService.UpdateProductManufacturer(productManufacturer);
        }
        public virtual async Task DeleteProductManufacturer(ProductDto product, string manufacturerId)
        {
            var productdb = await _productService.GetProductById(product.Id);
            var productManufacturer = productdb.ProductManufacturers.Where(x => x.ManufacturerId == manufacturerId).FirstOrDefault();
            if (productManufacturer == null)
                throw new ArgumentException("No product manufacturer mapping found with the specified id");

            productManufacturer.ProductId = product.Id;
            await _manufacturerService.DeleteProductManufacturer(productManufacturer);
        }

        public virtual async Task InsertProductPicture(ProductDto product, ProductPictureDto model)
        {
            var picture = await _pictureService.GetPictureById(model.PictureId);
            if (picture != null)
            {
                await _pictureService.UpdatePicture(picture.Id, await _pictureService.LoadPictureBinary(picture),
                picture.MimeType,
                picture.SeoFilename,
                model.AltAttribute,
                model.TitleAttribute);

                await _productService.InsertProductPicture(new ProductPicture
                {
                    PictureId = model.PictureId,
                    ProductId = product.Id,
                    DisplayOrder = model.DisplayOrder,
                    AltAttribute = model.AltAttribute,
                    MimeType = picture.MimeType,
                    SeoFilename = model.SeoFilename,
                    TitleAttribute = model.TitleAttribute
                });
                await _pictureService.SetSeoFilename(model.PictureId, _pictureService.GetPictureSeName(product.Name));
            }
        }
        public virtual async Task UpdateProductPicture(ProductDto product, ProductPictureDto model)
        {
            var productdb = await _productService.GetProductById(product.Id);

            var productPicture = productdb.ProductPictures.Where(x => x.PictureId == model.PictureId).FirstOrDefault();
            if (productPicture == null)
                throw new ArgumentException("No product picture found with the specified id");
            productPicture.ProductId = product.Id;

            var picture = await _pictureService.GetPictureById(productPicture.PictureId);
            if (picture == null)
                throw new ArgumentException("No picture found with the specified id");

            productPicture.DisplayOrder = model.DisplayOrder;
            productPicture.MimeType = picture.MimeType;
            productPicture.SeoFilename = model.SeoFilename;
            productPicture.AltAttribute = model.AltAttribute;
            productPicture.TitleAttribute = model.TitleAttribute;

            await _productService.UpdateProductPicture(productPicture);

            await _pictureService.UpdatePicture(picture.Id,
                await _pictureService.LoadPictureBinary(picture),
                picture.MimeType,
                model.SeoFilename,
                model.AltAttribute,
                model.TitleAttribute);
        }
        public virtual async Task DeleteProductPicture(ProductDto product, string pictureId)
        {
            var productdb = await _productService.GetProductById(product.Id);

            var productPicture = productdb.ProductPictures.Where(x => x.PictureId == pictureId).FirstOrDefault();
            if (productPicture == null)
                throw new ArgumentException("No product picture found with the specified pictureid");
            productPicture.ProductId = product.Id;
            await _productService.DeleteProductPicture(productPicture);
        }

        public virtual async Task InsertProductSpecification(ProductDto product, ProductSpecificationAttributeDto model)
        {
            //we allow filtering only for "Option" attribute type
            if (model.AttributeType != SpecificationAttributeType.Option)
            {
                model.AllowFiltering = false;
                model.SpecificationAttributeOptionId = null;
            }

            var psa = new ProductSpecificationAttribute
            {
                AttributeTypeId = (int)model.AttributeType,
                SpecificationAttributeOptionId = model.SpecificationAttributeOptionId,
                SpecificationAttributeId = model.SpecificationAttributeId,
                ProductId = product.Id,
                CustomValue = model.CustomValue,
                AllowFiltering = model.AllowFiltering,
                ShowOnProductPage = model.ShowOnProductPage,
                DisplayOrder = model.DisplayOrder,
            };
            await _specificationAttributeService.InsertProductSpecificationAttribute(psa);
        }
        public virtual async Task UpdateProductSpecification(ProductDto product, ProductSpecificationAttributeDto model)
        {
            var productdb = await _productService.GetProductById(product.Id);
            var psa = productdb.ProductSpecificationAttributes.FirstOrDefault(x => x.Id == model.Id);
            if (psa != null)
            {
                if (model.AttributeType == SpecificationAttributeType.Option)
                {
                    psa.AllowFiltering = model.AllowFiltering;
                    psa.SpecificationAttributeOptionId = model.SpecificationAttributeOptionId;
                }
                else
                {
                    psa.CustomValue = model.CustomValue;
                }
                psa.SpecificationAttributeId = model.SpecificationAttributeId;
                psa.SpecificationAttributeOptionId = model.SpecificationAttributeOptionId;
                psa.AttributeTypeId = (int)model.AttributeType;
                psa.ShowOnProductPage = model.ShowOnProductPage;
                psa.DisplayOrder = model.DisplayOrder;
                psa.ProductId = product.Id;
                await _specificationAttributeService.UpdateProductSpecificationAttribute(psa);
            }
        }
        public virtual async Task DeleteProductSpecification(ProductDto product, string id)
        {
            var productdb = await _productService.GetProductById(product.Id);
            var psa = productdb.ProductSpecificationAttributes.FirstOrDefault(x => x.Id == id);
            if (psa != null)
            {
                psa.ProductId = product.Id;
                await _specificationAttributeService.DeleteProductSpecificationAttribute(psa);
            }
        }

        public virtual async Task InsertProductTierPrice(ProductDto product, ProductTierPriceDto model)
        {
            var tierPrice = model.ToEntity();
            tierPrice.ProductId = product.Id;
            await _productService.InsertTierPrice(tierPrice);

        }

        public virtual async Task UpdateProductTierPrice(ProductDto product, ProductTierPriceDto model)
        {
            var productdb = await _productService.GetProductById(product.Id);
            var tierPrice = model.ToEntity();
            tierPrice.ProductId = product.Id;
            await _productService.UpdateTierPrice(tierPrice);
        }

        public virtual async Task DeleteProductTierPrice(ProductDto product, string id)
        {
            var productdb = await _productService.GetProductById(product.Id);
            var tierPrice = productdb.TierPrices.Where(x => x.Id == id).FirstOrDefault();
            tierPrice.ProductId = product.Id;
            await _productService.DeleteTierPrice(tierPrice);
        }
    }
}
