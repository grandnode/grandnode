using Grand.Domain.Messages;
using Grand.Domain.Orders;
using Grand.Services.Catalog;
using Grand.Services.Commands.Models.Messages;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Messages.DotLiquidDrops;
using MediatR;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Commands.Handlers.Messages
{
    public class GetShoppingCartTokensCommandHandler : IRequestHandler<GetShoppingCartTokensCommand, LiquidShoppingCart>
    {
        private readonly IProductService _productService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly ILocalizationService _localizationService;
        private readonly IPictureService _pictureService;
        private readonly MessageTemplatesSettings _templatesSettings;

        public GetShoppingCartTokensCommandHandler(
            IProductService productService, 
            IProductAttributeFormatter productAttributeFormatter, 
            ILocalizationService localizationService, 
            IPictureService pictureService, 
            MessageTemplatesSettings templatesSettings)
        {
            _productService = productService;
            _productAttributeFormatter = productAttributeFormatter;
            _localizationService = localizationService;
            _pictureService = pictureService;
            _templatesSettings = templatesSettings;
        }

        public async Task<LiquidShoppingCart> Handle(GetShoppingCartTokensCommand request, CancellationToken cancellationToken)
        {
            var liquidShoppingCart = new LiquidShoppingCart(request.Customer, request.Store, request.Language, request.PersonalMessage, request.CustomerEmail);
            liquidShoppingCart.ShoppingCartProducts = await ShoppingCartWishListProductListToHtmlTable(true, false);
            liquidShoppingCart.ShoppingCartProductsWithPictures = await ShoppingCartWishListProductListToHtmlTable(true, true);
            liquidShoppingCart.WishlistProducts = await ShoppingCartWishListProductListToHtmlTable(false, false);
            liquidShoppingCart.WishlistProductsWithPictures = await ShoppingCartWishListProductListToHtmlTable(false, true);

            async Task<string> ShoppingCartWishListProductListToHtmlTable(bool cart, bool withPicture)
            {
                string result;

                var sb = new StringBuilder();
                sb.AppendLine("<table border=\"0\" style=\"width:100%;\">");

                #region Products
                sb.AppendLine(string.Format("<tr style=\"background-color:{0};text-align:center;\">", _templatesSettings.Color1));
                if (withPicture)
                    sb.AppendLine(string.Format("<th>{0}</th>", cart ? _localizationService.GetResource("Messages.ShoppingCart.Product(s).Picture", request.Language.Id) : _localizationService.GetResource("Messages.Wishlist.Product(s).Picture", request.Language.Id)));
                sb.AppendLine(string.Format("<th>{0}</th>", cart ? _localizationService.GetResource("Messages.ShoppingCart.Product(s).Name", request.Language.Id) : _localizationService.GetResource("Messages.Wishlist.Product(s).Name", request.Language.Id)));
                sb.AppendLine(string.Format("<th>{0}</th>", cart ? _localizationService.GetResource("Messages.ShoppingCart.Product(s).Quantity", request.Language.Id) : _localizationService.GetResource("Messages.Wishlist.Product(s).Quantity", request.Language.Id)));
                sb.AppendLine("</tr>");

                foreach (var item in cart ? request.Customer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart) :
                    request.Customer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.Wishlist))
                {
                    var product = await _productService.GetProductById(item.ProductId);
                    sb.AppendLine(string.Format("<tr style=\"background-color: {0};text-align: center;\">", _templatesSettings.Color2));
                    //product name
                    string productName = product.GetLocalized(x => x.Name, request.Language.Id);
                    if (withPicture)
                    {
                        string pictureUrl = "";
                        if (product.ProductPictures.Any())
                        {
                            pictureUrl = await _pictureService.GetPictureUrl(product.ProductPictures.OrderBy(x => x.DisplayOrder).FirstOrDefault().PictureId, _templatesSettings.PictureSize, storeLocation: request.Store.SslEnabled ? request.Store.SecureUrl : request.Store.Url);
                        }
                        sb.Append(string.Format("<td><img src=\"{0}\" alt=\"\"/></td>", pictureUrl));
                    }
                    sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + WebUtility.HtmlEncode(productName));
                    //attributes
                    if (!string.IsNullOrEmpty(item.AttributesXml))
                    {
                        sb.AppendLine("<br />");
                        string attributeDescription = await _productAttributeFormatter.FormatAttributes(product, item.AttributesXml, request.Customer);
                        sb.AppendLine(attributeDescription);
                    }
                    sb.AppendLine("</td>");

                    sb.AppendLine(string.Format("<td style=\"padding: 0.6em 0.4em;text-align: center;\">{0}</td>", item.Quantity));

                    sb.AppendLine("</tr>");
                }
                #endregion

                sb.AppendLine("</table>");
                result = sb.ToString();
                return result;
            }
            return liquidShoppingCart;
        }
    }
}
