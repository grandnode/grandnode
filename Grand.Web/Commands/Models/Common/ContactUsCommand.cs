using Grand.Domain.Customers;
using Grand.Domain.Localization;
using Grand.Domain.Stores;
using Grand.Web.Models.Common;
using MediatR;

namespace Grand.Web.Commands.Models.Common
{
    public class ContactUsCommand : IRequest<ContactUsModel>
    {
        public Customer Customer { get; set; }
        public Store Store { get; set; }
        public Language Language { get; set; }
    }
}
