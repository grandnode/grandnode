using Grand.Domain.Catalog;
using Grand.Domain.Localization;
using MediatR;

namespace Grand.Services.Commands.Models.Catalog
{
    public class SendOutBidCustomerNotificationCommand : IRequest<bool>
    {
        public Product Product { get; set; }
        public Bid Bid { get; set; }
        public Language Language { get; set; }
    }
}
