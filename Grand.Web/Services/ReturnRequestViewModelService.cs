using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Tax;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Orders;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Interfaces;
using Grand.Web.Models.Common;
using Grand.Web.Models.Order;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Services
{
    public partial class ReturnRequestViewModelService : IReturnRequestViewModelService
    {

        private readonly IReturnRequestService _returnRequestService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ICacheManager _cacheManager;
        private readonly ICountryService _countryService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IAddressViewModelService _addressViewModelService;
        private readonly IServiceProvider _serviceProvider;
        private readonly OrderSettings _orderSettings;

        public ReturnRequestViewModelService(IReturnRequestService returnRequestService,
            IOrderService orderService,
            IProductService productService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter,
            ILocalizationService localizationService,
            IDateTimeHelper dateTimeHelper,
            ICacheManager cacheManager,
            ICountryService countryService,
            IStoreMappingService storeMappingService,
            IAddressViewModelService addressViewModelService,
            IServiceProvider serviceProvider,
            OrderSettings orderSettings)
        {
            this._returnRequestService = returnRequestService;
            this._orderService = orderService;
            this._productService = productService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._currencyService = currencyService;
            this._priceFormatter = priceFormatter;
            this._localizationService = localizationService;
            this._dateTimeHelper = dateTimeHelper;
            this._cacheManager = cacheManager;
            this._countryService = countryService;
            this._storeMappingService = storeMappingService;
            this._addressViewModelService = addressViewModelService;
            this._serviceProvider = serviceProvider;
            this._orderSettings = orderSettings;
        }

        public virtual async Task<SubmitReturnRequestModel> PrepareReturnRequest(SubmitReturnRequestModel model, Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (model == null)
                throw new ArgumentNullException("model");

            model.OrderId = order.Id;
            model.OrderNumber = (await _orderService.GetOrderById(order.Id)).OrderNumber;
            //return reasons
            model.AvailableReturnReasons = await _cacheManager.GetAsync(string.Format(ModelCacheEventConsumer.RETURNREQUESTREASONS_MODEL_KEY, _workContext.WorkingLanguage.Id),
                async () =>
                {
                    var reasons = new List<SubmitReturnRequestModel.ReturnRequestReasonModel>();
                    foreach (var rrr in await _returnRequestService.GetAllReturnRequestReasons())
                        reasons.Add(new SubmitReturnRequestModel.ReturnRequestReasonModel()
                        {
                            Id = rrr.Id,
                            Name = rrr.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id)
                        });
                    return reasons;
                });

            //return actions
            model.AvailableReturnActions = await _cacheManager.GetAsync(string.Format(ModelCacheEventConsumer.RETURNREQUESTACTIONS_MODEL_KEY, _workContext.WorkingLanguage.Id),
                async () =>
                {
                    var actions = new List<SubmitReturnRequestModel.ReturnRequestActionModel>();
                    foreach (var rra in await _returnRequestService.GetAllReturnRequestActions())
                        actions.Add(new SubmitReturnRequestModel.ReturnRequestActionModel()
                        {
                            Id = rra.Id,
                            Name = rra.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id)
                        });
                    return actions;
                });

            var shipments = await _serviceProvider.GetRequiredService<Grand.Services.Shipping.IShipmentService>().GetShipmentsByOrder(order.Id);

            //products
            var orderItems = await _orderService.GetAllOrderItems(order.Id, null, null, null, null, null, null);
            foreach (var orderItem in orderItems)
            {
                var qtyDelivery = shipments.Where(x => x.DeliveryDateUtc.HasValue).SelectMany(x => x.ShipmentItems).Where(x => x.OrderItemId == orderItem.Id).Sum(x => x.Quantity);
                var returnRequests = await _returnRequestService.SearchReturnRequests(customerId: order.CustomerId, orderItemId: orderItem.Id);
                int qtyReturn = 0;

                foreach (var rr in returnRequests)
                {
                    foreach (var rrItem in rr.ReturnRequestItems)
                    {
                        qtyReturn += rrItem.Quantity;
                    }
                }

                var product = await _productService.GetProductByIdIncludeArch(orderItem.ProductId);
                if (!product.NotReturnable)
                {
                    var orderItemModel = new SubmitReturnRequestModel.OrderItemModel
                    {
                        Id = orderItem.Id,
                        ProductId = orderItem.ProductId,
                        ProductName = product.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                        ProductSeName = product.GetSeName(_workContext.WorkingLanguage.Id),
                        AttributeInfo = orderItem.AttributeDescription,
                        Quantity = qtyDelivery - qtyReturn,
                    };
                    if (orderItemModel.Quantity > 0)
                        model.Items.Add(orderItemModel);
                    //unit price
                    if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                    {
                        //including tax
                        var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
                        orderItemModel.UnitPrice = await _priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true);
                    }
                    else
                    {
                        //excluding tax
                        var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
                        orderItemModel.UnitPrice = await _priceFormatter.FormatPrice(unitPriceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false);
                    }
                }
            }
            if (_orderSettings.ReturnRequests_AllowToSpecifyPickupAddress)
            {
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
                var countries = await _countryService.GetAllCountriesForShipping();
                await _addressViewModelService.PrepareModel(model: model.NewAddress,
                    address: null,
                    excludeProperties: false,
                    loadCountries: () => countries,
                    prePopulateWithCustomerFields: true,
                    customer: _workContext.CurrentCustomer);
            }
            model.ShowPickupAddress = _orderSettings.ReturnRequests_AllowToSpecifyPickupAddress;
            model.ShowPickupDate = _orderSettings.ReturnRequests_AllowToSpecifyPickupDate;
            model.PickupDateRequired = _orderSettings.ReturnRequests_PickupDateRequired;

            return model;
        }
        public virtual async Task<ReturnRequestDetailsModel> PrepareReturnRequestDetails(ReturnRequest returnRequest, Order order)
        {
            var model = new ReturnRequestDetailsModel();
            model.Comments = returnRequest.CustomerComments;
            model.ReturnNumber = returnRequest.ReturnNumber;
            model.ReturnRequestStatus = returnRequest.ReturnRequestStatus;
            model.CreatedOnUtc = returnRequest.CreatedOnUtc;
            model.ShowPickupAddress = _orderSettings.ReturnRequests_AllowToSpecifyPickupAddress;
            model.ShowPickupDate = _orderSettings.ReturnRequests_AllowToSpecifyPickupDate;
            model.PickupDate = returnRequest.PickupDate;
            await _addressViewModelService.PrepareModel(model: model.PickupAddress, address: returnRequest.PickupAddress, excludeProperties: false);

            foreach (var item in returnRequest.ReturnRequestItems)
            {
                var orderItem = order.OrderItems.Where(x => x.Id == item.OrderItemId).FirstOrDefault();
                var product = await _productService.GetProductByIdIncludeArch(orderItem.ProductId);

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
                    ProductName = product.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                    ProductSeName = product.GetSeName(_workContext.WorkingLanguage.Id),
                    ProductPrice = unitPrice
                });
            }
            return model;
        }

        public virtual async Task<CustomerReturnRequestsModel> PrepareCustomerReturnRequests()
        {
            var model = new CustomerReturnRequestsModel();

            var returnRequests = await _returnRequestService.SearchReturnRequests(_storeContext.CurrentStore.Id, _workContext.CurrentCustomer.Id);
            foreach (var returnRequest in returnRequests)
            {
                var order = await _orderService.GetOrderById(returnRequest.OrderId);
                decimal total = 0;
                foreach (var rrItem in returnRequest.ReturnRequestItems)
                {
                    var orderItem = order.OrderItems.Where(x => x.Id == rrItem.OrderItemId).First();

                    if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                    {
                        //including tax
                        var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
                        total += unitPriceInclTaxInCustomerCurrency * rrItem.Quantity;
                    }
                    else
                    {
                        //excluding tax
                        var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
                        total += unitPriceExclTaxInCustomerCurrency * rrItem.Quantity;
                    }
                }

                var itemModel = new CustomerReturnRequestsModel.ReturnRequestModel
                {
                    Id = returnRequest.Id,
                    ReturnNumber = returnRequest.ReturnNumber,
                    ReturnRequestStatus = returnRequest.ReturnRequestStatus.GetLocalizedEnum(_localizationService, _workContext),
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(returnRequest.CreatedOnUtc, DateTimeKind.Utc),
                    ProductsCount = returnRequest.ReturnRequestItems.Sum(x => x.Quantity),
                    ReturnTotal = _priceFormatter.FormatPrice(total)
                };

                model.Items.Add(itemModel);
            }

            return model;
        }
    }
}