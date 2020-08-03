using Grand.Services.Catalog;
using Grand.Services.Orders;
using Grand.Web.Features.Models.Customers;
using Grand.Web.Models.Customer;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Customers
{
    public class GetUserAgreementHandler : IRequestHandler<GetUserAgreement, UserAgreementModel>
    {
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;

        public GetUserAgreementHandler(IOrderService orderService, 
            IProductService productService)
        {
            _orderService = orderService;
            _productService = productService;
        }

        public async Task<UserAgreementModel> Handle(GetUserAgreement request, CancellationToken cancellationToken)
        {
            var orderItem = await _orderService.GetOrderItemByGuid(request.OrderItemId);
            if (orderItem == null)
                return null;

            var product = await _productService.GetProductById(orderItem.ProductId);
            if (product == null || !product.HasUserAgreement)
                return null;

            var model = new UserAgreementModel();
            model.UserAgreementText = product.UserAgreementText;
            model.OrderItemGuid = request.OrderItemId;
            return model;

        }
    }
}
