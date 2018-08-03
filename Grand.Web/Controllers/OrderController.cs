using Grand.Core;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Shipping;
using Grand.Core.Infrastructure;
using Grand.Framework.Controllers;
using Grand.Framework.Security;
using Grand.Services.Common;
using Grand.Services.Orders;
using Grand.Services.Payments;
using Grand.Services.Shipping;
using Grand.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;

namespace Grand.Web.Controllers
{
    public partial class OrderController : BasePublicController
    {
        #region Fields

        private readonly IOrderWebService _orderWebService;
        private readonly IOrderService _orderService;
        private readonly IWorkContext _workContext;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IPaymentService _paymentService;

        #endregion

		#region Constructors

        public OrderController(IOrderWebService orderWebService,
            IOrderService orderService,
            IWorkContext workContext,
            IOrderProcessingService orderProcessingService, 
            IPaymentService paymentService)
        {
            this._orderWebService = orderWebService;
            this._orderService = orderService;
            this._workContext = workContext;
            this._orderProcessingService = orderProcessingService;
            this._paymentService = paymentService;
        }

        #endregion

        #region Methods

        //My account / Orders
        public virtual IActionResult CustomerOrders()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var model = _orderWebService.PrepareCustomerOrderList();
            return View(model);
        }

        //My account / Orders / Cancel recurring order
        [HttpPost, ActionName("CustomerOrders")]
        [PublicAntiForgery]
        [FormValueRequired(FormValueRequirement.StartsWith, "cancelRecurringPayment")]
        public virtual IActionResult CancelRecurringPayment(IFormCollection form)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            //get recurring payment identifier
            string recurringPaymentId = "";
            foreach (var formValue in form.Keys)
                if (formValue.StartsWith("cancelRecurringPayment", StringComparison.OrdinalIgnoreCase))
                    recurringPaymentId = formValue.Substring("cancelRecurringPayment".Length);

            var recurringPayment = _orderService.GetRecurringPaymentById(recurringPaymentId);
            if (recurringPayment == null)
            {
                return RedirectToRoute("CustomerOrders");
            }

            if (_orderProcessingService.CanCancelRecurringPayment(_workContext.CurrentCustomer, recurringPayment))
            {
                var errors = _orderProcessingService.CancelRecurringPayment(recurringPayment);

                var model = _orderWebService.PrepareCustomerOrderList();
                model.CancelRecurringPaymentErrors = errors;

                return View(model);
            }
            else
            {
                return RedirectToRoute("CustomerOrders");
            }
        }

        //My account / Reward points
        public virtual IActionResult CustomerRewardPoints()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var rewardPointsSettings = EngineContext.Current.Resolve<RewardPointsSettings>();
            if (!rewardPointsSettings.Enabled)
                return RedirectToRoute("CustomerInfo");

            var customer = _workContext.CurrentCustomer;
            var model = _orderWebService.PrepareCustomerRewardPoints(customer);
            return View(model);
        }

        //My account / Order details page
        public virtual IActionResult Details(string orderId)
        {
            var order = _orderService.GetOrderById(orderId);
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return Challenge();

            var model = _orderWebService.PrepareOrderDetails(order);

            return View(model);
        }

        //My account / Order details page / Print
        public virtual IActionResult PrintOrderDetails(string orderId)
        {
            var order = _orderService.GetOrderById(orderId);
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return Challenge();

            var model = _orderWebService.PrepareOrderDetails(order);
            model.PrintMode = true;

            return View("Details", model);
        }

        //My account / Order details page / Cancel Unpaid Order
        public IActionResult CancelOrder(string orderId)
        {
            var orderSettings = EngineContext.Current.Resolve<OrderSettings>();
            var order = _orderService.GetOrderById(orderId);
            if (order == null || order.PaymentStatus != Core.Domain.Payments.PaymentStatus.Pending
                || (order.ShippingStatus != ShippingStatus.ShippingNotRequired && order.ShippingStatus != ShippingStatus.NotYetShipped)
                || order.OrderStatus != OrderStatus.Pending
                || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId
                || !orderSettings.UserCanCancelUnpaidOrder)

                return Challenge();

            _orderProcessingService.CancelOrder(order, true, true);

            return RedirectToRoute("OrderDetails", new { orderId = orderId });
        }

        //My account / Order details page / PDF invoice
        public virtual IActionResult GetPdfInvoice(string orderId)
        {
            var order = _orderService.GetOrderById(orderId);
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return Challenge();

            var orders = new List<Order>();
            orders.Add(order);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                EngineContext.Current.Resolve<IPdfService>().PrintOrdersToPdf(stream, orders, _workContext.WorkingLanguage.Id);
                bytes = stream.ToArray();
            }
            return File(bytes, "application/pdf", string.Format("order_{0}.pdf", order.Id));
        }

        //My account / Order details page / re-order
        public virtual IActionResult ReOrder(string orderId)
        {
            var order = _orderService.GetOrderById(orderId);
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return Challenge();

            _orderProcessingService.ReOrder(order);
            return RedirectToRoute("ShoppingCart");
        }

        //My account / Order details page / Complete payment
        [HttpPost, ActionName("Details")]
        [FormValueRequired("repost-payment")]
        [PublicAntiForgery]
        public virtual IActionResult RePostPayment(string orderId, [FromServices] IWebHelper webHelper)
        {
            var order = _orderService.GetOrderById(orderId);
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return Challenge();

            if (!_paymentService.CanRePostProcessPayment(order))
                return RedirectToRoute("OrderDetails", new { orderId = orderId });

            var postProcessPaymentRequest = new PostProcessPaymentRequest
            {
                Order = order
            };
            _paymentService.PostProcessPayment(postProcessPaymentRequest);

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
        public virtual IActionResult ShipmentDetails(string shipmentId)
        {
            var shipment = EngineContext.Current.Resolve<IShipmentService>().GetShipmentById(shipmentId);
            if (shipment == null)
                return Challenge();

            var order = _orderService.GetOrderById(shipment.OrderId);
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return Challenge();

            var model = _orderWebService.PrepareShipmentDetails(shipment);

            return View(model);
        }

        #endregion
    }
}
