using Grand.Core;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Payments;
using Grand.Core.Domain.Shipping;
using Grand.Framework.Extensions;
using Grand.Framework.Kendoui;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Orders;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Services.Vendors;
using Grand.Web.Areas.Admin.Models.Orders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Areas.Admin.Controllers
{
    public class ReportsController : BaseAdminController
    {

        private readonly IOrderService _orderService;
        private readonly IOrderReportService _orderReportService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductService _productService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreService _storeService;
        private readonly ICountryService _countryService;
        private readonly IVendorService _vendorService;
        private readonly IDateTimeHelper _dateTimeHelper;

        public ReportsController(IOrderService orderService,
        IOrderReportService orderReportService,
        IPermissionService permissionService,
        IWorkContext workContext,
        IPriceFormatter priceFormatter,
        IProductService productService,
        ILocalizationService localizationService,
        IStoreService storeService,
        ICountryService countryService,
        IVendorService vendorService,
        IDateTimeHelper dateTimeHelper)
        {
            _orderService = orderService;
            _orderReportService = orderReportService;
            _permissionService = permissionService;
            _workContext = workContext;
            _priceFormatter = priceFormatter;
            _productService = productService;
            _localizationService = localizationService;
            _storeService = storeService;
            _countryService = countryService;
            _vendorService = vendorService;
            _dateTimeHelper = dateTimeHelper;
        }

        [NonAction]
        protected DataSourceResult GetBestsellersBriefReportModel(int pageIndex,
            int pageSize, int orderBy)
        {
            //a vendor should have access only to his products
            string vendorId = "";
            if (_workContext.CurrentVendor != null)
                vendorId = _workContext.CurrentVendor.Id;

            var items = _orderReportService.BestSellersReport(
                vendorId: vendorId,
                orderBy: orderBy,
                pageIndex: pageIndex,
                pageSize: pageSize,
                showHidden: true);
            var gridModel = new DataSourceResult
            {
                Data = items.Select(x =>
                {
                    var m = new BestsellersReportLineModel
                    {
                        ProductId = x.ProductId,
                        TotalAmount = _priceFormatter.FormatPrice(x.TotalAmount, true, false),
                        TotalQuantity = x.TotalQuantity,
                    };
                    var product = _productService.GetProductById(x.ProductId);
                    if (product != null)
                        m.ProductName = product.Name;
                    return m;
                }),
                Total = items.TotalCount
            };
            return gridModel;
        }

        [NonAction]
        protected virtual IList<OrderPeriodReportLineModel> GetReportOrderPeriodModel()
        {
            var report = new List<OrderPeriodReportLineModel>();
            var reportperiod7days = _orderReportService.GetOrderPeriodReport(7);
            report.Add(new OrderPeriodReportLineModel
            {
                Period = _localizationService.GetResource("Admin.SalesReport.Period.7days"),
                Count = reportperiod7days.Count,
                Amount = reportperiod7days.Amount
            });

            var reportperiod14days = _orderReportService.GetOrderPeriodReport(14);
            report.Add(new OrderPeriodReportLineModel
            {
                Period = _localizationService.GetResource("Admin.SalesReport.Period.14days"),
                Count = reportperiod14days.Count,
                Amount = reportperiod14days.Amount
            });

            var reportperiodmonth = _orderReportService.GetOrderPeriodReport(30);
            report.Add(new OrderPeriodReportLineModel
            {
                Period = _localizationService.GetResource("Admin.SalesReport.Period.month"),
                Count = reportperiodmonth.Count,
                Amount = reportperiodmonth.Amount
            });

            var reportperiodyear = _orderReportService.GetOrderPeriodReport(365);
            report.Add(new OrderPeriodReportLineModel
            {
                Period = _localizationService.GetResource("Admin.SalesReport.Period.year"),
                Count = reportperiodyear.Count,
                Amount = reportperiodyear.Amount
            });

            return report;
        }


        [HttpPost]
        public IActionResult BestsellersBriefReportByQuantityList(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content("");

            var gridModel = GetBestsellersBriefReportModel(command.Page - 1,
                command.PageSize, 1);

            return Json(gridModel);
        }
        [HttpPost]
        public IActionResult BestsellersBriefReportByAmountList(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content("");

            var gridModel = GetBestsellersBriefReportModel(command.Page - 1,
                command.PageSize, 2);

            return Json(gridModel);
        }

        public IActionResult BestsellersReport()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var model = new BestsellersReportModel();
            //vendor
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            //order statuses
            model.AvailableOrderStatuses = OrderStatus.Pending.ToSelectList(false).ToList();
            model.AvailableOrderStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });

            //payment statuses
            model.AvailablePaymentStatuses = PaymentStatus.Pending.ToSelectList(false).ToList();
            model.AvailablePaymentStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });

            //billing countries
            foreach (var c in _countryService.GetAllCountriesForBilling(showHidden: true))
            {
                model.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
            }
            model.AvailableCountries.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            var vendors = _vendorService.GetAllVendors(showHidden: true);
            foreach (var v in vendors)
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            return View(model);
        }
        [HttpPost]
        public IActionResult BestsellersReportList(DataSourceRequest command, BestsellersReportModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content("");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.VendorId = _workContext.CurrentVendor.Id;
            }

            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            OrderStatus? orderStatus = model.OrderStatusId > 0 ? (OrderStatus?)(model.OrderStatusId) : null;
            PaymentStatus? paymentStatus = model.PaymentStatusId > 0 ? (PaymentStatus?)(model.PaymentStatusId) : null;

            var items = _orderReportService.BestSellersReport(
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                os: orderStatus,
                ps: paymentStatus,
                billingCountryId: model.BillingCountryId,
                orderBy: 2,
                vendorId: model.VendorId,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize,
                showHidden: true,
                storeId: model.StoreId);
            var gridModel = new DataSourceResult
            {
                Data = items.Select(x =>
                {
                    var m = new BestsellersReportLineModel
                    {
                        ProductId = x.ProductId,
                        TotalAmount = _priceFormatter.FormatPrice(x.TotalAmount, true, false),
                        TotalQuantity = x.TotalQuantity,
                    };
                    var product = _productService.GetProductById(x.ProductId);
                    if (product != null)
                        m.ProductName = product.Name;
                    return m;
                }),
                Total = items.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ReportOrderPeriodList(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content("");

            var model = GetReportOrderPeriodModel();
            var gridModel = new DataSourceResult
            {
                Data = model,
                Total = model.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ReportOrderTimeChart(DataSourceRequest command, DateTime? startDate, DateTime? endDate)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content("");

            var model = _orderReportService.GetOrderByTimeReport(startDate, endDate);
            var gridModel = new DataSourceResult
            {
                Data = model
            };
            return Json(gridModel);
        }

        public IActionResult NeverSoldReport()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var model = new NeverSoldReportModel();
            return View(model);
        }
        [HttpPost]
        public IActionResult NeverSoldReportList(DataSourceRequest command, NeverSoldReportModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content("");

            //a vendor should have access only to his products
            string vendorId = "";
            if (_workContext.CurrentVendor != null)
                vendorId = _workContext.CurrentVendor.Id;

            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            var items = _orderReportService.ProductsNeverSold(vendorId,
                startDateValue, endDateValue,
                command.Page - 1, command.PageSize, true);
            var gridModel = new DataSourceResult
            {
                Data = items.Select(x =>
                    new NeverSoldReportLineModel
                    {
                        ProductId = x.Id,
                        ProductName = x.Name,
                    }),
                Total = items.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult OrderAverageReportList(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content("");

            //a vendor does have access to this report
            if (_workContext.CurrentVendor != null)
                return Content("");


            var report = new List<OrderAverageReportLineSummary>();
            report.Add(_orderReportService.OrderAverageReport("", OrderStatus.Pending));
            report.Add(_orderReportService.OrderAverageReport("", OrderStatus.Processing));
            report.Add(_orderReportService.OrderAverageReport("", OrderStatus.Complete));
            report.Add(_orderReportService.OrderAverageReport("", OrderStatus.Cancelled));
            var model = report.Select(x => new OrderAverageReportLineSummaryModel
            {
                OrderStatus = x.OrderStatus.GetLocalizedEnum(_localizationService, _workContext),
                SumTodayOrders = _priceFormatter.FormatPrice(x.SumTodayOrders, true, false),
                SumThisWeekOrders = _priceFormatter.FormatPrice(x.SumThisWeekOrders, true, false),
                SumThisMonthOrders = _priceFormatter.FormatPrice(x.SumThisMonthOrders, true, false),
                SumThisYearOrders = _priceFormatter.FormatPrice(x.SumThisYearOrders, true, false),
                SumAllTimeOrders = _priceFormatter.FormatPrice(x.SumAllTimeOrders, true, false),
            }).ToList();

            var gridModel = new DataSourceResult
            {
                Data = model,
                Total = model.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ReportLatestOrder(DataSourceRequest command, DateTime? startDate, DateTime? endDate)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content("");

            //a vendor does have access to this report
            if (_workContext.CurrentVendor != null)
                return Content("");


            //load orders
            var orders = _orderService.SearchOrders(
                createdFromUtc: startDate,
                createdToUtc: endDate,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = orders.Select(x =>
                {
                    var store = _storeService.GetStoreById(x.StoreId);
                    return new OrderModel
                    {
                        Id = x.Id,
                        OrderNumber = x.OrderNumber,
                        StoreName = store != null ? store.Name : "Unknown",
                        OrderTotal = _priceFormatter.FormatPrice(x.OrderTotal, true, false),
                        OrderStatus = x.OrderStatus.GetLocalizedEnum(_localizationService, _workContext),
                        PaymentStatus = x.PaymentStatus.GetLocalizedEnum(_localizationService, _workContext),
                        ShippingStatus = x.ShippingStatus.GetLocalizedEnum(_localizationService, _workContext),
                        CustomerEmail = x.BillingAddress.Email,
                        CustomerFullName = string.Format("{0} {1}", x.BillingAddress.FirstName, x.BillingAddress.LastName),
                        CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc)
                    };
                }),
                Total = orders.TotalCount
            };


            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult OrderIncompleteReportList(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content("");

            //a vendor does have access to this report
            if (_workContext.CurrentVendor != null)
                return Content("");

            var model = new List<OrderIncompleteReportLineModel>();
            //not paid
            var psPending = _orderReportService.GetOrderAverageReportLine(ps: PaymentStatus.Pending, ignoreCancelledOrders: true);
            model.Add(new OrderIncompleteReportLineModel
            {
                Item = _localizationService.GetResource("Admin.SalesReport.Incomplete.TotalUnpaidOrders"),
                Count = psPending.CountOrders,
                Total = _priceFormatter.FormatPrice(psPending.SumOrders, true, false),
                ViewLink = Url.Action("List", "Order", new { paymentStatusId = ((int)PaymentStatus.Pending).ToString() })
            });
            //not shipped
            var ssPending = _orderReportService.GetOrderAverageReportLine(ss: ShippingStatus.NotYetShipped, ignoreCancelledOrders: true);
            model.Add(new OrderIncompleteReportLineModel
            {
                Item = _localizationService.GetResource("Admin.SalesReport.Incomplete.TotalNotShippedOrders"),
                Count = ssPending.CountOrders,
                Total = _priceFormatter.FormatPrice(ssPending.SumOrders, true, false),
                ViewLink = Url.Action("List", "Order", new { shippingStatusId = ((int)ShippingStatus.NotYetShipped).ToString() })
            });
            //pending
            var osPending = _orderReportService.GetOrderAverageReportLine(os: OrderStatus.Pending, ignoreCancelledOrders: true);
            model.Add(new OrderIncompleteReportLineModel
            {
                Item = _localizationService.GetResource("Admin.SalesReport.Incomplete.TotalIncompleteOrders"),
                Count = osPending.CountOrders,
                Total = _priceFormatter.FormatPrice(osPending.SumOrders, true, false),
                ViewLink = Url.Action("List", "Order", new { orderStatusId = ((int)OrderStatus.Pending).ToString() })
            });

            var gridModel = new DataSourceResult
            {
                Data = model,
                Total = model.Count
            };

            return Json(gridModel);
        }

        public IActionResult CountryReport()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.OrderCountryReport))
                return AccessDeniedView();

            var model = new CountryReportModel();

            //order statuses
            model.AvailableOrderStatuses = OrderStatus.Pending.ToSelectList(false).ToList();
            model.AvailableOrderStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });

            //payment statuses
            model.AvailablePaymentStatuses = PaymentStatus.Pending.ToSelectList(false).ToList();
            model.AvailablePaymentStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });

            return View(model);
        }

        [HttpPost]
        public IActionResult CountryReportList(DataSourceRequest command, CountryReportModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.OrderCountryReport))
                return Content("");

            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            OrderStatus? orderStatus = model.OrderStatusId > 0 ? (OrderStatus?)(model.OrderStatusId) : null;
            PaymentStatus? paymentStatus = model.PaymentStatusId > 0 ? (PaymentStatus?)(model.PaymentStatusId) : null;

            var items = _orderReportService.GetCountryReport(
                os: orderStatus,
                ps: paymentStatus,
                startTimeUtc: startDateValue,
                endTimeUtc: endDateValue);
            var gridModel = new DataSourceResult
            {
                Data = items.Select(x =>
                {
                    var country = _countryService.GetCountryById(!String.IsNullOrEmpty(x.CountryId) ? x.CountryId : "");
                    var m = new CountryReportLineModel
                    {
                        CountryName = country != null ? country.Name : "Unknown",
                        SumOrders = _priceFormatter.FormatPrice(x.SumOrders, true, false),
                        TotalOrders = x.TotalOrders,
                    };
                    return m;
                }),
                Total = items.Count
            };

            return Json(gridModel);
        }

      
    }
}
