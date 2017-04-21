using Grand.Core.Domain.Messages;
using Grand.Web.Models.Newsletter;


namespace Grand.Web.Services
{
    public partial interface INewsletterWebService
    {
        NewsletterCategoryModel PrepareNewsletterCategory(string id);
        NewsletterBoxModel PrepareNewsletterBox();
        SubscribeNewsletterResultModel SubscribeNewsletter(string email, bool subscribe);
        SubscriptionActivationModel PrepareSubscriptionActivation(NewsLetterSubscription subscription, bool active);
    }
}