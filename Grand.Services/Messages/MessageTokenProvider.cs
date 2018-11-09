using Grand.Core;
using Grand.Core.Domain;
using Grand.Core.Domain.Blogs;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Forums;
using Grand.Core.Domain.Knowledgebase;
using Grand.Core.Domain.Media;
using Grand.Core.Domain.Messages;
using Grand.Core.Domain.News;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Shipping;
using Grand.Core.Domain.Stores;
using Grand.Core.Domain.Tax;
using Grand.Core.Domain.Vendors;
using Grand.Core.Html;
using Grand.Core.Infrastructure;
using Grand.Services.Blogs;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Events;
using Grand.Services.Forums;
using Grand.Services.Helpers;
using Grand.Services.Knowledgebase;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.News;
using Grand.Services.Orders;
using Grand.Services.Payments;
using Grand.Services.Seo;
using Grand.Services.Shipping;
using Grand.Services.Stores;
using Grand.Services.Vendors;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

namespace Grand.Services.Messages
{
    public partial class MessageTokenProvider : IMessageTokenProvider
    {
        #region Fields

        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICurrencyService _currencyService;
        private readonly IWorkContext _workContext;
        private readonly IDownloadService _downloadService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly IStoreService _storeService;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerAttributeFormatter _customerAttributeFormatter;

        private readonly MessageTemplatesSettings _templatesSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly TaxSettings _taxSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly MediaSettings _mediaSettings;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        public MessageTokenProvider(ILanguageService languageService,
            ILocalizationService localizationService,
            IDateTimeHelper dateTimeHelper,
            IPriceFormatter priceFormatter,
            ICurrencyService currencyService,
            IWorkContext workContext,
            IDownloadService downloadService,
            IOrderService orderService,
            IPaymentService paymentService,
            IStoreService storeService,
            IStoreContext storeContext,
            IProductAttributeParser productAttributeParser,
            IAddressAttributeFormatter addressAttributeFormatter,
            ICustomerAttributeFormatter customerAttributeFormatter,
            MessageTemplatesSettings templatesSettings,
            CatalogSettings catalogSettings,
            TaxSettings taxSettings,
            CurrencySettings currencySettings,
            ShippingSettings shippingSettings,
            StoreInformationSettings storeInformationSettings,
            MediaSettings _mediaSettings,
            IEventPublisher eventPublisher)
        {
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._dateTimeHelper = dateTimeHelper;
            this._priceFormatter = priceFormatter;
            this._currencyService = currencyService;
            this._workContext = workContext;
            this._downloadService = downloadService;
            this._orderService = orderService;
            this._paymentService = paymentService;
            this._productAttributeParser = productAttributeParser;
            this._addressAttributeFormatter = addressAttributeFormatter;
            this._customerAttributeFormatter = customerAttributeFormatter;
            this._storeService = storeService;
            this._storeContext = storeContext;
            this._shippingSettings = shippingSettings;
            this._templatesSettings = templatesSettings;
            this._catalogSettings = catalogSettings;
            this._taxSettings = taxSettings;
            this._currencySettings = currencySettings;
            this._storeInformationSettings = storeInformationSettings;
            this._mediaSettings = _mediaSettings;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region Utilities
       

        /// <summary>
        /// Convert a collection to a HTML table
        /// </summary>
        /// <param name="shipment">Shipment</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>HTML table of products</returns>
        protected virtual string ProductListToHtmlTable(Shipment shipment, string languageId)
        {
            string result;

            var sb = new StringBuilder();
            sb.AppendLine("<table border=\"0\" style=\"width:100%;\">");

            #region Products
            sb.AppendLine(string.Format("<tr style=\"background-color:{0};text-align:center;\">", _templatesSettings.Color1));
            sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).Name", languageId)));
            sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).Quantity", languageId)));
            sb.AppendLine("</tr>");

