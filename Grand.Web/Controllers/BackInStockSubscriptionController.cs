using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Services.Seo;
using Grand.Web.Models.Catalog;
using Grand.Web.Models.Common;
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
        private readonly CatalogSettings _catalogSettings;
        private readonly CustomerSettings _customerSettings;
        
        #endregion

		#region Constructors

        public BackInStockSubscriptionController(IProductService productService,
            IWorkContext workContext, 
            IStoreContext storeContext,
            ILocalizationService localizationService,
            IBackInStockSubscriptionService backInStockSubscriptionService,
            CatalogSettings catalogSettings,
            CustomerSettings customerSettings)
        {
            this._productService = productService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._localizationService = localizationService;
            this._backInStockSubscriptionService = backInStockSubscriptionService;
            this._catalogSettings = catalogSettings;
            this._customerSettings = customerSettings;
        }

        #endregion

        #region Methods

        // Product details page > back in stock subscribe
        public virtual async Task<IActionResult> SubscribePopup(string productId)
        {
            var product = await _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var customer = _workContext.CurrentCustomer;
            var store = _storeContext.CurrentStore;

            var model = new BackInStockSubscribeModel();
            model.ProductId = product.Id;
            model.ProductName = product.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id);
            model.ProductSeName = product.GetSeName(_workContext.WorkingLanguage.Id);
            model.IsCurrentCustomerRegistered = customer.IsRegistered();
            model.MaximumBackInStockSubscriptions = _catalogSettings.MaximumBackInStockSubscriptions;
            model.CurrentNumberOfBackInStockSubscriptions = (await _backInStockSubscriptionService
                .GetAllSubscriptionsByCustomerId(customer.Id, store.Id, 0, 1))
                .TotalCount;
            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                product.BackorderMode == BackorderMode.NoBackorders &&
                product.AllowBackInStockSubscriptions &&
                product.GetTotalStockQuantity(warehouseId: store.DefaultWarehouseId) <= 0)
            {
                //out of stock
                model.SubscriptionAllowed = true;
                model.AlreadySubscribed = (await _backInStockSubscriptionService
                    .FindSubscription(customer.Id, product.Id, store.Id, product.UseMultipleWarehouses ? store.DefaultWarehouseId : "")) != null;
            }
            return View(model);
        }
        [HttpPost, ActionName("SubscribePopup")]
        public virtual async Task<IActionResult> SubscribePopupPOST(string productId)
        {
            var product = await _productService.GetProductById(productId);
            if (product == null )
                throw new ArgumentException("No product found with the specified id");

            var customer = _workContext.CurrentCustomer;

            if (!customer.IsRegistered())
                return Content(_localizationService.GetResource("BackInStockSubscriptions.OnlyRegistered"));

            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                product.BackorderMode == BackorderMode.NoBackorders &&
                product.AllowBackInStockSubscriptions &&
                product.GetTotalStockQuantity(warehouseId: _storeContext.CurrentStore.DefaultWarehouseId) <= 0)
            {
                //out of stock
                var subscription = await _backInStockSubscriptionService
                    .FindSubscription(customer.Id, product.Id, _storeContext.CurrentStore.Id, product.UseMultipleWarehouses ? _storeContext.CurrentStore.DefaultWarehouseId : "");
                if (subscription != null)
                {
                    //subscription already exists
                    //unsubscribe
                    await _backInStockSubscriptionService.DeleteSubscription(subscription);
                    return Content("Unsubscribed");
                }

                //subscription does not exist
                //subscribe
                if ((await _backInStockSubscriptionService
                    .GetAllSubscriptionsByCustomerId(customer.Id, _storeContext.CurrentStore.Id, 0, 1))
                    .TotalCount >= _catalogSettings.MaximumBackInStockSubscriptions)
                {
                    return Content(string.Format(_localizationService.GetResource("BackInStockSubscriptions.MaxSubscriptions"), _catalogSettings.MaximumBackInStockSubscriptions));
                }
                subscription = new BackInStockSubscription
                {
                    CustomerId = customer.Id,
                    ProductId = product.Id,
                    StoreId = _storeContext.CurrentStore.Id,
                    WarehouseId = product.UseMultipleWarehouses ? _storeContext.CurrentStore.DefaultWarehouseId : "",
                    CreatedOnUtc = DateTime.UtcNow
                };
                await _backInStockSubscriptionService.InsertSubscription(subscription);
                return Content("Subscribed");
            }

            //subscription not possible
            return Content(_localizationService.GetResource("BackInStockSubscriptions.NotAllowed"));
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
                    var subscriptionModel = new CustomerBackInStockSubscriptionsModel.BackInStockSubscriptionModel
                    {
                        Id = subscription.Id,
                        ProductId = product.Id,
                        ProductName = product.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                        SeName = product.GetSeName(_workContext.WorkingLanguage.Id),
                    };
                    model.Subscriptions.Add(subscriptionModel);
                }
            }

            model.PagerModel = new PagerModel(_localizationService)
            {
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
