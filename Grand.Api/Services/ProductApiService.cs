using Grand.Api.DTOs.Catalog;
using Grand.Api.Extensions;
using Grand.Core.Domain.Catalog;
using Grand.Data;
using Grand.Services.Catalog;
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
        private readonly IUrlRecordService _urlRecordService;
        private readonly IBackInStockSubscriptionService _backInStockSubscriptionService;

        private readonly IMongoCollection<ProductDto> _product;


        public ProductApiService(IMongoDBContext mongoDBContext, IProductService productService, IUrlRecordService urlRecordService, 
            IBackInStockSubscriptionService backInStockSubscriptionService)
        {
            _mongoDBContext = mongoDBContext;
            _productService = productService;
            _urlRecordService = urlRecordService;
            _backInStockSubscriptionService = backInStockSubscriptionService;
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

            _productService.UpdateProduct(product);
            return product.ToModel();
        }
        public virtual void DeleteProduct(ProductDto model)
        {
            var product = _productService.GetProductById(model.Id);
            if (product != null)
                _productService.DeleteProduct(product);
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
            }
        }

    }
}
