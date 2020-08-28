using Grand.Core;
using Grand.Domain.Customers;
using Grand.Domain.Orders;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Customers;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Orders;
using Grand.Services.Payments;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Models.Orders;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.RecurringPayments)]
    public partial class RecurringPaymentController : BaseAdminController
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPaymentService _paymentService;
        private readonly ICustomerService _customerService;
        #endregion Fields

        #region Constructors

        public RecurringPaymentController(IOrderService orderService,
            IOrderProcessingService orderProcessingService, ILocalizationService localizationService,
            IWorkContext workContext, IDateTimeHelper dateTimeHelper, IPaymentService paymentService,
            ICustomerService customerService)
        {
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _localizationService = localizationService;
            _workContext = workContext;
            _dateTimeHelper = dateTimeHelper;
            _paymentService = paymentService;
            _customerService = customerService;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected virtual async Task PrepareRecurringPaymentModel(RecurringPaymentModel model, 
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
            var customer = await _customerService.GetCustomerById(recurringPayment.InitialOrder.CustomerId);
            model.CustomerId = customer.Id;
            model.CustomerEmail = customer.IsRegistered() ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest");
            model.PaymentType = _paymentService.GetRecurringPaymentType(recurringPayment.InitialOrder.PaymentMethodSystemName).GetLocalizedEnum(_localizationService, _workContext);
            model.CanCancelRecurringPayment = await _orderProcessingService.CanCancelRecurringPayment(_workContext.CurrentCustomer, recurringPayment);
        }

        [NonAction]
        protected virtual async Task PrepareRecurringPaymentHistoryModel(RecurringPaymentModel.RecurringPaymentHistoryModel model,
            RecurringPaymentHistory history)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (history == null)
                throw new ArgumentNullException("history");

            var order = await _orderService.GetOrderById(history.OrderId);

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
        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List() => View();

        [PermissionAuthorizeAction(PermissionActionName.List)]
        [HttpPost]
        public async Task<IActionResult> List(DataSourceRequest command)
        {
            var payments = await _orderService.SearchRecurringPayments("", "", "", null, command.Page - 1, command.PageSize, true);
            var items = new List<RecurringPaymentModel>();
            foreach (var x in payments)
            {
                var m = new RecurringPaymentModel();
                await PrepareRecurringPaymentModel(m, x);
                items.Add(m);
            }
            var gridModel = new DataSourceResult
            {
                Data = items,
                Total = payments.TotalCount,
            };
            return Json(gridModel);
        }

        //edit
        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        public async Task<IActionResult> Edit(string id)
        {
            var payment = await _orderService.GetRecurringPaymentById(id);
            if (payment == null || payment.Deleted)
                //No recurring payment found with the specified id
                return RedirectToAction("List");

            var model = new RecurringPaymentModel();
            await PrepareRecurringPaymentModel(model, payment);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public async Task<IActionResult> Edit(RecurringPaymentModel model, bool continueEditing)
        {
            var payment = await _orderService.GetRecurringPaymentById(model.Id);
            if (payment == null || payment.Deleted)
                //No recurring payment found with the specified id
                return RedirectToAction("List");

            payment.CycleLength = model.CycleLength;
            payment.CyclePeriodId = model.CyclePeriodId;
            payment.TotalCycles = model.TotalCycles;
            payment.IsActive = model.IsActive;
            await _orderService.UpdateRecurringPayment(payment);

            SuccessNotification(_localizationService.GetResource("Admin.RecurringPayments.Updated"));

            if (continueEditing)
            {
                //selected tab
                await SaveSelectedTabIndex();

                return RedirectToAction("Edit",  new {id = payment.Id});
            }
            return RedirectToAction("List");
        }

        //delete
        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var payment = await _orderService.GetRecurringPaymentById(id);
            if (payment == null)
                //No recurring payment found with the specified id
                return RedirectToAction("List");

            await _orderService.DeleteRecurringPayment(payment);

            SuccessNotification(_localizationService.GetResource("Admin.RecurringPayments.Deleted"));
            return RedirectToAction("List");
        }

        #endregion

        #region History

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> HistoryList(string recurringPaymentId, DataSourceRequest command)
        {
            var payment = await _orderService.GetRecurringPaymentById(recurringPaymentId);
            if (payment == null)
                throw new ArgumentException("No recurring payment found with the specified id");
            var historyModel = new List<RecurringPaymentModel.RecurringPaymentHistoryModel>();
            foreach (var x in payment.RecurringPaymentHistory.OrderBy(x => x.CreatedOnUtc))
            {
                var m = new RecurringPaymentModel.RecurringPaymentHistoryModel();
                await PrepareRecurringPaymentHistoryModel(m, x);
                historyModel.Add(m);
            }
            var gridModel = new DataSourceResult
            {
                Data = historyModel,
                Total = historyModel.Count
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("processnextpayment")]
        public async Task<IActionResult> ProcessNextPayment(string id)
        {
            var payment = await _orderService.GetRecurringPaymentById(id);
            if (payment == null)
                //No recurring payment found with the specified id
                return RedirectToAction("List");
            
            try
            {
                await _orderProcessingService.ProcessNextRecurringPayment(payment);
                var model = new RecurringPaymentModel();
                await PrepareRecurringPaymentModel(model, payment);

                SuccessNotification(_localizationService.GetResource("Admin.RecurringPayments.NextPaymentProcessed"), false);

                //selected tab
                await SaveSelectedTabIndex(persistForTheNextRequest: false);

                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new RecurringPaymentModel();
                await PrepareRecurringPaymentModel(model, payment);
                ErrorNotification(exc, false);

                //selected tab
                await SaveSelectedTabIndex(persistForTheNextRequest: false);

                return View(model);
            }
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("cancelpayment")]
        public async Task<IActionResult> CancelRecurringPayment(string id)
        {
            var payment = await _orderService.GetRecurringPaymentById(id);
            if (payment == null)
                //No recurring payment found with the specified id
                return RedirectToAction("List");

            try
            {
                var errors = await _orderProcessingService.CancelRecurringPayment(payment);
                var model = new RecurringPaymentModel();
                await PrepareRecurringPaymentModel(model, payment);
                if (errors.Count > 0)
                {
                    foreach (var error in errors)
                        ErrorNotification(error, false);
                }
                else
                    SuccessNotification(_localizationService.GetResource("Admin.RecurringPayments.Cancelled"), false);

                //selected tab
                await SaveSelectedTabIndex(persistForTheNextRequest: false);

                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new RecurringPaymentModel();
                await PrepareRecurringPaymentModel(model, payment);
                ErrorNotification(exc, false);

                //selected tab
                await SaveSelectedTabIndex(persistForTheNextRequest: false);

                return View(model);
            }
        }

        #endregion
    }
}
