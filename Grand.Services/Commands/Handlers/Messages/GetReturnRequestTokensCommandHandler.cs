using Grand.Domain.Messages;
using Grand.Domain.Tax;
using Grand.Services.Catalog;
using Grand.Services.Commands.Models.Messages;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Messages.DotLiquidDrops;
using MediatR;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Commands.Handlers.Messages
{
    public class GetReturnRequestTokensCommandHandler : IRequestHandler<GetReturnRequestTokensCommand, LiquidReturnRequest>
    {
        private readonly ILocalizationService _localizationService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICountryService _countryService;
        private readonly IProductService _productService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;

        private readonly MessageTemplatesSettings _templatesSettings;

        public GetReturnRequestTokensCommandHandler(
            ILocalizationService localizationService,
            IStateProvinceService stateProvinceService,
            ICountryService countryService,
            IProductService productService,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter,
            MessageTemplatesSettings templatesSettings)
        {
            _localizationService = localizationService;
            _stateProvinceService = stateProvinceService;
            _countryService = countryService;
            _productService = productService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _templatesSettings = templatesSettings;
        }

        public async Task<LiquidReturnRequest> Handle(GetReturnRequestTokensCommand request, CancellationToken cancellationToken)
        {
            var liquidReturnRequest = new LiquidReturnRequest(request.ReturnRequest, request.Store, request.Order, request.ReturnRequestNote);

            liquidReturnRequest.Status = request.ReturnRequest.ReturnRequestStatus.GetLocalizedEnum(_localizationService, request.Language.Id);
            liquidReturnRequest.Products = await ProductListToHtmlTable();
            liquidReturnRequest.PickupAddressStateProvince =
                            !string.IsNullOrEmpty(request.ReturnRequest.PickupAddress.StateProvinceId) ?
                            (await _stateProvinceService.GetStateProvinceById(request.ReturnRequest.PickupAddress.StateProvinceId))?.GetLocalized(x => x.Name, request.Language.Id) : "";

            liquidReturnRequest.PickupAddressCountry =
                            !string.IsNullOrEmpty(request.ReturnRequest.PickupAddress.CountryId) ?
                            (await _countryService.GetCountryById(request.ReturnRequest.PickupAddress.CountryId))?.GetLocalized(x => x.Name, request.Language.Id) : "";

            async Task<string> ProductListToHtmlTable()
            {
                var sb = new StringBuilder();
                sb.AppendLine("<table border=\"0\" style=\"width:100%;\">");

                sb.AppendLine(string.Format("<tr style=\"text-align:center;\">"));
                sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).Name")));
                sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).Price")));
                sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).Quantity")));
                sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).ReturnReason")));
                sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).ReturnAction")));
                sb.AppendLine("</tr>");

                var currency = await _currencyService.GetCurrencyByCode(request.Order.CustomerCurrencyCode);
                foreach (var rrItem in request.ReturnRequest.ReturnRequestItems)
                {
                    var orderItem = request.Order.OrderItems.Where(x => x.Id == rrItem.OrderItemId).First();

                    sb.AppendLine(string.Format("<tr style=\"background-color: {0};text-align: center;\">", _templatesSettings.Color2));
                    string productName = (await _productService.GetProductById(orderItem.ProductId))?.GetLocalized(x => x.Name, request.Order.CustomerLanguageId);

                    sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + WebUtility.HtmlEncode(productName));

                    sb.AppendLine("</td>");

                    string unitPriceStr;
                    if (request.Order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                    {
                        //including tax
                        var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, request.Order.CurrencyRate);
                        unitPriceStr = _priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency, true, currency, request.Language, true);
                    }
                    else
                    {
                        //excluding tax
                        var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, request.Order.CurrencyRate);
                        unitPriceStr = _priceFormatter.FormatPrice(unitPriceExclTaxInCustomerCurrency, true, currency, request.Language, false);
                    }
                    sb.AppendLine(string.Format("<td style=\"padding: 0.6em 0.4em;text-align: right;\">{0}</td>", unitPriceStr));
                    sb.AppendLine(string.Format("<td style=\"padding: 0.6em 0.4em;text-align: center;\">{0}</td>", orderItem.Quantity));
                    sb.AppendLine(string.Format("<td style=\"padding: 0.6em 0.4em;text-align: center;\">{0}</td>", rrItem.ReasonForReturn));
                    sb.AppendLine(string.Format("<td style=\"padding: 0.6em 0.4em;text-align: center;\">{0}</td>", rrItem.RequestedAction));
                }

                sb.AppendLine("</table>");
                return sb.ToString();
            }

            return liquidReturnRequest;

        }
    }
}
