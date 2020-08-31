using Grand.Domain.Customers;
using Grand.Services.Catalog;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Queries.Models.Orders;
using Grand.Services.Seo;
using Grand.Web.Features.Models.Customers;
using Grand.Web.Models.Customer;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Customers
{
    public class GetDownloadableProductsHandler : IRequestHandler<GetDownloadableProducts, CustomerDownloadableProductsModel>
    {
        private readonly IProductService _productService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IDownloadService _downloadService;
        private readonly IMediator _mediator;

        public GetDownloadableProductsHandler(
            IProductService productService,
            IDateTimeHelper dateTimeHelper,
            IDownloadService downloadService,
            IMediator mediator)
        {
            _productService = productService;
            _dateTimeHelper = dateTimeHelper;
            _downloadService = downloadService;
            _mediator = mediator;
        }

        public async Task<CustomerDownloadableProductsModel> Handle(GetDownloadableProducts request, CancellationToken cancellationToken)
        {
            var model = new CustomerDownloadableProductsModel();

            var query = new GetOrderQuery {
                StoreId = request.Store.Id
            };

            if (!request.Customer.IsOwner())
                query.CustomerId = request.Customer.Id;
            else
                query.OwnerId = request.Customer.Id;

            var orders = await _mediator.Send(query);

            foreach (var order in orders)
            {
                foreach (var orderitem in order.OrderItems)
                {
                    var product = await _productService.GetProductByIdIncludeArch(orderitem.ProductId);
                    if (product != null && product.IsDownload)
                    {
                        var itemModel = new CustomerDownloadableProductsModel.DownloadableProductsModel {
                            OrderItemGuid = orderitem.OrderItemGuid,
                            OrderId = order.Id,
                            OrderNumber = order.OrderNumber,
                            CreatedOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc),
                            ProductName = product.GetLocalized(x => x.Name, request.Language.Id),
                            ProductSeName = product.GetSeName(request.Language.Id),
                            ProductAttributes = orderitem.AttributeDescription,
                            ProductId = orderitem.ProductId
                        };
                        model.Items.Add(itemModel);

                        if (_downloadService.IsDownloadAllowed(order, orderitem, product))
                            itemModel.DownloadId = product.DownloadId;

                        if (_downloadService.IsLicenseDownloadAllowed(order, orderitem, product))
                            itemModel.LicenseId = !string.IsNullOrEmpty(orderitem.LicenseDownloadId) ? orderitem.LicenseDownloadId : "";
                    }
                }
            }

            return model;
        }
    }
}
