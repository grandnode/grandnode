using Grand.Core;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Shipping;
using Grand.Framework.Controllers;
using Grand.Framework.Security;
using Grand.Services.Common;
using Grand.Services.Localization;
using Grand.Services.Orders;
using Grand.Services.Payments;
using Grand.Services.Shipping;
using Grand.Web.Interfaces;
using Grand.Web.Models.Order;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Grand.Web.Controllers
{
    public partial class OrderController : BasePublicController
    {
        #region Fields

        private readonly IOrderViewModelService _orderViewModelService;
        private readonly IOrderService _orderService;
        private readonly IWorkContext _workContext;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IPaymentService _paymentService;
        private readonly ILocalizationService _localizationService;
        private readonly OrderSettings _orderSettings;

        #endregion

        #region Constructors

        public OrderController(IOrderViewModelService orderViewModelService,
            IOrderService orderService,
            IWorkContext workContext,
            IOrderProcessingService orderProcessingService,
            IPaymentService paymentService,
            ILocalizationService localizationService,
            OrderSettings orderSettings)
        {
            _orderViewModelService = orderViewModelService;
            _orderService = orderService;
            _workContext = workContext;
            _orderProcessingService = orderProcessingService;
            _paymentService = paymentService;
            _localizationService = localizationService;
            _orderSettings = orderSettings;
        }

        #endregion

        #region Methods

        //My account / Orders
        public virtual async Task<IActionResult> CustomerOrders()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var model = await _orderViewModelService.PrepareCustomerOrderList();
            return View(model);
        }

        //My account / Orders / Cancel recurring order
        [HttpPost, ActionName("CustomerOrders")]
        [PublicAntiForgery]
        [FormValueRequired(FormValueRequirement.StartsWith, "cancelRecurringPayment")]
        public virtual async Task<IActionResult> CancelRecurringPayment(IFormCollection form)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            //get recurring payment identifier
            string recurringPaymentId = "";
            foreach (var formValue in form.Keys)
                if (formValue.StartsWith("cancelRecurringPayment", StringComparison.OrdinalIgnoreCase))
                    recurringPaymentId = formValue.Substring("cancelRecurringPayment".Length);

            var recurringPayment = await _orderService.GetRecurringPaymentById(recurringPaymentId);
            if (recurringPayment == null)
            {
                return RedirectToRoute("CustomerOrders");
            }

            if (await _orderProcessingService.CanCancelRecurringPayment(_workContext.CurrentCustomer, recurringPayment))
            {
                var errors = await _orderProcessingService.CancelRecurringPayment(recurringPayment);

                var model = await _orderViewModelService.PrepareCustomerOrderList();
                model.CancelRecurringPaymentErrors = errors;

                return View(model);
            }
            else
            {
                return RedirectToRoute("CustomerOrders");
            }
        }

        //My account / Reward points
        public virtual async Task<IActionResult> CustomerRewardPoints([FromServices] RewardPointsSettings rewardPointsSettings)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            if (!rewardPointsSettings.Enabled)
                return RedirectToRoute("CustomerInfo");

            var customer = _workContext.CurrentCustomer;
            var model = await _orderViewModelService.PrepareCustomerRewardPoints(customer);
            return View(model);
        }

        //My account / Order details page
        public virtual async Task<IActionResult> Details(string orderId)
        {
            var order = await _orderService.GetOrderById(orderId);
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return Challenge();

            var model = await _orderViewModelService.PrepareOrderDetails(order);

            return View(model);
        }

        //My account / Order details page / Print
        public virtual async Task<IActionResult> PrintOrderDetails(string orderId)
        {
            var order = await _orderService.GetOrderById(orderId);
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return Challenge();

            var model = await _orderViewModelService.PrepareOrderDetails(order);
            model.PrintMode = true;

            return View("Details", model);
        }

        //My account / Order details page / Cancel Unpaid Order
        public virtual async Task<IActionResult> CancelOrder(string orderId)
        {
            var order = await _orderService.GetOrderById(orderId);
            if (order == null || order.PaymentStatus != Core.Domain.Payments.PaymentStatus.Pending
                || (order.ShippingStatus != ShippingStatus.ShippingNotRequired && order.ShippingStatus != ShippingStatus.NotYetShipped)
                || order.OrderStatus != OrderStatus.Pending
                || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId
                || !_orderSettings.UserCanCancelUnpaidOrder)

                return Challenge();

            await _orderProcessingService.CancelOrder(order, true, true);

            return RedirectToRoute("OrderDetails", new { orderId = orderId });
        }

        //My account / Order details page / PDF invoice
        public virtual async Task<IActionResult> GetPdfInvoice(string orderId, [FromServices] IPdfService pdfService)
        {
            var order = await _orderService.GetOrderById(orderId);
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return Challenge();

            var orders = new List<Order>();
            orders.Add(order);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await pdfService.PrintOrdersToPdf(stream, orders, _workContext.WorkingLanguage.Id);
                bytes = stream.ToArray();
            }
            return File(bytes, "application/pdf", string.Format("order_{0}.pdf", order.Id));
        }

        //My account / Order details page / Add order note
        public virtual async Task<IActionResult> AddOrderNote(string orderId)
        {
            if (!_orderSettings.AllowCustomerToAddOrderNote)
                return RedirectToRoute("HomePage");

            var order = await _orderService.GetOrderById(orderId);
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return Challenge();

            var model = new AddOrderNoteModel();
            model.OrderId = orderId;
            return View("AddOrderNote", model);
        }

        //My account / Order details page / Add order note
        [HttpPost]
        [PublicAntiForgery]
        public virtual async Task<IActionResult> AddOrderNote(AddOrderNoteModel model)
        {
            if (!_orderSettings.AllowCustomerToAddOrderNote)
                return RedirectToRoute("HomePage");

            if (!ModelState.IsValid)
            {
                return View("AddOrderNote", model);
            }

            var order = await _orderService.GetOrderById(model.OrderId);
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return Challenge();

            await _orderViewModelService.InsertOrderNote(model);

            AddNotification(Framework.UI.NotifyType.Success, _localizationService.GetResource("OrderNote.Added"), true);
            return RedirectToRoute("OrderDetails", model.OrderId);
        }

        //My account / Order details page / re-order
        public virtual async Task<IActionResult> ReOrder(string orderId)
        {
            var order = await _orderService.GetOrderById(orderId);
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return Challenge();

            await _orderProcessingService.ReOrder(order);
            return RedirectToRoute("ShoppingCart");
        }

        //My account / Order details page / Complete payment
        [HttpPost, ActionName("Details")]
        [FormValueRequired("repost-payment")]
        [PublicAntiForgery]
        public virtual async Task<IActionResult> RePostPayment(string orderId, [FromServices] IWebHelper webHelper)
        {
            var order = await _orderService.GetOrderById(orderId);
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return Challenge();

            if (!await _paymentService.CanRePostProcessPayment(order))
                return RedirectToRoute("OrderDetails", new { orderId = orderId });

            var postProcessPaymentRequest = new PostProcessPaymentRequest
            {
                Order = order
            };
            await _paymentService.PostProcessPayment(postProcessPaymentRequest);

            if (webHelper.IsRequestBeingRedirected || webHelper.IsPostBeingDone)
            {
                //redirection or POST has been done in PostProcessPayment
                return Content("Redirected");
            }

            //if no redirection has been done (to a third-party payment page)
            //theoretically it's not possible
            return RedirectToRoute("OrderDetails", new { orderId = orderId });
        }

        //My account / Order details page / Shipment details page
        public virtual async Task<IActionResult> ShipmentDetails(string shipmentId, [FromServices] IShipmentService shipmentService)
        {
            var shipment = await shipmentService.GetShipmentById(shipmentId);
            if (shipment == null)
                return Challenge();

            var order = await _orderService.GetOrderById(shipment.OrderId);
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return Challenge();

            var model = await _orderViewModelService.PrepareShipmentDetails(shipment);

            return View(model);
        }

        #endregion
    }
}
