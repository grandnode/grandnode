using Grand.Core;
using Grand.Domain.Customers;
using Grand.Domain.Orders;
using Grand.Domain.Shipping;
using Grand.Framework.Controllers;
using Grand.Services.Commands.Models.Orders;
using Grand.Services.Common;
using Grand.Services.Localization;
using Grand.Services.Orders;
using Grand.Services.Payments;
using Grand.Services.Shipping;
using Grand.Web.Commands.Models.Orders;
using Grand.Web.Events;
using Grand.Web.Features.Models.Orders;
using Grand.Web.Models.Orders;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Grand.Web.Extensions;

namespace Grand.Web.Controllers
{
    public partial class OrderController : BasePublicController
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IPaymentService _paymentService;
        private readonly ILocalizationService _localizationService;
        private readonly IMediator _mediator;
        private readonly OrderSettings _orderSettings;

        #endregion

        #region Constructors

        public OrderController(IOrderService orderService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IOrderProcessingService orderProcessingService,
            IPaymentService paymentService,
            ILocalizationService localizationService,
            IMediator mediator,
            OrderSettings orderSettings)
        {
            _orderService = orderService;
            _workContext = workContext;
            _storeContext = storeContext;
            _orderProcessingService = orderProcessingService;
            _paymentService = paymentService;
            _localizationService = localizationService;
            _mediator = mediator;
            _orderSettings = orderSettings;
        }

        #endregion

        #region Methods

        //My account / Orders
        public virtual async Task<IActionResult> CustomerOrders()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var model = await _mediator.Send(new GetCustomerOrderList() {
                Customer = _workContext.CurrentCustomer,
                Language = _workContext.WorkingLanguage,
                Store = _storeContext.CurrentStore
            });
            return View(model);
        }

        //My account / Orders / Cancel recurring order
        [HttpPost, ActionName("CustomerOrders")]
        [AutoValidateAntiforgeryToken]
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

                var model = await _mediator.Send(new GetCustomerOrderList() {
                    Customer = _workContext.CurrentCustomer,
                    Language = _workContext.WorkingLanguage,
                    Store = _storeContext.CurrentStore
                });
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

            var model = await _mediator.Send(new GetCustomerRewardPoints() {
                Customer = _workContext.CurrentCustomer,
                Store = _storeContext.CurrentStore,
                Currency = _workContext.WorkingCurrency
            });
            return View(model);
        }

        //My account / Order details page
        public virtual async Task<IActionResult> Details(string orderId)
        {
            var order = await _orderService.GetOrderById(orderId);
            if (!order.Access(_workContext.CurrentCustomer))
                return Challenge();

            var model = await _mediator.Send(new GetOrderDetails() { Order = order, Language = _workContext.WorkingLanguage });

            return View(model);
        }

        //My account / Order details page / Print
        public virtual async Task<IActionResult> PrintOrderDetails(string orderId)
        {
            var order = await _orderService.GetOrderById(orderId);
            if (!order.Access(_workContext.CurrentCustomer))
                return Challenge();

            var model = await _mediator.Send(new GetOrderDetails() { Order = order, Language = _workContext.WorkingLanguage });
            model.PrintMode = true;

            return View("Details", model);
        }

        //My account / Order details page / Cancel Unpaid Order
        public virtual async Task<IActionResult> CancelOrder(string orderId)
        {
            var order = await _orderService.GetOrderById(orderId);
            if (!order.Access(_workContext.CurrentCustomer) || order.PaymentStatus != Domain.Payments.PaymentStatus.Pending
                || (order.ShippingStatus != ShippingStatus.ShippingNotRequired && order.ShippingStatus != ShippingStatus.NotYetShipped)
                || order.OrderStatus != OrderStatus.Pending
                || !_orderSettings.UserCanCancelUnpaidOrder)

                return Challenge();

            await _mediator.Send(new CancelOrderCommand() { Order = order, NotifyCustomer = true, NotifyStoreOwner = true });

            return RedirectToRoute("OrderDetails", new { orderId = orderId });
        }

        //My account / Order details page / PDF invoice
        public virtual async Task<IActionResult> GetPdfInvoice(string orderId, [FromServices] IPdfService pdfService)
        {
            var order = await _orderService.GetOrderById(orderId);
            if (!order.Access(_workContext.CurrentCustomer))
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
            if (!order.Access(_workContext.CurrentCustomer))
                return Challenge();

            var model = new AddOrderNoteModel();
            model.OrderId = orderId;
            return View("AddOrderNote", model);
        }

        //My account / Order details page / Add order note
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public virtual async Task<IActionResult> AddOrderNote(AddOrderNoteModel model)
        {
            if (!_orderSettings.AllowCustomerToAddOrderNote)
                return RedirectToRoute("HomePage");

            if (!ModelState.IsValid)
            {
                return View("AddOrderNote", model);
            }

            var order = await _orderService.GetOrderById(model.OrderId);
            if (!order.Access(_workContext.CurrentCustomer))
                return Challenge();

            await _mediator.Send(new InsertOrderNoteCommand() { Order = order, OrderNote = model, Language = _workContext.WorkingLanguage });

            //notification
            await _mediator.Publish(new OrderNoteEvent(order, model));

            AddNotification(Framework.UI.NotifyType.Success, _localizationService.GetResource("OrderNote.Added"), true);
            return RedirectToRoute("OrderDetails", new { orderId = model.OrderId });
        }

        //My account / Order details page / re-order
        public virtual async Task<IActionResult> ReOrder(string orderId)
        {
            var order = await _orderService.GetOrderById(orderId);
            if (!order.Access(_workContext.CurrentCustomer))
                return Challenge();

            var warnings = await _mediator.Send(new ReOrderCommand() { Order = order });
            if(warnings.Any())
                AddNotification(Framework.UI.NotifyType.Error, string.Join(",", warnings), true);

            return RedirectToRoute("ShoppingCart");
        }

        //My account / Order details page / Complete payment
        [HttpPost, ActionName("Details")]
        [FormValueRequired("repost-payment")]
        [AutoValidateAntiforgeryToken]
        public virtual async Task<IActionResult> RePostPayment(string orderId, [FromServices] IWebHelper webHelper)
        {
            var order = await _orderService.GetOrderById(orderId);
            if (!order.Access(_workContext.CurrentCustomer))
                return Challenge();

            if (!await _paymentService.CanRePostProcessPayment(order))
                return RedirectToRoute("OrderDetails", new { orderId = orderId });

            var postProcessPaymentRequest = new PostProcessPaymentRequest {
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
            if (!order.Access(_workContext.CurrentCustomer))
                return Challenge();

            var model = await _mediator.Send(new GetShipmentDetails() {
                Customer = _workContext.CurrentCustomer,
                Language = _workContext.WorkingLanguage,
                Order = order,
                Shipment = shipment
            });

            return View(model);
        }

        #endregion
    }
}
