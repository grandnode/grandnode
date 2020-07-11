using Grand.Domain.Customers;
using Grand.Services.Common;
using Grand.Services.Notifications.Authentication;
using MediatR;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Plugin.ExternalAuth.Google.Infrastructure.Cache
{
    /// <summary>
    /// Google authentication event consumer (used for saving customer fields on registration)
    /// </summary>
    public partial class GoogleAuthenticationEventConsumer : INotificationHandler<CustomerAutoRegisteredByExternalMethodEvent>
    {
        #region Fields

        private readonly IGenericAttributeService _genericAttributeService;

        #endregion

        #region Ctor

        public GoogleAuthenticationEventConsumer(IGenericAttributeService genericAttributeService)
        {
            _genericAttributeService = genericAttributeService;
        }

        #endregion

        #region Methods

        public async Task Handle(CustomerAutoRegisteredByExternalMethodEvent eventMessage, CancellationToken cancellationToken)
        {
            if (eventMessage?.Customer == null || eventMessage.AuthenticationParameters == null)
                return;

            //handle event only for this authentication method
            if (!eventMessage.AuthenticationParameters.ProviderSystemName.Equals(GoogleAuthenticationDefaults.ProviderSystemName))
                return;

            //store some of the customer fields
            var firstName = eventMessage.AuthenticationParameters.Claims?.FirstOrDefault(claim => claim.Type == ClaimTypes.GivenName)?.Value;
            if (!string.IsNullOrEmpty(firstName))
                await _genericAttributeService.SaveAttribute(eventMessage.Customer, SystemCustomerAttributeNames.FirstName, firstName);

            var lastName = eventMessage.AuthenticationParameters.Claims?.FirstOrDefault(claim => claim.Type == ClaimTypes.Surname)?.Value;
            if (!string.IsNullOrEmpty(lastName))
                await _genericAttributeService.SaveAttribute(eventMessage.Customer, SystemCustomerAttributeNames.LastName, lastName);
        }

        public void HandleEvent(CustomerAutoRegisteredByExternalMethodEvent eventMessage) { }


        #endregion
    }
}