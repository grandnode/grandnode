using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Domain.Directory;
using Grand.Domain.Media;
using Grand.Domain.Security;
using Grand.Domain.Stores;
using Grand.Core.Plugins;
using Grand.Plugin.Feed.GoogleShopping.Services;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Configuration;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Seo;
using Grand.Services.Tax;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace Grand.Plugin.Feed.GoogleShopping
{
    public class GoogleShoppingService : BasePlugin, IMiscPlugin
    {
        #region Fields

        private readonly IGoogleService _googleService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ITaxService _taxService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IPictureService _pictureService;
        private readonly ICurrencyService _currencyService;
        private readonly ILanguageService _languageService;
        private readonly ISettingService _settingService;
        private readonly IWorkContext _workContext;
        private readonly IMeasureService _measureService;
        private readonly ILocalizationService _localizationService;
        private readonly MeasureSettings _measureSettings;
        private readonly GoogleShoppingSettings _googleShoppingSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly IWebHelper _webHelper;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        #endregion

        #region Ctor
        public GoogleShoppingService(IGoogleService googleService,
            IPriceCalculationService priceCalculationService,
            ITaxService taxService,
            IProductService productService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IPictureService pictureService,
            ICurrencyService currencyService,
            ILanguageService languageService,
            ISettingService settingService,
            IWorkContext workContext,
            IMeasureService measureService,
            ILocalizationService localizationService,
            MeasureSettings measureSettings,
            GoogleShoppingSettings googleShoppingSettings,
            CurrencySettings currencySettings,
            IWebHelper webHelper,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor)
        {
            _googleService = googleService;
            _priceCalculationService = priceCalculationService;
            _taxService = taxService;
            _productService = productService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _pictureService = pictureService;
            _currencyService = currencyService;
            _languageService = languageService;
            _settingService = settingService;
            _workContext = workContext;
            _measureService = measureService;
            _localizationService = localizationService;
            _measureSettings = measureSettings;
            _googleShoppingSettings = googleShoppingSettings;
            _currencySettings = currencySettings;
            _webHelper = webHelper;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
        }

        #endregion

        #region Utilities
        /// <summary>
        /// Removes invalid characters
        /// </summary>
        /// <param name="input">Input string</param>
        /// <param name="isHtmlEncoded">A value indicating whether input string is HTML encoded</param>
        /// <returns>Valid string</returns>
        private string StripInvalidChars(string input, bool isHtmlEncoded)
        {
            if (String.IsNullOrWhiteSpace(input))
                return input;

            //Microsoft uses a proprietary encoding (called CP-1252) for the bullet symbol and some other special characters, 
            //whereas most websites and data feeds use UTF-8. When you copy-paste from a Microsoft product into a website, 
            //some characters may appear as junk. Our system generates data feeds in the UTF-8 character encoding, 
            //which many shopping engines now require.

            //http://www.atensoftware.com/p90.php?q=182

            if (isHtmlEncoded)
                input = HttpUtility.HtmlDecode(input);

            input = input.Replace("¼", "");
            input = input.Replace("½", "");
            input = input.Replace("¾", "");
            //input = input.Replace("•", "");
            //input = input.Replace("”", "");
            //input = input.Replace("“", "");
            //input = input.Replace("’", "");
            //input = input.Replace("‘", "");
            //input = input.Replace("™", "");
            //input = input.Replace("®", "");
            //input = input.Replace("°", "");

            if (isHtmlEncoded)
                input = HttpUtility.HtmlEncode(input);

            return input;
        }
        private async Task<Currency> GetUsedCurrency()
        {
            var currency = await _currencyService.GetCurrencyById(_googleShoppingSettings.CurrencyId);
            if (currency == null || !currency.Published)
                currency = await _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
            return currency;
        }

        private IUrlHelper GetUrlHelper()
        {
            return _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
        }
        protected virtual string GetHttpProtocol()
        {
            return _webHelper.IsCurrentConnectionSecured() ? "https" : "http";
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/FeedGoogleShopping/Configure";
        }

        /// <summary>
        /// Generate a feed
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="store">Store</param>
        /// <returns>Generated feed</returns>
        public async Task GenerateFeed(Stream stream, Store store)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (store == null)
                throw new ArgumentNullException("store");

            const string googleBaseNamespace = "http://base.google.com/ns/1.0";

            var settings = new XmlWriterSettings {
                Encoding = Encoding.UTF8,
                Async = true,
            };

            //language
            var languageId = "";
            var languages = await _languageService.GetAllLanguages(storeId: store.Id);
            //if we have only one language, let's use it
            if (languages.Count == 1)
            {
                //let's use the first one
                var language = languages.FirstOrDefault();
                languageId = language != null ? language.Id : "";
            }
            //otherwise, use the current one
            if (String.IsNullOrEmpty(languageId))
                languageId = _workContext.WorkingLanguage.Id;

            //we load all google products here using one SQL request (performance optimization)
            var allGoogleProducts = await _googleService.GetAll();

            using (var writer = XmlWriter.Create(stream, settings))
            {
                //Generate feed according to the following specs: http://www.google.com/support/merchants/bin/answer.py?answer=188494&expand=GB
                writer.WriteStartDocument();
                writer.WriteStartElement("rss");
                writer.WriteAttributeString("version", "2.0");
                writer.WriteAttributeString("xmlns", "g", null, googleBaseNamespace);
                writer.WriteStartElement("channel");
                writer.WriteElementString("title", "Google Base feed");
                writer.WriteElementString("link", "http://base.google.com/base/");
                writer.WriteElementString("description", "Information about products");


                var products1 = (await _productService.SearchProducts(storeId: store.Id, visibleIndividuallyOnly: true)).products;
                foreach (var product1 in products1)
                {
                    var productsToProcess = new List<Product>();
                    switch (product1.ProductType)
                    {
                        case ProductType.SimpleProduct:
                            {
                                //simple product doesn't have child products
                                productsToProcess.Add(product1);
                            }
                            break;
                        case ProductType.GroupedProduct:
                            {
                                //grouped products could have several child products
                                var associatedProducts = await _productService.GetAssociatedProducts(product1.Id, store.Id);
                                productsToProcess.AddRange(associatedProducts);
                            }
                            break;
                        default:
                            continue;
                    }
                    foreach (var product in productsToProcess)
                    {
                        writer.WriteStartElement("item");

                        #region Basic Product Information

                        //id [id]- An identifier of the item
                        writer.WriteElementString("g", "id", googleBaseNamespace, product.Id.ToString());

                        //title [title] - Title of the item
                        writer.WriteStartElement("title");
                        var title = product.GetLocalized(x => x.Name, languageId);
                        //title should be not longer than 70 characters
                        if (title.Length > 70)
                            title = title.Substring(0, 70);
                        writer.WriteCData(title);
                        writer.WriteEndElement(); // title

                        //description [description] - Description of the item
                        writer.WriteStartElement("description");
                        string description = product.GetLocalized(x => x.FullDescription, languageId);
                        if (String.IsNullOrEmpty(description))
                            description = product.GetLocalized(x => x.ShortDescription, languageId);
                        if (String.IsNullOrEmpty(description))
                            description = product.GetLocalized(x => x.Name, languageId); //description is required
                        //resolving character encoding issues in your data feed
                        description = StripInvalidChars(description, true);
                        writer.WriteCData(description);
                        writer.WriteEndElement(); // description



                        //google product category [google_product_category] - Google's category of the item
                        //the category of the product according to Google’s product taxonomy. http://www.google.com/support/merchants/bin/answer.py?answer=160081
                        string googleProductCategory = "";
                        var googleProduct = allGoogleProducts.FirstOrDefault(x => x.ProductId == product.Id);
                        if (googleProduct != null)
                            googleProductCategory = googleProduct.Taxonomy;
                        if (String.IsNullOrEmpty(googleProductCategory))
                            googleProductCategory = _googleShoppingSettings.DefaultGoogleCategory;
                        if (String.IsNullOrEmpty(googleProductCategory))
                            throw new GrandException("Default Google category is not set");
                        writer.WriteStartElement("g", "google_product_category", googleBaseNamespace);
                        writer.WriteCData(googleProductCategory);
                        writer.WriteFullEndElement(); // g:google_product_category

                        //product type [product_type] - Your category of the item
                        if (product.ProductCategories.Count > 0)
                        {
                            var defaultProductCategory = await _categoryService.GetCategoryById(product.ProductCategories.OrderBy(x => x.DisplayOrder).FirstOrDefault().CategoryId);

                            if (defaultProductCategory != null)
                            {
                                //TODO localize categories
                                var category = await _categoryService.GetFormattedBreadCrumb(defaultProductCategory, separator: ">", languageId: languageId);
                                if (!String.IsNullOrEmpty((category)))
                                {
                                    writer.WriteStartElement("g", "product_type", googleBaseNamespace);
                                    writer.WriteCData(category);
                                    writer.WriteFullEndElement(); // g:product_type
                                }
                            }
                        }
                        //link [link] - URL directly linking to your item's page on your website
                        var productUrl = GetUrlHelper().RouteUrl("Product", new { SeName = product.GetSeName(languageId) }, GetHttpProtocol());
                        writer.WriteElementString("link", productUrl);

                        //image link [image_link] - URL of an image of the item
                        //additional images [additional_image_link]
                        //up to 10 pictures
                        const int maximumPictures = 10;
                        var storeLocation = _webHelper.IsCurrentConnectionSecured() ?
                            (!string.IsNullOrWhiteSpace(store.SecureUrl) ? store.SecureUrl : store.Url.Replace("http://", "https://")) :
                            store.Url;

                        var pictures = product.ProductPictures.Take(maximumPictures).ToList();
                        for (int i = 0; i < pictures.Count; i++)
                        {
                            var picture = pictures[i];
                            var imageUrl = await _pictureService.GetPictureUrl(picture.PictureId,
                                _googleShoppingSettings.ProductPictureSize,
                                storeLocation: storeLocation);

                            if (i == 0)
                            {
                                //default image
                                writer.WriteElementString("g", "image_link", googleBaseNamespace, imageUrl);
                            }
                            else
                            {
                                //additional image
                                writer.WriteElementString("g", "additional_image_link", googleBaseNamespace, imageUrl);
                            }
                        }
                        if (pictures.Count == 0)
                        {
                            //no picture? submit a default one
                            var imageUrl = await _pictureService.GetDefaultPictureUrl(_googleShoppingSettings.ProductPictureSize, storeLocation: storeLocation);
                            writer.WriteElementString("g", "image_link", googleBaseNamespace, imageUrl);
                        }

                        //condition [condition] - Condition or state of the item
                        writer.WriteElementString("g", "condition", googleBaseNamespace, "new");

                        writer.WriteElementString("g", "expiration_date", googleBaseNamespace, DateTime.Now.AddDays(_googleShoppingSettings.ExpirationNumberOfDays).ToString("yyyy-MM-dd"));

                        #endregion

                        #region Availability & Price

                        //availability [availability] - Availability status of the item
                        string availability = "in stock"; //in stock by default
                        if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock
                            && product.BackorderMode == BackorderMode.NoBackorders
                            && product.GetTotalStockQuantity() <= 0)
                        {
                            availability = "out of stock";
                        }
                        //uncomment th code below in order to support "preorder" value for "availability"
                        //if (product.AvailableForPreOrder &&
                        //    (!product.PreOrderAvailabilityStartDateTimeUtc.HasValue || 
                        //    product.PreOrderAvailabilityStartDateTimeUtc.Value >= DateTime.UtcNow))
                        //{
                        //    availability = "preorder";
                        //}
                        writer.WriteElementString("g", "availability", googleBaseNamespace, availability);

                        //price [price] - Price of the item
                        var currency = await GetUsedCurrency();
                        decimal finalPriceBase;
                        if (_googleShoppingSettings.PricesConsiderPromotions)
                        {
                            //calculate for the maximum quantity (in case if we have tier prices)
                            decimal minPossiblePrice = (await _priceCalculationService.GetFinalPrice(product,
                                _workContext.CurrentCustomer, decimal.Zero, true, int.MaxValue)).finalPrice;
                            finalPriceBase = (await _taxService.GetProductPrice(product, minPossiblePrice)).productprice;

                        }
                        else
                        {
                            finalPriceBase = product.Price;
                        }
                        decimal price = await _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceBase, currency);
                        price = RoundingHelper.RoundPrice(price, currency);
                        writer.WriteElementString("g", "price", googleBaseNamespace,
                                                  price.ToString(new CultureInfo("en-US", false).NumberFormat) + " " +
                                                  currency.CurrencyCode);

                        #endregion

                        #region Unique Product Identifiers

                        /* Unique product identifiers such as UPC, EAN, JAN or ISBN allow us to show your listing on the appropriate product page. If you don't provide the required unique product identifiers, your store may not appear on product pages, and all your items may be removed from Product Search.
                         * We require unique product identifiers for all products - except for custom made goods. For apparel, you must submit the 'brand' attribute. For media (such as books, movies, music and video games), you must submit the 'gtin' attribute. In all cases, we recommend you submit all three attributes.
                         * You need to submit at least two attributes of 'brand', 'gtin' and 'mpn', but we recommend that you submit all three if available. For media (such as books, movies, music and video games), you must submit the 'gtin' attribute, but we recommend that you include 'brand' and 'mpn' if available.
                        */

                        //GTIN [gtin] - GTIN
                        var gtin = product.Gtin;
                        if (!String.IsNullOrEmpty(gtin))
                        {
                            writer.WriteStartElement("g", "gtin", googleBaseNamespace);
                            writer.WriteCData(gtin);
                            writer.WriteFullEndElement(); // g:gtin
                        }

                        //brand [brand] - Brand of the item
                        if (product.ProductManufacturers.Count > 0)
                        {
                            var defaultManufacturer = await _manufacturerService.GetManufacturerById((product.ProductManufacturers.FirstOrDefault().ManufacturerId));

                            if (defaultManufacturer != null)
                            {
                                writer.WriteStartElement("g", "brand", googleBaseNamespace);
                                writer.WriteCData(defaultManufacturer.Name);
                                writer.WriteFullEndElement(); // g:brand
                            }
                        }


                        //mpn [mpn] - Manufacturer Part Number (MPN) of the item
                        var mpn = product.ManufacturerPartNumber;
                        if (!String.IsNullOrEmpty(mpn))
                        {
                            writer.WriteStartElement("g", "mpn", googleBaseNamespace);
                            writer.WriteCData(mpn);
                            writer.WriteFullEndElement(); // g:mpn
                        }

                        //identifier exists [identifier_exists] - Submit custom goods
                        if (googleProduct != null && googleProduct.CustomGoods)
                        {
                            writer.WriteElementString("g", "identifier_exists", googleBaseNamespace, "FALSE");
                        }

                        #endregion

                        #region Apparel Products

                        /* Apparel includes all products that fall under 'Apparel & Accessories' (including all sub-categories)
                         * in Google’s product taxonomy.
                        */

                        //gender [gender] - Gender of the item
                        if (googleProduct != null && !String.IsNullOrEmpty(googleProduct.Gender))
                        {
                            writer.WriteStartElement("g", "gender", googleBaseNamespace);
                            writer.WriteCData(googleProduct.Gender);
                            writer.WriteFullEndElement(); // g:gender
                        }

                        //age group [age_group] - Target age group of the item
                        if (googleProduct != null && !String.IsNullOrEmpty(googleProduct.AgeGroup))
                        {
                            writer.WriteStartElement("g", "age_group", googleBaseNamespace);
                            writer.WriteCData(googleProduct.AgeGroup);
                            writer.WriteFullEndElement(); // g:age_group
                        }

                        //color [color] - Color of the item
                        if (googleProduct != null && !String.IsNullOrEmpty(googleProduct.Color))
                        {
                            writer.WriteStartElement("g", "color", googleBaseNamespace);
                            writer.WriteCData(googleProduct.Color);
                            writer.WriteFullEndElement(); // g:color
                        }

                        //size [size] - Size of the item
                        if (googleProduct != null && !String.IsNullOrEmpty(googleProduct.Size))
                        {
                            writer.WriteStartElement("g", "size", googleBaseNamespace);
                            writer.WriteCData(googleProduct.Size);
                            writer.WriteFullEndElement(); // g:size
                        }

                        #endregion

                        #region Tax & Shipping

                        //tax [tax]
                        //The tax attribute is an item-level override for merchant-level tax settings as defined in your Google Merchant Center account. This attribute is only accepted in the US, if your feed targets a country outside of the US, please do not use this attribute.
                        //IMPORTANT NOTE: Set tax in your Google Merchant Center account settings

                        //IMPORTANT NOTE: Set shipping in your Google Merchant Center account settings

                        //shipping weight [shipping_weight] - Weight of the item for shipping
                        //We accept only the following units of weight: lb, oz, g, kg.
                        if (_googleShoppingSettings.PassShippingInfoWeight)
                        {
                            string weightName;
                            var shippingWeight = product.Weight;
                            var weightSystemName = (await _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId)).SystemKeyword;
                            switch (weightSystemName)
                            {
                                case "ounce":
                                    weightName = "oz";
                                    break;
                                case "lb":
                                    weightName = "lb";
                                    break;
                                case "grams":
                                    weightName = "g";
                                    break;
                                case "kg":
                                    weightName = "kg";
                                    break;
                                default:
                                    //unknown weight 
                                    throw new Exception("Not supported weight. Google accepts the following units: lb, oz, g, kg.");
                            }
                            writer.WriteElementString("g", "shipping_weight", googleBaseNamespace, string.Format(CultureInfo.InvariantCulture, "{0} {1}", shippingWeight.ToString(new CultureInfo("en-US", false).NumberFormat), weightName));
                        }

                        //shipping length [shipping_length] - Length of the item for shipping
                        //shipping width [shipping_width] - Width of the item for shipping
                        //shipping height [shipping_height] - Height of the item for shipping
                        //We accept only the following units of length: in, cm
                        if (_googleShoppingSettings.PassShippingInfoDimensions)
                        {
                            string dimensionName;
                            var length = product.Length;
                            var width = product.Width;
                            var height = product.Height;
                            var dimensionSystemName = (await _measureService.GetMeasureDimensionById(_measureSettings.BaseDimensionId)).SystemKeyword;
                            switch (dimensionSystemName)
                            {
                                case "inches":
                                    dimensionName = "in";
                                    break;
                                //TODO support other dimensions (convert to cm)
                                default:
                                    //unknown dimension 
                                    throw new Exception("Not supported dimension. Google accepts the following units: in, cm.");
                            }
                            writer.WriteElementString("g", "shipping_length", googleBaseNamespace, string.Format(CultureInfo.InvariantCulture, "{0} {1}", length.ToString(new CultureInfo("en-US", false).NumberFormat), dimensionName));
                            writer.WriteElementString("g", "shipping_width", googleBaseNamespace, string.Format(CultureInfo.InvariantCulture, "{0} {1}", width.ToString(new CultureInfo("en-US", false).NumberFormat), dimensionName));
                            writer.WriteElementString("g", "shipping_height", googleBaseNamespace, string.Format(CultureInfo.InvariantCulture, "{0} {1}", height.ToString(new CultureInfo("en-US", false).NumberFormat), dimensionName));
                        }

                        #endregion

                        writer.WriteEndElement(); // item
                    }
                }

                writer.WriteEndElement(); // channel
                writer.WriteEndElement(); // rss
                writer.WriteEndDocument();
            }
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override async Task Install()
        {
            //settings
            var settings = new GoogleShoppingSettings {
                PricesConsiderPromotions = false,
                ProductPictureSize = 125,
                PassShippingInfoWeight = false,
                PassShippingInfoDimensions = false,
                StaticFileName = string.Format("GoogleShopping_{0}.xml", CommonHelper.GenerateRandomDigitCode(10)),
                ExpirationNumberOfDays = 28
            };
            await _settingService.SaveSetting(settings);

            //locales
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.Store", "Store");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.Store.Hint", "Select the store that will be used to generate the feed.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.Currency", "Currency");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.Currency.Hint", "Select the default currency that will be used to generate the feed.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.DefaultGoogleCategory", "Default Google category");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.DefaultGoogleCategory.Hint", "The default Google category to use if one is not specified.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.General", "General");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.Generate", "Generate feed");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.Override", "Override product settings");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.PassShippingInfoWeight", "Pass shipping info (weight)");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.PassShippingInfoWeight.Hint", "Check if you want to include shipping information (weight) in generated XML file.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.PassShippingInfoDimensions", "Pass shipping info (dimensions)");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.PassShippingInfoDimensions.Hint", "Check if you want to include shipping information (dimensions) in generated XML file.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.PricesConsiderPromotions", "Prices consider promotions");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.PricesConsiderPromotions.Hint", "Check if you want prices to be calculated with promotions (tier prices, discounts, special prices, tax, etc). But please note that it can significantly reduce time required to generate the feed file.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.ProductPictureSize", "Product thumbnail image size");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.ProductPictureSize.Hint", "The default size (pixels) for product thumbnail images.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.Products.ProductName", "Product");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.Products.GoogleCategory", "Google Category");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.Products.Gender", "Gender");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.Products.AgeGroup", "Age group");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.Products.Color", "Color");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.Products.Size", "Size");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.Products.CustomGoods", "Custom goods (no identifier exists)");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.SuccessResult", "GoogleShopping feed has been successfully generated.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.StaticFilePath", "Generated file path (static)");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.StaticFilePath.Hint", "A file path of the generated GoogleShopping file. It's static for your store and can be shared with the GoogleShopping service.");

            await base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override async Task Uninstall()
        {
            //settings
            await _settingService.DeleteSetting<GoogleShoppingSettings>();

            //locales
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.Store");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.Store.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.Currency");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.Currency.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.DefaultGoogleCategory");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.DefaultGoogleCategory.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.General");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.Generate");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.Override");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.PassShippingInfoWeight");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.PassShippingInfoWeight.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.PassShippingInfoDimensions");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.PassShippingInfoDimensions.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.PricesConsiderPromotions");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.PricesConsiderPromotions.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.ProductPictureSize");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.ProductPictureSize.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.Products.ProductName");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.Products.GoogleCategory");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.Products.Gender");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.Products.AgeGroup");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.Products.Color");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.Products.Size");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.Products.CustomGoods");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.SuccessResult");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.StaticFilePath");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Feed.GoogleShopping.StaticFilePath.Hint");

            await base.Uninstall();
        }

        /// <summary>
        /// Generate a static file for GoogleShopping
        /// </summary>
        /// <param name="store">Store</param>
        public virtual async Task GenerateStaticFile(Store store)
        {
            var appPath = CommonHelper.WebMapPath("content/files/exportimport");
            if (store == null)
                throw new ArgumentNullException("store");
            string filePath = Path.Combine(appPath, store.Id + "-" + _googleShoppingSettings.StaticFileName);
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                await GenerateFeed(fs, store);
            }
        }

        #endregion
    }
}