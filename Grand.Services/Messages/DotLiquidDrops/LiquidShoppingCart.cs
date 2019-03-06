using DotLiquid;
using Grand.Core;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Messages;
using Grand.Core.Domain.Orders;
using Grand.Core.Infrastructure;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidShoppingCart : Drop
    {
        private Customer _customer;
        private string _languageId;
        private string _personalMessage;
        private string _customerEmail;

        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        private readonly IStoreService _storeService;
        private readonly MessageTemplatesSettings _templatesSettings;

        public LiquidShoppingCart(Customer customer, string personalMessage = "", string customerEmail = "")
        {
            this._localizationService = EngineContext.Current.Resolve<ILocalizationService>();
            this._languageService = EngineContext.Current.Resolve<ILanguageService>();
            this._templatesSettings = EngineContext.Current.Resolve<MessageTemplatesSettings>();
            this._storeService = EngineContext.Current.Resolve<IStoreService>();

            this._customer = customer;

            string languageId = _languageService.GetAllLanguages().FirstOrDefault().Id;
            if (customer.GenericAttributes.FirstOrDefault(x => x.Key == "LanguageId") != null)
            {
                languageId = customer.GenericAttributes.FirstOrDefault(x => x.Key == "LanguageId").Value;
            }

            this._languageId = languageId;
            this._personalMessage = personalMessage;
            this._customerEmail = customerEmail;

            AdditionalTokens = new Dictionary<string, string>();
        }

        public string ShoppingCartProducts
        {
            get { return ShoppingCartWishListProductListToHtmlTable(_customer, _languageId, true, false); }
        }

        public string ShoppingCartProductsWithPictures
        {
            get { return ShoppingCartWishListProductListToHtmlTable(_customer, _languageId, true, true); }
        }

        public string WishlistProducts
        {
            get { return ShoppingCartWishListProductListToHtmlTable(_customer, _languageId, false, false); }
        }

        public string WishlistProductsWithPictures
        {
            get { return ShoppingCartWishListProductListToHtmlTable(_customer, _languageId, false, true); }
        }

        public string WishlistPersonalMessage
        {
            get { return _personalMessage; }
        }

        public string WishlistEmail
        {
            get { return _customerEmail; }
        }

        public string WishlistURLForCustomer
        {
            get
            {
                var wishlistUrl = string.Format("{0}wishlist/{1}", _storeService.GetStoreUrl(_customer.StoreId), _customer.CustomerGuid);
                return wishlistUrl;
            }
        }

        protected virtual string ShoppingCartWishListProductListToHtmlTable(Customer customer, string languageId, bool cart, bool withPicture)
        {
            string result;

            var sb = new StringBuilder();
            sb.AppendLine("<table border=\"0\" style=\"width:100%;\">");

            #region Products
            sb.AppendLine(string.Format("<tr style=\"background-color:{0};text-align:center;\">", _templatesSettings.Color1));
            if (withPicture)
                sb.AppendLine(string.Format("<th>{0}</th>", cart ? _localizationService.GetResource("Messages.ShoppingCart.Product(s).Picture", languageId) : _localizationService.GetResource("Messages.Wishlist.Product(s).Picture", languageId)));
            sb.AppendLine(string.Format("<th>{0}</th>", cart ? _localizationService.GetResource("Messages.ShoppingCart.Product(s).Name", languageId) : _localizationService.GetResource("Messages.Wishlist.Product(s).Name", languageId)));
            sb.AppendLine(string.Format("<th>{0}</th>", cart ? _localizationService.GetResource("Messages.ShoppingCart.Product(s).Quantity", languageId) : _localizationService.GetResource("Messages.Wishlist.Product(s).Quantity", languageId)));
            sb.AppendLine("</tr>");
            var productService = EngineContext.Current.Resolve<IProductService>();
            var pictureService = EngineContext.Current.Resolve<IPictureService>();

            foreach (var item in cart ? customer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart) :
                customer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.Wishlist))
            {
                var product = productService.GetProductById(item.ProductId);
                sb.AppendLine(string.Format("<tr style=\"background-color: {0};text-align: center;\">", _templatesSettings.Color2));
                //product name
                string productName = product.GetLocalized(x => x.Name, languageId);
                if (withPicture)
                {
                    string pictureUrl = "";
                    if (product.ProductPictures.Any())
                    {
                        var picture = pictureService.GetPictureById(product.ProductPictures.OrderBy(x => x.DisplayOrder).FirstOrDefault().PictureId);
                        if (picture != null)
                        {
                            pictureUrl = pictureService.GetPictureUrl(picture, _templatesSettings.PictureSize);
                        }
                    }
                    sb.Append(string.Format("<td><img src=\"{0}\" alt=\"\"/></td>", pictureUrl));
                }
                sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + WebUtility.HtmlEncode(productName));
                //attributes
                if (!String.IsNullOrEmpty(item.AttributesXml))
                {
                    sb.AppendLine("<br />");
                    sb.AppendLine(item.AttributesXml);
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

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}