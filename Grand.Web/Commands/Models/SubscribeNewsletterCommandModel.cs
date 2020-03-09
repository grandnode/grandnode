using Grand.Web.Models.Newsletter;
using MediatR;

namespace Grand.Web.Commands.Models
{
    public class SubscribeNewsletterCommandModel : IRequest<SubscribeNewsletterResultModel>
    {
        public string Email { get; set; }
        public bool Subscribe { get; set; }
    }
}
