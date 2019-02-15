using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Shipping;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Orders;
using Grand.Services.Shipping;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Orders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Areas.Admin.Services
{
    public partial class ShipmentViewModelService : IShipmentViewModelService
    {
        private readonly IOrderService _orderService;
        private readonly IWorkContext _workContext;
        private readonly IProductService _productService;
        private readonly IShipmentService _shipmentService;
        private readonly IShippingService _shippingService;
        private readonly IMeasureService _measureService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ICountryService _countryService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;

        private readonly MeasureSettings _measureSettings;
        private readonly ShippingSettings _shippingSettings;

        public ShipmentViewModelService(
            IOrderService orderService,
            IWorkContext workContext,
            IProductService productService,
            IShipmentService shipmentService,
            IShippingService shippingService,
            IMeasureService measureService,
            IDateTimeHelper dateTimeHelper,
            IProductAttributeParser productAttributeParser,
            ICountryService countryService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            MeasureSettings measureSettings,
            ShippingSettings shippingSettings)
        {
            _orderService = orderService;
            _workContext = workContext;
            _productService = productService;
            _shipmentService = shipmentService;
            _shippingService = shippingService;
            _measureService = measureService;
            _dateTimeHelper = dateTimeHelper;
            _productAttributeParser = productAttributeParser;
            _countryService = countryService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _measureSettings = measureSettings;
            _shippingSettings = shippingSettings;
        }

        public virtual ShipmentModel PrepareShipmentModel(Shipment shipment, bool prepareProducts, bool prepareShipmentEvent = false)
        {
            //measures
            var baseWeight = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId);
            var baseWeightIn = baseWeight != null ? baseWeight.Name : "";
            var baseDimension = _measureService.GetMeasureDimensionById(_measureSettings.BaseDimensionId);
            var baseDimensionIn = baseDimension != null ? baseDimension.Name : "";
            var order = _orderService.GetOrderById(shipment.OrderId);

            var model = new ShipmentModel
            {
                Id = shipment.Id,
                ShipmentNumber = shipment.ShipmentNumber,
                OrderId = shipment.OrderId,
                OrderNumber = order != null ? order.OrderNumber : 0,
                TrackingNumber = shipment.TrackingNumber,
                TotalWeight = shipment.TotalWeight.HasValue ? string.Format("{0:F2} [{1}]", shipment.TotalWeight, baseWeightIn) : "",
                ShippedDate = shipment.ShippedDateUtc.HasValue ? _dateTimeHelper.ConvertToUserTime(shipment.ShippedDateUtc.Value, DateTimeKind.Utc) : new DateTime?(),
                ShippedDateUtc = shipment.ShippedDateUtc,
                CanShip = !shipment.ShippedDateUtc.HasValue,
                DeliveryDate = shipment.DeliveryDateUtc.HasValue ? _dateTimeHelper.ConvertToUserTime(shipment.DeliveryDateUtc.Value, DateTimeKind.Utc) : new DateTime?(),
                DeliveryDateUtc = shipment.DeliveryDateUtc,
                CanDeliver = shipment.ShippedDateUtc.HasValue && !shipment.DeliveryDateUtc.HasValue,
                AdminComment = shipment.AdminComment,
            };

            if (prepareProducts)
            {
                foreach (var shipmentItem in shipment.ShipmentItems)
                {

                    var orderItem = order.OrderItems.Where(x => x.Id == shipmentItem.OrderItemId).FirstOrDefault();
                    if (orderItem == null)
                        continue;

                    //quantities
                    var qtyInThisShipment = shipmentItem.Quantity;
                    var maxQtyToAdd = orderItem.GetTotalNumberOfItemsCanBeAddedToShipment();
                    var qtyOrdered = shipmentItem.Quantity;
                    var qtyInAllShipments = orderItem.GetTotalNumberOfItemsInAllShipment();
                    var product = _productService.GetProductByIdIncludeArch(orderItem.ProductId);
                    if (product != null)
                    {
                        var warehouse = _shippingService.GetWarehouseById(shipmentItem.WarehouseId);
                        var shipmentItemModel = new ShipmentModel.ShipmentItemModel
                        {
                            Id = shipmentItem.Id,
                            OrderItemId = orderItem.Id,
                            ProductId = orderItem.ProductId,
                            ProductName = product.Name,
                            Sku = product.FormatSku(orderItem.AttributesXml, _productAttributeParser),
                            AttributeInfo = orderItem.AttributeDescription,
                            ShippedFromWarehouse = warehouse != null ? warehouse.Name : null,
                            ShipSeparately = product.ShipSeparately,
                            ItemWeight = orderItem.ItemWeight.HasValue ? string.Format("{0:F2} [{1}]", orderItem.ItemWeight, baseWeightIn) : "",
                            ItemDimensions = string.Format("{0:F2} x {1:F2} x {2:F2} [{3}]", product.Length, product.Width, product.Height, baseDimensionIn),
                            QuantityOrdered = qtyOrdered,
                            QuantityInThisShipment = qtyInThisShipment,
                            QuantityInAllShipments = qtyInAllShipments,
                            QuantityToAdd = maxQtyToAdd,
                        };

                        model.Items.Add(shipmentItemModel);
                    }
                }
            }

            if (prepareShipmentEvent && !String.IsNullOrEmpty(shipment.TrackingNumber))
            {
                var srcm = _shippingService.LoadShippingRateComputationMethodBySystemName(order.ShippingRateComputationMethodSystemName);
                if (srcm != null &&
                    srcm.PluginDescriptor.Installed &&
                    srcm.IsShippingRateComputationMethodActive(_shippingSettings))
                {
                    var shipmentTracker = srcm.ShipmentTracker;
                    if (shipmentTracker != null)
                    {
                        model.TrackingNumberUrl = shipmentTracker.GetUrl(shipment.TrackingNumber);
                        if (_shippingSettings.DisplayShipmentEventsToStoreOwner)
                        {
                            var shipmentEvents = shipmentTracker.GetShipmentEvents(shipment.TrackingNumber);
                            if (shipmentEvents != null)
                            {
                                foreach (var shipmentEvent in shipmentEvents)
                                {
                                    var shipmentStatusEventModel = new ShipmentModel.ShipmentStatusEventModel();
                                    var shipmentEventCountry = _countryService.GetCountryByTwoLetterIsoCode(shipmentEvent.CountryCode);
                                    shipmentStatusEventModel.Country = shipmentEventCountry != null
                                        ? shipmentEventCountry.GetLocalized(x => x.Name)
                                        : shipmentEvent.CountryCode;
                                    shipmentStatusEventModel.Date = shipmentEvent.Date;
                                    shipmentStatusEventModel.EventName = shipmentEvent.EventName;
                                    shipmentStatusEventModel.Location = shipmentEvent.Location;
                                    model.ShipmentStatusEvents.Add(shipmentStatusEventModel);
                                }
                            }
                        }
                    }
                }
            }

            return model;
        }


        public virtual int GetStockQty(Product product, string warehouseId)
        {
            List<int> _qty = new List<int>();
            foreach (var item in product.BundleProducts)
            {
                var p1 = _productService.GetProductById(item.ProductId);
                if (p1.UseMultipleWarehouses)
                {
                    var stock = p1.ProductWarehouseInventory.FirstOrDefault(x => x.WarehouseId == warehouseId);
                    if (stock != null)
                    {
                        _qty.Add(stock.StockQuantity / item.Quantity);
                    }
                }
                else
                {
                    _qty.Add(p1.StockQuantity / item.Quantity);
                }
            }

            return _qty.Count > 0 ? _qty.Min() : 0;
        }

        public virtual int GetPlannedQty(Product product, string warehouseId)
        {
            List<int> _qty = new List<int>();
            foreach (var item in product.BundleProducts)
            {
                var p1 = _productService.GetProductById(item.ProductId);
                if (p1.UseMultipleWarehouses)
                {
                    var stock = p1.ProductWarehouseInventory.FirstOrDefault(x => x.WarehouseId == warehouseId);
                    if (stock != null)
                    {
                        _qty.Add((stock.StockQuantity - stock.ReservedQuantity) / item.Quantity);
                    }
                }
            }
            return _qty.Count > 0 ? _qty.Min() : 0;
        }

        public virtual int GetReservedQty(Product product, string warehouseId)
        {
            List<int> _qty = new List<int>();
            foreach (var item in product.BundleProducts)
            {
                var p1 = _productService.GetProductById(item.ProductId);
                if (p1.UseMultipleWarehouses)
                {
                    var stock = p1.ProductWarehouseInventory.FirstOrDefault(x => x.WarehouseId == warehouseId);
                    if (stock != null)
                    {
                        _qty.Add(stock.ReservedQuantity / item.Quantity);
                    }
                }
            }
            return _qty.Count > 0 ? _qty.Min() : 0;
        }
        public virtual void LogShipment(string shipmentId, string message)
        {
            _customerActivityService.InsertActivity("EditShipment", shipmentId, message);
        }
        public virtual (IEnumerable<Shipment> shipments, int totalCount) PrepareShipments(ShipmentListModel model, int pageIndex, int pageSize)
        {
            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            //a vendor should have access only to his products
            string vendorId = "";
            if (_workContext.CurrentVendor != null)
                vendorId = _workContext.CurrentVendor.Id;

            //load shipments
            var shipments = _shipmentService.GetAllShipments(vendorId: vendorId,
                warehouseId: model.WarehouseId,
                shippingCountryId: model.CountryId,
                shippingStateId: model.StateProvinceId,
                shippingCity: model.City,
                trackingNumber: model.TrackingNumber,
                loadNotShipped: model.LoadNotShipped,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                pageIndex: pageIndex - 1,
                pageSize: pageSize);

            return (shipments.ToList(), shipments.TotalCount);
        }
        public virtual ShipmentListModel PrepareShipmentListModel()
        {
            var model = new ShipmentListModel();
            //countries
            model.AvailableCountries.Add(new SelectListItem { Text = "*", Value = "" });
            foreach (var c in _countryService.GetAllCountries(showHidden: true))
                model.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
            //states
            model.AvailableStates.Add(new SelectListItem { Text = "*", Value = "" });

            //warehouses
            model.AvailableWarehouses.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            foreach (var w in _shippingService.GetAllWarehouses())
                model.AvailableWarehouses.Add(new SelectListItem { Text = w.Name, Value = w.Id.ToString() });

            return model;
        }
        public virtual ShipmentModel PrepareShipmentModel(Order order)
        {
            var model = new ShipmentModel
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber
            };

            //measures
            var baseWeight = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId);
            var baseWeightIn = baseWeight != null ? baseWeight.Name : "";
            var baseDimension = _measureService.GetMeasureDimensionById(_measureSettings.BaseDimensionId);
            var baseDimensionIn = baseDimension != null ? baseDimension.Name : "";

            var orderItems = order.OrderItems;
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                orderItems = orderItems.Where(_workContext.HasAccessToOrderItem).ToList();
            }

            foreach (var orderItem in orderItems)
            {
                var product = _productService.GetProductByIdIncludeArch(orderItem.ProductId);
                //we can ship only shippable products
                if (!product.IsShipEnabled)
                    continue;

                //quantities
                var qtyInThisShipment = 0;
                var maxQtyToAdd = orderItem.GetTotalNumberOfItemsCanBeAddedToShipment();
                var qtyOrdered = orderItem.Quantity;
                var qtyInAllShipments = orderItem.GetTotalNumberOfItemsInAllShipment();

                //ensure that this product can be added to a shipment
                if (maxQtyToAdd <= 0)
                    continue;

                var shipmentItemModel = new ShipmentModel.ShipmentItemModel
                {
                    OrderItemId = orderItem.Id,
                    ProductId = orderItem.ProductId,
                    ProductName = product.Name,
                    WarehouseId = orderItem.WarehouseId,
                    Sku = product.FormatSku(orderItem.AttributesXml, _productAttributeParser),
                    AttributeInfo = orderItem.AttributeDescription,
                    ShipSeparately = product.ShipSeparately,
                    ItemWeight = orderItem.ItemWeight.HasValue ? string.Format("{0:F2} [{1}]", orderItem.ItemWeight, baseWeightIn) : "",
                    ItemDimensions = string.Format("{0:F2} x {1:F2} x {2:F2} [{3}]", product.Length, product.Width, product.Height, baseDimensionIn),
                    QuantityOrdered = qtyOrdered,
                    QuantityInThisShipment = qtyInThisShipment,
                    QuantityInAllShipments = qtyInAllShipments,
                    QuantityToAdd = maxQtyToAdd,
                };

                if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                {
                    if (product.UseMultipleWarehouses)
                    {
                        //multiple warehouses supported
                        shipmentItemModel.AllowToChooseWarehouse = true;
                        foreach (var pwi in product.ProductWarehouseInventory
                            .OrderBy(w => w.WarehouseId).ToList())
                        {

                            var warehouse = _shippingService.GetWarehouseById(pwi.WarehouseId);
                            if (warehouse != null)
                            {
                                shipmentItemModel.AvailableWarehouses.Add(new ShipmentModel.ShipmentItemModel.WarehouseInfo
                                {
                                    WarehouseId = warehouse.Id,
                                    WarehouseName = warehouse.Name,
                                    StockQuantity = pwi.StockQuantity,
                                    ReservedQuantity = pwi.ReservedQuantity,
                                    PlannedQuantity = _shipmentService.GetQuantityInShipments(product, orderItem.AttributesXml, warehouse.Id, true, true)
                                });
                            }
                        }
                    }
                    else
                    {
                        //multiple warehouses are not supported
                        var warehouse = _shippingService.GetWarehouseById(product.WarehouseId);
                        if (warehouse != null)
                        {
                            shipmentItemModel.AvailableWarehouses.Add(new ShipmentModel.ShipmentItemModel.WarehouseInfo
                            {
                                WarehouseId = warehouse.Id,
                                WarehouseName = warehouse.Name,
                                StockQuantity = product.StockQuantity
                            });
                        }
                    }
                }

                if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
                {
                    if (product.UseMultipleWarehouses)
                    {
                        //multiple warehouses supported
                        shipmentItemModel.AllowToChooseWarehouse = true;
                        foreach (var pwi in product.ProductAttributeCombinations.FirstOrDefault(x => x.AttributesXml == orderItem.AttributesXml)?.WarehouseInventory
                            .OrderBy(w => w.WarehouseId).ToList())
                        {

                            var warehouse = _shippingService.GetWarehouseById(pwi.WarehouseId);
                            if (warehouse != null)
                            {
                                shipmentItemModel.AvailableWarehouses.Add(new ShipmentModel.ShipmentItemModel.WarehouseInfo
                                {
                                    WarehouseId = warehouse.Id,
                                    WarehouseName = warehouse.Name,
                                    StockQuantity = pwi.StockQuantity,
                                    ReservedQuantity = pwi.ReservedQuantity,
                                    PlannedQuantity = _shipmentService.GetQuantityInShipments(product, orderItem.AttributesXml, warehouse.Id, true, true)
                                });
                            }
                        }
                    }
                    else
                    {
                        //multiple warehouses are not supported
                        var warehouse = _shippingService.GetWarehouseById(product.WarehouseId);
                        if (warehouse != null)
                        {
                            shipmentItemModel.AvailableWarehouses.Add(new ShipmentModel.ShipmentItemModel.WarehouseInfo
                            {
                                WarehouseId = warehouse.Id,
                                WarehouseName = warehouse.Name,
                                StockQuantity = product.StockQuantity
                            });
                        }
                    }
                }

                if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByBundleProducts)
                {
                    if (!string.IsNullOrEmpty(orderItem.WarehouseId))
                    {
                        var warehouse = _shippingService.GetWarehouseById(product.WarehouseId);
                        if (warehouse != null)
                        {
                            shipmentItemModel.AvailableWarehouses.Add(new ShipmentModel.ShipmentItemModel.WarehouseInfo
                            {
                                WarehouseId = warehouse.Id,
                                WarehouseName = warehouse.Name,
                                StockQuantity = GetStockQty(product, orderItem.WarehouseId),
                                ReservedQuantity = GetReservedQty(product, orderItem.WarehouseId),
                                PlannedQuantity = GetPlannedQty(product, orderItem.WarehouseId)
                            });
                        }
                    }
                    else
                    {
                        shipmentItemModel.AllowToChooseWarehouse = true;
                        var warehouses = _shippingService.GetAllWarehouses();
                        foreach (var warehouse in warehouses)
                        {
                            shipmentItemModel.AvailableWarehouses.Add(new ShipmentModel.ShipmentItemModel.WarehouseInfo
                            {
                                WarehouseId = warehouse.Id,
                                WarehouseName = warehouse.Name,
                                StockQuantity = GetStockQty(product, warehouse.Id),
                                ReservedQuantity = GetReservedQty(product, warehouse.Id),
                                PlannedQuantity = GetPlannedQty(product, warehouse.Id)
                            });
                        }
                    }
                }

                model.Items.Add(shipmentItemModel);
            }
            return model;
        }

        public virtual (Shipment shipment, decimal? totalWeight) PrepareShipment(Order order, IList<OrderItem> orderItems, IFormCollection form)
        {
            Shipment shipment = null;
            decimal? totalWeight = null;
            foreach (var orderItem in orderItems)
            {
                //is shippable
                var product = _productService.GetProductByIdIncludeArch(orderItem.ProductId);
                if (!product.IsShipEnabled)
                    continue;

                //ensure that this product can be shipped (have at least one item to ship)
                var maxQtyToAdd = orderItem.GetTotalNumberOfItemsCanBeAddedToShipment();
                if (maxQtyToAdd <= 0)
                    continue;

                int qtyToAdd = 0; //parse quantity
                foreach (string formKey in form.Keys)
                    if (formKey.Equals(string.Format("qtyToAdd{0}", orderItem.Id), StringComparison.OrdinalIgnoreCase))
                    {
                        int.TryParse(form[formKey], out qtyToAdd);
                        break;
                    }

                string warehouseId = "";
                if (((product.ManageInventoryMethod == ManageInventoryMethod.ManageStock || product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes) &&
                    product.UseMultipleWarehouses) || (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByBundleProducts))
                {
                    //multiple warehouses supported
                    //warehouse is chosen by a store owner
                    foreach (string formKey in form.Keys)
                        if (formKey.Equals(string.Format("warehouse_{0}", orderItem.Id), StringComparison.OrdinalIgnoreCase))
                        {
                            warehouseId = form[formKey];
                            break;
                        }
                }
                else
                {
                    //multiple warehouses are not supported
                    warehouseId = product.WarehouseId;
                }

                foreach (string formKey in form.Keys)
                    if (formKey.Equals(string.Format("qtyToAdd{0}", orderItem.Id), StringComparison.OrdinalIgnoreCase))
                    {
                        int.TryParse(form[formKey], out qtyToAdd);
                        break;
                    }

                //validate quantity
                if (qtyToAdd <= 0)
                    continue;
                if (qtyToAdd > maxQtyToAdd)
                    qtyToAdd = maxQtyToAdd;

                //ok. we have at least one item. let's create a shipment (if it does not exist)

                var orderItemTotalWeight = orderItem.ItemWeight.HasValue ? orderItem.ItemWeight * qtyToAdd : null;
                if (orderItemTotalWeight.HasValue)
                {
                    if (!totalWeight.HasValue)
                        totalWeight = 0;
                    totalWeight += orderItemTotalWeight.Value;
                }
                if (shipment == null)
                {
                    var trackingNumber = form["TrackingNumber"];
                    var adminComment = form["AdminComment"];
                    shipment = new Shipment
                    {
                        OrderId = order.Id,
                        TrackingNumber = trackingNumber,
                        TotalWeight = null,
                        ShippedDateUtc = null,
                        DeliveryDateUtc = null,
                        AdminComment = adminComment,
                        CreatedOnUtc = DateTime.UtcNow,
                    };
                    if (_workContext.CurrentVendor != null)
                    {
                        shipment.VendorId = _workContext.CurrentVendor.Id;
                    }
                }
                //create a shipment item
                var shipmentItem = new ShipmentItem
                {
                    ProductId = orderItem.ProductId,
                    OrderItemId = orderItem.Id,
                    Quantity = qtyToAdd,
                    WarehouseId = warehouseId,
                    AttributeXML = orderItem.AttributesXml
                };
                shipment.ShipmentItems.Add(shipmentItem);
            }
            return (shipment, totalWeight);
        }
    }
}
