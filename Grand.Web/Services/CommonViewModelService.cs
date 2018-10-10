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
using Grand.Web.Models.Catalog;
using Grand.Web.Models.Common;
using Grand.Web.Models.Topics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Grand.Web.Services
{
    public partial class CommonViewModelService: ICommonViewModelService
    {
        private readonly ICacheManager _cacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
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
        private readonly ISitemapGenerator _sitemapGenerator;
        private readonly IThemeProvider _themeProvider;
        private readonly IForumService _forumservice;
        private readonly IContactAttributeService _contactAttributeService;
        private readonly IContactAttributeParser _contactAttributeParser;
        private readonly IHostingEnvironment _hostingEnvironment;
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

        public CommonViewModelService(ICacheManager cacheManager, 
            IStoreContext storeContext,
            IStoreService storeService,
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
            ISitemapGenerator sitemapGenerator,
            IThemeProvider themeProvider,
            IForumService forumservice,
            IContactAttributeService contactAttributeService,
            IContactAttributeParser contactAttributeParser,
            IHostingEnvironment hostingEnvironment,
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
            CaptchaSettings captchaSettings
            )
        {
            this._cacheManager = cacheManager;
            this._storeContext = storeContext;
            this._storeService = storeService;
            this._themeContext = themeContext;
            this._pictureService = pictureService;
            this._webHelper = webHelper;
            this._languageService = languageService;
            this._workContext = workContext;
            this._currencyService = currencyService;
            this._permissionService = permissionService;
            this._pageHeadBuilder = pageHeadBuilder;
            this._topicService = topicService;
            this._workflowMessageService = workflowMessageService;
            this._localizationService = localizationService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._productService = productService;
            this._sitemapGenerator = sitemapGenerator;
            this._themeProvider = themeProvider;
            this._forumservice = forumservice;
            this._contactAttributeService = contactAttributeService;
            this._contactAttributeParser = contactAttributeParser;
            this._hostingEnvironment = hostingEnvironment;

            this._storeInformationSettings = storeInformationSettings;
            this._localizationSettings = localizationSettings;
            this._taxSettings = taxSettings;
            this._customerSettings = customerSettings;
            this._forumSettings = forumSettings;
            this._catalogSettings = catalogSettings;
            this._blogSettings = blogSettings;
            this._knowledgebaseSettings = knowledgebaseSettings;
            this._newsSettings = newsSettings;
            this._vendorSettings = vendorSettings;
            this._commonSettings = commonSettings;
            this._captchaSettings = captchaSettings;
        }
        public virtual LogoModel PrepareLogo()
        {
            var model = new LogoModel
            {
                StoreName = _storeContext.CurrentStore.GetLocalized(x => x.Name)
            };

            var cacheKey = string.Format(ModelCacheEventConsumer.STORE_LOGO_PATH, _storeContext.CurrentStore.Id, _themeContext.WorkingThemeName, _webHelper.IsCurrentConnectionSecured());
            model.LogoPath = _cacheManager.Get(cacheKey, () =>
            {
                var logo = "";
                var logoPictureId = _storeInformationSettings.LogoPictureId;
                if (!String.IsNullOrEmpty(logoPictureId))
                {
                    logo = _pictureService.GetPictureUrl(logoPictureId, showDefaultPicture: false);
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

        public virtual LanguageSelectorModel PrepareLanguageSelector()
        {
            var availableLanguages = _cacheManager.Get(string.Format(ModelCacheEventConsumer.AVAILABLE_LANGUAGES_MODEL_KEY, _storeContext.CurrentStore.Id), () =>
            {
                var result = _languageService
                    .GetAllLanguages(storeId: _storeContext.CurrentStore.Id)
                    .Select(x => new LanguageModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        FlagImageFileName = x.FlagImageFileName,
                    })
                    .ToList();
                return result;
            });

            var model = new LanguageSelectorModel
            {
                CurrentLanguageId = _workContext.WorkingLanguage.Id,
                AvailableLanguages = availableLanguages,
                UseImages = _localizationSettings.UseImagesForLanguageSelection
            };

            return model;
        }
        public virtual void SetLanguage(string langid)
        {
            var language = _languageService.GetLanguageById(langid);
            if (language != null && language.Published)
            {
                _workContext.WorkingLanguage = language;
            }
        }

        public virtual CurrencySelectorModel PrepareCurrencySelector()
        {
            var availableCurrencies = _cacheManager.Get(string.Format(ModelCacheEventConsumer.AVAILABLE_CURRENCIES_MODEL_KEY, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id), () =>
            {
                var result = _currencyService
                    .GetAllCurrencies(storeId: _storeContext.CurrentStore.Id)
                    .Select(x =>
                    {
                        //currency char
                        var currencySymbol = "";
                        if (!string.IsNullOrEmpty(x.DisplayLocale))
                            currencySymbol = new RegionInfo(x.DisplayLocale).CurrencySymbol;
                        else
                            currencySymbol = x.CurrencyCode;
                        //model
                        var currencyModel = new CurrencyModel
                        {
                            Id = x.Id,
                            Name = x.GetLocalized(y => y.Name),
                            CurrencyCode = x.CurrencyCode,
                            CurrencySymbol = currencySymbol
                        };
                        return currencyModel;
                    })
                    .ToList();
                return result;
            });

            var model = new CurrencySelectorModel
            {
                CurrentCurrencyId = _workContext.WorkingCurrency.Id,
                AvailableCurrencies = availableCurrencies
            };

            return model;
        }

        public virtual void SetCurrency(string customerCurrency)
        {
            var currency = _currencyService.GetCurrencyById(customerCurrency);
            if (currency != null)
                _workContext.WorkingCurrency = currency;

        }
        public virtual TaxTypeSelectorModel PrepareTaxTypeSelector()
        {
            if (!_taxSettings.AllowCustomersToSelectTaxDisplayType)
                return null;

            var model = new TaxTypeSelectorModel
            {
                CurrentTaxType = _workContext.TaxDisplayType
            };
            return model;
        }
        
        public virtual void SetTaxType(int customerTaxType)
        {
            var taxDisplayType = (TaxDisplayType)Enum.ToObject(typeof(TaxDisplayType), customerTaxType);
            _workContext.TaxDisplayType = taxDisplayType;
        }

        public virtual StoreSelectorModel PrepareStoreSelector()
        {
            if (!_commonSettings.AllowToSelectStore)
                return null;

            var availableStores = _cacheManager.Get(ModelCacheEventConsumer.AVAILABLE_STORES_MODEL_KEY, () =>
            {
                var result = _storeService.GetAllStores()
                    .Select(x => new StoreModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                    })
                    .ToList();
                return result;
            });

            var model = new StoreSelectorModel
            {
                CurrentStoreId = _storeContext.CurrentStore.Id,
                AvailableStores = availableStores,
            };

            return model;
        }

        public virtual void SetStore(string storeid)
        {
            if (_commonSettings.AllowToSelectStore)
            {
                var store = _storeService.GetStoreById(storeid);
                if (store != null)
                    _storeContext.CurrentStore = store;
            }
        }

        public virtual int GetUnreadPrivateMessages()
        {
            var result = 0;
            var customer = _workContext.CurrentCustomer;
            if (_forumSettings.AllowPrivateMessages && !customer.IsGuest())
            {
                var privateMessages = _forumservice.GetAllPrivateMessages(_storeContext.CurrentStore.Id,
                    "", customer.Id, false, null, false, string.Empty, 0, 1);

                if (privateMessages.Any())
                {
                    result = privateMessages.TotalCount;
                }
            }
            return result;

        }

        public virtual HeaderLinksModel PrepareHeaderLinks(Customer customer)
        {
            var isRegister = customer.IsRegistered();
            var model = new HeaderLinksModel
            {
                IsAuthenticated = isRegister,
                CustomerEmailUsername = isRegister ? (_customerSettings.UsernamesEnabled ? customer.Username : customer.Email) : "",
                ShoppingCartEnabled = _permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart),
                WishlistEnabled = _permissionService.Authorize(StandardPermissionProvider.EnableWishlist),
                AllowPrivateMessages = isRegister && _forumSettings.AllowPrivateMessages,
            };
            //performance optimization (use "HasShoppingCartItems" property)
            if (customer.ShoppingCartItems.Any())
            {
                model.ShoppingCartItems = customer.ShoppingCartItems
                    .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart || sci.ShoppingCartType == ShoppingCartType.Auctions)
                    .LimitPerStore(_storeContext.CurrentStore.Id)
                    .Sum(x => x.Quantity);

                model.WishlistItems = customer.ShoppingCartItems
                    .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                    .LimitPerStore(_storeContext.CurrentStore.Id)
                    .Sum(x => x.Quantity);

            }

            return model;
        }
        public virtual AdminHeaderLinksModel PrepareAdminHeaderLinks(Customer customer)
        {
            var model = new AdminHeaderLinksModel
            {
                ImpersonatedCustomerEmailUsername = customer.IsRegistered() ? (_customerSettings.UsernamesEnabled ? customer.Username : customer.Email) : "",
                IsCustomerImpersonated = _workContext.OriginalCustomerIfImpersonated != null,
                DisplayAdminLink = _permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel),
                EditPageUrl = _pageHeadBuilder.GetEditPageUrl()
            };
            return model;
        }
        public virtual FooterModel PrepareFooter()
        {
            //footer topics
            string topicCacheKey = string.Format(ModelCacheEventConsumer.TOPIC_FOOTER_MODEL_KEY,
                _workContext.WorkingLanguage.Id,
                _storeContext.CurrentStore.Id,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()));
            var cachedTopicModel = _cacheManager.Get(topicCacheKey, () =>
                _topicService.GetAllTopics(_storeContext.CurrentStore.Id)
                .Where(t => t.IncludeInFooterColumn1 || t.IncludeInFooterColumn2 || t.IncludeInFooterColumn3)
                .Select(t => new FooterModel.FooterTopicModel
                {
                    Id = t.Id,
                    Name = t.GetLocalized(x => x.Title),
                    SeName = t.GetSeName(),
                    IncludeInFooterColumn1 = t.IncludeInFooterColumn1,
                    IncludeInFooterColumn2 = t.IncludeInFooterColumn2,
                    IncludeInFooterColumn3 = t.IncludeInFooterColumn3
                })
                .ToList()
            );

            //model
            var model = new FooterModel
            {
                StoreName = _storeContext.CurrentStore.GetLocalized(x => x.Name),
                WishlistEnabled = _permissionService.Authorize(StandardPermissionProvider.EnableWishlist),
                ShoppingCartEnabled = _permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart),
                SitemapEnabled = _commonSettings.SitemapEnabled,
                WorkingLanguageId = _workContext.WorkingLanguage.Id,
                FacebookLink = _storeInformationSettings.FacebookLink,
                TwitterLink = _storeInformationSettings.TwitterLink,
                YoutubeLink = _storeInformationSettings.YoutubeLink,
                GooglePlusLink = _storeInformationSettings.GooglePlusLink,
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
                HidePoweredByGrandNode = _storeInformationSettings.HidePoweredByGrandNode,
                AllowCustomersToApplyForVendorAccount = _vendorSettings.AllowCustomersToApplyForVendorAccount,
                Topics = cachedTopicModel
            };

            return model;
        }

        public virtual string ParseContactAttributes(IFormCollection form)
        {

            if (form == null)
                throw new ArgumentNullException("form");

            string attributesXml = "";
            var checkoutAttributes = _contactAttributeService.GetAllContactAttributes(_storeContext.CurrentStore.Id);
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
                            var _downloadService = Grand.Core.Infrastructure.EngineContext.Current.Resolve<IDownloadService>();
                            var download = _downloadService.GetDownloadByGuid(downloadGuid);
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
                var conditionMet = _contactAttributeParser.IsConditionMet(attribute, attributesXml);
                if (conditionMet.HasValue && !conditionMet.Value)
                    attributesXml = _contactAttributeParser.RemoveContactAttribute(attributesXml, attribute);
            }

            return attributesXml;
        }

        public virtual IList<string> GetContactAttributesWarnings(string contactAttributesXml)
        {
            var warnings = new List<string>();

            //selected attributes
            var attributes1 = _contactAttributeParser.ParseContactAttributes(contactAttributesXml);

            //existing checkout attributes
            var attributes2 = _contactAttributeService.GetAllContactAttributes(_storeContext.CurrentStore.Id);
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
                        if (!string.IsNullOrEmpty(a2.GetLocalized(a => a.TextPrompt)))
                            warnings.Add(a2.GetLocalized(a => a.TextPrompt));
                        else
                            warnings.Add(string.Format(_localizationService.GetResource("ContactUs.SelectAttribute"), a2.GetLocalized(a => a.Name)));
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
                            warnings.Add(string.Format(_localizationService.GetResource("ContactUs.TextboxMinimumLength"), ca.GetLocalized(a => a.Name), ca.ValidationMinLength.Value));
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
                            warnings.Add(string.Format(_localizationService.GetResource("ContactUs.TextboxMaximumLength"), ca.GetLocalized(a => a.Name), ca.ValidationMaxLength.Value));
                        }
                    }
                }
            }

            return warnings;
        }

        public virtual IList<ContactUsModel.ContactAttributeModel> PrepareContactAttributeModel(string selectedContactAttributes)
        {
            var model = new List<ContactUsModel.ContactAttributeModel>();

            var contactAttributes = _contactAttributeService.GetAllContactAttributes(_storeContext.CurrentStore.Id);
            foreach (var attribute in contactAttributes)
            {
                var attributeModel = new ContactUsModel.ContactAttributeModel
                {
                    Id = attribute.Id,
                    Name = attribute.GetLocalized(x => x.Name),
                    TextPrompt = attribute.GetLocalized(x => x.TextPrompt),
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
                        var attributeValueModel = new ContactUsModel.ContactAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = attributeValue.GetLocalized(x => x.Name),
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
                                var selectedValues = _contactAttributeParser.ParseContactAttributeValues(selectedContactAttributes);
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
                                var download = Grand.Core.Infrastructure.EngineContext.Current.Resolve<IDownloadService>().GetDownloadByGuid(downloadGuid);
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

        public virtual ContactUsModel PrepareContactUs()
        {
            var model = new ContactUsModel
            {
                Email = _workContext.CurrentCustomer.Email,
                FullName = _workContext.CurrentCustomer.GetFullName(),
                SubjectEnabled = _commonSettings.SubjectFieldOnContactUsForm,
                DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnContactUsPage
            };
            model.ContactAttributes = PrepareContactAttributeModel("");
            return model;
        }

        public virtual ContactUsModel SendContactUs(ContactUsModel model)
        {
            string email = model.Email.Trim();
            string fullName = model.FullName;
            string subject = _commonSettings.SubjectFieldOnContactUsForm ? model.Subject : null;
            string body = Core.Html.HtmlHelper.FormatText(model.Enquiry, false, true, false, false, false, false);

            _workflowMessageService.SendContactUsMessage(_workContext.CurrentCustomer, _workContext.WorkingLanguage.Id, model.Email.Trim(), model.FullName, subject, body, model.ContactAttributeInfo, model.ContactAttributeXml);

            model.SuccessfullySent = true;
            model.Result = _localizationService.GetResource("ContactUs.YourEnquiryHasBeenSent");
            
            return model;
        }
        public virtual ContactVendorModel PrepareContactVendor(Vendor vendor)
        {
            var model = new ContactVendorModel
            {
                Email = _workContext.CurrentCustomer.Email,
                FullName = _workContext.CurrentCustomer.GetFullName(),
                SubjectEnabled = _commonSettings.SubjectFieldOnContactUsForm,
                DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnContactUsPage,
                VendorId = vendor.Id,
                VendorName = vendor.GetLocalized(x => x.Name)
            };
            return model;
        }

        public virtual ContactVendorModel SendContactVendor(ContactVendorModel model, Vendor vendor)
        {
            string subject = _commonSettings.SubjectFieldOnContactUsForm ? model.Subject : null;
            string body = Core.Html.HtmlHelper.FormatText(model.Enquiry, false, true, false, false, false, false);

            _workflowMessageService.SendContactVendorMessage(_workContext.CurrentCustomer, vendor, _workContext.WorkingLanguage.Id, model.Email.Trim(), model.FullName, subject, body);

            model.SuccessfullySent = true;
            model.Result = _localizationService.GetResource("ContactVendor.YourEnquiryHasBeenSent");
            return model;
        }

        public virtual SitemapModel PrepareSitemap()
        {
            string cacheKey = string.Format(ModelCacheEventConsumer.SITEMAP_PAGE_MODEL_KEY,
                _workContext.WorkingLanguage.Id,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            var cachedModel = _cacheManager.Get(cacheKey, () =>
            {
                var model = new SitemapModel
                {
                    BlogEnabled = _blogSettings.Enabled,
                    ForumEnabled = _forumSettings.ForumsEnabled,
                    NewsEnabled = _newsSettings.Enabled,
                    KnowledgebaseEnabled = _knowledgebaseSettings.Enabled
                };
                //categories
                if (_commonSettings.SitemapIncludeCategories)
                {
                    var categories = _categoryService.GetAllCategories();
                    model.Categories = categories.Select(x => x.ToModel()).ToList();
                }
                //manufacturers
                if (_commonSettings.SitemapIncludeManufacturers)
                {
                    var manufacturers = _manufacturerService.GetAllManufacturers();
                    model.Manufacturers = manufacturers.Select(x => x.ToModel()).ToList();
                }
                //products
                if (_commonSettings.SitemapIncludeProducts)
                {
                    //limit product to 200 until paging is supported on this page
                    var products = _productService.SearchProducts(storeId: _storeContext.CurrentStore.Id,
                        visibleIndividuallyOnly: true,
                        pageSize: 200);
                    model.Products = products.Select(product => new ProductOverviewModel
                    {
                        Id = product.Id,
                        Name = product.GetLocalized(x => x.Name),
                        ShortDescription = product.GetLocalized(x => x.ShortDescription),
                        FullDescription = product.GetLocalized(x => x.FullDescription),
                        SeName = product.GetSeName(),
                    }).ToList();
                }

                //topics
                var topics = _topicService.GetAllTopics(_storeContext.CurrentStore.Id)
                    .Where(t => t.IncludeInSitemap)
                    .ToList();
                model.Topics = topics.Select(topic => new TopicModel
                {
                    Id = topic.Id,
                    SystemName = topic.SystemName,
                    IncludeInSitemap = topic.IncludeInSitemap,
                    IsPasswordProtected = topic.IsPasswordProtected,
                    Title = topic.GetLocalized(x => x.Title),
                })
                .ToList();
                return model;
            });
            return cachedModel;
        }
        public virtual string SitemapXml(int? id, IUrlHelper url)
        {
            string cacheKey = string.Format(ModelCacheEventConsumer.SITEMAP_SEO_MODEL_KEY, id,
                _workContext.WorkingLanguage.Id,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            var siteMap = _cacheManager.Get(cacheKey, () => _sitemapGenerator.Generate(url, id));
            return siteMap;
        }
        public virtual StoreThemeSelectorModel PrepareStoreThemeSelector()
        {
            var model = new StoreThemeSelectorModel();
            var currentTheme = _themeProvider.GetThemeConfiguration(_themeContext.WorkingThemeName);
            model.CurrentStoreTheme = new StoreThemeModel
            {
                Name = currentTheme.ThemeName,
                Title = currentTheme.ThemeTitle
            };
            model.AvailableStoreThemes = _themeProvider.GetThemeConfigurations()
                .Select(x => new StoreThemeModel
                {
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
        public virtual string PrepareRobotsTextFile()
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
                    foreach (var language in _languageService.GetAllLanguages(storeId: _storeContext.CurrentStore.Id))
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
                    foreach (var language in _languageService.GetAllLanguages(storeId: _storeContext.CurrentStore.Id))
                    {
                        foreach (var path in localizableDisallowPaths)
                        {
                            sb.AppendFormat("Disallow: {0}{1}", language.UniqueSeoCode, path);
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