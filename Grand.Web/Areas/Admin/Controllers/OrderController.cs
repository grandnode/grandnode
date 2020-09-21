using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Orders;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Security.Authorization;
using Grand.Services.Catalog;
using Grand.Services.Commands.Models.Orders;
using Grand.Services.Common;
using Grand.Services.ExportImport;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Orders;
using Grand.Services.Security;
using Grand.Services.Shipping;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Orders;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private readonly IMediator _mediator;

        #endregion

        #region Ctor

        public OrderController(
            IOrderViewModelService orderViewModelService,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IPdfService pdfService,
            IExportManager exportManager,
            IMediator mediator)
        {
            _orderViewModelService = orderViewModelService;
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _localizationService = localizationService;
            _workContext = workContext;
            _pdfService = pdfService;
            _exportManager = exportManager;
            _mediator = mediator;
        }

        #endregion

        #region Order list

        public IActionResult Index() => RedirectToAction("List");

        public async Task<IActionResult> List(int? orderStatusId = null,
            int? paymentStatusId = null, int? shippingStatusId = null, DateTime? startDate = null, string code = null)
        {
            var model = await _orderViewModelService.PrepareOrderListModel(orderStatusId, paymentStatusId, shippingStatusId, startDate, _workContext.CurrentCustomer.StaffStoreId, code);
            return View(model);
        }

        public async Task<IActionResult> ProductSearchAutoComplete(string term, [FromServices] IProductService productService)
        {
            const int searchTermMinimumLength = 3;
            if (string.IsNullOrWhiteSpace(term) || term.Length < searchTermMinimumLength)
                return Content("");

            var storeId = string.Empty;
            if (_workContext.CurrentCustomer.IsStaff())
                storeId = _workContext.CurrentCustomer.StaffStoreId;

            string vendorId = string.Empty;
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                vendorId = _workContext.CurrentVendor.Id;
            }
            //products
            const int productNumber = 15;
            var products = (await productService.SearchProducts(
                storeId: storeId,
                vendorId: vendorId,
                keywords: term,
                pageSize: productNumber,
                showHidden: true)).products;

            var result = (from p in products
                          select new
                          {
                              label = p.Name,
                              productid = p.Id
                          })
                .ToList();
            return Json(result);
        }

        [PermissionAuthorizeAction(PermissionActionName.List)]
        [HttpPost]
        public async Task<IActionResult> OrderList(DataSourceRequest command, OrderListModel model)
        {
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
            {
                model.VendorId = _workContext.CurrentVendor.Id;
            }

            if (_workContext.CurrentCustomer.IsStaff())
            {
                model.StoreId = _workContext.CurrentCustomer.StaffStoreId;
            }

            var (orderModels, aggreratorModel, totalCount) = await _orderViewModelService.PrepareOrderModel(model, command.Page, command.PageSize);

            var gridModel = new DataSourceResult {
                Data = orderModels.ToList(),
                ExtraData = aggreratorModel,
                Total = totalCount
            };
            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost, ActionName("List")]
        [FormValueRequired("go-to-order-by-number")]
        public async Task<IActionResult> GoToOrderId(OrderListModel model)
        {
            Order order = null;
            int.TryParse(model.GoDirectlyToNumber, out var orderNumber);
            if (orderNumber > 0)
            {
                order = await _orderService.GetOrderByNumber(orderNumber);
            }
            var orders = await _orderService.GetOrdersByCode(model.GoDirectlyToNumber);
            if (orders.Count > 1)
            {
                return RedirectToAction("List", new { Code = model.GoDirectlyToNumber });
            }
            if (orders.Count == 1)
            {
                order = orders.FirstOrDefault();
            }
            if (order == null)
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff() && order.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            return RedirectToAction("Edit", "Order", new { id = order.Id });
        }

        #endregion

        #region Export / Import

        [PermissionAuthorizeAction(PermissionActionName.Export)]
        [HttpPost, ActionName("List")]
        [FormValueRequired("exportxml-all")]
        public async Task<IActionResult> ExportXmlAll(OrderListModel model)
        {
            //a vendor cannot export orders
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return AccessDeniedView();

            if (_workContext.CurrentCustomer.IsStaff())
            {
                model.StoreId = _workContext.CurrentCustomer.StaffStoreId;
            }

            var orders = await _orderViewModelService.PrepareOrders(model);
            try
            {
                var xml = await _exportManager.ExportOrdersToXml(orders);
                return File(Encoding.UTF8.GetBytes(xml), "application/xml", "orders.xml");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [PermissionAuthorizeAction(PermissionActionName.Export)]
        [HttpPost]
        public async Task<IActionResult> ExportXmlSelected(string selectedIds)
        {
            //a vendor cannot export orders
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return AccessDeniedView();

            var orders = new List<Order>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x)
                    .ToArray();
                orders.AddRange(await _orderService.GetOrdersByIds(ids));
            }
            if (_workContext.CurrentCustomer.IsStaff())
            {
                orders = orders.Where(x => x.StoreId == _workContext.CurrentCustomer.StaffStoreId).ToList();
            }
            var xml = await _exportManager.ExportOrdersToXml(orders);
            return File(Encoding.UTF8.GetBytes(xml), "application/xml", "orders.xml");
        }

        [PermissionAuthorizeAction(PermissionActionName.Export)]
        [HttpPost, ActionName("List")]
        [FormValueRequired("exportexcel-all")]
        public async Task<IActionResult> ExportExcelAll(OrderListModel model)
        {
            //a vendor cannot export orders
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return AccessDeniedView();

            if (_workContext.CurrentCustomer.IsStaff())
            {
                model.StoreId = _workContext.CurrentCustomer.StaffStoreId;
            }

            //load orders
            var orders = await _orderViewModelService.PrepareOrders(model);
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

        [PermissionAuthorizeAction(PermissionActionName.Export)]
        [HttpPost]
        public async Task<IActionResult> ExportExcelSelected(string selectedIds)
        {
            //a vendor cannot export orders
            if (_workContext.CurrentVendor != null)
                return AccessDeniedView();

            var orders = new List<Order>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x)
                    .ToArray();
                orders.AddRange(await _orderService.GetOrdersByIds(ids));
            }
            if (_workContext.CurrentCustomer.IsStaff())
            {
                orders = orders.Where(x => x.StoreId == _workContext.CurrentCustomer.StaffStoreId).ToList();
            }
            byte[] bytes = _exportManager.ExportOrdersToXlsx(orders);
            return File(bytes, "text/xls", "orders.xlsx");
        }

        #endregion

        #region Order details

        #region Payments and other order workflow

        [PermissionAuthorizeAction(PermissionActionName.Cancel)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("cancelorder")]
        public async Task<IActionResult> CancelOrder(string id)
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Order", new { id = id });

            if (_workContext.CurrentCustomer.IsStaff() && order.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            try
            {
                await _mediator.Send(new CancelOrderCommand() { Order = order, NotifyCustomer = true });
                await _orderViewModelService.LogEditOrder(order.Id);
                var model = new OrderModel();
                await _orderViewModelService.PrepareOrderDetailsModel(model, order);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new OrderModel();
                await _orderViewModelService.PrepareOrderDetailsModel(model, order);
                ErrorNotification(exc, false);
                return View(model);
            }
        }

        [PermissionAuthorizeAction(PermissionActionName.Payments)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("captureorder")]
        public async Task<IActionResult> CaptureOrder(string id)
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Order", new { id = id });

            if (_workContext.CurrentCustomer.IsStaff() && order.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            try
            {
                var errors = await _orderProcessingService.Capture(order);
                await _orderViewModelService.LogEditOrder(order.Id);
                var model = new OrderModel();
                await _orderViewModelService.PrepareOrderDetailsModel(model, order);
                foreach (var error in errors)
                    ErrorNotification(error, false);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new OrderModel();
                await _orderViewModelService.PrepareOrderDetailsModel(model, order);
                ErrorNotification(exc, false);
                return View(model);
            }

        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("btnSaveOrderTags")]
        public async Task<IActionResult> SaveOrderTags(OrderModel orderModel)
        {
            var order = await _orderService.GetOrderById(orderModel.Id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Order", new { id = order.Id });

            if (_workContext.CurrentCustomer.IsStaff() && order.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            try
            {
                await _orderViewModelService.SaveOrderTags(order, orderModel.OrderTags);
                await _orderViewModelService.LogEditOrder(order.Id);
                var model = new OrderModel();
                await _orderViewModelService.PrepareOrderDetailsModel(model, order);
                return View(model);
            }
            catch (Exception exception)
            {
                //error
                var model = new OrderModel();
                await _orderViewModelService.PrepareOrderDetailsModel(model, order);
                ErrorNotification(exception, false);
                return View(model);
            }
        }

        [PermissionAuthorizeAction(PermissionActionName.Payments)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("markorderaspaid")]
        public async Task<IActionResult> MarkOrderAsPaid(string id)
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Order", new { id = id });

            if (_workContext.CurrentCustomer.IsStaff() && order.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            try
            {
                await _orderProcessingService.MarkOrderAsPaid(order);
                await _orderViewModelService.LogEditOrder(order.Id);
                var model = new OrderModel();
                await _orderViewModelService.PrepareOrderDetailsModel(model, order);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new OrderModel();
                await _orderViewModelService.PrepareOrderDetailsModel(model, order);
                ErrorNotification(exc, false);
                return View(model);
            }
        }

        [PermissionAuthorizeAction(PermissionActionName.Payments)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("refundorder")]
        public async Task<IActionResult> RefundOrder(string id)
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Order", new { id = id });

            if (_workContext.CurrentCustomer.IsStaff() && order.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            try
            {
                var errors = await _orderProcessingService.Refund(order);
                await _orderViewModelService.LogEditOrder(order.Id);
                var model = new OrderModel();
                await _orderViewModelService.PrepareOrderDetailsModel(model, order);
                foreach (var error in errors)
                    ErrorNotification(error, false);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new OrderModel();
                await _orderViewModelService.PrepareOrderDetailsModel(model, order);
                ErrorNotification(exc, false);
                return View(model);
            }
        }

        [PermissionAuthorizeAction(PermissionActionName.Payments)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("refundorderoffline")]
        public async Task<IActionResult> RefundOrderOffline(string id)
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Order", new { id = id });

            if (_workContext.CurrentCustomer.IsStaff() && order.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            try
            {
                await _orderProcessingService.RefundOffline(order);
                await _orderViewModelService.LogEditOrder(order.Id);
                var model = new OrderModel();
                await _orderViewModelService.PrepareOrderDetailsModel(model, order);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new OrderModel();
                await _orderViewModelService.PrepareOrderDetailsModel(model, order);
                ErrorNotification(exc, false);
                return View(model);
            }
        }

        [PermissionAuthorizeAction(PermissionActionName.Payments)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("voidorder")]
        public async Task<IActionResult> VoidOrder(string id)
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Order", new { id = id });

            if (_workContext.CurrentCustomer.IsStaff() && order.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            try
            {
                var errors = await _orderProcessingService.Void(order);
                await _orderViewModelService.LogEditOrder(order.Id);
                var model = new OrderModel();
                await _orderViewModelService.PrepareOrderDetailsModel(model, order);
                foreach (var error in errors)
                    ErrorNotification(error, false);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new OrderModel();
                await _orderViewModelService.PrepareOrderDetailsModel(model, order);
                ErrorNotification(exc, false);
                return View(model);
            }
        }

        [PermissionAuthorizeAction(PermissionActionName.Payments)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("voidorderoffline")]
        public async Task<IActionResult> VoidOrderOffline(string id)
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Order", new { id = id });

            if (_workContext.CurrentCustomer.IsStaff() && order.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            try
            {
                await _orderProcessingService.VoidOffline(order);
                await _orderViewModelService.LogEditOrder(order.Id);
                var model = new OrderModel();
                await _orderViewModelService.PrepareOrderDetailsModel(model, order);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new OrderModel();
                await _orderViewModelService.PrepareOrderDetailsModel(model, order);
                ErrorNotification(exc, false);
                return View(model);
            }
        }

        [PermissionAuthorizeAction(PermissionActionName.Payments)]
        public async Task<IActionResult> PartiallyRefundOrderPopup(string id, bool online)
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Order", new { id = id });

            if (_workContext.CurrentCustomer.IsStaff() && order.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            var model = new OrderModel();
            await _orderViewModelService.PrepareOrderDetailsModel(model, order);

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Payments)]
        [HttpPost]
        [FormValueRequired("partialrefundorder")]
        public async Task<IActionResult> PartiallyRefundOrderPopup(string id, bool online, OrderModel model)
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Order", new { id = id });

            if (_workContext.CurrentCustomer.IsStaff() && order.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

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
                    errors = (await _orderProcessingService.PartiallyRefund(order, amountToRefund)).ToList();
                else
                    await _orderProcessingService.PartiallyRefundOffline(order, amountToRefund);

                await _orderViewModelService.LogEditOrder(order.Id);
                if (errors.Count == 0)
                {
                    //success
                    ViewBag.RefreshPage = true;
                    await _orderViewModelService.PrepareOrderDetailsModel(model, order);
                    return View(model);
                }
                //error
                await _orderViewModelService.PrepareOrderDetailsModel(model, order);
                foreach (var error in errors)
                    ErrorNotification(error, false);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                await _orderViewModelService.PrepareOrderDetailsModel(model, order);
                ErrorNotification(exc, false);
                return View(model);
            }
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("btnSaveOrderStatus")]
        public async Task<IActionResult> ChangeOrderStatus(string id, OrderModel model)
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Order", new { id = id });

            if (_workContext.CurrentCustomer.IsStaff() && order.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            try
            {
                order.OrderStatusId = model.OrderStatusId;
                await _orderService.UpdateOrder(order);

                //add a note
                await _orderService.InsertOrderNote(new OrderNote {
                    Note = string.Format("Order status has been edited. New status: {0}", order.OrderStatus.GetLocalizedEnum(_localizationService, _workContext)),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    OrderId = order.Id,

                });
                await _orderViewModelService.LogEditOrder(order.Id);
                model = new OrderModel();
                await _orderViewModelService.PrepareOrderDetailsModel(model, order);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                model = new OrderModel();
                await _orderViewModelService.PrepareOrderDetailsModel(model, order);
                ErrorNotification(exc, false);
                return View(model);
            }
        }

        #endregion

        #region Edit, delete

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        public async Task<IActionResult> Edit(string id)
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null || order.Deleted)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToOrder(order) && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff() && order.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            var model = new OrderModel();
            await _orderViewModelService.PrepareOrderDetailsModel(model, order);

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        [HttpPost]
        public async Task<IActionResult> Delete(string id, [FromServices] ICustomerActivityService customerActivityService)
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor or staff does not have access to this functionality
            if (_workContext.CurrentVendor != null || _workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Order", new { id = id });

            if (ModelState.IsValid)
            {
                await _mediator.Send(new DeleteOrderCommand() { Order = order });
                await customerActivityService.InsertActivity("DeleteOrder", id, _localizationService.GetResource("ActivityLog.DeleteOrder"), order.Id);
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("Edit", "Order", new { id = id });
        }

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        [HttpPost]
        public async Task<IActionResult> DeleteSelected(ICollection<string> selectedIds, [FromServices] ICustomerActivityService customerActivityService)
        {
            if (_workContext.CurrentVendor != null || _workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("List", "Order");

            if (selectedIds != null)
            {
                var orders = new List<Order>();
                orders.AddRange(await _orderService.GetOrdersByIds(selectedIds.ToArray()));
                for (var i = 0; i < orders.Count; i++)
                {
                    var order = orders[i];
                    await _orderService.DeleteOrder(order);
                    await customerActivityService.InsertActivity("DeleteOrder", order.Id, _localizationService.GetResource("ActivityLog.DeleteOrder"), order.Id);
                }
            }

            return Json(new { Result = true });
        }

        public async Task<IActionResult> PdfInvoice(string orderId)
        {
            var vendorId = string.Empty;

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
            {
                vendorId = _workContext.CurrentVendor.Id;
            }

            var order = await _orderService.GetOrderById(orderId);
            if (_workContext.CurrentCustomer.IsStaff() && order.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            var orders = new List<Order>
            {
                order
            };
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await _pdfService.PrintOrdersToPdf(stream, orders, _workContext.WorkingLanguage.Id, vendorId);
                bytes = stream.ToArray();
            }
            return File(bytes, "application/pdf", string.Format("order_{0}.pdf", order.Id));
        }

        [PermissionAuthorizeAction(PermissionActionName.Export)]
        [HttpPost, ActionName("List")]
        [FormValueRequired("pdf-invoice-all")]
        public async Task<IActionResult> PdfInvoiceAll(OrderListModel model)
        {
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
            {
                model.VendorId = _workContext.CurrentVendor.Id;
            }
            //load orders
            var orders = await _orderViewModelService.PrepareOrders(model);
            if (_workContext.CurrentCustomer.IsStaff())
            {
                orders = orders.Where(x => x.StoreId == _workContext.CurrentCustomer.StaffStoreId).ToList();
            }

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await _pdfService.PrintOrdersToPdf(stream, orders, _workContext.WorkingLanguage.Id, model.VendorId);
                bytes = stream.ToArray();
            }
            return File(bytes, "application/pdf", "orders.pdf");
        }

        [PermissionAuthorizeAction(PermissionActionName.Export)]
        [HttpPost]
        public async Task<IActionResult> PdfInvoiceSelected(string selectedIds)
        {
            var orders = new List<Order>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x)
                    .ToArray();
                orders.AddRange(await _orderService.GetOrdersByIds(ids));
            }
            var vendorId = string.Empty;
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
            {
                orders = orders.Where(_workContext.HasAccessToOrder).ToList();
                vendorId = _workContext.CurrentVendor.Id;
            }
            if (_workContext.CurrentCustomer.IsStaff())
            {
                orders = orders.Where(x => x.StoreId == _workContext.CurrentCustomer.StaffStoreId).ToList();
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
                await _pdfService.PrintOrdersToPdf(stream, orders, _workContext.WorkingLanguage.Id, vendorId);
                bytes = stream.ToArray();
            }
            return File(bytes, "application/pdf", "orders.pdf");
        }

        [PermissionAuthorizeAction(PermissionActionName.Payments)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("btnSaveCC")]
        public async Task<IActionResult> EditCreditCardInfo(string id, OrderModel model)
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Order", new { id = id });

            if (_workContext.CurrentCustomer.IsStaff() && order.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("Edit", "Order", new { id = id });
            }

            if (order.AllowStoringCreditCardNumber)
            {
                await _orderViewModelService.EditCreditCardInfo(order, model);
            }

            //add a note
            await _orderService.InsertOrderNote(new OrderNote {
                Note = "Credit card info has been edited",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
                OrderId = order.Id,
            });
            await _orderViewModelService.LogEditOrder(order.Id);
            await _orderViewModelService.PrepareOrderDetailsModel(model, order);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("btnSaveOrderTotals")]
        public async Task<IActionResult> EditOrderTotals(string id, OrderModel model)
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Order", new { id = id });

            if (_workContext.CurrentCustomer.IsStaff() && order.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("Edit", "Order", new { id = id });
            }

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
            order.CurrencyRate = model.CurrencyRate;
            await _orderService.UpdateOrder(order);

            //add a note
            await _orderService.InsertOrderNote(new OrderNote {
                Note = "Order totals have been edited",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
                OrderId = order.Id,
            });

            await _orderViewModelService.LogEditOrder(order.Id);
            await _orderViewModelService.PrepareOrderDetailsModel(model, order);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("save-shipping-method")]
        public async Task<IActionResult> EditShippingMethod(string id, OrderModel model)
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Order", new { id = id });

            if (_workContext.CurrentCustomer.IsStaff() && order.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("Edit", "Order", new { id = id });
            }

            order.ShippingMethod = model.ShippingMethod;
            await _orderService.UpdateOrder(order);

            //add a note
            await _orderService.InsertOrderNote(new OrderNote {
                Note = "Shipping method has been edited",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
                OrderId = order.Id,
            });
            await _orderViewModelService.LogEditOrder(order.Id);
            await _orderViewModelService.PrepareOrderDetailsModel(model, order);

            //selected tab
            await SaveSelectedTabIndex(persistForTheNextRequest: false);

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("save-generic-attributes")]
        public async Task<IActionResult> EditGenericAttributes(string id, OrderModel model)
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Order", new { id = id });

            if (_workContext.CurrentCustomer.IsStaff() && order.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("Edit", "Order", new { id = id });
            }

            order.GenericAttributes = model.GenericAttributes;

            await _orderService.UpdateOrder(order);
            await _orderViewModelService.LogEditOrder(order.Id);

            await _orderViewModelService.PrepareOrderDetailsModel(model, order);

            //selected tab
            await SaveSelectedTabIndex(persistForTheNextRequest: false);

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnSaveOrderItem")]
        public async Task<IActionResult> EditOrderItem(string id, IFormCollection form, [FromServices] IProductService productService)
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Order", new { id = id });

            if (_workContext.CurrentCustomer.IsStaff() && order.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("Edit", "Order", new { id = id });
            }

            //get order item identifier
            string orderItemId = "";
            foreach (var formValue in form.Keys)
                if (formValue.StartsWith("btnSaveOrderItem", StringComparison.OrdinalIgnoreCase))
                    orderItemId = formValue.Substring("btnSaveOrderItem".Length);

            var orderItem = order.OrderItems.FirstOrDefault(x => x.Id == orderItemId);
            if (orderItem == null)
                throw new ArgumentException("No order item found with the specified id");

            var product = await productService.GetProductByIdIncludeArch(orderItem.ProductId);

            if (!decimal.TryParse(form["pvUnitPriceInclTax" + orderItemId], out decimal unitPriceInclTax))
                unitPriceInclTax = orderItem.UnitPriceInclTax;
            if (!decimal.TryParse(form["pvUnitPriceExclTax" + orderItemId], out decimal unitPriceExclTax))
                unitPriceExclTax = orderItem.UnitPriceExclTax;
            if (!int.TryParse(form["pvQuantity" + orderItemId], out int quantity))
                quantity = orderItem.Quantity;
            if (!decimal.TryParse(form["pvDiscountInclTax" + orderItemId], out decimal discountInclTax))
                discountInclTax = orderItem.DiscountAmountInclTax;
            if (!decimal.TryParse(form["pvDiscountExclTax" + orderItemId], out decimal discountExclTax))
                discountExclTax = orderItem.DiscountAmountExclTax;
            if (!decimal.TryParse(form["pvPriceInclTax" + orderItemId], out decimal priceInclTax))
                priceInclTax = orderItem.PriceInclTax;
            if (!decimal.TryParse(form["pvPriceExclTax" + orderItemId], out decimal priceExclTax))
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
                await _orderService.UpdateOrder(order);
                //adjust inventory
                await productService.AdjustInventory(product, qtyDifference, orderItem.AttributesXml, orderItem.WarehouseId);

            }
            else
            {
                //adjust inventory
                await productService.AdjustInventory(product, orderItem.Quantity, orderItem.AttributesXml, orderItem.WarehouseId);
                await _orderService.DeleteOrderItem(orderItem);
            }

            order = await _orderService.GetOrderById(id);
            //add a note
            await _orderService.InsertOrderNote(new OrderNote {
                Note = "Order item has been edited",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
                OrderId = order.Id,
            });

            await _orderViewModelService.LogEditOrder(order.Id);
            var model = new OrderModel();
            await _orderViewModelService.PrepareOrderDetailsModel(model, order);

            //selected tab
            await SaveSelectedTabIndex(persistForTheNextRequest: false);

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnDeleteOrderItem")]
        public async Task<IActionResult> DeleteOrderItem(string id, IFormCollection form,
            [FromServices] IGiftCardService giftCardService,
            [FromServices] IProductService productService,
            [FromServices] IShipmentService shipmentService)
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Order", new { id = id });

            if (_workContext.CurrentCustomer.IsStaff() && order.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("Edit", "Order", new { id = id });
            }
            //get order item identifier
            string orderItemId = "";
            foreach (var formValue in form.Keys)
                if (formValue.StartsWith("btnDeleteOrderItem", StringComparison.OrdinalIgnoreCase))
                    orderItemId = formValue.Substring("btnDeleteOrderItem".Length);

            var orderItem = order.OrderItems.FirstOrDefault(x => x.Id == orderItemId);
            if (orderItem == null)
                throw new ArgumentException("No order item found with the specified id");

            var shipments = (await shipmentService.GetShipmentsByOrder(order.Id));
            foreach (var shipment in shipments)
            {
                if (shipment.ShipmentItems.Where(x => x.OrderItemId == orderItemId).Any())
                {
                    ErrorNotification($"This order item is in associated with shipment {shipment.ShipmentNumber}. Please delete it first.", false);
                    //selected tab
                    await SaveSelectedTabIndex(persistForTheNextRequest: false);
                    var model = new OrderModel();
                    await _orderViewModelService.PrepareOrderDetailsModel(model, order);
                    return View(model);
                }
            }

            var product = await productService.GetProductById(orderItem.ProductId);
            if ((await giftCardService.GetGiftCardsByPurchasedWithOrderItemId(orderItem.Id)).Count > 0)
            {
                //we cannot delete an order item with associated gift cards
                //a store owner should delete them first

                var model = new OrderModel();
                await _orderViewModelService.PrepareOrderDetailsModel(model, order);

                ErrorNotification("This order item has an associated gift card record. Please delete it first.", false);

                //selected tab
                await SaveSelectedTabIndex(persistForTheNextRequest: false);

                return View(model);

            }
            else
            {
                //add a note
                await _orderService.InsertOrderNote(new OrderNote {
                    Note = "Order item has been deleted",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    OrderId = order.Id,
                });

                //adjust inventory
                if (product != null)
                    await productService.AdjustInventory(product, orderItem.Quantity, orderItem.AttributesXml, orderItem.WarehouseId);

                await _orderService.DeleteOrderItem(orderItem);
                order = await _orderService.GetOrderById(id);
                await _orderViewModelService.LogEditOrder(order.Id);
                var model = new OrderModel();
                await _orderViewModelService.PrepareOrderDetailsModel(model, order);

                //selected tab
                await SaveSelectedTabIndex(persistForTheNextRequest: false);

                return View(model);
            }
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnResetDownloadCount")]
        public async Task<IActionResult> ResetDownloadCount(string id, IFormCollection form)
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff() && order.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("Edit", "Order", new { id = id });
            }

            //get order item identifier
            string orderItemId = "";
            foreach (var formValue in form.Keys)
                if (formValue.StartsWith("btnResetDownloadCount", StringComparison.OrdinalIgnoreCase))
                    orderItemId = formValue.Substring("btnResetDownloadCount".Length);

            var orderItem = order.OrderItems.FirstOrDefault(x => x.Id == orderItemId);
            if (orderItem == null)
                throw new ArgumentException("No order item found with the specified id");

            //ensure a vendor has access only to his products 
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToOrderItem(orderItem) && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("List");


            orderItem.DownloadCount = 0;
            await _orderService.UpdateOrder(order);
            await _orderViewModelService.LogEditOrder(order.Id);
            var model = new OrderModel();
            await _orderViewModelService.PrepareOrderDetailsModel(model, order);

            //selected tab
            await SaveSelectedTabIndex(persistForTheNextRequest: false);

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnPvActivateDownload")]

        public async Task<IActionResult> ActivateDownloadItem(string id, IFormCollection form)
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff() && order.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("Edit", "Order", new { id = id });
            }

            //get order item identifier
            string orderItemId = "";
            foreach (var formValue in form.Keys)
                if (formValue.StartsWith("btnPvActivateDownload", StringComparison.OrdinalIgnoreCase))
                    orderItemId = formValue.Substring("btnPvActivateDownload".Length);

            var orderItem = order.OrderItems.FirstOrDefault(x => x.Id == orderItemId);
            if (orderItem == null)
                throw new ArgumentException("No order item found with the specified id");

            //ensure a vendor has access only to his products 
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToOrderItem(orderItem) && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("List");

            orderItem.IsDownloadActivated = !orderItem.IsDownloadActivated;
            await _orderService.UpdateOrder(order);
            await _orderViewModelService.LogEditOrder(order.Id);
            var model = new OrderModel();
            await _orderViewModelService.PrepareOrderDetailsModel(model, order);

            //selected tab
            await SaveSelectedTabIndex(persistForTheNextRequest: false);

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> UploadLicenseFilePopup(string id, string orderItemId, [FromServices] IProductService productService)
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff() && order.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("Edit", "Order", new { id = id });
            }

            var orderItem = order.OrderItems.FirstOrDefault(x => x.Id == orderItemId);
            if (orderItem == null)
                throw new ArgumentException("No order item found with the specified id");

            var product = await productService.GetProductByIdIncludeArch(orderItem.ProductId);

            if (!product.IsDownload)
                throw new ArgumentException("Product is not downloadable");

            //ensure a vendor has access only to his products 
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToOrderItem(orderItem) && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("List");

            var model = new OrderModel.UploadLicenseModel {
                LicenseDownloadId = !String.IsNullOrEmpty(orderItem.LicenseDownloadId) ? orderItem.LicenseDownloadId : "",
                OrderId = order.Id,
                OrderItemId = orderItem.Id
            };

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        [FormValueRequired("uploadlicense")]
        public async Task<IActionResult> UploadLicenseFilePopup(OrderModel.UploadLicenseModel model)
        {
            var order = await _orderService.GetOrderById(model.OrderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff() && order.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("Edit", "Order", new { id = order.Id });
            }

            var orderItem = order.OrderItems.FirstOrDefault(x => x.Id == model.OrderItemId);
            if (orderItem == null)
                throw new ArgumentException("No order item found with the specified id");

            //ensure a vendor has access only to his products 
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToOrderItem(orderItem) && _workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("List");

            //attach license
            if (!string.IsNullOrEmpty(model.LicenseDownloadId))
                orderItem.LicenseDownloadId = model.LicenseDownloadId;
            else
                orderItem.LicenseDownloadId = null;
            await _orderService.UpdateOrder(order);

            await _orderViewModelService.LogEditOrder(order.Id);
            //success
            ViewBag.RefreshPage = true;

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ActionName("UploadLicenseFilePopup")]
        [FormValueRequired("deletelicense")]
        public async Task<IActionResult> DeleteLicenseFilePopup(OrderModel.UploadLicenseModel model)
        {
            var order = await _orderService.GetOrderById(model.OrderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff() && order.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("Edit", "Order", new { id = model.OrderId });
            }

            var orderItem = order.OrderItems.FirstOrDefault(x => x.Id == model.OrderItemId);
            if (orderItem == null)
                throw new ArgumentException("No order item found with the specified id");

            //ensure a vendor has access only to his products 
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToOrderItem(orderItem) && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("List");

            //attach license
            orderItem.LicenseDownloadId = null;
            await _orderService.UpdateOrder(order);
            await _orderViewModelService.LogEditOrder(order.Id);

            //success
            ViewBag.RefreshPage = true;

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> AddProductToOrder(string orderId)
        {
            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Order", new { id = orderId });

            var order = await _orderService.GetOrderById(orderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            var model = await _orderViewModelService.PrepareAddOrderProductModel(order);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> AddProductToOrder(DataSourceRequest command, OrderModel.AddOrderProductModel model, [FromServices] IProductService productService)
        {
            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return Content("");

            var categoryIds = new List<string>();
            if (!string.IsNullOrEmpty(model.SearchCategoryId))
                categoryIds.Add(model.SearchCategoryId);

            var storeId = string.Empty;
            if (_workContext.CurrentCustomer.IsStaff())
            {
                storeId = _workContext.CurrentCustomer.StaffStoreId;
            }

            var gridModel = new DataSourceResult();
            var products = (await productService.SearchProducts(categoryIds: categoryIds,
                storeId: storeId,
                manufacturerId: model.SearchManufacturerId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize,
                showHidden: true)).products;
            gridModel.Data = products.Select(x =>
            {
                var productModel = new OrderModel.AddOrderProductModel.ProductModel {
                    Id = x.Id,
                    Name = x.Name,
                    Sku = x.Sku,
                };

                return productModel;
            });
            gridModel.Total = products.TotalCount;

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> AddProductToOrderDetails(string orderId, string productId)
        {
            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Order", new { id = orderId });

            var order = await _orderService.GetOrderById(orderId);
            if (order == null)
                return RedirectToAction("List");

            if (order == null)
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff() && order.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            var model = await _orderViewModelService.PrepareAddProductToOrderModel(order, productId);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> AddProductToOrderDetails(string orderId, string productId, IFormCollection form)
        {
            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Order", new { id = orderId });

            var order = await _orderService.GetOrderById(orderId);
            if (order == null)
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff() && order.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            var warnings = await _orderViewModelService.AddProductToOrderDetails(orderId, productId, form);
            if (!warnings.Any())
                //redirect to order details page
                return RedirectToAction("Edit", "Order", new { id = orderId });

            //errors
            var model = await _orderViewModelService.PrepareAddProductToOrderModel(order, productId);
            model.Warnings.AddRange(warnings);
            return View(model);
        }

        #endregion

        #endregion

        #region Addresses

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        public async Task<IActionResult> AddressEdit(string addressId, string orderId)
        {
            var order = await _orderService.GetOrderById(orderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Order", new { id = orderId });

            if (_workContext.CurrentCustomer.IsStaff() && order.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            var address = new Address();
            if (order.BillingAddress != null)
                if (order.BillingAddress.Id == addressId)
                    address = order.BillingAddress;
            if (order.ShippingAddress != null)
                if (order.ShippingAddress.Id == addressId)
                    address = order.ShippingAddress;

            if (address == null)
                throw new ArgumentException("No address found with the specified id", "addressId");

            var model = await _orderViewModelService.PrepareOrderAddressModel(order, address);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> AddressEdit(OrderAddressModel model, IFormCollection form,
            [FromServices] IAddressAttributeService addressAttributeService,
            [FromServices] IAddressAttributeParser addressAttributeParser)
        {
            var order = await _orderService.GetOrderById(model.OrderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Order", new { id = order.Id });

            if (_workContext.CurrentCustomer.IsStaff() && order.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            var address = new Address();
            if (order.BillingAddress != null)
                if (order.BillingAddress.Id == model.Address.Id)
                    address = order.BillingAddress;
            if (order.ShippingAddress != null)
                if (order.ShippingAddress.Id == model.Address.Id)
                    address = order.ShippingAddress;

            if (address == null)
                throw new ArgumentException("No address found with the specified id");

            //custom address attributes
            var customAttributes = await form.ParseCustomAddressAttributes(addressAttributeParser, addressAttributeService);
            var customAttributeWarnings = await addressAttributeParser.GetAttributeWarnings(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            if (ModelState.IsValid)
            {
                address = await _orderViewModelService.UpdateOrderAddress(order, address, model, customAttributes);
                return RedirectToAction("AddressEdit", new { addressId = model.Address.Id, orderId = model.OrderId });
            }

            //If we got this far, something failed, redisplay form
            model = await _orderViewModelService.PrepareOrderAddressModel(order, address);

            return View(model);
        }

        #endregion

        #region Order notes

        [PermissionAuthorizeAction(PermissionActionName.List)]
        [HttpPost]
        public async Task<IActionResult> OrderNotesSelect(string orderId, DataSourceRequest command)
        {
            var order = await _orderService.GetOrderById(orderId);
            if (order == null)
                throw new ArgumentException("No order found with the specified id");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return Content("");

            if (_workContext.CurrentCustomer.IsStaff() && order.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return Content("");
            }
            //order notes
            var orderNoteModels = await _orderViewModelService.PrepareOrderNotes(order);
            var gridModel = new DataSourceResult {
                Data = orderNoteModels,
                Total = orderNoteModels.Count
            };
            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> OrderNoteAdd(string orderId, string downloadId, bool displayToCustomer, string message)
        {
            var order = await _orderService.GetOrderById(orderId);
            if (order == null)
                return Json(new { Result = false });

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return Json(new { Result = false });

            if (_workContext.CurrentCustomer.IsStaff() && order.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return Json(new { Result = false });
            }
            await _orderViewModelService.InsertOrderNote(order, downloadId, displayToCustomer, message);

            return Json(new { Result = true });
        }

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        [HttpPost]
        public async Task<IActionResult> OrderNoteDelete(string id, string orderId)
        {
            var order = await _orderService.GetOrderById(orderId);
            if (order == null)
                throw new ArgumentException("No order found with the specified id");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return Json(new { Result = false });

            if (_workContext.CurrentCustomer.IsStaff() && order.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return Json(new { Result = false });
            }

            await _orderViewModelService.DeleteOrderNote(order, id);

            return new NullJsonResult();
        }
        #endregion

    }
}
