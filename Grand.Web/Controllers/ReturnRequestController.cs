using System;
using Microsoft.AspNetCore.Mvc;
using Grand.Core;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Orders;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Services.Orders;
using Grand.Framework.Security;
using Grand.Web.Models.Order;
using Grand.Web.Services;
using Microsoft.AspNetCore.Http;
using Grand.Web.Extensions;
using Grand.Services.Seo;
using System.Linq;
using Grand.Core.Domain.Tax;
using Grand.Services.Directory;
using System.Globalization;
using Grand.Core.Domain.Common;
using Grand.Services.Common;

namespace Grand.Web.Controllers
{
    public partial class ReturnRequestController : BasePublicController
    {
        #region Fields
        private readonly IReturnRequestWebService _returnRequestWebService;
        private readonly IReturnRequestService _returnRequestService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICurrencyService _currencyService;
        private readonly OrderSettings _orderSettings;
        private readonly IAddressWebService _addressWebService;

        #endregion

        #region Constructors

        public ReturnRequestController(
            IReturnRequestWebService returnRequestWebService,
            IReturnRequestService returnRequestService,
            IOrderService orderService,
            IProductService productService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IOrderProcessingService orderProcessingService,
            ILocalizationService localizationService,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            IPriceFormatter priceFormatter,
            ICurrencyService currencyService,
            OrderSettings orderSettings,
            IAddressWebService addressWebService)
        {
            this._returnRequestWebService = returnRequestWebService;
            this._returnRequestService = returnRequestService;
            this._orderService = orderService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._orderProcessingService = orderProcessingService;
            this._localizationService = localizationService;
            this._workflowMessageService = workflowMessageService;
            this._localizationSettings = localizationSettings;
            this._productService = productService;
            this._priceFormatter = priceFormatter;
            this._currencyService = currencyService;
            this._orderSettings = orderSettings;
            this._addressWebService = addressWebService;
        }

        #endregion


        #region Methods

        public virtual IActionResult CustomerReturnRequests()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var model = _returnRequestWebService.PrepareCustomerReturnRequests();

            return View(model);
        }

        public virtual IActionResult ReturnRequest(string orderId, string errors = "")
        {
            var order = _orderService.GetOrderById(orderId);
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return Challenge();

            if (!_orderProcessingService.IsReturnRequestAllowed(order))
                return RedirectToRoute("HomePage");

            var model = new SubmitReturnRequestModel();
            model = _returnRequestWebService.PrepareReturnRequest(model, order);
            model.Error = errors;
            return View(model);
        }

