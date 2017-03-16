using System;
using AutoMapper;
using Grand.Admin.Models.Blogs;
using Grand.Admin.Models.Catalog;
using Grand.Admin.Models.Cms;
using Grand.Admin.Models.Common;
using Grand.Admin.Models.Customers;
using Grand.Admin.Models.Directory;
using Grand.Admin.Models.Discounts;
using Grand.Admin.Models.ExternalAuthentication;
using Grand.Admin.Models.Forums;
using Grand.Admin.Models.Localization;
using Grand.Admin.Models.Logging;
using Grand.Admin.Models.Messages;
using Grand.Admin.Models.News;
using Grand.Admin.Models.Orders;
using Grand.Admin.Models.Payments;
using Grand.Admin.Models.Plugins;
using Grand.Admin.Models.Polls;
using Grand.Admin.Models.Settings;
using Grand.Admin.Models.Shipping;
using Grand.Admin.Models.Stores;
using Grand.Admin.Models.Tax;
using Grand.Admin.Models.Templates;
using Grand.Admin.Models.Topics;
using Grand.Admin.Models.Vendors;
using Grand.Core.Domain.Blogs;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Discounts;
using Grand.Core.Domain.Forums;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Logging;
using Grand.Core.Domain.Media;
using Grand.Core.Domain.Messages;
using Grand.Core.Domain.News;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Polls;
using Grand.Core.Domain.Shipping;
using Grand.Core.Domain.Stores;
using Grand.Core.Domain.Tax;
using Grand.Core.Domain.Topics;
using Grand.Core.Domain.Vendors;
using Grand.Core.Infrastructure.Mapper;
using Grand.Core.Plugins;
using Grand.Services.Authentication.External;
using Grand.Services.Cms;
using Grand.Services.Payments;
using Grand.Services.Seo;
using Grand.Services.Shipping;
using Grand.Services.Tax;

namespace Grand.Admin.Infrastructure.Mapper
{
    public class AdminMapperConfiguration : IMapperConfiguration
    {

        public Action<IMapperConfigurationExpression> GetConfiguration()
        {

            Action<IMapperConfigurationExpression> action = cfg =>
            {
                cfg.CreateMap<Address, AddressModel>()
               .ForMember(dest => dest.AddressHtml, mo => mo.Ignore())
               .ForMember(dest => dest.CustomAddressAttributes, mo => mo.Ignore())
               .ForMember(dest => dest.FormattedCustomAddressAttributes, mo => mo.Ignore())
               .ForMember(dest => dest.AvailableCountries, mo => mo.Ignore())
               .ForMember(dest => dest.AvailableStates, mo => mo.Ignore())
               .ForMember(dest => dest.FirstNameEnabled, mo => mo.Ignore())
               .ForMember(dest => dest.FirstNameRequired, mo => mo.Ignore())
               .ForMember(dest => dest.LastNameEnabled, mo => mo.Ignore())
               .ForMember(dest => dest.LastNameRequired, mo => mo.Ignore())
               .ForMember(dest => dest.EmailEnabled, mo => mo.Ignore())
               .ForMember(dest => dest.EmailRequired, mo => mo.Ignore())
               .ForMember(dest => dest.CompanyEnabled, mo => mo.Ignore())
               .ForMember(dest => dest.CompanyRequired, mo => mo.Ignore())
               .ForMember(dest => dest.CountryEnabled, mo => mo.Ignore())
               .ForMember(dest => dest.StateProvinceEnabled, mo => mo.Ignore())
               .ForMember(dest => dest.CityEnabled, mo => mo.Ignore())
               .ForMember(dest => dest.CityRequired, mo => mo.Ignore())
               .ForMember(dest => dest.StreetAddressEnabled, mo => mo.Ignore())
               .ForMember(dest => dest.StreetAddressRequired, mo => mo.Ignore())
               .ForMember(dest => dest.StreetAddress2Enabled, mo => mo.Ignore())
               .ForMember(dest => dest.StreetAddress2Required, mo => mo.Ignore())
               .ForMember(dest => dest.ZipPostalCodeEnabled, mo => mo.Ignore())
               .ForMember(dest => dest.ZipPostalCodeRequired, mo => mo.Ignore())
               .ForMember(dest => dest.PhoneEnabled, mo => mo.Ignore())
               .ForMember(dest => dest.PhoneRequired, mo => mo.Ignore())
               .ForMember(dest => dest.FaxEnabled, mo => mo.Ignore())
               .ForMember(dest => dest.FaxRequired, mo => mo.Ignore())
               .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());

                //address
                cfg.CreateMap<AddressModel, Address>()
                    .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomAttributes, mo => mo.Ignore());

