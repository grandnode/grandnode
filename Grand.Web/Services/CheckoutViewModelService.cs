using Grand.Core;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Payments;
using Grand.Core.Domain.Shipping;
using Grand.Core.Plugins;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Directory;
using Grand.Services.Discounts;
using Grand.Services.Localization;
using Grand.Services.Orders;
using Grand.Services.Payments;
using Grand.Services.Shipping;
using Grand.Services.Stores;
using Grand.Services.Tax;
using Grand.Web.Interfaces;
using Grand.Web.Models.Checkout;
using Grand.Web.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Services
{
    public partial class CheckoutViewModelService : ICheckoutViewModelService
    {
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IWorkContext _workContext;
        private readonly ICountryService _countryService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IAddressViewModelService _addressViewModelService;
        private readonly ITaxService _taxService;
        private readonly IShippingService _shippingService;
        private readonly ILocalizationService _localizationService;
        private readonly ICurrencyService _currencyService;
        private readonly IStoreContext _storeContext;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IPaymentService _paymentService;
        private readonly IRewardPointsService _rewardPointsService;
        private readonly IWebHelper _webHelper;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;

        private readonly ShippingSettings _shippingSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly OrderSettings _orderSettings;

        public CheckoutViewModelService(
            IOrderTotalCalculationService orderTotalCalculationService,
            IWorkContext workContext,
            ICountryService countryService,
            IStoreMappingService storeMappingService,
            IAddressViewModelService addressViewModelService,
            ITaxService taxService,
            IShippingService shippingService,
            ILocalizationService localizationService,
            ICurrencyService currencyService,
            IStoreContext storeContext,
            IPriceFormatter priceFormatter,
            IGenericAttributeService genericAttributeService,
            IPaymentService paymentService,
            IRewardPointsService rewardPointsService,
            IWebHelper webHelper,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            ShippingSettings shippingSettings,
            RewardPointsSettings rewardPointsSettings,
            PaymentSettings paymentSettings,
            OrderSettings orderSettings)
        {
            _orderTotalCalculationService = orderTotalCalculationService;
            _workContext = workContext;
            _countryService = countryService;
            _storeMappingService = storeMappingService;
            _addressViewModelService = addressViewModelService;
            _taxService = taxService;
            _shippingService = shippingService;
            _localizationService = localizationService;
            _currencyService = currencyService;
            _storeContext = storeContext;
            _priceFormatter = priceFormatter;
            _genericAttributeService = genericAttributeService;
            _paymentService = paymentService;
            _rewardPointsService = rewardPointsService;
            _webHelper = webHelper;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _shippingSettings = shippingSettings;
            _rewardPointsSettings = rewardPointsSettings;
            _paymentSettings = paymentSettings;
            _orderSettings = orderSettings;
        }
        public virtual async Task<bool> IsPaymentWorkflowRequired(IList<ShoppingCartItem> cart, bool? useRewardPoints = null)
        {
            bool result = true;

            //check whether order total equals zero
            decimal? shoppingCartTotalBase = (await _orderTotalCalculationService.GetShoppingCartTotal(cart, useRewardPoints: useRewardPoints)).shoppingCartTotal;
            if (shoppingCartTotalBase.HasValue && shoppingCartTotalBase.Value == decimal.Zero)
                result = false;
            return result;
        }

        public virtual async Task<CheckoutBillingAddressModel> PrepareBillingAddress(
            IList<ShoppingCartItem> cart, string selectedCountryId = null,
            bool prePopulateNewAddressWithCustomerFields = false, string overrideAttributesXml = "")
        {
            var model = new CheckoutBillingAddressModel();
            model.ShipToSameAddressAllowed = _shippingSettings.ShipToSameAddress && cart.RequiresShipping();
            model.ShipToSameAddress = true;

            //existing addresses
            var addresses = new List<Address>();
            foreach (var item in _workContext.CurrentCustomer.Addresses)
            {
                if (string.IsNullOrEmpty(item.CountryId))
                {
                    addresses.Add(item);
                    continue;
                }
                var country = await _countryService.GetCountryById(item.CountryId);
                if (country == null || (country.AllowsBilling && _storeMappingService.Authorize(country)))
                {
                    addresses.Add(item);
                    continue;
                }
            }

            foreach (var address in addresses)
            {
                var addressModel = new AddressModel();
                await _addressViewModelService.PrepareModel(model: addressModel, address: address, excludeProperties: false);
                model.ExistingAddresses.Add(addressModel);
            }

            //new address
            model.NewAddress.CountryId = selectedCountryId;
            var countries = await _countryService.GetAllCountriesForBilling(_workContext.WorkingLanguage.Id);
            await _addressViewModelService.PrepareModel(model: model.NewAddress, address: null, excludeProperties: false,
                loadCountries: () => countries,
                prePopulateWithCustomerFields: prePopulateNewAddressWithCustomerFields,
                customer: _workContext.CurrentCustomer,
                overrideAttributesXml: overrideAttributesXml
                );
            return model;
        }
        public virtual async Task<CheckoutShippingAddressModel> PrepareShippingAddress(string selectedCountryId = null,
            bool prePopulateNewAddressWithCustomerFields = false, string overrideAttributesXml = "")
        {
            var model = new CheckoutShippingAddressModel();
            //allow pickup in store?
            model.AllowPickUpInStore = _shippingSettings.AllowPickUpInStore;
            if (model.AllowPickUpInStore)
            {

                var pickupPoints = await _shippingService.LoadActivePickupPoints(_storeContext.CurrentStore.Id);

                if (pickupPoints.Any())
                {
                    foreach (var pickupPoint in pickupPoints)
                    {
                        var pickupPointModel = new CheckoutPickupPointModel()
                        {
                            Id = pickupPoint.Id,
                            Name = pickupPoint.Name,
                            Description = pickupPoint.Description,
                            Address = pickupPoint.Address,
                        };
                        if (pickupPoint.PickupFee > 0)
                        {
                            var amount = (await _taxService.GetShippingPrice(pickupPoint.PickupFee, _workContext.CurrentCustomer)).shippingPrice;
                            amount = await _currencyService.ConvertFromPrimaryStoreCurrency(amount, _workContext.WorkingCurrency);
                            pickupPointModel.PickupFee = _priceFormatter.FormatShippingPrice(amount, true);
                        }
                        model.PickupPoints.Add(pickupPointModel);
                    }
                }

                if (!(await _shippingService.LoadActiveShippingRateComputationMethods(_storeContext.CurrentStore.Id)).Any())
                {
                    if (!pickupPoints.Any())
                    {
                        model.Warnings.Add(_localizationService.GetResource("Checkout.ShippingIsNotAllowed"));
                        model.Warnings.Add(_localizationService.GetResource("Checkout.PickupPoints.NotAvailable"));
                    }
                    model.PickUpInStoreOnly = true;
                    model.PickUpInStore = true;
                    return model;
                }

            }
            //existing addresses
            var addresses = new List<Address>();
            foreach (var item in _workContext.CurrentCustomer.Addresses)
            {
                if (string.IsNullOrEmpty(item.CountryId))
                {
                    addresses.Add(item);
                    continue;
                }
                var country = await _countryService.GetCountryById(item.CountryId);
                if (country == null || (country.AllowsShipping && _storeMappingService.Authorize(country)))
                {
                    addresses.Add(item);
                    continue;
                }
            }
            foreach (var address in addresses)
            {
                var addressModel = new AddressModel();
                await _addressViewModelService.PrepareModel(model: addressModel,
                    address: address,
                    excludeProperties: false);

                model.ExistingAddresses.Add(addressModel);
            }

            //new address
            model.NewAddress.CountryId = selectedCountryId;
            var countries = await _countryService.GetAllCountriesForShipping();
            await _addressViewModelService.PrepareModel(model: model.NewAddress,
                address: null,
                excludeProperties: false,
                loadCountries: () => countries,
                prePopulateWithCustomerFields: prePopulateNewAddressWithCustomerFields,
                customer: _workContext.CurrentCustomer,
                overrideAttributesXml: overrideAttributesXml);
            return model;
        }
        public virtual async Task<CheckoutShippingMethodModel> PrepareShippingMethod(IList<ShoppingCartItem> cart, Address shippingAddress)
        {
            var model = new CheckoutShippingMethodModel();

            var getShippingOptionResponse = await _shippingService
                .GetShippingOptions(_workContext.CurrentCustomer, cart, shippingAddress,
                "", _storeContext.CurrentStore);
            if (getShippingOptionResponse.Success)
            {
                //performance optimization. cache returned shipping options.
                //we'll use them later (after a customer has selected an option).
                await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                                                       SystemCustomerAttributeNames.OfferedShippingOptions,
                                                       getShippingOptionResponse.ShippingOptions,
                                                       _storeContext.CurrentStore.Id);

                foreach (var shippingOption in getShippingOptionResponse.ShippingOptions)
                {
                    var soModel = new CheckoutShippingMethodModel.ShippingMethodModel
                    {
                        Name = shippingOption.Name,
                        Description = shippingOption.Description,
                        ShippingRateComputationMethodSystemName = shippingOption.ShippingRateComputationMethodSystemName,
                        ShippingOption = shippingOption,
                    };

                    //adjust rate
                    var shippingTotal = (await _orderTotalCalculationService.AdjustShippingRate(
                        shippingOption.Rate, cart)).shippingRate;

                    decimal rateBase = (await _taxService.GetShippingPrice(shippingTotal, _workContext.CurrentCustomer)).shippingPrice;
                    decimal rate = await _currencyService.ConvertFromPrimaryStoreCurrency(rateBase, _workContext.WorkingCurrency);
                    soModel.Fee = _priceFormatter.FormatShippingPrice(rate, true);

                    model.ShippingMethods.Add(soModel);
                }

                //find a selected (previously) shipping method
                var selectedShippingOption = await _workContext.CurrentCustomer.GetAttribute<ShippingOption>(
                        _genericAttributeService, SystemCustomerAttributeNames.SelectedShippingOption, _storeContext.CurrentStore.Id);
                if (selectedShippingOption != null)
                {
                    var shippingOptionToSelect = model.ShippingMethods.ToList()
                        .Find(so =>
                           !String.IsNullOrEmpty(so.Name) &&
                           so.Name.Equals(selectedShippingOption.Name, StringComparison.OrdinalIgnoreCase) &&
                           !String.IsNullOrEmpty(so.ShippingRateComputationMethodSystemName) &&
                           so.ShippingRateComputationMethodSystemName.Equals(selectedShippingOption.ShippingRateComputationMethodSystemName, StringComparison.OrdinalIgnoreCase));
                    if (shippingOptionToSelect != null)
                    {
                        shippingOptionToSelect.Selected = true;
                    }
                }
                //if no option has been selected, let's do it for the first one
                if (model.ShippingMethods.FirstOrDefault(so => so.Selected) == null)
                {
                    var shippingOptionToSelect = model.ShippingMethods.FirstOrDefault();
                    if (shippingOptionToSelect != null)
                    {
                        shippingOptionToSelect.Selected = true;
                    }
                }

                //notify about shipping from multiple locations
                if (_shippingSettings.NotifyCustomerAboutShippingFromMultipleLocations)
                {
                    model.NotifyCustomerAboutShippingFromMultipleLocations = getShippingOptionResponse.ShippingFromMultipleLocations;
                }
            }
            else
            {
                foreach (var error in getShippingOptionResponse.Errors)
                    model.Warnings.Add(error);
            }

            return model;
        }
        public virtual async Task<CheckoutPaymentMethodModel> PreparePaymentMethod(IList<ShoppingCartItem> cart, string filterByCountryId)
        {
            var model = new CheckoutPaymentMethodModel();

            //reward points
            if (_rewardPointsSettings.Enabled && !cart.IsRecurring())
            {
                int rewardPointsBalance = await _rewardPointsService.GetRewardPointsBalance(_workContext.CurrentCustomer.Id, _storeContext.CurrentStore.Id);
                decimal rewardPointsAmountBase = await _orderTotalCalculationService.ConvertRewardPointsToAmount(rewardPointsBalance);
                decimal rewardPointsAmount = await _currencyService.ConvertFromPrimaryStoreCurrency(rewardPointsAmountBase, _workContext.WorkingCurrency);
                if (rewardPointsAmount > decimal.Zero &&
                    _orderTotalCalculationService.CheckMinimumRewardPointsToUseRequirement(rewardPointsBalance))
                {
                    model.DisplayRewardPoints = true;
                    model.RewardPointsAmount = _priceFormatter.FormatPrice(rewardPointsAmount, true, false);
                    model.RewardPointsBalance = rewardPointsBalance;
                    model.RewardPointsEnoughToPayForOrder = !(await  IsPaymentWorkflowRequired(cart, true));
                }
            }

            //filter by country
            var paymentMethods = (await _paymentService
                .LoadActivePaymentMethods(_workContext.CurrentCustomer, _storeContext.CurrentStore.Id, filterByCountryId))
                .Where(pm => pm.PaymentMethodType == PaymentMethodType.Standard || pm.PaymentMethodType == PaymentMethodType.Redirection).ToList();
            var availablepaymentMethods = new List<IPaymentMethod>();
            foreach (var pm in paymentMethods)
            {
                if (!await pm.HidePaymentMethod(cart))
                    availablepaymentMethods.Add(pm);
            }

            foreach (var pm in availablepaymentMethods)
            {
                if (cart.IsRecurring() && pm.RecurringPaymentType == RecurringPaymentType.NotSupported)
                    continue;

                var pmModel = new CheckoutPaymentMethodModel.PaymentMethodModel
                {
                    Name = pm.GetLocalizedFriendlyName(_localizationService, _workContext.WorkingLanguage.Id),
                    Description = _paymentSettings.ShowPaymentMethodDescriptions ? await pm.PaymentMethodDescription() : string.Empty,
                    PaymentMethodSystemName = pm.PluginDescriptor.SystemName,
                    LogoUrl = pm.PluginDescriptor.GetLogoUrl(_webHelper)
                };
                //payment method additional fee
                decimal paymentMethodAdditionalFee = await _paymentService.GetAdditionalHandlingFee(cart, pm.PluginDescriptor.SystemName);
                decimal rateBase = (await _taxService.GetPaymentMethodAdditionalFee(paymentMethodAdditionalFee, _workContext.CurrentCustomer)).paymentPrice;
                decimal rate = await _currencyService.ConvertFromPrimaryStoreCurrency(rateBase, _workContext.WorkingCurrency);
                if (rate > decimal.Zero)
                    pmModel.Fee = _priceFormatter.FormatPaymentMethodAdditionalFee(rate, true);

                model.PaymentMethods.Add(pmModel);
            }

            //find a selected (previously) payment method
            var selectedPaymentMethodSystemName = await _workContext.CurrentCustomer.GetAttribute<string>(
                _genericAttributeService, SystemCustomerAttributeNames.SelectedPaymentMethod, _storeContext.CurrentStore.Id);
            if (!String.IsNullOrEmpty(selectedPaymentMethodSystemName))
            {
                var paymentMethodToSelect = model.PaymentMethods.ToList()
                    .Find(pm => pm.PaymentMethodSystemName.Equals(selectedPaymentMethodSystemName, StringComparison.OrdinalIgnoreCase));
                if (paymentMethodToSelect != null)
                    paymentMethodToSelect.Selected = true;
            }
            //if no option has been selected, let's do it for the first one
            if (model.PaymentMethods.FirstOrDefault(so => so.Selected) == null)
            {
                var paymentMethodToSelect = model.PaymentMethods.FirstOrDefault();
                if (paymentMethodToSelect != null)
                    paymentMethodToSelect.Selected = true;
            }

            return model;
        }
        public virtual CheckoutPaymentInfoModel PreparePaymentInfo(IPaymentMethod paymentMethod)
        {
            paymentMethod.GetPublicViewComponent(out string viewComponentName);

            var model = new CheckoutPaymentInfoModel
            {
                PaymentViewComponentName = viewComponentName,
                DisplayOrderTotals = _orderSettings.OnePageCheckoutDisplayOrderTotalsOnPaymentInfoTab
            };

            return model;

        }
        public virtual async Task<CheckoutConfirmModel> PrepareConfirmOrder(IList<ShoppingCartItem> cart)
        {
            var model = new CheckoutConfirmModel();
            //terms of service
            model.TermsOfServiceOnOrderConfirmPage = _orderSettings.TermsOfServiceOnOrderConfirmPage;
            //min order amount validation
            bool minOrderTotalAmountOk = await _orderProcessingService.ValidateMinOrderTotalAmount(cart);
            if (!minOrderTotalAmountOk)
            {
                decimal minOrderTotalAmount = await _currencyService.ConvertFromPrimaryStoreCurrency(_orderSettings.MinOrderTotalAmount, _workContext.WorkingCurrency);
                model.MinOrderTotalWarning = string.Format(_localizationService.GetResource("Checkout.MinOrderTotalAmount"), _priceFormatter.FormatPrice(minOrderTotalAmount, true, false));
            }
            return model;
        }
        public virtual async Task<bool> IsMinimumOrderPlacementIntervalValid(Customer customer)
        {
            if (_orderSettings.MinimumOrderPlacementInterval == 0)
                return true;

            var lastOrder = (await _orderService.SearchOrders(storeId: _storeContext.CurrentStore.Id,
                customerId: _workContext.CurrentCustomer.Id, pageSize: 1))
                .FirstOrDefault();
            if (lastOrder == null)
                return true;

            var interval = DateTime.UtcNow - lastOrder.CreatedOnUtc;
            return interval.TotalSeconds > _orderSettings.MinimumOrderPlacementInterval;
        }
    }
}