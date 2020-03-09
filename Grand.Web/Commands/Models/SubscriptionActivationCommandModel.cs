using Grand.Web.Models.Newsletter;
using MediatR;
using System;

namespace Grand.Web.Commands.Models
{
    public class SubscriptionActivationCommandModel : IRequest<SubscriptionActivationModel>
    {
        public Guid Token { get; set; }
        public bool Active { get; set; }
    }
}