            var table = shipment.ShipmentItems.ToList();
            var order = _orderService.GetOrderById(shipment.OrderId);
            var productService = EngineContext.Current.Resolve<IProductService>();
            for (int i = 0; i <= table.Count - 1; i++)
            {
                var si = table[i];
                var orderItem = order.OrderItems.Where(x => x.Id == si.OrderItemId).FirstOrDefault();
                if (orderItem == null)
                    continue;

                var product = productService.GetProductByIdIncludeArch(orderItem.ProductId);
                if (product == null)
                    continue;

                sb.AppendLine(string.Format("<tr style=\"background-color: {0};text-align: center;\">", _templatesSettings.Color2));
                //product name
                string productName = product.GetLocalized(x => x.Name, languageId);

                sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + WebUtility.HtmlEncode(productName));
                //attributes
                if (!String.IsNullOrEmpty(orderItem.AttributeDescription))
                {
                    sb.AppendLine("<br />");
                    sb.AppendLine(orderItem.AttributeDescription);
                }
                //sku
                if (_catalogSettings.ShowSkuOnProductDetailsPage)
                {
                    var sku = product.FormatSku(orderItem.AttributesXml, _productAttributeParser);
                    if (!String.IsNullOrEmpty(sku))
                    {
                        sb.AppendLine("<br />");
                        sb.AppendLine(string.Format(_localizationService.GetResource("Messages.Order.Product(s).SKU", languageId), WebUtility.HtmlEncode(sku)));
                    }
                }
                sb.AppendLine("</td>");

                sb.AppendLine(string.Format("<td style=\"padding: 0.6em 0.4em;text-align: center;\">{0}</td>", si.Quantity));

                sb.AppendLine("</tr>");
            }
            #endregion

