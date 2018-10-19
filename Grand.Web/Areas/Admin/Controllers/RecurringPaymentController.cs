using Grand.Core;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Orders;
using Grand.Core.Infrastructure;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc.Filters;
using Grand.Services.Customers;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Orders;
using Grand.Services.Payments;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Models.Orders;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Grand.Web.Areas.Admin.Controllers
{
    public partial class RecurringPaymentController : BaseAdminController
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPaymentService _paymentService;
        private readonly IPermissionService _permissionService;

        #endregion Fields

        #region Constructors

        public RecurringPaymentController(IOrderService orderService,
            IOrderProcessingService orderProcessingService, ILocalizationService localizationService,
            IWorkContext workContext, IDateTimeHelper dateTimeHelper, IPaymentService paymentService,
            IPermissionService permissionService)
        {
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._dateTimeHelper = dateTimeHelper;
            this._paymentService = paymentService;
            this._permissionService = permissionService;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected virtual void PrepareRecurringPaymentModel(RecurringPaymentModel model, 
            RecurringPayment recurringPayment)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (recurringPayment == null)
                throw new ArgumentNullException("recurringPayment");
            
            model.Id = recurringPayment.Id;
            model.CycleLength = recurringPayment.CycleLength;
            model.CyclePeriodId = recurringPayment.CyclePeriodId;
            model.CyclePeriodStr = recurringPayment.CyclePeriod.GetLocalizedEnum(_localizationService, _workContext);
            model.TotalCycles = recurringPayment.TotalCycles;
            model.StartDate = _dateTimeHelper.ConvertToUserTime(recurringPayment.StartDateUtc, DateTimeKind.Utc).ToString();
            model.IsActive = recurringPayment.IsActive;
            model.NextPaymentDate = recurringPayment.NextPaymentDate.HasValue ? _dateTimeHelper.ConvertToUserTime(recurringPayment.NextPaymentDate.Value, DateTimeKind.Utc).ToString() : "";
            model.CyclesRemaining = recurringPayment.CyclesRemaining;
            model.InitialOrderId = recurringPayment.InitialOrder.Id;
            var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(recurringPayment.InitialOrder.CustomerId);
            model.CustomerId = customer.Id;
            model.CustomerEmail = customer.IsRegistered() ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest");
            model.PaymentType = _paymentService.GetRecurringPaymentType(recurringPayment.InitialOrder.PaymentMethodSystemName).GetLocalizedEnum(_localizationService, _workContext);
            model.CanCancelRecurringPayment = _orderProcessingService.CanCancelRecurringPayment(_workContext.CurrentCustomer, recurringPayment);
        }

        [NonAction]
        protected virtual void PrepareRecurringPaymentHistoryModel(RecurringPaymentModel.RecurringPaymentHistoryModel model,
            RecurringPaymentHistory history)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (history == null)
                throw new ArgumentNullException("history");

            var order = _orderService.GetOrderById(history.OrderId);

            model.Id = history.Id;
            model.OrderId = history.OrderId;
            model.OrderNumber = order != null ? order.OrderNumber: 0;
            model.RecurringPaymentId = history.RecurringPaymentId;
            model.OrderStatus = order.OrderStatus.GetLocalizedEnum(_localizationService, _workContext);
            model.PaymentStatus = order.PaymentStatus.GetLocalizedEnum(_localizationService, _workContext);
            model.ShippingStatus = order.ShippingStatus.GetLocalizedEnum(_localizationService, _workContext);
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(history.CreatedOnUtc, DateTimeKind.Utc);
        }

        #endregion

        #region Recurring payment

        //list
        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageRecurringPayments))
                return AccessDeniedView();

            return View();
        }

        [HttpPost]
        public IActionResult List(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageRecurringPayments))
                return AccessDeniedView();

            var payments = _orderService.SearchRecurringPayments("", "", "", null, command.Page - 1, command.PageSize, true);
            var gridModel = new DataSourceResult
            {
                Data = payments.Select(x =>
                {
                    var m = new RecurringPaymentModel();
                    PrepareRecurringPaymentModel(m, x);
                    return m;
                }),
                Total = payments.TotalCount,
            };

            return Json(gridModel);
        }

        //edit
        public IActionResult Edit(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageRecurringPayments))
                return AccessDeniedView();

            var payment = _orderService.GetRecurringPaymentById(id);
            if (payment == null || payment.Deleted)
                //No recurring payment found with the specified id
                return RedirectToAction("List");

            var model = new RecurringPaymentModel();
            PrepareRecurringPaymentModel(model, payment);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public IActionResult Edit(RecurringPaymentModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageRecurringPayments))
                return AccessDeniedView();

            var payment = _orderService.GetRecurringPaymentById(model.Id);
            if (payment == null || payment.Deleted)
                //No recurring payment found with the specified id
                return RedirectToAction("List");

            payment.CycleLength = model.CycleLength;
            payment.CyclePeriodId = model.CyclePeriodId;
            payment.TotalCycles = model.TotalCycles;
            payment.IsActive = model.IsActive;
            _orderService.UpdateRecurringPayment(payment);

            SuccessNotification(_localizationService.GetResource("Admin.RecurringPayments.Updated"));

            if (continueEditing)
            {
                //selected tab
                SaveSelectedTabIndex();

                return RedirectToAction("Edit",  new {id = payment.Id});
            }
            return RedirectToAction("List");
        }

        //delete
        [HttpPost]
        public IActionResult Delete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageRecurringPayments))
                return AccessDeniedView();

            var payment = _orderService.GetRecurringPaymentById(id);
            if (payment == null)
                //No recurring payment found with the specified id
                return RedirectToAction("List");

            _orderService.DeleteRecurringPayment(payment);

            SuccessNotification(_localizationService.GetResource("Admin.RecurringPayments.Deleted"));
            return RedirectToAction("List");
        }

        #endregion

        #region History

        [HttpPost]
        public IActionResult HistoryList(string recurringPaymentId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageRecurringPayments))
                return AccessDeniedView();

            var payment = _orderService.GetRecurringPaymentById(recurringPaymentId);
            if (payment == null)
                throw new ArgumentException("No recurring payment found with the specified id");

            var historyModel = payment.RecurringPaymentHistory.OrderBy(x => x.CreatedOnUtc)
                .Select(x =>
                {
                    var m = new RecurringPaymentModel.RecurringPaymentHistoryModel();
                    PrepareRecurringPaymentHistoryModel(m, x);
                    return m;
                })
                .ToList();
            var gridModel = new DataSourceResult
            {
                Data = historyModel,
                Total = historyModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("processnextpayment")]
        public IActionResult ProcessNextPayment(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageRecurringPayments))
                return AccessDeniedView();

            var payment = _orderService.GetRecurringPaymentById(id);
            if (payment == null)
                //No recurring payment found with the specified id
                return RedirectToAction("List");
            
            try
            {
                _orderProcessingService.ProcessNextRecurringPayment(payment);
                var model = new RecurringPaymentModel();
                PrepareRecurringPaymentModel(model, payment);

                SuccessNotification(_localizationService.GetResource("Admin.RecurringPayments.NextPaymentProcessed"), false);

                //selected tab
                SaveSelectedTabIndex(persistForTheNextRequest: false);

                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new RecurringPaymentModel();
                PrepareRecurringPaymentModel(model, payment);
                ErrorNotification(exc, false);

                //selected tab
                SaveSelectedTabIndex(persistForTheNextRequest: false);

                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("cancelpayment")]
        public IActionResult CancelRecurringPayment(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageRecurringPayments))
                return AccessDeniedView();

            var payment = _orderService.GetRecurringPaymentById(id);
            if (payment == null)
                //No recurring payment found with the specified id
                return RedirectToAction("List");

            try
            {
                var errors = _orderProcessingService.CancelRecurringPayment(payment);
                var model = new RecurringPaymentModel();
                PrepareRecurringPaymentModel(model, payment);
                if (errors.Count > 0)
                {
                    foreach (var error in errors)
                        ErrorNotification(error, false);
                }
                else
                    SuccessNotification(_localizationService.GetResource("Admin.RecurringPayments.Cancelled"), false);

                //selected tab
                SaveSelectedTabIndex(persistForTheNextRequest: false);

                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new RecurringPaymentModel();
                PrepareRecurringPaymentModel(model, payment);
                ErrorNotification(exc, false);

                //selected tab
                SaveSelectedTabIndex(persistForTheNextRequest: false);

                return View(model);
            }
        }

        #endregion
    }
}
