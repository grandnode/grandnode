using Grand.Core;
using Grand.Domain.Customers;
using Grand.Domain.Orders;
using Grand.Domain.Payments;
using Grand.Domain.Shipping;
using Grand.Framework.Extensions;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Admin.Interfaces;
using Grand.Admin.Models.Customers;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Admin.Services
{
    public class CustomerReportViewModelService : ICustomerReportViewModelService
    {
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerReportService _customerReportService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPriceFormatter _priceFormatter;

        public CustomerReportViewModelService(IWorkContext workContext,
            ICustomerService customerService,
            ILocalizationService localizationService, ICustomerReportService customerReportService,
            IDateTimeHelper dateTimeHelper, IPriceFormatter priceFormatter)
        {
            _workContext = workContext;
            _customerService = customerService;
            _localizationService = localizationService;
            _customerReportService = customerReportService;
            _dateTimeHelper = dateTimeHelper;
            _priceFormatter = priceFormatter;
        }

        public virtual CustomerReportsModel PrepareCustomerReportsModel()
        {
            var model = new CustomerReportsModel {
                //customers by number of orders
                BestCustomersByNumberOfOrders = new BestCustomersReportModel()
            };
            model.BestCustomersByNumberOfOrders.AvailableOrderStatuses = OrderStatus.Pending.ToSelectList(_localizationService, _workContext, false).ToList();
            model.BestCustomersByNumberOfOrders.AvailableOrderStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            model.BestCustomersByNumberOfOrders.AvailablePaymentStatuses = PaymentStatus.Pending.ToSelectList(_localizationService, _workContext, false).ToList();
            model.BestCustomersByNumberOfOrders.AvailablePaymentStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            model.BestCustomersByNumberOfOrders.AvailableShippingStatuses = ShippingStatus.NotYetShipped.ToSelectList(_localizationService, _workContext, false).ToList();
            model.BestCustomersByNumberOfOrders.AvailableShippingStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });

            //customers by order total
            model.BestCustomersByOrderTotal = new BestCustomersReportModel {
                AvailableOrderStatuses = OrderStatus.Pending.ToSelectList(_localizationService, _workContext, false).ToList()
            };
            model.BestCustomersByOrderTotal.AvailableOrderStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            model.BestCustomersByOrderTotal.AvailablePaymentStatuses = PaymentStatus.Pending.ToSelectList(_localizationService, _workContext, false).ToList();
            model.BestCustomersByOrderTotal.AvailablePaymentStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            model.BestCustomersByOrderTotal.AvailableShippingStatuses = ShippingStatus.NotYetShipped.ToSelectList(_localizationService, _workContext, false).ToList();
            model.BestCustomersByOrderTotal.AvailableShippingStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });

            return model;
        }

        public virtual async Task<IList<RegisteredCustomerReportLineModel>> GetReportRegisteredCustomersModel(string storeId)
        {
            var report = new List<RegisteredCustomerReportLineModel>
            {
                new RegisteredCustomerReportLineModel
                {
                    Period = _localizationService.GetResource("Admin.Reports.Customers.RegisteredCustomers.Fields.Period.7days"),
                    Customers = await _customerReportService.GetRegisteredCustomersReport(storeId, 7)
                },

                new RegisteredCustomerReportLineModel
                {
                    Period = _localizationService.GetResource("Admin.Reports.Customers.RegisteredCustomers.Fields.Period.14days"),
                    Customers = await _customerReportService.GetRegisteredCustomersReport(storeId, 14)
                },
                new RegisteredCustomerReportLineModel
                {
                    Period = _localizationService.GetResource("Admin.Reports.Customers.RegisteredCustomers.Fields.Period.month"),
                    Customers = await _customerReportService.GetRegisteredCustomersReport(storeId, 30)
                },
                new RegisteredCustomerReportLineModel
                {
                    Period = _localizationService.GetResource("Admin.Reports.Customers.RegisteredCustomers.Fields.Period.year"),
                    Customers = await _customerReportService.GetRegisteredCustomersReport(storeId, 365)
                }
            };

            return report;
        }

        public virtual async Task<(IEnumerable<BestCustomerReportLineModel> bestCustomerReportLineModels, int totalCount)> PrepareBestCustomerReportLineModel(BestCustomersReportModel model, int orderBy, int pageIndex, int pageSize)
        {
            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            OrderStatus? orderStatus = model.OrderStatusId > 0 ? (OrderStatus?)(model.OrderStatusId) : null;
            PaymentStatus? paymentStatus = model.PaymentStatusId > 0 ? (PaymentStatus?)(model.PaymentStatusId) : null;
            ShippingStatus? shippingStatus = model.ShippingStatusId > 0 ? (ShippingStatus?)(model.ShippingStatusId) : null;

            var items = _customerReportService.GetBestCustomersReport(model.StoreId, startDateValue, endDateValue,
                orderStatus, paymentStatus, shippingStatus, 2, pageIndex - 1, pageSize);

            var report = new List<BestCustomerReportLineModel>();
            foreach (var x in items)
            {
                var m = new BestCustomerReportLineModel {
                    CustomerId = x.CustomerId,
                    OrderTotal = _priceFormatter.FormatPrice(x.OrderTotal, true, false),
                    OrderCount = x.OrderCount,
                };
                var customer = await _customerService.GetCustomerById(x.CustomerId);
                if (customer != null)
                {
                    m.CustomerName = customer.IsRegistered() ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest");
                }
                report.Add(m);
            }
            return (report, items.TotalCount);
        }
    }
}
