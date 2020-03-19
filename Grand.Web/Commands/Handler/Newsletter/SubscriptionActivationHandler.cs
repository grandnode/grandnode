using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Web.Commands.Models.Newsletter;
using Grand.Web.Models.Newsletter;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Commands.Handler.Newsletter
{
    public class SubscriptionActivationHandler : IRequestHandler<SubscriptionActivationCommand, SubscriptionActivationModel>
    {
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly ILocalizationService _localizationService;

        public SubscriptionActivationHandler(INewsLetterSubscriptionService newsLetterSubscriptionService, ILocalizationService localizationService)
        {
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _localizationService = localizationService;
        }

        public async Task<SubscriptionActivationModel> Handle(SubscriptionActivationCommand request, CancellationToken cancellationToken)
        {
            var subscription = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByGuid(request.Token);
            if (subscription == null)
                return null;

            var model = new SubscriptionActivationModel();

            if (request.Active)
            {
                subscription.Active = true;
                await _newsLetterSubscriptionService.UpdateNewsLetterSubscription(subscription);
            }
            else
                await _newsLetterSubscriptionService.DeleteNewsLetterSubscription(subscription);

            model.Result = request.Active
                ? _localizationService.GetResource("Newsletter.ResultActivated")
                : _localizationService.GetResource("Newsletter.ResultDeactivated");

            return model;

        }
    }
}
