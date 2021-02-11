using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Core.DependencyInjection;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Grand.Web.Areas.Admin.Infrastructure
{
    public class DependencyInjection : IDependencyInjection
    {
        public virtual void Register(IServiceCollection serviceCollection, ITypeFinder typeFinder, GrandConfig config)
        {
            serviceCollection.AddScoped<IActivityLogViewModelService, ActivityLogViewModelService>();
            serviceCollection.AddScoped<IAddressAttributeViewModelService, AddressAttributeViewModelService>();
            serviceCollection.AddScoped<IAffiliateViewModelService, AffiliateViewModelService>();
            serviceCollection.AddScoped<IBlogViewModelService, BlogViewModelService>();
            serviceCollection.AddScoped<ICampaignViewModelService, CampaignViewModelService>();
            serviceCollection.AddScoped<ICategoryViewModelService, CategoryViewModelService>();
            serviceCollection.AddScoped<ICheckoutAttributeViewModelService, CheckoutAttributeViewModelService>();
            serviceCollection.AddScoped<IContactAttributeViewModelService, ContactAttributeViewModelService>();
            serviceCollection.AddScoped<IContactFormViewModelService, ContactFormViewModelService>();
            serviceCollection.AddScoped<ICountryViewModelService, CountryViewModelService>();
            serviceCollection.AddScoped<ICourseViewModelService, CourseViewModelService>();
            serviceCollection.AddScoped<ICurrencyViewModelService, CurrencyViewModelService>();
            serviceCollection.AddScoped<ICustomerActionViewModelService, CustomerActionViewModelService>();
            serviceCollection.AddScoped<ICustomerAttributeViewModelService, CustomerAttributeViewModelService>();
            serviceCollection.AddScoped<ICustomerViewModelService, CustomerViewModelService>();
            serviceCollection.AddScoped<ICustomerReportViewModelService, CustomerReportViewModelService>();
            serviceCollection.AddScoped<ICustomerReminderViewModelService, CustomerReminderViewModelService>();
            serviceCollection.AddScoped<ICustomerRoleViewModelService, CustomerRoleViewModelService>();
            serviceCollection.AddScoped<ICustomerTagViewModelService, CustomerTagViewModelService>();
            serviceCollection.AddScoped<IDiscountViewModelService, DiscountViewModelService>();
            serviceCollection.AddScoped<IDocumentViewModelService, DocumentViewModelService>();
            serviceCollection.AddScoped<IEmailAccountViewModelService, EmailAccountViewModelService>();
            serviceCollection.AddScoped<IGiftCardViewModelService, GiftCardViewModelService>();
            serviceCollection.AddScoped<IKnowledgebaseViewModelService, KnowledgebaseViewModelService>();
            serviceCollection.AddScoped<ILanguageViewModelService, LanguageViewModelService>();
            serviceCollection.AddScoped<ILogViewModelService, LogViewModelService>();
            serviceCollection.AddScoped<IManufacturerViewModelService, ManufacturerViewModelService>();
            serviceCollection.AddScoped<INewsViewModelService, NewsViewModelService>();
            serviceCollection.AddScoped<IOrderViewModelService, OrderViewModelService>();
            serviceCollection.AddScoped<IShipmentViewModelService, ShipmentViewModelService>();
            serviceCollection.AddScoped<IProductReviewViewModelService, ProductReviewViewModelService>();
            serviceCollection.AddScoped<IReturnRequestViewModelService, ReturnRequestViewModelService>();
            serviceCollection.AddScoped<IVendorViewModelService, VendorViewModelService>();
            serviceCollection.AddScoped<ITopicViewModelService, TopicViewModelService>();
            serviceCollection.AddScoped<IStoreViewModelService, StoreViewModelService>();
            serviceCollection.AddScoped<IProductViewModelService, ProductViewModelService>();
        }
        public int Order
        {
            get { return 3; }
        }
    }
}
