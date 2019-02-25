using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Orders;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Security.Authorization;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Directory;
using Grand.Services.ExportImport;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Orders;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Orders;
using Grand.Web.Areas.Admin.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Orders)]
    public partial class OrderController : BaseAdminController
    {
        #region Fields
        private readonly IOrderViewModelService _orderViewModelService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IPdfService _pdfService;
        private readonly IExportManager _exportManager;        
        #endregion

        #region Ctor

        public OrderController(
            IOrderViewModelService orderViewModelService,
            IOrderService orderService, 
            IOrderProcessingService orderProcessingService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            ICurrencyService currencyService,
            IPdfService pdfService,
            IExportManager exportManager)
        {
            this._orderViewModelService = orderViewModelService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._pdfService = pdfService;
            this._exportManager = exportManager;
        }
        
        #endregion
       
        #region Order list

        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List(int? orderStatusId = null,
            int? paymentStatusId = null, int? shippingStatusId = null, DateTime? startDate = null)
        {
            var model = _orderViewModelService.PrepareOrderListModel(orderStatusId, paymentStatusId, shippingStatusId, startDate);
            return View(model);
		}

		[HttpPost]
		public IActionResult OrderList(DataSourceRequest command, OrderListModel model)
        {
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.VendorId = _workContext.CurrentVendor.Id;
            }
            var orders = _orderViewModelService.PrepareOrderModel(model, command.Page, command.PageSize);
            
            var gridModel = new DataSourceResult
            {
                Data = orders.orderModels.ToList(),
                ExtraData = orders.aggreratorModel,
                Total = orders.totalCount
            };
            return Json(gridModel);
        }
        
        [HttpPost, ActionName("List")]
        [FormValueRequired("go-to-order-by-number")]
        public IActionResult GoToOrderId(OrderListModel model)
        {
            var order = _orderService.GetOrderByNumber(model.GoDirectlyToNumber);
            if (order == null)
                return RedirectToAction("List", "Order");

            return RedirectToAction("Edit", "Order", new { id = order.Id });
        }

        #endregion

        #region Export / Import

        [HttpPost, ActionName("List")]
        [FormValueRequired("exportxml-all")]
        public IActionResult ExportXmlAll(OrderListModel model)
        {
            //a vendor cannot export orders
            if (_workContext.CurrentVendor != null)
                return AccessDeniedView();

            var orders = _orderViewModelService.PrepareOrders(model);
            try
            {
                var xml = _exportManager.ExportOrdersToXml(orders);
                return File(Encoding.UTF8.GetBytes(xml), "application/xml", "orders.xml");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public IActionResult ExportXmlSelected(string selectedIds)
        {
            //a vendor cannot export orders
            if (_workContext.CurrentVendor != null)
                return AccessDeniedView();

            var orders = new List<Order>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x)
                    .ToArray();
                orders.AddRange(_orderService.GetOrdersByIds(ids));
            }

            var xml = _exportManager.ExportOrdersToXml(orders);
            return File(Encoding.UTF8.GetBytes(xml), "application/xml", "orders.xml");
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("exportexcel-all")]
        public IActionResult ExportExcelAll(OrderListModel model)
        {
            //a vendor cannot export orders
            if (_workContext.CurrentVendor != null)
                return AccessDeniedView();

            //load orders
            var orders = _orderViewModelService.PrepareOrders(model);
            try
            {
                byte[] bytes = _exportManager.ExportOrdersToXlsx(orders);
                return File(bytes, "text/xls", "orders.xlsx");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public IActionResult ExportExcelSelected(string selectedIds)
        {
            //a vendor cannot export orders
            if (_workContext.CurrentVendor != null)
                return AccessDeniedView();

            var orders = new List<Order>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x)
                    .ToArray();
                orders.AddRange(_orderService.GetOrdersByIds(ids));
            }

            byte[] bytes = _exportManager.ExportOrdersToXlsx(orders);
            return File(bytes, "text/xls", "orders.xlsx");
        }

        #endregion

        #region Order details

        #region Payments and other order workflow

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("cancelorder")]
        public IActionResult CancelOrder(string id)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = id });

            try
            {
                _orderProcessingService.CancelOrder(order, true);
                _orderViewModelService.LogEditOrder(order.Id);
                var model = new OrderModel();
                _orderViewModelService.PrepareOrderDetailsModel(model, order);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new OrderModel();
                _orderViewModelService.PrepareOrderDetailsModel(model, order);
                ErrorNotification(exc, false);
                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("captureorder")]
        public IActionResult CaptureOrder(string id)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = id });

            try
            {
                var errors = _orderProcessingService.Capture(order);
                _orderViewModelService.LogEditOrder(order.Id);
                var model = new OrderModel();
                _orderViewModelService.PrepareOrderDetailsModel(model, order);
                foreach (var error in errors)
                    ErrorNotification(error, false);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new OrderModel();
                _orderViewModelService.PrepareOrderDetailsModel(model, order);
                ErrorNotification(exc, false);
                return View(model);
            }

        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("markorderaspaid")]
        public IActionResult MarkOrderAsPaid(string id)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = id });

            try
            {
                _orderProcessingService.MarkOrderAsPaid(order);
                _orderViewModelService.LogEditOrder(order.Id);
                var model = new OrderModel();
                _orderViewModelService.PrepareOrderDetailsModel(model, order);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new OrderModel();
                _orderViewModelService.PrepareOrderDetailsModel(model, order);
                ErrorNotification(exc, false);
                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("refundorder")]
        public IActionResult RefundOrder(string id)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = id });

            try
            {
                var errors = _orderProcessingService.Refund(order);
                _orderViewModelService.LogEditOrder(order.Id);
                var model = new OrderModel();
                _orderViewModelService.PrepareOrderDetailsModel(model, order);
                foreach (var error in errors)
                    ErrorNotification(error, false);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new OrderModel();
                _orderViewModelService.PrepareOrderDetailsModel(model, order);
                ErrorNotification(exc, false);
                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("refundorderoffline")]
        public IActionResult RefundOrderOffline(string id)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = id });

            try
            {
                _orderProcessingService.RefundOffline(order);
                _orderViewModelService.LogEditOrder(order.Id);
                var model = new OrderModel();
                _orderViewModelService.PrepareOrderDetailsModel(model, order);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new OrderModel();
                _orderViewModelService.PrepareOrderDetailsModel(model, order);
                ErrorNotification(exc, false);
                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("voidorder")]
        public IActionResult VoidOrder(string id)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = id });

            try
            {
                var errors = _orderProcessingService.Void(order);
                _orderViewModelService.LogEditOrder(order.Id);
                var model = new OrderModel();
                _orderViewModelService.PrepareOrderDetailsModel(model, order);
                foreach (var error in errors)
                    ErrorNotification(error, false);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new OrderModel();
                _orderViewModelService.PrepareOrderDetailsModel(model, order);
                ErrorNotification(exc, false);
                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("voidorderoffline")]
        public IActionResult VoidOrderOffline(string id)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = id });

            try
            {
                _orderProcessingService.VoidOffline(order);
                _orderViewModelService.LogEditOrder(order.Id);
                var model = new OrderModel();
                _orderViewModelService.PrepareOrderDetailsModel(model, order);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new OrderModel();
                _orderViewModelService.PrepareOrderDetailsModel(model, order);
                ErrorNotification(exc, false);
                return View(model);
            }
        }
        
        public IActionResult PartiallyRefundOrderPopup(string id, bool online)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = id });

            var model = new OrderModel();
            _orderViewModelService.PrepareOrderDetailsModel(model, order);

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("partialrefundorder")]
        public IActionResult PartiallyRefundOrderPopup(string id, bool online, OrderModel model)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = id });

            try
            {
                decimal amountToRefund = model.AmountToRefund;
                if (amountToRefund <= decimal.Zero)
                    throw new GrandException("Enter amount to refund");

                decimal maxAmountToRefund = order.OrderTotal - order.RefundedAmount;
                if (amountToRefund > maxAmountToRefund)
                    amountToRefund = maxAmountToRefund;

                var errors = new List<string>();
                if (online)
                    errors = _orderProcessingService.PartiallyRefund(order, amountToRefund).ToList();
                else
                    _orderProcessingService.PartiallyRefundOffline(order, amountToRefund);

                _orderViewModelService.LogEditOrder(order.Id);
                if (errors.Count == 0)
                {
                    //success
                    ViewBag.RefreshPage = true;
                    _orderViewModelService.PrepareOrderDetailsModel(model, order);
                    return View(model);
                }
                //error
                _orderViewModelService.PrepareOrderDetailsModel(model, order);
                foreach (var error in errors)
                    ErrorNotification(error, false);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                _orderViewModelService.PrepareOrderDetailsModel(model, order);
                ErrorNotification(exc, false);
                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("btnSaveOrderStatus")]
        public IActionResult ChangeOrderStatus(string id, OrderModel model)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = id });

            try
            {
                order.OrderStatusId = model.OrderStatusId;
                _orderService.UpdateOrder(order);

                //add a note
                _orderService.InsertOrderNote(new OrderNote
                {
                    Note = string.Format("Order status has been edited. New status: {0}", order.OrderStatus.GetLocalizedEnum(_localizationService, _workContext)),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    OrderId = order.Id,

                });
                _orderViewModelService.LogEditOrder(order.Id);
                model = new OrderModel();
                _orderViewModelService.PrepareOrderDetailsModel(model, order);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                model = new OrderModel();
                _orderViewModelService.PrepareOrderDetailsModel(model, order);
                ErrorNotification(exc, false);
                return View(model);
            }
        }

        #endregion

        #region Edit, delete

        public IActionResult Edit(string id)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null || order.Deleted)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToOrder(order))
                return RedirectToAction("List");

            var model = new OrderModel();
            _orderViewModelService.PrepareOrderDetailsModel(model, order);

            return View(model);
        }

        [HttpPost]
        public IActionResult Delete(string id, [FromServices] ICustomerActivityService customerActivityService)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = id });
            if (ModelState.IsValid)
            {
                _orderProcessingService.DeleteOrder(order);
                customerActivityService.InsertActivity("DeleteOrder", id, _localizationService.GetResource("ActivityLog.DeleteOrder"), order.Id);
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("Edit", "Order", new { id = id });
        }

        public IActionResult PdfInvoice(string orderId)
        {
            var vendorId = String.Empty;

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
            {
                vendorId = _workContext.CurrentVendor.Id;
            }

            var order = _orderService.GetOrderById(orderId);
            var orders = new List<Order>();
            orders.Add(order);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                _pdfService.PrintOrdersToPdf(stream, orders, _workContext.WorkingLanguage.Id, vendorId);
                bytes = stream.ToArray();
            }
            return File(bytes, "application/pdf", string.Format("order_{0}.pdf", order.Id));
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("pdf-invoice-all")]
        public IActionResult PdfInvoiceAll(OrderListModel model)
        {
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.VendorId = _workContext.CurrentVendor.Id;
            }
            //load orders
            var orders = _orderViewModelService.PrepareOrders(model);

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                _pdfService.PrintOrdersToPdf(stream, orders, _workContext.WorkingLanguage.Id, model.VendorId);
                bytes = stream.ToArray();
            }
            return File(bytes, "application/pdf", "orders.pdf");
        }

        [HttpPost]
        public IActionResult PdfInvoiceSelected(string selectedIds)
        {
            var orders = new List<Order>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x)
                    .ToArray();
                orders.AddRange(_orderService.GetOrdersByIds(ids));
            }
            var vendorId = String.Empty;
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                orders = orders.Where(_workContext.HasAccessToOrder).ToList();
                vendorId = _workContext.CurrentVendor.Id;
            }

            //ensure that we at least one order selected
            if (orders.Count == 0)
            {
                ErrorNotification(_localizationService.GetResource("Admin.Orders.PdfInvoice.NoOrders"));
                return RedirectToAction("List");
            }

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                _pdfService.PrintOrdersToPdf(stream, orders, _workContext.WorkingLanguage.Id, vendorId);
                bytes = stream.ToArray();
            }
            return File(bytes, "application/pdf", "orders.pdf");
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("btnSaveCC")]
        public IActionResult EditCreditCardInfo(string id, OrderModel model)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = id });

            if (order.AllowStoringCreditCardNumber)
            {
                _orderViewModelService.EditCreditCardInfo(order, model);
            }

            //add a note
            _orderService.InsertOrderNote(new OrderNote
            {
                Note = "Credit card info has been edited",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
                OrderId = order.Id,
            });
            _orderViewModelService.LogEditOrder(order.Id);

            _orderViewModelService.PrepareOrderDetailsModel(model, order);
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("btnSaveOrderTotals")]
        public IActionResult EditOrderTotals(string id, OrderModel model)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = id });

            order.OrderSubtotalInclTax = model.OrderSubtotalInclTaxValue;
            order.OrderSubtotalExclTax = model.OrderSubtotalExclTaxValue;
            order.OrderSubTotalDiscountInclTax = model.OrderSubTotalDiscountInclTaxValue;
            order.OrderSubTotalDiscountExclTax = model.OrderSubTotalDiscountExclTaxValue;
            order.OrderShippingInclTax = model.OrderShippingInclTaxValue;
            order.OrderShippingExclTax = model.OrderShippingExclTaxValue;
            order.PaymentMethodAdditionalFeeInclTax = model.PaymentMethodAdditionalFeeInclTaxValue;
            order.PaymentMethodAdditionalFeeExclTax = model.PaymentMethodAdditionalFeeExclTaxValue;
            order.TaxRates = model.TaxRatesValue;
            order.OrderTax = model.TaxValue;
            order.OrderDiscount = model.OrderTotalDiscountValue;
            order.OrderTotal = model.OrderTotalValue;
            _orderService.UpdateOrder(order);

            //add a note
            _orderService.InsertOrderNote(new OrderNote
            {
                Note = "Order totals have been edited",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
                OrderId = order.Id,
            });

            _orderViewModelService.LogEditOrder(order.Id);
            _orderViewModelService.PrepareOrderDetailsModel(model, order);
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("save-shipping-method")]
        public IActionResult EditShippingMethod(string id, OrderModel model)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = id });

            order.ShippingMethod = model.ShippingMethod;
            _orderService.UpdateOrder(order);

            //add a note
            _orderService.InsertOrderNote(new OrderNote
            {
                Note = "Shipping method has been edited",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
                OrderId = order.Id,
            });
            _orderViewModelService.LogEditOrder(order.Id);
            _orderViewModelService.PrepareOrderDetailsModel(model, order);

            //selected tab
            SaveSelectedTabIndex(persistForTheNextRequest: false);

            return View(model);
        }
        
        [HttpPost, ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnSaveOrderItem")]
        
        public IActionResult EditOrderItem(string id, IFormCollection form, [FromServices] IProductService productService)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = id });

            //get order item identifier
            string orderItemId = "";
            foreach (var formValue in form.Keys)
                if (formValue.StartsWith("btnSaveOrderItem", StringComparison.OrdinalIgnoreCase))
                    orderItemId = formValue.Substring("btnSaveOrderItem".Length);

            var orderItem = order.OrderItems.FirstOrDefault(x => x.Id == orderItemId);
            if (orderItem == null)
                throw new ArgumentException("No order item found with the specified id");

            var product = productService.GetProductByIdIncludeArch(orderItem.ProductId);

            decimal unitPriceInclTax, unitPriceExclTax, discountInclTax, discountExclTax,priceInclTax,priceExclTax;
            int quantity;
            if (!decimal.TryParse(form["pvUnitPriceInclTax" + orderItemId], out unitPriceInclTax))
                unitPriceInclTax = orderItem.UnitPriceInclTax;
            if (!decimal.TryParse(form["pvUnitPriceExclTax" + orderItemId], out unitPriceExclTax))
                unitPriceExclTax = orderItem.UnitPriceExclTax;
            if (!int.TryParse(form["pvQuantity" + orderItemId], out quantity))
                quantity = orderItem.Quantity;
            if (!decimal.TryParse(form["pvDiscountInclTax" + orderItemId], out discountInclTax))
                discountInclTax = orderItem.DiscountAmountInclTax;
            if (!decimal.TryParse(form["pvDiscountExclTax" + orderItemId], out discountExclTax))
                discountExclTax = orderItem.DiscountAmountExclTax;
            if (!decimal.TryParse(form["pvPriceInclTax" + orderItemId], out priceInclTax))
                priceInclTax = orderItem.PriceInclTax;
            if (!decimal.TryParse(form["pvPriceExclTax" + orderItemId], out priceExclTax))
                priceExclTax = orderItem.PriceExclTax;

            if (quantity > 0)
            {
                int qtyDifference = orderItem.Quantity - quantity;

                orderItem.UnitPriceInclTax = unitPriceInclTax;
                orderItem.UnitPriceExclTax = unitPriceExclTax;
                orderItem.Quantity = quantity;
                orderItem.DiscountAmountInclTax = discountInclTax;
                orderItem.DiscountAmountExclTax = discountExclTax;
                orderItem.PriceInclTax = priceInclTax;
                orderItem.PriceExclTax = priceExclTax;
                _orderService.UpdateOrder(order);
                //adjust inventory
                productService.AdjustInventory(product, qtyDifference, orderItem.AttributesXml, orderItem.WarehouseId);

            }
            else
            {
                //adjust inventory
                productService.AdjustInventory(product, orderItem.Quantity, orderItem.AttributesXml, orderItem.WarehouseId);
                _orderService.DeleteOrderItem(orderItem);
            }

            order = _orderService.GetOrderById(id);
            //add a note
            _orderService.InsertOrderNote(new OrderNote
            {
                Note = "Order item has been edited",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
                OrderId = order.Id,
            });

            _orderViewModelService.LogEditOrder(order.Id);
            var model = new OrderModel();
            _orderViewModelService.PrepareOrderDetailsModel(model, order);

            //selected tab
            SaveSelectedTabIndex(persistForTheNextRequest: false);

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnDeleteOrderItem")]
        public IActionResult DeleteOrderItem(string id, IFormCollection form, 
            [FromServices] IGiftCardService giftCardService,
            [FromServices] IProductService productService
            )
        {
            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = id });

            //get order item identifier
            string orderItemId = "";
            foreach (var formValue in form.Keys)
                if (formValue.StartsWith("btnDeleteOrderItem", StringComparison.OrdinalIgnoreCase))
                    orderItemId = formValue.Substring("btnDeleteOrderItem".Length);

            var orderItem = order.OrderItems.FirstOrDefault(x => x.Id == orderItemId);
            if (orderItem == null)
                throw new ArgumentException("No order item found with the specified id");
            var product = productService.GetProductById(orderItem.ProductId);
            if (giftCardService.GetGiftCardsByPurchasedWithOrderItemId(orderItem.Id).Count > 0)
            {
                //we cannot delete an order item with associated gift cards
                //a store owner should delete them first

                var model = new OrderModel();
                _orderViewModelService.PrepareOrderDetailsModel(model, order);

                ErrorNotification("This order item has an associated gift card record. Please delete it first.", false);

                //selected tab
                SaveSelectedTabIndex(persistForTheNextRequest: false);

                return View(model);

            }
            else
            {
                //add a note
                _orderService.InsertOrderNote(new OrderNote
                {
                    Note = "Order item has been deleted",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    OrderId = order.Id,
                });

                //adjust inventory
                if(product!=null)
                    productService.AdjustInventory(product, orderItem.Quantity, orderItem.AttributesXml, orderItem.WarehouseId);

                _orderService.DeleteOrderItem(orderItem);
                order = _orderService.GetOrderById(id);
                _orderViewModelService.LogEditOrder(order.Id);
                var model = new OrderModel();
                _orderViewModelService.PrepareOrderDetailsModel(model, order);

                //selected tab
                SaveSelectedTabIndex(persistForTheNextRequest: false);

                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnResetDownloadCount")]
        public IActionResult ResetDownloadCount(string id, IFormCollection form)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //get order item identifier
            string orderItemId = "";
            foreach (var formValue in form.Keys)
                if (formValue.StartsWith("btnResetDownloadCount", StringComparison.OrdinalIgnoreCase))
                    orderItemId = formValue.Substring("btnResetDownloadCount".Length);

            var orderItem = order.OrderItems.FirstOrDefault(x => x.Id == orderItemId);
            if (orderItem == null)
                throw new ArgumentException("No order item found with the specified id");

            //ensure a vendor has access only to his products 
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToOrderItem(orderItem))
                return RedirectToAction("List");

            orderItem.DownloadCount = 0;
            _orderService.UpdateOrder(order);
            _orderViewModelService.LogEditOrder(order.Id);
            var model = new OrderModel();
            _orderViewModelService.PrepareOrderDetailsModel(model, order);

            //selected tab
            SaveSelectedTabIndex(persistForTheNextRequest: false);

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnPvActivateDownload")]
        
        public IActionResult ActivateDownloadItem(string id, IFormCollection form)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //get order item identifier
            string orderItemId = "";
            foreach (var formValue in form.Keys)
                if (formValue.StartsWith("btnPvActivateDownload", StringComparison.OrdinalIgnoreCase))
                    orderItemId = formValue.Substring("btnPvActivateDownload".Length);

            var orderItem = order.OrderItems.FirstOrDefault(x => x.Id == orderItemId);
            if (orderItem == null)
                throw new ArgumentException("No order item found with the specified id");

            //ensure a vendor has access only to his products 
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToOrderItem(orderItem))
                return RedirectToAction("List");

            orderItem.IsDownloadActivated = !orderItem.IsDownloadActivated;
            _orderService.UpdateOrder(order);
            _orderViewModelService.LogEditOrder(order.Id);
            var model = new OrderModel();
            _orderViewModelService.PrepareOrderDetailsModel(model, order);

            //selected tab
            SaveSelectedTabIndex(persistForTheNextRequest: false);

            return View(model);
        }

        public IActionResult UploadLicenseFilePopup(string id, string orderItemId, [FromServices] IProductService productService)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            var orderItem = order.OrderItems.FirstOrDefault(x => x.Id == orderItemId);
            if (orderItem == null)
                throw new ArgumentException("No order item found with the specified id");

            var product = productService.GetProductByIdIncludeArch(orderItem.ProductId);

            if (!product.IsDownload)
                throw new ArgumentException("Product is not downloadable");

            //ensure a vendor has access only to his products 
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToOrderItem(orderItem))
                return RedirectToAction("List");

            var model = new OrderModel.UploadLicenseModel
            {
                LicenseDownloadId = !String.IsNullOrEmpty(orderItem.LicenseDownloadId) ? orderItem.LicenseDownloadId : "",
                OrderId = order.Id,
                OrderItemId = orderItem.Id
            };

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("uploadlicense")]
        public IActionResult UploadLicenseFilePopup(OrderModel.UploadLicenseModel model)
        {
            var order = _orderService.GetOrderById(model.OrderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            var orderItem = order.OrderItems.FirstOrDefault(x => x.Id == model.OrderItemId);
            if (orderItem == null)
                throw new ArgumentException("No order item found with the specified id");

            //ensure a vendor has access only to his products 
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToOrderItem(orderItem))
                return RedirectToAction("List");

            //attach license
            if (!String.IsNullOrEmpty(model.LicenseDownloadId))
                orderItem.LicenseDownloadId = model.LicenseDownloadId;
            else
                orderItem.LicenseDownloadId = null;
            _orderService.UpdateOrder(order);

            _orderViewModelService.LogEditOrder(order.Id);
            //success
            ViewBag.RefreshPage = true;

            return View(model);
        }

        [HttpPost, ActionName("UploadLicenseFilePopup")]
        [FormValueRequired("deletelicense")]
        public IActionResult DeleteLicenseFilePopup(OrderModel.UploadLicenseModel model)
        {
            var order = _orderService.GetOrderById(model.OrderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            var orderItem = order.OrderItems.FirstOrDefault(x => x.Id == model.OrderItemId);
            if (orderItem == null)
                throw new ArgumentException("No order item found with the specified id");

            //ensure a vendor has access only to his products 
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToOrderItem(orderItem))
                return RedirectToAction("List");

            //attach license
            orderItem.LicenseDownloadId = null;
            _orderService.UpdateOrder(order);
            _orderViewModelService.LogEditOrder(order.Id);

            //success
            ViewBag.RefreshPage = true;

            return View(model);
        }

        public IActionResult AddProductToOrder(string orderId)
        {
            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = orderId });

            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            var model = _orderViewModelService.PrepareAddOrderProductModel(order);
            return View(model);
        }

        [HttpPost]
        public IActionResult AddProductToOrder(DataSourceRequest command, OrderModel.AddOrderProductModel model, [FromServices] IProductService productService)
        {
            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return Content("");
            var categoryIds = new List<string>();
            if (!String.IsNullOrEmpty(model.SearchCategoryId))
                categoryIds.Add(model.SearchCategoryId);

            var gridModel = new DataSourceResult();
            var products = productService.SearchProducts(categoryIds: categoryIds,
                manufacturerId: model.SearchManufacturerId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName, 
                pageIndex: command.Page - 1, 
                pageSize: command.PageSize,
                showHidden: true);
            gridModel.Data = products.Select(x =>
            {
                var productModel = new OrderModel.AddOrderProductModel.ProductModel
                {
                    Id = x.Id,
                    Name =  x.Name,
                    Sku = x.Sku,
                };

                return productModel;
            });
            gridModel.Total = products.TotalCount;

            return Json(gridModel);
        }

        public IActionResult AddProductToOrderDetails(string orderId, string productId)
        {
            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = orderId });

            var model = _orderViewModelService.PrepareAddProductToOrderModel(orderId, productId);
            return View(model);
        }

        [HttpPost]
        public IActionResult AddProductToOrderDetails(string orderId, string productId, IFormCollection form)
        {
            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = orderId });

            var warnings = _orderViewModelService.AddProductToOrderDetails(orderId, productId, form);
            if(!warnings.Any())
                //redirect to order details page
                return RedirectToAction("Edit", "Order", new { id = orderId });

            //errors
            var model = _orderViewModelService.PrepareAddProductToOrderModel(orderId, productId);
            model.Warnings.AddRange(warnings);
            return View(model);
        }

        #endregion

        #endregion

        #region Addresses
        public IActionResult AddressEdit(string addressId, string orderId)
        {
            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = orderId });
            Address address = new Address();
            if (order.BillingAddress != null)
                if (order.BillingAddress.Id == addressId)
                    address = order.BillingAddress;
            if (order.ShippingAddress != null)
                if (order.ShippingAddress.Id == addressId)
                    address = order.ShippingAddress;

            if (address == null)
                throw new ArgumentException("No address found with the specified id", "addressId");

            var model = _orderViewModelService.PrepareOrderAddressModel(order, address);
            return View(model);
        }

        [HttpPost]
        public IActionResult AddressEdit(OrderAddressModel model, IFormCollection form, 
            [FromServices] IAddressAttributeService addressAttributeService,
            [FromServices] IAddressAttributeParser addressAttributeParser)
        {
            var order = _orderService.GetOrderById(model.OrderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = order.Id });

            Address address = new Address();
            if (order.BillingAddress != null)
                if (order.BillingAddress.Id == model.Address.Id)
                    address = order.BillingAddress;
            if (order.ShippingAddress != null)
                if (order.ShippingAddress.Id == model.Address.Id)
                    address = order.ShippingAddress;

            if (address == null)
                throw new ArgumentException("No address found with the specified id");

            //custom address attributes
            var customAttributes = form.ParseCustomAddressAttributes(addressAttributeParser, addressAttributeService);
            var customAttributeWarnings = addressAttributeParser.GetAttributeWarnings(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            if (ModelState.IsValid)
            {
                address = _orderViewModelService.UpdateOrderAddress(order, address, model, customAttributes);
                return RedirectToAction("AddressEdit", new { addressId = model.Address.Id, orderId = model.OrderId });
            }

            //If we got this far, something failed, redisplay form
            model = _orderViewModelService.PrepareOrderAddressModel(order, address);

            return View(model);
        }

        #endregion

        #region Order notes
        
        [HttpPost]
        public IActionResult OrderNotesSelect(string orderId, DataSourceRequest command)
        {
            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                throw new ArgumentException("No order found with the specified id");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return Content("");

            //order notes
            var orderNoteModels = _orderViewModelService.PrepareOrderNotes(order);
            var gridModel = new DataSourceResult
            {
                Data = orderNoteModels,
                Total = orderNoteModels.Count
            };
            return Json(gridModel);
        }
        
        public IActionResult OrderNoteAdd(string orderId, string downloadId, bool displayToCustomer, string message)
        {
            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                return Json(new { Result = false });

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return Json(new { Result = false });

            _orderViewModelService.InsertOrderNote(order, downloadId, displayToCustomer, message);

            return Json(new { Result = true });
        }

        [HttpPost]
        public IActionResult OrderNoteDelete(string id, string orderId)
        {
            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                throw new ArgumentException("No order found with the specified id");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = orderId });

            _orderViewModelService.DeleteOrderNote(order, id);

            return new NullJsonResult();
        }
        #endregion
    }
}
