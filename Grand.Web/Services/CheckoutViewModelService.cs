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
using Grand.Web.Models.Checkout;
using Grand.Web.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Services
{
    public partial class CheckoutViewModelService: ICheckoutViewModelService
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
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._workContext = workContext;
            this._countryService = countryService;
            this._storeMappingService = storeMappingService;
            this._addressViewModelService = addressViewModelService;
            this._taxService = taxService;
            this._shippingService = shippingService;
            this._localizationService = localizationService;
            this._currencyService = currencyService;
            this._storeContext = storeContext;
            this._priceFormatter = priceFormatter;
            this._genericAttributeService = genericAttributeService;
            this._paymentService = paymentService;
            this._rewardPointsService = rewardPointsService;
            this._webHelper = webHelper;
            this._orderProcessingService = orderProcessingService;
            this._orderService = orderService;
            this._shippingSettings = shippingSettings;
            this._rewardPointsSettings = rewardPointsSettings;
            this._paymentSettings = paymentSettings;
            this._orderSettings = orderSettings;
        }
        public virtual bool IsPaymentWorkflowRequired(IList<ShoppingCartItem> cart, bool? useRewardPoints = null)
        {
            bool result = true;

            //check whether order total equals zero
            decimal? shoppingCartTotalBase = _orderTotalCalculationService.GetShoppingCartTotal(cart, useRewardPoints: useRewardPoints);
            if (shoppingCartTotalBase.HasValue && shoppingCartTotalBase.Value == decimal.Zero)
                result = false;
            return result;
        }

        public virtual CheckoutBillingAddressModel PrepareBillingAddress(
            IList<ShoppingCartItem> cart, string selectedCountryId = null,
            bool prePopulateNewAddressWithCustomerFields = false, string overrideAttributesXml = "")
        {
            var model = new CheckoutBillingAddressModel();
            model.ShipToSameAddressAllowed = _shippingSettings.ShipToSameAddress && cart.RequiresShipping();
            model.ShipToSameAddress = true;

            //existing addresses

            var addresses = _workContext.CurrentCustomer.Addresses
                .Where(a => a.CountryId == "" ||
                (_countryService.GetCountryById(a.CountryId) != null ? _countryService.GetCountryById(a.CountryId).AllowsBilling : false)
                )
                .Where(a => a.CountryId == "" ||
                _storeMappingService.Authorize((_countryService.GetCountryById(a.CountryId))
                ))
                .ToList();
            foreach (var address in addresses)
            {
                var addressModel = new AddressModel();
                _addressViewModelService.PrepareModel(model: addressModel, address: address, excludeProperties: false);
                model.ExistingAddresses.Add(addressModel);
            }

            //new address
            model.NewAddress.CountryId = selectedCountryId;
            _addressViewModelService.PrepareModel(model: model.NewAddress, address: null, excludeProperties: false,
                loadCountries: () => _countryService.GetAllCountriesForBilling(_workContext.WorkingLanguage.Id),
                prePopulateWithCustomerFields: prePopulateNewAddressWithCustomerFields,
                customer: _workContext.CurrentCustomer,
                overrideAttributesXml: overrideAttributesXml
                );
            return model;
        }
        public virtual CheckoutShippingAddressModel PrepareShippingAddress(string selectedCountryId = null,
            bool prePopulateNewAddressWithCustomerFields = false, string overrideAttributesXml = "")
        {
            var model = new CheckoutShippingAddressModel();
            //allow pickup in store?
            model.AllowPickUpInStore = _shippingSettings.AllowPickUpInStore;
            if (model.AllowPickUpInStore)
            {

                var pickupPoints = _shippingService.LoadActivePickupPoints(_storeContext.CurrentStore.Id);

                if (pickupPoints.Any())
                {
                    model.PickupPoints = pickupPoints.Select(x =>
                    {
                        var pickupPointModel = new CheckoutPickupPointModel()
                        {
                            Id = x.Id,
                            Name = x.Name,
                            Description = x.Description,
                            Address = x.Address,
                        };
                        if (x.PickupFee > 0)
                        {
                            var amount = _taxService.GetShippingPrice(x.PickupFee, _workContext.CurrentCustomer);
                            amount = _currencyService.ConvertFromPrimaryStoreCurrency(amount, _workContext.WorkingCurrency);
                            pickupPointModel.PickupFee = _priceFormatter.FormatShippingPrice(amount, true);
                        }
                        return pickupPointModel;
                    }).ToList();
                }

                if (!_shippingService.LoadActiveShippingRateComputationMethods(_storeContext.CurrentStore.Id).Any())
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
            var addresses = _workContext.CurrentCustomer.Addresses
                //allow shipping
                .Where(a => a.CountryId == "" ||
                (_countryService.GetCountryById(a.CountryId) != null ? _countryService.GetCountryById(a.CountryId).AllowsShipping : false)
                //a.Country.AllowsShipping
                )
                //enabled for the current store
                .Where(a => a.CountryId == "" ||
                _storeMappingService.Authorize(_countryService.GetCountryById(a.CountryId)))
                .ToList();
            foreach (var address in addresses)
            {
                var addressModel = new AddressModel();
                _addressViewModelService.PrepareModel(model: addressModel,
                    address: address,
                    excludeProperties: false);

                model.ExistingAddresses.Add(addressModel);
            }

            //new address
            model.NewAddress.CountryId = selectedCountryId;
            _addressViewModelService.PrepareModel(model: model.NewAddress,
                address: null,
                excludeProperties: false,
                loadCountries: () => _countryService.GetAllCountriesForShipping(),
                prePopulateWithCustomerFields: prePopulateNewAddressWithCustomerFields,
                customer: _workContext.CurrentCustomer,
                overrideAttributesXml: overrideAttributesXml);
            return model;
        }
        public virtual CheckoutShippingMethodModel PrepareShippingMethod(IList<ShoppingCartItem> cart, Address shippingAddress)
        {
            var model = new CheckoutShippingMethodModel();

            var getShippingOptionResponse = _shippingService
                .GetShippingOptions(_workContext.CurrentCustomer, cart, shippingAddress,
                "", _storeContext.CurrentStore.Id);
            if (getShippingOptionResponse.Success)
            {
                //performance optimization. cache returned shipping options.
                //we'll use them later (after a customer has selected an option).
                _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
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
                    var shippingTotal = _orderTotalCalculationService.AdjustShippingRate(
                        shippingOption.Rate, cart, out List<AppliedDiscount> appliedDiscounts);

                    decimal rateBase = _taxService.GetShippingPrice(shippingTotal, _workContext.CurrentCustomer);
                    decimal rate = _currencyService.ConvertFromPrimaryStoreCurrency(rateBase, _workContext.WorkingCurrency);
                    soModel.Fee = _priceFormatter.FormatShippingPrice(rate, true);

                    model.ShippingMethods.Add(soModel);
                }

                //find a selected (previously) shipping method
                var selectedShippingOption = _workContext.CurrentCustomer.GetAttribute<ShippingOption>(
                        SystemCustomerAttributeNames.SelectedShippingOption, _storeContext.CurrentStore.Id);
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
        public virtual CheckoutPaymentMethodModel PreparePaymentMethod(IList<ShoppingCartItem> cart, string filterByCountryId)
        {
            var model = new CheckoutPaymentMethodModel();

            //reward points
            if (_rewardPointsSettings.Enabled && !cart.IsRecurring())
            {
                int rewardPointsBalance = _rewardPointsService.GetRewardPointsBalance(_workContext.CurrentCustomer.Id, _storeContext.CurrentStore.Id);
                decimal rewardPointsAmountBase = _orderTotalCalculationService.ConvertRewardPointsToAmount(rewardPointsBalance);
                decimal rewardPointsAmount = _currencyService.ConvertFromPrimaryStoreCurrency(rewardPointsAmountBase, _workContext.WorkingCurrency);
                if (rewardPointsAmount > decimal.Zero &&
                    _orderTotalCalculationService.CheckMinimumRewardPointsToUseRequirement(rewardPointsBalance))
                {
                    model.DisplayRewardPoints = true;
                    model.RewardPointsAmount = _priceFormatter.FormatPrice(rewardPointsAmount, true, false);
                    model.RewardPointsBalance = rewardPointsBalance;
                    model.RewardPointsEnoughToPayForOrder = !IsPaymentWorkflowRequired(cart, true);
                }
            }

            //filter by country
            var paymentMethods = _paymentService
                .LoadActivePaymentMethods(_workContext.CurrentCustomer, _storeContext.CurrentStore.Id, filterByCountryId)
                .Where(pm => pm.PaymentMethodType == PaymentMethodType.Standard || pm.PaymentMethodType == PaymentMethodType.Redirection)
                .Where(pm => !pm.HidePaymentMethod(cart))
                .ToList();
            foreach (var pm in paymentMethods)
            {
                if (cart.IsRecurring() && pm.RecurringPaymentType == RecurringPaymentType.NotSupported)
                    continue;

                var pmModel = new CheckoutPaymentMethodModel.PaymentMethodModel
                {
                    Name = pm.GetLocalizedFriendlyName(_localizationService, _workContext.WorkingLanguage.Id),
                    Description = _paymentSettings.ShowPaymentMethodDescriptions ? pm.PaymentMethodDescription : string.Empty,
                    PaymentMethodSystemName = pm.PluginDescriptor.SystemName,
                    LogoUrl = pm.PluginDescriptor.GetLogoUrl(_webHelper)
                };
                //payment method additional fee
                decimal paymentMethodAdditionalFee = _paymentService.GetAdditionalHandlingFee(cart, pm.PluginDescriptor.SystemName);
                decimal rateBase = _taxService.GetPaymentMethodAdditionalFee(paymentMethodAdditionalFee, _workContext.CurrentCustomer);
                decimal rate = _currencyService.ConvertFromPrimaryStoreCurrency(rateBase, _workContext.WorkingCurrency);
                if (rate > decimal.Zero)
                    pmModel.Fee = _priceFormatter.FormatPaymentMethodAdditionalFee(rate, true);

                model.PaymentMethods.Add(pmModel);
            }

            //find a selected (previously) payment method
            var selectedPaymentMethodSystemName = _workContext.CurrentCustomer.GetAttribute<string>(
                SystemCustomerAttributeNames.SelectedPaymentMethod, _storeContext.CurrentStore.Id);
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
        public virtual CheckoutConfirmModel PrepareConfirmOrder(IList<ShoppingCartItem> cart)
        {
            var model = new CheckoutConfirmModel();
            //terms of service
            model.TermsOfServiceOnOrderConfirmPage = _orderSettings.TermsOfServiceOnOrderConfirmPage;
            //min order amount validation
            bool minOrderTotalAmountOk = _orderProcessingService.ValidateMinOrderTotalAmount(cart);
            if (!minOrderTotalAmountOk)
            {
                decimal minOrderTotalAmount = _currencyService.ConvertFromPrimaryStoreCurrency(_orderSettings.MinOrderTotalAmount, _workContext.WorkingCurrency);
                model.MinOrderTotalWarning = string.Format(_localizationService.GetResource("Checkout.MinOrderTotalAmount"), _priceFormatter.FormatPrice(minOrderTotalAmount, true, false));
            }
            return model;
        }
        public virtual bool IsMinimumOrderPlacementIntervalValid(Customer customer)
        {
            if (_orderSettings.MinimumOrderPlacementInterval == 0)
                return true;

            var lastOrder = _orderService.SearchOrders(storeId: _storeContext.CurrentStore.Id,
                customerId: _workContext.CurrentCustomer.Id, pageSize: 1)
                .FirstOrDefault();
            if (lastOrder == null)
                return true;

            var interval = DateTime.UtcNow - lastOrder.CreatedOnUtc;
            return interval.TotalSeconds > _orderSettings.MinimumOrderPlacementInterval;
        }
    }
}