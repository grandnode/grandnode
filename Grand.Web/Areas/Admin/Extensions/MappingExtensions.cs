using Grand.Core.Domain.Blogs;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Discounts;
using Grand.Core.Domain.Forums;
using Grand.Core.Domain.Knowledgebase;
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
using Grand.Services.Common;
using Grand.Services.Payments;
using Grand.Services.Shipping;
using Grand.Services.Tax;
using Grand.Web.Areas.Admin.Models.Blogs;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Web.Areas.Admin.Models.Cms;
using Grand.Web.Areas.Admin.Models.Common;
using Grand.Web.Areas.Admin.Models.Customers;
using Grand.Web.Areas.Admin.Models.Directory;
using Grand.Web.Areas.Admin.Models.Discounts;
using Grand.Web.Areas.Admin.Models.ExternalAuthentication;
using Grand.Web.Areas.Admin.Models.Forums;
using Grand.Web.Areas.Admin.Models.Knowledgebase;
using Grand.Web.Areas.Admin.Models.Localization;
using Grand.Web.Areas.Admin.Models.Logging;
using Grand.Web.Areas.Admin.Models.Messages;
using Grand.Web.Areas.Admin.Models.News;
using Grand.Web.Areas.Admin.Models.Orders;
using Grand.Web.Areas.Admin.Models.Payments;
using Grand.Web.Areas.Admin.Models.Plugins;
using Grand.Web.Areas.Admin.Models.Polls;
using Grand.Web.Areas.Admin.Models.Settings;
using Grand.Web.Areas.Admin.Models.Shipping;
using Grand.Web.Areas.Admin.Models.Stores;
using Grand.Web.Areas.Admin.Models.Tax;
using Grand.Web.Areas.Admin.Models.Templates;
using Grand.Web.Areas.Admin.Models.Topics;
using Grand.Web.Areas.Admin.Models.Vendors;
using System;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class MappingExtensions
    {
        public static TDestination MapTo<TSource, TDestination>(this TSource source)
        {
            return AutoMapperConfiguration.Mapper.Map<TSource, TDestination>(source);
        }

        public static TDestination MapTo<TSource, TDestination>(this TSource source, TDestination destination)
        {
            return AutoMapperConfiguration.Mapper.Map(source, destination);
        }

        #region Category

        public static CategoryModel ToModel(this Category entity)
        {
            return entity.MapTo<Category, CategoryModel>();
        }

        public static Category ToEntity(this CategoryModel model)
        {
            return model.MapTo<CategoryModel, Category>();
        }

        public static Category ToEntity(this CategoryModel model, Category destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Manufacturer

        public static ManufacturerModel ToModel(this Manufacturer entity)
        {
            return entity.MapTo<Manufacturer, ManufacturerModel>();
        }

        public static Manufacturer ToEntity(this ManufacturerModel model)
        {
            return model.MapTo<ManufacturerModel, Manufacturer>();
        }

        public static Manufacturer ToEntity(this ManufacturerModel model, Manufacturer destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Vendor

        public static VendorModel ToModel(this Vendor entity)
        {
            return entity.MapTo<Vendor, VendorModel>();
        }

        public static Vendor ToEntity(this VendorModel model)
        {
            return model.MapTo<VendorModel, Vendor>();
        }

        public static Vendor ToEntity(this VendorModel model, Vendor destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Products

        public static ProductModel ToModel(this Product entity)
        {
            return entity.MapTo<Product, ProductModel>();
        }

        public static Product ToEntity(this ProductModel model)
        {
            return model.MapTo<ProductModel, Product>();
        }

        public static Product ToEntity(this ProductModel model, Product destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Product attributes

        public static ProductAttributeModel ToModel(this ProductAttribute entity)
        {
            return entity.MapTo<ProductAttribute, ProductAttributeModel>();
        }

        public static ProductAttribute ToEntity(this ProductAttributeModel model)
        {
            return model.MapTo<ProductAttributeModel, ProductAttribute>();
        }

        public static ProductAttribute ToEntity(this ProductAttributeModel model, ProductAttribute destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Product attributes combinations

        public static ProductAttributeCombinationModel ToModel(this ProductAttributeCombination entity)
        {
            return entity.MapTo<ProductAttributeCombination, ProductAttributeCombinationModel>();
        }

        public static ProductAttributeCombination ToEntity(this ProductAttributeCombinationModel model)
        {
            return model.MapTo<ProductAttributeCombinationModel, ProductAttributeCombination>();
        }

        public static ProductAttributeCombination ToEntity(this ProductAttributeCombinationModel model, ProductAttributeCombination destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Specification attributes

        //attributes
        public static SpecificationAttributeModel ToModel(this SpecificationAttribute entity)
        {
            return entity.MapTo<SpecificationAttribute, SpecificationAttributeModel>();
        }

        public static SpecificationAttribute ToEntity(this SpecificationAttributeModel model)
        {
            return model.MapTo<SpecificationAttributeModel, SpecificationAttribute>();
        }

        public static SpecificationAttribute ToEntity(this SpecificationAttributeModel model, SpecificationAttribute destination)
        {
            return model.MapTo(destination);
        }

        //attribute options
        public static SpecificationAttributeOptionModel ToModel(this SpecificationAttributeOption entity)
        {
            return entity.MapTo<SpecificationAttributeOption, SpecificationAttributeOptionModel>();
        }

        public static SpecificationAttributeOption ToEntity(this SpecificationAttributeOptionModel model)
        {
            return model.MapTo<SpecificationAttributeOptionModel, SpecificationAttributeOption>();
        }

        public static SpecificationAttributeOption ToEntity(this SpecificationAttributeOptionModel model, SpecificationAttributeOption destination)
        {
            return model.MapTo(destination);
        }
        #endregion

        #region Checkout attributes

        //attributes
        public static CheckoutAttributeModel ToModel(this CheckoutAttribute entity)
        {
            return entity.MapTo<CheckoutAttribute, CheckoutAttributeModel>();
        }

        public static CheckoutAttribute ToEntity(this CheckoutAttributeModel model)
        {
            return model.MapTo<CheckoutAttributeModel, CheckoutAttribute>();
        }

        public static CheckoutAttribute ToEntity(this CheckoutAttributeModel model, CheckoutAttribute destination)
        {
            return model.MapTo(destination);
        }

        #endregion
        #region Contact attributes

        //attributes
        public static ContactAttributeModel ToModel(this ContactAttribute entity)
        {
            return entity.MapTo<ContactAttribute, ContactAttributeModel>();
        }

        public static ContactAttribute ToEntity(this ContactAttributeModel model)
        {
            return model.MapTo<ContactAttributeModel, ContactAttribute>();
        }

        public static ContactAttribute ToEntity(this ContactAttributeModel model, ContactAttribute destination)
        {
            return model.MapTo(destination);
        }

        #endregion
        #region Customer attributes

        //attributes
        public static CustomerAttributeModel ToModel(this CustomerAttribute entity)
        {
            return entity.MapTo<CustomerAttribute, CustomerAttributeModel>();
        }

        public static CustomerAttribute ToEntity(this CustomerAttributeModel model)
        {
            return model.MapTo<CustomerAttributeModel, CustomerAttribute>();
        }

        public static CustomerAttribute ToEntity(this CustomerAttributeModel model, CustomerAttribute destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Address attributes

        //attributes
        public static AddressAttributeModel ToModel(this AddressAttribute entity)
        {
            return entity.MapTo<AddressAttribute, AddressAttributeModel>();
        }

        public static AddressAttribute ToEntity(this AddressAttributeModel model)
        {
            return model.MapTo<AddressAttributeModel, AddressAttribute>();
        }

        public static AddressAttribute ToEntity(this AddressAttributeModel model, AddressAttribute destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Languages

        public static LanguageModel ToModel(this Language entity)
        {
            return entity.MapTo<Language, LanguageModel>();
        }

        public static Language ToEntity(this LanguageModel model)
        {
            return model.MapTo<LanguageModel, Language>();
        }

        public static Language ToEntity(this LanguageModel model, Language destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Email account

        public static EmailAccountModel ToModel(this EmailAccount entity)
        {
            return entity.MapTo<EmailAccount, EmailAccountModel>();
        }

        public static EmailAccount ToEntity(this EmailAccountModel model)
        {
            return model.MapTo<EmailAccountModel, EmailAccount>();
        }

        public static EmailAccount ToEntity(this EmailAccountModel model, EmailAccount destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Message templates

        public static MessageTemplateModel ToModel(this MessageTemplate entity)
        {
            return entity.MapTo<MessageTemplate, MessageTemplateModel>();
        }

        public static MessageTemplate ToEntity(this MessageTemplateModel model)
        {
            return model.MapTo<MessageTemplateModel, MessageTemplate>();
        }

        public static MessageTemplate ToEntity(this MessageTemplateModel model, MessageTemplate destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Contact form

        public static ContactFormModel ToModel(this ContactUs entity)
        {
            return entity.MapTo<ContactUs, ContactFormModel>();
        }
        #endregion

        #region Queued email

        public static QueuedEmailModel ToModel(this QueuedEmail entity)
        {
            return entity.MapTo<QueuedEmail, QueuedEmailModel>();
        }

        public static QueuedEmail ToEntity(this QueuedEmailModel model)
        {
            return model.MapTo<QueuedEmailModel, QueuedEmail>();
        }

        public static QueuedEmail ToEntity(this QueuedEmailModel model, QueuedEmail destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Banner

        public static BannerModel ToModel(this Banner entity)
        {
            return entity.MapTo<Banner, BannerModel>();
        }

        public static Banner ToEntity(this BannerModel model)
        {
            return model.MapTo<BannerModel, Banner>();
        }

        public static Banner ToEntity(this BannerModel model, Banner destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Interactive form

        public static InteractiveFormModel ToModel(this InteractiveForm entity)
        {
            return entity.MapTo<InteractiveForm, InteractiveFormModel>();
        }

        public static InteractiveForm ToEntity(this InteractiveFormModel model)
        {
            return model.MapTo<InteractiveFormModel, InteractiveForm>();
        }

        public static InteractiveForm ToEntity(this InteractiveFormModel model, InteractiveForm destination)
        {
            return model.MapTo(destination);
        }


        public static InteractiveFormAttributeModel ToModel(this InteractiveForm.FormAttribute entity)
        {
            return entity.MapTo<InteractiveForm.FormAttribute, InteractiveFormAttributeModel>();
        }

        public static InteractiveForm.FormAttribute ToEntity(this InteractiveFormAttributeModel model)
        {
            return model.MapTo<InteractiveFormAttributeModel, InteractiveForm.FormAttribute>();
        }

        public static InteractiveForm.FormAttribute ToEntity(this InteractiveFormAttributeModel model, InteractiveForm.FormAttribute destination)
        {
            return model.MapTo(destination);
        }


        #endregion


        #region Campaigns

        public static CampaignModel ToModel(this Campaign entity)
        {
            return entity.MapTo<Campaign, CampaignModel>();
        }

        public static Campaign ToEntity(this CampaignModel model)
        {
            return model.MapTo<CampaignModel, Campaign>();
        }

        public static Campaign ToEntity(this CampaignModel model, Campaign destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Topics

        public static TopicModel ToModel(this Topic entity)
        {
            return entity.MapTo<Topic, TopicModel>();
        }

        public static Topic ToEntity(this TopicModel model)
        {
            return model.MapTo<TopicModel, Topic>();
        }

        public static Topic ToEntity(this TopicModel model, Topic destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Log

        public static LogModel ToModel(this Log entity)
        {
            return entity.MapTo<Log, LogModel>();
        }

        public static Log ToEntity(this LogModel model)
        {
            return model.MapTo<LogModel, Log>();
        }

        public static Log ToEntity(this LogModel model, Log destination)
        {
            return model.MapTo(destination);
        }

        public static ActivityLogTypeModel ToModel(this ActivityLogType entity)
        {
            return entity.MapTo<ActivityLogType, ActivityLogTypeModel>();
        }

        public static ActivityLogModel ToModel(this ActivityLog entity)
        {
            return entity.MapTo<ActivityLog, ActivityLogModel>();
        }

        public static ActivityStatsModel ToModel(this ActivityStats entity)
        {
            return entity.MapTo<ActivityStats, ActivityStatsModel>();
        }
        #endregion

        #region Currencies

        public static CurrencyModel ToModel(this Currency entity)
        {
            return entity.MapTo<Currency, CurrencyModel>();
        }

        public static Currency ToEntity(this CurrencyModel model)
        {
            return model.MapTo<CurrencyModel, Currency>();
        }

        public static Currency ToEntity(this CurrencyModel model, Currency destination)
        {
            return model.MapTo(destination);
        }
        #endregion

        #region Measure weights

        public static MeasureWeightModel ToModel(this MeasureWeight entity)
        {
            return entity.MapTo<MeasureWeight, MeasureWeightModel>();
        }

        public static MeasureWeight ToEntity(this MeasureWeightModel model)
        {
            return model.MapTo<MeasureWeightModel, MeasureWeight>();
        }

        public static MeasureWeight ToEntity(this MeasureWeightModel model, MeasureWeight destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Measure dimension

        public static MeasureDimensionModel ToModel(this MeasureDimension entity)
        {
            return entity.MapTo<MeasureDimension, MeasureDimensionModel>();
        }

        public static MeasureDimension ToEntity(this MeasureDimensionModel model)
        {
            return model.MapTo<MeasureDimensionModel, MeasureDimension>();
        }

        public static MeasureDimension ToEntity(this MeasureDimensionModel model, MeasureDimension destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Units

        public static MeasureUnitModel ToModel(this MeasureUnit entity)
        {
            return entity.MapTo<MeasureUnit, MeasureUnitModel>();
        }

        public static MeasureUnit ToEntity(this MeasureUnitModel model)
        {
            return model.MapTo<MeasureUnitModel, MeasureUnit>();
        }

        public static MeasureUnit ToEntity(this MeasureUnitModel model, MeasureUnit destination)
        {
            return model.MapTo(destination);
        }
        #endregion

        #region Tax providers

        public static TaxProviderModel ToModel(this ITaxProvider entity)
        {
            return entity.MapTo<ITaxProvider, TaxProviderModel>();
        }

        #endregion

        #region Tax categories

        public static TaxCategoryModel ToModel(this TaxCategory entity)
        {
            return entity.MapTo<TaxCategory, TaxCategoryModel>();
        }

        public static TaxCategory ToEntity(this TaxCategoryModel model)
        {
            return model.MapTo<TaxCategoryModel, TaxCategory>();
        }

        public static TaxCategory ToEntity(this TaxCategoryModel model, TaxCategory destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Shipping rate computation method

        public static ShippingRateComputationMethodModel ToModel(this IShippingRateComputationMethod entity)
        {
            return entity.MapTo<IShippingRateComputationMethod, ShippingRateComputationMethodModel>();
        }

        #endregion

        #region Shipping methods

        public static ShippingMethodModel ToModel(this ShippingMethod entity)
        {
            return entity.MapTo<ShippingMethod, ShippingMethodModel>();
        }

        public static ShippingMethod ToEntity(this ShippingMethodModel model)
        {
            return model.MapTo<ShippingMethodModel, ShippingMethod>();
        }

        public static ShippingMethod ToEntity(this ShippingMethodModel model, ShippingMethod destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Delivery dates

        public static DeliveryDateModel ToModel(this DeliveryDate entity)
        {
            return entity.MapTo<DeliveryDate, DeliveryDateModel>();
        }

        public static DeliveryDate ToEntity(this DeliveryDateModel model)
        {
            return model.MapTo<DeliveryDateModel, DeliveryDate>();
        }

        public static DeliveryDate ToEntity(this DeliveryDateModel model, DeliveryDate destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Payment methods

        public static PaymentMethodModel ToModel(this IPaymentMethod entity)
        {
            return entity.MapTo<IPaymentMethod, PaymentMethodModel>();
        }

        #endregion

        #region External authentication methods

        public static AuthenticationMethodModel ToModel(this IExternalAuthenticationMethod entity)
        {
            return entity.MapTo<IExternalAuthenticationMethod, AuthenticationMethodModel>();
        }

        #endregion

        #region Widgets

        public static WidgetModel ToModel(this IWidgetPlugin entity)
        {
            return entity.MapTo<IWidgetPlugin, WidgetModel>();
        }

        #endregion

        #region Address

        public static AddressModel ToModel(this Address entity)
        {
            return entity.MapTo<Address, AddressModel>();
        }

        public static Address ToEntity(this AddressModel model)
        {
            return model.MapTo<AddressModel, Address>();
        }

        public static Address ToEntity(this AddressModel model, Address destination)
        {
            return model.MapTo(destination);
        }

        public static string CountryName(this Address model)
        {
            var country = Grand.Core.Infrastructure.EngineContext.Current.Resolve<Grand.Services.Directory.ICountryService>().GetCountryById(model.CountryId);
            return country?.Name;
        }

        public static string StateProvinceName(this Address model)
        {
            var state = Grand.Core.Infrastructure.EngineContext.Current.Resolve<Grand.Services.Directory.IStateProvinceService>().GetStateProvinceById(model.StateProvinceId);
            return state?.Name;
        }

        public static void PrepareCustomAddressAttributes(this AddressModel model,
            Address address,
            IAddressAttributeService addressAttributeService,
            IAddressAttributeParser addressAttributeParser)
        {
            //this method is very similar to the same one in Grand.Web project
            if (addressAttributeService == null)
                throw new ArgumentNullException("addressAttributeService");

            if (addressAttributeParser == null)
                throw new ArgumentNullException("addressAttributeParser");

            var attributes = addressAttributeService.GetAllAddressAttributes();
            foreach (var attribute in attributes)
            {
                var attributeModel = new AddressModel.AddressAttributeModel
                {
                    Id = attribute.Id,
                    Name = attribute.Name,
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                };

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = attribute.AddressAttributeValues; 
                    foreach (var attributeValue in attributeValues)
                    {
                        var attributeValueModel = new AddressModel.AddressAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = attributeValue.Name,
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(attributeValueModel);
                    }
                }

                //set already selected attributes
                var selectedAddressAttributes = address != null ? address.CustomAttributes : null;
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.Checkboxes:
                        {
                            if (!String.IsNullOrEmpty(selectedAddressAttributes))
                            {
                                //clear default selection
                                foreach (var item in attributeModel.Values)
                                    item.IsPreSelected = false;

                                //select new values
                                var selectedValues = addressAttributeParser.ParseAddressAttributeValues(selectedAddressAttributes);
                                foreach (var attributeValue in selectedValues)
                                    if (attributeModel.Id == attributeValue.AddressAttributeId)
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
                            if (!String.IsNullOrEmpty(selectedAddressAttributes))
                            {
                                var enteredText = addressAttributeParser.ParseValues(selectedAddressAttributes, attribute.Id);
                                if (enteredText.Count > 0)
                                    attributeModel.DefaultValue = enteredText[0];
                            }
                        }
                        break;
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.Datepicker:
                    case AttributeControlType.FileUpload:
                    case AttributeControlType.ImageSquares:
                    default:
                        //not supported attribute control types
                        break;
                }

                model.CustomAddressAttributes.Add(attributeModel);
            }
        }

        #endregion

        #region NewsLetter subscriptions

        public static NewsLetterSubscriptionModel ToModel(this NewsLetterSubscription entity)
        {
            return entity.MapTo<NewsLetterSubscription, NewsLetterSubscriptionModel>();
        }

        public static NewsLetterSubscription ToEntity(this NewsLetterSubscriptionModel model)
        {
            return model.MapTo<NewsLetterSubscriptionModel, NewsLetterSubscription>();
        }

        public static NewsLetterSubscription ToEntity(this NewsLetterSubscriptionModel model, NewsLetterSubscription destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region NewsletterCategory

        public static NewsletterCategoryModel ToModel(this NewsletterCategory entity)
        {
            return entity.MapTo<NewsletterCategory, NewsletterCategoryModel>();
        }

        public static NewsletterCategory ToEntity(this NewsletterCategoryModel model)
        {
            return model.MapTo<NewsletterCategoryModel, NewsletterCategory>();
        }

        public static NewsletterCategory ToEntity(this NewsletterCategoryModel model, NewsletterCategory destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Discounts

        public static DiscountModel ToModel(this Discount entity)
        {
            return entity.MapTo<Discount, DiscountModel>();
        }

        public static Discount ToEntity(this DiscountModel model)
        {
            return model.MapTo<DiscountModel, Discount>();
        }

        public static Discount ToEntity(this DiscountModel model, Discount destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Forums

        //forum groups
        public static ForumGroupModel ToModel(this ForumGroup entity)
        {
            return entity.MapTo<ForumGroup, ForumGroupModel>();
        }

        public static ForumGroup ToEntity(this ForumGroupModel model)
        {
            return model.MapTo<ForumGroupModel, ForumGroup>();
        }

        public static ForumGroup ToEntity(this ForumGroupModel model, ForumGroup destination)
        {
            return model.MapTo(destination);
        }
        //forums
        public static ForumModel ToModel(this Forum entity)
        {
            return entity.MapTo<Forum, ForumModel>();
        }

        public static Forum ToEntity(this ForumModel model)
        {
            return model.MapTo<ForumModel, Forum>();
        }

        public static Forum ToEntity(this ForumModel model, Forum destination)
        {
            return model.MapTo(destination);
        }
        #endregion

        #region Knowledgebase

        public static KnowledgebaseCategory ToEntity(this KnowledgebaseCategoryModel model)
        {
            return model.MapTo<KnowledgebaseCategoryModel, KnowledgebaseCategory>();
        }

        public static KnowledgebaseCategoryModel ToModel(this KnowledgebaseCategory entity)
        {
            return entity.MapTo<KnowledgebaseCategory, KnowledgebaseCategoryModel>();
        }

        public static KnowledgebaseCategory ToEntity(this KnowledgebaseCategoryModel model, KnowledgebaseCategory destination)
        {
            return model.MapTo(destination);
        }

        public static KnowledgebaseArticle ToEntity(this KnowledgebaseArticleModel model)
        {
            return model.MapTo<KnowledgebaseArticleModel, KnowledgebaseArticle>();
        }

        public static KnowledgebaseArticleModel ToModel(this KnowledgebaseArticle entity)
        {
            return entity.MapTo<KnowledgebaseArticle, KnowledgebaseArticleModel>();
        }

        public static KnowledgebaseArticle ToEntity(this KnowledgebaseArticleModel model, KnowledgebaseArticle destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Blog

        //blog posts
        public static BlogPostModel ToModel(this BlogPost entity)
        {
            return entity.MapTo<BlogPost, BlogPostModel>();
        }

        public static BlogPost ToEntity(this BlogPostModel model)
        {
            return model.MapTo<BlogPostModel, BlogPost>();
        }

        public static BlogPost ToEntity(this BlogPostModel model, BlogPost destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region News

        //news items
        public static NewsItemModel ToModel(this NewsItem entity)
        {
            return entity.MapTo<NewsItem, NewsItemModel>();
        }

        public static NewsItem ToEntity(this NewsItemModel model)
        {
            return model.MapTo<NewsItemModel, NewsItem>();
        }

        public static NewsItem ToEntity(this NewsItemModel model, NewsItem destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Polls

        //news items
        public static PollModel ToModel(this Poll entity)
        {
            return entity.MapTo<Poll, PollModel>();
        }

        public static Poll ToEntity(this PollModel model)
        {
            return model.MapTo<PollModel, Poll>();
        }

        public static Poll ToEntity(this PollModel model, Poll destination)
        {
            return model.MapTo(destination);
        }

        //poll answer
        public static PollAnswerModel ToModel(this PollAnswer entity)
        {
            return entity.MapTo<PollAnswer, PollAnswerModel>();
        }

        public static PollAnswer ToEntity(this PollAnswerModel model)
        {
            return model.MapTo<PollAnswerModel, PollAnswer>();
        }

        public static PollAnswer ToEntity(this PollAnswerModel model, PollAnswer destination)
        {
            return model.MapTo(destination);
        }


        #endregion

        #region Customer roles

        //customer roles
        public static CustomerRoleModel ToModel(this CustomerRole entity)
        {
            return entity.MapTo<CustomerRole, CustomerRoleModel>();
        }

        public static CustomerRole ToEntity(this CustomerRoleModel model)
        {
            return model.MapTo<CustomerRoleModel, CustomerRole>();
        }

        public static CustomerRole ToEntity(this CustomerRoleModel model, CustomerRole destination)
        {
            return model.MapTo(destination);
        }

        #endregion


        #region Customer Tag

        //customer tags
        public static CustomerTagModel ToModel(this CustomerTag entity)
        {
            return entity.MapTo<CustomerTag, CustomerTagModel>();
        }

        public static CustomerTag ToEntity(this CustomerTagModel model)
        {
            return model.MapTo<CustomerTagModel, CustomerTag>();
        }

        public static CustomerTag ToEntity(this CustomerTagModel model, CustomerTag destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Customer Action

        //customer action
        public static CustomerActionModel ToModel(this CustomerAction entity)
        {
            return entity.MapTo<CustomerAction, CustomerActionModel>();
        }

        public static CustomerAction ToEntity(this CustomerActionModel model)
        {
            return model.MapTo<CustomerActionModel, CustomerAction>();
        }

        public static CustomerAction ToEntity(this CustomerActionModel model, CustomerAction destination)
        {
            return model.MapTo(destination);
        }


        public static CustomerActionConditionModel ToModel(this CustomerAction.ActionCondition entity)
        {
            return entity.MapTo<CustomerAction.ActionCondition, CustomerActionConditionModel>();
        }

        public static CustomerAction.ActionCondition ToEntity(this CustomerActionConditionModel model)
        {
            return model.MapTo<CustomerActionConditionModel, CustomerAction.ActionCondition>();
        }

        public static CustomerAction.ActionCondition ToEntity(this CustomerActionConditionModel model, CustomerAction.ActionCondition destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Customer Action Type

        public static CustomerActionTypeModel ToModel(this CustomerActionType entity)
        {
            return entity.MapTo<CustomerActionType, CustomerActionTypeModel>();
        }

        #endregion


        #region Customer Reminder

        //customer action
        public static CustomerReminderModel ToModel(this CustomerReminder entity)
        {
            return entity.MapTo<CustomerReminder, CustomerReminderModel>();
        }

        public static CustomerReminder ToEntity(this CustomerReminderModel model)
        {
            return model.MapTo<CustomerReminderModel, CustomerReminder>();
        }

        public static CustomerReminder ToEntity(this CustomerReminderModel model, CustomerReminder destination)
        {
            return model.MapTo(destination);
        }



        public static CustomerReminderModel.ReminderLevelModel ToModel(this CustomerReminder.ReminderLevel entity)
        {
            return entity.MapTo<CustomerReminder.ReminderLevel, CustomerReminderModel.ReminderLevelModel>();
        }

        public static CustomerReminder.ReminderLevel ToEntity(this CustomerReminderModel.ReminderLevelModel model)
        {
            return model.MapTo<CustomerReminderModel.ReminderLevelModel, CustomerReminder.ReminderLevel>();
        }

        public static CustomerReminder.ReminderLevel ToEntity(this CustomerReminderModel.ReminderLevelModel model, CustomerReminder.ReminderLevel destination)
        {
            return model.MapTo(destination);
        }

        public static CustomerReminderModel.ConditionModel ToModel(this CustomerReminder.ReminderCondition entity)
        {
            return entity.MapTo<CustomerReminder.ReminderCondition, CustomerReminderModel.ConditionModel>();
        }

        public static CustomerReminder.ReminderCondition ToEntity(this CustomerReminderModel.ConditionModel model)
        {
            return model.MapTo<CustomerReminderModel.ConditionModel, CustomerReminder.ReminderCondition>();
        }

        public static CustomerReminder.ReminderCondition ToEntity(this CustomerReminderModel.ConditionModel model, CustomerReminder.ReminderCondition destination)
        {
            return model.MapTo(destination);
        }


        #endregion

        #region Gift Cards

        public static GiftCardModel ToModel(this GiftCard entity)
        {
            return entity.MapTo<GiftCard, GiftCardModel>();
        }

        public static GiftCard ToEntity(this GiftCardModel model)
        {
            return model.MapTo<GiftCardModel, GiftCard>();
        }

        public static GiftCard ToEntity(this GiftCardModel model, GiftCard destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Countries / states

        public static CountryModel ToModel(this Country entity)
        {
            return entity.MapTo<Country, CountryModel>();
        }

        public static Country ToEntity(this CountryModel model)
        {
            return model.MapTo<CountryModel, Country>();
        }

        public static Country ToEntity(this CountryModel model, Country destination)
        {
            return model.MapTo(destination);
        }

        public static StateProvinceModel ToModel(this StateProvince entity)
        {
            return entity.MapTo<StateProvince, StateProvinceModel>();
        }

        public static StateProvince ToEntity(this StateProvinceModel model)
        {
            return model.MapTo<StateProvinceModel, StateProvince>();
        }

        public static StateProvince ToEntity(this StateProvinceModel model, StateProvince destination)
        {
            return model.MapTo(destination);
        }


        #endregion

        #region Settings

        public static TaxSettingsModel ToModel(this TaxSettings entity)
        {
            return entity.MapTo<TaxSettings, TaxSettingsModel>();
        }
        public static TaxSettings ToEntity(this TaxSettingsModel model, TaxSettings destination)
        {
            return model.MapTo(destination);
        }


        public static ShippingSettingsModel ToModel(this ShippingSettings entity)
        {
            return entity.MapTo<ShippingSettings, ShippingSettingsModel>();
        }
        public static ShippingSettings ToEntity(this ShippingSettingsModel model, ShippingSettings destination)
        {
            return model.MapTo(destination);
        }


        public static ForumSettingsModel ToModel(this ForumSettings entity)
        {
            return entity.MapTo<ForumSettings, ForumSettingsModel>();
        }
        public static ForumSettings ToEntity(this ForumSettingsModel model, ForumSettings destination)
        {
            return model.MapTo(destination);
        }


        public static BlogSettingsModel ToModel(this BlogSettings entity)
        {
            return entity.MapTo<BlogSettings, BlogSettingsModel>();
        }
        public static BlogSettings ToEntity(this BlogSettingsModel model, BlogSettings destination)
        {
            return model.MapTo(destination);
        }


        public static VendorSettingsModel ToModel(this VendorSettings entity)
        {
            return entity.MapTo<VendorSettings, VendorSettingsModel>();
        }
        public static VendorSettings ToEntity(this VendorSettingsModel model, VendorSettings destination)
        {
            return model.MapTo(destination);
        }


        public static NewsSettingsModel ToModel(this NewsSettings entity)
        {
            return entity.MapTo<NewsSettings, NewsSettingsModel>();
        }
        public static NewsSettings ToEntity(this NewsSettingsModel model, NewsSettings destination)
        {
            return model.MapTo(destination);
        }


        public static CatalogSettingsModel ToModel(this CatalogSettings entity)
        {
            return entity.MapTo<CatalogSettings, CatalogSettingsModel>();
        }
        public static CatalogSettings ToEntity(this CatalogSettingsModel model, CatalogSettings destination)
        {
            return model.MapTo(destination);
        }


        public static RewardPointsSettingsModel ToModel(this RewardPointsSettings entity)
        {
            return entity.MapTo<RewardPointsSettings, RewardPointsSettingsModel>();
        }
        public static RewardPointsSettings ToEntity(this RewardPointsSettingsModel model, RewardPointsSettings destination)
        {
            return model.MapTo(destination);
        }


        public static OrderSettingsModel ToModel(this OrderSettings entity)
        {
            return entity.MapTo<OrderSettings, OrderSettingsModel>();
        }
        public static OrderSettings ToEntity(this OrderSettingsModel model, OrderSettings destination)
        {
            return model.MapTo(destination);
        }


        public static ShoppingCartSettingsModel ToModel(this ShoppingCartSettings entity)
        {
            return entity.MapTo<ShoppingCartSettings, ShoppingCartSettingsModel>();
        }
        public static ShoppingCartSettings ToEntity(this ShoppingCartSettingsModel model, ShoppingCartSettings destination)
        {
            return model.MapTo(destination);
        }


        public static MediaSettingsModel ToModel(this MediaSettings entity)
        {
            return entity.MapTo<MediaSettings, MediaSettingsModel>();
        }
        public static MediaSettings ToEntity(this MediaSettingsModel model, MediaSettings destination)
        {
            return model.MapTo(destination);
        }

        //customer/user settings
        public static CustomerUserSettingsModel.CustomerSettingsModel ToModel(this CustomerSettings entity)
        {
            return entity.MapTo<CustomerSettings, CustomerUserSettingsModel.CustomerSettingsModel>();
        }
        public static CustomerSettings ToEntity(this CustomerUserSettingsModel.CustomerSettingsModel model, CustomerSettings destination)
        {
            return model.MapTo(destination);
        }
        public static CustomerUserSettingsModel.AddressSettingsModel ToModel(this AddressSettings entity)
        {
            return entity.MapTo<AddressSettings, CustomerUserSettingsModel.AddressSettingsModel>();
        }
        public static AddressSettings ToEntity(this CustomerUserSettingsModel.AddressSettingsModel model, AddressSettings destination)
        {
            return model.MapTo(destination);
        }
        #endregion

        #region Plugins

        public static PluginModel ToModel(this PluginDescriptor entity)
        {
            return entity.MapTo<PluginDescriptor, PluginModel>();
        }

        #endregion

        #region Stores

        public static StoreModel ToModel(this Store entity)
        {
            return entity.MapTo<Store, StoreModel>();
        }

        public static Store ToEntity(this StoreModel model)
        {
            return model.MapTo<StoreModel, Store>();
        }

        public static Store ToEntity(this StoreModel model, Store destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Templates

        public static CategoryTemplateModel ToModel(this CategoryTemplate entity)
        {
            return entity.MapTo<CategoryTemplate, CategoryTemplateModel>();
        }

        public static CategoryTemplate ToEntity(this CategoryTemplateModel model)
        {
            return model.MapTo<CategoryTemplateModel, CategoryTemplate>();
        }

        public static CategoryTemplate ToEntity(this CategoryTemplateModel model, CategoryTemplate destination)
        {
            return model.MapTo(destination);
        }


        public static ManufacturerTemplateModel ToModel(this ManufacturerTemplate entity)
        {
            return entity.MapTo<ManufacturerTemplate, ManufacturerTemplateModel>();
        }

        public static ManufacturerTemplate ToEntity(this ManufacturerTemplateModel model)
        {
            return model.MapTo<ManufacturerTemplateModel, ManufacturerTemplate>();
        }

        public static ManufacturerTemplate ToEntity(this ManufacturerTemplateModel model, ManufacturerTemplate destination)
        {
            return model.MapTo(destination);
        }


        public static ProductTemplateModel ToModel(this ProductTemplate entity)
        {
            return entity.MapTo<ProductTemplate, ProductTemplateModel>();
        }

        public static ProductTemplate ToEntity(this ProductTemplateModel model)
        {
            return model.MapTo<ProductTemplateModel, ProductTemplate>();
        }

        public static ProductTemplate ToEntity(this ProductTemplateModel model, ProductTemplate destination)
        {
            return model.MapTo(destination);
        }



        public static TopicTemplateModel ToModel(this TopicTemplate entity)
        {
            return entity.MapTo<TopicTemplate, TopicTemplateModel>();
        }

        public static TopicTemplate ToEntity(this TopicTemplateModel model)
        {
            return model.MapTo<TopicTemplateModel, TopicTemplate>();
        }

        public static TopicTemplate ToEntity(this TopicTemplateModel model, TopicTemplate destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Return request reason

        public static ReturnRequestReasonModel ToModel(this ReturnRequestReason entity)
        {
            return entity.MapTo<ReturnRequestReason, ReturnRequestReasonModel>();
        }

        public static ReturnRequestReason ToEntity(this ReturnRequestReasonModel model)
        {
            return model.MapTo<ReturnRequestReasonModel, ReturnRequestReason>();
        }

        public static ReturnRequestReason ToEntity(this ReturnRequestReasonModel model, ReturnRequestReason destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Return request action

        public static ReturnRequestActionModel ToModel(this ReturnRequestAction entity)
        {
            return entity.MapTo<ReturnRequestAction, ReturnRequestActionModel>();
        }

        public static ReturnRequestAction ToEntity(this ReturnRequestActionModel model)
        {
            return model.MapTo<ReturnRequestActionModel, ReturnRequestAction>();
        }

        public static ReturnRequestAction ToEntity(this ReturnRequestActionModel model, ReturnRequestAction destination)
        {
            return model.MapTo(destination);
        }

        #endregion

    }
}