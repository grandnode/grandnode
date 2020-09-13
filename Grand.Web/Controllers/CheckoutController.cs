using Grand.Core;
using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Orders;
using Grand.Domain.Payments;
using Grand.Domain.Shipping;
using Grand.Core.Http;
using Grand.Core.Plugins;
using Grand.Framework.Controllers;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Orders;
using Grand.Services.Payments;
using Grand.Services.Shipping;
using Grand.Web.Extensions;
using Grand.Web.Features.Models.Checkout;
using Grand.Web.Features.Models.Common;
using Grand.Web.Models.Checkout;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Controllers
{
    public partial class CheckoutController : BasePublicController
    {
        #region Fields
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerService _customerService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IShippingService _shippingService;
        private readonly IPaymentService _paymentService;
        private readonly IPluginFinder _pluginFinder;
        private readonly ILogger _logger;
        private readonly IOrderService _orderService;
        private readonly IWebHelper _webHelper;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IMediator _mediator;
        private readonly OrderSettings _orderSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly AddressSettings _addressSettings;

        #endregion

        #region Constructors

        public CheckoutController(
            IWorkContext workContext,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            ICustomerService customerService,
            IShoppingCartService shoppingCartService,
            IGenericAttributeService genericAttributeService,
            IShippingService shippingService,
            IPaymentService paymentService,
            IPluginFinder pluginFinder,
            ILogger logger,
            IOrderService orderService,
            IWebHelper webHelper,
            IAddressAttributeParser addressAttributeParser,
            ICustomerActivityService customerActivityService,
            IMediator mediator,
            OrderSettings orderSettings,
            RewardPointsSettings rewardPointsSettings,
            PaymentSettings paymentSettings,
            ShippingSettings shippingSettings,
            AddressSettings addressSettings)
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _localizationService = localizationService;
            _customerService = customerService;
            _shoppingCartService = shoppingCartService;
            _genericAttributeService = genericAttributeService;
            _shippingService = shippingService;
            _paymentService = paymentService;
            _pluginFinder = pluginFinder;
            _logger = logger;
            _orderService = orderService;
            _webHelper = webHelper;
            _addressAttributeParser = addressAttributeParser;
            _customerActivityService = customerActivityService;
            _mediator = mediator;
            _orderSettings = orderSettings;
            _rewardPointsSettings = rewardPointsSettings;
            _paymentSettings = paymentSettings;
            _shippingSettings = shippingSettings;
            _addressSettings = addressSettings;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected IShippingRateComputationMethod GetShippingComputation(string input)
        {
            var shippingMethodName = input.Split(new[] { "___" }, StringSplitOptions.RemoveEmptyEntries)[1];
            var shippingMethod = _shippingService.LoadShippingRateComputationMethodBySystemName(shippingMethodName);
            if (shippingMethod == null)
                throw new Exception("Shipping method is not selected");

            return shippingMethod;
        }

        [NonAction]
        protected async Task<IList<string>> ValidateShippingForm(IFormCollection form)
        {
            var warnings = (await GetShippingComputation(form["shippingoption"]).ValidateShippingForm(form)).ToList();
            foreach (var warning in warnings)
                ModelState.AddModelError("", warning);
            return warnings;
        }

        #endregion

        #region Methods (common)

        public virtual async Task<IActionResult> Index([FromServices] IShoppingCartService shoppingCartService, [FromServices] IProductService productService)
        {
            var customer = _workContext.CurrentCustomer;

            var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, ShoppingCartType.ShoppingCart, ShoppingCartType.Auctions);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            if ((customer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return Challenge();

            //reset checkout data
            await _customerService.ResetCheckoutData(customer, _storeContext.CurrentStore.Id);

            //validation (cart)

            var checkoutAttributesXml = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.CheckoutAttributes, _storeContext.CurrentStore.Id);
            var scWarnings = await shoppingCartService.GetShoppingCartWarnings(cart, checkoutAttributesXml, true);
            if (scWarnings.Any())
                return RedirectToRoute("ShoppingCart", new { checkoutAttributes = true });

            //validation (each shopping cart item)
            foreach (ShoppingCartItem sci in cart)
            {
                var product = await productService.GetProductById(sci.ProductId);
                var sciWarnings = await shoppingCartService.GetShoppingCartItemWarnings(customer, sci, product, false);
                if (sciWarnings.Any())
                    return RedirectToRoute("ShoppingCart", new { checkoutAttributes = true });
            }

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            return RedirectToRoute("CheckoutBillingAddress");
        }

        public virtual async Task<IActionResult> Completed(string orderId)
        {
            //validation
            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return Challenge();

            Order order = null;
            if (!String.IsNullOrEmpty(orderId))
            {
                order = await _orderService.GetOrderById(orderId);
            }
            if (order == null)
            {
                order = (await _orderService.SearchOrders(storeId: _storeContext.CurrentStore.Id,
                customerId: _workContext.CurrentCustomer.Id, pageSize: 1))
                    .FirstOrDefault();
            }
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
            {
                return RedirectToRoute("HomePage");
            }

            //disable "order completed" page?
            if (_orderSettings.DisableOrderCompletedPage)
            {
                return RedirectToRoute("OrderDetails", new { orderId = order.Id });
            }

            //model
            var model = new CheckoutCompletedModel {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                OrderCode = order.Code,
                OnePageCheckoutEnabled = _orderSettings.OnePageCheckoutEnabled
            };

            return View(model);
        }

        #endregion

        #region Methods (multistep checkout)

        public virtual async Task<IActionResult> BillingAddress(IFormCollection form)
        {
            //validation
            var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, ShoppingCartType.ShoppingCart, ShoppingCartType.Auctions);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return Challenge();

            //model
            var model = await _mediator.Send(new GetBillingAddress() {
                Cart = cart,
                Currency = _workContext.WorkingCurrency,
                Customer = _workContext.CurrentCustomer,
                Language = _workContext.WorkingLanguage,
                Store = _storeContext.CurrentStore,
                PrePopulateNewAddressWithCustomerFields = true
            });

            //check whether "billing address" step is enabled
            if (_orderSettings.DisableBillingAddressCheckoutStep)
            {
                if (model.ExistingAddresses.Any())
                {
                    //choose the first one
                    return await SelectBillingAddress(model.ExistingAddresses.First().Id);
                }

                TryValidateModel(model);
                TryValidateModel(model.NewAddress);
                return await NewBillingAddress(model, form);
            }

            return View(model);
        }
        public virtual async Task<IActionResult> SelectBillingAddress(string addressId, bool shipToSameAddress = false)
        {
            var address = _workContext.CurrentCustomer.Addresses.FirstOrDefault(a => a.Id == addressId);
            if (address == null)
                return RedirectToRoute("CheckoutBillingAddress");

            _workContext.CurrentCustomer.BillingAddress = address;
            address.CustomerId = _workContext.CurrentCustomer.Id;
            await _customerService.UpdateBillingAddress(address);

            var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, ShoppingCartType.ShoppingCart, ShoppingCartType.Auctions);

            //ship to the same address?
            if (_shippingSettings.ShipToSameAddress && shipToSameAddress && cart.RequiresShipping())
            {
                _workContext.CurrentCustomer.ShippingAddress = _workContext.CurrentCustomer.BillingAddress;
                await _customerService.UpdateShippingAddress(address);
                //reset selected shipping method (in case if "pick up in store" was selected)
                await _genericAttributeService.SaveAttribute<ShippingOption>(_workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedShippingOption, null, _storeContext.CurrentStore.Id);
                await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedPickupPoint, "", _storeContext.CurrentStore.Id);

                //clear shipping option XML/Description
                await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.ShippingOptionAttributeXml, "", _storeContext.CurrentStore.Id);
                await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.ShippingOptionAttributeDescription, "", _storeContext.CurrentStore.Id);

                //limitation - "Ship to the same address" doesn't properly work in "pick up in store only" case (when no shipping plugins are available) 
                return RedirectToRoute("CheckoutShippingMethod");
            }

            return RedirectToRoute("CheckoutShippingAddress");
        }
        [HttpPost, ActionName("BillingAddress")]
        [FormValueRequired("nextstep")]
        public virtual async Task<IActionResult> NewBillingAddress(CheckoutBillingAddressModel model, IFormCollection form)
        {
            //validation
            var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, ShoppingCartType.ShoppingCart, ShoppingCartType.Auctions);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return Challenge();

            //custom address attributes
            var customAttributes = await _mediator.Send(new GetParseCustomAddressAttributes() { Form = form });
            var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarnings(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            if (ModelState.IsValid && ModelState.ErrorCount == 0)
            {
                //try to find an address with the same values (don't duplicate records)
                var address = _workContext.CurrentCustomer.Addresses.ToList().FindAddress(
                    model.NewAddress.FirstName, model.NewAddress.LastName, model.NewAddress.PhoneNumber,
                    model.NewAddress.Email, model.NewAddress.FaxNumber, model.NewAddress.Company,
                    model.NewAddress.Address1, model.NewAddress.Address2, model.NewAddress.City,
                    model.NewAddress.StateProvinceId, model.NewAddress.ZipPostalCode,
                    model.NewAddress.CountryId, customAttributes);
                if (address == null)
                {
                    //address is not found. let's create a new one
                    address = model.NewAddress.ToEntity();
                    address.CustomAttributes = customAttributes;
                    address.CreatedOnUtc = DateTime.UtcNow;
                    _workContext.CurrentCustomer.Addresses.Add(address);
                    address.CustomerId = _workContext.CurrentCustomer.Id;
                    await _customerService.InsertAddress(address);
                }
                _workContext.CurrentCustomer.BillingAddress = address;
                address.CustomerId = _workContext.CurrentCustomer.Id;
                await _customerService.UpdateBillingAddress(address);

                //ship to the same address?
                if (_shippingSettings.ShipToSameAddress && model.ShipToSameAddress && cart.RequiresShipping())
                {
                    _workContext.CurrentCustomer.ShippingAddress = _workContext.CurrentCustomer.BillingAddress;
                    await _customerService.UpdateShippingAddress(address);
                    //reset selected shipping method (in case if "pick up in store" was selected)
                    await _genericAttributeService.SaveAttribute<ShippingOption>(_workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedShippingOption, null, _storeContext.CurrentStore.Id);
                    await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedPickupPoint, "", _storeContext.CurrentStore.Id);

                    //clear shipping option XML/Description
                    await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.ShippingOptionAttributeXml, "", _storeContext.CurrentStore.Id);
                    await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.ShippingOptionAttributeDescription, "", _storeContext.CurrentStore.Id);

                    //limitation - "Ship to the same address" doesn't properly work in "pick up in store only" case (when no shipping plugins are available) 
                    return RedirectToRoute("CheckoutShippingMethod");
                }

                return RedirectToRoute("CheckoutShippingAddress");
            }

            //If we got this far, something failed, redisplay form
            model = await _mediator.Send(new GetBillingAddress() {
                Cart = cart,
                Currency = _workContext.WorkingCurrency,
                Customer = _workContext.CurrentCustomer,
                Language = _workContext.WorkingLanguage,
                Store = _storeContext.CurrentStore,
                SelectedCountryId = model.NewAddress.CountryId,
                OverrideAttributesXml = customAttributes
            });

            return View(model);
        }

        public virtual async Task<IActionResult> ShippingAddress()
        {
            //validation
            var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, ShoppingCartType.ShoppingCart, ShoppingCartType.Auctions);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return Challenge();

            if (!cart.RequiresShipping())
            {
                _workContext.CurrentCustomer.ShippingAddress = null;
                await _customerService.RemoveShippingAddress(_workContext.CurrentCustomer.Id);
                return RedirectToRoute("CheckoutShippingMethod");
            }

            //model
            var model = await _mediator.Send(new GetShippingAddress() {
                Currency = _workContext.WorkingCurrency,
                Customer = _workContext.CurrentCustomer,
                Language = _workContext.WorkingLanguage,
                Store = _storeContext.CurrentStore,
                PrePopulateNewAddressWithCustomerFields = true,
            });

            return View(model);
        }
        public virtual async Task<IActionResult> SelectShippingAddress(string addressId)
        {
            var address = _workContext.CurrentCustomer.Addresses.FirstOrDefault(a => a.Id == addressId);
            if (address == null)
                return RedirectToRoute("CheckoutShippingAddress");

            _workContext.CurrentCustomer.ShippingAddress = address;
            address.CustomerId = _workContext.CurrentCustomer.Id;
            await _customerService.UpdateShippingAddress(address);

            if (_shippingSettings.AllowPickUpInStore)
            {
                //set value indicating that "pick up in store" option has not been chosen
                await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedPickupPoint, "", _storeContext.CurrentStore.Id);
            }

            return RedirectToRoute("CheckoutShippingMethod");
        }
        [HttpPost, ActionName("ShippingAddress")]
        [FormValueRequired("nextstep")]
        public virtual async Task<IActionResult> NewShippingAddress(CheckoutShippingAddressModel model, IFormCollection form)
        {
            //validation
            var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, ShoppingCartType.ShoppingCart, ShoppingCartType.Auctions);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return Challenge();

            if (!cart.RequiresShipping())
            {
                _workContext.CurrentCustomer.ShippingAddress = null;
                await _customerService.RemoveShippingAddress(_workContext.CurrentCustomer.Id);
                return RedirectToRoute("CheckoutShippingMethod");
            }


            //Pick up in store?
            if (_shippingSettings.AllowPickUpInStore)
            {
                if (model.PickUpInStore)
                {
                    //customer decided to pick up in store
                    //no shipping address selected
                    _workContext.CurrentCustomer.ShippingAddress = null;
                    await _customerService.RemoveShippingAddress(_workContext.CurrentCustomer.Id);
                    //clear shipping option XML/Description
                    await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.ShippingOptionAttributeXml, "", _storeContext.CurrentStore.Id);
                    await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.ShippingOptionAttributeDescription, "", _storeContext.CurrentStore.Id);


                    var pickupPoint = form["pickup-point-id"];
                    var pickupPoints = await _shippingService.LoadActivePickupPoints(_storeContext.CurrentStore.Id);
                    var selectedPoint = pickupPoints.FirstOrDefault(x => x.Id.Equals(pickupPoint));
                    if (selectedPoint == null)
                        return RedirectToRoute("CheckoutShippingAddress");

                    //save "pick up in store" shipping method
                    var pickUpInStoreShippingOption = new ShippingOption {
                        Name = string.Format(_localizationService.GetResource("Checkout.PickupPoints.Name"), selectedPoint.Name),
                        Rate = selectedPoint.PickupFee,
                        Description = selectedPoint.Description,
                        ShippingRateComputationMethodSystemName = string.Format("PickupPoint_{0}", selectedPoint.Id)
                    };

                    await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                        SystemCustomerAttributeNames.SelectedShippingOption,
                        pickUpInStoreShippingOption,
                        _storeContext.CurrentStore.Id);

                    await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                        SystemCustomerAttributeNames.SelectedPickupPoint,
                        selectedPoint.Id,
                        _storeContext.CurrentStore.Id);

                    //load next step
                    return RedirectToRoute("CheckoutPaymentMethod");
                }

                //set value indicating that "pick up in store" option has not been chosen
                await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedPickupPoint, "", _storeContext.CurrentStore.Id);
            }

            //custom address attributes
            var customAttributes = await _mediator.Send(new GetParseCustomAddressAttributes() { Form = form });
            var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarnings(customAttributes);

            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            if (ModelState.IsValid && ModelState.ErrorCount == 0)
            {
                //try to find an address with the same values (don't duplicate records)
                var address = _workContext.CurrentCustomer.Addresses.ToList().FindAddress(
                    model.NewAddress.FirstName, model.NewAddress.LastName, model.NewAddress.PhoneNumber,
                    model.NewAddress.Email, model.NewAddress.FaxNumber, model.NewAddress.Company,
                    model.NewAddress.Address1, model.NewAddress.Address2, model.NewAddress.City,
                    model.NewAddress.StateProvinceId, model.NewAddress.ZipPostalCode,
                    model.NewAddress.CountryId, customAttributes);
                if (address == null)
                {
                    address = model.NewAddress.ToEntity();
                    address.CustomAttributes = customAttributes;
                    address.CreatedOnUtc = DateTime.UtcNow;
                    _workContext.CurrentCustomer.Addresses.Add(address);
                    address.CustomerId = _workContext.CurrentCustomer.Id;
                    await _customerService.InsertAddress(address);
                }
                _workContext.CurrentCustomer.ShippingAddress = address;
                address.CustomerId = _workContext.CurrentCustomer.Id;
                await _customerService.UpdateShippingAddress(address);

                return RedirectToRoute("CheckoutShippingMethod");
            }

            //If we got this far, something failed, redisplay form
            model = await _mediator.Send(new GetShippingAddress() {
                Currency = _workContext.WorkingCurrency,
                Customer = _workContext.CurrentCustomer,
                Language = _workContext.WorkingLanguage,
                Store = _storeContext.CurrentStore,
                SelectedCountryId = model.NewAddress.CountryId,
                OverrideAttributesXml = customAttributes
            });


            return View(model);
        }

        public virtual async Task<IActionResult> ShippingMethod()
        {
            //validation
            var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, ShoppingCartType.ShoppingCart, ShoppingCartType.Auctions);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return Challenge();

            if (!cart.RequiresShipping())
            {
                await _genericAttributeService.SaveAttribute<ShippingOption>(_workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedShippingOption, null, _storeContext.CurrentStore.Id);
                return RedirectToRoute("CheckoutPaymentMethod");
            }

            //model
            var model = await _mediator.Send(new GetShippingMethod() {
                Cart = cart,
                Currency = _workContext.WorkingCurrency,
                Customer = _workContext.CurrentCustomer,
                Language = _workContext.WorkingLanguage,
                ShippingAddress = _workContext.CurrentCustomer.ShippingAddress,
                Store = _storeContext.CurrentStore
            });

            if (_shippingSettings.BypassShippingMethodSelectionIfOnlyOne &&
                model.ShippingMethods.Count == 1)
            {
                //if we have only one shipping method, then a customer doesn't have to choose a shipping method
                await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                    SystemCustomerAttributeNames.SelectedShippingOption,
                    model.ShippingMethods.First().ShippingOption,
                    _storeContext.CurrentStore.Id);

                return RedirectToRoute("CheckoutPaymentMethod");
            }

            return View(model);
        }
        [HttpPost, ActionName("ShippingMethod")]
        [FormValueRequired("nextstep")]
        public virtual async Task<IActionResult> SelectShippingMethod(IFormCollection form)
        {
            //validation
            var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, ShoppingCartType.ShoppingCart, ShoppingCartType.Auctions);

            var customer = _workContext.CurrentCustomer;
            var store = _storeContext.CurrentStore;

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if ((customer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return Challenge();

            if (!cart.RequiresShipping())
            {
                await _genericAttributeService.SaveAttribute<ShippingOption>(customer,
                    SystemCustomerAttributeNames.SelectedShippingOption, null, store.Id);
                return RedirectToRoute("CheckoutPaymentMethod");
            }

            //parse selected method 
            if (String.IsNullOrEmpty(form["shippingoption"]))
                return await ShippingMethod();
            var splittedOption = form["shippingoption"].ToString().Split(new[] { "___" }, StringSplitOptions.RemoveEmptyEntries);
            if (splittedOption.Length != 2)
                return await ShippingMethod();
            string selectedName = splittedOption[0];
            string shippingRateComputationMethodSystemName = splittedOption[1];

            //clear shipping option XML/Description
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ShippingOptionAttributeXml, "", store.Id);
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ShippingOptionAttributeDescription, "", store.Id);

            //validate customer's input
            var warnings = (await ValidateShippingForm(form)).ToList();

            //find it
            //performance optimization. try cache first
            var shippingOptions = await customer.GetAttribute<List<ShippingOption>>(_genericAttributeService, SystemCustomerAttributeNames.OfferedShippingOptions, store.Id);
            if (shippingOptions == null || shippingOptions.Count == 0)
            {
                //not found? let's load them using shipping service
                shippingOptions = (await _shippingService
                    .GetShippingOptions(customer, cart, customer.ShippingAddress, shippingRateComputationMethodSystemName, store))
                    .ShippingOptions
                    .ToList();
            }
            else
            {
                //loaded cached results. let's filter result by a chosen shipping rate computation method
                shippingOptions = shippingOptions.Where(so => so.ShippingRateComputationMethodSystemName.Equals(shippingRateComputationMethodSystemName, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var shippingOption = shippingOptions
                .Find(so => !String.IsNullOrEmpty(so.Name) && so.Name.Equals(selectedName, StringComparison.OrdinalIgnoreCase));
            if (shippingOption == null)
                return await ShippingMethod();

            //save
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.SelectedShippingOption, shippingOption, store.Id);

            if (ModelState.IsValid)
            {
                return RedirectToRoute("CheckoutPaymentMethod");
            }

            var model = await _mediator.Send(new GetShippingMethod() {
                Cart = cart,
                Currency = _workContext.WorkingCurrency,
                Customer = _workContext.CurrentCustomer,
                Language = _workContext.WorkingLanguage,
                ShippingAddress = _workContext.CurrentCustomer.ShippingAddress,
                Store = _storeContext.CurrentStore
            });

            return View(model);
        }


        public virtual async Task<IActionResult> PaymentMethod()
        {
            //validation
            var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, ShoppingCartType.ShoppingCart, ShoppingCartType.Auctions);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return Challenge();

            //Check whether payment workflow is required
            //we ignore reward points during cart total calculation
            bool isPaymentWorkflowRequired = await _mediator.Send(new GetIsPaymentWorkflowRequired() { Cart = cart, UseRewardPoints = false });
            if (!isPaymentWorkflowRequired)
            {
                await _genericAttributeService.SaveAttribute<string>(_workContext.CurrentCustomer,
                    SystemCustomerAttributeNames.SelectedPaymentMethod, null, _storeContext.CurrentStore.Id);
                return RedirectToRoute("CheckoutPaymentInfo");
            }

            //filter by country
            var paymentMethodModel = await GetCheckoutPaymentMethodModel(cart);

            if (_paymentSettings.BypassPaymentMethodSelectionIfOnlyOne &&
                paymentMethodModel.PaymentMethods.Count == 1 && !paymentMethodModel.DisplayRewardPoints)
            {
                //if we have only one payment method and reward points are disabled or the current customer doesn't have any reward points
                //so customer doesn't have to choose a payment method

                await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                    SystemCustomerAttributeNames.SelectedPaymentMethod,
                    paymentMethodModel.PaymentMethods[0].PaymentMethodSystemName,
                    _storeContext.CurrentStore.Id);
                return RedirectToRoute("CheckoutPaymentInfo");
            }

            return View(paymentMethodModel);
        }

        [HttpPost, ActionName("PaymentMethod")]
        [FormValueRequired("nextstep")]
        public virtual async Task<IActionResult> SelectPaymentMethod(string paymentmethod, CheckoutPaymentMethodModel model)
        {
            //validation
            var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, ShoppingCartType.ShoppingCart, ShoppingCartType.Auctions);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return Challenge();

            //reward points
            if (_rewardPointsSettings.Enabled)
            {
                await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                    SystemCustomerAttributeNames.UseRewardPointsDuringCheckout, model.UseRewardPoints,
                    _storeContext.CurrentStore.Id);
            }

            //Check whether payment workflow is required
            bool isPaymentWorkflowRequired = await _mediator.Send(new GetIsPaymentWorkflowRequired() { Cart = cart });
            if (!isPaymentWorkflowRequired)
            {
                await _genericAttributeService.SaveAttribute<string>(_workContext.CurrentCustomer,
                    SystemCustomerAttributeNames.SelectedPaymentMethod, null, _storeContext.CurrentStore.Id);
                return RedirectToRoute("CheckoutPaymentInfo");
            }
            //payment method 
            if (String.IsNullOrEmpty(paymentmethod))
                return await PaymentMethod();

            var paymentMethodInst = _paymentService.LoadPaymentMethodBySystemName(paymentmethod);
            if (paymentMethodInst == null ||
                !paymentMethodInst.IsPaymentMethodActive(_paymentSettings) ||
                !_pluginFinder.AuthenticateStore(paymentMethodInst.PluginDescriptor, _storeContext.CurrentStore.Id))
                return await PaymentMethod();

            //save
            await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                SystemCustomerAttributeNames.SelectedPaymentMethod, paymentmethod, _storeContext.CurrentStore.Id);

            return RedirectToRoute("CheckoutPaymentInfo");
        }

        public virtual async Task<IActionResult> PaymentInfo()
        {
            //validation
            var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, ShoppingCartType.ShoppingCart, ShoppingCartType.Auctions);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return Challenge();

            //Check whether payment workflow is required
            bool isPaymentWorkflowRequired = await _mediator.Send(new GetIsPaymentWorkflowRequired() { Cart = cart });
            if (!isPaymentWorkflowRequired)
            {
                return RedirectToRoute("CheckoutConfirm");
            }

            //load payment method
            var paymentMethodSystemName = await _workContext.CurrentCustomer.GetAttribute<string>(
                _genericAttributeService, SystemCustomerAttributeNames.SelectedPaymentMethod,
                _storeContext.CurrentStore.Id);
            var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(paymentMethodSystemName);
            if (paymentMethod == null)
                return RedirectToRoute("CheckoutPaymentMethod");

            //Check whether payment info should be skipped
            if (await paymentMethod.SkipPaymentInfo() ||
                    (paymentMethod.PaymentMethodType == PaymentMethodType.Redirection
                    && _paymentSettings.SkipPaymentInfoStepForRedirectionPaymentMethods))
            {
                //skip payment info page
                var paymentInfo = new ProcessPaymentRequest();
                //session save
                HttpContext.Session.Set("OrderPaymentInfo", paymentInfo);
                return RedirectToRoute("CheckoutConfirm");
            }

            //model
            var model = await _mediator.Send(new GetPaymentInfo() { PaymentMethod = paymentMethod });
            return View(model);
        }

        [HttpPost, ActionName("PaymentInfo")]
        [FormValueRequired("nextstep")]
        public virtual async Task<IActionResult> EnterPaymentInfo(IFormCollection form)
        {
            //validation
            var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, ShoppingCartType.ShoppingCart, ShoppingCartType.Auctions);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return Challenge();

            //Check whether payment workflow is required
            bool isPaymentWorkflowRequired = await _mediator.Send(new GetIsPaymentWorkflowRequired() { Cart = cart });
            if (!isPaymentWorkflowRequired)
            {
                return RedirectToRoute("CheckoutConfirm");
            }

            //load payment method
            var paymentMethodSystemName = await _workContext.CurrentCustomer.GetAttribute<string>(
                _genericAttributeService, SystemCustomerAttributeNames.SelectedPaymentMethod,
                _storeContext.CurrentStore.Id);
            var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(paymentMethodSystemName);
            if (paymentMethod == null)
                return RedirectToRoute("CheckoutPaymentMethod");

            var warnings = await paymentMethod.ValidatePaymentForm(form);
            foreach (var warning in warnings)
                ModelState.AddModelError("", warning);
            if (ModelState.IsValid)
            {
                //get payment info
                var paymentInfo = await paymentMethod.GetPaymentInfo(form);
                //session save
                HttpContext.Session.Set("OrderPaymentInfo", paymentInfo);
                return RedirectToRoute("CheckoutConfirm");
            }

            //If we got this far, something failed, redisplay form
            var model = await _mediator.Send(new GetPaymentInfo() { PaymentMethod = paymentMethod });

            return View(model);
        }


        public virtual async Task<IActionResult> Confirm()
        {
            //validation
            var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, ShoppingCartType.ShoppingCart, ShoppingCartType.Auctions);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return Challenge();

            //model
            var model = await _mediator.Send(new GetConfirmOrder() { Cart = cart, Customer = _workContext.CurrentCustomer });
            return View(model);
        }

        [HttpPost, ActionName("Confirm")]
        public virtual async Task<IActionResult> ConfirmOrder([FromServices] IOrderProcessingService orderProcessingService)
        {
            //validation
            var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, ShoppingCartType.ShoppingCart, ShoppingCartType.Auctions);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return Challenge();

            //model
            var model = await _mediator.Send(new GetConfirmOrder() { Cart = cart, Customer = _workContext.CurrentCustomer });
            try
            {
                var processPaymentRequest = HttpContext.Session.Get<ProcessPaymentRequest>("OrderPaymentInfo");
                if (processPaymentRequest == null)
                {
                    //Check whether payment workflow is required
                    if (await _mediator.Send(new GetIsPaymentWorkflowRequired() { Cart = cart }))
                        return RedirectToRoute("CheckoutPaymentInfo");

                    processPaymentRequest = new ProcessPaymentRequest();
                }

                //prevent 2 orders being placed within an X seconds time frame
                if (!await _mediator.Send(new GetMinOrderPlaceIntervalValid() {
                    Customer = _workContext.CurrentCustomer,
                    Store = _storeContext.CurrentStore
                }))
                    throw new Exception(_localizationService.GetResource("Checkout.MinOrderPlacementInterval"));

                //place order
                processPaymentRequest.StoreId = _storeContext.CurrentStore.Id;
                processPaymentRequest.CustomerId = _workContext.CurrentCustomer.Id;
                processPaymentRequest.PaymentMethodSystemName = await _workContext.CurrentCustomer.GetAttribute<string>(
                    _genericAttributeService, SystemCustomerAttributeNames.SelectedPaymentMethod,
                    _storeContext.CurrentStore.Id);
                var placeOrderResult = await orderProcessingService.PlaceOrder(processPaymentRequest);
                if (placeOrderResult.Success)
                {
                    await _customerActivityService.InsertActivity("PublicStore.PlaceOrder", "", _localizationService.GetResource("ActivityLog.PublicStore.PlaceOrder"), placeOrderResult.PlacedOrder.Id);

                    HttpContext.Session.Set<ProcessPaymentRequest>("OrderPaymentInfo", null);
                    var postProcessPaymentRequest = new PostProcessPaymentRequest {
                        Order = placeOrderResult.PlacedOrder
                    };
                    await _paymentService.PostProcessPayment(postProcessPaymentRequest);

                    if (_webHelper.IsRequestBeingRedirected || _webHelper.IsPostBeingDone)
                    {
                        //redirection or POST has been done in PostProcessPayment
                        return Content("Redirected");
                    }

                    return RedirectToRoute("CheckoutCompleted", new { orderId = placeOrderResult.PlacedOrder.Id });
                }

                foreach (var error in placeOrderResult.Errors)
                    model.Warnings.Add(error);
            }
            catch (Exception exc)
            {
                _logger.Warning(exc.Message, exc);
                model.Warnings.Add(exc.Message);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual IActionResult GetShippingFormPartialView(string shippingOption)
        {
            GetShippingComputation(shippingOption).GetPublicViewComponent(out string viewcomponent);
            if (string.IsNullOrEmpty(viewcomponent))
                return Content("");

            var component = RenderViewComponentToString(viewcomponent, new { shippingOption = shippingOption });
            return Content(component);
        }


        #endregion

        #region Methods (one page checkout)

        private void OpcCartValidate(IList<ShoppingCartItem> cart)
        {
            if (!cart.Any())
                throw new Exception("Your cart is empty");

            if (!_orderSettings.OnePageCheckoutEnabled)
                throw new Exception("One page checkout is disabled");

            if (_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
                throw new Exception("Anonymous checkout is not allowed");
        }

        [NonAction]
        protected async Task<JsonResult> OpcLoadStepAfterShippingAddress(IList<ShoppingCartItem> cart)
        {
            var shippingMethodModel = await _mediator.Send(new GetShippingMethod() {
                Cart = cart,
                Currency = _workContext.WorkingCurrency,
                Customer = _workContext.CurrentCustomer,
                Language = _workContext.WorkingLanguage,
                ShippingAddress = _workContext.CurrentCustomer.ShippingAddress,
                Store = _storeContext.CurrentStore
            });

            if (_shippingSettings.BypassShippingMethodSelectionIfOnlyOne &&
                shippingMethodModel.ShippingMethods.Count == 1)
            {
                //if we have only one shipping method, then a customer doesn't have to choose a shipping method
                await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                    SystemCustomerAttributeNames.SelectedShippingOption,
                    shippingMethodModel.ShippingMethods.First().ShippingOption,
                    _storeContext.CurrentStore.Id);

                //load next step
                return await OpcLoadStepAfterShippingMethod(cart);
            }


            return Json(new
            {
                update_section = new UpdateSectionJsonModel {
                    name = "shipping-method",
                    html = await RenderPartialViewToString("OpcShippingMethods", shippingMethodModel)
                },
                goto_section = "shipping_method"
            });
        }

        [NonAction]
        protected async Task<JsonResult> OpcLoadStepAfterShippingMethod(IList<ShoppingCartItem> cart)
        {
            //Check whether payment workflow is required
            //we ignore reward points during cart total calculation
            bool isPaymentWorkflowRequired = await _mediator.Send(new GetIsPaymentWorkflowRequired() { Cart = cart, UseRewardPoints = false });
            if (isPaymentWorkflowRequired)
            {
                //filter by country
                string filterByCountryId = "";
                if (_addressSettings.CountryEnabled &&
                    _workContext.CurrentCustomer.BillingAddress != null &&
                    !String.IsNullOrEmpty(_workContext.CurrentCustomer.BillingAddress.CountryId))
                {
                    filterByCountryId = _workContext.CurrentCustomer.BillingAddress.CountryId;
                }

                //payment is required
                var paymentMethodModel = await _mediator.Send(new GetPaymentMethod() {
                    Cart = cart,
                    Currency = _workContext.WorkingCurrency,
                    Customer = _workContext.CurrentCustomer,
                    FilterByCountryId = filterByCountryId,
                    Language = _workContext.WorkingLanguage,
                    Store = _storeContext.CurrentStore
                });

                if (_paymentSettings.BypassPaymentMethodSelectionIfOnlyOne &&
                    paymentMethodModel.PaymentMethods.Count == 1 && !paymentMethodModel.DisplayRewardPoints)
                {
                    //if we have only one payment method and reward points are disabled or the current customer doesn't have any reward points
                    //so customer doesn't have to choose a payment method

                    var selectedPaymentMethodSystemName = paymentMethodModel.PaymentMethods[0].PaymentMethodSystemName;
                    await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                        SystemCustomerAttributeNames.SelectedPaymentMethod,
                        selectedPaymentMethodSystemName, _storeContext.CurrentStore.Id);

                    var paymentMethodInst = _paymentService.LoadPaymentMethodBySystemName(selectedPaymentMethodSystemName);
                    if (paymentMethodInst == null ||
                        !paymentMethodInst.IsPaymentMethodActive(_paymentSettings) ||
                        !_pluginFinder.AuthenticateStore(paymentMethodInst.PluginDescriptor, _storeContext.CurrentStore.Id))
                        throw new Exception("Selected payment method can't be parsed");

                    return await OpcLoadStepAfterPaymentMethod(paymentMethodInst, cart);
                }

                //customer have to choose a payment method
                return Json(new
                {
                    update_section = new UpdateSectionJsonModel {
                        name = "payment-method",
                        html = await RenderPartialViewToString("OpcPaymentMethods", paymentMethodModel)
                    },
                    goto_section = "payment_method"
                });
            }

            //payment is not required
            await _genericAttributeService.SaveAttribute<string>(_workContext.CurrentCustomer,
                SystemCustomerAttributeNames.SelectedPaymentMethod, null, _storeContext.CurrentStore.Id);

            var confirmOrderModel = await _mediator.Send(new GetConfirmOrder() { Cart = cart, Customer = _workContext.CurrentCustomer });
            return Json(new
            {
                update_section = new UpdateSectionJsonModel {
                    name = "confirm-order",
                    html = await RenderPartialViewToString("OpcConfirmOrder", confirmOrderModel)
                },
                goto_section = "confirm_order"
            });
        }

        [NonAction]
        protected async Task<JsonResult> OpcLoadStepAfterPaymentMethod(IPaymentMethod paymentMethod, IList<ShoppingCartItem> cart)
        {
            if (await paymentMethod.SkipPaymentInfo() ||
                    (paymentMethod.PaymentMethodType == PaymentMethodType.Redirection
                    && _paymentSettings.SkipPaymentInfoStepForRedirectionPaymentMethods))
            {
                //skip payment info page
                var paymentInfo = new ProcessPaymentRequest();
                //session save
                HttpContext.Session.Set("OrderPaymentInfo", paymentInfo);

                var confirmOrderModel = await _mediator.Send(new GetConfirmOrder() { Cart = cart, Customer = _workContext.CurrentCustomer });
                return Json(new
                {
                    update_section = new UpdateSectionJsonModel {
                        name = "confirm-order",
                        html = await RenderPartialViewToString("OpcConfirmOrder", confirmOrderModel)
                    },
                    goto_section = "confirm_order"
                });
            }

            //return payment info page
            var paymenInfoModel = await _mediator.Send(new GetPaymentInfo() { PaymentMethod = paymentMethod });
            return Json(new
            {
                update_section = new UpdateSectionJsonModel {
                    name = "payment-info",
                    html = await RenderPartialViewToString("OpcPaymentInfo", paymenInfoModel)
                },
                goto_section = "payment_info"
            });
        }

        public virtual async Task<IActionResult> OnePageCheckout()
        {
            //validation
            var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, ShoppingCartType.ShoppingCart, ShoppingCartType.Auctions);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            if (!_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("Checkout");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return Challenge();

            var paymentMethodModel = await GetCheckoutPaymentMethodModel(cart);

            var model = new OnePageCheckoutModel {
                ShippingRequired = cart.RequiresShipping(),
                DisableBillingAddressCheckoutStep = _orderSettings.DisableBillingAddressCheckoutStep,
                BillingAddress = await _mediator.Send(new GetBillingAddress() {
                    Cart = cart,
                    Currency = _workContext.WorkingCurrency,
                    Customer = _workContext.CurrentCustomer,
                    Language = _workContext.WorkingLanguage,
                    Store = _storeContext.CurrentStore,
                    PrePopulateNewAddressWithCustomerFields = true
                }),
                HasSinglePaymentMethod = paymentMethodModel.PaymentMethods?.Count == 1
            };
            return View(model);
        }

        public virtual async Task<IActionResult> OpcSaveBilling(IFormCollection form)
        {
            try
            {
                //validation
                var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, ShoppingCartType.ShoppingCart, ShoppingCartType.Auctions);
                OpcCartValidate(cart);

                string billingAddressId = form["billing_address_id"];

                if (!String.IsNullOrEmpty(billingAddressId))
                {
                    //existing address
                    var address = _workContext.CurrentCustomer.Addresses.FirstOrDefault(a => a.Id == billingAddressId);
                    if (address == null)
                        throw new Exception("Address can't be loaded");

                    _workContext.CurrentCustomer.BillingAddress = address;
                    address.CustomerId = _workContext.CurrentCustomer.Id;
                    await _customerService.UpdateBillingAddress(address);
                }
                else
                {
                    //new address
                    var model = new CheckoutBillingAddressModel();
                    await TryUpdateModelAsync(model.NewAddress, "BillingNewAddress");

                    //custom address attributes
                    var customAttributes = await _mediator.Send(new GetParseCustomAddressAttributes() { Form = form });
                    var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarnings(customAttributes);
                    foreach (var error in customAttributeWarnings)
                    {
                        ModelState.AddModelError("", error);
                    }

                    //validate model
                    TryValidateModel(model.NewAddress);
                    if (!ModelState.IsValid)
                    {
                        //model is not valid. redisplay the form with errors
                        var billingAddressModel = await _mediator.Send(new GetBillingAddress() {
                            Cart = cart,
                            Currency = _workContext.WorkingCurrency,
                            Customer = _workContext.CurrentCustomer,
                            Language = _workContext.WorkingLanguage,
                            Store = _storeContext.CurrentStore,
                            SelectedCountryId = model.NewAddress.CountryId,
                            OverrideAttributesXml = customAttributes
                        });

                        billingAddressModel.NewAddressPreselected = true;
                        return Json(new
                        {
                            update_section = new UpdateSectionJsonModel {
                                name = "billing",
                                html = await RenderPartialViewToString("OpcBillingAddress", billingAddressModel)
                            },
                            wrong_billing_address = true,
                        });
                    }

                    //try to find an address with the same values (don't duplicate records)
                    var address = _workContext.CurrentCustomer.Addresses.ToList().FindAddress(
                        model.NewAddress.FirstName, model.NewAddress.LastName, model.NewAddress.PhoneNumber,
                        model.NewAddress.Email, model.NewAddress.FaxNumber, model.NewAddress.Company,
                        model.NewAddress.Address1, model.NewAddress.Address2, model.NewAddress.City,
                        model.NewAddress.StateProvinceId, model.NewAddress.ZipPostalCode,
                        model.NewAddress.CountryId, customAttributes);
                    if (address == null)
                    {
                        //address is not found. let's create a new one
                        address = model.NewAddress.ToEntity();
                        address.CustomAttributes = customAttributes;
                        address.CreatedOnUtc = DateTime.UtcNow;
                        _workContext.CurrentCustomer.Addresses.Add(address);
                        address.CustomerId = _workContext.CurrentCustomer.Id;
                        await _customerService.InsertAddress(address);
                    }
                    _workContext.CurrentCustomer.BillingAddress = address;
                    address.CustomerId = _workContext.CurrentCustomer.Id;
                    await _customerService.UpdateBillingAddress(address);
                }

                if (cart.RequiresShipping())
                {
                    //shipping is required

                    var model = new CheckoutBillingAddressModel();
                    await TryUpdateModelAsync(model);
                    if (_shippingSettings.ShipToSameAddress && model.ShipToSameAddress)
                    {
                        _workContext.CurrentCustomer.ShippingAddress = _workContext.CurrentCustomer.BillingAddress;
                        await _customerService.UpdateShippingAddress(_workContext.CurrentCustomer.BillingAddress);
                        await _genericAttributeService.SaveAttribute<ShippingOption>(_workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedShippingOption, null, _storeContext.CurrentStore.Id);
                        await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedPickupPoint, "", _storeContext.CurrentStore.Id);
                        return await OpcLoadStepAfterShippingAddress(cart);
                    }
                    else
                    {
                        var shippingAddressModel = await _mediator.Send(new GetShippingAddress() {
                            Currency = _workContext.WorkingCurrency,
                            Customer = _workContext.CurrentCustomer,
                            Language = _workContext.WorkingLanguage,
                            Store = _storeContext.CurrentStore,
                            PrePopulateNewAddressWithCustomerFields = true
                        });

                        if (_shippingSettings.AllowPickUpInStore && !(await _shippingService.LoadActiveShippingRateComputationMethods(_storeContext.CurrentStore.Id)).Any())
                        {
                            shippingAddressModel.PickUpInStoreOnly = true;
                            shippingAddressModel.PickUpInStore = true;
                        }

                        return Json(new
                        {
                            update_section = new UpdateSectionJsonModel {
                                name = "shipping",
                                html = await RenderPartialViewToString("OpcShippingAddress", shippingAddressModel)
                            },
                            goto_section = "shipping"
                        });
                    }

                }
                //shipping is not required
                _workContext.CurrentCustomer.ShippingAddress = null;
                await _customerService.RemoveShippingAddress(_workContext.CurrentCustomer.Id);
                await _genericAttributeService.SaveAttribute<ShippingOption>(_workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedShippingOption, null, _storeContext.CurrentStore.Id);
                //load next step
                return await OpcLoadStepAfterShippingMethod(cart);
            }
            catch (Exception exc)
            {
                _logger.Warning(exc.Message, exc, _workContext.CurrentCustomer);
                return Json(new { error = 1, message = exc.Message });
            }
        }

        public virtual async Task<IActionResult> OpcSaveShipping(CheckoutShippingAddressModel model, IFormCollection form)
        {
            try
            {
                //validation
                var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, ShoppingCartType.ShoppingCart, ShoppingCartType.Auctions);
                OpcCartValidate(cart);

                if (!cart.RequiresShipping())
                    throw new Exception("Shipping is not required");

                //Pick up in store?
                if (_shippingSettings.AllowPickUpInStore)
                {

                    if (model.PickUpInStore)
                    {
                        //customer decided to pick up in store

                        //no shipping address selected
                        _workContext.CurrentCustomer.ShippingAddress = null;
                        await _customerService.RemoveShippingAddress(_workContext.CurrentCustomer.Id);

                        //clear shipping option XML/Description
                        await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.ShippingOptionAttributeXml, "", _storeContext.CurrentStore.Id);
                        await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.ShippingOptionAttributeDescription, "", _storeContext.CurrentStore.Id);


                        var pickupPoint = form["pickup-point-id"];
                        var pickupPoints = await _shippingService.LoadActivePickupPoints(_storeContext.CurrentStore.Id);
                        var selectedPoint = pickupPoints.FirstOrDefault(x => x.Id.Equals(pickupPoint));
                        if (selectedPoint == null)
                            throw new Exception("Pickup point is not allowed");

                        //save "pick up in store" shipping method
                        var pickUpInStoreShippingOption = new ShippingOption {
                            Name = string.Format(_localizationService.GetResource("Checkout.PickupPoints.Name"), selectedPoint.Name),
                            Rate = selectedPoint.PickupFee,
                            Description = selectedPoint.Description,
                            ShippingRateComputationMethodSystemName = string.Format("PickupPoint_{0}", selectedPoint.Id)
                        };

                        await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                        SystemCustomerAttributeNames.SelectedShippingOption,
                        pickUpInStoreShippingOption,
                        _storeContext.CurrentStore.Id);

                        await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                        SystemCustomerAttributeNames.SelectedPickupPoint,
                        selectedPoint.Id,
                        _storeContext.CurrentStore.Id);

                        //load next step
                        return await OpcLoadStepAfterShippingMethod(cart);
                    }

                    //set value indicating that "pick up in store" option has not been chosen
                    await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedPickupPoint, "", _storeContext.CurrentStore.Id);

                }

                string shippingAddressId = form["shipping_address_id"];

                if (!String.IsNullOrEmpty(shippingAddressId))
                {
                    //existing address
                    var address = _workContext.CurrentCustomer.Addresses.FirstOrDefault(a => a.Id == shippingAddressId);
                    if (address == null)
                        throw new Exception("Address can't be loaded");

                    _workContext.CurrentCustomer.ShippingAddress = address;
                    address.CustomerId = _workContext.CurrentCustomer.Id;
                    await _customerService.UpdateShippingAddress(address);
                }
                else
                {
                    ModelState.Clear();
                    //new address
                    await TryUpdateModelAsync(model.NewAddress, "ShippingNewAddress");
                    //custom address attributes
                    var customAttributes = await _mediator.Send(new GetParseCustomAddressAttributes() { Form = form });
                    var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarnings(customAttributes);
                    foreach (var error in customAttributeWarnings)
                    {
                        ModelState.AddModelError("", error);
                    }

                    //validate model
                    TryValidateModel(model.NewAddress);
                    if (!ModelState.IsValid)
                    {
                        var xx = ModelState.Values.SelectMany(v => v.Errors);
                        foreach (var item in xx)
                        {
                            string tt = item.ErrorMessage;
                        }
                        //model is not valid. redisplay the form with errors
                        var shippingAddressModel = await _mediator.Send(new GetShippingAddress() {
                            Currency = _workContext.WorkingCurrency,
                            Customer = _workContext.CurrentCustomer,
                            Language = _workContext.WorkingLanguage,
                            Store = _storeContext.CurrentStore,
                            SelectedCountryId = model.NewAddress.CountryId,
                            OverrideAttributesXml = customAttributes,
                        });

                        shippingAddressModel.NewAddressPreselected = true;
                        return Json(new
                        {
                            update_section = new UpdateSectionJsonModel {
                                name = "shipping",
                                html = await RenderPartialViewToString("OpcShippingAddress", shippingAddressModel)
                            }
                        });
                    }

                    //try to find an address with the same values (don't duplicate records)
                    var address = _workContext.CurrentCustomer.Addresses.ToList().FindAddress(
                        model.NewAddress.FirstName, model.NewAddress.LastName, model.NewAddress.PhoneNumber,
                        model.NewAddress.Email, model.NewAddress.FaxNumber, model.NewAddress.Company,
                        model.NewAddress.Address1, model.NewAddress.Address2, model.NewAddress.City,
                        model.NewAddress.StateProvinceId, model.NewAddress.ZipPostalCode,
                        model.NewAddress.CountryId, customAttributes);
                    if (address == null)
                    {
                        address = model.NewAddress.ToEntity();
                        address.CustomAttributes = customAttributes;
                        address.CreatedOnUtc = DateTime.UtcNow;

                        //other null validations
                        _workContext.CurrentCustomer.Addresses.Add(address);
                        address.CustomerId = _workContext.CurrentCustomer.Id;
                        await _customerService.InsertAddress(address);
                    }
                    _workContext.CurrentCustomer.ShippingAddress = address;
                    address.CustomerId = _workContext.CurrentCustomer.Id;
                    await _customerService.UpdateShippingAddress(address);
                }

                return await OpcLoadStepAfterShippingAddress(cart);
            }
            catch (Exception exc)
            {
                _logger.Warning(exc.Message, exc, _workContext.CurrentCustomer);
                return Json(new { error = 1, message = exc.Message });
            }
        }

        public virtual async Task<IActionResult> OpcSaveShippingMethod(IFormCollection form)
        {
            try
            {
                //validation
                var customer = _workContext.CurrentCustomer;
                var store = _storeContext.CurrentStore;

                var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, ShoppingCartType.ShoppingCart, ShoppingCartType.Auctions);
                OpcCartValidate(cart);

                if (!cart.RequiresShipping())
                    throw new Exception("Shipping is not required");

                //parse selected method 
                string shippingoption = form["shippingoption"];
                if (String.IsNullOrEmpty(shippingoption))
                    throw new Exception("Selected shipping method can't be parsed");
                var splittedOption = shippingoption.Split(new[] { "___" }, StringSplitOptions.RemoveEmptyEntries);
                if (splittedOption.Length != 2)
                    throw new Exception("Selected shipping method can't be parsed");
                string selectedName = splittedOption[0];
                string shippingRateComputationMethodSystemName = splittedOption[1];

                //clear shipping option XML/Description
                await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ShippingOptionAttributeXml, "", store.Id);
                await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ShippingOptionAttributeDescription, "", store.Id);

                //validate customer's input
                var warnings = (await ValidateShippingForm(form)).ToList();

                //find it
                //performance optimization. try cache first
                var shippingOptions = await customer.GetAttribute<List<ShippingOption>>(_genericAttributeService, SystemCustomerAttributeNames.OfferedShippingOptions, store.Id);
                if (shippingOptions == null || shippingOptions.Count == 0)
                {
                    //not found? let's load them using shipping service
                    shippingOptions = (await _shippingService
                        .GetShippingOptions(customer, cart, customer.ShippingAddress, shippingRateComputationMethodSystemName, store))
                        .ShippingOptions
                        .ToList();
                }
                else
                {
                    //loaded cached results. let's filter result by a chosen shipping rate computation method
                    shippingOptions = shippingOptions.Where(so => so.ShippingRateComputationMethodSystemName.Equals(shippingRateComputationMethodSystemName, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                var shippingOption = shippingOptions
                    .Find(so => !String.IsNullOrEmpty(so.Name) && so.Name.Equals(selectedName, StringComparison.OrdinalIgnoreCase));
                if (shippingOption == null)
                    throw new Exception("Selected shipping method can't be loaded");

                //save
                await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.SelectedShippingOption, shippingOption, store.Id);

                if (ModelState.IsValid)
                {
                    //load next step
                    return await OpcLoadStepAfterShippingMethod(cart);
                }

                var message = String.Join(", ", warnings.ToArray());
                return Json(new { error = 1, message = message });
            }
            catch (Exception exc)
            {
                _logger.Warning(exc.Message, exc, _workContext.CurrentCustomer);
                return Json(new { error = 1, message = exc.Message });
            }
        }

        public virtual async Task<IActionResult> OpcSavePaymentMethod(IFormCollection form)
        {
            try
            {
                //validation
                var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, ShoppingCartType.ShoppingCart, ShoppingCartType.Auctions);
                OpcCartValidate(cart);

                string paymentmethod = form["paymentmethod"];
                //payment method 
                if (String.IsNullOrEmpty(paymentmethod))
                    throw new Exception("Selected payment method can't be parsed");


                var model = new CheckoutPaymentMethodModel();
                await TryUpdateModelAsync(model);

                //reward points
                if (_rewardPointsSettings.Enabled)
                {
                    await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                        SystemCustomerAttributeNames.UseRewardPointsDuringCheckout, model.UseRewardPoints,
                        _storeContext.CurrentStore.Id);
                }

                //Check whether payment workflow is required
                bool isPaymentWorkflowRequired = await _mediator.Send(new GetIsPaymentWorkflowRequired() { Cart = cart });
                if (!isPaymentWorkflowRequired)
                {
                    //payment is not required
                    await _genericAttributeService.SaveAttribute<string>(_workContext.CurrentCustomer,
                        SystemCustomerAttributeNames.SelectedPaymentMethod, null, _storeContext.CurrentStore.Id);

                    var confirmOrderModel = await _mediator.Send(new GetConfirmOrder() { Cart = cart, Customer = _workContext.CurrentCustomer });
                    return Json(new
                    {
                        update_section = new UpdateSectionJsonModel {
                            name = "confirm-order",
                            html = await RenderPartialViewToString("OpcConfirmOrder", confirmOrderModel)
                        },
                        goto_section = "confirm_order"
                    });
                }

                var paymentMethodInst = _paymentService.LoadPaymentMethodBySystemName(paymentmethod);
                if (paymentMethodInst == null ||
                    !paymentMethodInst.IsPaymentMethodActive(_paymentSettings) ||
                    !_pluginFinder.AuthenticateStore(paymentMethodInst.PluginDescriptor, _storeContext.CurrentStore.Id))
                    throw new Exception("Selected payment method can't be parsed");

                //save
                await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                    SystemCustomerAttributeNames.SelectedPaymentMethod, paymentmethod, _storeContext.CurrentStore.Id);

                return await OpcLoadStepAfterPaymentMethod(paymentMethodInst, cart);
            }
            catch (Exception exc)
            {
                _logger.Warning(exc.Message, exc, _workContext.CurrentCustomer);
                return Json(new { error = 1, message = exc.Message });
            }
        }

        public virtual async Task<IActionResult> OpcSavePaymentInfo(IFormCollection form)
        {
            try
            {
                //validation
                var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, ShoppingCartType.ShoppingCart, ShoppingCartType.Auctions);
                OpcCartValidate(cart);

                var paymentMethodSystemName = await _workContext.CurrentCustomer.GetAttribute<string>(
                    _genericAttributeService, SystemCustomerAttributeNames.SelectedPaymentMethod,
                    _storeContext.CurrentStore.Id);
                var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(paymentMethodSystemName);
                if (paymentMethod == null)
                    throw new Exception("Payment method is not selected");

                var warnings = await paymentMethod.ValidatePaymentForm(form);
                foreach (var warning in warnings)
                    ModelState.AddModelError("", warning);
                if (ModelState.IsValid)
                {
                    //get payment info
                    var paymentInfo = await paymentMethod.GetPaymentInfo(form);
                    //session save
                    HttpContext.Session.Set("OrderPaymentInfo", paymentInfo);

                    var confirmOrderModel = await _mediator.Send(new GetConfirmOrder() { Cart = cart, Customer = _workContext.CurrentCustomer });
                    return Json(new
                    {
                        update_section = new UpdateSectionJsonModel {
                            name = "confirm-order",
                            html = await RenderPartialViewToString("OpcConfirmOrder", confirmOrderModel)
                        },
                        goto_section = "confirm_order"
                    });
                }

                //If we got this far, something failed, redisplay form
                var paymenInfoModel = await _mediator.Send(new GetPaymentInfo() { PaymentMethod = paymentMethod });
                return Json(new
                {
                    update_section = new UpdateSectionJsonModel {
                        name = "payment-info",
                        html = await RenderPartialViewToString("OpcPaymentInfo", paymenInfoModel)
                    }
                });
            }
            catch (Exception exc)
            {
                _logger.Warning(exc.Message, exc, _workContext.CurrentCustomer);
                return Json(new { error = 1, message = exc.Message });
            }
        }

        public virtual async Task<IActionResult> OpcConfirmOrder([FromServices] IOrderProcessingService orderProcessingService)
        {
            try
            {
                //validation
                var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, ShoppingCartType.ShoppingCart, ShoppingCartType.Auctions);
                OpcCartValidate(cart);

                //prevent 2 orders being placed within an X seconds time frame
                if (!await _mediator.Send(new GetMinOrderPlaceIntervalValid() {
                    Customer = _workContext.CurrentCustomer,
                    Store = _storeContext.CurrentStore
                }))
                    throw new Exception(_localizationService.GetResource("Checkout.MinOrderPlacementInterval"));

                //place order
                var processPaymentRequest = this.HttpContext.Session.Get<ProcessPaymentRequest>("OrderPaymentInfo");
                if (processPaymentRequest == null)
                {
                    //Check whether payment workflow is required
                    if (await _mediator.Send(new GetIsPaymentWorkflowRequired() { Cart = cart }))
                    {
                        throw new Exception("Payment information is not entered");
                    }
                    else
                        processPaymentRequest = new ProcessPaymentRequest();
                }

                processPaymentRequest.StoreId = _storeContext.CurrentStore.Id;
                processPaymentRequest.CustomerId = _workContext.CurrentCustomer.Id;
                processPaymentRequest.PaymentMethodSystemName = await _workContext.CurrentCustomer.GetAttribute<string>(
                    _genericAttributeService, SystemCustomerAttributeNames.SelectedPaymentMethod,
                    _storeContext.CurrentStore.Id);
                var placeOrderResult = await orderProcessingService.PlaceOrder(processPaymentRequest);
                if (placeOrderResult.Success)
                {
                    await _customerActivityService.InsertActivity("PublicStore.PlaceOrder", "", _localizationService.GetResource("ActivityLog.PublicStore.PlaceOrder"), placeOrderResult.PlacedOrder.Id);

                    HttpContext.Session.Set<ProcessPaymentRequest>("OrderPaymentInfo", null);
                    var postProcessPaymentRequest = new PostProcessPaymentRequest {
                        Order = placeOrderResult.PlacedOrder
                    };


                    var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(placeOrderResult.PlacedOrder.PaymentMethodSystemName);
                    if (paymentMethod == null)
                        //payment method could be null if order total is 0
                        //success
                        return Json(new { success = 1 });

                    if (paymentMethod.PaymentMethodType == PaymentMethodType.Redirection)
                    {
                        //Redirection will not work because it's AJAX request.
                        //That's why we don't process it here (we redirect a user to another page where he'll be redirected)

                        //redirect
                        return Json(new
                        {
                            redirect = string.Format("{0}checkout/OpcCompleteRedirectionPayment", _webHelper.GetStoreLocation())
                        });
                    }

                    await _paymentService.PostProcessPayment(postProcessPaymentRequest);
                    //success
                    return Json(new { success = 1 });
                }

                //error
                var confirmOrderModel = new CheckoutConfirmModel();
                foreach (var error in placeOrderResult.Errors)
                    confirmOrderModel.Warnings.Add(error);

                return Json(new
                {
                    update_section = new UpdateSectionJsonModel {
                        name = "confirm-order",
                        html = await RenderPartialViewToString("OpcConfirmOrder", confirmOrderModel)
                    },
                    goto_section = "confirm_order"
                });
            }
            catch (Exception exc)
            {
                _logger.Warning(exc.Message, exc, _workContext.CurrentCustomer);
                return Json(new { error = 1, message = exc.Message });
            }
        }

        public virtual async Task<IActionResult> OpcCompleteRedirectionPayment()
        {
            try
            {
                //validation
                if (!_orderSettings.OnePageCheckoutEnabled)
                    return RedirectToRoute("HomePage");

                if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                    return Challenge();

                //get the order
                var order = (await _orderService.SearchOrders(storeId: _storeContext.CurrentStore.Id,
                customerId: _workContext.CurrentCustomer.Id, pageSize: 1))
                    .FirstOrDefault();
                if (order == null)
                    return RedirectToRoute("HomePage");


                var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(order.PaymentMethodSystemName);
                if (paymentMethod == null)
                    return RedirectToRoute("HomePage");
                if (paymentMethod.PaymentMethodType != PaymentMethodType.Redirection)
                    return RedirectToRoute("HomePage");

                //ensure that order has been just placed
                if ((DateTime.UtcNow - order.CreatedOnUtc).TotalMinutes > 3)
                    return RedirectToRoute("HomePage");


                //Redirection will not work on one page checkout page because it's AJAX request.
                //That's why we process it here
                var postProcessPaymentRequest = new PostProcessPaymentRequest {
                    Order = order
                };

                await _paymentService.PostProcessPayment(postProcessPaymentRequest);

                if (_webHelper.IsRequestBeingRedirected || _webHelper.IsPostBeingDone)
                {
                    //redirection or POST has been done in PostProcessPayment
                    return Content("Redirected");
                }

                //if no redirection has been done (to a third-party payment page)
                //theoretically it's not possible
                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
            }
            catch (Exception exc)
            {
                _logger.Warning(exc.Message, exc, _workContext.CurrentCustomer);
                return Content(exc.Message);
            }
        }

        private async Task<CheckoutPaymentMethodModel> GetCheckoutPaymentMethodModel(IList<ShoppingCartItem> cart)
        {
            var filterByCountryId = "";
            if (_addressSettings.CountryEnabled &&
                _workContext.CurrentCustomer.BillingAddress != null &&
                !string.IsNullOrWhiteSpace(_workContext.CurrentCustomer.BillingAddress.CountryId))
            {
                filterByCountryId = _workContext.CurrentCustomer.BillingAddress.CountryId;
            }

            var paymentMethodModel = await _mediator.Send(new GetPaymentMethod() {
                Cart = cart,
                Currency = _workContext.WorkingCurrency,
                Customer = _workContext.CurrentCustomer,
                FilterByCountryId = filterByCountryId,
                Language = _workContext.WorkingLanguage,
                Store = _storeContext.CurrentStore
            });

            return paymentMethodModel;
        }

        #endregion
    }
}
