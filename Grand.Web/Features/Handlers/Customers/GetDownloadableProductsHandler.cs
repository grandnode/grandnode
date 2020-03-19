using Grand.Services.Catalog;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Orders;
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
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IDownloadService _downloadService;

        public GetDownloadableProductsHandler(
            IOrderService orderService,
            IProductService productService,
            IDateTimeHelper dateTimeHelper,
            IDownloadService downloadService)
        {
            _orderService = orderService;
            _productService = productService;
            _dateTimeHelper = dateTimeHelper;
            _downloadService = downloadService;
        }

        public async Task<CustomerDownloadableProductsModel> Handle(GetDownloadableProducts request, CancellationToken cancellationToken)
        {
            var model = new CustomerDownloadableProductsModel();
            var items = await _orderService.GetAllOrderItems(null, request.Customer.Id, null, null,
                null, null, null, true);

            foreach (var item in items)
            {
                var order = await _orderService.GetOrderByOrderItemId(item.Id);
                var product = await _productService.GetProductByIdIncludeArch(item.ProductId);
                var itemModel = new CustomerDownloadableProductsModel.DownloadableProductsModel {
                    OrderItemGuid = item.OrderItemGuid,
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(item.CreatedOnUtc, DateTimeKind.Utc),
                    ProductName = product.GetLocalized(x => x.Name, request.Language.Id),
                    ProductSeName = product.GetSeName(request.Language.Id),
                    ProductAttributes = item.AttributeDescription,
                    ProductId = item.ProductId
                };
                model.Items.Add(itemModel);

                if (_downloadService.IsDownloadAllowed(order, item, product))
                    itemModel.DownloadId = product.DownloadId;

                if (_downloadService.IsLicenseDownloadAllowed(order, item, product))
                    itemModel.LicenseId = !string.IsNullOrEmpty(item.LicenseDownloadId) ? item.LicenseDownloadId : "";
            }
            return model;
        }
    }
}
