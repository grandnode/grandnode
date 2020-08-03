using Grand.Domain.Catalog;
using Grand.Domain.Shipping;
using Grand.Services.Catalog;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Seo;
using Grand.Services.Shipping;
using Grand.Web.Features.Models.Common;
using Grand.Web.Features.Models.Orders;
using Grand.Web.Models.Orders;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Orders
{
    public class GetShipmentDetailsHandler : IRequestHandler<GetShipmentDetails, ShipmentDetailsModel>
    {
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IShippingService _shippingService;
        private readonly IShipmentService _shipmentService; 
        private readonly IProductService _productService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IMediator _mediator;

        private readonly ShippingSettings _shippingSettings;
        private readonly CatalogSettings _catalogSettings;


        public GetShipmentDetailsHandler(
            IDateTimeHelper dateTimeHelper, 
            IProductService productService, 
            IProductAttributeParser productAttributeParser,
            IShippingService shippingService,
            IShipmentService shipmentService,
            IMediator mediator,
            ShippingSettings shippingSettings, 
            CatalogSettings catalogSettings)
        {
            _dateTimeHelper = dateTimeHelper;
            _productService = productService;
            _productAttributeParser = productAttributeParser;
            _shippingService = shippingService;
            _shipmentService = shipmentService;
            _mediator = mediator;
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

            //shipment notes 
            model.ShipmentNotes = await PrepareShipmentNotesModel(request);
            //order details model
            model.Order = await PrepareOrderModel(request);

            return model;
        }

        private async Task<IList<ShipmentDetailsModel.ShipmentNote>> PrepareShipmentNotesModel(GetShipmentDetails request)
        {
            var notes = new List<ShipmentDetailsModel.ShipmentNote>();
            foreach (var shipmentNote in (await  _shipmentService.GetShipmentNotes(request.Shipment.Id))
                .Where(on => on.DisplayToCustomer)
                .OrderByDescending(on => on.CreatedOnUtc)
                .ToList())
            {
                notes.Add(new ShipmentDetailsModel.ShipmentNote {
                    Id = shipmentNote.Id,
                    ShipmentId = shipmentNote.ShipmentId,
                    HasDownload = !string.IsNullOrEmpty(shipmentNote.DownloadId),
                    Note = shipmentNote.FormatOrderNoteText(),
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(shipmentNote.CreatedOnUtc, DateTimeKind.Utc)
                });
            }
            return notes;
        }
        private async Task<ShipmentDetailsModel.OrderModel> PrepareOrderModel(GetShipmentDetails request)
        {
            var model = new ShipmentDetailsModel.OrderModel();
            var order = request.Order;
            model.GenericAttributes = order.GenericAttributes;
            model.Id = order.Id;
            model.OrderNumber = order.OrderNumber;
            model.OrderCode = order.Code;
            model.ShippingMethod = order.ShippingMethod;
            model.PickUpInStore = order.PickUpInStore;
            if (!order.PickUpInStore)
            {
                model.ShippingAddress = await _mediator.Send(new GetAddressModel() {
                    Language = request.Language,
                    Address = order.ShippingAddress,
                    ExcludeProperties = false,
                });
            }
            else
            {
                if (order.PickupPoint != null)
                {
                    if (order.PickupPoint.Address != null)
                    {
                        model.PickupAddress = await _mediator.Send(new GetAddressModel() {
                            Language = request.Language,
                            Address = order.PickupPoint.Address,
                            ExcludeProperties = false,
                        });
                    }
                }
            }
            return model;
        }

        
    }
}
