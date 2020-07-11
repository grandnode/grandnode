using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Domain.Orders;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Services.Seo;
using Grand.Web.Features.Models.ShoppingCart;
using Grand.Web.Models.Catalog;
using Grand.Web.Models.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Grand.Web.Controllers
{
    public partial class BackInStockSubscriptionController : BasePublicController
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly IBackInStockSubscriptionService _backInStockSubscriptionService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IMediator _mediator;
        private readonly CatalogSettings _catalogSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;

        #endregion

        #region Constructors

        public BackInStockSubscriptionController(IProductService productService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            IBackInStockSubscriptionService backInStockSubscriptionService,
            IProductAttributeFormatter productAttributeFormatter,
            IMediator mediator,
            CatalogSettings catalogSettings,
            CustomerSettings customerSettings,
            ShoppingCartSettings shoppingCartSettings)
        {
            _productService = productService;
            _workContext = workContext;
            _storeContext = storeContext;
            _localizationService = localizationService;
            _backInStockSubscriptionService = backInStockSubscriptionService;
            _productAttributeFormatter = productAttributeFormatter;
            _mediator = mediator;
            _catalogSettings = catalogSettings;
            _customerSettings = customerSettings;
            _shoppingCartSettings = shoppingCartSettings;
        }

        #endregion

        #region Methods

        // Product details page > back in stock subscribe button
        public virtual async Task<IActionResult> SubscribeButton(string productId, string warehouseId)
        {
            var product = await _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var customer = _workContext.CurrentCustomer;
            if (!customer.IsRegistered())
                return Content(_localizationService.GetResource("BackInStockSubscriptions.NotifyMeWhenAvailable"));

            if (product.ManageInventoryMethod != ManageInventoryMethod.ManageStock)
                return Content(_localizationService.GetResource("BackInStockSubscriptions.NotifyMeWhenAvailable"));

            warehouseId = _shoppingCartSettings.AllowToSelectWarehouse ?
               (string.IsNullOrEmpty(warehouseId) ? "" : warehouseId) :
               (string.IsNullOrEmpty(_storeContext.CurrentStore.DefaultWarehouseId) ? product.WarehouseId : _storeContext.CurrentStore.DefaultWarehouseId);

            var subscription = await _backInStockSubscriptionService
                   .FindSubscription(customer.Id, product.Id, string.Empty, _storeContext.CurrentStore.Id,
                   warehouseId);

            if (subscription != null)
            {
                return Content(_localizationService.GetResource("BackInStockSubscriptions.DeleteNotifyWhenAvailable"));
            }
            return Content(_localizationService.GetResource("BackInStockSubscriptions.NotifyMeWhenAvailable"));
        }

        [HttpPost, ActionName("SubscribePopup")]
        public virtual async Task<IActionResult> SubscribePopup(string productId, IFormCollection form)
        {
            var product = await _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var customer = _workContext.CurrentCustomer;

            string warehouseId = _shoppingCartSettings.AllowToSelectWarehouse ?
                form["WarehouseId"].ToString() :
                 product.UseMultipleWarehouses ? _storeContext.CurrentStore.DefaultWarehouseId :
                 (string.IsNullOrEmpty(_storeContext.CurrentStore.DefaultWarehouseId) ? product.WarehouseId : _storeContext.CurrentStore.DefaultWarehouseId);

            if (!customer.IsRegistered())
                return Json(new
                {
                    subscribe = false,
                    buttontext = _localizationService.GetResource("BackInStockSubscriptions.NotifyMeWhenAvailable"),
                    resource = _localizationService.GetResource("BackInStockSubscriptions.OnlyRegistered")
                });

            if ((product.ManageInventoryMethod == ManageInventoryMethod.ManageStock) &&
                product.BackorderMode == BackorderMode.NoBackorders &&
                product.AllowBackInStockSubscriptions &&
                product.GetTotalStockQuantity(warehouseId: warehouseId) <= 0)
            {
                var subscription = await _backInStockSubscriptionService
                    .FindSubscription(customer.Id, product.Id, string.Empty, _storeContext.CurrentStore.Id, warehouseId);
                if (subscription != null)
                {
                    //subscription already exists
                    //unsubscribe
                    await _backInStockSubscriptionService.DeleteSubscription(subscription);
                    return Json(new
                    {
                        subscribe = false,
                        buttontext = _localizationService.GetResource("BackInStockSubscriptions.NotifyMeWhenAvailable"),
                        resource = _localizationService.GetResource("BackInStockSubscriptions.Unsubscribed")
                    });

                }

                //subscription does not exist
                //subscribe
                if ((await _backInStockSubscriptionService
                    .GetAllSubscriptionsByCustomerId(customer.Id, _storeContext.CurrentStore.Id, 0, 1))
                    .TotalCount >= _catalogSettings.MaximumBackInStockSubscriptions)
                {
                    return Json(new
                    {
                        subscribe = false,
                        buttontext = _localizationService.GetResource("BackInStockSubscriptions.NotifyMeWhenAvailable"),
                        resource = string.Format(_localizationService.GetResource("BackInStockSubscriptions.MaxSubscriptions"), _catalogSettings.MaximumBackInStockSubscriptions)
                    });
                }
                subscription = new BackInStockSubscription {
                    CustomerId = customer.Id,
                    ProductId = product.Id,
                    StoreId = _storeContext.CurrentStore.Id,
                    WarehouseId = warehouseId,
                    CreatedOnUtc = DateTime.UtcNow
                };
                await _backInStockSubscriptionService.InsertSubscription(subscription);
                return Json(new
                {
                    subscribe = true,
                    buttontext = _localizationService.GetResource("BackInStockSubscriptions.DeleteNotifyWhenAvailable"),
                    resource = _localizationService.GetResource("BackInStockSubscriptions.Subscribed")
                });
            }

            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes &&
                product.BackorderMode == BackorderMode.NoBackorders &&
                product.AllowBackInStockSubscriptions)
            {
                string attributeXml = await _mediator.Send(new GetParseProductAttributes() { Product = product, Form = form });
                var subscription = await _backInStockSubscriptionService
                    .FindSubscription(customer.Id, product.Id, attributeXml, _storeContext.CurrentStore.Id, warehouseId);

                if (subscription != null)
                {
                    //subscription already exists
                    //unsubscribe
                    await _backInStockSubscriptionService.DeleteSubscription(subscription);
                    return Json(new
                    {
                        subscribe = false,
                        buttontext = _localizationService.GetResource("BackInStockSubscriptions.NotifyMeWhenAvailable"),
                        resource = _localizationService.GetResource("BackInStockSubscriptions.Unsubscribed")
                    });
                }

                //subscription does not exist
                //subscribe
                if ((await _backInStockSubscriptionService
                    .GetAllSubscriptionsByCustomerId(customer.Id, _storeContext.CurrentStore.Id, 0, 1))
                    .TotalCount >= _catalogSettings.MaximumBackInStockSubscriptions)
                {
                    return Json(new
                    {
                        subscribe = false,
                        buttontext = _localizationService.GetResource("BackInStockSubscriptions.NotifyMeWhenAvailable"),
                        resource = string.Format(_localizationService.GetResource("BackInStockSubscriptions.MaxSubscriptions"), _catalogSettings.MaximumBackInStockSubscriptions)
                    });
                }

                subscription = new BackInStockSubscription {
                    CustomerId = customer.Id,
                    ProductId = product.Id,
                    AttributeXml = attributeXml,
                    StoreId = _storeContext.CurrentStore.Id,
                    WarehouseId = warehouseId,
                    CreatedOnUtc = DateTime.UtcNow
                };

                await _backInStockSubscriptionService.InsertSubscription(subscription);
                return Json(new
                {
                    subscribe = true,
                    buttontext = _localizationService.GetResource("BackInStockSubscriptions.DeleteNotifyWhenAvailable"),
                    resource = _localizationService.GetResource("BackInStockSubscriptions.Subscribed")
                });
            }

            return Json(new
            {
                subscribe = false,
                buttontext = _localizationService.GetResource("BackInStockSubscriptions.NotifyMeWhenAvailable"),
                resource = _localizationService.GetResource("BackInStockSubscriptions.NotAllowed")
            });
        }


        // My account / Back in stock subscriptions
        public virtual async Task<IActionResult> CustomerSubscriptions(int? pageNumber)
        {
            if (_customerSettings.HideBackInStockSubscriptionsTab)
            {
                return RedirectToRoute("CustomerInfo");
            }

            int pageIndex = 0;
            if (pageNumber > 0)
            {
                pageIndex = pageNumber.Value - 1;
            }
            var pageSize = 10;

            var customer = _workContext.CurrentCustomer;
            var list = await _backInStockSubscriptionService.GetAllSubscriptionsByCustomerId(customer.Id,
                _storeContext.CurrentStore.Id, pageIndex, pageSize);

            var model = new CustomerBackInStockSubscriptionsModel();

            foreach (var subscription in list)
            {
                var product = await _productService.GetProductById(subscription.ProductId);
                if (product != null)
                {
                    var subscriptionModel = new CustomerBackInStockSubscriptionsModel.BackInStockSubscriptionModel {
                        Id = subscription.Id,
                        ProductId = product.Id,
                        ProductName = product.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                        AttributeDescription = string.IsNullOrEmpty(subscription.AttributeXml) ? "" : await _productAttributeFormatter.FormatAttributes(product, subscription.AttributeXml),
                        SeName = product.GetSeName(_workContext.WorkingLanguage.Id),
                    };
                    model.Subscriptions.Add(subscriptionModel);
                }
            }

            model.PagerModel = new PagerModel(_localizationService) {
                PageSize = list.PageSize,
                TotalRecords = list.TotalCount,
                PageIndex = list.PageIndex,
                ShowTotalSummary = false,
                RouteActionName = "CustomerBackInStockSubscriptionsPaged",
                UseRouteLinks = true,
                RouteValues = new BackInStockSubscriptionsRouteValues { pageNumber = pageIndex }
            };

            return View(model);
        }
        [HttpPost, ActionName("CustomerSubscriptions")]
        public virtual async Task<IActionResult> CustomerSubscriptionsPOST(IFormCollection formCollection)
        {
            foreach (var key in formCollection.Keys)
            {
                var value = formCollection[key];

                if (value.Equals("on") && key.StartsWith("biss", StringComparison.OrdinalIgnoreCase))
                {
                    var id = key.Replace("biss", "").Trim();
                    var subscription = await _backInStockSubscriptionService.GetSubscriptionById(id);
                    if (subscription != null && subscription.CustomerId == _workContext.CurrentCustomer.Id)
                    {
                        await _backInStockSubscriptionService.DeleteSubscription(subscription);
                    }
                }
            }

            return RedirectToRoute("CustomerBackInStockSubscriptions");
        }

        #endregion
    }
}
