using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Payments;
using Grand.Core.Domain.Shipping;
using Grand.Framework.Extensions;
using Grand.Framework.Kendoui;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Orders;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Services.Vendors;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Web.Areas.Admin.Models.Common;
using Grand.Web.Areas.Admin.Models.Orders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreService _storeService;
        private readonly ICountryService _countryService;
        private readonly IVendorService _vendorService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ISearchTermService _searchTermService;

        public ReportsController(IOrderService orderService,
        IOrderReportService orderReportService,
        IPermissionService permissionService,
        IWorkContext workContext,
        IPriceFormatter priceFormatter,
        IProductService productService,
        IProductAttributeFormatter productAttributeFormatter,
        ILocalizationService localizationService,
        IStoreService storeService,
        ICountryService countryService,
        IVendorService vendorService,
        IDateTimeHelper dateTimeHelper,
        ISearchTermService searchTermService)
        {
            _orderService = orderService;
            _orderReportService = orderReportService;
            _permissionService = permissionService;
            _workContext = workContext;
            _priceFormatter = priceFormatter;
            _productService = productService;
            _productAttributeFormatter = productAttributeFormatter;
            _localizationService = localizationService;
            _storeService = storeService;
            _countryService = countryService;
            _vendorService = vendorService;
            _dateTimeHelper = dateTimeHelper;
            _searchTermService = searchTermService;
        }

        [NonAction]
        protected async Task<DataSourceResult> GetBestsellersBriefReportModel(int pageIndex,
            int pageSize, int orderBy)
        {
            //a vendor should have access only to his products
            string vendorId = "";
            if (_workContext.CurrentVendor != null)
                vendorId = _workContext.CurrentVendor.Id;

            var items = await _orderReportService.BestSellersReport(
                vendorId: vendorId,
                orderBy: orderBy,
                pageIndex: pageIndex,
                pageSize: pageSize,
                showHidden: true);
            var result = new List<BestsellersReportLineModel>();
            foreach (var x in items)
            {
                var m = new BestsellersReportLineModel
                {
                    ProductId = x.ProductId,
                    TotalAmount = _priceFormatter.FormatPrice(x.TotalAmount, true, false),
                    TotalQuantity = x.TotalQuantity,
                };
                var product = await _productService.GetProductById(x.ProductId);
                if (product != null)
                    m.ProductName = product.Name;
                result.Add(m);
            }

            var gridModel = new DataSourceResult
            {
                Data = result,
                Total = items.TotalCount
            };
            return gridModel;
        }

        [NonAction]
        protected virtual async Task<IList<OrderPeriodReportLineModel>> GetReportOrderPeriodModel()
        {
            var report = new List<OrderPeriodReportLineModel>();
            var reportperiod7days = await _orderReportService.GetOrderPeriodReport(7);
            report.Add(new OrderPeriodReportLineModel
            {
                Period = _localizationService.GetResource("Admin.SalesReport.Period.7days"),
                Count = reportperiod7days.Count,
                Amount = reportperiod7days.Amount
            });

            var reportperiod14days = await _orderReportService.GetOrderPeriodReport(14);
            report.Add(new OrderPeriodReportLineModel
            {
                Period = _localizationService.GetResource("Admin.SalesReport.Period.14days"),
                Count = reportperiod14days.Count,
                Amount = reportperiod14days.Amount
            });

            var reportperiodmonth = await _orderReportService.GetOrderPeriodReport(30);
            report.Add(new OrderPeriodReportLineModel
            {
                Period = _localizationService.GetResource("Admin.SalesReport.Period.month"),
                Count = reportperiodmonth.Count,
                Amount = reportperiodmonth.Amount
            });

            var reportperiodyear = await _orderReportService.GetOrderPeriodReport(365);
            report.Add(new OrderPeriodReportLineModel
            {
                Period = _localizationService.GetResource("Admin.SalesReport.Period.year"),
                Count = reportperiodyear.Count,
                Amount = reportperiodyear.Amount
            });

            return report;
        }


        [HttpPost]
        public async Task<IActionResult> BestsellersBriefReportByQuantityList(DataSourceRequest command)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content("");

            var gridModel = await GetBestsellersBriefReportModel(command.Page - 1,
                command.PageSize, 1);

            return Json(gridModel);
        }
        [HttpPost]
        public async Task<IActionResult> BestsellersBriefReportByAmountList(DataSourceRequest command)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content("");

            var gridModel = await GetBestsellersBriefReportModel(command.Page - 1,
                command.PageSize, 2);

            return Json(gridModel);
        }

        public async Task<IActionResult> BestsellersReport()
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var model = new BestsellersReportModel
            {
                //vendor
                IsLoggedInAsVendor = _workContext.CurrentVendor != null
            };

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            foreach (var s in await _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            //order statuses
            model.AvailableOrderStatuses = OrderStatus.Pending.ToSelectList(false).ToList();
            model.AvailableOrderStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });

            //payment statuses
            model.AvailablePaymentStatuses = PaymentStatus.Pending.ToSelectList(false).ToList();
            model.AvailablePaymentStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });

            //billing countries
            foreach (var c in await _countryService.GetAllCountriesForBilling(showHidden: true))
            {
                model.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
            }
            model.AvailableCountries.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            var vendors = await _vendorService.GetAllVendors(showHidden: true);
            foreach (var v in vendors)
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> BestsellersReportList(DataSourceRequest command, BestsellersReportModel model)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManageOrders))
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

            var items = await _orderReportService.BestSellersReport(
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

            var result = new List<BestsellersReportLineModel>();
            foreach (var x in items)
            {
                var m = new BestsellersReportLineModel
                {
                    ProductId = x.ProductId,
                    TotalAmount = _priceFormatter.FormatPrice(x.TotalAmount, true, false),
                    TotalQuantity = x.TotalQuantity,
                };
                var product = await _productService.GetProductById(x.ProductId);
                if (product != null)
                    m.ProductName = product.Name;
                if (_workContext.CurrentVendor != null)
                {
                    if(product.VendorId == _workContext.CurrentVendor.Id)
                        result.Add(m);
                }
                else
                    result.Add(m);
            }
            var gridModel = new DataSourceResult
            {
                Data = result,
                Total = items.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        public async Task<IActionResult> ReportOrderPeriodList(DataSourceRequest command)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content("");

            var model = await GetReportOrderPeriodModel();
            var gridModel = new DataSourceResult
            {
                Data = model,
                Total = model.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public async Task<IActionResult> ReportOrderTimeChart(DataSourceRequest command, DateTime? startDate, DateTime? endDate)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content("");

            var model = await _orderReportService.GetOrderByTimeReport(startDate, endDate);
            var gridModel = new DataSourceResult
            {
                Data = model
            };
            return Json(gridModel);
        }

        public async Task<IActionResult> NeverSoldReport()
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var model = new NeverSoldReportModel();
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> NeverSoldReportList(DataSourceRequest command, NeverSoldReportModel model)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content("");

            //a vendor should have access only to his products
            string vendorId = "";
            if (_workContext.CurrentVendor != null)
                vendorId = _workContext.CurrentVendor.Id;

            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            var items = await _orderReportService.ProductsNeverSold(vendorId,
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
        public async Task<IActionResult> OrderAverageReportList(DataSourceRequest command)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content("");

            //a vendor does have access to this report
            if (_workContext.CurrentVendor != null)
                return Content("");


            var report = new List<OrderAverageReportLineSummary>
            {
                await _orderReportService.OrderAverageReport("", OrderStatus.Pending),
                await _orderReportService.OrderAverageReport("", OrderStatus.Processing),
                await _orderReportService.OrderAverageReport("", OrderStatus.Complete),
                await _orderReportService.OrderAverageReport("", OrderStatus.Cancelled)
            };
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
        public async Task<IActionResult> ReportLatestOrder(DataSourceRequest command, DateTime? startDate, DateTime? endDate)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content("");

            //a vendor does have access to this report
            if (_workContext.CurrentVendor != null)
                return Content("");


            //load orders
            var orders = await _orderService.SearchOrders(
                createdFromUtc: startDate,
                createdToUtc: endDate,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);

            var items = new List<OrderModel>();
            foreach (var x in orders)
            {
                var store = await _storeService.GetStoreById(x.StoreId);
                items.Add(new OrderModel
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
                });
            }
            var gridModel = new DataSourceResult
            {
                Data = items,
                Total = orders.TotalCount
            };
            return Json(gridModel);
        }

        [HttpPost]
        public async Task<IActionResult> OrderIncompleteReportList(DataSourceRequest command)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content("");

            //a vendor does have access to this report
            if (_workContext.CurrentVendor != null)
                return Content("");

            var model = new List<OrderIncompleteReportLineModel>();
            //not paid
            var psPending = await _orderReportService.GetOrderAverageReportLine(ps: PaymentStatus.Pending, ignoreCancelledOrders: true);
            model.Add(new OrderIncompleteReportLineModel
            {
                Item = _localizationService.GetResource("Admin.SalesReport.Incomplete.TotalUnpaidOrders"),
                Count = psPending.CountOrders,
                Total = _priceFormatter.FormatPrice(psPending.SumOrders, true, false),
                ViewLink = Url.Action("List", "Order", new { paymentStatusId = ((int)PaymentStatus.Pending).ToString() })
            });
            //not shipped
            var ssPending = await _orderReportService.GetOrderAverageReportLine(ss: ShippingStatus.NotYetShipped, ignoreCancelledOrders: true);
            model.Add(new OrderIncompleteReportLineModel
            {
                Item = _localizationService.GetResource("Admin.SalesReport.Incomplete.TotalNotShippedOrders"),
                Count = ssPending.CountOrders,
                Total = _priceFormatter.FormatPrice(ssPending.SumOrders, true, false),
                ViewLink = Url.Action("List", "Order", new { shippingStatusId = ((int)ShippingStatus.NotYetShipped).ToString() })
            });
            //pending
            var osPending = await _orderReportService.GetOrderAverageReportLine(os: OrderStatus.Pending, ignoreCancelledOrders: true);
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

        public async Task<IActionResult> CountryReport()
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.OrderCountryReport))
                return AccessDeniedView();

            var model = new CountryReportModel
            {

                //order statuses
                AvailableOrderStatuses = OrderStatus.Pending.ToSelectList(false).ToList()
            };
            model.AvailableOrderStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });

            //payment statuses
            model.AvailablePaymentStatuses = PaymentStatus.Pending.ToSelectList(false).ToList();
            model.AvailablePaymentStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CountryReportList(DataSourceRequest command, CountryReportModel model)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.OrderCountryReport))
                return Content("");

            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            OrderStatus? orderStatus = model.OrderStatusId > 0 ? (OrderStatus?)(model.OrderStatusId) : null;
            PaymentStatus? paymentStatus = model.PaymentStatusId > 0 ? (PaymentStatus?)(model.PaymentStatusId) : null;

            var items = await _orderReportService.GetCountryReport(
                os: orderStatus,
                ps: paymentStatus,
                startTimeUtc: startDateValue,
                endTimeUtc: endDateValue);
            var result = new List<CountryReportLineModel>();
            foreach (var x in items)
            {
                var country = await _countryService.GetCountryById(!String.IsNullOrEmpty(x.CountryId) ? x.CountryId : "");
                var m = new CountryReportLineModel
                {
                    CountryName = country != null ? country.Name : "Unknown",
                    SumOrders = _priceFormatter.FormatPrice(x.SumOrders, true, false),
                    TotalOrders = x.TotalOrders,
                };
                result.Add(m);
            }
            var gridModel = new DataSourceResult
            {
                Data = result,
                Total = items.Count
            };

            return Json(gridModel);
        }

        #region Low stock reports

        public async Task<IActionResult> LowStockReport()
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> LowStockReportList(DataSourceRequest command)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            string vendorId = "";
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
                vendorId = _workContext.CurrentVendor.Id;

            _productService.GetLowStockProducts(vendorId, out IList<Product> products, out IList<ProductAttributeCombination> combinations);

            var models = new List<LowStockProductModel>();
            //products
            foreach (var product in products)
            {
                var lowStockModel = new LowStockProductModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    ManageInventoryMethod = product.ManageInventoryMethod.GetLocalizedEnum(_localizationService, _workContext.WorkingLanguage.Id),
                    StockQuantity = product.GetTotalStockQuantity(),
                    Published = product.Published
                };
                models.Add(lowStockModel);
            }
            //combinations
            foreach (var combination in combinations)
            {
                var product = await _productService.GetProductById(combination.ProductId);
                var lowStockModel = new LowStockProductModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    Attributes = await _productAttributeFormatter.FormatAttributes(product, combination.AttributesXml, _workContext.CurrentCustomer, "<br />", true, true, true, false),
                    ManageInventoryMethod = product.ManageInventoryMethod.GetLocalizedEnum(_localizationService, _workContext.WorkingLanguage.Id),
                    StockQuantity = combination.StockQuantity,
                    Published = product.Published
                };
                models.Add(lowStockModel);
            }
            var gridModel = new DataSourceResult
            {
                Data = models.PagedForCommand(command),
                Total = models.Count
            };

            return Json(gridModel);
        }

        #endregion

        [HttpPost]
        public async Task<IActionResult> PopularSearchTermsReport(DataSourceRequest command)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var searchTermRecordLines = await _searchTermService.GetStats(command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = searchTermRecordLines.Select(x => new SearchTermReportLineModel
                {
                    Keyword = x.Keyword,
                    Count = x.Count,
                }),
                Total = searchTermRecordLines.TotalCount
            };
            return Json(gridModel);
        }

    }
}