        [HttpPost, ActionName("ReturnRequest")]
        [PublicAntiForgery]
        public virtual IActionResult ReturnRequestSubmit(string orderId, SubmitReturnRequestModel model, IFormCollection form)
        {
            var order = _orderService.GetOrderById(orderId);
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return Challenge();

            if (!_orderProcessingService.IsReturnRequestAllowed(order))
                return RedirectToRoute("HomePage");

            string pD = form["pickupDate"];
            DateTime pickupDate = default(DateTime);
            if (!string.IsNullOrEmpty(pD))
                pickupDate = DateTime.ParseExact(form["pickupDate"], "MM/dd/yyyy", CultureInfo.InvariantCulture);

            string pickupAddressId = form["pickup_address_id"];
            Address address = new Address();
            if (!String.IsNullOrEmpty(pickupAddressId))
            {
                address = _workContext.CurrentCustomer.Addresses.FirstOrDefault(a => a.Id == pickupAddressId);
            }
            else
            {
                var customAttributes = _addressWebService.ParseCustomAddressAttributes(form);
                address = model.NewAddress.ToEntity();
                address.CustomAttributes = customAttributes;
                address.CreatedOnUtc = DateTime.UtcNow;
            }

            var rr = new ReturnRequest
            {
                StoreId = _storeContext.CurrentStore.Id,
                OrderId = order.Id,
                CustomerId = _workContext.CurrentCustomer.Id,
                CustomerComments = model.Comments,
                StaffNotes = string.Empty,
                ReturnRequestStatus = ReturnRequestStatus.Pending,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                PickupAddress = address,
                PickupDate = pickupDate
            };

            int count = 0;
            foreach (var orderItem in order.OrderItems)
            {
                var product = _productService.GetProductById(orderItem.ProductId);
                if (!product.NotReturnable)
                {
                    int quantity = 0; //parse quantity
                    string rrrId = "";
                    string rraId = "";

                    foreach (string formKey in form.Keys)
                    {
                        if (formKey.Equals(string.Format("quantity{0}", orderItem.Id), StringComparison.OrdinalIgnoreCase))
                        {
                            int.TryParse(form[formKey], out quantity);
                        }

                        if (formKey.Equals(string.Format("reason{0}", orderItem.Id), StringComparison.OrdinalIgnoreCase))
                        {
                            rrrId = form[formKey];
                        }

                        if (formKey.Equals(string.Format("action{0}", orderItem.Id), StringComparison.OrdinalIgnoreCase))
                        {
                            rraId = form[formKey];
                        }
                    }

                    if (quantity > 0)
                    {
                        var rrr = _returnRequestService.GetReturnRequestReasonById(rrrId);
                        var rra = _returnRequestService.GetReturnRequestActionById(rraId);
                        rr.ReturnRequestItems.Add(new ReturnRequestItem
                        {
                            RequestedAction = rra != null ? rra.GetLocalized(x => x.Name) : "not available",
                            ReasonForReturn = rrr != null ? rrr.GetLocalized(x => x.Name) : "not available",
                            Quantity = quantity,
                            OrderItemId = orderItem.Id
                        });

                        count++;
                    }
                }
            }

            model = _returnRequestWebService.PrepareReturnRequest(model, order);
            if (count > 0)
            {
                model.Result = _localizationService.GetResource("ReturnRequests.Submitted");

                _returnRequestService.InsertReturnRequest(rr);
                //notify store owner here (email)
                _workflowMessageService.SendNewReturnRequestStoreOwnerNotification(rr, order, _localizationSettings.DefaultAdminLanguageId);
                //notify customer
                _workflowMessageService.SendNewReturnRequestCustomerNotification(rr, order, order.CustomerLanguageId);
            }
            else
            {
                model.Result = _localizationService.GetResource("ReturnRequests.NoItemsSubmitted");
                return ReturnRequest(orderId, model.Result);
            }

            return View(model);
        }

        public virtual IActionResult ReturnRequestDetails(string returnRequestId)
        {
            var rr = _returnRequestService.GetReturnRequestById(returnRequestId);
            if (rr == null || _workContext.CurrentCustomer.Id != rr.CustomerId)
                return Challenge();

            var order = _orderService.GetOrderById(rr.OrderId);
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return Challenge();

            if (!_orderProcessingService.IsReturnRequestAllowed(order))
                return RedirectToRoute("HomePage");

            var model = new ReturnRequestDetailsModel();
            model.Comments = rr.CustomerComments;
            model.ReturnNumber = rr.ReturnNumber;
            model.ReturnRequestStatus = rr.ReturnRequestStatus;
            model.CreatedOnUtc = rr.CreatedOnUtc;
            model.ShowPickupAddress = _orderSettings.ReturnRequests_AllowToSpecifyPickupAddress;
            model.ShowPickupDate = _orderSettings.ReturnRequests_AllowToSpecifyPickupDate;
            model.PickupDate = rr.PickupDate;
            _addressWebService.PrepareModel(model: model.PickupAddress, address: rr.PickupAddress, excludeProperties: false);

            foreach (var item in rr.ReturnRequestItems)
            {
                var orderItem = order.OrderItems.Where(x => x.Id == item.OrderItemId).FirstOrDefault();
                var product = _productService.GetProductByIdIncludeArch(orderItem.ProductId);

                string unitPrice = string.Empty;
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
                    unitPrice = _priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency);
                }
                else
                {
                    //excluding tax
                    var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
                    unitPrice = _priceFormatter.FormatPrice(unitPriceExclTaxInCustomerCurrency);
                }

                model.ReturnRequestItems.Add(new ReturnRequestDetailsModel.ReturnRequestItemModel
                {
                    OrderItemId = item.OrderItemId,
                    Quantity = item.Quantity,
                    ReasonForReturn = item.ReasonForReturn,
                    RequestedAction = item.RequestedAction,
                    ProductName = product.GetLocalized(x => x.Name),
                    ProductSeName = product.GetSeName(),
                    ProductPrice = unitPrice
                });
            }

            return View(model);
        }

        #endregion
    }
}
