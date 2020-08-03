using Grand.Domain.Catalog;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Services.Logging;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Api.Commands.Models.Catalog
{
    public class UpdateProductStockCommandHandler : IRequestHandler<UpdateProductStockCommand, bool>
    {
        private readonly IProductService _productService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly IBackInStockSubscriptionService _backInStockSubscriptionService;

        public UpdateProductStockCommandHandler(
            IProductService productService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IBackInStockSubscriptionService backInStockSubscriptionService)
        {
            _productService = productService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _backInStockSubscriptionService = backInStockSubscriptionService;
        }

        public async Task<bool> Handle(UpdateProductStockCommand request, CancellationToken cancellationToken)
        {
            var product = await _productService.GetProductById(request.Product.Id);
            if (product != null)
            {
                var prevStockQuantity = product.GetTotalStockQuantity();
                var prevMultiWarehouseStock = product.ProductWarehouseInventory.Select(i => new ProductWarehouseInventory() { WarehouseId = i.WarehouseId, StockQuantity = i.StockQuantity, ReservedQuantity = i.ReservedQuantity }).ToList();

                if (string.IsNullOrEmpty(request.WarehouseId))
                {
                    product.StockQuantity = request.Stock;
                    await _productService.UpdateStockProduct(product, false);
                }
                else
                {
                    if (product.UseMultipleWarehouses)
                    {
                        var existingPwI = product.ProductWarehouseInventory.FirstOrDefault(x => x.WarehouseId == request.WarehouseId);
                        if (existingPwI != null)
                        {
                            existingPwI.StockQuantity = request.Stock;
                            existingPwI.ProductId = product.Id;
                            await _productService.UpdateProductWarehouseInventory(existingPwI);
                        }
                        else
                        {
                            var newPwI = new ProductWarehouseInventory {
                                WarehouseId = request.WarehouseId,
                                ProductId = product.Id,
                                StockQuantity = request.Stock,
                                ReservedQuantity = 0
                            };
                            await _productService.InsertProductWarehouseInventory(newPwI);
                        }

                        product.StockQuantity = product.ProductWarehouseInventory.Sum(x => x.StockQuantity);
                        await _productService.UpdateStockProduct(product, false);
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
            return true;
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



    }
}
