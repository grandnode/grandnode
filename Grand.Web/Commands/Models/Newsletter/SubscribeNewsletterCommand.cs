using Grand.Web.Models.Newsletter;
using MediatR;

namespace Grand.Web.Commands.Models.Newsletter
{
    public class SubscribeNewsletterCommand : IRequest<SubscribeNewsletterResultModel>
    {
        public string Email { get; set; }
        public bool Subscribe { get; set; }
    }
}
