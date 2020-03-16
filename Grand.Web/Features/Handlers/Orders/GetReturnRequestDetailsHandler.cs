using Grand.Core;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Tax;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Seo;
using Grand.Web.Features.Models.Orders;
using Grand.Web.Interfaces;
using Grand.Web.Models.Orders;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Orders
{
    public class GetReturnRequestDetailsHandler : IRequestHandler<GetReturnRequestDetails, ReturnRequestDetailsModel>
    {
        private readonly IAddressViewModelService _addressViewModelService;
        private readonly IProductService _productService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IWorkContext _workContext;

        private readonly OrderSettings _orderSettings;

        public GetReturnRequestDetailsHandler(IAddressViewModelService addressViewModelService,
            IProductService productService,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter,
            IWorkContext workContext,
            OrderSettings orderSettings)
        {
            _addressViewModelService = addressViewModelService;
            _productService = productService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _workContext = workContext;
            _orderSettings = orderSettings;
        }

        public async Task<ReturnRequestDetailsModel> Handle(GetReturnRequestDetails request, CancellationToken cancellationToken)
        {
            var model = new ReturnRequestDetailsModel();
            model.Comments = request.ReturnRequest.CustomerComments;
            model.ReturnNumber = request.ReturnRequest.ReturnNumber;
            model.ReturnRequestStatus = request.ReturnRequest.ReturnRequestStatus;
            model.CreatedOnUtc = request.ReturnRequest.CreatedOnUtc;
            model.ShowPickupAddress = _orderSettings.ReturnRequests_AllowToSpecifyPickupAddress;
            model.ShowPickupDate = _orderSettings.ReturnRequests_AllowToSpecifyPickupDate;
            model.PickupDate = request.ReturnRequest.PickupDate;
            await _addressViewModelService.PrepareModel(model: model.PickupAddress, address: request.ReturnRequest.PickupAddress, excludeProperties: false);

            foreach (var item in request.ReturnRequest.ReturnRequestItems)
            {
                var orderItem = request.Order.OrderItems.Where(x => x.Id == item.OrderItemId).FirstOrDefault();
                var product = await _productService.GetProductByIdIncludeArch(orderItem.ProductId);

                string unitPrice = string.Empty;
                if (request.Order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, request.Order.CurrencyRate);
                    unitPrice = _priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency);
                }
                else
                {
                    //excluding tax
                    var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, request.Order.CurrencyRate);
                    unitPrice = _priceFormatter.FormatPrice(unitPriceExclTaxInCustomerCurrency);
                }

                model.ReturnRequestItems.Add(new ReturnRequestDetailsModel.ReturnRequestItemModel {
                    OrderItemId = item.OrderItemId,
                    Quantity = item.Quantity,
                    ReasonForReturn = item.ReasonForReturn,
                    RequestedAction = item.RequestedAction,
                    ProductName = product.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                    ProductSeName = product.GetSeName(_workContext.WorkingLanguage.Id),
                    ProductPrice = unitPrice
                });
            }
            return model;
        }
    }
}
