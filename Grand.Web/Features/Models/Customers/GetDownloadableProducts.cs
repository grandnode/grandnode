using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Localization;
using Grand.Web.Models.Customer;
using MediatR;

namespace Grand.Web.Features.Models.Customers
{
    public class GetDownloadableProducts : IRequest<CustomerDownloadableProductsModel>
    {
        public Customer Customer { get; set; }
        public Language Language { get; set; }

    }
}
