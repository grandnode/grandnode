using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Domain;
using Grand.Core.Domain.Blogs;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Forums;
using Grand.Core.Domain.Knowledgebase;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.News;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Tax;
using Grand.Core.Domain.Vendors;
using Grand.Framework.Themes;
using Grand.Framework.UI;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Forums;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Orders;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Services.Topics;
using Grand.Web.Extensions;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Interfaces;
using Grand.Web.Models.Catalog;
using Grand.Web.Models.Common;
using Grand.Web.Models.Topics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Web.Services
{
    public partial class CommonViewModelService : ICommonViewModelService
    {
        private readonly ICacheManager _cacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IThemeContext _themeContext;
        private readonly IPictureService _pictureService;
        private readonly IWebHelper _webHelper;
        private readonly ILanguageService _languageService;
        private readonly IWorkContext _workContext;
        private readonly ICurrencyService _currencyService;
        private readonly IPermissionService _permissionService;
        private readonly IPageHeadBuilder _pageHeadBuilder;
        private readonly ITopicService _topicService;
        private readonly ILocalizationService _localizationService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductService _productService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IServiceProvider _serviceProvider;

        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly TaxSettings _taxSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly ForumSettings _forumSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly BlogSettings _blogSettings;
        private readonly KnowledgebaseSettings _knowledgebaseSettings;
        private readonly NewsSettings _newsSettings;
        private readonly VendorSettings _vendorSettings;
        private readonly CommonSettings _commonSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;

        public CommonViewModelService(ICacheManager cacheManager,
            IStoreContext storeContext,
            IThemeContext themeContext,
            IPictureService pictureService,
            IWebHelper webHelper,
            ILanguageService languageService,
            IWorkContext workContext,
            ICurrencyService currencyService,
            IPermissionService permissionService,
            IPageHeadBuilder pageHeadBuilder,
            ITopicService topicService,
            ILocalizationService localizationService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IProductService productService,
            IWebHostEnvironment hostingEnvironment,
            IServiceProvider serviceProvider,
            StoreInformationSettings storeInformationSettings,
            LocalizationSettings localizationSettings,
            TaxSettings taxSettings,
            CustomerSettings customerSettings,
            ForumSettings forumSettings,
            CatalogSettings catalogSettings,
            BlogSettings blogSettings,
            KnowledgebaseSettings knowledgebaseSettings,
            NewsSettings newsSettings,
            VendorSettings vendorSettings,
            CommonSettings commonSettings,
            ShoppingCartSettings shoppingCartSettings)
        {
            _cacheManager = cacheManager;
            _storeContext = storeContext;
            _themeContext = themeContext;
            _pictureService = pictureService;
            _webHelper = webHelper;
            _languageService = languageService;
            _workContext = workContext;
            _currencyService = currencyService;
            _permissionService = permissionService;
            _pageHeadBuilder = pageHeadBuilder;
            _topicService = topicService;
            _localizationService = localizationService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _productService = productService;
            _hostingEnvironment = hostingEnvironment;
            _serviceProvider = serviceProvider;
            _storeInformationSettings = storeInformationSettings;
            _localizationSettings = localizationSettings;
            _taxSettings = taxSettings;
            _customerSettings = customerSettings;
            _forumSettings = forumSettings;
            _catalogSettings = catalogSettings;
            _blogSettings = blogSettings;
            _knowledgebaseSettings = knowledgebaseSettings;
            _newsSettings = newsSettings;
            _vendorSettings = vendorSettings;
            _commonSettings = commonSettings;
            _shoppingCartSettings = shoppingCartSettings;
        }

        protected async Task<HeaderLinksModel> prepareHeaderLinks(Customer customer)
        {
            var isRegister = customer.IsRegistered();
            var model = new HeaderLinksModel {
                IsAuthenticated = isRegister,
                CustomerEmailUsername = isRegister ? (_customerSettings.UsernamesEnabled ? customer.Username : customer.Email) : "",
                AllowPrivateMessages = isRegister && _forumSettings.AllowPrivateMessages,
            };
            if (_forumSettings.AllowPrivateMessages)
            {
                var unreadMessageCount = await GetUnreadPrivateMessages();
                var unreadMessage = string.Empty;
                var alertMessage = string.Empty;
                if (unreadMessageCount > 0)
                {
                    unreadMessage = string.Format(_localizationService.GetResource("PrivateMessages.TotalUnread"), unreadMessageCount);

                    //notifications here
                    if (_forumSettings.ShowAlertForPM &&
                        !customer.GetAttributeFromEntity<bool>(SystemCustomerAttributeNames.NotifiedAboutNewPrivateMessages, _storeContext.CurrentStore.Id))
                    {
                        await _serviceProvider.GetRequiredService<IGenericAttributeService>().SaveAttribute(customer, SystemCustomerAttributeNames.NotifiedAboutNewPrivateMessages, true, _storeContext.CurrentStore.Id);
                        alertMessage = string.Format(_localizationService.GetResource("PrivateMessages.YouHaveUnreadPM"), unreadMessageCount);
                    }
                }
                model.UnreadPrivateMessages = unreadMessage;
                model.AlertMessage = alertMessage;
            }
            return model;
        }

        protected async Task<ShoppingCartLinksModel> prepareShoppingCartLinks(Customer customer)
        {
            var model = new ShoppingCartLinksModel {
                ShoppingCartEnabled = await _permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart),
                WishlistEnabled = await _permissionService.Authorize(StandardPermissionProvider.EnableWishlist),
                MiniShoppingCartEnabled = _shoppingCartSettings.MiniShoppingCartEnabled,
                //performance optimization (use "HasShoppingCartItems" property)
                ShoppingCartItems = customer.ShoppingCartItems.Any() ? customer.ShoppingCartItems
                        .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart || sci.ShoppingCartType == ShoppingCartType.Auctions)
                        .LimitPerStore(_shoppingCartSettings.CartsSharedBetweenStores, _storeContext.CurrentStore.Id)
                        .Sum(x => x.Quantity) : 0,

                WishlistItems = customer.ShoppingCartItems.Any() ? customer.ShoppingCartItems
                        .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                        .LimitPerStore(_shoppingCartSettings.CartsSharedBetweenStores, _storeContext.CurrentStore.Id)
                        .Sum(x => x.Quantity) : 0
            };
            return model;
        }

        public virtual async Task<LogoModel> PrepareLogo()
        {
            var model = new LogoModel {
                StoreName = _storeContext.CurrentStore.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id)
            };

            var cacheKey = string.Format(ModelCacheEventConst.STORE_LOGO_PATH, _storeContext.CurrentStore.Id, _themeContext.WorkingThemeName, _webHelper.GetMachineName());
            model.LogoPath = await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var logo = "";
                var picture = await _pictureService.GetPictureById(_storeInformationSettings.LogoPictureId);
                if (picture != null)
                {
                    logo = await _pictureService.GetPictureUrl(picture, showDefaultPicture: false);
                }
                if (String.IsNullOrEmpty(logo))
                {
                    //use default logo
                    logo = string.Format("{0}Themes/{1}/Content/images/logo.png", _webHelper.GetStoreLocation(), _themeContext.WorkingThemeName);
                }
                return logo;
            });
            return model;
        }

        public virtual async Task<LanguageSelectorModel> PrepareLanguageSelector()
        {
            var availableLanguages = (await _languageService
                     .GetAllLanguages(storeId: _storeContext.CurrentStore.Id))
                     .Select(x => new LanguageModel {
                         Id = x.Id,
                         Name = x.Name,
                         FlagImageFileName = x.FlagImageFileName,
                     }).ToList();

            var model = new LanguageSelectorModel {
                CurrentLanguageId = _workContext.WorkingLanguage.Id,
                AvailableLanguages = availableLanguages,
                UseImages = _localizationSettings.UseImagesForLanguageSelection
            };

            return model;
        }

        public virtual async Task<CurrencySelectorModel> PrepareCurrencySelector()
        {
            var availableCurrencies = (await _currencyService.GetAllCurrencies(storeId: _storeContext.CurrentStore.Id))
                .Select(x =>
                {
                    //currency char
                    var currencySymbol = "";
                    if (!string.IsNullOrEmpty(x.DisplayLocale))
                        currencySymbol = new RegionInfo(x.DisplayLocale).CurrencySymbol;
                    else
                        currencySymbol = x.CurrencyCode;
                    //model
                    var currencyModel = new CurrencyModel {
                        Id = x.Id,
                        Name = x.GetLocalized(y => y.Name, _workContext.WorkingLanguage.Id),
                        CurrencyCode = x.CurrencyCode,
                        CurrencySymbol = currencySymbol
                    };
                    return currencyModel;
                }).ToList();

            var model = new CurrencySelectorModel {
                CurrentCurrencyId = _workContext.WorkingCurrency.Id,
                AvailableCurrencies = availableCurrencies
            };

            return model;
        }

        public virtual async Task SetCurrency(string customerCurrency)
        {
            var currency = await _currencyService.GetCurrencyById(customerCurrency);
            if (currency != null)
                await _workContext.SetWorkingCurrency(currency);

        }
        public virtual TaxTypeSelectorModel PrepareTaxTypeSelector()
        {
            if (!_taxSettings.AllowCustomersToSelectTaxDisplayType)
                return null;

            var model = new TaxTypeSelectorModel {
                CurrentTaxType = _workContext.TaxDisplayType
            };
            return model;
        }

        public virtual async Task SetTaxType(int customerTaxType)
        {
            var taxDisplayType = (TaxDisplayType)Enum.ToObject(typeof(TaxDisplayType), customerTaxType);
            await _workContext.SetTaxDisplayType(taxDisplayType);
        }

        public virtual async Task<StoreSelectorModel> PrepareStoreSelector()
        {
            if (!_commonSettings.AllowToSelectStore)
                return null;

            var storeService = _serviceProvider.GetRequiredService<IStoreService>();
            var availableStores = (await storeService.GetAllStores())
                .Select(x => new StoreModel {
                    Id = x.Id,
                    Name = x.Name,
                }).ToList();

            var model = new StoreSelectorModel {
                CurrentStoreId = _storeContext.CurrentStore.Id,
                AvailableStores = availableStores,
            };

            return model;
        }

        public virtual async Task SetStore(string storeid)
        {
            if (_commonSettings.AllowToSelectStore)
            {
                var storeService = _serviceProvider.GetRequiredService<IStoreService>();
                var store = await storeService.GetStoreById(storeid);
                if (store != null)
                    await _storeContext.SetStoreCookie(storeid);
            }
        }

        public virtual async Task<int> GetUnreadPrivateMessages()
        {
            var result = 0;
            var customer = _workContext.CurrentCustomer;
            if (_forumSettings.AllowPrivateMessages && !customer.IsGuest())
            {
                var forumservice = _serviceProvider.GetRequiredService<IForumService>();
                var privateMessages = await forumservice.GetAllPrivateMessages(_storeContext.CurrentStore.Id,
                    "", customer.Id, false, null, false, string.Empty, 0, 1);

                if (privateMessages.Any())
                {
                    result = privateMessages.TotalCount;
                }
            }
            return result;

        }
        public virtual Task<HeaderLinksModel> PrepareHeaderLinks(Customer customer)
        {
            return prepareHeaderLinks(customer);
        }

        public virtual Task<ShoppingCartLinksModel> PrepareShoppingCartLinks(Customer customer)
        {
            return prepareShoppingCartLinks(customer);
        }

        public virtual async Task<AdminHeaderLinksModel> PrepareAdminHeaderLinks(Customer customer)
        {
            var model = new AdminHeaderLinksModel {
                ImpersonatedCustomerEmailUsername = customer.IsRegistered() ? (_customerSettings.UsernamesEnabled ? customer.Username : customer.Email) : "",
                IsCustomerImpersonated = _workContext.OriginalCustomerIfImpersonated != null,
                DisplayAdminLink = await _permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel),
                EditPageUrl = _pageHeadBuilder.GetEditPageUrl()
            };
            return model;
        }

        public virtual async Task<FooterModel> PrepareFooter()
        {
            var topicModel = (await _topicService.GetAllTopics(_storeContext.CurrentStore.Id))
                .Where(t => (t.IncludeInFooterRow1 || t.IncludeInFooterRow2 || t.IncludeInFooterRow3) && t.Published)
                .Select(t => new FooterModel.FooterTopicModel {
                    Id = t.Id,
                    Name = t.GetLocalized(x => x.Title, _workContext.WorkingLanguage.Id),
                    SeName = t.GetSeName(_workContext.WorkingLanguage.Id),
                    IncludeInFooterRow1 = t.IncludeInFooterRow1,
                    IncludeInFooterRow2 = t.IncludeInFooterRow2,
                    IncludeInFooterRow3 = t.IncludeInFooterRow3
                }).ToList();

            //model
            var currentstore = _storeContext.CurrentStore;
            var model = new FooterModel {
                StoreName = currentstore.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                CompanyEmail = currentstore.CompanyEmail,
                CompanyAddress = currentstore.CompanyAddress,
                CompanyPhone = currentstore.CompanyPhoneNumber,
                CompanyHours = currentstore.CompanyHours,
                WishlistEnabled = await _permissionService.Authorize(StandardPermissionProvider.EnableWishlist),
                ShoppingCartEnabled = await _permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart),
                SitemapEnabled = _commonSettings.SitemapEnabled,
                WorkingLanguageId = _workContext.WorkingLanguage.Id,
                FacebookLink = _storeInformationSettings.FacebookLink,
                TwitterLink = _storeInformationSettings.TwitterLink,
                YoutubeLink = _storeInformationSettings.YoutubeLink,
                InstagramLink = _storeInformationSettings.InstagramLink,
                LinkedInLink = _storeInformationSettings.LinkedInLink,
                PinterestLink = _storeInformationSettings.PinterestLink,
                BlogEnabled = _blogSettings.Enabled,
                KnowledgebaseEnabled = _knowledgebaseSettings.Enabled,
                CompareProductsEnabled = _catalogSettings.CompareProductsEnabled,
                ForumEnabled = _forumSettings.ForumsEnabled,
                NewsEnabled = _newsSettings.Enabled,
                RecentlyViewedProductsEnabled = _catalogSettings.RecentlyViewedProductsEnabled,
                RecommendedProductsEnabled = _catalogSettings.RecommendedProductsEnabled,
                NewProductsEnabled = _catalogSettings.NewProductsEnabled,
                DisplayTaxShippingInfoFooter = _catalogSettings.DisplayTaxShippingInfoFooter,
                InclTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax,
                HidePoweredByGrandNode = _storeInformationSettings.HidePoweredByGrandNode,
                AllowCustomersToApplyForVendorAccount = _vendorSettings.AllowCustomersToApplyForVendorAccount,
                Topics = topicModel
            };

            return model;
        }
       
        public virtual async Task<SitemapModel> PrepareSitemap()
        {
            string cacheKey = string.Format(ModelCacheEventConst.SITEMAP_PAGE_MODEL_KEY,
                _workContext.WorkingLanguage.Id,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            var cachedModel = await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var model = new SitemapModel {
                    BlogEnabled = _blogSettings.Enabled,
                    ForumEnabled = _forumSettings.ForumsEnabled,
                    NewsEnabled = _newsSettings.Enabled,
                    KnowledgebaseEnabled = _knowledgebaseSettings.Enabled
                };
                //categories
                if (_commonSettings.SitemapIncludeCategories)
                {
                    var categories = await _categoryService.GetAllCategories();
                    model.Categories = categories.Select(x => x.ToModel(_workContext.WorkingLanguage)).ToList();
                }
                //manufacturers
                if (_commonSettings.SitemapIncludeManufacturers)
                {
                    var manufacturers = await _manufacturerService.GetAllManufacturers();
                    model.Manufacturers = manufacturers.Select(x => x.ToModel(_workContext.WorkingLanguage)).ToList();
                }
                //products
                if (_commonSettings.SitemapIncludeProducts)
                {
                    //limit product to 200 until paging is supported on this page
                    var products = (await _productService.SearchProducts(
                        storeId: _storeContext.CurrentStore.Id,
                        visibleIndividuallyOnly: true,
                        pageSize: 200)).products;
                    model.Products = products.Select(product => new ProductOverviewModel {
                        Id = product.Id,
                        Name = product.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                        ShortDescription = product.GetLocalized(x => x.ShortDescription, _workContext.WorkingLanguage.Id),
                        FullDescription = product.GetLocalized(x => x.FullDescription, _workContext.WorkingLanguage.Id),
                        SeName = product.GetSeName(_workContext.WorkingLanguage.Id),
                    }).ToList();
                }

                //topics
                var topics = (await _topicService.GetAllTopics(_storeContext.CurrentStore.Id))
                    .Where(t => t.IncludeInSitemap)
                    .ToList();
                model.Topics = topics.Select(topic => new TopicModel {
                    Id = topic.Id,
                    SystemName = topic.SystemName,
                    IncludeInSitemap = topic.IncludeInSitemap,
                    IsPasswordProtected = topic.IsPasswordProtected,
                    Title = topic.GetLocalized(x => x.Title, _workContext.WorkingLanguage.Id),
                })
                .ToList();
                return model;
            });
            return cachedModel;
        }
        public virtual async Task<string> SitemapXml(int? id, [FromServices] IUrlHelper url)
        {
            var sitemapGenerator = _serviceProvider.GetRequiredService<ISitemapGenerator>();
            string cacheKey = string.Format(ModelCacheEventConst.SITEMAP_SEO_MODEL_KEY, id,
                _workContext.WorkingLanguage.Id,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            var siteMap = await _cacheManager.GetAsync(cacheKey, () => sitemapGenerator.Generate(url, id, _workContext.WorkingLanguage.Id));
            return siteMap;
        }
        public virtual StoreThemeSelectorModel PrepareStoreThemeSelector()
        {
            var model = new StoreThemeSelectorModel();
            var themeProvider = _serviceProvider.GetRequiredService<IThemeProvider>();

            var currentTheme = themeProvider.GetThemeConfiguration(_themeContext.WorkingThemeName);
            model.CurrentStoreTheme = new StoreThemeModel {
                Name = currentTheme.ThemeName,
                Title = currentTheme.ThemeTitle
            };
            model.AvailableStoreThemes = themeProvider.GetThemeConfigurations()
                .Select(x => new StoreThemeModel {
                    Name = x.ThemeName,
                    Title = x.ThemeTitle
                })
                .ToList();
            return model;
        }

        public virtual FaviconModel PrepareFavicon()
        {
            var model = new FaviconModel();
            var faviconFileName = string.Format("favicon-{0}.ico", _storeContext.CurrentStore.Id);
            var localFaviconPath = System.IO.Path.Combine(_hostingEnvironment.WebRootPath, faviconFileName);
            if (!System.IO.File.Exists(localFaviconPath))
            {
                //try loading a generic favicon
                faviconFileName = "favicon.ico";
                localFaviconPath = System.IO.Path.Combine(_hostingEnvironment.WebRootPath, faviconFileName);
                if (!System.IO.File.Exists(localFaviconPath))
                {
                    return model;
                }
            }
            model.FaviconUrl = _webHelper.GetStoreLocation() + faviconFileName;
            return model;
        }
        public virtual async Task<string> PrepareRobotsTextFile()
        {
            var sb = new StringBuilder();

            //if robots.txt exists, let's use it
            string robotsFile = System.IO.Path.Combine(CommonHelper.MapPath("~/"), "robots.custom.txt");
            if (System.IO.File.Exists(robotsFile))
            {
                //the robots.txt file exists
                string robotsFileContent = System.IO.File.ReadAllText(robotsFile);
                sb.Append(robotsFileContent);
            }
            else
            {
                //doesn't exist. Let's generate it (default behavior)

                var disallowPaths = new List<string>
                {
                    "/admin",
                    "/bin/",
                    "/content/files/",
                    "/content/files/exportimport/",
                    "/country/getstatesbycountryid",
                    "/install",
                    "/upgrade",
                    "/setproductreviewhelpfulness",
                };
                var localizableDisallowPaths = new List<string>
                {
                    "/addproducttocart/catalog/",
                    "/addproducttocart/details/",
                    "/backinstocksubscriptions/manage",
                    "/boards/forumsubscriptions",
                    "/boards/forumwatch",
                    "/boards/postedit",
                    "/boards/postdelete",
                    "/boards/postcreate",
                    "/boards/topicedit",
                    "/boards/topicdelete",
                    "/boards/topiccreate",
                    "/boards/topicmove",
                    "/boards/topicwatch",
                    "/cart",
                    "/checkout",
                    "/checkout/billingaddress",
                    "/checkout/completed",
                    "/checkout/confirm",
                    "/checkout/shippingaddress",
                    "/checkout/shippingmethod",
                    "/checkout/paymentinfo",
                    "/checkout/paymentmethod",
                    "/clearcomparelist",
                    "/compareproducts",
                    "/compareproducts/add/*",
                    "/customer/avatar",
                    "/customer/activation",
                    "/customer/addresses",
                    "/customer/changepassword",
                    "/customer/checkusernameavailability",
                    "/customer/downloadableproducts",
                    "/customer/info",
                    "/customer/auctions",
                    "/common/customeractioneventurl",
                    "/common/getactivepopup",
                    "/common/removepopup",
                    "/deletepm",
                    "/emailwishlist",
                    "/inboxupdate",
                    "/newsletter/subscriptionactivation",
                    "/onepagecheckout",
                    "/order/history",
                    "/orderdetails",
                    "/passwordrecovery/confirm",
                    "/poll/vote",
                    "/popupinteractiveform",
                    "/privatemessages",
                    "/returnrequest",
                    "/returnrequest/history",
                    "/rewardpoints/history",
                    "/sendpm",
                    "/sentupdate",
                    "/shoppingcart/*",
                    "/storeclosed",
                    "/subscribenewsletter",
                    "/subscribenewsletter/SaveCategories",
                    "/topic/authenticate",
                    "/viewpm",
                    "/uploadfileproductattribute",
                    "/uploadfilecheckoutattribute",
                    "/wishlist",
                };


                const string newLine = "\r\n"; //Environment.NewLine
                sb.Append("User-agent: *");
                sb.Append(newLine);
                //sitemaps
                if (_localizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
                {
                    //URLs are localizable. Append SEO code
                    foreach (var language in await _languageService.GetAllLanguages(storeId: _storeContext.CurrentStore.Id))
                    {
                        sb.AppendFormat("Sitemap: {0}{1}/sitemap.xml", _storeContext.CurrentStore.Url, language.UniqueSeoCode);
                        sb.Append(newLine);
                    }
                }
                else
                {
                    //localizable paths (without SEO code)
                    sb.AppendFormat("Sitemap: {0}sitemap.xml", _storeContext.CurrentStore.Url);
                    sb.Append(newLine);
                }
                //host
                sb.AppendFormat("Host: {0}", _webHelper.GetStoreLocation());
                sb.Append(newLine);

                //usual paths
                foreach (var path in disallowPaths)
                {
                    sb.AppendFormat("Disallow: {0}", path);
                    sb.Append(newLine);
                }
                //localizable paths (without SEO code)
                foreach (var path in localizableDisallowPaths)
                {
                    sb.AppendFormat("Disallow: {0}", path);
                    sb.Append(newLine);
                }
                if (_localizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
                {
                    //URLs are localizable. Append SEO code
                    foreach (var language in await _languageService.GetAllLanguages(storeId: _storeContext.CurrentStore.Id))
                    {
                        foreach (var path in localizableDisallowPaths)
                        {
                            sb.AppendFormat("Disallow: /{0}{1}", language.UniqueSeoCode, path);
                            sb.Append(newLine);
                        }
                    }
                }

                //load and add robots.txt additions to the end of file.
                string robotsAdditionsFile = System.IO.Path.Combine(CommonHelper.MapPath("~/"), "robots.additions.txt");
                if (System.IO.File.Exists(robotsAdditionsFile))
                {
                    string robotsFileContent = System.IO.File.ReadAllText(robotsAdditionsFile);
                    sb.Append(robotsFileContent);
                }

            }

            return sb.ToString();

        }
    }
}