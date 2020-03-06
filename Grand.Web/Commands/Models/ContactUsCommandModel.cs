using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Stores;
using Grand.Web.Models.Common;
using MediatR;

namespace Grand.Web.Commands.Models
{
    public class ContactUsCommandModel : IRequest<ContactUsModel>
    {
        public Customer Customer { get; set; }
        public Store Store { get; set; }
        public Language Language { get; set; }
    }
}
