using Grand.Core.Domain.Messages;
using Grand.Web.Models.Newsletter;
using System.Threading.Tasks;

namespace Grand.Web.Interfaces
{
    public partial interface INewsletterViewModelService
    {
        Task<NewsletterCategoryModel> PrepareNewsletterCategory(string id);
        Task<NewsletterBoxModel> PrepareNewsletterBox();
        Task<SubscribeNewsletterResultModel> SubscribeNewsletter(string email, bool subscribe);
        Task<SubscriptionActivationModel> PrepareSubscriptionActivation(NewsLetterSubscription subscription, bool active);
    }
}