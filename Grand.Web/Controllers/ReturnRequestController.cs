using Grand.Core;
using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Orders;
using Grand.Services.Common;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Orders;
using Grand.Services.Queries.Models.Orders;
using Grand.Web.Commands.Models.Orders;
using Grand.Web.Extensions;
using Grand.Web.Features.Models.Common;
using Grand.Web.Features.Models.Orders;
using Grand.Web.Models.Common;
using Grand.Web.Models.Orders;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Controllers
{
    public partial class ReturnRequestController : BasePublicController
    {
        #region Fields

        private readonly IReturnRequestService _returnRequestService;
        private readonly IOrderService _orderService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly IMediator _mediator;

        private readonly OrderSettings _orderSettings;
        #endregion

        #region Constructors

        public ReturnRequestController(
            IReturnRequestService returnRequestService,
            IOrderService orderService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            IMediator mediator,
            OrderSettings orderSettings)
        {
            _returnRequestService = returnRequestService;
            _orderService = orderService;
            _workContext = workContext;
            _storeContext = storeContext;
            _localizationService = localizationService;
            _mediator = mediator;
            _orderSettings = orderSettings;
        }

        #endregion

        #region Utilities

        private async Task PrepareModelAddress(AddressModel addressModel)
        {
            var countryService = HttpContext.RequestServices.GetRequiredService<ICountryService>();
            var countries = await countryService.GetAllCountries(_workContext.WorkingLanguage.Id);
            addressModel = await _mediator.Send(new GetAddressModel() {
                Language = _workContext.WorkingLanguage,
                Store = _storeContext.CurrentStore,
                Customer = _workContext.CurrentCustomer,
                Model = addressModel,
                Address = null,
                ExcludeProperties = true,
                PrePopulateWithCustomerFields = true,
                LoadCountries = () => countries
            });
        }

        protected async Task<Address> PrepareAddress(ReturnRequestModel model, IFormCollection form)
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
                    var customAttributes = await _mediator.Send(new GetParseCustomAddressAttributes() { Form = form });
                    var addressAttributeParser = HttpContext.RequestServices.GetRequiredService<IAddressAttributeParser>();
                    var customAttributeWarnings = await addressAttributeParser.GetAttributeWarnings(customAttributes);
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

            var model = await _mediator.Send(new GetReturnRequests() {
                Customer = _workContext.CurrentCustomer,
                Store = _storeContext.CurrentStore,
                Language = _workContext.WorkingLanguage
            });

            return View(model);
        }

        public virtual async Task<IActionResult> ReturnRequest(string orderId, string errors = "")
        {
            var order = await _orderService.GetOrderById(orderId);
            if (!order.Access(_workContext.CurrentCustomer))
                return Challenge();

            if (!await _mediator.Send(new IsReturnRequestAllowedQuery() { Order = order }))
                return RedirectToRoute("HomePage");

            var model = await _mediator.Send(new GetReturnRequest() {
                Order = order,
                Language = _workContext.WorkingLanguage,
                Store = _storeContext.CurrentStore
            });
            model.Error = errors;
            return View(model);
        }

        [HttpPost, ActionName("ReturnRequest")]
        [AutoValidateAntiforgeryToken]
        public virtual async Task<IActionResult> ReturnRequestSubmit(string orderId, ReturnRequestModel model, IFormCollection form)
        {
            var order = await _orderService.GetOrderById(orderId);
            if (!order.Access(_workContext.CurrentCustomer))
                return Challenge();

            if (!await _mediator.Send(new IsReturnRequestAllowedQuery() { Order = order }))
                return RedirectToRoute("HomePage");

            ModelState.Clear();

            if (_orderSettings.ReturnRequests_AllowToSpecifyPickupDate && _orderSettings.ReturnRequests_PickupDateRequired && model.PickupDate == null)
            {
                ModelState.AddModelError("", _localizationService.GetResource("ReturnRequests.PickupDateRequired"));
            }

            var address = await PrepareAddress(model, form);

            if (!ModelState.IsValid && ModelState.ErrorCount > 0)
            {
                var returnmodel = await _mediator.Send(new GetReturnRequest() {
                    Order = order,
                    Language = _workContext.WorkingLanguage,
                    Store = _storeContext.CurrentStore
                });
                returnmodel.Error = string.Join(", ", ModelState.Keys.SelectMany(k => ModelState[k].Errors).Select(m => m.ErrorMessage).ToArray());
                returnmodel.Comments = model.Comments;
                returnmodel.PickupDate = model.PickupDate;
                returnmodel.NewAddressPreselected = model.NewAddressPreselected;
                returnmodel.NewAddress = model.NewAddress;
                if (returnmodel.NewAddressPreselected || _orderSettings.ReturnRequests_AllowToSpecifyPickupAddress)
                {
                    await PrepareModelAddress(model.NewAddress);
                }
                return View(returnmodel);
            }
            else
            {
                var result = await _mediator.Send(new ReturnRequestSubmitCommand() { Address = address, Model = model, Form = form, Order = order });
                if (result.rr.ReturnNumber > 0)
                {
                    model.Result = string.Format(_localizationService.GetResource("ReturnRequests.Submitted"), result.rr.ReturnNumber, Url.Link("ReturnRequestDetails", new { returnRequestId = result.rr.Id }));
                    model.OrderNumber = order.OrderNumber;
                    model.OrderCode = order.Code;
                    return View(result.model);
                }

                var returnmodel = await _mediator.Send(new GetReturnRequest() {
                    Order = order,
                    Language = _workContext.WorkingLanguage,
                    Store = _storeContext.CurrentStore
                });
                returnmodel.Error = result.model.Error;
                returnmodel.Comments = model.Comments;
                returnmodel.PickupDate = model.PickupDate;
                returnmodel.NewAddressPreselected = model.NewAddressPreselected;
                returnmodel.NewAddress = model.NewAddress;
                if (returnmodel.NewAddressPreselected || _orderSettings.ReturnRequests_AllowToSpecifyPickupAddress)
                {
                    await PrepareModelAddress(model.NewAddress);
                }
                return View(returnmodel);
            }

        }

        public virtual async Task<IActionResult> ReturnRequestDetails(string returnRequestId)
        {
            var rr = await _returnRequestService.GetReturnRequestById(returnRequestId);
            if (!rr.Access(_workContext.CurrentCustomer))
                return Challenge();

            var order = await _orderService.GetOrderById(rr.OrderId);
            if (!order.Access(_workContext.CurrentCustomer))
                return Challenge();

            var model = await _mediator.Send(new GetReturnRequestDetails() {
                Order = order,
                Language = _workContext.WorkingLanguage,
                ReturnRequest = rr,
            });

            return View(model);
        }

        #endregion
    }
}
