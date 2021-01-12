using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Core.Infrastructure.DependencyManagement;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Grand.Web.Areas.Admin.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(IServiceCollection builder, ITypeFinder typeFinder, GrandConfig config)
        {
            builder.AddScoped<IActivityLogViewModelService, ActivityLogViewModelService>();
            builder.AddScoped<IAddressAttributeViewModelService, AddressAttributeViewModelService>();
            builder.AddScoped<IAffiliateViewModelService, AffiliateViewModelService>();
            builder.AddScoped<IBlogViewModelService, BlogViewModelService>();
            builder.AddScoped<ICampaignViewModelService, CampaignViewModelService>();
            builder.AddScoped<ICategoryViewModelService, CategoryViewModelService>();
            builder.AddScoped<ICheckoutAttributeViewModelService, CheckoutAttributeViewModelService>();
            builder.AddScoped<IContactAttributeViewModelService, ContactAttributeViewModelService>();
            builder.AddScoped<IContactFormViewModelService, ContactFormViewModelService>();
            builder.AddScoped<ICountryViewModelService, CountryViewModelService>();
            builder.AddScoped<ICourseViewModelService, CourseViewModelService>();
            builder.AddScoped<ICurrencyViewModelService, CurrencyViewModelService>();
            builder.AddScoped<ICustomerActionViewModelService, CustomerActionViewModelService>();
            builder.AddScoped<ICustomerAttributeViewModelService, CustomerAttributeViewModelService>();
            builder.AddScoped<ICustomerViewModelService, CustomerViewModelService>();
            builder.AddScoped<ICustomerReportViewModelService, CustomerReportViewModelService>();
            builder.AddScoped<ICustomerReminderViewModelService, CustomerReminderViewModelService>();
            builder.AddScoped<ICustomerRoleViewModelService, CustomerRoleViewModelService>();
            builder.AddScoped<ICustomerTagViewModelService, CustomerTagViewModelService>();
            builder.AddScoped<IDiscountViewModelService, DiscountViewModelService>();
            builder.AddScoped<IDocumentViewModelService, DocumentViewModelService>();
            builder.AddScoped<IEmailAccountViewModelService, EmailAccountViewModelService>();
            builder.AddScoped<IGiftCardViewModelService, GiftCardViewModelService>();
            builder.AddScoped<IKnowledgebaseViewModelService, KnowledgebaseViewModelService>();
            builder.AddScoped<ILanguageViewModelService, LanguageViewModelService>();
            builder.AddScoped<ILogViewModelService, LogViewModelService>();
            builder.AddScoped<IManufacturerViewModelService, ManufacturerViewModelService>();
            builder.AddScoped<INewsViewModelService, NewsViewModelService>();
            builder.AddScoped<IOrderViewModelService, OrderViewModelService>();
            builder.AddScoped<IShipmentViewModelService, ShipmentViewModelService>();
            builder.AddScoped<IProductReviewViewModelService, ProductReviewViewModelService>();
            builder.AddScoped<IReturnRequestViewModelService, ReturnRequestViewModelService>();
            builder.AddScoped<IVendorViewModelService, VendorViewModelService>();
            builder.AddScoped<ITopicViewModelService, TopicViewModelService>();
            builder.AddScoped<IStoreViewModelService, StoreViewModelService>();
            builder.AddScoped<IProductViewModelService, ProductViewModelService>();
        }
        public int Order
        {
            get { return 3; }
        }
    }
}
