﻿using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Shipping;
using Grand.Services.Catalog;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Seo;
using Grand.Services.Shipping;
using Grand.Web.Features.Models.Orders;
using Grand.Web.Interfaces;
using Grand.Web.Models.Common;
using Grand.Web.Models.Orders;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Orders
{
    public class GetShipmentDetailsHandler : IRequestHandler<GetShipmentDetails, ShipmentDetailsModel>
    {
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IShippingService _shippingService;
        private readonly IProductService _productService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IAddressViewModelService _addressViewModelService;

        private readonly ShippingSettings _shippingSettings;
        private readonly CatalogSettings _catalogSettings;


        public GetShipmentDetailsHandler(IDateTimeHelper dateTimeHelper, IProductService productService, IProductAttributeParser productAttributeParser,
            IShippingService shippingService, IAddressViewModelService addressViewModelService,
            ShippingSettings shippingSettings, CatalogSettings catalogSettings)
        {
            _dateTimeHelper = dateTimeHelper;
            _productService = productService;
            _productAttributeParser = productAttributeParser;
            _shippingService = shippingService;
            _addressViewModelService = addressViewModelService;

            _shippingSettings = shippingSettings;
            _catalogSettings = catalogSettings;
        }

        public async Task<ShipmentDetailsModel> Handle(GetShipmentDetails request, CancellationToken cancellationToken)
        {
            if (request.Shipment == null)
                throw new ArgumentNullException("shipment");

            var model = new ShipmentDetailsModel();

            model.Id = request.Shipment.Id;
            model.ShipmentNumber = request.Shipment.ShipmentNumber;
            if (request.Shipment.ShippedDateUtc.HasValue)
                model.ShippedDate = _dateTimeHelper.ConvertToUserTime(request.Shipment.ShippedDateUtc.Value, DateTimeKind.Utc);
            if (request.Shipment.DeliveryDateUtc.HasValue)
                model.DeliveryDate = _dateTimeHelper.ConvertToUserTime(request.Shipment.DeliveryDateUtc.Value, DateTimeKind.Utc);

            //tracking number and shipment information
            if (!string.IsNullOrEmpty(request.Shipment.TrackingNumber))
            {
                model.TrackingNumber = request.Shipment.TrackingNumber;
                var srcm = _shippingService.LoadShippingRateComputationMethodBySystemName(request.Order.ShippingRateComputationMethodSystemName);
                if (srcm != null &&
                    srcm.PluginDescriptor.Installed &&
                    srcm.IsShippingRateComputationMethodActive(_shippingSettings))
                {
                    var shipmentTracker = srcm.ShipmentTracker;
                    if (shipmentTracker != null)
                    {
                        model.TrackingNumberUrl = await shipmentTracker.GetUrl(request.Shipment.TrackingNumber);
                        if (_shippingSettings.DisplayShipmentEventsToCustomers)
                        {
                            var shipmentEvents = await shipmentTracker.GetShipmentEvents(request.Shipment.TrackingNumber);
                            if (shipmentEvents != null)
                            {
                                foreach (var shipmentEvent in shipmentEvents)
                                {
                                    var shipmentStatusEventModel = new ShipmentDetailsModel.ShipmentStatusEventModel();
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

            //products in this shipment
            model.ShowSku = _catalogSettings.ShowSkuOnProductDetailsPage;
            foreach (var shipmentItem in request.Shipment.ShipmentItems)
            {
                var orderItem = request.Order.OrderItems.Where(x => x.Id == shipmentItem.OrderItemId).FirstOrDefault();
                if (orderItem == null)
                    continue;
                var product = await _productService.GetProductByIdIncludeArch(orderItem.ProductId);
                var shipmentItemModel = new ShipmentDetailsModel.ShipmentItemModel {
                    Id = shipmentItem.Id,
                    Sku = product.FormatSku(orderItem.AttributesXml, _productAttributeParser),
                    ProductId = orderItem.ProductId,
                    ProductName = product.GetLocalized(x => x.Name, request.Language.Id),
                    ProductSeName = product.GetSeName(request.Language.Id),
                    AttributeInfo = orderItem.AttributeDescription,
                    QuantityOrdered = orderItem.Quantity,
                    QuantityShipped = shipmentItem.Quantity,
                };

                model.Items.Add(shipmentItemModel);
            }

            //order details model
            model.Order = await PrepareOrderModel(request.Order);

            return model;
        }

        private async Task<ShipmentDetailsModel.OrderModel> PrepareOrderModel(Order order)
        {
            var model = new ShipmentDetailsModel.OrderModel();
            model.GenericAttributes = order.GenericAttributes;
            model.Id = order.Id;
            model.OrderNumber = order.OrderNumber;
            model.ShippingMethod = order.ShippingMethod;
            model.PickUpInStore = order.PickUpInStore;
            if (!order.PickUpInStore)
            {
                model.ShippingAddress = new AddressModel();
                await _addressViewModelService.PrepareModel(model: model.ShippingAddress,
                    address: order.ShippingAddress,
                    excludeProperties: false);
            }
            else
            {
                if (order.PickupPoint != null)
                {
                    if (order.PickupPoint.Address != null)
                    {
                        model.PickupAddress = new AddressModel();
                        await _addressViewModelService.PrepareModel(model: model.PickupAddress,
                            address: order.PickupPoint.Address,
                            excludeProperties: false);
                    }
                }
            }

            return model;
        }

    }
}
