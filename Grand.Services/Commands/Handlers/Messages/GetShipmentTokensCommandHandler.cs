using Grand.Services.Catalog;
using Grand.Services.Commands.Models.Messages;
using Grand.Services.Messages.DotLiquidDrops;
using MediatR;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Commands.Handlers.Messages
{
    public class GetShipmentTokensCommandHandler : IRequestHandler<GetShipmentTokensCommand, LiquidShipment>
    {
        private readonly IProductService _productService;
        private readonly IProductAttributeParser _productAttributeParser;

        public GetShipmentTokensCommandHandler(
            IProductService productService,
            IProductAttributeParser productAttributeParser)
        {
            _productService = productService;
            _productAttributeParser = productAttributeParser;
        }

        public async Task<LiquidShipment> Handle(GetShipmentTokensCommand request, CancellationToken cancellationToken)
        {
            var liquidShipment = new LiquidShipment(request.Shipment, request.Order, request.Store, request.Language);
            foreach (var shipmentItem in request.Shipment.ShipmentItems)
            {
                var orderitem = request.Order.OrderItems.FirstOrDefault(x => x.Id == shipmentItem.OrderItemId);
                var product = await _productService.GetProductById(shipmentItem.ProductId);
                var liquidshipmentItems = new LiquidShipmentItem(shipmentItem, request.Shipment, request.Order, orderitem, product, request.Language);
                string sku = "";
                if (product != null)
                    sku = product.FormatSku(orderitem.AttributesXml, _productAttributeParser);

                liquidshipmentItems.ProductSku = WebUtility.HtmlEncode(sku);

                liquidShipment.ShipmentItems.Add(liquidshipmentItems);
            }
            return liquidShipment;
        }
    }
}
