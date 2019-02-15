using Grand.Api.DTOs.Catalog;
using Grand.Api.Extensions;
using Grand.Api.Interfaces;
using Grand.Core.Domain.Catalog;
using Grand.Data;
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

        private readonly IMongoCollection<ProductDto> _product;

        public ProductApiService(IMongoDBContext mongoDBContext, IProductService productService, ICategoryService categoryService, IManufacturerService manufacturerService,
            IPictureService pictureService, ISpecificationAttributeService specificationAttributeService, IUrlRecordService urlRecordService,
            IBackInStockSubscriptionService backInStockSubscriptionService, ICustomerActivityService customerActivityService, ILocalizationService localizationService)
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

            _product = _mongoDBContext.Database().GetCollection<ProductDto>(typeof(Core.Domain.Catalog.Product).Name);
        }

        protected void BackInStockNotifications(Product product, int prevStockQuantity, List<ProductWarehouseInventory> prevMultiWarehouseStock)
        {
            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                product.BackorderMode == BackorderMode.NoBackorders &&
                product.AllowBackInStockSubscriptions &&
                product.GetTotalStockQuantity() > 0 &&
                prevStockQuantity <= 0 && !product.UseMultipleWarehouses &&
                product.Published)
            {
                _backInStockSubscriptionService.SendNotificationsToSubscribers(product, "");
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
                                _backInStockSubscriptionService.SendNotificationsToSubscribers(product, prevstock.WarehouseId);
                        }
                    }
                }
                if (product.ProductWarehouseInventory.Sum(x => x.StockQuantity - x.ReservedQuantity) > 0)
                {
                    if (prevMultiWarehouseStock.Sum(x => x.StockQuantity - x.ReservedQuantity) <= 0)
                    {
                        _backInStockSubscriptionService.SendNotificationsToSubscribers(product, "");
                    }
                }
            }
        }

        public virtual ProductDto GetById(string id)
        {
            return _product.AsQueryable().FirstOrDefault(x => x.Id == id);
        }
        public virtual IMongoQueryable<ProductDto> GetProducts()
        {
            return _product.AsQueryable();
        }

        public virtual ProductDto InsertOrUpdateProduct(ProductDto model)
        {
            if (string.IsNullOrEmpty(model.Id))
                model = InsertProduct(model);
            else
                model = UpdateProduct(model);

            return model;
        }

        public virtual ProductDto InsertProduct(ProductDto model)
        {
            var product = model.ToEntity();
            product.CreatedOnUtc = DateTime.UtcNow;
            product.UpdatedOnUtc = DateTime.UtcNow;
            _productService.InsertProduct(product);

            model.SeName = product.ValidateSeName(model.SeName, product.Name, true);
            product.SeName = model.SeName;
            //search engine name
            _urlRecordService.SaveSlug(product, model.SeName, "");
            _productService.UpdateProduct(product);

            //activity log
            _customerActivityService.InsertActivity("AddNewProduct", product.Id, _localizationService.GetResource("ActivityLog.AddNewProduct"), product.Name);

            return product.ToModel();
        }

        public virtual ProductDto UpdateProduct(ProductDto model)
        {
            //product
            var product = _productService.GetProductById(model.Id);
            product = model.ToEntity(product);
            product.UpdatedOnUtc = DateTime.UtcNow;
            model.SeName = product.ValidateSeName(model.SeName, product.Name, true);
            product.SeName = model.SeName;
            //search engine name
            _urlRecordService.SaveSlug(product, model.SeName, "");
            //update product
            _productService.UpdateProduct(product);

            //activity log
            _customerActivityService.InsertActivity("EditProduct", product.Id, _localizationService.GetResource("ActivityLog.EditProduct"), product.Name);

            return product.ToModel();
        }
        public virtual void DeleteProduct(ProductDto model)
        {
            var product = _productService.GetProductById(model.Id);
            if (product != null)
            {
                _productService.DeleteProduct(product);

                //activity log
                _customerActivityService.InsertActivity("DeleteProduct", product.Id, _localizationService.GetResource("ActivityLog.DeleteProduct"), product.Name);

            }
        }

        public virtual void UpdateStock(ProductDto model, string warehouseId, int stock)
        {
            var product = _productService.GetProductById(model.Id);
            if (product != null)
            {
                var prevStockQuantity = product.GetTotalStockQuantity();
                var prevMultiWarehouseStock = product.ProductWarehouseInventory.Select(i => new ProductWarehouseInventory() { WarehouseId = i.WarehouseId, StockQuantity = i.StockQuantity, ReservedQuantity = i.ReservedQuantity }).ToList();

                if (string.IsNullOrEmpty(warehouseId))
                {
                    product.StockQuantity = stock;
                    _productService.UpdateProduct(product);
                }
                else
                {
                    if (product.UseMultipleWarehouses)
                    {
                        var existingPwI = product.ProductWarehouseInventory.FirstOrDefault(x => x.WarehouseId == warehouseId);
                        if (existingPwI != null)
                        {
                            existingPwI.StockQuantity = stock;
                            _productService.UpdateProductWarehouseInventory(existingPwI);
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
                            _productService.InsertProductWarehouseInventory(newPwI);
                        }
                    }
                }
                BackInStockNotifications(product, prevStockQuantity, prevMultiWarehouseStock);

                //activity log
                _customerActivityService.InsertActivity("EditProduct", product.Id, _localizationService.GetResource("ActivityLog.EditProduct"), product.Name);

            }
        }

        public virtual void InsertProductCategory(ProductDto product, ProductCategoryDto model)
        {
            var productCategory = new ProductCategory
            {
                ProductId = product.Id,
                CategoryId = model.CategoryId,
                IsFeaturedProduct = model.IsFeaturedProduct,
            };
            _categoryService.InsertProductCategory(productCategory);
        }
        public virtual void UpdateProductCategory(ProductDto product, ProductCategoryDto model)
        {
            var productdb = _productService.GetProductById(product.Id);
            var productCategory = productdb.ProductCategories.Where(x => x.CategoryId == model.CategoryId).FirstOrDefault();
            if (productCategory == null)
                throw new ArgumentException("No product category mapping found with the specified id");

            productCategory.CategoryId = model.CategoryId;
            productCategory.ProductId = product.Id;
            productCategory.IsFeaturedProduct = model.IsFeaturedProduct;
            _categoryService.UpdateProductCategory(productCategory);
        }
        public virtual void DeleteProductCategory(ProductDto product, string categoryId)
        {
            var productdb = _productService.GetProductById(product.Id);
            var productCategory = productdb.ProductCategories.Where(x => x.CategoryId == categoryId).FirstOrDefault();
            if (productCategory == null)
                throw new ArgumentException("No product category mapping found with the specified id");

            productCategory.ProductId = product.Id;
            _categoryService.DeleteProductCategory(productCategory);
        }
        public virtual void InsertProductManufacturer(ProductDto product, ProductManufacturerDto model)
        {
            var productManufacturer = new ProductManufacturer
            {
                ProductId = product.Id,
                ManufacturerId = model.ManufacturerId,
                IsFeaturedProduct = model.IsFeaturedProduct,
            };
            _manufacturerService.InsertProductManufacturer(productManufacturer);
        }
        public virtual void UpdateProductManufacturer(ProductDto product, ProductManufacturerDto model)
        {
            var productdb = _productService.GetProductById(product.Id);
            var productManufacturer = productdb.ProductManufacturers.Where(x => x.ManufacturerId == model.ManufacturerId).FirstOrDefault();
            if (productManufacturer == null)
                throw new ArgumentException("No product manufacturer mapping found with the specified id");

            productManufacturer.ManufacturerId = model.ManufacturerId;
            productManufacturer.ProductId = product.Id;
            productManufacturer.IsFeaturedProduct = model.IsFeaturedProduct;
            _manufacturerService.UpdateProductManufacturer(productManufacturer);
        }
        public virtual void DeleteProductManufacturer(ProductDto product, string manufacturerId)
        {
            var productdb = _productService.GetProductById(product.Id);
            var productManufacturer = productdb.ProductManufacturers.Where(x => x.ManufacturerId == manufacturerId).FirstOrDefault();
            if (productManufacturer == null)
                throw new ArgumentException("No product manufacturer mapping found with the specified id");

            productManufacturer.ProductId = product.Id;
            _manufacturerService.DeleteProductManufacturer(productManufacturer);
        }

        public virtual void InsertProductPicture(ProductDto product, ProductPictureDto model)
        {
            var picture = _pictureService.GetPictureById(model.PictureId);
            if (picture != null)
            {
                _pictureService.UpdatePicture(picture.Id, _pictureService.LoadPictureBinary(picture),
                picture.MimeType,
                picture.SeoFilename,
                model.AltAttribute,
                model.TitleAttribute);

                _productService.InsertProductPicture(new ProductPicture
                {
                    PictureId = model.PictureId,
                    ProductId = product.Id,
                    DisplayOrder = model.DisplayOrder,
                    AltAttribute = model.AltAttribute,
                    MimeType = picture.MimeType,
                    SeoFilename = model.SeoFilename,
                    TitleAttribute = model.TitleAttribute
                });
                _pictureService.SetSeoFilename(model.PictureId, _pictureService.GetPictureSeName(product.Name));
            }
        }
        public virtual void UpdateProductPicture(ProductDto product, ProductPictureDto model)
        {
            var productdb = _productService.GetProductById(product.Id);

            var productPicture = productdb.ProductPictures.Where(x => x.PictureId == model.PictureId).FirstOrDefault();
            if (productPicture == null)
                throw new ArgumentException("No product picture found with the specified id");
            productPicture.ProductId = product.Id;

            var picture = _pictureService.GetPictureById(productPicture.PictureId);
            if (picture == null)
                throw new ArgumentException("No picture found with the specified id");

            productPicture.DisplayOrder = model.DisplayOrder;
            productPicture.MimeType = picture.MimeType;
            productPicture.SeoFilename = model.SeoFilename;
            productPicture.AltAttribute = model.AltAttribute;
            productPicture.TitleAttribute = model.TitleAttribute;

            _productService.UpdateProductPicture(productPicture);

            _pictureService.UpdatePicture(picture.Id,
                _pictureService.LoadPictureBinary(picture),
                picture.MimeType,
                model.SeoFilename,
                model.AltAttribute,
                model.TitleAttribute);
        }
        public virtual void DeleteProductPicture(ProductDto product, string pictureId)
        {
            var productdb = _productService.GetProductById(product.Id);

            var productPicture = productdb.ProductPictures.Where(x => x.PictureId == pictureId).FirstOrDefault();
            if (productPicture == null)
                throw new ArgumentException("No product picture found with the specified pictureid");
            productPicture.ProductId = product.Id;
            _productService.DeleteProductPicture(productPicture);
        }

        public virtual void InsertProductSpecification(ProductDto product, ProductSpecificationAttributeDto model)
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
            _specificationAttributeService.InsertProductSpecificationAttribute(psa);
        }
        public virtual void UpdateProductSpecification(ProductDto product, ProductSpecificationAttributeDto model)
        {
            var productdb = _productService.GetProductById(product.Id);
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
                _specificationAttributeService.UpdateProductSpecificationAttribute(psa);
            }
        }
        public virtual void DeleteProductSpecification(ProductDto product, string id)
        {
            var productdb = _productService.GetProductById(product.Id);
            var psa = productdb.ProductSpecificationAttributes.FirstOrDefault(x => x.Id == id);
            if (psa != null)
            {
                psa.ProductId = product.Id;
                _specificationAttributeService.DeleteProductSpecificationAttribute(psa);
            }
        }
    }
}
