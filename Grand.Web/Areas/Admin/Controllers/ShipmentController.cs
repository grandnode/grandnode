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
            CurrencySettings currencySettings,
            TaxSettings taxSettings,
            MeasureSettings measureSettings,
            AddressSettings addressSettings,
            ShippingSettings shippingSettings,
            MediaSettings mediaSettings)
        {
            this._shipmentViewModelService = shipmentViewModelService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._measureService = measureService;
            this._pdfService = pdfService;
            this._productService = productService;
            this._exportManager = exportManager;
            this._workflowMessageService = workflowMessageService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._productAttributeService = productAttributeService;
            this._productAttributeParser = productAttributeParser;
            this._productAttributeFormatter = productAttributeFormatter;
            this._shoppingCartService = shoppingCartService;
            this._giftCardService = giftCardService;
            this._downloadService = downloadService;
            this._shipmentService = shipmentService;
            this._shippingService = shippingService;
            this._storeService = storeService;
            this._vendorService = vendorService;
            this._addressAttributeParser = addressAttributeParser;
            this._addressAttributeService = addressAttributeService;
            this._addressAttributeFormatter = addressAttributeFormatter;
            this._affiliateService = affiliateService;
            this._pictureService = pictureService;
            this._taxService = taxService;
            this._returnRequestService = returnRequestService;
            this._customerActivityService = customerActivityService;
            this._currencySettings = currencySettings;
            this._taxSettings = taxSettings;
            this._measureSettings = measureSettings;
            this._addressSettings = addressSettings;
            this._shippingSettings = shippingSettings;
            this._customerService = customerService;
            this._mediaSettings = mediaSettings;
        }

        #region Shipments

        public IActionResult List()
        {
            var model = _shipmentViewModelService.PrepareShipmentListModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult ShipmentListSelect(DataSourceRequest command, ShipmentListModel model)
        {
            var shipments = _shipmentViewModelService.PrepareShipments(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = shipments.shipments.Select(shipment => _shipmentViewModelService.PrepareShipmentModel(shipment, false)),
                Total = shipments.totalCount
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ShipmentsByOrder(string orderId, DataSourceRequest command)
        {
            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                throw new ArgumentException("No order found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToOrder(order))
                return Content("");

            //shipments
            var shipmentModels = new List<ShipmentModel>();
            var shipments = _shipmentService.GetShipmentsByOrder(orderId)
                //a vendor should have access only to his products
                .Where(s => _workContext.CurrentVendor == null || _workContext.HasAccessToShipment(order, s))
                .OrderBy(s => s.CreatedOnUtc)
                .ToList();
            foreach (var shipment in shipments)
                shipmentModels.Add(_shipmentViewModelService.PrepareShipmentModel(shipment, false));
            
            var gridModel = new DataSourceResult
            {
                Data = shipmentModels,
                Total = shipmentModels.Count
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ShipmentsItemsByShipmentId(string shipmentId, DataSourceRequest command)
        {
            var shipment = _shipmentService.GetShipmentById(shipmentId);
            if (shipment == null)
                throw new ArgumentException("No shipment found with the specified id");

            var order = _orderService.GetOrderById(shipment.OrderId);
            if (order == null)
                throw new ArgumentException("No order found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToOrder(order))
                return Content("");

            //a vendor should have access only to his products
            //if (_workContext.CurrentVendor != null && !_workContext.HasAccessToShipment(order, shipment))
            //    return Content("");

            //shipments
            var shipmentModel = _shipmentViewModelService.PrepareShipmentModel(shipment, true);
            var gridModel = new DataSourceResult
            {
                Data = shipmentModel.Items,
                Total = shipmentModel.Items.Count
            };

            return Json(gridModel);
        }

        public IActionResult AddShipment(string orderId)
        {
            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToOrder(order))
                return RedirectToAction("List");

            var model = _shipmentViewModelService.PrepareShipmentModel(order);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public IActionResult AddShipment(string orderId, IFormCollection form, bool continueEditing)
        {
            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToOrder(order))
                return RedirectToAction("List");

            var orderItems = order.OrderItems;
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                orderItems = orderItems.Where(_workContext.HasAccessToOrderItem).ToList();
            }
            var sh = _shipmentViewModelService.PrepareShipment(order, orderItems.ToList(), form);
            Shipment shipment = sh.shipment;
            //if we have at least one item in the shipment, then save it
            if (shipment != null && shipment.ShipmentItems.Count > 0)
            {
                shipment.TotalWeight = sh.totalWeight;
                _shipmentService.InsertShipment(shipment);

                //add a note
                _orderService.InsertOrderNote(new OrderNote
                {
                    Note = $"A shipment #{shipment.ShipmentNumber} has been added",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    OrderId = order.Id,
                });

                _shipmentViewModelService.LogShipment(shipment.Id, $"A shipment #{shipment.ShipmentNumber} has been added");
                SuccessNotification(_localizationService.GetResource("Admin.Orders.Shipments.Added"));
                return continueEditing
                           ? RedirectToAction("ShipmentDetails", new { id = shipment.Id })
                           : RedirectToAction("List", new { id = shipment.Id });
            }

            ErrorNotification(_localizationService.GetResource("Admin.Orders.Shipments.NoProductsSelected"));
            return RedirectToAction("AddShipment", new { orderId = orderId });
        }

        public IActionResult ShipmentDetails(string id)
        {
            var shipment = _shipmentService.GetShipmentById(id);
            if (shipment == null)
                //No shipment found with the specified id
                return RedirectToAction("List");

            var orderId = shipment.OrderId;
            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToShipment(order, shipment))
                return RedirectToAction("List");

            var model = _shipmentViewModelService.PrepareShipmentModel(shipment, true, true);
            return View(model);
        }

        [HttpPost]
        public IActionResult DeleteShipment(string id)
        {
            var shipment = _shipmentService.GetShipmentById(id);
            if (shipment == null)
                //No shipment found with the specified id
                return RedirectToAction("List");

            var orderId = shipment.OrderId;
            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToShipment(order, shipment))
                return RedirectToAction("List");

            foreach (var shipmentItem in shipment.ShipmentItems)
            {
                var product = _productService.GetProductById(shipmentItem.ProductId);
                shipmentItem.ShipmentId = shipment.Id;
                if (product != null)
                    _productService.ReverseBookedInventory(product, shipmentItem);
            }

            _shipmentService.DeleteShipment(shipment);
            //add a note
            _orderService.InsertOrderNote(new OrderNote
            {
                Note = $"A shipment #{shipment.ShipmentNumber} has been deleted",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
                OrderId = order.Id,
            });

            _shipmentViewModelService.LogShipment(shipment.Id, $"A shipment #{shipment.ShipmentNumber} has been deleted");

            SuccessNotification(_localizationService.GetResource("Admin.Orders.Shipments.Deleted"));

            return RedirectToAction("Edit", "Order", new { Id = order.Id });
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("settrackingnumber")]
        public IActionResult SetTrackingNumber(ShipmentModel model)
        {
            var shipment = _shipmentService.GetShipmentById(model.Id);
            if (shipment == null)
                //No shipment found with the specified id
                return RedirectToAction("List");

            var order = _orderService.GetOrderById(shipment.OrderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToShipment(order, shipment))
                return RedirectToAction("List");

            shipment.TrackingNumber = model.TrackingNumber;
            _shipmentService.UpdateShipment(shipment);

            return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("setadmincomment")]
        public IActionResult SetShipmentAdminComment(ShipmentModel model)
        {
            var shipment = _shipmentService.GetShipmentById(model.Id);
            if (shipment == null)
                //No shipment found with the specified id
                return RedirectToAction("List");

            var order = _orderService.GetOrderById(shipment.OrderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToShipment(order, shipment))
                return RedirectToAction("List");

            shipment.AdminComment = model.AdminComment;
            _shipmentService.UpdateShipment(shipment);

            return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("setasshipped")]
        public IActionResult SetAsShipped(string id)
        {
            var shipment = _shipmentService.GetShipmentById(id);
            if (shipment == null)
                //No shipment found with the specified id
                return RedirectToAction("List");

            var order = _orderService.GetOrderById(shipment.OrderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToShipment(order, shipment))
                return RedirectToAction("List");

            try
            {
                _orderProcessingService.Ship(shipment, true);
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
        public IActionResult EditShippedDate(ShipmentModel model)
        {
            var shipment = _shipmentService.GetShipmentById(model.Id);
            if (shipment == null)
                //No shipment found with the specified id
                return RedirectToAction("List");

            var order = _orderService.GetOrderById(shipment.OrderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToShipment(order, shipment))
                return RedirectToAction("List");

            try
            {
                if (!model.ShippedDate.HasValue)
                {
                    throw new Exception("Enter shipped date");
                }
                shipment.ShippedDateUtc = model.ShippedDate.ConvertToUtcTime();
                _shipmentService.UpdateShipment(shipment);
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
        public IActionResult SetAsDelivered(string id)
        {
            var shipment = _shipmentService.GetShipmentById(id);
            if (shipment == null)
                //No shipment found with the specified id
                return RedirectToAction("List");

            var order = _orderService.GetOrderById(shipment.OrderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToShipment(order, shipment))
                return RedirectToAction("List");

            try
            {
                _orderProcessingService.Deliver(shipment, true);
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
        public IActionResult EditDeliveryDate(ShipmentModel model)
        {
            var shipment = _shipmentService.GetShipmentById(model.Id);
            if (shipment == null)
                //No shipment found with the specified id
                return RedirectToAction("List");

            var order = _orderService.GetOrderById(shipment.OrderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToShipment(order, shipment))
                return RedirectToAction("List");

            try
            {
                if (!model.DeliveryDate.HasValue)
                {
                    throw new Exception("Enter delivery date");
                }
                shipment.DeliveryDateUtc = model.DeliveryDate.ConvertToUtcTime();
                _shipmentService.UpdateShipment(shipment);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
            catch (Exception exc)
            {
                //error
                ErrorNotification(exc, true);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
        }

        public IActionResult PdfPackagingSlip(string shipmentId)
        {
            var shipment = _shipmentService.GetShipmentById(shipmentId);
            if (shipment == null)
                //no shipment found with the specified id
                return RedirectToAction("List");

            var order = _orderService.GetOrderById(shipment.OrderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToShipment(order, shipment))
                return RedirectToAction("List");

            var shipments = new List<Shipment>();
            shipments.Add(shipment);

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                _pdfService.PrintPackagingSlipsToPdf(stream, shipments, _workContext.WorkingLanguage.Id);
                bytes = stream.ToArray();
            }
            return File(bytes, "application/pdf", string.Format("packagingslip_{0}.pdf", shipment.Id));
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("exportpackagingslips-all")]
        public IActionResult PdfPackagingSlipAll(ShipmentListModel model)
        {
            //load shipments
            var shipments = _shipmentViewModelService.PrepareShipments(model, 1, 100);

            //ensure that we at least one shipment selected
            if (shipments.totalCount == 0)
            {
                ErrorNotification(_localizationService.GetResource("Admin.Orders.Shipments.NoShipmentsSelected"));
                return RedirectToAction("List");
            }

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                _pdfService.PrintPackagingSlipsToPdf(stream, shipments.shipments.ToList(), _workContext.WorkingLanguage.Id);
                bytes = stream.ToArray();
            }
            return File(bytes, "application/pdf", "packagingslips.pdf");
        }

        [HttpPost]
        public IActionResult PdfPackagingSlipSelected(string selectedIds)
        {
            var shipments = new List<Shipment>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x)
                    .ToArray();
                shipments.AddRange(_shipmentService.GetShipmentsByIds(ids));
            }

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                shipments = shipments.Where(x =>
                {
                    var order = _orderService.GetOrderById(x.OrderId);
                    return _workContext.HasAccessToShipment(order, x);
                })
                .ToList();
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
                _pdfService.PrintPackagingSlipsToPdf(stream, shipments, _workContext.WorkingLanguage.Id);
                bytes = stream.ToArray();
            }
            return File(bytes, "application/pdf", "packagingslips.pdf");
        }

        [HttpPost]
        public IActionResult SetAsShippedSelected(ICollection<string> selectedIds)
        {
            var shipments = new List<Shipment>();
            if (selectedIds != null)
            {
                shipments.AddRange(_shipmentService.GetShipmentsByIds(selectedIds.ToArray()));
            }
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                shipments = shipments.Where(x =>
                {
                    var order = _orderService.GetOrderById(x.OrderId);
                    return _workContext.HasAccessToShipment(order, x);
                }).ToList();
            }

            foreach (var shipment in shipments)
            {
                try
                {
                    _orderProcessingService.Ship(shipment, true);
                }
                catch
                {
                    //ignore any exception
                }
            }

            return Json(new { Result = true });
        }

        [HttpPost]
        public IActionResult SetAsDeliveredSelected(ICollection<string> selectedIds)
        {
            var shipments = new List<Shipment>();
            if (selectedIds != null)
            {
                shipments.AddRange(_shipmentService.GetShipmentsByIds(selectedIds.ToArray()));
            }
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                shipments = shipments.Where(x =>
                {
                    var order = _orderService.GetOrderById(x.OrderId);
                    return _workContext.HasAccessToShipment(order, x);
                }).ToList();
            }

            foreach (var shipment in shipments)
            {
                try
                {
                    _orderProcessingService.Deliver(shipment, true);
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
