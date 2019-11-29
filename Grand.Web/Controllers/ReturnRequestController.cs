using Grand.Core;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Orders;
using Grand.Framework.Security;
using Grand.Services.Localization;
using Grand.Services.Orders;
using Grand.Web.Extensions;
using Grand.Web.Interfaces;
using Grand.Web.Models.Order;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Controllers
{
    public partial class ReturnRequestController : BasePublicController
    {
        #region Fields
        private readonly IReturnRequestViewModelService _returnRequestViewModelService;
        private readonly IReturnRequestService _returnRequestService;
        private readonly IOrderService _orderService;
        private readonly IWorkContext _workContext;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILocalizationService _localizationService;
        private readonly IAddressViewModelService _addressViewModelService;
        private readonly OrderSettings _orderSettings;
        #endregion

        #region Constructors

        public ReturnRequestController(
            IReturnRequestViewModelService returnRequestViewModelService,
            IReturnRequestService returnRequestService,
            IOrderService orderService,
            IWorkContext workContext,
            IOrderProcessingService orderProcessingService,
            ILocalizationService localizationService,
            IAddressViewModelService addressViewModelService,
            OrderSettings orderSettings)
        {
            _returnRequestViewModelService = returnRequestViewModelService;
            _returnRequestService = returnRequestService;
            _orderService = orderService;
            _workContext = workContext;
            _orderProcessingService = orderProcessingService;
            _localizationService = localizationService;
            _addressViewModelService = addressViewModelService;
            _orderSettings = orderSettings;
        }

        #endregion

        #region Utilities

        protected async Task<Address> PrepareAddress(SubmitReturnRequestModel model, IFormCollection form)
        {
            string pickupAddressId = form["pickup_address_id"];
            var address = new Address();
            if (_orderSettings.ReturnRequests_AllowToSpecifyPickupAddress)
            {
                if (!string.IsNullOrEmpty(pickupAddressId))
                {
                    address = _workContext.CurrentCustomer.Addresses.FirstOrDefault(a => a.Id == pickupAddressId);
                }
                else
                {
                    var customAttributes = await _addressViewModelService.ParseCustomAddressAttributes(form);
                    var customAttributeWarnings = await _addressViewModelService.GetAttributeWarnings(customAttributes);
                    foreach (var error in customAttributeWarnings)
                    {
                        ModelState.AddModelError("", error);
                    }
                    await TryUpdateModelAsync(model.NewAddress, "ReturnRequestNewAddress");
                    address = model.NewAddress.ToEntity();
                    model.NewAddressPreselected = true;
                    address.CustomAttributes = customAttributes;
                    address.CreatedOnUtc = DateTime.UtcNow;
                }
            }
            return address;
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> CustomerReturnRequests()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var model = await _returnRequestViewModelService.PrepareCustomerReturnRequests();

            return View(model);
        }

        public virtual async Task<IActionResult> ReturnRequest(string orderId, string errors = "")
        {
            var order = await _orderService.GetOrderById(orderId);
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return Challenge();

            if (!await _orderProcessingService.IsReturnRequestAllowed(order))
                return RedirectToRoute("HomePage");

            var model = new SubmitReturnRequestModel();
            model = await _returnRequestViewModelService.PrepareReturnRequest(model, order);
            model.Error = errors;
            return View(model);
        }

        [HttpPost, ActionName("ReturnRequest")]
        [PublicAntiForgery]
        public virtual async Task<IActionResult> ReturnRequestSubmit(string orderId, SubmitReturnRequestModel model, IFormCollection form)
        {
            var order = await _orderService.GetOrderById(orderId);
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return Challenge();

            if (!await _orderProcessingService.IsReturnRequestAllowed(order))
                return RedirectToRoute("HomePage");

            ModelState.Clear();

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

            var address = await PrepareAddress(model, form);

            if (!ModelState.IsValid && ModelState.ErrorCount > 0)
            {
                model.Error = string.Join(", ", ModelState.Keys.SelectMany(k => ModelState[k].Errors).Select(m => m.ErrorMessage).ToArray());
                model = await _returnRequestViewModelService.PrepareReturnRequest(model, order);
                return View(model);
            }

            var result = await _returnRequestViewModelService.ReturnRequestSubmit(model, order, address, pickupDate, form);
            if(result.rr.ReturnNumber > 0)
                model.Result = string.Format(_localizationService.GetResource("ReturnRequests.Submitted"), result.rr.ReturnNumber, Url.Link("ReturnRequestDetails", new { returnRequestId = result.rr.Id }));

            return View(result.model);
        }

        public virtual async Task<IActionResult> ReturnRequestDetails(string returnRequestId)
        {
            var rr = await _returnRequestService.GetReturnRequestById(returnRequestId);
            if (rr == null || _workContext.CurrentCustomer.Id != rr.CustomerId)
                return Challenge();

            var order = await _orderService.GetOrderById(rr.OrderId);
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return Challenge();

            var model = await _returnRequestViewModelService.PrepareReturnRequestDetails(rr, order);
            
            return View(model);
        }

        #endregion
    }
}
