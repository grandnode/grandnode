using Grand.Services.Messages;
using Grand.Web.Commands.Models.Newsletter;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Commands.Handler.Newsletter
{
    public class SubscriptionCategoryHandler : IRequestHandler<SubscriptionCategoryCommand, (string message, bool success)>
    {
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;

        public SubscriptionCategoryHandler(INewsLetterSubscriptionService newsLetterSubscriptionService)
        {
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
        }

        public async Task<(string message, bool success)> Handle(SubscriptionCategoryCommand request, CancellationToken cancellationToken)
        {
            bool success = false;
            string message = string.Empty;

            var newsletterEmailId = request.Values["NewsletterEmailId"].ToString();
            if (!string.IsNullOrEmpty(newsletterEmailId))
            {
                var subscription = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionById(newsletterEmailId);
                if (subscription != null)
                {
                    foreach (string formKey in request.Values.Keys)
                    {
                        if (formKey.Contains("Category_"))
                        {
                            try
                            {
                                var category = formKey.Split('_')[1];
                                subscription.Categories.Add(category);
                            }
                            catch (Exception ex)
                            {
                                message = ex.Message;
                            }
                        }
                    }
                    success = true;
                    await _newsLetterSubscriptionService.UpdateNewsLetterSubscription(subscription, false);
                }
                else
                {
                    message = "Email not exist";
                }
            }
            else
                message = "Empty NewsletterEmailId";

            return (message, success);
        }
    }
}
