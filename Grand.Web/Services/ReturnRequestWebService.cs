using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Tax;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Orders;
using Grand.Services.Seo;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Models.Order;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Services
{
    public partial class ReturnRequestWebService: IReturnRequestWebService
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

        public ReturnRequestWebService(IReturnRequestService returnRequestService,
            IOrderService orderService,
            IProductService productService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter,
            ILocalizationService localizationService,
            IDateTimeHelper dateTimeHelper,
            ICacheManager cacheManager)
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
        }

        public virtual SubmitReturnRequestModel PrepareReturnRequest(SubmitReturnRequestModel model, Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (model == null)
                throw new ArgumentNullException("model");

            model.OrderId = order.Id;
            model.OrderNumber = _orderService.GetOrderById(order.Id).OrderNumber;
            //return reasons
            model.AvailableReturnReasons = _cacheManager.Get(string.Format(ModelCacheEventConsumer.RETURNREQUESTREASONS_MODEL_KEY, _workContext.WorkingLanguage.Id),
                () =>
                {
                    var reasons = new List<SubmitReturnRequestModel.ReturnRequestReasonModel>();
                    foreach (var rrr in _returnRequestService.GetAllReturnRequestReasons())
                        reasons.Add(new SubmitReturnRequestModel.ReturnRequestReasonModel()
                        {
                            Id = rrr.Id,
                            Name = rrr.GetLocalized(x => x.Name)
                        });
                    return reasons;
                });

            //return actions
            model.AvailableReturnActions = _cacheManager.Get(string.Format(ModelCacheEventConsumer.RETURNREQUESTACTIONS_MODEL_KEY, _workContext.WorkingLanguage.Id),
                () =>
                {
                    var actions = new List<SubmitReturnRequestModel.ReturnRequestActionModel>();
                    foreach (var rra in _returnRequestService.GetAllReturnRequestActions())
                        actions.Add(new SubmitReturnRequestModel.ReturnRequestActionModel()
                        {
                            Id = rra.Id,
                            Name = rra.GetLocalized(x => x.Name)
                        });
                    return actions;
                });

            var shipments = Grand.Core.Infrastructure.EngineContext.Current.Resolve<Grand.Services.Shipping.IShipmentService>().GetShipmentsByOrder(order.Id);

            //products
            var orderItems = _orderService.GetAllOrderItems(order.Id, null, null, null, null, null, null);
            foreach (var orderItem in orderItems)
            {
                var qtyDelivery = shipments.Where(x => x.DeliveryDateUtc.HasValue).SelectMany(x => x.ShipmentItems).Where(x => x.OrderItemId == orderItem.Id).Sum(x => x.Quantity);
                var qtyReturn = _returnRequestService.SearchReturnRequests(customerId: order.CustomerId, orderItemId: orderItem.Id).Sum(x => x.Quantity);

                var product = _productService.GetProductByIdIncludeArch(orderItem.ProductId);
                if (!product.NotReturnable)
                {
                    var orderItemModel = new SubmitReturnRequestModel.OrderItemModel
                    {
                        Id = orderItem.Id,
                        ProductId = orderItem.ProductId,
                        ProductName = product.GetLocalized(x => x.Name),
                        ProductSeName = product.GetSeName(),
                        AttributeInfo = orderItem.AttributeDescription,
                        Quantity = qtyDelivery - qtyReturn,
                    };
                    model.Items.Add(orderItemModel);
                    //unit price
                    if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                    {
                        //including tax
                        var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
                        orderItemModel.UnitPrice = _priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true);
                    }
                    else
                    {
                        //excluding tax
                        var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
                        orderItemModel.UnitPrice = _priceFormatter.FormatPrice(unitPriceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false);
                    }
                }
            }

            return model;
        }
        public virtual CustomerReturnRequestsModel PrepareCustomerReturnRequests()
        {
            var model = new CustomerReturnRequestsModel();

            var returnRequests = _returnRequestService.SearchReturnRequests(_storeContext.CurrentStore.Id,
                _workContext.CurrentCustomer.Id);
            foreach (var returnRequest in returnRequests)
            {
                var order = _orderService.GetOrderById(returnRequest.OrderId);
                var orderItem = order.OrderItems.Where(x => x.Id == returnRequest.OrderItemId).FirstOrDefault();
                if (orderItem != null)
                {
                    var product = _productService.GetProductByIdIncludeArch(orderItem.ProductId);

                    var itemModel = new CustomerReturnRequestsModel.ReturnRequestModel
                    {
                        Id = returnRequest.Id,
                        ReturnNumber = returnRequest.ReturnNumber,
                        ReturnRequestStatus = returnRequest.ReturnRequestStatus.GetLocalizedEnum(_localizationService, _workContext),
                        ProductId = product.Id,
                        ProductName = product.GetLocalized(x => x.Name),
                        ProductSeName = product.GetSeName(),
                        Quantity = returnRequest.Quantity,
                        ReturnAction = returnRequest.RequestedAction,
                        ReturnReason = returnRequest.ReasonForReturn,
                        Comments = returnRequest.CustomerComments,
                        CreatedOn = _dateTimeHelper.ConvertToUserTime(returnRequest.CreatedOnUtc, DateTimeKind.Utc),
                    };
                    model.Items.Add(itemModel);
                }
            }
            return model;
        }
    }
}