                //countries
                cfg.CreateMap<CountryModel, Country>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.StateProvinces, mo => mo.Ignore())
                    .ForMember(dest => dest.RestrictedShippingMethods, mo => mo.Ignore());
                cfg.CreateMap<Country, CountryModel>()
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.NumberOfStates, mo => mo.MapFrom(src => src.StateProvinces != null ? src.StateProvinces.Count : 0))
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
                    .ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                //state/provinces
                cfg.CreateMap<StateProvince, StateProvinceModel>()
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<StateProvinceModel, StateProvince>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.Locales, mo => mo.Ignore());

                //language
                cfg.CreateMap<Language, LanguageModel>()
                    .ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableCurrencies, mo => mo.Ignore())
                    .ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
                    .ForMember(dest => dest.FlagFileNames, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<LanguageModel, Language>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore());
                //email account
                cfg.CreateMap<EmailAccount, EmailAccountModel>()
                    .ForMember(dest => dest.Password, mo => mo.Ignore())
                    .ForMember(dest => dest.IsDefaultEmailAccount, mo => mo.Ignore())
                    .ForMember(dest => dest.SendTestEmailTo, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<EmailAccountModel, EmailAccount>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.Password, mo => mo.Ignore());
                //message template
                cfg.CreateMap<MessageTemplate, MessageTemplateModel>()
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.AllowedTokens, mo => mo.Ignore())
                    .ForMember(dest => dest.HasAttachedDownload, mo => mo.Ignore())
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableEmailAccounts, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
                    .ForMember(dest => dest.ListOfStores, mo => mo.Ignore())
                    .ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<MessageTemplateModel, MessageTemplate>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.DelayPeriod, mo => mo.Ignore())
                    .ForMember(dest => dest.Locales, mo => mo.Ignore());
                //queued email
                cfg.CreateMap<QueuedEmail, QueuedEmailModel>()
                    .ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
                    .ForMember(dest => dest.PriorityName, mo => mo.Ignore())
                    .ForMember(dest => dest.DontSendBeforeDate, mo => mo.Ignore())
                    .ForMember(dest => dest.SendImmediately, mo => mo.Ignore())
                    .ForMember(dest => dest.SentOn, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<QueuedEmailModel, QueuedEmail>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.Priority, dt => dt.Ignore())
                    .ForMember(dest => dest.PriorityId, dt => dt.Ignore())
                    .ForMember(dest => dest.CreatedOnUtc, dt => dt.Ignore())
                    .ForMember(dest => dest.DontSendBeforeDateUtc, mo => mo.Ignore())
                    .ForMember(dest => dest.SentOnUtc, mo => mo.Ignore())
                    .ForMember(dest => dest.EmailAccountId, mo => mo.Ignore())
                    .ForMember(dest => dest.AttachmentFilePath, mo => mo.Ignore())
                    .ForMember(dest => dest.AttachmentFileName, mo => mo.Ignore());
                //contact form
                cfg.CreateMap<ContactUs, ContactFormModel>()
                    .ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());

                //banner
                cfg.CreateMap<Banner, BannerModel>()
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<BannerModel, Banner>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.Locales, mo => mo.Ignore());

                //InteractiveForm
                cfg.CreateMap<InteractiveForm, InteractiveFormModel>()
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableEmailAccounts, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableTokens, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<InteractiveFormModel, InteractiveForm>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.FormAttributes, mo => mo.Ignore())
                    .ForMember(dest => dest.Locales, mo => mo.Ignore());

                cfg.CreateMap<InteractiveForm.FormAttribute, InteractiveFormAttributeModel>()
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<InteractiveFormAttributeModel, InteractiveForm.FormAttribute>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.Locales, mo => mo.Ignore());


                //campaign
                cfg.CreateMap<Campaign, CampaignModel>()
                    .ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
                    .ForMember(dest => dest.AllowedTokens, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
                    .ForMember(dest => dest.TestEmail, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<CampaignModel, Campaign>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore());
                //topcis
                cfg.CreateMap<Topic, TopicModel>()
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableTopicTemplates, mo => mo.Ignore())
                    .ForMember(dest => dest.Url, mo => mo.Ignore())
                    .ForMember(dest => dest.SeName, mo => mo.MapFrom(src => src.GetSeName("", true, false)))
                    .ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
                    .ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableCustomerRoles, mo => mo.Ignore())
                    .ForMember(dest => dest.SelectedCustomerRoleIds, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<TopicModel, Topic>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.Locales, mo => mo.Ignore());

                //category
                cfg.CreateMap<Category, CategoryModel>()
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableCategoryTemplates, mo => mo.Ignore())
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.Breadcrumb, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableCategories, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableDiscounts, mo => mo.Ignore())
                    .ForMember(dest => dest.SelectedDiscountIds, mo => mo.Ignore())
                    .ForMember(dest => dest.SeName, mo => mo.MapFrom(src => src.GetSeName("", true, false)))
                    .ForMember(dest => dest.AvailableCustomerRoles, mo => mo.Ignore())
                    .ForMember(dest => dest.SelectedCustomerRoleIds, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
                    .ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<CategoryModel, Category>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                    .ForMember(dest => dest.UpdatedOnUtc, mo => mo.Ignore())
                    .ForMember(dest => dest.AppliedDiscounts, mo => mo.Ignore());
                //manufacturer
                cfg.CreateMap<Manufacturer, ManufacturerModel>()
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableManufacturerTemplates, mo => mo.Ignore())
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableDiscounts, mo => mo.Ignore())
                    .ForMember(dest => dest.SelectedDiscountIds, mo => mo.Ignore())
                    .ForMember(dest => dest.SeName, mo => mo.MapFrom(src => src.GetSeName("", true, false)))
                    .ForMember(dest => dest.AvailableCustomerRoles, mo => mo.Ignore())
                    .ForMember(dest => dest.SelectedCustomerRoleIds, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
                    .ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<ManufacturerModel, Manufacturer>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                    .ForMember(dest => dest.UpdatedOnUtc, mo => mo.Ignore())
                    .ForMember(dest => dest.AppliedDiscounts, mo => mo.Ignore()); ;

                //vendors
                cfg.CreateMap<Vendor, VendorModel>()
                    .ForMember(dest => dest.AssociatedCustomers, mo => mo.Ignore())
                    .ForMember(dest => dest.AddVendorNoteMessage, mo => mo.Ignore())
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableDiscounts, mo => mo.Ignore())
                    .ForMember(dest => dest.SelectedDiscountIds, mo => mo.Ignore())
                    .ForMember(dest => dest.SeName, mo => mo.MapFrom(src => src.GetSeName("", true, false)))
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<VendorModel, Vendor>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.Deleted, mo => mo.Ignore());

                //products
                cfg.CreateMap<Product, ProductModel>()
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.ProductTypeName, mo => mo.Ignore())
                    .ForMember(dest => dest.AssociatedToProductId, mo => mo.Ignore())
                    .ForMember(dest => dest.AssociatedToProductName, mo => mo.Ignore())
                    .ForMember(dest => dest.StockQuantityStr, mo => mo.Ignore())
                    .ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
                    .ForMember(dest => dest.UpdatedOn, mo => mo.Ignore())
                    .ForMember(dest => dest.ProductTags, mo => mo.Ignore())
                    .ForMember(dest => dest.PictureThumbnailUrl, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableVendors, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableProductTemplates, mo => mo.Ignore())
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableCategories, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableManufacturers, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableProductAttributes, mo => mo.Ignore())
                    .ForMember(dest => dest.AddPictureModel, mo => mo.Ignore())
                    .ForMember(dest => dest.ProductPictureModels, mo => mo.Ignore())
                    .ForMember(dest => dest.AddSpecificationAttributeModel, mo => mo.Ignore())
                    .ForMember(dest => dest.CopyProductModel, mo => mo.Ignore())
                    .ForMember(dest => dest.ProductWarehouseInventoryModels, mo => mo.Ignore())
                    .ForMember(dest => dest.IsLoggedInAsVendor, mo => mo.Ignore())
                    .ForMember(dest => dest.SeName, mo => mo.MapFrom(src => src.GetSeName("", true, false)))
                    .ForMember(dest => dest.AvailableCustomerRoles, mo => mo.Ignore())
                    .ForMember(dest => dest.SelectedCustomerRoleIds, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
                    .ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableTaxCategories, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableUnits, mo => mo.Ignore())
                    .ForMember(dest => dest.PrimaryStoreCurrencyCode, mo => mo.Ignore())
                    .ForMember(dest => dest.BaseDimensionIn, mo => mo.Ignore())
                    .ForMember(dest => dest.BaseWeightIn, mo => mo.Ignore())
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableDiscounts, mo => mo.Ignore())
                    .ForMember(dest => dest.SelectedDiscountIds, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableDeliveryDates, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableWarehouses, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableBasepriceUnits, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableBasepriceBaseUnits, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<ProductModel, Product>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.ProductTags, mo => mo.Ignore())
                    .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                    .ForMember(dest => dest.UpdatedOnUtc, mo => mo.Ignore())
                    .ForMember(dest => dest.ParentGroupedProductId, mo => mo.Ignore())
                    .ForMember(dest => dest.ProductType, mo => mo.Ignore())
                    .ForMember(dest => dest.ApprovedRatingSum, mo => mo.Ignore())
                    .ForMember(dest => dest.NotApprovedRatingSum, mo => mo.Ignore())
                    .ForMember(dest => dest.ApprovedTotalReviews, mo => mo.Ignore())
                    .ForMember(dest => dest.NotApprovedTotalReviews, mo => mo.Ignore())
                    .ForMember(dest => dest.ProductCategories, mo => mo.Ignore())
                    .ForMember(dest => dest.ProductManufacturers, mo => mo.Ignore())
                    .ForMember(dest => dest.ProductPictures, mo => mo.Ignore())
                    .ForMember(dest => dest.ProductSpecificationAttributes, mo => mo.Ignore())
                    .ForMember(dest => dest.ProductWarehouseInventory, mo => mo.Ignore())
                    .ForMember(dest => dest.HasTierPrices, mo => mo.Ignore())
                    .ForMember(dest => dest.BackorderMode, mo => mo.Ignore())
                    .ForMember(dest => dest.DownloadActivationType, mo => mo.Ignore())
                    .ForMember(dest => dest.GiftCardType, mo => mo.Ignore())
                    .ForMember(dest => dest.LowStockActivity, mo => mo.Ignore())
                    .ForMember(dest => dest.ManageInventoryMethod, mo => mo.Ignore())
                    .ForMember(dest => dest.RecurringCyclePeriod, mo => mo.Ignore())
                    .ForMember(dest => dest.RentalPricePeriod, mo => mo.Ignore())
                    .ForMember(dest => dest.ProductAttributeMappings, mo => mo.Ignore())
                    .ForMember(dest => dest.ProductAttributeCombinations, mo => mo.Ignore())
                    .ForMember(dest => dest.TierPrices, mo => mo.Ignore())
                    .ForMember(dest => dest.AppliedDiscounts, mo => mo.Ignore());
                //logs
                cfg.CreateMap<Log, LogModel>()
                    .ForMember(dest => dest.CustomerEmail, mo => mo.Ignore())
                    .ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<LogModel, Log>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                    .ForMember(dest => dest.LogLevelId, mo => mo.Ignore());

                //ActivityLogType
                cfg.CreateMap<ActivityLogTypeModel, ActivityLogType>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.SystemKeyword, mo => mo.Ignore());
                cfg.CreateMap<ActivityLogType, ActivityLogTypeModel>()
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<ActivityLog, ActivityLogModel>()
                    .ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<ActivityStats, ActivityStatsModel>()
                    .ForMember(dest => dest.ActivityLogTypeName, mo => mo.Ignore());


                //currencies
                cfg.CreateMap<Currency, CurrencyModel>()
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
                    .ForMember(dest => dest.IsPrimaryExchangeRateCurrency, mo => mo.Ignore())
                    .ForMember(dest => dest.IsPrimaryStoreCurrency, mo => mo.Ignore())
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
                    .ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<CurrencyModel, Currency>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                    .ForMember(dest => dest.UpdatedOnUtc, mo => mo.Ignore());
                //measure weights
                cfg.CreateMap<MeasureWeight, MeasureWeightModel>()
                    .ForMember(dest => dest.IsPrimaryWeight, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<MeasureWeightModel, MeasureWeight>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore());

                //measure units
                cfg.CreateMap<MeasureUnit, MeasureUnitModel>()
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<MeasureUnitModel, MeasureUnit>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore());

                //measure dimensions
                cfg.CreateMap<MeasureDimension, MeasureDimensionModel>()
                .ForMember(dest => dest.IsPrimaryDimension, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<MeasureDimensionModel, MeasureDimension>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore());

                //tax providers
                cfg.CreateMap<ITaxProvider, TaxProviderModel>()
                .ForMember(dest => dest.FriendlyName, mo => mo.MapFrom(src => src.PluginDescriptor.FriendlyName))
                .ForMember(dest => dest.SystemName, mo => mo.MapFrom(src => src.PluginDescriptor.SystemName))
                .ForMember(dest => dest.IsPrimaryTaxProvider, mo => mo.Ignore())
                .ForMember(dest => dest.ConfigurationActionName, mo => mo.Ignore())
                .ForMember(dest => dest.ConfigurationControllerName, mo => mo.Ignore())
                .ForMember(dest => dest.ConfigurationRouteValues, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                //tax categories
                cfg.CreateMap<TaxCategory, TaxCategoryModel>()
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<TaxCategoryModel, TaxCategory>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore());


                //shipping methods
                cfg.CreateMap<ShippingMethod, ShippingMethodModel>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<ShippingMethodModel, ShippingMethod>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.RestrictedCountries, mo => mo.Ignore());

                //delivery dates
                cfg.CreateMap<DeliveryDate, DeliveryDateModel>()
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<DeliveryDateModel, DeliveryDate>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.Locales, mo => mo.Ignore());

                //shipping rate computation methods
                cfg.CreateMap<IShippingRateComputationMethod, ShippingRateComputationMethodModel>()
                    .ForMember(dest => dest.FriendlyName, mo => mo.MapFrom(src => src.PluginDescriptor.FriendlyName))
                    .ForMember(dest => dest.SystemName, mo => mo.MapFrom(src => src.PluginDescriptor.SystemName))
                    .ForMember(dest => dest.DisplayOrder, mo => mo.MapFrom(src => src.PluginDescriptor.DisplayOrder))
                    .ForMember(dest => dest.IsActive, mo => mo.Ignore())
                    .ForMember(dest => dest.LogoUrl, mo => mo.Ignore())
                    .ForMember(dest => dest.ConfigurationActionName, mo => mo.Ignore())
                    .ForMember(dest => dest.ConfigurationControllerName, mo => mo.Ignore())
                    .ForMember(dest => dest.ConfigurationRouteValues, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());

                //payment methods
                cfg.CreateMap<IPaymentMethod, PaymentMethodModel>()
                    .ForMember(dest => dest.FriendlyName, mo => mo.MapFrom(src => src.PluginDescriptor.FriendlyName))
                    .ForMember(dest => dest.SystemName, mo => mo.MapFrom(src => src.PluginDescriptor.SystemName))
                    .ForMember(dest => dest.DisplayOrder, mo => mo.MapFrom(src => src.PluginDescriptor.DisplayOrder))
                    .ForMember(dest => dest.RecurringPaymentType, mo => mo.MapFrom(src => src.RecurringPaymentType.ToString()))
                    .ForMember(dest => dest.IsActive, mo => mo.Ignore())
                    .ForMember(dest => dest.LogoUrl, mo => mo.Ignore())
                    .ForMember(dest => dest.ConfigurationActionName, mo => mo.Ignore())
                    .ForMember(dest => dest.ConfigurationControllerName, mo => mo.Ignore())
                    .ForMember(dest => dest.ConfigurationRouteValues, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                //external authentication methods
                cfg.CreateMap<IExternalAuthenticationMethod, AuthenticationMethodModel>()
                    .ForMember(dest => dest.FriendlyName, mo => mo.MapFrom(src => src.PluginDescriptor.FriendlyName))
                    .ForMember(dest => dest.SystemName, mo => mo.MapFrom(src => src.PluginDescriptor.SystemName))
                    .ForMember(dest => dest.DisplayOrder, mo => mo.MapFrom(src => src.PluginDescriptor.DisplayOrder))
                    .ForMember(dest => dest.IsActive, mo => mo.Ignore())
                    .ForMember(dest => dest.ConfigurationActionName, mo => mo.Ignore())
                    .ForMember(dest => dest.ConfigurationControllerName, mo => mo.Ignore())
                    .ForMember(dest => dest.ConfigurationRouteValues, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                //widgets
                cfg.CreateMap<IWidgetPlugin, WidgetModel>()
                    .ForMember(dest => dest.FriendlyName, mo => mo.MapFrom(src => src.PluginDescriptor.FriendlyName))
                    .ForMember(dest => dest.SystemName, mo => mo.MapFrom(src => src.PluginDescriptor.SystemName))
                    .ForMember(dest => dest.DisplayOrder, mo => mo.MapFrom(src => src.PluginDescriptor.DisplayOrder))
                    .ForMember(dest => dest.IsActive, mo => mo.Ignore())
                    .ForMember(dest => dest.ConfigurationActionName, mo => mo.Ignore())
                    .ForMember(dest => dest.ConfigurationControllerName, mo => mo.Ignore())
                    .ForMember(dest => dest.ConfigurationRouteValues, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                //plugins
                cfg.CreateMap<PluginDescriptor, PluginModel>()
                    .ForMember(dest => dest.ConfigurationUrl, mo => mo.Ignore())
                    .ForMember(dest => dest.CanChangeEnabled, mo => mo.Ignore())
                    .ForMember(dest => dest.IsEnabled, mo => mo.Ignore())
                    .ForMember(dest => dest.LogoUrl, mo => mo.Ignore())
                    .ForMember(dest => dest.LimitedToStores, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
                    .ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                //newsLetter subscriptions
                cfg.CreateMap<NewsLetterSubscription, NewsLetterSubscriptionModel>()
                    .ForMember(dest => dest.StoreName, mo => mo.Ignore())
                    .ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<NewsLetterSubscriptionModel, NewsLetterSubscription>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.StoreId, mo => mo.Ignore())
                    .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                    .ForMember(dest => dest.NewsLetterSubscriptionGuid, mo => mo.Ignore());

                //newsLetter categories
                cfg.CreateMap<NewsletterCategory, NewsletterCategoryModel>()
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<NewsletterCategoryModel, NewsletterCategory>()
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.Stores, mo => mo.Ignore())
                    .ForMember(dest => dest.Id, mo => mo.Ignore());

                //forums
                cfg.CreateMap<ForumGroup, ForumGroupModel>()
                    .ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<ForumGroupModel, ForumGroup>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                    .ForMember(dest => dest.UpdatedOnUtc, mo => mo.Ignore());
                //.ForMember(dest => dest.Forums, mo => mo.Ignore());
                cfg.CreateMap<Forum, ForumModel>()
                    .ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
                    .ForMember(dest => dest.ForumGroups, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<ForumModel, Forum>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.NumTopics, mo => mo.Ignore())
                    .ForMember(dest => dest.NumPosts, mo => mo.Ignore())
                    .ForMember(dest => dest.LastTopicId, mo => mo.Ignore())
                    .ForMember(dest => dest.LastPostId, mo => mo.Ignore())
                    .ForMember(dest => dest.LastPostCustomerId, mo => mo.Ignore())
                    .ForMember(dest => dest.LastPostTime, mo => mo.Ignore())
                    .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                    .ForMember(dest => dest.UpdatedOnUtc, mo => mo.Ignore());
                //blogs
                cfg.CreateMap<BlogPost, BlogPostModel>()
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.SeName, mo => mo.MapFrom(src => src.GetSeName("", true, false)))
                    .ForMember(dest => dest.Comments, mo => mo.Ignore())
                    .ForMember(dest => dest.StartDate, mo => mo.Ignore())
                    .ForMember(dest => dest.EndDate, mo => mo.Ignore())
                    .ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
                    .ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<BlogPostModel, BlogPost>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.CommentCount, mo => mo.Ignore())
                    .ForMember(dest => dest.StartDateUtc, mo => mo.Ignore())
                    .ForMember(dest => dest.EndDateUtc, mo => mo.Ignore())
                    .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore());
                //news
                cfg.CreateMap<NewsItem, NewsItemModel>()
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.SeName, mo => mo.MapFrom(src => src.GetSeName("", true, false)))
                    .ForMember(dest => dest.Comments, mo => mo.Ignore())
                    .ForMember(dest => dest.StartDate, mo => mo.Ignore())
                    .ForMember(dest => dest.EndDate, mo => mo.Ignore())
                    .ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
                    .ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableCustomerRoles, mo => mo.Ignore())
                    .ForMember(dest => dest.SelectedCustomerRoleIds, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());

                cfg.CreateMap<NewsItemModel, NewsItem>()
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.NewsComments, mo => mo.Ignore())
                    .ForMember(dest => dest.CommentCount, mo => mo.Ignore())
                    .ForMember(dest => dest.StartDateUtc, mo => mo.Ignore())
                    .ForMember(dest => dest.EndDateUtc, mo => mo.Ignore())
                    .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore());
                //news
                cfg.CreateMap<Poll, PollModel>()
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.StartDate, mo => mo.Ignore())
                    .ForMember(dest => dest.EndDate, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
                    .ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableCustomerRoles, mo => mo.Ignore())
                    .ForMember(dest => dest.SelectedCustomerRoleIds, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());

                cfg.CreateMap<PollModel, Poll>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.PollAnswers, mo => mo.Ignore())
                    .ForMember(dest => dest.StartDateUtc, mo => mo.Ignore())
                    .ForMember(dest => dest.EndDateUtc, mo => mo.Ignore());

                cfg.CreateMap<PollAnswer, PollAnswerModel>()
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<PollAnswerModel, PollAnswer>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.Locales, mo => mo.Ignore());


                //customer roles
                cfg.CreateMap<CustomerRole, CustomerRoleModel>()
                    .ForMember(dest => dest.PurchasedWithProductName, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<CustomerRoleModel, CustomerRole>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore());


                //customer tags
                cfg.CreateMap<CustomerTag, CustomerTagModel>()
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<CustomerTagModel, CustomerTag>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore());


                //customer action
                cfg.CreateMap<CustomerAction, CustomerActionModel>()
                    .ForMember(dest => dest.MessageTemplates, mo => mo.Ignore())
                    .ForMember(dest => dest.Banners, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<CustomerActionModel, CustomerAction>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore());


                cfg.CreateMap<CustomerAction.ActionCondition, CustomerActionConditionModel>()
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomerActionConditionType, mo => mo.Ignore());
                cfg.CreateMap<CustomerActionConditionModel, CustomerAction.ActionCondition>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomerActionConditionType, mo => mo.Ignore());

                //Customer action type
                cfg.CreateMap<CustomerActionTypeModel, CustomerActionType>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.SystemKeyword, mo => mo.Ignore());
                cfg.CreateMap<CustomerActionType, CustomerActionTypeModel>()
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());

                //Customer reminder
                cfg.CreateMap<CustomerReminderModel, CustomerReminder>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore());
                cfg.CreateMap<CustomerReminder, CustomerReminderModel>()
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());

                cfg.CreateMap<CustomerReminder.ReminderLevel, CustomerReminderModel.ReminderLevelModel>()
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore())
                    .ForMember(dest => dest.EmailAccounts, mo => mo.Ignore());
                cfg.CreateMap<CustomerReminderModel.ReminderLevelModel, CustomerReminder.ReminderLevel>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore());

                cfg.CreateMap<CustomerReminder.ReminderCondition, CustomerReminderModel.ConditionModel>()
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore())
                    .ForMember(dest => dest.ConditionType, mo => mo.Ignore());
                cfg.CreateMap<CustomerReminderModel.ConditionModel, CustomerReminder.ReminderCondition>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.ConditionType, mo => mo.Ignore());

                //product attributes
                cfg.CreateMap<ProductAttribute, ProductAttributeModel>()
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<ProductAttributeModel, ProductAttribute>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.Locales, mo => mo.Ignore());

                //specification attributes
                cfg.CreateMap<SpecificationAttribute, SpecificationAttributeModel>()
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<SpecificationAttributeModel, SpecificationAttribute>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.SpecificationAttributeOptions, mo => mo.Ignore());
                cfg.CreateMap<SpecificationAttributeOption, SpecificationAttributeOptionModel>()
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.NumberOfAssociatedProducts, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<SpecificationAttributeOptionModel, SpecificationAttributeOption>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.Locales, mo => mo.Ignore());

                //checkout attributes
                cfg.CreateMap<CheckoutAttribute, CheckoutAttributeModel>()
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableTaxCategories, mo => mo.Ignore())
                    .ForMember(dest => dest.AttributeControlTypeName, mo => mo.Ignore())
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
                    .ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableCustomerRoles, mo => mo.Ignore())
                    .ForMember(dest => dest.SelectedCustomerRoleIds, mo => mo.Ignore())
                    .ForMember(dest => dest.ConditionAllowed, mo => mo.Ignore())
                    .ForMember(dest => dest.ConditionModel, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<CheckoutAttributeModel, CheckoutAttribute>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.AttributeControlType, mo => mo.Ignore())
                    .ForMember(dest => dest.ConditionAttributeXml, mo => mo.Ignore())
                    .ForMember(dest => dest.CheckoutAttributeValues, mo => mo.Ignore());
                //customer attributes
                cfg.CreateMap<CustomerAttribute, CustomerAttributeModel>()
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.AttributeControlTypeName, mo => mo.Ignore())
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<CustomerAttributeModel, CustomerAttribute>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.AttributeControlType, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomerAttributeValues, mo => mo.Ignore());
                //address attributes
                cfg.CreateMap<AddressAttribute, AddressAttributeModel>()
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.AttributeControlTypeName, mo => mo.Ignore())
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<AddressAttributeModel, AddressAttribute>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.AttributeControlType, mo => mo.Ignore())
                    .ForMember(dest => dest.AddressAttributeValues, mo => mo.Ignore());
                //discounts
                cfg.CreateMap<Discount, DiscountModel>()
                    .ForMember(dest => dest.DiscountTypeName, mo => mo.Ignore())
                    .ForMember(dest => dest.TimesUsed, mo => mo.Ignore())
                    .ForMember(dest => dest.PrimaryStoreCurrencyCode, mo => mo.Ignore())
                    .ForMember(dest => dest.AddDiscountRequirement, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableDiscountRequirementRules, mo => mo.Ignore())
                    .ForMember(dest => dest.DiscountRequirementMetaInfos, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<DiscountModel, Discount>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.DiscountType, mo => mo.Ignore())
                    .ForMember(dest => dest.DiscountLimitation, mo => mo.Ignore())
                    .ForMember(dest => dest.DiscountRequirements, mo => mo.Ignore());

                //gift cards
                cfg.CreateMap<GiftCard, GiftCardModel>()
                    .ForMember(dest => dest.PurchasedWithOrderId, mo => mo.Ignore())
                    .ForMember(dest => dest.AmountStr, mo => mo.Ignore())
                    .ForMember(dest => dest.RemainingAmountStr, mo => mo.Ignore())
                    .ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
                    .ForMember(dest => dest.PrimaryStoreCurrencyCode, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<GiftCardModel, GiftCard>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.GiftCardType, mo => mo.Ignore())
                    .ForMember(dest => dest.GiftCardUsageHistory, mo => mo.Ignore())
                    .ForMember(dest => dest.PurchasedWithOrderItem, mo => mo.Ignore())
                    .ForMember(dest => dest.IsRecipientNotified, mo => mo.Ignore())
                    .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore());
                //stores
                cfg.CreateMap<Store, StoreModel>()
                    .ForMember(dest => dest.AvailableLanguages, mo => mo.Ignore())
                    .ForMember(dest => dest.AvailableWarehouses, mo => mo.Ignore())
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<StoreModel, Store>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore())
                    .ForMember(dest => dest.Locales, mo => mo.Ignore());

                //Settings
                cfg.CreateMap<TaxSettings, TaxSettingsModel>()
                    .ForMember(dest => dest.DefaultTaxAddress, mo => mo.Ignore())
                    .ForMember(dest => dest.TaxDisplayTypeValues, mo => mo.Ignore())
                    .ForMember(dest => dest.TaxBasedOnValues, mo => mo.Ignore())
                    .ForMember(dest => dest.PaymentMethodAdditionalFeeTaxCategories, mo => mo.Ignore())
                    .ForMember(dest => dest.TaxCategories, mo => mo.Ignore())
                    .ForMember(dest => dest.EuVatShopCountries, mo => mo.Ignore())
                    .ForMember(dest => dest.ActiveStoreScopeConfiguration, mo => mo.Ignore())
                    .ForMember(dest => dest.PricesIncludeTax_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.AllowCustomersToSelectTaxDisplayType_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.TaxDisplayType_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.DisplayTaxSuffix_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.DisplayTaxRates_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.HideZeroTax_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.HideTaxInOrderSummary_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ForceTaxExclusionFromOrderSubtotal_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.TaxBasedOn_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.DefaultTaxAddress_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ShippingIsTaxable_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ShippingPriceIncludesTax_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ShippingTaxClassId_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.PaymentMethodAdditionalFeeIsTaxable_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.PaymentMethodAdditionalFeeIncludesTax_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.PaymentMethodAdditionalFeeTaxClassId_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.EuVatEnabled_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.EuVatShopCountryId_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.EuVatAllowVatExemption_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.EuVatUseWebService_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.EuVatAssumeValid_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.EuVatEmailAdminWhenNewVatSubmitted_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<TaxSettingsModel, TaxSettings>()
                    .ForMember(dest => dest.ActiveTaxProviderSystemName, mo => mo.Ignore());
                cfg.CreateMap<NewsSettings, NewsSettingsModel>()
                    .ForMember(dest => dest.ActiveStoreScopeConfiguration, mo => mo.Ignore())
                    .ForMember(dest => dest.Enabled_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.AllowNotRegisteredUsersToLeaveComments_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.NotifyAboutNewNewsComments_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ShowNewsOnMainPage_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.MainPageNewsCount_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.NewsArchivePageSize_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ShowHeaderRssUrl_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<NewsSettingsModel, NewsSettings>();
                cfg.CreateMap<ForumSettings, ForumSettingsModel>()
                    .ForMember(dest => dest.ForumEditorValues, mo => mo.Ignore())
                    .ForMember(dest => dest.ActiveStoreScopeConfiguration, mo => mo.Ignore())
                    .ForMember(dest => dest.ForumsEnabled_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.RelativeDateTimeFormattingEnabled_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ShowCustomersPostCount_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.AllowGuestsToCreatePosts_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.AllowGuestsToCreateTopics_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.AllowCustomersToEditPosts_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.AllowCustomersToDeletePosts_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.AllowPostVoting_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.MaxVotesPerDay_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.AllowCustomersToManageSubscriptions_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.TopicsPageSize_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.PostsPageSize_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ForumEditor_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.SignaturesEnabled_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.AllowPrivateMessages_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ShowAlertForPM_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.NotifyAboutPrivateMessages_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ActiveDiscussionsFeedEnabled_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ActiveDiscussionsFeedCount_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ForumFeedsEnabled_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ForumFeedCount_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.SearchResultsPageSize_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ActiveDiscussionsPageSize_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<ForumSettingsModel, ForumSettings>()
                    .ForMember(dest => dest.TopicSubjectMaxLength, mo => mo.Ignore())
                    .ForMember(dest => dest.StrippedTopicMaxLength, mo => mo.Ignore())
                    .ForMember(dest => dest.PostMaxLength, mo => mo.Ignore())
                    .ForMember(dest => dest.LatestCustomerPostsPageSize, mo => mo.Ignore())
                    .ForMember(dest => dest.PrivateMessagesPageSize, mo => mo.Ignore())
                    .ForMember(dest => dest.ForumSubscriptionsPageSize, mo => mo.Ignore())
                    .ForMember(dest => dest.PMSubjectMaxLength, mo => mo.Ignore())
                    .ForMember(dest => dest.PMTextMaxLength, mo => mo.Ignore())
                    .ForMember(dest => dest.HomePageActiveDiscussionsTopicCount, mo => mo.Ignore())
                    .ForMember(dest => dest.ForumSearchTermMinimumLength, mo => mo.Ignore());
                cfg.CreateMap<BlogSettings, BlogSettingsModel>()
                    .ForMember(dest => dest.ActiveStoreScopeConfiguration, mo => mo.Ignore())
                    .ForMember(dest => dest.Enabled_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.PostsPageSize_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.AllowNotRegisteredUsersToLeaveComments_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.NotifyAboutNewBlogComments_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.NumberOfTags_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ShowHeaderRssUrl_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<BlogSettingsModel, BlogSettings>();
                cfg.CreateMap<VendorSettings, VendorSettingsModel>()
                    .ForMember(dest => dest.ActiveStoreScopeConfiguration, mo => mo.Ignore())
                    .ForMember(dest => dest.VendorsBlockItemsToDisplay_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ShowVendorOnProductDetailsPage_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.AllowCustomersToContactVendors_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.AllowCustomersToApplyForVendorAccount_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.AllowSearchByVendor_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<VendorSettingsModel, VendorSettings>()
                    .ForMember(dest => dest.DefaultVendorPageSizeOptions, mo => mo.Ignore());
                cfg.CreateMap<ShippingSettings, ShippingSettingsModel>()
                    .ForMember(dest => dest.ShippingOriginAddress, mo => mo.Ignore())
                    .ForMember(dest => dest.ActiveStoreScopeConfiguration, mo => mo.Ignore())
                    .ForMember(dest => dest.AllowPickUpInStore_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ShipToSameAddress_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.UseWarehouseLocation_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.NotifyCustomerAboutShippingFromMultipleLocations_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.FreeShippingOverXEnabled_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.FreeShippingOverXValue_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.FreeShippingOverXIncludingTax_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.EstimateShippingEnabled_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.DisplayShipmentEventsToCustomers_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.DisplayShipmentEventsToStoreOwner_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.BypassShippingMethodSelectionIfOnlyOne_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ShippingOriginAddress_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<ShippingSettingsModel, ShippingSettings>()
                    .ForMember(dest => dest.ActiveShippingRateComputationMethodSystemNames, mo => mo.Ignore())
                    .ForMember(dest => dest.ReturnValidOptionsIfThereAreAny, mo => mo.Ignore())
                    .ForMember(dest => dest.UseCubeRootMethod, mo => mo.Ignore());
                cfg.CreateMap<CatalogSettings, CatalogSettingsModel>()
                    .ForMember(dest => dest.ActiveStoreScopeConfiguration, mo => mo.Ignore())
                    .ForMember(dest => dest.AllowViewUnpublishedProductPage_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.DisplayDiscontinuedMessageForUnpublishedProducts_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ShowSkuOnProductDetailsPage_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ShowSkuOnCatalogPages_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ShowManufacturerPartNumber_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ShowGtin_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ShowFreeShippingNotification_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.AllowProductSorting_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.AllowProductViewModeChanging_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ShowProductsFromSubcategories_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ShowCategoryProductNumber_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ShowCategoryProductNumberIncludingSubcategories_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.CategoryBreadcrumbEnabled_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ShowShareButton_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.PageShareCode_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ProductReviewsMustBeApproved_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.AllowAnonymousUsersToReviewProduct_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ProductReviewPossibleOnlyAfterPurchasing_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.NotifyStoreOwnerAboutNewProductReviews_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ShowProductReviewsPerStore_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.EmailAFriendEnabled_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.AllowAnonymousUsersToEmailAFriend_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.RecentlyViewedProductsNumber_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.RecentlyViewedProductsEnabled_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.RecommendedProductsEnabled_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.NewProductsNumber_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.NewProductsEnabled_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.CompareProductsEnabled_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ShowBestsellersOnHomepage_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.NumberOfBestsellersOnHomepage_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.SearchPageProductsPerPage_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.SearchPageAllowCustomersToSelectPageSize_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.SearchPagePageSizeOptions_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ProductSearchAutoCompleteEnabled_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ProductSearchAutoCompleteNumberOfProducts_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ShowProductImagesInSearchAutoComplete_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ProductSearchTermMinimumLength_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ProductsAlsoPurchasedEnabled_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ProductsAlsoPurchasedNumber_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.NumberOfProductTags_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ProductsByTagPageSize_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ProductsByTagAllowCustomersToSelectPageSize_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ProductsByTagPageSizeOptions_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.IncludeShortDescriptionInCompareProducts_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.IncludeFullDescriptionInCompareProducts_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.IgnoreDiscounts_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.IgnoreFeaturedProducts_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.IgnoreAcl_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.IgnoreStoreLimitations_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.CacheProductPrices_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ManufacturersBlockItemsToDisplay_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.DisplayTaxShippingInfoFooter_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.DisplayTaxShippingInfoProductDetailsPage_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.DisplayTaxShippingInfoProductBoxes_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.DisplayTaxShippingInfoShoppingCart_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.DisplayTaxShippingInfoWishlist_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.DisplayTaxShippingInfoOrderDetailsPage_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<CatalogSettingsModel, CatalogSettings>()
                    .ForMember(dest => dest.PublishBackProductWhenCancellingOrders, mo => mo.Ignore())
                    .ForMember(dest => dest.DefaultViewMode, mo => mo.Ignore())
                    .ForMember(dest => dest.DefaultProductRatingValue, mo => mo.Ignore())
                    .ForMember(dest => dest.AjaxProcessAttributeChange, mo => mo.Ignore())
                    .ForMember(dest => dest.IncludeFeaturedProductsInNormalLists, mo => mo.Ignore())
                    .ForMember(dest => dest.MaximumBackInStockSubscriptions, mo => mo.Ignore())
                    .ForMember(dest => dest.DisplayTierPricesWithDiscounts, mo => mo.Ignore())
                    .ForMember(dest => dest.CompareProductsNumber, mo => mo.Ignore())
                    .ForMember(dest => dest.DefaultCategoryPageSizeOptions, mo => mo.Ignore())
                    .ForMember(dest => dest.DefaultCategoryPageSize, mo => mo.Ignore())
                    .ForMember(dest => dest.LimitOfFeaturedProducts, mo => mo.Ignore())
                    .ForMember(dest => dest.DefaultManufacturerPageSizeOptions, mo => mo.Ignore())
                    .ForMember(dest => dest.DefaultManufacturerPageSize, mo => mo.Ignore())
                    .ForMember(dest => dest.ProductSortingEnumDisabled, mo => mo.Ignore())
                    .ForMember(dest => dest.ProductSortingEnumDisplayOrder, mo => mo.Ignore());
                cfg.CreateMap<RewardPointsSettings, RewardPointsSettingsModel>()
                    .ForMember(dest => dest.PrimaryStoreCurrencyCode, mo => mo.Ignore())
                    .ForMember(dest => dest.ActiveStoreScopeConfiguration, mo => mo.Ignore())
                    .ForMember(dest => dest.Enabled_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ExchangeRate_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.MinimumRewardPointsToUse_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.PointsForRegistration_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.PointsForPurchases_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.PointsForPurchases_Awarded_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.PointsForPurchases_Canceled_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.DisplayHowMuchWillBeEarned_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<RewardPointsSettingsModel, RewardPointsSettings>();
                cfg.CreateMap<OrderSettings, OrderSettingsModel>()
                    .ForMember(dest => dest.GiftCards_Activated_OrderStatuses, mo => mo.Ignore())
                    .ForMember(dest => dest.GiftCards_Deactivated_OrderStatuses, mo => mo.Ignore())
                    .ForMember(dest => dest.PrimaryStoreCurrencyCode, mo => mo.Ignore())
                    .ForMember(dest => dest.OrderIdent, mo => mo.Ignore())
                    .ForMember(dest => dest.ActiveStoreScopeConfiguration, mo => mo.Ignore())
                    .ForMember(dest => dest.IsReOrderAllowed_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.MinOrderSubtotalAmount_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.MinOrderSubtotalAmountIncludingTax_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.MinOrderTotalAmount_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.AnonymousCheckoutAllowed_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.TermsOfServiceOnShoppingCartPage_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.TermsOfServiceOnOrderConfirmPage_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.OnePageCheckoutEnabled_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.OnePageCheckoutDisplayOrderTotalsOnPaymentInfoTab_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ReturnRequestsEnabled_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.NumberOfDaysReturnRequestAvailable_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.DisableBillingAddressCheckoutStep_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.DisableOrderCompletedPage_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.AttachPdfInvoiceToOrderPlacedEmail_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.AttachPdfInvoiceToOrderPaidEmail_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.AttachPdfInvoiceToOrderCompletedEmail_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<OrderSettingsModel, OrderSettings>()
                    .ForMember(dest => dest.MinimumOrderPlacementInterval, mo => mo.Ignore());
                cfg.CreateMap<ShoppingCartSettings, ShoppingCartSettingsModel>()
                    .ForMember(dest => dest.ActiveStoreScopeConfiguration, mo => mo.Ignore())
                    .ForMember(dest => dest.DisplayCartAfterAddingProduct_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.DisplayWishlistAfterAddingProduct_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.MaximumShoppingCartItems_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.MaximumWishlistItems_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.AllowOutOfStockItemsToBeAddedToWishlist_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.MoveItemsFromWishlistToCart_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ShowProductImagesOnShoppingCart_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ShowProductImagesOnWishList_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ShowDiscountBox_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ShowGiftCardBox_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.CrossSellsNumber_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.EmailWishlistEnabled_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.AllowAnonymousUsersToEmailWishlist_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.MiniShoppingCartEnabled_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ShowProductImagesInMiniShoppingCart_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.MiniShoppingCartProductNumber_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.AllowCartItemEditing_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<ShoppingCartSettingsModel, ShoppingCartSettings>()
                    .ForMember(dest => dest.RoundPricesDuringCalculation, mo => mo.Ignore())
                    .ForMember(dest => dest.GroupTierPricesForDistinctShoppingCartItems, mo => mo.Ignore())
                    .ForMember(dest => dest.RenderAssociatedAttributeValueQuantity, mo => mo.Ignore());
                cfg.CreateMap<MediaSettings, MediaSettingsModel>()
                    .ForMember(dest => dest.PicturesStoredIntoDatabase, mo => mo.Ignore())
                    .ForMember(dest => dest.ActiveStoreScopeConfiguration, mo => mo.Ignore())
                    .ForMember(dest => dest.AvatarPictureSize_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ProductThumbPictureSize_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ProductDetailsPictureSize_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ProductThumbPictureSizeOnProductDetailsPage_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.AssociatedProductPictureSize_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.CategoryThumbPictureSize_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.ManufacturerThumbPictureSize_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.VendorThumbPictureSize_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.CartThumbPictureSize_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.MiniCartThumbPictureSize_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.MaximumImageSize_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.MultipleThumbDirectories_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.DefaultImageQuality_OverrideForStore, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<MediaSettingsModel, MediaSettings>()
                    .ForMember(dest => dest.DefaultPictureZoomEnabled, mo => mo.Ignore())
                    .ForMember(dest => dest.ImageSquarePictureSize, mo => mo.Ignore())
                    .ForMember(dest => dest.AutoCompleteSearchThumbPictureSize, mo => mo.Ignore());
                cfg.CreateMap<CustomerSettings, CustomerUserSettingsModel.CustomerSettingsModel>()
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<CustomerUserSettingsModel.CustomerSettingsModel, CustomerSettings>()
                    .ForMember(dest => dest.HashedPasswordFormat, mo => mo.Ignore())
                    .ForMember(dest => dest.AvatarMaximumSizeBytes, mo => mo.Ignore())
                    .ForMember(dest => dest.DownloadableProductsValidateUser, mo => mo.Ignore())
                    .ForMember(dest => dest.OnlineCustomerMinutes, mo => mo.Ignore())
                    .ForMember(dest => dest.SuffixDeletedCustomers, mo => mo.Ignore());
                cfg.CreateMap<AddressSettings, CustomerUserSettingsModel.AddressSettingsModel>()
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<CustomerUserSettingsModel.AddressSettingsModel, AddressSettings>();

                //return request reasons
                cfg.CreateMap<ReturnRequestReason, ReturnRequestReasonModel>()
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<ReturnRequestReasonModel, ReturnRequestReason>()
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.Id, mo => mo.Ignore());
                //return request actions
                cfg.CreateMap<ReturnRequestAction, ReturnRequestActionModel>()
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<ReturnRequestActionModel, ReturnRequestAction>()
                    .ForMember(dest => dest.Locales, mo => mo.Ignore())
                    .ForMember(dest => dest.Id, mo => mo.Ignore());


                //category template
                cfg.CreateMap<CategoryTemplate, CategoryTemplateModel>()
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<CategoryTemplateModel, CategoryTemplate>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore());

                //manufacturer template
                cfg.CreateMap<ManufacturerTemplate, ManufacturerTemplateModel>()
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<ManufacturerTemplateModel, ManufacturerTemplate>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore());
                //product template
                cfg.CreateMap<ProductTemplate, ProductTemplateModel>()
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<ProductTemplateModel, ProductTemplate>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore());
                //topic template
                cfg.CreateMap<TopicTemplate, TopicTemplateModel>()
                    .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<TopicTemplateModel, TopicTemplate>()
                    .ForMember(dest => dest.Id, mo => mo.Ignore());

            };

            return action;

        }

        public int Order
        {
            get { return 0; }
        }
    }
}