            sb.AppendLine("</table>");
            result = sb.ToString();
            return result;
        }



        #endregion

        #region Methods

        /// <summary>
        /// Gets list of allowed (supported) message tokens for campaigns
        /// </summary>
        /// <returns>List of allowed (supported) message tokens for campaigns</returns>
        public virtual string[] GetListOfCampaignAllowedTokens()
        {
            var allowedTokens = new List<string>
            {
                "%Store.Name%",
                "%Store.URL%",
                "%Store.Email%",
                "%Store.CompanyName%",
                "%Store.CompanyAddress%",
                "%Store.CompanyPhoneNumber%",
                "%Store.CompanyVat%",
                "%NewsLetterSubscription.Email%",
                "%NewsLetterSubscription.ActivationUrl%",
                "%NewsLetterSubscription.DeactivationUrl%",
                "%ShoppingCart.Products%",
                "%ShoppingCart.ProductsWithPictures%",
                "%Wishlist.Products%",
                "%Wishlist.ProductsWithPictures%",
                "%RecommendedProducts.Products%",
                "%RecommendedProducts.ProductsWithPictures%",
                "%RecentlyViewedProducts.Products%",
                "%RecentlyViewedProducts.ProductsWithPictures%",
            };
            return allowedTokens.ToArray();
        }

        public virtual string[] GetListOfAllowedTokens()
        {
            var allowedTokens = new List<string>
            {
                "%Store.Name%",
                "%Store.URL%",
                "%Store.Email%",
                "%Store.CompanyName%",
                "%Store.CompanyAddress%",
                "%Store.CompanyPhoneNumber%",
                "%Store.CompanyVat%",
                "%Twitter.URL%",
                "%Facebook.URL%",
                "%YouTube.URL%",
                "%GooglePlus.URL%",
                "%Instagram.URL%",
                "%LinkedIn.URL%",
                "%Pinterest.URL%",
                "%Order.OrderNumber%",
                "%Order.CustomerFullName%",
                "%Order.CustomerEmail%",
                "%Order.BillingFirstName%",
                "%Order.BillingLastName%",
                "%Order.BillingPhoneNumber%",
                "%Order.BillingEmail%",
                "%Order.BillingFaxNumber%",
                "%Order.BillingCompany%",
                "%Order.BillingAddress1%",
                "%Order.BillingAddress2%",
                "%Order.BillingCity%",
                "%Order.BillingStateProvince%",
                "%Order.BillingZipPostalCode%",
                "%Order.BillingCountry%",
                "%Order.BillingCustomAttributes%",
                "%Order.ShippingMethod%",
                "%Order.ShippingAdditionDescription%",
                "%Order.ShippingFirstName%",
                "%Order.ShippingLastName%",
                "%Order.ShippingPhoneNumber%",
                "%Order.ShippingEmail%",
                "%Order.ShippingFaxNumber%",
                "%Order.ShippingCompany%",
                "%Order.ShippingAddress1%",
                "%Order.ShippingAddress2%",
                "%Order.ShippingCity%",
                "%Order.ShippingStateProvince%",
                "%Order.ShippingZipPostalCode%",
                "%Order.ShippingCountry%",
                "%Order.ShippingCustomAttributes%",
                "%Order.PaymentMethod%",
                "%Order.VatNumber%",
                "%Order.CustomValues%",
                "%Order.Product(s)%",
                "%Order.CreatedOn%",
                "%Order.OrderURLForCustomer%",
                "%Order.NewNoteText%",
                "%Order.OrderNoteAttachmentUrl%",
                "%Order.AmountRefunded%",
                "%RecurringPayment.ID%",
                "%Shipment.ShipmentNumber%",
                "%Shipment.TrackingNumber%",
                "%Shipment.TrackingNumberURL%",
                "%Shipment.Product(s)%",
                "%Shipment.URLForCustomer%",
                "%ReturnRequest.ID%",
                "%ReturnRequest.OrderId%",
                "%ReturnRequest.Product.Quantity%",
                "%ReturnRequest.Product.Name%",
                "%ReturnRequest.Reason%",
                "%ReturnRequest.RequestedAction%",
                "%ReturnRequest.CustomerComment%",
                "%ReturnRequest.StaffNotes%",
                "%ReturnRequest.Status%",
                "%GiftCard.SenderName%",
                "%GiftCard.SenderEmail%",
                "%GiftCard.RecipientName%",
                "%GiftCard.RecipientEmail%",
                "%GiftCard.Amount%",
                "%GiftCard.CouponCode%",
                "%GiftCard.Message%",
                "%Customer.Email%",
                "%Customer.Username%",
                "%Customer.FullName%",
                "%Customer.FirstName%",
                "%Customer.LastName%",
                "%Customer.VatNumber%",
                "%Customer.CustomAttributes%",
                "%Customer.PasswordRecoveryURL%",
                "%Customer.AccountActivationURL%",
                "%Customer.NewNoteText%",
                "%Customer.NewTitleText%",
                "%Customer.CustomerNoteAttachmentUrl%",
                "%ContactUs.SenderEmail%",
                "%ContactUs.SenderName%",
                "%ContactUs.Body%",
                "%ContactUs.AttributeDescription%",
                "%Vendor.Address1%",
                "%Vendor.Address2%",
                "%Vendor.City%",
                "%Vendor.Company%",
                "%Vendor.Country%",
                "%Vendor.Description%",
                "%Vendor.Email%",
                "%Vendor.FaxNumber%",
                "%Vendor.Name%",
                "%Vendor.PhoneNumber%",
                "%Vendor.StateProvince%",
                "%Vendor.ZipPostalCode%",
                "%Wishlist.URLForCustomer%",
                "%NewsLetterSubscription.Email%",
                "%NewsLetterSubscription.ActivationUrl%",
                "%NewsLetterSubscription.DeactivationUrl%",
                "%ProductReview.ProductName%",
                "%BlogComment.BlogPostTitle%",
                "%BlogPost.URL%",
                "%NewsComment.NewsTitle%",
                "%NewsComment.CommentText%",
                "%NewsComment.CommentTitle%",
                "%News.Url%",
                "%Product.ID%",
                "%Product.Name%",
                "%Product.ShortDescription%",
                "%Product.ProductURLForCustomer%",
                "%Product.SKU%",
                "%Product.StockQuantity%",
                "%Forums.TopicURL%",
                "%Forums.TopicName%",
                "%Forums.PostAuthor%",
                "%Forums.PostBody%",
                "%Forums.ForumURL%",
                "%Forums.ForumName%",
                "%AttributeCombination.Formatted%",
                "%AttributeCombination.SKU%",
                "%AttributeCombination.StockQuantity%",
                "%PrivateMessage.Subject%",
                "%PrivateMessage.Text%",
                "%BackInStockSubscription.ProductName%",
                "%BackInStockSubscription.ProductUrl%",
                "%Auctions.ProductName%",
                "%Auctions.Price%",
                "%Auctions.EndTime%",
                "%Auctions.ProductSeName%"
            };
            return allowedTokens.ToArray();
        }

        public virtual string[] GetListOfCustomerReminderAllowedTokens(CustomerReminderRuleEnum rule)
        {
            var allowedTokens = new List<string>();
            allowedTokens.AddRange(
                new List<string>{ "%Store.Name%",
                "%Store.URL%",
                "%Store.Email%",
                "%Store.CompanyName%",
                "%Store.CompanyAddress%",
                "%Store.CompanyPhoneNumber%",
                "%Store.CompanyVat%",
                "%Twitter.URL%",
                "%Facebook.URL%",
                "%YouTube.URL%",
                "%GooglePlus.URL%",
                "%Instagram.URL%",
                "%LinkedIn.URL%",
                "%Pinterest.URL%"});

            if (rule == CustomerReminderRuleEnum.AbandonedCart)
            {
                allowedTokens.Add("%ShoppingCart.Products%");
                allowedTokens.Add("%ShoppingCart.ProductsWithPictures%");
                allowedTokens.Add("%Wishlist.Products%");
                allowedTokens.Add("%Wishlist.ProductsWithPictures%");
            }
            if (rule == CustomerReminderRuleEnum.CompletedOrder || rule == CustomerReminderRuleEnum.UnpaidOrder)
            {
                allowedTokens.AddRange(
                new List<string>{
                "%Order.OrderNumber%",
                "%Order.CustomerFullName%",
                "%Order.CustomerEmail%",
                "%Order.BillingFirstName%",
                "%Order.BillingLastName%",
                "%Order.BillingPhoneNumber%",
                "%Order.BillingEmail%",
                "%Order.BillingFaxNumber%",
                "%Order.BillingCompany%",
                "%Order.BillingAddress1%",
                "%Order.BillingAddress2%",
                "%Order.BillingCity%",
                "%Order.BillingStateProvince%",
                "%Order.BillingZipPostalCode%",
                "%Order.BillingCountry%",
                "%Order.BillingCustomAttributes%",
                "%Order.ShippingMethod%",
                "%Order.ShippingAdditionDescription%",
                "%Order.ShippingFirstName%",
                "%Order.ShippingLastName%",
                "%Order.ShippingPhoneNumber%",
                "%Order.ShippingEmail%",
                "%Order.ShippingFaxNumber%",
                "%Order.ShippingCompany%",
                "%Order.ShippingAddress1%",
                "%Order.ShippingAddress2%",
                "%Order.ShippingCity%",
                "%Order.ShippingStateProvince%",
                "%Order.ShippingZipPostalCode%",
                "%Order.ShippingCountry%",
                "%Order.ShippingCustomAttributes%",
                "%Order.PaymentMethod%",
                "%Order.VatNumber%",
                "%Order.CustomValues%",
                "%Order.Product(s)%",
                "%Order.CreatedOn%",
                "%Order.OrderURLForCustomer%",
                "%Order.NewNoteText%",
                "%Order.OrderNoteAttachmentUrl%",
                "%Order.AmountRefunded%"
                });
            }
            allowedTokens.AddRange(
                new List<string>{
                "%Customer.Email%",
                "%Customer.Username%",
                "%Customer.FullName%",
                "%Customer.FirstName%",
                "%Customer.LastName%",
                "%RecommendedProducts.Products%",
                "%RecommendedProducts.ProductsWithPictures%",
                "%RecentlyViewedProducts.Products%",
                "%RecentlyViewedProducts.ProductsWithPictures%",
                });
            return allowedTokens.ToArray();
        }

        #endregion
    }
}
