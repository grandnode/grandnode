﻿using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Orders;
using Grand.Web.Features.Models.Orders;
using Grand.Web.Models.Orders;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Orders
{
    public class GetCustomerOrderListHandler : IRequestHandler<GetCustomerOrderList, CustomerOrderListModel>
    {
        private readonly IOrderService _orderService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderProcessingService _orderProcessingService;

        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;

        public GetCustomerOrderListHandler(IOrderService orderService, IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService, IOrderProcessingService orderProcessingService,
            ICurrencyService currencyService, IPriceFormatter priceFormatter)
        {
            _orderService = orderService;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _orderProcessingService = orderProcessingService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
        }

        public async Task<CustomerOrderListModel> Handle(GetCustomerOrderList request, CancellationToken cancellationToken)
        {
            var model = new CustomerOrderListModel();
            await PrepareOrder(model, request);
            await PrepareRecurringPayments(model, request);
            return model;
        }

        private async Task PrepareOrder(CustomerOrderListModel model, GetCustomerOrderList request)
        {
            var orders = await _orderService.SearchOrders(storeId: request.Store.Id,
                customerId: request.Customer.Id);

            foreach (var order in orders)
            {
                var orderModel = new CustomerOrderListModel.OrderDetailsModel {
                    Id = order.Id,
                    OrderNumber = order.OrderNumber,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc),
                    OrderStatusEnum = order.OrderStatus,
                    OrderStatus = order.OrderStatus.GetLocalizedEnum(_localizationService, request.Language.Id),
                    PaymentStatus = order.PaymentStatus.GetLocalizedEnum(_localizationService, request.Language.Id),
                    ShippingStatus = order.ShippingStatus.GetLocalizedEnum(_localizationService, request.Language.Id),
                    IsReturnRequestAllowed = await _orderProcessingService.IsReturnRequestAllowed(order)
                };
                var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
                orderModel.OrderTotal = await _priceFormatter.FormatPrice(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, request.Language);

                model.Orders.Add(orderModel);
            }
        }
        private async Task PrepareRecurringPayments(CustomerOrderListModel model, GetCustomerOrderList request)
        {
            var recurringPayments = await _orderService.SearchRecurringPayments(request.Store.Id,
                request.Customer.Id);
            foreach (var recurringPayment in recurringPayments)
            {
                var recurringPaymentModel = new CustomerOrderListModel.RecurringOrderModel {
                    Id = recurringPayment.Id,
                    StartDate = _dateTimeHelper.ConvertToUserTime(recurringPayment.StartDateUtc, DateTimeKind.Utc).ToString(),
                    CycleInfo = string.Format("{0} {1}", recurringPayment.CycleLength, recurringPayment.CyclePeriod.GetLocalizedEnum(_localizationService, request.Language.Id)),
                    NextPayment = recurringPayment.NextPaymentDate.HasValue ? _dateTimeHelper.ConvertToUserTime(recurringPayment.NextPaymentDate.Value, DateTimeKind.Utc).ToString() : "",
                    TotalCycles = recurringPayment.TotalCycles,
                    CyclesRemaining = recurringPayment.CyclesRemaining,
                    InitialOrderId = recurringPayment.InitialOrder.Id,
                    CanCancel = await _orderProcessingService.CanCancelRecurringPayment(request.Customer, recurringPayment),
                };

                model.RecurringOrders.Add(recurringPaymentModel);
            }
        }

    }
}
