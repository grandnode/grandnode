using Grand.Core;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Orders;
using Grand.Framework.Security;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Services.Orders;
using Grand.Web.Extensions;
using Grand.Web.Models.Order;
using Grand.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Globalization;
using System.Linq;

namespace Grand.Web.Controllers
{
    public partial class ReturnRequestController : BasePublicController
    {
        #region Fields
        private readonly IReturnRequestViewModelService _returnRequestViewModelService;
        private readonly IReturnRequestService _returnRequestService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IAddressViewModelService _addressViewModelService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly OrderSettings _orderSettings;
        #endregion

        #region Constructors

        public ReturnRequestController(
            IReturnRequestViewModelService returnRequestViewModelService,
            IReturnRequestService returnRequestService,
            IOrderService orderService,
            IProductService productService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IOrderProcessingService orderProcessingService,
            ILocalizationService localizationService,
            IWorkflowMessageService workflowMessageService,
            IAddressViewModelService addressViewModelService,
            LocalizationSettings localizationSettings,
            OrderSettings orderSettings)
        {
            this._returnRequestViewModelService = returnRequestViewModelService;
            this._returnRequestService = returnRequestService;
            this._orderService = orderService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._orderProcessingService = orderProcessingService;
            this._localizationService = localizationService;
            this._workflowMessageService = workflowMessageService;
            this._productService = productService;
            this._addressViewModelService = addressViewModelService;
            this._localizationSettings = localizationSettings;
            this._orderSettings = orderSettings;
        }

        #endregion

        #region Methods

        public virtual IActionResult CustomerReturnRequests()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var model = _returnRequestViewModelService.PrepareCustomerReturnRequests();

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
            model = _returnRequestViewModelService.PrepareReturnRequest(model, order);
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
            {
                pickupDate = DateTime.ParseExact(form["pickupDate"], "MM/dd/yyyy", CultureInfo.InvariantCulture);
            }
            else if (_orderSettings.ReturnRequests_AllowToSpecifyPickupDate && _orderSettings.ReturnRequests_PickupDateRequired)
            {
                ModelState.AddModelError("", _localizationService.GetResource("ReturnRequests.PickupDateRequired"));
            }

            string pickupAddressId = form["pickup_address_id"];
            Address address = new Address();
            if (!String.IsNullOrEmpty(pickupAddressId))
            {
                address = _workContext.CurrentCustomer.Addresses.FirstOrDefault(a => a.Id == pickupAddressId);
            }
            else
            {
                var customAttributes = _addressViewModelService.ParseCustomAddressAttributes(form);
                var customAttributeWarnings = _addressViewModelService.GetAttributeWarnings(customAttributes);
                foreach (var error in customAttributeWarnings)
                {
                    ModelState.AddModelError("", error);
                }
                address = model.NewAddress.ToEntity();
                model.NewAddressPreselected = true;
                address.CustomAttributes = customAttributes;
                address.CreatedOnUtc = DateTime.UtcNow;
            }

            if (!ModelState.IsValid && ModelState.ErrorCount > 0)
            {
                model.Error = string.Join(", ", ModelState.Keys.SelectMany(k => ModelState[k].Errors).Select(m => m.ErrorMessage).ToArray());
                model = _returnRequestViewModelService.PrepareReturnRequest(model, order);
                return View(model);
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
            model = _returnRequestViewModelService.PrepareReturnRequest(model, order);
            if (count > 0)
            {
                _returnRequestService.InsertReturnRequest(rr);

                model.Result = string.Format(_localizationService.GetResource("ReturnRequests.Submitted"), rr.ReturnNumber, Url.Link("ReturnRequestDetails", new { returnRequestId = rr.Id }));

                //notify store owner here (email)
                _workflowMessageService.SendNewReturnRequestStoreOwnerNotification(rr, order, _localizationSettings.DefaultAdminLanguageId);
                //notify customer
                _workflowMessageService.SendNewReturnRequestCustomerNotification(rr, order, order.CustomerLanguageId);
            }
            else
            {
                model.Error = _localizationService.GetResource("ReturnRequests.NoItemsSubmitted");
                return View(model);
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

            var model = _returnRequestViewModelService.PrepareReturnRequestDetails(rr, order);

            return View(model);
        }

        #endregion
    }
}
