using Grand.Core;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Media;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Shipping;
using Grand.Core.Domain.Tax;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Affiliates;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.ExportImport;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Media;
using Grand.Services.Messages;
using Grand.Services.Orders;
using Grand.Services.Security;
using Grand.Services.Shipping;
using Grand.Services.Stores;
using Grand.Services.Tax;
using Grand.Services.Vendors;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Orders;
using Grand.Web.Areas.Admin.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Grand.Core.Domain.Customers;
using Grand.Services.Helpers;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Shipments)]
    public partial class ShipmentController : BaseAdminController
    {
        #region Fields
        private readonly IShipmentViewModelService _shipmentViewModelService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IMeasureService _measureService;
        private readonly IPdfService _pdfService;
        private readonly IProductService _productService;
        private readonly IExportManager _exportManager;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IGiftCardService _giftCardService;
        private readonly IDownloadService _downloadService;
        private readonly IShipmentService _shipmentService;
        private readonly IShippingService _shippingService;
        private readonly IStoreService _storeService;
        private readonly IVendorService _vendorService;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressAttributeService _addressAttributeService;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly IAffiliateService _affiliateService;
        private readonly IPictureService _pictureService;
        private readonly ITaxService _taxService;
        private readonly IReturnRequestService _returnRequestService;
        private readonly ICustomerService _customerService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly CurrencySettings _currencySettings;
        private readonly TaxSettings _taxSettings;
        private readonly MeasureSettings _measureSettings;
        private readonly AddressSettings _addressSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly MediaSettings _mediaSettings;
        #endregion

        public ShipmentController(
            IShipmentViewModelService shipmentViewModelService,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IMeasureService measureService,
            IPdfService pdfService,
            IProductService productService,
            IExportManager exportManager,
            IWorkflowMessageService workflowMessageService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IProductAttributeService productAttributeService,
            IProductAttributeParser productAttributeParser,
            IProductAttributeFormatter productAttributeFormatter,
            IShoppingCartService shoppingCartService,
            IGiftCardService giftCardService,
            IDownloadService downloadService,
            IShipmentService shipmentService,
            IShippingService shippingService,
            IStoreService storeService,
            IVendorService vendorService,
            IAddressAttributeParser addressAttributeParser,
            IAddressAttributeService addressAttributeService,
            IAddressAttributeFormatter addressAttributeFormatter,
            IAffiliateService affiliateService,
            IPictureService pictureService,
            ITaxService taxService,
            IReturnRequestService returnRequestService,
            ICustomerService customerService,
            ICustomerActivityService customerActivityService,
            IDateTimeHelper dateTimeHelper,
            CurrencySettings currencySettings,
            TaxSettings taxSettings,
            MeasureSettings measureSettings,
            AddressSettings addressSettings,
            ShippingSettings shippingSettings,
            MediaSettings mediaSettings)
        {
             _shipmentViewModelService = shipmentViewModelService;
             _orderService = orderService;
             _orderProcessingService = orderProcessingService;
             _localizationService = localizationService;
             _workContext = workContext;
             _measureService = measureService;
             _pdfService = pdfService;
             _productService = productService;
             _exportManager = exportManager;
             _workflowMessageService = workflowMessageService;
             _categoryService = categoryService;
             _manufacturerService = manufacturerService;
             _productAttributeService = productAttributeService;
             _productAttributeParser = productAttributeParser;
             _productAttributeFormatter = productAttributeFormatter;
             _shoppingCartService = shoppingCartService;
             _giftCardService = giftCardService;
             _downloadService = downloadService;
             _shipmentService = shipmentService;
             _shippingService = shippingService;
             _storeService = storeService;
             _vendorService = vendorService;
             _addressAttributeParser = addressAttributeParser;
             _addressAttributeService = addressAttributeService;
             _addressAttributeFormatter = addressAttributeFormatter;
             _affiliateService = affiliateService;
             _pictureService = pictureService;
             _taxService = taxService;
             _returnRequestService = returnRequestService;
             _customerActivityService = customerActivityService;
             _dateTimeHelper = dateTimeHelper;
             _currencySettings = currencySettings;
             _taxSettings = taxSettings;
             _measureSettings = measureSettings;
             _addressSettings = addressSettings;
             _shippingSettings = shippingSettings;
             _customerService = customerService;
             _mediaSettings = mediaSettings;
        }

        #region Shipments

        public async Task<IActionResult> List()
        {
            var model = await _shipmentViewModelService.PrepareShipmentListModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ShipmentListSelect(DataSourceRequest command, ShipmentListModel model)
        {
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                model.VendorId = _workContext.CurrentVendor.Id;

            if (_workContext.CurrentCustomer.IsStaff())
            {
                model.StoreId = _workContext.CurrentCustomer.StaffStoreId;
            }

            var shipments = await _shipmentViewModelService.PrepareShipments(model, command.Page, command.PageSize);
            var items = new List<ShipmentModel>();
            foreach (var item in shipments.shipments)
            {
                items.Add(await _shipmentViewModelService.PrepareShipmentModel(item, false));
            }
            var gridModel = new DataSourceResult
            {
                Data = items,
                Total = shipments.totalCount
            };
            return Json(gridModel);
        }

        [HttpPost]
        public async Task<IActionResult> ShipmentsByOrder(string orderId, DataSourceRequest command)
        {
            var order = await _orderService.GetOrderById(orderId);
            if (order == null)
                throw new ArgumentException("No order found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToOrder(order) && !_workContext.CurrentCustomer.IsStaff())
                return Content("");

            if (_workContext.CurrentCustomer.IsStaff() && order.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return Content("");
            }

            //shipments
            var shipmentModels = new List<ShipmentModel>();
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
            {
                var shipments = (await _shipmentService.GetShipmentsByOrder(orderId))
                    //a vendor should have access only to his products
                    .Where(s => _workContext.CurrentVendor == null || _workContext.HasAccessToShipment(order, s))
                    .OrderBy(s => s.CreatedOnUtc)
                    .ToList();
                foreach (var shipment in shipments)
                    shipmentModels.Add(await _shipmentViewModelService.PrepareShipmentModel(shipment, false));
            }
            else
            {
                var shipments = (await _shipmentService.GetShipmentsByOrder(orderId))
                   .OrderBy(s => s.CreatedOnUtc)
                   .ToList();
                foreach (var shipment in shipments)
                    shipmentModels.Add(await _shipmentViewModelService.PrepareShipmentModel(shipment, false));
            }
            var gridModel = new DataSourceResult
            {
                Data = shipmentModels,
                Total = shipmentModels.Count
            };
            return Json(gridModel);
        }

        [HttpPost]
        public async Task<IActionResult> ShipmentsItemsByShipmentId(string shipmentId, DataSourceRequest command)
        {
            var shipment = await _shipmentService.GetShipmentById(shipmentId);
            if (shipment == null)
                throw new ArgumentException("No shipment found with the specified id");

            var order = await _orderService.GetOrderById(shipment.OrderId);
            if (order == null)
                throw new ArgumentException("No order found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToOrder(order) && !_workContext.CurrentCustomer.IsStaff())
                return Content("");

            if (_workContext.CurrentCustomer.IsStaff() && shipment.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return Content("");
            }

            //shipments
            var shipmentModel = await _shipmentViewModelService.PrepareShipmentModel(shipment, true);
            var gridModel = new DataSourceResult
            {
                Data = shipmentModel.Items,
                Total = shipmentModel.Items.Count
            };

            return Json(gridModel);
        }

        public async Task<IActionResult> AddShipment(string orderId)
        {
            var order = await _orderService.GetOrderById(orderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToOrder(order) && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff() && order.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            var model = await _shipmentViewModelService.PrepareShipmentModel(order);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public async Task<IActionResult> AddShipment(string orderId, IFormCollection form, bool continueEditing)
        {
            var order = await _orderService.GetOrderById(orderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToOrder(order) && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff() && order.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            var orderItems = order.OrderItems;
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
            {
                orderItems = orderItems.Where(_workContext.HasAccessToOrderItem).ToList();
            }
            var sh = await _shipmentViewModelService.PrepareShipment(order, orderItems.ToList(), form);
            Shipment shipment = sh.shipment;
            //if we have at least one item in the shipment, then save it
            if (shipment != null && shipment.ShipmentItems.Count > 0)
            {
                shipment.TotalWeight = sh.totalWeight;
                await _shipmentService.InsertShipment(shipment);

                //add a note
                await _orderService.InsertOrderNote(new OrderNote
                {
                    Note = $"A shipment #{shipment.ShipmentNumber} has been added",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    OrderId = order.Id,
                });

                await _shipmentViewModelService.LogShipment(shipment.Id, $"A shipment #{shipment.ShipmentNumber} has been added");
                SuccessNotification(_localizationService.GetResource("Admin.Orders.Shipments.Added"));
                return continueEditing
                           ? RedirectToAction("ShipmentDetails", new { id = shipment.Id })
                           : RedirectToAction("List", new { id = shipment.Id });
            }

            ErrorNotification(_localizationService.GetResource("Admin.Orders.Shipments.NoProductsSelected"));
            return RedirectToAction("AddShipment", new { orderId = orderId });
        }

        public async Task<IActionResult> ShipmentDetails(string id)
        {
            var shipment = await _shipmentService.GetShipmentById(id);
            if (shipment == null)
                //No shipment found with the specified id
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff() && shipment.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            var orderId = shipment.OrderId;
            var order = await _orderService.GetOrderById(orderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToShipment(order, shipment) && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("List");

            var model = await _shipmentViewModelService.PrepareShipmentModel(shipment, true, true);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteShipment(string id)
        {
            var shipment = await _shipmentService.GetShipmentById(id);
            if (shipment == null)
                //No shipment found with the specified id
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff() && shipment.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            var orderId = shipment.OrderId;
            var order = await _orderService.GetOrderById(orderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToShipment(order, shipment) && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("List");

            foreach (var shipmentItem in shipment.ShipmentItems)
            {
                var product = await _productService.GetProductById(shipmentItem.ProductId);
                shipmentItem.ShipmentId = shipment.Id;
                if (product != null)
                    await _productService.ReverseBookedInventory(product, shipmentItem);
            }

            await _shipmentService.DeleteShipment(shipment);
            //add a note
            await _orderService.InsertOrderNote(new OrderNote
            {
                Note = $"A shipment #{shipment.ShipmentNumber} has been deleted",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
                OrderId = order.Id,
            });

            await _shipmentViewModelService.LogShipment(shipment.Id, $"A shipment #{shipment.ShipmentNumber} has been deleted");

            SuccessNotification(_localizationService.GetResource("Admin.Orders.Shipments.Deleted"));

            return RedirectToAction("Edit", "Order", new { Id = order.Id });
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("settrackingnumber")]
        public async Task<IActionResult> SetTrackingNumber(ShipmentModel model)
        {
            var shipment = await _shipmentService.GetShipmentById(model.Id);
            if (shipment == null)
                //No shipment found with the specified id
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff() && shipment.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            var order = await _orderService.GetOrderById(shipment.OrderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToShipment(order, shipment) && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("List");

            shipment.TrackingNumber = model.TrackingNumber;
            await _shipmentService.UpdateShipment(shipment);

            return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("setadmincomment")]
        public async Task<IActionResult> SetShipmentAdminComment(ShipmentModel model)
        {
            var shipment = await _shipmentService.GetShipmentById(model.Id);
            if (shipment == null)
                //No shipment found with the specified id
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff() && shipment.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            var order = await _orderService.GetOrderById(shipment.OrderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToShipment(order, shipment) && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("List");

            shipment.AdminComment = model.AdminComment;
            await _shipmentService.UpdateShipment(shipment);

            return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("setasshipped")]
        public async Task<IActionResult> SetAsShipped(string id)
        {
            var shipment = await _shipmentService.GetShipmentById(id);
            if (shipment == null)
                //No shipment found with the specified id
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff() && shipment.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            var order = await _orderService.GetOrderById(shipment.OrderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToShipment(order, shipment) && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("List");

            try
            {
                await _orderProcessingService.Ship(shipment, true);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
            catch (Exception exc)
            {
                //error
                ErrorNotification(exc, true);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("saveshippeddate")]
        public async Task<IActionResult> EditShippedDate(ShipmentModel model)
        {
            var shipment = await _shipmentService.GetShipmentById(model.Id);
            if (shipment == null)
                //No shipment found with the specified id
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff() && shipment.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            var order = await _orderService.GetOrderById(shipment.OrderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToShipment(order, shipment) && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("List");

            try
            {
                if (!model.ShippedDate.HasValue)
                {
                    throw new Exception("Enter shipped date");
                }
                shipment.ShippedDateUtc = model.ShippedDate.ConvertToUtcTime(_dateTimeHelper);
                await _shipmentService.UpdateShipment(shipment);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
            catch (Exception exc)
            {
                //error
                ErrorNotification(exc, true);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("setasdelivered")]
        public async Task<IActionResult> SetAsDelivered(string id)
        {
            var shipment = await _shipmentService.GetShipmentById(id);
            if (shipment == null)
                //No shipment found with the specified id
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff() && shipment.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            var order = await _orderService.GetOrderById(shipment.OrderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToShipment(order, shipment) && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("List");

            try
            {
                await _orderProcessingService.Deliver(shipment, true);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
            catch (Exception exc)
            {
                //error
                ErrorNotification(exc, true);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
        }


        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("savedeliverydate")]
        public async Task<IActionResult> EditDeliveryDate(ShipmentModel model)
        {
            var shipment = await _shipmentService.GetShipmentById(model.Id);
            if (shipment == null)
                //No shipment found with the specified id
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff() && shipment.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            var order = await _orderService.GetOrderById(shipment.OrderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToShipment(order, shipment) && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("List");

            try
            {
                if (!model.DeliveryDate.HasValue)
                {
                    throw new Exception("Enter delivery date");
                }
                shipment.DeliveryDateUtc = model.DeliveryDate.ConvertToUtcTime(_dateTimeHelper);
                await _shipmentService.UpdateShipment(shipment);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
            catch (Exception exc)
            {
                //error
                ErrorNotification(exc, true);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
        }

        public async Task<IActionResult> PdfPackagingSlip(string shipmentId)
        {
            var shipment = await _shipmentService.GetShipmentById(shipmentId);
            if (shipment == null)
                //no shipment found with the specified id
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff() && shipment.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            var order = await _orderService.GetOrderById(shipment.OrderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToShipment(order, shipment) && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("List");

            var shipments = new List<Shipment>
            {
                shipment
            };

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await _pdfService.PrintPackagingSlipsToPdf(stream, shipments, _workContext.WorkingLanguage.Id);
                bytes = stream.ToArray();
            }
            return File(bytes, "application/pdf", string.Format("packagingslip_{0}.pdf", shipment.Id));
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("exportpackagingslips-all")]
        public async Task<IActionResult> PdfPackagingSlipAll(ShipmentListModel model)
        {
            if (_workContext.CurrentCustomer.IsStaff())
            {
                model.StoreId = _workContext.CurrentCustomer.StaffStoreId;
            }

            //load shipments
            var shipments = await _shipmentViewModelService.PrepareShipments(model, 1, 100);
            
            //ensure that we at least one shipment selected
            if (shipments.totalCount == 0)
            {
                ErrorNotification(_localizationService.GetResource("Admin.Orders.Shipments.NoShipmentsSelected"));
                return RedirectToAction("List");
            }

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await _pdfService.PrintPackagingSlipsToPdf(stream, shipments.shipments.ToList(), _workContext.WorkingLanguage.Id);
                bytes = stream.ToArray();
            }
            return File(bytes, "application/pdf", "packagingslips.pdf");
        }

        [HttpPost]
        public async Task<IActionResult> PdfPackagingSlipSelected(string selectedIds)
        {
            var shipments = new List<Shipment>();
            var shipments_access = new List<Shipment>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x)
                    .ToArray();
                shipments.AddRange(await _shipmentService.GetShipmentsByIds(ids));
            }

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
            {
                foreach (var item in shipments)
                {
                    var order = await _orderService.GetOrderById(item.OrderId);
                    var hasaccess = _workContext.HasAccessToShipment(order, item);
                    if (hasaccess)
                        shipments_access.Add(item);
                }
            }
            else
            {
                var storeId = "";
                if (_workContext.CurrentCustomer.IsStaff())
                {
                    storeId = _workContext.CurrentCustomer.StaffStoreId;
                }
                shipments_access = shipments.Where(x=>x.StoreId == storeId || string.IsNullOrEmpty(storeId)).ToList();
            }
            //ensure that we at least one shipment selected
            if (shipments.Count == 0)
            {
                ErrorNotification(_localizationService.GetResource("Admin.Orders.Shipments.NoShipmentsSelected"));
                return RedirectToAction("List");
            }

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await _pdfService.PrintPackagingSlipsToPdf(stream, shipments_access, _workContext.WorkingLanguage.Id);
                bytes = stream.ToArray();
            }
            return File(bytes, "application/pdf", "packagingslips.pdf");
        }

        [HttpPost]
        public async Task<IActionResult> SetAsShippedSelected(ICollection<string> selectedIds)
        {
            var shipments = new List<Shipment>();
            var shipments_access = new List<Shipment>();
            if (selectedIds != null)
            {
                shipments.AddRange(await _shipmentService.GetShipmentsByIds(selectedIds.ToArray()));
            }
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
            {
                foreach (var item in shipments)
                {
                    var order = await _orderService.GetOrderById(item.OrderId);
                    var hasaccess = _workContext.HasAccessToShipment(order, item);
                    if (hasaccess)
                        shipments_access.Add(item);
                }
            }
            else
            {
                var storeId = "";
                if (_workContext.CurrentCustomer.IsStaff())
                {
                    storeId = _workContext.CurrentCustomer.StaffStoreId;
                }
                shipments_access = shipments.Where(x => x.StoreId == storeId || string.IsNullOrEmpty(storeId)).ToList();
            }

            foreach (var shipment in shipments_access)
            {
                try
                {
                    await _orderProcessingService.Ship(shipment, true);
                }
                catch
                {
                    //ignore any exception
                }
            }

            return Json(new { Result = true });
        }

        [HttpPost]
        public async Task<IActionResult> SetAsDeliveredSelected(ICollection<string> selectedIds)
        {
            var shipments = new List<Shipment>();
            var shipments_access = new List<Shipment>();
            if (selectedIds != null)
            {
                shipments.AddRange(await _shipmentService.GetShipmentsByIds(selectedIds.ToArray()));
            }
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
            {
                foreach (var item in shipments)
                {
                    var order = await _orderService.GetOrderById(item.OrderId);
                    var hasaccess = _workContext.HasAccessToShipment(order, item);
                    if (hasaccess)
                        shipments_access.Add(item);
                }
            }
            else
            {
                var storeId = "";
                if (_workContext.CurrentCustomer.IsStaff())
                {
                    storeId = _workContext.CurrentCustomer.StaffStoreId;
                }
                shipments_access = shipments.Where(x => x.StoreId == storeId || string.IsNullOrEmpty(storeId)).ToList();
            }

            foreach (var shipment in shipments_access)
            {
                try
                {
                    await _orderProcessingService.Deliver(shipment, true);
                }
                catch
                {
                    //ignore any exception
                }
            }

            return Json(new { Result = true });
        }

        #endregion
    }
}
