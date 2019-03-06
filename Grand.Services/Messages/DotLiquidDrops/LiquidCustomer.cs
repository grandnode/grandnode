using DotLiquid;
using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Messages;
using Grand.Core.Domain.Tax;
using Grand.Core.Infrastructure;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Customers;
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
    public partial class LiquidCustomer : Drop
    {
        private Customer _customer;
        private CustomerNote _customerNote;
        private string _languageId;

        private readonly ICustomerAttributeFormatter _customerAttributeFormatter;
        private readonly IStoreService _storeService;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        private readonly MessageTemplatesSettings _templatesSettings;
        private readonly CatalogSettings _catalogSettings;

        public LiquidCustomer(Customer customer, CustomerNote customerNote = null)
        {
            this._customerAttributeFormatter = EngineContext.Current.Resolve<ICustomerAttributeFormatter>();
            this._storeService = EngineContext.Current.Resolve<IStoreService>();
            this._localizationService = EngineContext.Current.Resolve<ILocalizationService>();
            this._languageService = EngineContext.Current.Resolve<ILanguageService>();
            this._templatesSettings = EngineContext.Current.Resolve<MessageTemplatesSettings>();
            this._catalogSettings = EngineContext.Current.Resolve<CatalogSettings>();

            this._customer = customer;
            this._customerNote = customerNote;

            string languageId = _languageService.GetAllLanguages().FirstOrDefault().Id;
            if (customer.GenericAttributes.FirstOrDefault(x => x.Key == "LanguageId") != null)
            {
                languageId = customer.GenericAttributes.FirstOrDefault(x => x.Key == "LanguageId").Value;
            }

            this._languageId = languageId;

            AdditionalTokens = new Dictionary<string, string>();
        }

        public string Email
        {
            get { return _customer.Email; }
        }

        public string Username
        {
            get { return _customer.Username; }
        }

        public string FullName
        {
            get { return _customer.GetFullName(); }
        }

        public string FirstName
        {
            get { return _customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName); }
        }

        public string LastName
        {
            get { return _customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName); }
        }

        public string VatNumber
        {
            get { return _customer.GetAttribute<string>(SystemCustomerAttributeNames.VatNumber); }
        }

        public string VatNumberStatus
        {
            get { return ((VatNumberStatus)_customer.GetAttribute<int>(SystemCustomerAttributeNames.VatNumberStatusId)).ToString(); }
        }

        public string CustomAttributes
        {
            get
            {
                var customAttributesXml = _customer.GetAttribute<string>(SystemCustomerAttributeNames.CustomCustomerAttributes);
                return _customerAttributeFormatter.FormatAttributes(customAttributesXml);
            }
        }

        public string PasswordRecoveryURL
        {
            get { return string.Format("{0}passwordrecovery/confirm?token={1}&email={2}", _storeService.GetStoreUrl(_customer.StoreId), _customer.GetAttribute<string>(SystemCustomerAttributeNames.PasswordRecoveryToken), WebUtility.UrlEncode(_customer.Email)); }
        }

        public string AccountActivationURL
        {
            get { return string.Format("{0}customer/activation?token={1}&email={2}", _storeService.GetStoreUrl(_customer.StoreId), _customer.GetAttribute<string>(SystemCustomerAttributeNames.AccountActivationToken), WebUtility.UrlEncode(_customer.Email)); ; }
        }

        public string WishlistURLForCustomer
        {
            get { return string.Format("{0}wishlist/{1}", _storeService.GetStoreUrl(_customer.StoreId), _customer.CustomerGuid); }
        }

        public string NewNoteText
        {
            get { return _customerNote.FormatCustomerNoteText(); }
        }

        public string NewTitleText
        {
            get { return _customerNote.Title; }
        }

        public string CustomerNoteAttachmentUrl
        {
            get { return string.Format("{0}download/customernotefile/{1}", _storeService.GetStoreUrl(_customer.StoreId), _customerNote.Id); }
        }

        public string RecommendedProducts
        {
            get { return RecommendedProductsListToHtmlTable(_customer, _languageId, false); }
        }

        public string RecommendedProductsWithPictures
        {
            get { return RecommendedProductsListToHtmlTable(_customer, _languageId, true); }
        }

        public string RecentlyViewedProducts
        {
            get { return RecentlyViewedProductsListToHtmlTable(_customer, _languageId, false); }
        }

        public string RecentlyViewedProductsWithPictures
        {
            get { return RecentlyViewedProductsListToHtmlTable(_customer, _languageId, true); }
        }


        protected virtual string RecommendedProductsListToHtmlTable(Customer customer, string languageId, bool withPicture)
        {
            string result;

            var sb = new StringBuilder();
            sb.AppendLine("<table border=\"0\" style=\"width:100%;\">");

            #region Products
            sb.AppendLine(string.Format("<tr style=\"background-color:{0};text-align:center;\">", _templatesSettings.Color1));
            if (withPicture)
                sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.RecommendedProducts.Product(s).Picture", languageId)));
            sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.RecommendedProducts.Product(s).Name", languageId)));
            sb.AppendLine("</tr>");

            var productService = EngineContext.Current.Resolve<IProductService>();
            var pictureService = EngineContext.Current.Resolve<IPictureService>();
            var products = productService.GetRecommendedProducts(customer.GetCustomerRoleIds());

            foreach (var item in products)
            {

                sb.AppendLine(string.Format("<tr style=\"background-color: {0};text-align: center;\">", _templatesSettings.Color2));
                //product name
                string productName = item.GetLocalized(x => x.Name, languageId);
                if (withPicture)
                {
                    string pictureUrl = "";
                    if (item.ProductPictures.Any())
                    {
                        var picture = pictureService.GetPictureById(item.ProductPictures.OrderBy(x => x.DisplayOrder).FirstOrDefault().PictureId);
                        if (picture != null)
                        {
                            pictureUrl = pictureService.GetPictureUrl(picture, _templatesSettings.PictureSize);
                        }
                    }
                    sb.Append(string.Format("<td><img src=\"{0}\" alt=\"\"/></td>", pictureUrl));
                }
                sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + WebUtility.HtmlEncode(productName));

                sb.AppendLine("</td>");

                sb.AppendLine("</tr>");
            }
            #endregion

            sb.AppendLine("</table>");
            result = sb.ToString();
            return result;
        }

        protected virtual string RecentlyViewedProductsListToHtmlTable(Customer customer, string languageId, bool withPicture)
        {
            string result;

            var sb = new StringBuilder();
            sb.AppendLine("<table border=\"0\" style=\"width:100%;\">");

            #region Products
            sb.AppendLine(string.Format("<tr style=\"background-color:{0};text-align:center;\">", _templatesSettings.Color1));
            if (withPicture)
                sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.RecentlyViewedProducts.Product(s).Picture", languageId)));
            sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.RecentlyViewedProducts.Product(s).Name", languageId)));
            sb.AppendLine("</tr>");

            var recentlyViewedProductsService = EngineContext.Current.Resolve<IRecentlyViewedProductsService>();
            var pictureService = EngineContext.Current.Resolve<IPictureService>();
            var products = recentlyViewedProductsService.GetRecentlyViewedProducts(customer.Id, _catalogSettings.RecentlyViewedProductsNumber);
            foreach (var item in products)
            {
                sb.AppendLine(string.Format("<tr style=\"background-color: {0};text-align: center;\">", _templatesSettings.Color2));
                //product name
                string productName = item.GetLocalized(x => x.Name, languageId);
                if (withPicture)
                {
                    string pictureUrl = "";
                    if (item.ProductPictures.Any())
                    {
                        var picture = pictureService.GetPictureById(item.ProductPictures.OrderBy(x => x.DisplayOrder).FirstOrDefault().PictureId);
                        if (picture != null)
                        {
                            pictureUrl = pictureService.GetPictureUrl(picture, _templatesSettings.PictureSize);
                        }
                    }
                    sb.Append(string.Format("<td><img src=\"{0}\" alt=\"\"/></td>", pictureUrl));
                }
                sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + WebUtility.HtmlEncode(productName));
                sb.AppendLine("</td>");
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