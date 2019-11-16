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
using Grand.Framework.Security.Captcha;
using Grand.Framework.Themes;
using Grand.Framework.UI;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Forums;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Messages;
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
using Microsoft.AspNetCore.Http;
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
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ILocalizationService _localizationService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductService _productService;
        private readonly IContactAttributeService _contactAttributeService;
        private readonly IContactAttributeParser _contactAttributeParser;
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
        private readonly CaptchaSettings _captchaSettings;
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
            IWorkflowMessageService workflowMessageService,
            ILocalizationService localizationService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IProductService productService,
            IContactAttributeService contactAttributeService,
            IContactAttributeParser contactAttributeParser,
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
            CaptchaSettings captchaSettings,
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
            _workflowMessageService = workflowMessageService;
            _localizationService = localizationService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _productService = productService;
            _contactAttributeService = contactAttributeService;
            _contactAttributeParser = contactAttributeParser;
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
            _captchaSettings = captchaSettings;
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

            var cacheKey = string.Format(ModelCacheEventConsumer.STORE_LOGO_PATH, _storeContext.CurrentStore.Id, _themeContext.WorkingThemeName, _webHelper.IsCurrentConnectionSecured());
            model.LogoPath = await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var logo = "";
                var logoPictureId = _storeInformationSettings.LogoPictureId;
                if (!String.IsNullOrEmpty(logoPictureId))
                {
                    logo = await _pictureService.GetPictureUrl(logoPictureId, showDefaultPicture: false);
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
            var availableLanguages = await _cacheManager.GetAsync(string.Format(ModelCacheEventConsumer.AVAILABLE_LANGUAGES_MODEL_KEY, _storeContext.CurrentStore.Id), async () =>
            {
                var result = (await _languageService
                    .GetAllLanguages(storeId: _storeContext.CurrentStore.Id))
                    .Select(x => new LanguageModel {
                        Id = x.Id,
                        Name = x.Name,
                        FlagImageFileName = x.FlagImageFileName,
                    })
                    .ToList();
                return result;
            });

            var model = new LanguageSelectorModel {
                CurrentLanguageId = _workContext.WorkingLanguage.Id,
                AvailableLanguages = availableLanguages,
                UseImages = _localizationSettings.UseImagesForLanguageSelection
            };

            return model;
        }

        public virtual async Task<CurrencySelectorModel> PrepareCurrencySelector()
        {
            var availableCurrencies = await _cacheManager.GetAsync(string.Format(ModelCacheEventConsumer.AVAILABLE_CURRENCIES_MODEL_KEY, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id),
                async () =>
            {
                var result = (await _currencyService
                    .GetAllCurrencies(storeId: _storeContext.CurrentStore.Id))
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
                    })
                    .ToList();
                return result;
            });

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

            var availableStores = await _cacheManager.GetAsync(ModelCacheEventConsumer.AVAILABLE_STORES_MODEL_KEY, async () =>
            {
                var storeService = _serviceProvider.GetRequiredService<IStoreService>();
                var result = (await storeService.GetAllStores())
                    .Select(x => new StoreModel {
                        Id = x.Id,
                        Name = x.Name,
                    })
                    .ToList();
                return result;
            });

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
            //footer topics
            string topicCacheKey = string.Format(ModelCacheEventConsumer.TOPIC_FOOTER_MODEL_KEY,
                _workContext.WorkingLanguage.Id,
                _storeContext.CurrentStore.Id,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()));
            var cachedTopicModel = await _cacheManager.GetAsync(topicCacheKey, async () =>
                (await _topicService.GetAllTopics(_storeContext.CurrentStore.Id))
                .Where(t => (t.IncludeInFooterRow1 || t.IncludeInFooterRow2 || t.IncludeInFooterRow3) && t.Published)
                .Select(t => new FooterModel.FooterTopicModel {
                    Id = t.Id,
                    Name = t.GetLocalized(x => x.Title, _workContext.WorkingLanguage.Id),
                    SeName = t.GetSeName(_workContext.WorkingLanguage.Id),
                    IncludeInFooterRow1 = t.IncludeInFooterRow1,
                    IncludeInFooterRow2 = t.IncludeInFooterRow2,
                    IncludeInFooterRow3 = t.IncludeInFooterRow3
                })
                .ToList()
            );

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
                Topics = cachedTopicModel
            };

            return model;
        }

        public virtual async Task<string> ParseContactAttributes(IFormCollection form)
        {

            if (form == null)
                throw new ArgumentNullException("form");

            string attributesXml = "";
            var checkoutAttributes = await _contactAttributeService.GetAllContactAttributes(_storeContext.CurrentStore.Id);
            foreach (var attribute in checkoutAttributes)
            {
                string controlId = string.Format("contact_attribute_{0}", attribute.Id);
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                attributesXml = _contactAttributeParser.AddContactAttribute(attributesXml,
                                        attribute, ctrlAttributes);

                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var cblAttributes = form[controlId].ToString();
                            if (!String.IsNullOrEmpty(cblAttributes))
                            {
                                foreach (var item in cblAttributes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    attributesXml = _contactAttributeParser.AddContactAttribute(attributesXml, attribute, item);
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = attribute.ContactAttributeValues;
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                attributesXml = _contactAttributeParser.AddContactAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = form[controlId].ToString();
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                string enteredText = ctrlAttributes.Trim();
                                attributesXml = _contactAttributeParser.AddContactAttribute(attributesXml,
                                    attribute, enteredText);
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                        {
                            var date = form[controlId + "_day"];
                            var month = form[controlId + "_month"];
                            var year = form[controlId + "_year"];
                            DateTime? selectedDate = null;
                            try
                            {
                                selectedDate = new DateTime(Int32.Parse(year), Int32.Parse(month), Int32.Parse(date));
                            }
                            catch { }
                            if (selectedDate.HasValue)
                            {
                                attributesXml = _contactAttributeParser.AddContactAttribute(attributesXml,
                                    attribute, selectedDate.Value.ToString("D"));
                            }
                        }
                        break;
                    case AttributeControlType.FileUpload:
                        {
                            Guid downloadGuid;
                            Guid.TryParse(form[controlId], out downloadGuid);
                            var _downloadService = _serviceProvider.GetRequiredService<IDownloadService>();
                            var download = await _downloadService.GetDownloadByGuid(downloadGuid);
                            if (download != null)
                            {
                                attributesXml = _contactAttributeParser.AddContactAttribute(attributesXml,
                                           attribute, download.DownloadGuid.ToString());
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            //save checkout attributes
            //validate conditional attributes (if specified)
            foreach (var attribute in checkoutAttributes)
            {
                var conditionMet = await _contactAttributeParser.IsConditionMet(attribute, attributesXml);
                if (conditionMet.HasValue && !conditionMet.Value)
                    attributesXml = _contactAttributeParser.RemoveContactAttribute(attributesXml, attribute);
            }

            return attributesXml;
        }

        public virtual async Task<IList<string>> GetContactAttributesWarnings(string contactAttributesXml)
        {
            var warnings = new List<string>();

            //selected attributes
            var attributes1 = await _contactAttributeParser.ParseContactAttributes(contactAttributesXml);

            //existing checkout attributes
            var attributes2 = await _contactAttributeService.GetAllContactAttributes(_storeContext.CurrentStore.Id);
            foreach (var a2 in attributes2)
            {
                if (a2.IsRequired)
                {
                    bool found = false;
                    //selected checkout attributes
                    foreach (var a1 in attributes1)
                    {
                        if (a1.Id == a2.Id)
                        {
                            var attributeValuesStr = _contactAttributeParser.ParseValues(contactAttributesXml, a1.Id);
                            foreach (string str1 in attributeValuesStr)
                                if (!String.IsNullOrEmpty(str1.Trim()))
                                {
                                    found = true;
                                    break;
                                }
                        }
                    }

                    //if not found
                    if (!found)
                    {
                        if (!string.IsNullOrEmpty(a2.GetLocalized(a => a.TextPrompt, _workContext.WorkingLanguage.Id)))
                            warnings.Add(a2.GetLocalized(a => a.TextPrompt, _workContext.WorkingLanguage.Id));
                        else
                            warnings.Add(string.Format(_localizationService.GetResource("ContactUs.SelectAttribute"), a2.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id)));
                    }
                }
            }

            //now validation rules

            //minimum length
            foreach (var ca in attributes2)
            {
                if (ca.ValidationMinLength.HasValue)
                {
                    if (ca.AttributeControlType == AttributeControlType.TextBox ||
                        ca.AttributeControlType == AttributeControlType.MultilineTextbox)
                    {
                        var valuesStr = _contactAttributeParser.ParseValues(contactAttributesXml, ca.Id);
                        var enteredText = valuesStr.FirstOrDefault();
                        int enteredTextLength = String.IsNullOrEmpty(enteredText) ? 0 : enteredText.Length;

                        if (ca.ValidationMinLength.Value > enteredTextLength)
                        {
                            warnings.Add(string.Format(_localizationService.GetResource("ContactUs.TextboxMinimumLength"), ca.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id), ca.ValidationMinLength.Value));
                        }
                    }
                }

                //maximum length
                if (ca.ValidationMaxLength.HasValue)
                {
                    if (ca.AttributeControlType == AttributeControlType.TextBox ||
                        ca.AttributeControlType == AttributeControlType.MultilineTextbox)
                    {
                        var valuesStr = _contactAttributeParser.ParseValues(contactAttributesXml, ca.Id);
                        var enteredText = valuesStr.FirstOrDefault();
                        int enteredTextLength = String.IsNullOrEmpty(enteredText) ? 0 : enteredText.Length;

                        if (ca.ValidationMaxLength.Value < enteredTextLength)
                        {
                            warnings.Add(string.Format(_localizationService.GetResource("ContactUs.TextboxMaximumLength"), ca.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id), ca.ValidationMaxLength.Value));
                        }
                    }
                }
            }

            return warnings;
        }

        public virtual async Task<IList<ContactUsModel.ContactAttributeModel>> PrepareContactAttributeModel(string selectedContactAttributes)
        {
            var model = new List<ContactUsModel.ContactAttributeModel>();

            var contactAttributes = await _contactAttributeService.GetAllContactAttributes(_storeContext.CurrentStore.Id);
            foreach (var attribute in contactAttributes)
            {
                var attributeModel = new ContactUsModel.ContactAttributeModel {
                    Id = attribute.Id,
                    Name = attribute.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                    TextPrompt = attribute.GetLocalized(x => x.TextPrompt, _workContext.WorkingLanguage.Id),
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                    DefaultValue = attribute.DefaultValue
                };
                if (!String.IsNullOrEmpty(attribute.ValidationFileAllowedExtensions))
                {
                    attributeModel.AllowedFileExtensions = attribute.ValidationFileAllowedExtensions
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                }

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = attribute.ContactAttributeValues;
                    foreach (var attributeValue in attributeValues)
                    {
                        var attributeValueModel = new ContactUsModel.ContactAttributeValueModel {
                            Id = attributeValue.Id,
                            Name = attributeValue.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                            ColorSquaresRgb = attributeValue.ColorSquaresRgb,
                            IsPreSelected = attributeValue.IsPreSelected,
                            DisplayOrder = attributeValue.DisplayOrder,
                        };
                        attributeModel.Values.Add(attributeValueModel);
                    }
                }

                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.Checkboxes:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                        {
                            if (!String.IsNullOrEmpty(selectedContactAttributes))
                            {
                                //clear default selection
                                foreach (var item in attributeModel.Values)
                                    item.IsPreSelected = false;

                                //select new values
                                var selectedValues = await _contactAttributeParser.ParseContactAttributeValues(selectedContactAttributes);
                                foreach (var attributeValue in selectedValues)
                                    if (attributeModel.Id == attributeValue.ContactAttributeId)
                                        foreach (var item in attributeModel.Values)
                                            if (attributeValue.Id == item.Id)
                                                item.IsPreSelected = true;
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //do nothing
                            //values are already pre-set
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            if (!String.IsNullOrEmpty(selectedContactAttributes))
                            {
                                var enteredText = _contactAttributeParser.ParseValues(selectedContactAttributes, attribute.Id);
                                if (enteredText.Any())
                                    attributeModel.DefaultValue = enteredText[0];
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                        {
                            //keep in mind my that the code below works only in the current culture
                            var selectedDateStr = _contactAttributeParser.ParseValues(selectedContactAttributes, attribute.Id);
                            if (selectedDateStr.Any())
                            {
                                DateTime selectedDate;
                                if (DateTime.TryParseExact(selectedDateStr[0], "D", CultureInfo.CurrentCulture,
                                                       DateTimeStyles.None, out selectedDate))
                                {
                                    //successfully parsed
                                    attributeModel.SelectedDay = selectedDate.Day;
                                    attributeModel.SelectedMonth = selectedDate.Month;
                                    attributeModel.SelectedYear = selectedDate.Year;
                                }
                            }

                        }
                        break;
                    case AttributeControlType.FileUpload:
                        {
                            if (!String.IsNullOrEmpty(selectedContactAttributes))
                            {
                                var downloadGuidStr = _contactAttributeParser.ParseValues(selectedContactAttributes, attribute.Id).FirstOrDefault();
                                Guid downloadGuid;
                                Guid.TryParse(downloadGuidStr, out downloadGuid);
                                var download = await _serviceProvider.GetRequiredService<IDownloadService>().GetDownloadByGuid(downloadGuid);
                                if (download != null)
                                    attributeModel.DefaultValue = download.DownloadGuid.ToString();
                            }
                        }
                        break;
                    default:
                        break;
                }

                model.Add(attributeModel);
            }

            return model;
        }

        public virtual async Task<ContactUsModel> PrepareContactUs()
        {
            var model = new ContactUsModel {
                Email = _workContext.CurrentCustomer.Email,
                FullName = _workContext.CurrentCustomer.GetFullName(),
                SubjectEnabled = _commonSettings.SubjectFieldOnContactUsForm,
                DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnContactUsPage
            };
            model.ContactAttributes = await PrepareContactAttributeModel("");
            return model;
        }

        public virtual async Task<ContactUsModel> SendContactUs(ContactUsModel model)
        {
            string email = model.Email.Trim();
            string fullName = model.FullName;
            string subject = _commonSettings.SubjectFieldOnContactUsForm ? model.Subject : null;
            string body = Core.Html.HtmlHelper.FormatText(model.Enquiry, false, true, false, false, false, false);

            await _workflowMessageService.SendContactUsMessage(_workContext.CurrentCustomer, _workContext.WorkingLanguage.Id, model.Email.Trim(), model.FullName, subject, body, model.ContactAttributeInfo, model.ContactAttributeXml);

            model.SuccessfullySent = true;
            model.Result = _localizationService.GetResource("ContactUs.YourEnquiryHasBeenSent");

            return model;
        }
        public virtual async Task<ContactVendorModel> PrepareContactVendor(Vendor vendor)
        {
            var model = new ContactVendorModel {
                Email = _workContext.CurrentCustomer.Email,
                FullName = _workContext.CurrentCustomer.GetFullName(),
                SubjectEnabled = _commonSettings.SubjectFieldOnContactUsForm,
                DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnContactUsPage,
                VendorId = vendor.Id,
                VendorName = vendor.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id)
            };
            return await Task.FromResult(model);
        }

        public virtual async Task<ContactVendorModel> SendContactVendor(ContactVendorModel model, Vendor vendor)
        {
            string subject = _commonSettings.SubjectFieldOnContactUsForm ? model.Subject : null;
            string body = Core.Html.HtmlHelper.FormatText(model.Enquiry, false, true, false, false, false, false);

            await _workflowMessageService.SendContactVendorMessage(_workContext.CurrentCustomer, vendor, _workContext.WorkingLanguage.Id, model.Email.Trim(), model.FullName, subject, body);

            model.SuccessfullySent = true;
            model.Result = _localizationService.GetResource("ContactVendor.YourEnquiryHasBeenSent");
            return model;
        }

        public virtual async Task<SitemapModel> PrepareSitemap()
        {
            string cacheKey = string.Format(ModelCacheEventConsumer.SITEMAP_PAGE_MODEL_KEY,
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
        public virtual async Task<string> SitemapXml(int? id, IUrlHelper url)
        {
            var sitemapGenerator = _serviceProvider.GetRequiredService<ISitemapGenerator>();
            string cacheKey = string.Format(ModelCacheEventConsumer.SITEMAP_SEO_MODEL_KEY, id,
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