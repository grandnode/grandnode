using Grand.Core;
using Grand.Core.Caching;
using Grand.Domain.Common;
using Grand.Domain.Orders;
using Grand.Domain.Tax;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Orders;
using Grand.Services.Queries.Models.Orders;
using Grand.Services.Seo;
using Grand.Services.Shipping;
using Grand.Services.Stores;
using Grand.Services.Vendors;
using Grand.Web.Features.Models.Common;
using Grand.Web.Features.Models.Orders;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Models.Orders;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Orders
{
    public class GetReturnRequestHandler : IRequestHandler<GetReturnRequest, ReturnRequestModel>
    {
        private readonly ICacheManager _cacheManager;
        private readonly IWorkContext _workContext;
        private readonly IReturnRequestService _returnRequestService;
        private readonly IShipmentService _shipmentService;
        private readonly IProductService _productService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICountryService _countryService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IVendorService _vendorService;
        private readonly IMediator _mediator;
        private readonly OrderSettings _orderSettings;

        public GetReturnRequestHandler(
            ICacheManager cacheManager,
            IWorkContext workContext,
            IReturnRequestService returnRequestService,
            IShipmentService shipmentService,
            IProductService productService,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter,
            ICountryService countryService,
            IStoreMappingService storeMappingService,
            IVendorService vendorService,
            IMediator mediator,
            OrderSettings orderSettings
            )
        {
            _cacheManager = cacheManager;
            _workContext = workContext;
            _returnRequestService = returnRequestService;
            _shipmentService = shipmentService;
            _productService = productService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _countryService = countryService;
            _storeMappingService = storeMappingService;
            _vendorService = vendorService;
            _mediator = mediator;
            _orderSettings = orderSettings;
        }

        public async Task<ReturnRequestModel> Handle(GetReturnRequest request, CancellationToken cancellationToken)
        {
            var model = new ReturnRequestModel();
            model.OrderId = request.Order.Id;
            model.OrderNumber = request.Order.OrderNumber;
            model.OrderCode = request.Order.Code;
            model.ShowPickupAddress = _orderSettings.ReturnRequests_AllowToSpecifyPickupAddress;
            model.ShowPickupDate = _orderSettings.ReturnRequests_AllowToSpecifyPickupDate;
            model.PickupDateRequired = _orderSettings.ReturnRequests_PickupDateRequired;

            //return reasons
            model.AvailableReturnReasons = await PrepareAvailableReturnReasons();

            //return actions
            model.AvailableReturnActions = await PrepareAvailableReturnActions();

            //products
            await PrepareItems(request, model);

            if (_orderSettings.ReturnRequests_AllowToSpecifyPickupAddress)
            {
                await PreparePickupAddress(request, model);
            }

            return model;
        }

        private async Task<IList<ReturnRequestModel.ReturnRequestReasonModel>> PrepareAvailableReturnReasons()
        {
            return await _cacheManager.GetAsync(string.Format(ModelCacheEventConst.RETURNREQUESTREASONS_MODEL_KEY, _workContext.WorkingLanguage.Id),
                async () =>
                {
                    var reasons = new List<ReturnRequestModel.ReturnRequestReasonModel>();
                    foreach (var rrr in await _returnRequestService.GetAllReturnRequestReasons())
                        reasons.Add(new ReturnRequestModel.ReturnRequestReasonModel() {
                            Id = rrr.Id,
                            Name = rrr.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id)
                        });
                    return reasons;
                });
        }

        private async Task<IList<ReturnRequestModel.ReturnRequestActionModel>> PrepareAvailableReturnActions()
        {
            return await _cacheManager.GetAsync(string.Format(ModelCacheEventConst.RETURNREQUESTACTIONS_MODEL_KEY, _workContext.WorkingLanguage.Id),
                async () =>
                {
                    var actions = new List<ReturnRequestModel.ReturnRequestActionModel>();
                    foreach (var rra in await _returnRequestService.GetAllReturnRequestActions())
                        actions.Add(new ReturnRequestModel.ReturnRequestActionModel() {
                            Id = rra.Id,
                            Name = rra.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id)
                        });
                    return actions;
                });
        }

        private async Task PrepareItems(GetReturnRequest request, ReturnRequestModel model)
        {
            var shipments = await _shipmentService.GetShipmentsByOrder(request.Order.Id);

            foreach (var orderItem in request.Order.OrderItems)
            {
                var qtyDelivery = shipments.Where(x => x.DeliveryDateUtc.HasValue).SelectMany(x => x.ShipmentItems).Where(x => x.OrderItemId == orderItem.Id).Sum(x => x.Quantity);

                var query = new GetReturnRequestQuery() {
                    StoreId = request.Store.Id,
                };

                var returnRequests = await _returnRequestService.SearchReturnRequests(orderItemId: orderItem.Id);
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
                    var orderItemModel = new ReturnRequestModel.OrderItemModel {
                        Id = orderItem.Id,
                        ProductId = orderItem.ProductId,
                        ProductName = product.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                        ProductSeName = product.GetSeName(_workContext.WorkingLanguage.Id),
                        AttributeInfo = orderItem.AttributeDescription,
                        VendorId = orderItem.VendorId,
                        VendorName = string.IsNullOrEmpty(orderItem.VendorId) ? "" : (await _vendorService.GetVendorById(orderItem.VendorId))?.Name,
                        Quantity = qtyDelivery - qtyReturn,
                    };
                    if (orderItemModel.Quantity > 0)
                        model.Items.Add(orderItemModel);
                    //unit price
                    if (request.Order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                    {
                        //including tax
                        var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, request.Order.CurrencyRate);
                        orderItemModel.UnitPrice = await _priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency, true, request.Order.CustomerCurrencyCode, _workContext.WorkingLanguage, true);
                    }
                    else
                    {
                        //excluding tax
                        var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, request.Order.CurrencyRate);
                        orderItemModel.UnitPrice = await _priceFormatter.FormatPrice(unitPriceExclTaxInCustomerCurrency, true, request.Order.CustomerCurrencyCode, _workContext.WorkingLanguage, false);
                    }
                }
            }

        }

        private async Task PreparePickupAddress(GetReturnRequest request, ReturnRequestModel model)
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
                var addressModel = await _mediator.Send(new GetAddressModel() {
                    Language = request.Language,
                    Store = request.Store,
                    Address = address,
                    ExcludeProperties = false,
                });
                model.ExistingAddresses.Add(addressModel);
            }

            //new address
            var countries = await _countryService.GetAllCountriesForShipping();
            model.NewAddress = await _mediator.Send(new GetAddressModel() {
                Language = request.Language,
                Store = request.Store,
                Address = null,
                ExcludeProperties = false,
                LoadCountries = () => countries,
                PrePopulateWithCustomerFields = true,
                Customer = _workContext.CurrentCustomer,
            });
        }
    }
}
